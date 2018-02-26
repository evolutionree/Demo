using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Dynamics;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.DomainModel.Notify;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.Dynamics;
using UBeat.Crm.CoreApi.Repository.Repository.Message;
using UBeat.Crm.CoreApi.Repository.Repository.Notify;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.Services.Models.PushService;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class MessageServices
    {
        private readonly IMessageRepository _msgRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly PushServices _pushServices;
        private readonly CacheServices _cacheService;
        private readonly IDynamicRepository _dynamicRepository;

        #region --GetDbConnect--
        private static string _connectString;
        public DbConnection GetDbConnect(string connectStr = null)
        {
            if (string.IsNullOrEmpty(connectStr))
            {
                if (_connectString == null)
                {
                    IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
                    _connectString = config.GetConnectionString("DefaultDB");
                }

                connectStr = _connectString;
            }

            return new NpgsqlConnection(connectStr);
        }
        #endregion


        public MessageServices(CacheServices cacheService)
        {
            _cacheService = cacheService;

            _msgRepository = ServiceLocator.Current.GetInstance<IMessageRepository>();
            _pushServices = ServiceLocator.Current.GetInstance<PushServices>();
            _accountRepository = ServiceLocator.Current.GetInstance<IAccountRepository>();
            _dynamicRepository = ServiceLocator.Current.GetInstance<IDynamicRepository>();
        }



        #region --写入离线消息，并发起推送--

        public bool HasMessageConfig(Guid entityid, EntityModelType entityModel, string funccode, Guid? relentityid = null)
        {
            var configData = GetMessageConfigInfo(entityid, funccode, relentityid);
            return configData == null;
        }


        #region --异步方式写离线消息并发送消息--
        /// <summary>
        /// 写入离线消息，并发起推送
        /// </summary>
        /// <param name="msgparam">消息的参数数据</param>
        /// <param name="userNumber">当前用户</param>
        /// <param name="isPushMsg">是否发起推送</param>
        /// <param name="pushCustomContent">推送消息扩展数据</param>
        public void WriteMessageAsyn(MessageParameter msgparam, int userNumber, Dictionary<string, object> pushCustomContent = null)
        {
            Task.Run(() =>
            {
                WriteMessage(null, msgparam, userNumber, pushCustomContent);
            });
        }
        #endregion

        #region --同步方式写离线消息并发送消息--
        /// <summary>
        /// 写入离线消息，并发起推送
        /// </summary>
        /// <param name="msgparam">消息的参数数据</param>
        /// <param name="userNumber">当前用户</param>
        /// <param name="pushCustomContent">推送消息扩展数据</param>
        /// <param name="typeStatus">发消息方式，0=消息+动态，1=消息，2=动态</param>
        public void WriteMessage(DbTransaction tran, MessageParameter msgparam, int userNumber, Dictionary<string, object> pushCustomContent = null, int typeStatus = 0)
        {
            if (msgparam == null)
                return;

            var configData = GetMessageConfigInfo(msgparam.EntityId, msgparam.FuncCode, msgparam.RelEntityId, msgparam.FlowId);
            if (configData == null)
            {
                return;
            }
            List<int> receiverIds = new List<int>();
            foreach (var receiverItem in msgparam.Receivers)
            {
                if (receiverItem.Value != null && receiverItem.Value.Count > 0 && configData.MessageUserType.Contains(receiverItem.Key))
                {
                    receiverIds.AddRange(receiverItem.Value);
                }
            }
            receiverIds = receiverIds.Distinct().ToList();


            bool msgSuccess = true;
            bool isLocalTransaction = false;
            DbConnection conn = null;
            if (tran == null)
            {
                isLocalTransaction = true;
                conn = GetDbConnect();
                conn.Open();
                tran = conn.BeginTransaction();
            }
            try
            {
                var msgContent = FormatMsgTemplate(msgparam.TemplateKeyValue, configData.MsgTemplate);
                MsgParamInfo tempData = null;
                //如果是动态点赞和动态评论，则先获取动态模板内容
                if (configData.MsgStyleType == MessageStyleType.DynamicPrase)
                {
                    var template = _dynamicRepository.GetDynamicTemplate(msgparam.EntityId, msgparam.TypeId);
                    tempData = new MsgParamInfo()
                    {
                        Template = JsonConvert.DeserializeObject(template),
                        Data = JsonConvert.DeserializeObject(string.IsNullOrEmpty(msgparam.ParamData) ? "{}" : msgparam.ParamData)
                    };
                }
                //如果是动态消息，则先插入动态
                //if (configData.MsgType == MessageType.DynamicMessage&& typeStatus != 1)
                if (configData.MsgType == MessageType.DynamicMessage )
                {
                    var dynamicInfo = new DynamicInsertInfo()
                    {
                        DynamicType = configData.MsgStyleType == MessageStyleType.EntityDynamic || configData.MsgStyleType == MessageStyleType.WorkReport ? DynamicType.Entity : DynamicType.System,
                        EntityId = msgparam.EntityId,
                        TypeId = msgparam.TypeId,
                        BusinessId = msgparam.BusinessId,
                        RelEntityId = msgparam.RelEntityId,
                        RelBusinessId = msgparam.RelBusinessId,
                        Content = msgContent,
                        TemplateData = msgparam.ParamData,
                    };
                    msgSuccess = _dynamicRepository.InsertDynamic(tran, dynamicInfo, userNumber, out tempData);

                }
                //写离线消息
                if (msgSuccess && typeStatus != 2)
                {
                    if (receiverIds.Count > 0)//没有接收人，则不发消息
                    {
                        if (tempData == null)
                        {
                            tempData = new MsgParamInfo();
                            tempData.Data = JsonConvert.DeserializeObject(string.IsNullOrEmpty(msgparam.ParamData) ? "{}" : msgparam.ParamData);
                        }
                        //var entityInfotemp = _entityProRepository.GetEntityInfo(typeId);
                        tempData.EntityId = msgparam.EntityId;
                        tempData.EntityName = msgparam.EntityName;
                        tempData.TypeId = msgparam.TypeId;
                        tempData.BusinessId = msgparam.BusinessId;
                        tempData.RelEntityId = msgparam.RelEntityId.GetValueOrDefault();
                        tempData.RelEntityName = msgparam.RelEntityName;
                        tempData.RelBusinessId = msgparam.RelBusinessId;
                        tempData.CopyUsers = msgparam.CopyUsers;
                        tempData.ApprovalUsers = msgparam.ApprovalUsers;

                        var msgInfo = new MessageInsertInfo()
                        {
                            EntityId = msgparam.EntityId,
                            BusinessId = msgparam.BusinessId,
                            MsgGroupId = configData.MsgGroupId,
                            MsgStyleType = configData.MsgStyleType,
                            MsgTitle = FormatMsgTemplate(msgparam.TemplateKeyValue, configData.TitleTemplate),
                            MsgTitleTip = "",
                            MsgContent = msgContent,
                            MsgpParam = tempData == null ? "" : JsonConvert.SerializeObject(tempData),
                            ReceiverIds = receiverIds,
                        };
                        msgSuccess = _msgRepository.WriteMessage(tran, msgInfo, userNumber);
                    }
                }

                if (msgSuccess == false)
                    throw new Exception("写入消息失败");
                if (isLocalTransaction)
                {
                    tran.Commit();
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw ex;
            }
            finally
            {
                if (isLocalTransaction)
                {
                    conn.Close();
                    conn.Dispose();
                }
                //推送消息

                if (!string.IsNullOrEmpty(configData.NotifyTemplate) && msgSuccess)
                {
                    var pushMsg = new AccountsPushExtModel()
                    {
                        Accounts = receiverIds.Select(m => m.ToString()).ToList(),
                        Title = FormatMsgTemplate(msgparam.TemplateKeyValue, configData.TitleTemplate),
                        Message = FormatMsgTemplate(msgparam.TemplateKeyValue, configData.NotifyTemplate),
                        SendTime = DateTime.Now.AddYears(-1).ToString(),
                        CustomContent = pushCustomContent
                    };

                    _pushServices.PushMessage(pushMsg.Accounts, pushMsg.Title, pushMsg.Message, pushMsg.CustomContent, 0, pushMsg.SendTime);
                }


            }

        }
        #endregion
        #endregion


        #region --实体类型离线消息的公共方法--

        //获取实体的圈子成员数据
        public EntityMemberModel GetEntityMember(Dictionary<string, object> entityDataDetail)
        {
            var model = new EntityMemberModel();
            if (entityDataDetail == null)
                return model;

            //相关人
            var viewusersobj = entityDataDetail.ContainsKey("viewusers") ? entityDataDetail["viewusers"] : null;
            model.ViewUsers = viewusersobj == null ? new List<int>() : viewusersobj.ToString().Split(',').Select(m => int.Parse(m)).ToList();

            //负责人
            var recmanager = entityDataDetail.ContainsKey("recmanager") ? entityDataDetail["recmanager"] ?? "" : "";
            var recmanagerId = 0;
            int.TryParse(recmanager.ToString(), out recmanagerId);
            model.RecManager = recmanagerId;

            //创建人
            var recCreateUser = entityDataDetail.ContainsKey("reccreator") ? entityDataDetail["reccreator"] ?? "" : "";
            var recCreateUserId = 0;
            int.TryParse(recCreateUser.ToString(), out recCreateUserId);
            model.RecCreateUserId = recCreateUserId;

            //抄送人
            var copyusersobj = entityDataDetail.ContainsKey("copyusers") ? entityDataDetail["copyusers"] : null;
            model.CopyUsers = copyusersobj == null ? new List<int>() : copyusersobj.ToString().Split(',').Select(m => int.Parse(m)).ToList();

            //关注人
            var followusersobj = entityDataDetail.ContainsKey("followusers") ? entityDataDetail["followusers"] : null;
            model.FollowUsers = followusersobj == null ? new List<int>() : followusersobj.ToString().Split(',').Select(m => int.Parse(m)).ToList();

            model.RecName = entityDataDetail.ContainsKey("recname") && entityDataDetail["recname"] != null ? entityDataDetail["recname"].ToString() : string.Empty;

            model.RecCode= entityDataDetail.ContainsKey("reccode") && entityDataDetail["reccode"] != null ? entityDataDetail["reccode"].ToString() : string.Empty;
            return model;
        }


        /// <summary>
        /// 获取实体消息参数数据
        /// </summary>
        public MessageParameter GetEntityMsgParameter(SimpleEntityInfo entityInfo, Guid bussinessId, Guid relbussinessId, string funcCode, int userNumber, EntityMemberModel newMembers, EntityMemberModel oldMembers = null, string msgpParam = null, Guid? workflowid = null)
        {
            try
            {
                var configData = GetMessageConfigInfo(entityInfo.EntityId, funcCode, entityInfo.RelEntityId, workflowid);
                if (configData == null)
                {
                    return null;
                }
                if (oldMembers == null)
                    oldMembers = new EntityMemberModel();
                if (newMembers == null)
                    newMembers = new EntityMemberModel();
                //新增的相关人
                List<int> addViewusers = newMembers.ViewUsers.Except(oldMembers.ViewUsers).ToList();
                //删除的相关人
                List<int> deleteViewusers = oldMembers.ViewUsers.Except(newMembers.ViewUsers).ToList();
                //如果没有消息内容包含用户信息的数据，则从数据库获取

                var users = new List<int>();
                users.Add(userNumber);
                users.Add(newMembers.RecManager);
                users.Add(oldMembers.RecManager);
                users.Add(newMembers.RecCreateUserId);
                users.Add(oldMembers.RecCreateUserId);
                users.AddRange(newMembers.CopyUsers);
                users.AddRange(oldMembers.CopyUsers);
                users.AddRange(addViewusers);
                users.AddRange(deleteViewusers);
                var userInfos = GetUserInfoList(users.Distinct().ToList());

                var operatorInfo = userInfos.Find(m => m.UserId == userNumber);
                var newRecManagerInfo = userInfos.Find(m => m.UserId == newMembers.RecManager);
                var oldRecManagerInfo = userInfos.Find(m => m.UserId == oldMembers.RecManager);
                var addViewuserInfos = userInfos.Where(m => addViewusers.Exists(a => a == m.UserId));
                var deleteViewuserInfos = userInfos.Where(m => deleteViewusers.Exists(a => a == m.UserId));

                var msg = new MessageParameter();
                msg.EntityId = entityInfo.EntityId;
                msg.EntityName = entityInfo.EntityName;
                msg.TypeId = entityInfo.CategoryId;
                msg.RelBusinessId = relbussinessId;
                msg.RelEntityId = entityInfo.RelEntityId;
                msg.RelEntityName = entityInfo.RelEntityName;
                msg.BusinessId = bussinessId;
                msg.ParamData = msgpParam;
                msg.FuncCode = funcCode;
                msg.FlowId = workflowid;

                msg.Receivers = GetEntityMessageReceivers(newMembers, oldMembers);

                var paramData = new Dictionary<string, string>();
                paramData.Add("entityname", entityInfo.EntityName);//实体名称
                paramData.Add("relentityname", entityInfo.RelEntityName);//实体名称
                paramData.Add("operator", operatorInfo == null ? "" : operatorInfo.UserName);//操作人
                paramData.Add("recname", newMembers.RecName);//记录名称
                paramData.Add("reccode", newMembers.RecCode);//记录编码
                paramData.Add("beforeTransferManager", oldRecManagerInfo == null ? "" : oldRecManagerInfo.UserName);//转移前负责人名称
                paramData.Add("afterTransferManager", newRecManagerInfo == null ? "" : newRecManagerInfo.UserName);//转移后负责人名称
                paramData.Add("viewuseradded", string.Join(",", addViewuserInfos.Select(m => m.UserName).ToList()));//新增的相关人名称
                paramData.Add("viewuserremoved", string.Join(",", deleteViewuserInfos.Select(m => m.UserName).ToList()));//移除的相关人名称
                msg.TemplateKeyValue = paramData;
                return msg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        #region --获取实体的消息接收人--
        /// <summary>
        /// 获取实体的消息接收人
        /// </summary>
        /// <param name="newMembers">新实体对象的成员数据</param>
        /// <param name="oldMembers">旧实体对象的成员数据</param>
        /// <returns></returns>
        public Dictionary<MessageUserType, List<int>> GetEntityMessageReceivers(EntityMemberModel newMembers, EntityMemberModel oldMembers = null)
        {
            if (oldMembers == null)
                oldMembers = new EntityMemberModel();
            if (newMembers == null)
                newMembers = new EntityMemberModel();
            //新增的相关人
            List<int> addViewusers = newMembers.ViewUsers.Except(oldMembers.ViewUsers).ToList();
            //删除的相关人
            List<int> deleteViewusers = oldMembers.ViewUsers.Except(newMembers.ViewUsers).ToList();



            var receivers = new Dictionary<MessageUserType, List<int>>();

            if (newMembers.RecManager > 0)
            {
                receivers.Add(MessageUserType.EntityManager, new List<int>() { newMembers.RecManager });
            }
            if (oldMembers.RecManager > 0)
            {
                receivers.Add(MessageUserType.EntityOldManager, new List<int>() { oldMembers.RecManager });
            }
            receivers.Add(MessageUserType.EntityViewUser, newMembers.ViewUsers);
            receivers.Add(MessageUserType.EntityOldViewUser, oldMembers.ViewUsers);
            receivers.Add(MessageUserType.EntityViewUserAdd, addViewusers);
            receivers.Add(MessageUserType.EntityViewUserRemove, deleteViewusers);
            receivers.Add(MessageUserType.EntityMember, newMembers.Members);
            receivers.Add(MessageUserType.EntityOldMember, oldMembers.Members);
            receivers.Add(MessageUserType.EntityFollowUser, newMembers.FollowUsers);


            return receivers;
        }
        #endregion


        /// <summary>
        /// 获取日报周报消息参数数据
        /// </summary>
        public MessageParameter GetDailyMsgParameter(DateTime reportdate, SimpleEntityInfo entityInfo, Guid bussinessId, Guid relbussinessId, string funcCode, int userNumber, EntityMemberModel newMembers, EntityMemberModel oldMembers = null, string msgpParam = null)
        {
            try
            {
                var configData = GetMessageConfigInfo(entityInfo.EntityId, funcCode, entityInfo.RelEntityId);
                if (configData == null)
                {
                    return null;
                }
                if (oldMembers == null)
                    oldMembers = new EntityMemberModel();
                if (newMembers == null)
                    newMembers = new EntityMemberModel();

                //如果没有消息内容包含用户信息的数据，则从数据库获取
                var users = new List<int>();
                users.Add(userNumber);
                users.Add(newMembers.RecCreateUserId);
                users.Add(oldMembers.RecCreateUserId);
                var userInfos = GetUserInfoList(users.Distinct().ToList());

                var operatorInfo = userInfos.Find(m => m.UserId == userNumber);
                var reccreaterInfo = userInfos.Find(m => m.UserId == newMembers.RecCreateUserId);


                var msg = new MessageParameter();
                msg.EntityId = entityInfo.EntityId;
                msg.EntityName = entityInfo.EntityName;
                msg.TypeId = entityInfo.CategoryId;
                msg.RelBusinessId = relbussinessId;
                msg.RelEntityId = entityInfo.RelEntityId;
                msg.RelEntityName = entityInfo.RelEntityName;
                msg.BusinessId = bussinessId;
                msg.ParamData = msgpParam;
                msg.FuncCode = funcCode;
                msg.Receivers = GetDailyMessageReceivers(newMembers, oldMembers);
                msg.CopyUsers = msg.Receivers[MessageUserType.DailyCarbonCopyUser];
                msg.ApprovalUsers = msg.Receivers[MessageUserType.DailyApprover];

                var paramData = new Dictionary<string, string>();
                paramData.Add("operator", operatorInfo == null ? "" : operatorInfo.UserName);//操作人
                paramData.Add("reccreater", reccreaterInfo == null ? "" : reccreaterInfo.UserName);//创建人
                //paramData.Add("recname", newMembers.RecName);//记录名称
                GregorianCalendar gc = new GregorianCalendar();
                int weekOfYear = gc.GetWeekOfYear(reportdate, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                var beginDate = string.Format("{0:yyyy-MM-dd}", reportdate);
                var endDate = string.Format("{0:yyyy-MM-dd}", reportdate.AddDays(6));
                paramData.Add("dailytitle", beginDate);//记录名称
                paramData.Add("weeklytitle", string.Format("第{0}周 {1}至{2}", weekOfYear, beginDate, endDate));//记录名称
                msg.TemplateKeyValue = paramData;
                return msg;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        #region --获取日报周报的消息接收人--
        /// <summary>
        /// 获取日报周报的消息接收人
        /// </summary>
        /// <param name="newMembers">新实体对象的成员数据</param>
        /// <param name="oldMembers">旧实体对象的成员数据</param>
        /// <returns></returns>
        public Dictionary<MessageUserType, List<int>> GetDailyMessageReceivers(EntityMemberModel newMembers, EntityMemberModel oldMembers = null)
        {
            if (oldMembers == null)
                oldMembers = new EntityMemberModel();
            if (newMembers == null)
                newMembers = new EntityMemberModel();
            //编辑保留的抄送人
            List<int> copyusers = newMembers.CopyUsers.Intersect(oldMembers.CopyUsers).ToList();
            //编辑保留的批阅人
            List<int> Viewusers = newMembers.ViewUsers.Intersect(oldMembers.ViewUsers).ToList();
            //编辑新增的抄送人
            List<int> newcopyusers = newMembers.CopyUsers.Except(oldMembers.CopyUsers).ToList();
            //编辑新增的批阅人
            List<int> newViewusers = newMembers.ViewUsers.Except(oldMembers.ViewUsers).ToList();
            //删除的抄送人
            List<int> deletecopyusers = oldMembers.CopyUsers.Except(newMembers.CopyUsers).ToList();
            //删除的批阅人
            List<int> deleteViewusers = oldMembers.ViewUsers.Except(newMembers.ViewUsers).ToList();


            var receivers = new Dictionary<MessageUserType, List<int>>();

            if (newMembers.RecCreateUserId > 0)
            {
                receivers.Add(MessageUserType.DailyCreateUser, new List<int>() { newMembers.RecCreateUserId });
            }

            receivers.Add(MessageUserType.DailyApprover, newMembers.ViewUsers);
            receivers.Add(MessageUserType.DailyCarbonCopyUser, newMembers.CopyUsers);
            receivers.Add(MessageUserType.DailyNewCopyUser, newcopyusers);
            receivers.Add(MessageUserType.DailyNewApprover, newViewusers);
            receivers.Add(MessageUserType.DailyCopyUserDeleted, deletecopyusers);
            receivers.Add(MessageUserType.DailyApproverDeleted, deleteViewusers);
            receivers.Add(MessageUserType.DailyEditApprover, Viewusers);
            receivers.Add(MessageUserType.DailyEditCarbonCopyUser, copyusers);

            return receivers;
        }
        #endregion

        #endregion

        #region --流程节点消息接收人--
        /// <summary>
        /// 流程节点消息接收人
        /// </summary>
        /// <param name="createUser"></param>
        /// <param name="approvers"></param>
        /// <param name="copyusers"></param>
        /// <param name="completedApprovers"></param>
        /// <returns></returns>
        public Dictionary<MessageUserType, List<int>> GetWorkFlowMessageReceivers(int createUser, List<int> approvers, List<int> copyusers, List<int> completedApprovers = null)
        {
            var receivers = new Dictionary<MessageUserType, List<int>>();

            receivers.Add(MessageUserType.WorkFlowCreateUser, new List<int>() { createUser });
            receivers.Add(MessageUserType.WorkFlowCarbonCopyUser, copyusers);
            receivers.Add(MessageUserType.WorkFlowNextApprover, approvers);
            receivers.Add(MessageUserType.WorkFlowCompletedApprover, completedApprovers);
            return receivers;
        }
        #endregion

        public List<DomainModel.Account.UserInfo> GetUserInfoList(List<int> userids)
        {
            return _accountRepository.GetUserInfoList(userids);

        }

        /// <summary>
        /// 获取消息接收人列表
        /// </summary>
        /// <param name="entityid"></param>
        /// <param name="businessid"></param>
        /// <returns></returns>
        public List<MessageReceiverInfo> GetMessageRecevers(Guid entityid, Guid businessid)
        {
            return _msgRepository.GetMessageRecevers(entityid, businessid);
        }

        /// <summary>
        /// 增量获取消息列表
        /// </summary>
        /// <param name="incrementPage"></param>
        /// <param name="userNumber"></param>
        /// <param name="entityId"></param>
        /// <param name="businessId"></param>
        /// <param name="msgGroupIds"></param>
        /// <returns></returns>
        public IncrementPageDataInfo<MessageInfo> GetMessageList(IncrementPageParameter incrementPage, int userNumber, Guid? entityId = null, Guid? businessId = null, List<MessageGroupType> msgGroupIds = null, List<MessageStyleType> msgStyleType = null)
        {
            Guid entityIdTemp = entityId ?? Guid.Empty;
            Guid businessIdTemp = businessId ?? Guid.Empty;
            return _msgRepository.GetMessageList(entityIdTemp, businessIdTemp, msgGroupIds, msgStyleType, incrementPage, userNumber);
        }

        /// <summary>
        /// 分页获取消息列表
        /// </summary>
        /// <param name="userNumber"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="entityId"></param>
        /// <param name="businessId"></param>
        /// <param name="msgGroupIds"></param>
        /// <returns></returns>
        public PageDataInfo<MessageInfo> GetMessageList(int userNumber, int pageIndex = -1, int pageSize = 100, Guid? entityId = null, Guid? businessId = null, List<MessageGroupType> msgGroupIds = null, List<MessageStyleType> msgStyleType = null)
        {
            Guid entityIdTemp = entityId ?? Guid.Empty;
            Guid businessIdTemp = businessId ?? Guid.Empty;
            return _msgRepository.GetMessageList(entityIdTemp, businessIdTemp, msgGroupIds, msgStyleType, pageIndex, pageSize, userNumber);
        }

        /// <summary>
        /// 回写消息
        /// </summary>
        /// <param name="messageids"></param>
        /// <param name="userNumber"></param>
        public void MessageWriteBack(List<Guid> messageids, int userNumber)
        {
            _msgRepository.MessageWriteBack(messageids, userNumber);
        }

        /// <summary>
        /// 更新消息状态
        /// </summary>
        /// <param name="messages"></param>
        /// <param name="userNumber"></param>
        public void UpdateMessageStatus(List<MsgWriteBackInfo> messages, int userNumber)
        {
            _msgRepository.UpdateMessageStatus(messages, userNumber);
        }

        /// <summary>
        /// 统计未读消息数量
        /// </summary>
        /// <param name="msgGroupIds"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic StatisticUnreadMessage(List<MessageGroupType> msgGroupIds, int userNumber)
        {
            return _msgRepository.StatisticUnreadMessage(msgGroupIds, userNumber);
        }


        #region ---private Methor---

        #region --格式化消息模板--
        /// <summary>
        /// 格式化消息模板
        /// </summary>
        /// <param name="paramData"></param>
        /// <param name="template"></param>
        /// <returns></returns>
        private string FormatMsgTemplate(Dictionary<string, string> paramData, string template)
        {
            if (template == null)
                return string.Empty;
            foreach (var item in paramData)
            {
                string key = "{" + item.Key + "}";
                if (template.Contains(key))
                    template = template.Replace(key, item.Value);
            }
            return template.Trim('{').Trim('}');
        }

        #endregion

        #region --获取消息配置数据--

        private MessageConfigInfo GetMessageConfigInfo(Guid entityid, string funccode, Guid? relentityid = null, Guid? workflowid = null)
        {

            var listData = GetMessageConfigInfo();
            if (listData == null)
                return null;
            if (workflowid.HasValue)//如果有值，则直接通过流程匹配
            {
                var flowResults = listData.Where(m => m.FuncCode == funccode && m.FlowId == workflowid.GetValueOrDefault());
                if (flowResults.Count() == 0)
                {
                    flowResults = listData.Where(m => m.FuncCode == funccode && m.FlowId == new Guid("00000000-0000-0000-0000-000000000001"));
                }
                if (flowResults.Count() == 1)
                {
                    return flowResults.FirstOrDefault();
                }
                else
                {
                    var flowResultstemp = flowResults.Where(m => m.EntityId == entityid);
                   
                    if(flowResultstemp.Count()==0)
                        flowResultstemp = flowResults.Where(m => m.EntityId == Guid.Empty);

                    if (flowResultstemp.Count() == 1)
                    {
                        return flowResultstemp.FirstOrDefault();
                    }
                    flowResultstemp = flowResults.Where(m => m.RelEntityId == relentityid);
                    if (flowResultstemp.Count() == 0)
                        flowResultstemp = flowResults.Where(m => m.RelEntityId == Guid.Empty);
                    return flowResultstemp.FirstOrDefault();
                }
                
            }

            //1、获取funccode，entityid和relentityid完全匹配的模板
            var result = listData.Where(m => m.FuncCode == funccode && m.EntityId == entityid && m.RelEntityId == relentityid).FirstOrDefault();
            if (result == null)
            {
                //2.如果不存在完全匹配的模板，则获取funccode和relentityid匹配的关联实体的通用模板
                result = listData.Where(m => m.FuncCode == funccode && m.EntityId == Guid.Empty && m.RelEntityId == relentityid).FirstOrDefault();
                if (result == null)
                {
                    //3.获取funccode和entityid匹配的实体通用模板
                    result = listData.Where(m => m.FuncCode == funccode && m.EntityId == entityid && m.RelEntityId == Guid.Empty).FirstOrDefault();
                    if (result == null)
                    {
                        //4.获取funccode匹配的通用模板
                        
                        result = listData.Where(m => m.FuncCode == funccode && m.EntityId == Guid.Empty && m.RelEntityId == Guid.Empty && m.FlowId == Guid.Empty).FirstOrDefault();
                        if (result == null)
                        {
                            result = listData.Where(m => m.FuncCode == funccode && m.EntityId == Guid.Empty && m.RelEntityId == Guid.Empty && m.FlowId == new Guid("00000000-0000-0000-0000-000000000001")).FirstOrDefault();
                        }
                    }
                }
            }

            return result;
        }

        private List<MessageConfigInfo> GetMessageConfigInfo()
        {
            List<MessageConfigInfo> data = null;
            string cacheKey = CacheKeyManager.MessageConfigKey;

            if (_cacheService != null)
            {
                //如果缓存不存在，则从数据库获取数据，并保存到缓存中
                if (!_cacheService.Repository.Exists(cacheKey))
                {
                    data = _msgRepository.GetMessageConfigInfo();
                    _cacheService.Repository.Add(cacheKey, data, CacheKeyManager.MessageConfigExpires);
                }
                else data = _cacheService.Repository.Get<List<MessageConfigInfo>>(cacheKey);
            }
            if (data == null)
            {
                //如果不使用缓存管理数据，直接从数据库获取
                data = _msgRepository.GetMessageConfigInfo();
                _cacheService.Repository.Add(cacheKey, data, CacheKeyManager.MessageConfigExpires);
            }
            return data;

        }


        #endregion



        #endregion

    }
}
