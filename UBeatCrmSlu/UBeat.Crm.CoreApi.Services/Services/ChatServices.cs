﻿using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Chat;
using UBeat.Crm.CoreApi.DomainModel.PushService;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Chat;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ChatServices : BasicBaseServices
    {
        IChatRepository _repository;
        PushServices _pushServices;
        private static readonly Logger Logger = LogManager.GetLogger(typeof(ChatServices).FullName);

        public ChatServices(IChatRepository repository, PushServices pushService)
        {
            _repository = repository;
            _pushServices = pushService;

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
            return new OutputResult<object>(_repository.ChatList(crmData));
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

            });
            return new OutputResult<object>(customContent);
        }
    }
}
