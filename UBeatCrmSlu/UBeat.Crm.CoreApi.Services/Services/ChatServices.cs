﻿using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.Chat;
using UBeat.Crm.CoreApi.DomainModel.FileService;
using UBeat.Crm.CoreApi.DomainModel.PushService;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Chat;
using UBeat.Crm.CoreApi.Services.webchat;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ChatServices : BasicBaseServices
    {
        IChatRepository _repository;
        PushServices _pushServices;
        private static readonly Logger Logger = LogManager.GetLogger(typeof(ChatServices).FullName);
        private readonly FileServices _fileService;
        private readonly IAccountRepository _accountRepository;
        public ChatServices(IChatRepository repository, PushServices pushService,
                IAccountRepository accountRepository)
        {

            _repository = repository;
            _pushServices = pushService;
            _accountRepository = accountRepository;

        }

        #region --创建群--
        public OutputResult<object> AddGroup(AddGroupModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            //修改返回值的id数据格式
            string pinyin = PinYinConvert.ToChinese(data.GroupName, false);
            GroupInsert crmData = new GroupInsert()
            {
                GroupName = data.GroupName,
                Pinyin = pinyin,
                EntityId = data.EntityId.HasValue ? data.EntityId.Value : Guid.Empty,
                BusinessId = data.BusinessId.HasValue ? data.BusinessId.Value : Guid.Empty,
                GroupIcon = data.GroupIcon,
                GroupType = data.GroupType,
                MemberIds = data.MemberIds,
                UserNo = userId,
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            #region 需要处理部门的情况
            if (data.DeptIds != null && data.DeptIds.Count > 0) {

                foreach (Guid deptid in data.DeptIds) {
                    PageParam pageParam = new PageParam() {
                        PageIndex = 1,
                        PageSize = 10000
                    };
                    AccountUserQueryMapper accountUserQueryMapper = new AccountUserQueryMapper()
                    {
                        RecStatus = 1,
                        DeptId = deptid
                    };
                    Dictionary<string, List<IDictionary<string, object>>> pageData = this._accountRepository.GetUserList(pageParam, accountUserQueryMapper, userId);
                    List<IDictionary<string, object>> userList = pageData["PageData"];
                    foreach (IDictionary<string, object> user in userList) {
                        int userid = 0;
                        if (user.ContainsKey("userid") && user["userid"] != null) {
                            if (int.TryParse(user["userid"].ToString(), out userid)) {
                                if (crmData.MemberIds.Contains(userid) == false) {
                                    crmData.MemberIds.Add(userid);
                                }
                            }
                        }
                    }
                }
            }
            #endregion 
            var result = _repository.AddGroup(crmData);
            if (result.Flag == 1)
            {
                //result.Id的格式：GroupId|username
                var idData = result.Id.Split('|');
                ChatInsert msgData = new ChatInsert()
                {
                    ChatType = 1,
                    GroupId = new Guid(idData[0]),
                    ChatContent = string.Format("{0}创建了{1}群", idData[1], data.GroupName),
                    ContentType = 1,
                    FriendId = 0,
                    MsgType = 2,
                    UserNo = userId,
                };
                var msg = InsertChat(msgData, userId, null);
                string msgid = null;
                if (msg.Status == 0)//消息新增成功
                {
                    Dictionary<string, object> mdata = msg.DataBody as Dictionary<string, object>;
                    msgid = mdata["mid"].ToString();
                }

                result.Id = string.Format("{0}|{1}|{2}", msgData.GroupId, pinyin, msgid);
            }

            return HandleResult(result);
        }

        public OutputResult<object> GetMembers(GetMembersModel bodyData, int userId)
        {
            List<IDictionary<string, object>> result = this._repository.GetMembers(new GroupMemberSelect()
            {
                GroupId = (Guid)bodyData.GroupId,
                MaxRecVersion = bodyData.RecVersion,
                UserNo = userId
            });
            return new OutputResult<object>(result);
        }
        #endregion

        #region --修改群资料--
        public OutputResult<object> UpdateGroup(UpdateGroupModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            //修改返回值的id数据格式
            string pinyin = PinYinConvert.ToChinese(data.GroupName, false);
            GroupUpdate crmData = new GroupUpdate()
            {
                GroupId = data.GroupId.GetValueOrDefault(),
                GroupName = data.GroupName,
                Pinyin = pinyin,
                //GroupIcon = data.GroupIcon,
                UserNo = userId
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            var oldGroupInfo = _repository.GetGroupInfo(crmData.GroupId);
            if(crmData.GroupName==oldGroupInfo.chatgroupname)
            {
                var resulttemp = new OperateResult();
                resulttemp.Id= string.Format("{0}|{1}|{2}", oldGroupInfo.chatgroupid, oldGroupInfo.pinyinname, "");
                resulttemp.Flag = 1;
                return HandleResult(resulttemp);
            }
            var result = _repository.UpdateGroup(crmData);
            if (result.Flag == 1)
            {
                //result.Id的格式：username

                ChatInsert msgData = new ChatInsert()
                {
                    ChatType = 1,
                    GroupId = crmData.GroupId,
                    ChatContent = string.Format("{0}将“{1}”修改为“{2}”", result.Id, oldGroupInfo.chatgroupname, data.GroupName),
                    ContentType = 1,
                    FriendId = 0,
                    MsgType = 7,
                    UserNo = userId,
                };

                var msg = InsertChat(msgData, userId, null);
                string msgid = null;
                if (msg.Status == 0)//消息新增成功
                {
                    Dictionary<string, object> mdata = msg.DataBody as Dictionary<string, object>;
                    msgid = mdata["mid"].ToString();
                }

                result.Id = string.Format("{0}|{1}|{2}", msgData.GroupId, pinyin, msgid);
            }
            return HandleResult(result);
        }

        public List<IDictionary<string, object>> GetRecentChatList(int userId)
        {
            DbTransaction tran = null;
            List<IDictionary<string, object>> chatList = this._repository.GetRecentChatList(tran,userId);
            List<Guid> groupchatids = new List<Guid>();
            List<int> singlechatids = new List<int>();
            foreach (IDictionary<string, object> item in chatList) {
                string chatid = item["chatid"].ToString();
                int id = 0;
                if (int.TryParse(chatid, out id))
                {
                    singlechatids.Add(id);
                }
                else {
                    Guid tmp = Guid.Empty;
                    if (Guid.TryParse(chatid, out tmp)) {
                        groupchatids.Add(tmp);
                    }
                }
            }
            if (groupchatids.Count > 0) {
                List<IDictionary<string, object>> groupmsgs = this._repository.GetRecentMsgByGroupChatIds(tran, groupchatids, userId);
                foreach (IDictionary<string, object> item in chatList) {
                    string chatid = item["chatid"].ToString();
                    List<IDictionary<string, object>> msgs = new List<IDictionary<string, object>>();
                    foreach (IDictionary<string, object> msgitem in groupmsgs) {
                        string groupid = msgitem["groupid"].ToString();
                        if (chatid.Equals(groupid)) {
                            msgs.Add(msgitem);
                        }
                    }
                    if (item.ContainsKey("msglist")) {
                        ((List<IDictionary<string, object>>)item["msglist"]).AddRange(msgs);
                    }
                    else
                    {
                        item.Add("msglist", msgs);
                    }
                }
            }
            if (singlechatids.Count > 0) {
                List<IDictionary<string, object>> groupmsgs = this._repository.GetRecentMsgByPersonalChatIds(tran, singlechatids, userId);
                foreach (IDictionary<string, object> item in chatList)
                {
                    string chatid = item["chatid"].ToString();
                    List<IDictionary<string, object>> msgs = new List<IDictionary<string, object>>();
                    foreach (IDictionary<string, object> msgitem in groupmsgs)
                    {
                        string relateuser = msgitem["relateuser"].ToString();
                        if (chatid.Equals(relateuser))
                        {
                            msgs.Add(msgitem);
                        }
                        
                    }
                    if (item.ContainsKey("msglist"))
                    {
                        ((List<IDictionary<string, object>>)item["msglist"]).AddRange(msgs);
                    }
                    else
                    {
                        item.Add("msglist", msgs);
                    }
                }
            }
            return chatList;
        }
        #endregion

        #region --新增讨论群的成员--
        public OutputResult<object> AddMembers(AddMembersModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            if (data.MemberIds == null || data.MemberIds.Count == 0)
            {
                return ShowError<object>("成员不可为空");
            }

            GroupMemberAdd crmData = new GroupMemberAdd()
            {
                GroupId = data.GroupId.HasValue ? data.GroupId.Value : Guid.Empty,
                Members = data.MemberIds,
                UserNo = userId,
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            #region 需要处理部门的情况
            if (data.DeptIds != null && data.DeptIds.Count > 0)
            {

                foreach (Guid deptid in data.DeptIds)
                {
                    PageParam pageParam = new PageParam()
                    {
                        PageIndex = 1,
                        PageSize = 10000
                    };
                    AccountUserQueryMapper accountUserQueryMapper = new AccountUserQueryMapper()
                    {
                        RecStatus = 1,
                        DeptId = deptid
                    };
                    Dictionary<string, List<IDictionary<string, object>>> pageData = this._accountRepository.GetUserList(pageParam, accountUserQueryMapper, userId);
                    List<IDictionary<string, object>> userList = pageData["PageData"];
                    foreach (IDictionary<string, object> user in userList)
                    {
                        int userid = 0;
                        if (user.ContainsKey("userid") && user["userid"] != null)
                        {
                            if (int.TryParse(user["userid"].ToString(), out userid))
                            {
                                if (crmData.Members.Contains(userid) == false)
                                {
                                    crmData.Members.Add(userid);
                                }
                            }
                        }
                    }
                }
            }
            #endregion 
            var result = _repository.AddMembers(crmData);
            //result.Id格式：邀请人名字|被邀请人1，被邀请人2....
            var resultIDData = result.Id.Split('|');
            if (resultIDData.Length != 2)
            {
                Logger.Error("新增成员，返回结果集格式不对");
                return ShowError<object>("返回结果集格式不对");
            }

            var _chatContent = "";
            if (resultIDData[1].Split(',').Length > 2)
            {
                var newMembers = resultIDData[1].Length > 12 ? resultIDData[1].Substring(0, 12) : resultIDData[1];
                if (newMembers.LastIndexOf(',') > 0)
                    newMembers = newMembers.Substring(0, newMembers.LastIndexOf(','));
                _chatContent = string.Format("{0}邀请{1}等加入了群聊", resultIDData[0], newMembers);
            }
            else
            {
                _chatContent = string.Format("{0}邀请{1}加入了群聊", resultIDData[0], resultIDData[1]);
            }
            if (result.Flag == 1)
            {

                ChatInsert msgData = new ChatInsert()
                {
                    ChatType = 1,
                    GroupId = crmData.GroupId,
                    ChatContent = _chatContent,
                    ContentType = 1,
                    FriendId = 0,
                    MsgType = 4,
                    UserNo = userId,
                };

                var msg = InsertChat(msgData, userId, null);
                string msgid = null;
                if (msg.Status == 0)//消息新增成功
                {
                    Dictionary<string, object> mdata = msg.DataBody as Dictionary<string, object>;
                    msgid = mdata["mid"].ToString();
                }
                result.Id = msgid;
            }

            return HandleResult(result);
        }
        #endregion
        #region 更新讨论群的成员
        public OutputResult<object> UpdateMembers(AddMembersModel data, string userName,int userId) {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            if (data.MemberIds == null || data.MemberIds.Count == 0)
            {
                return ShowError<object>("成员不可为空");
            }
            GroupMemberSelect groupMemberSelect = new GroupMemberSelect()
            {
                GroupId = (Guid)data.GroupId
            };
            List<IDictionary<string, object>> oldUsers = this._repository.GetMembers(groupMemberSelect);
            #region  检查本人是否存在群聊中
            bool isFoundInGroupChat = false;
            bool isAdminInGroupChat = false;
            foreach (IDictionary<string, object> userInfo in oldUsers) {
                int oldUserid = 0;
                if (userInfo.ContainsKey("userid") && userInfo["userid"] != null) {
                    if (int.TryParse(userInfo["userid"].ToString(), out oldUserid)) {
                        if (oldUserid == userId) {
                            isFoundInGroupChat = true;
                            if (userInfo.ContainsKey("isadmin") && userInfo["isadmin"] != null) {
                                int iIsAdmin = 0;
                                if (int.TryParse(userInfo["isadmin"].ToString(), out iIsAdmin)) {
                                    isAdminInGroupChat =( iIsAdmin == 1);
                                }
                            }
                        }
                    }
                }
            }
            if (isFoundInGroupChat == false ) {
                throw (new Exception("您不是群成员，无法管理群"));
            }
            if (isAdminInGroupChat == false) {
                throw (new Exception("您不是群管理员，无法管理群"));
            }
            #endregion

            GroupMemberAdd crmData = new GroupMemberAdd()
            {
                GroupId = data.GroupId.HasValue ? data.GroupId.Value : Guid.Empty,
                Members = data.MemberIds,
                UserNo = userId,
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            #region 需要处理部门的情况
            if (data.DeptIds != null && data.DeptIds.Count > 0)
            {

                foreach (Guid deptid in data.DeptIds)
                {
                    PageParam pageParam = new PageParam()
                    {
                        PageIndex = 1,
                        PageSize = 10000
                    };
                    AccountUserQueryMapper accountUserQueryMapper = new AccountUserQueryMapper()
                    {
                        RecStatus = 1,
                        DeptId = deptid
                    };
                    Dictionary<string, List<IDictionary<string, object>>> pageData = this._accountRepository.GetUserList(pageParam, accountUserQueryMapper, userId);
                    List<IDictionary<string, object>> userList = pageData["PageData"];
                    foreach (IDictionary<string, object> user in userList)
                    {
                        int userid = 0;
                        if (user.ContainsKey("userid") && user["userid"] != null)
                        {
                            if (int.TryParse(user["userid"].ToString(), out userid))
                            {
                                if (crmData.Members.Contains(userid) == false)
                                {
                                    crmData.Members.Add(userid);
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            _repository.UpdateMembers(crmData,userId);
            List<IDictionary<string, object>> newUsers = this._repository.GetMembers(groupMemberSelect);
            List<IDictionary<string, object>> addList = new List<IDictionary<string, object>>();
            List<IDictionary<string, object>> removeList = new List<IDictionary<string, object>>();

            #region 检查增加的和检查移除的
            foreach (IDictionary<string, object> oldUserInfo in oldUsers) {
                bool isFoundInNew = false;
                int oldUserId = 0;
                if (oldUserInfo.ContainsKey("userid") == false && oldUserInfo["userid"] == null) continue;
                if (int.TryParse(oldUserInfo["userid"].ToString(), out oldUserId) == false) continue;
                if (oldUserId <= 0) continue;
                foreach (IDictionary<string, object> newUserInfo in newUsers)
                {
                    int newUserId = 0;
                    if (newUserInfo.ContainsKey("userid") == false && newUserInfo["userid"] == null) continue;
                    if (int.TryParse(newUserInfo["userid"].ToString(), out newUserId) == false) continue;
                    if (newUserId <= 0) continue;
                    if (newUserId == oldUserId) {
                        isFoundInNew = true;
                        break;
                    }
                }
                if (isFoundInNew == false) {
                    removeList.Add(oldUserInfo);
                }
            }
            foreach (IDictionary<string, object> newUserInfo  in newUsers)
            {
                bool isFoundInOld = false;
                int newUserId = 0;
                if (newUserInfo.ContainsKey("userid") == false && newUserInfo["userid"] == null) continue;
                if (int.TryParse(newUserInfo["userid"].ToString(), out newUserId) == false) continue;
                if (newUserId <= 0) continue;
                foreach (IDictionary<string, object> oldUserInfo in newUsers)
                {
                    int oldUserId = 0;
                    if (oldUserInfo.ContainsKey("userid") == false && oldUserInfo["userid"] == null) continue;
                    if (int.TryParse(oldUserInfo["userid"].ToString(), out oldUserId) == false) continue;
                    if (oldUserId <= 0) continue;
                    if (newUserId == oldUserId)
                    {
                        isFoundInOld = true;
                        break;
                    }
                }
                if (isFoundInOld == false)
                {
                    addList.Add(newUserInfo);
                }
            }
            #endregion

            #region 发送踢人和加人消息
            string currentUserName = userName;
            string addUsersName = "";
            foreach (IDictionary<string, object> userInfo in addList) {
                if (userInfo.ContainsKey("username") && userInfo["username"] != null) {
                    addUsersName = addUsersName + "、" + userInfo["username"].ToString();
                }
            }
            if (addUsersName.Length > 0) {
                addUsersName = addUsersName.Substring(1);
                string _chatContent = string.Format("{0}邀请{1}加入了群聊", currentUserName, addUsersName);
                ChatInsert msgData = new ChatInsert()
                {
                    ChatType = 1,
                    GroupId = crmData.GroupId,
                    ChatContent = _chatContent,
                    ContentType = 1,
                    FriendId = 0,
                    MsgType = 4,
                    UserNo = userId,
                };

                var msg = InsertChat(msgData, userId, null);
                string msgid = null;
                if (msg.Status == 0)//消息新增成功
                {
                    Dictionary<string, object> mdata = msg.DataBody as Dictionary<string, object>;
                    msgid = mdata["mid"].ToString();
                }
            }
            string removeUsersName = "";
            foreach (IDictionary<string, object> userInfo in removeList)
            {
                if (userInfo.ContainsKey("username") && userInfo["username"] != null)
                {
                    removeUsersName = removeUsersName + "、" + userInfo["username"].ToString();
                }
            }
            if (removeUsersName.Length > 0)
            {
                removeUsersName = removeUsersName.Substring(1);
                string _chatContent = string.Format("{0}已被管理员移除群聊", removeUsersName);
                ChatInsert msgData = new ChatInsert()
                {
                    ChatType = 1,
                    GroupId = crmData.GroupId,
                    ChatContent = _chatContent,
                    ContentType = 1,
                    FriendId = 0,
                    MsgType = 4,
                    UserNo = userId,
                };

                var msg = InsertChat(msgData, userId, null);
                string msgid = null;
                if (msg.Status == 0)//消息新增成功
                {
                    Dictionary<string, object> mdata = msg.DataBody as Dictionary<string, object>;
                    msgid = mdata["mid"].ToString();
                }
            }
            #endregion
            return new OutputResult<object>("ok");
        }
        #endregion
        #region --设置成员：设置管理员，取消管理员，设置屏蔽群，取消屏蔽群，管理员踢人--
        public OutputResult<object> SetMembers(SetMembersModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }

            GroupMemberSet crmData = new GroupMemberSet()
            {
                Memberid = data.Memberid,
                GroupId = data.GroupId.GetValueOrDefault(),
                OperateType = data.OperateType,
                UserNo = userId,
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            var result = _repository.SetMembers(crmData);
            //result.Id格式：chatgroupid|username|memberid|membername
            var resultIDData = result.Id.Split('|');
            if (resultIDData.Length != 4)
            {
                Logger.Error("编辑成员，返回结果集格式不对");
                return ShowError<object>("返回结果集格式不对");
            }
            //修改成功，需推送时
            int msgType = 0;
            string pushAccounts = resultIDData[2];
            if (result.Flag == 1 && (data.OperateType == 0 || data.OperateType == 1 || data.OperateType == 4))
            {
                string mes = "";
                switch (data.OperateType)
                {
                    case 0:
                        mes = string.Format("{0}设置{1}为管理员", resultIDData[1], resultIDData[3]);
                        msgType = 8;

                        break;
                    case 1:
                        mes = string.Format("{0}取消了{1}的管理员权限", resultIDData[1], resultIDData[3]);
                        msgType = 8;
                        break;
                    case 4:
                        mes = string.Format("{0}已被管理员移除群聊", resultIDData[3]);

                        msgType = 6;
                        //pushAccounts = null;
                        break;
                }
                ChatInsert msgData = new ChatInsert()
                {
                    ChatType = 1,
                    GroupId = new Guid(resultIDData[0]),
                    ChatContent = mes,
                    ContentType = 1,
                    FriendId = 0,
                    MsgType = msgType,
                    UserNo = userId,
                };

                var msg = InsertChat(msgData, userId, pushAccounts);
                string msgid = null;
                if (msg.Status == 0)//消息新增成功
                {
                    Dictionary<string, object> mdata = msg.DataBody as Dictionary<string, object>;
                    msgid = mdata["mid"].ToString();
                }
                result.Id = msgid;
            }

            return HandleResult(result);
        }
        #endregion

        #region --（暂时无用，由群列表接口统一返回）
        //public OutputResult<object> GetMembers(GetMembersModel data, int userId)
        //{
        //    if (data == null)
        //    {
        //        return ShowError<object>("参数错误");
        //    }
        //    GroupMemberSelect crmData = new GroupMemberSelect()
        //    {
        //        GroupId = data.GroupId.GetValueOrDefault(),
        //        UserNo = userId,
        //        MaxRecVersion = data.RecVersion
        //    };
        //    if (!crmData.IsValid())
        //    {
        //        return HandleValid(crmData);
        //    }
        //    return new OutputResult<object>(_repository.GetMembers(crmData));
        //} 
        #endregion

        public OutputResult<object> DeleteGroup(DeleteGroupModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            GroupDelete crmData = new GroupDelete()
            {
                GroupId = data.GroupId.Value,
                OperateType = data.OperateType,
                UserNo = userId,
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            var result = _repository.DeleteGroup(crmData);
            //result.Id格式：username

            if (result.Flag == 1)
            {

                ChatInsert msgData = new ChatInsert()
                {
                    ChatType = 1,
                    GroupId = crmData.GroupId,
                    ChatContent = data.OperateType == 0 ? string.Format("{0}已退出群聊", result.Id) : string.Format("{0}解散了群聊", result.Id),
                    ContentType = 1,
                    FriendId = 0,
                    MsgType = data.OperateType == 0 ? 5 : 3,
                    UserNo = userId,
                };

                var msg = InsertChat(msgData, userId, null);
                string msgid = null;
                if (msg.Status == 0)//消息新增成功
                {
                    Dictionary<string, object> mdata = msg.DataBody as Dictionary<string, object>;
                    msgid = mdata["mid"].ToString();
                }
                result.Id = msgid;
            }

            return HandleResult(result);
        }

        #region --获取群列表--
        public OutputResult<object> Grouplist(GrouplistModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            GroupSelect crmData = new GroupSelect()
            {
                EntityId = data.EntityId.GetValueOrDefault(),
                BusinessId = data.BusinessId.GetValueOrDefault(),
                GroupType = data.GroupType,
                UserNo = userId,
                MaxRecVersion = data.RecVersion
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            return new OutputResult<object>(_repository.Grouplist(crmData));
        }
        #endregion

        #region --发送聊天信息--
        public OutputResult<object> SendChat(SendChatModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            ChatInsert crmData = new ChatInsert()
            {
                ChatType = data.ChatType,
                GroupId = data.GroupId.HasValue ? data.GroupId.Value : Guid.Empty,
                ChatContent = data.ChatContent,
                ContentType = data.ContentType,
                FriendId = data.FriendId,
                MsgType = 1,
                UserNo = userId,
            };
            return InsertChat(crmData, userId, null);
        }
        #endregion

        #region --获取聊天列表,每次最多50条记录--
        public OutputResult<object> ChatList(ChatListModel data, int userId)
        {
            if (data == null)
            {
                return ShowError<object>("参数错误");
            }
            ChatSelect crmData = new ChatSelect()
            {
                FriendId = data.FriendId,
                UserNo = userId,
                RecVersion = data.RecVersion,
                IsHistory = data.IsHistory,
                GroupId = data.GroupId.GetValueOrDefault()
            };
            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            List<IDictionary<string, object>> chatList = _repository.ChatList(crmData);
            #region 如果是文件，则返回文件信息
            if (chatList != null && DeviceType == DeviceType.WEB)//只有WEB才解析，手机端为了减少网络流量，这些数据都不获取
            {
                foreach (IDictionary<string, object> msg in chatList) {
                    if (msg.ContainsKey("chattype") && msg["chattype"] != null) {
                        int chattype = 0;
                        if (!int.TryParse(msg["chattype"].ToString(), out chattype)) {
                            continue;
                        }
                        if (chattype == 5) //文件
                        {
                            if (msg.ContainsKey("chatcon") && msg["chatcon"] != null) {
                                string fileid = msg["chatcon"].ToString();
                                try
                                {

                                    FileInfoModel fileInfo = _fileService.GetOneFileInfo(null, fileid);
                                    if (fileInfo != null)
                                    {
                                        msg.Add("file", fileInfo);
                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                    }
                    if (msg.ContainsKey("reccreator") && msg["reccreator"] != null) {
                        //处理发送人
                        try
                        {
                            int recc = 0;
                            recc = int.Parse(msg["reccreator"].ToString());
                            UserInfo userInfo = WebChatCachedDataUtils.getInstance().getUserInfo(recc);
                            if (userInfo != null) {
                                msg.Add("ud", userInfo);
                            }
                        }
                        catch (Exception ex) {

                        }
                    }
                    if (msg.ContainsKey("groupid") && msg["groupid"] != null) {
                        Guid gid = Guid.Empty;
                        if (Guid.TryParse(msg["groupid"].ToString(), out gid)){
                            if (gid != Guid.Empty) {
                                try
                                {
                                    ChatGroupModel groupInfo = WebChatCachedDataUtils.getInstance().GetGroupInfo(gid);
                                    if (groupInfo != null)
                                    {
                                        msg.Add("gd", groupInfo);
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                    }
                }
            }
            #endregion
            return new OutputResult<object>(chatList);
        }
        #endregion

        #region --获取聊天未读消息--
        public OutputResult<object> ChatUnreadList(int userId, long recversion = 0)
        {
            return new OutputResult<object>(_repository.ChatUnreadList(userId, recversion));
        }
        #endregion

        #region --已读消息回写--
        public OutputResult<object> ReadedCallback(ReadedCallbackModel data, int userId)
        {
            if (data == null || data.ChatMsgIds == null || data.ChatMsgIds.Count == 0)
            {
                return ShowError<object>("参数错误");
            }
            if (data.ChatMsgIds.Count == 0)
            {
                return ShowError<object>("消息id不可为空");
            }
            var ids = new List<Guid>();
            foreach (var m in data.ChatMsgIds)
            {
                ids.Add(new Guid(m));
            }

            var result = _repository.ReadedCallback(ids, userId);

            return HandleResult(result);
        }
        #endregion

        
        private OutputResult<object> InsertChat(ChatInsert crmData, int userId, string pushAccounts)
        {
            

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }
            var result = _repository.InsertChatMessage(crmData);
            if (result.Flag == 0)
            {
                return HandleResult(result);
            }
            //result.Id格式：chatmsgid|entityid|businessid|recversion|receivers|reccreated|chattype
            var resultIDData = result.Id.Split('|');

            if (resultIDData.Length != 7)
            {
                Logger.Error("新增消息记录，返回结果集格式不对");
                return ShowError<object>("返回结果集格式不对");
            }
            //如果推送人已经指定，则不从数据库返回的数据获取，否则，直接从数据库插入消息后返回
            if (string.IsNullOrEmpty(pushAccounts))
            {
                pushAccounts = resultIDData[4];
            }
            string fileid = crmData.ContentType == 1 ? "" : crmData.ChatContent;
            var customContent = new Dictionary<string, object>();
            customContent.Add("mid", resultIDData[0]);//chatmsgid
            customContent.Add("ctype", resultIDData[6]);//ChatType
            customContent.Add("gid", crmData.GroupId);//groupid
            customContent.Add("eid", resultIDData[1]);//entityid
            customContent.Add("bid", resultIDData[2]);//businessid
            customContent.Add("mt", crmData.MsgType); //mtype
            customContent.Add("ct", crmData.ContentType); //contype
            customContent.Add("fid", fileid);  //fileid
            customContent.Add("s", userId);  //sender
            customContent.Add("t", resultIDData[5]); //created time
            customContent.Add("v", resultIDData[3]);//recversion
            Task.Run(() =>
            {
                int messageType = 1;
                string titile = "聊天消息";
                string message = crmData.ContentType == 1 ? crmData.ChatContent : "您有一条新消息";

                //如果是长消息
                if (message.Length > 120)
                {
                    messageType = 2;
                    message = string.Format("{0}...", crmData.ChatContent.Substring(0, 30));
                }

                var pushResult = _pushServices.PushMessage(pushAccounts, titile, message, customContent, messageType, DateTime.Now.AddDays(-1).ToString());
                WebChatResponseHandler.getInstance().addMessages(pushAccounts, titile, message, customContent);//web聊天
            });
            return new OutputResult<object>(customContent);
        }
        public OutputResult<object> GetUserList(string searchKey, int userId)
        {
            try
            {
                PageParam pageParam = new PageParam()
                {
                    PageIndex = 1,
                    PageSize = 100000
                };
                AccountUserQueryMapper accountUserQueryMapper = new AccountUserQueryMapper()
                {
                    DeptId = Guid.Empty,
                    RecStatus = 1,
                    UserName = "",
                    UserPhone = ""
                };
                Dictionary<string, List<IDictionary<string, object>>> result = _accountRepository.GetUserList(pageParam, accountUserQueryMapper, userId);
                List<IDictionary<string, object>> userList = result["PageData"];
                List<UserInfo> fullUserList = new List<UserInfo>();
                #region 整理用户数据
                foreach (IDictionary<string, object> item in userList)
                {
                    int userid = 0;
                    if (item.ContainsKey("userid") && item["userid"] != null)
                    {
                        int.TryParse(item["userid"].ToString(), out userid);
                    }
                    if (userid <= 0) continue;
                    UserInfo userInfo = new UserInfo();
                    userInfo.UserId = userid;
                    if (item.ContainsKey("usericon") && item["usericon"] != null)
                    {
                        userInfo.UserIcon = item["usericon"].ToString();
                    }

                    if (item.ContainsKey("username") && item["username"] != null)
                    {
                        userInfo.UserName = item["username"].ToString();
                    }
                    if (item.ContainsKey("usersex") && item["usersex"] != null)
                    {
                        int sex = 0;
                        if (int.TryParse(item["usersex"].ToString(), out sex))
                        {
                            userInfo.UserSex = sex;
                        }
                    }
                    if (userInfo.UserName != null && userInfo.UserName.Length > 0)
                    {
                        userInfo.NamePinyin = PingYinHelper.ConvertToAllSpell(userInfo.UserName);
                        userInfo.NamePinyin_FistChar = PingYinHelper.ConvertFirstChar(userInfo.UserName);
                    }
                    fullUserList.Add(userInfo);
                }
                #endregion

                #region 排序
                List<UserInfo> rawList = new List<UserInfo>();
                string lowersearch = searchKey ==null?"": searchKey.ToLower();
                foreach (UserInfo item in fullUserList) {
                    bool IsMatch = false;
                    if (item.UserName.ToLower().IndexOf(lowersearch) >= 0) {
                        IsMatch = true;
                    }
                    if (!IsMatch) {
                        if (item.NamePinyin.ToLower().IndexOf(lowersearch) > 0) {
                            IsMatch = true;
                        }
                    }
                    if (!IsMatch) {
                        if (item.NamePinyin_FistChar.ToLower().IndexOf(lowersearch) > 0) {
                            IsMatch = true;
                        }
                    }
                    if (IsMatch) {
                        rawList.Add(item);
                    }
                }
                #endregion
                #region 处理分组
                rawList.Sort((o1, o2) => {
                    return o1.NamePinyin.CompareTo(o2.NamePinyin);
                });
                Dictionary<string, List<UserInfo>> returnDict = new Dictionary<string, List<UserInfo>>();
                foreach (UserInfo item in rawList) {
                    char firstChar = item.NamePinyin.ToUpper()[0];
                    if (!(firstChar >= 'A' && firstChar <= 'Z')) {
                        firstChar = '#';
                    }
                    List<UserInfo> tmplist = null;
                    if (returnDict.ContainsKey(firstChar.ToString()))
                    {
                        tmplist = returnDict[firstChar.ToString()];
                    }
                    else {
                        tmplist = new List<UserInfo>();
                        returnDict.Add(firstChar.ToString(), tmplist);
                    }
                    tmplist.Add(item);

                }
                #endregion
                return new OutputResult<object>(returnDict);
            }
            catch (Exception ex)
            {
                return new OutputResult<object>(new List<Dictionary<string, object>>());
            }
        }
    }
}
