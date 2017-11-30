using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class DynamicEntityServices : EntityBaseServices
    {
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IEntityProRepository _entityProRepository;
        private readonly IWorkFlowRepository _workFlowRepository;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IAccountRepository _accountRepository;

        private readonly WorkFlowServices _workflowService;


        private readonly IMapper _mapper;

        public DynamicEntityServices(IMapper mapper, IDynamicEntityRepository dynamicEntityRepository, IEntityProRepository entityProRepository, IWorkFlowRepository workFlowRepository, IDynamicRepository dynamicRepository, IAccountRepository accountRepository, WorkFlowServices workflowService)
        {
            _dynamicEntityRepository = dynamicEntityRepository;
            _entityProRepository = entityProRepository;
            _workFlowRepository = workFlowRepository;
            _mapper = mapper;
            _dynamicRepository = dynamicRepository;
            _accountRepository = accountRepository;

            _workflowService = workflowService;
        }

        public OutputResult<object> Add(DynamicEntityAddModel dynamicModel, AnalyseHeader header, int userNumber)
        {
            var dynamicEntity = _mapper.Map<DynamicEntityAddModel, DynamicEntityAddMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }

            //获取该实体分类的字段
            var fields = GetTypeFields(dynamicModel.TypeId, DynamicProtocolOperateType.Add, userNumber);

            if (fields.Count == 0)
            {
                return ShowError<object>("该实体分类没有配置相应字段");
            }

            var isMobile = header.Device.ToLower().Contains("android")
                           || header.Device.ToLower().Contains("ios");

            //验证字段
            var validResults = DynamicProtocolHelper.ValidData(fields, dynamicModel.FieldData, DynamicProtocolOperateType.Add, isMobile);

            //check if valid ok
            var data = new Dictionary<string, object>();
            var validTips = new List<string>();
            foreach (DynamicProtocolValidResult validResult in validResults.Values)
            {
                if (!validResult.IsValid)
                {
                    validTips.Add(validResult.Tips);
                }
                data.Add(validResult.FieldName, validResult.FieldData);
            }

            if (validTips.Count > 0)
            {
                return ShowError<object>(string.Join(";", validTips));
            }

            if (data.Count == 0)
            {
                return ShowError<object>("未找到相关的键值属性");
            }
            //根据实体设置的查重来校验查重

            //额外字段
            if (dynamicModel.ExtraData != null && dynamicModel.ExtraData.Count > 0)
            {
                foreach (KeyValuePair<string, object> pair in dynamicModel.ExtraData)
                {
                    data.Add(pair.Key, pair.Value);
                }
            }

            //额外数据处理
            var extraDic = ProcessExtraDataHandle(dynamicEntity.TypeId, data);
            foreach (KeyValuePair<string, object> pair in extraDic)
            {
                data.Add(pair.Key, pair.Value);
            }
            var entityInfo = _entityProRepository.GetEntityInfo(dynamicEntity.TypeId, userNumber);

            if (entityInfo != null)
            {
                bool success = false;
                SimpleEntityInfo entityInfotemp = null;
                Guid bussinessId;
                var res = ExcuteInsertAction((transaction, arg, userData) =>
                 {
                     #region --旧代码--
                     ////验证通过后，插入数据
                     //var result = !dynamicEntity.FlowId.HasValue ? _dynamicEntityRepository.DynamicAdd(transaction, dynamicEntity.TypeId, data, dynamicEntity.ExtraData, userNumber) : _dynamicEntityRepository.DynamicAdd(transaction, dynamicEntity.TypeId, data, dynamicEntity.FlowId.Value, dynamicEntity.RelEntityId, dynamicEntity.RelRecId, userNumber);
                     //if (result.Flag != 1)
                     //{
                     //    return HandleResult(result);
                     //}
                     ////处理回写
                     //if (dynamicEntity.WriteBackData != null && dynamicEntity.WriteBackData.Count > 0)
                     //{
                     //    _dynamicEntityRepository.WriteBack(transaction, dynamicEntity.WriteBackData, userNumber);
                     //}
                     //if (result.Flag == 1)
                     //{
                     //    if (!IsValidEntityPower(entityInfo) && !dynamicEntity.FlowId.HasValue && !userData.HasDataAccess(transaction, RoutePath, (Guid)entityInfo.entityid, DeviceClassic, new List<Guid>() { Guid.Parse(result.Id) }))
                     //    {
                     //        throw new Exception("您没有权限新增该实体数据");
                     //    }
                     //}

                     //success = result.Flag == 1;
                     //bussinessId = Guid.Parse(result.Id);
                     //var entityInfotemp = _entityProRepository.GetEntityInfo(dynamicEntity.TypeId);
                     //CheckCallBackService(transaction, OperatType.Insert, entityInfotemp.Servicesjson, bussinessId, (Guid)entityInfo.entityid, userNumber, "");

                     //return HandleResult(result); 
                     #endregion

                     //新增表单数据
                     var entityResult = _dynamicEntityRepository.DynamicAdd(transaction, dynamicEntity.TypeId, data, dynamicEntity.ExtraData, userNumber);
                     success = entityResult.Flag == 1;

                     if (success)
                     {
                         bussinessId = Guid.Parse(entityResult.Id);
                         //处理回写
                         if (dynamicEntity.WriteBackData != null && dynamicEntity.WriteBackData.Count > 0)
                         {
                             _dynamicEntityRepository.WriteBack(transaction, dynamicEntity.WriteBackData, userNumber);
                         }
                         if (!IsValidEntityPower(entityInfo) && !dynamicEntity.FlowId.HasValue && !userData.HasDataAccess(transaction, RoutePath, (Guid)entityInfo.entityid, DeviceClassic, new List<Guid>() { bussinessId }))
                         {
                             throw new Exception("您没有权限新增该实体数据");
                         }
                         entityInfotemp = _entityProRepository.GetEntityInfo(dynamicEntity.TypeId);

                         CheckCallBackService(transaction, OperatType.Insert, entityInfotemp.Servicesjson, bussinessId, (Guid)entityInfo.entityid, userNumber, "");
                         if (dynamicEntity.FlowId.HasValue)
                         {
                             //新增流程数据
                             var caseModel = new WorkFlowAddCaseMapper()
                             {
                                 EntityId = (Guid)entityInfo.entityid,
                                 FlowId = dynamicEntity.FlowId.Value,
                                 RecId = bussinessId,
                                 RelEntityId = dynamicEntity.RelEntityId,
                                 RelRecId = dynamicEntity.RelRecId,
                                 CaseData = data
                             };
                             var flowResult = _workflowService.AddCase(transaction, caseModel, userNumber);

                             return HandleResult(flowResult);
                         }
                         else
                         {
                             return HandleResult(entityResult);
                         }
                     }
                     else
                     {
                         return HandleResult(entityResult);
                     }


                 }, dynamicEntity, (Guid)entityInfo.entityid, userNumber);

                if (success)
                {
                    var relbussinessId = dynamicEntity.RelRecId.GetValueOrDefault();
                    if (!dynamicEntity.FlowId.HasValue)
                    {
                        //单据转换消息
                        if (dynamicEntity.ExtraData.ContainsKey("funccode") && dynamicEntity.ExtraData["funccode"] != null
                            && dynamicEntity.ExtraData.ContainsKey("entityId") && dynamicEntity.ExtraData["entityId"] != null
                            && dynamicEntity.ExtraData.ContainsKey("recordId") && dynamicEntity.ExtraData["recordId"] != null)
                        {
                            SendMessage(new Guid(dynamicEntity.ExtraData["recordId"].ToString()), userNumber, new Guid(dynamicEntity.ExtraData["entityId"].ToString()), dynamicEntity.ExtraData["funccode"].ToString());
                        }
                        else
                            WriteEntityAddMessage(dynamicEntity.TypeId, bussinessId, relbussinessId, userNumber);
                    }

                }
                return res;
            }
            else
            {
                throw new Exception("获取实体Id失败");
            }

        }

        private void SendMessage(Guid bussinessId, int userNumber, Guid custEntityId, string FuncCode)
        {
            Task.Run(() =>
            {
                try
                {
                    DynamicEntityDetailtMapper mainDetailMapper = new DynamicEntityDetailtMapper()
                    {
                        EntityId = custEntityId,
                        RecId = bussinessId,
                        NeedPower = 0
                    };
                    var mainCustDetail = _dynamicEntityRepository.Detail(mainDetailMapper, userNumber);

                    var relentityid = Guid.Empty;
                    var typeid = mainCustDetail.ContainsKey("rectype") ? Guid.Parse(mainCustDetail["rectype"].ToString()) : custEntityId;
                    var newMembers = MessageService.GetEntityMember(mainCustDetail as Dictionary<string, object>);
                    var msg = new MessageParameter();
                    msg.EntityId = custEntityId;
                    msg.RelBusinessId = Guid.Empty;
                    msg.RelEntityId = Guid.Empty;
                    msg.BusinessId = bussinessId;
                    msg.ParamData = null;
                    msg.FuncCode = FuncCode;

                    msg.Receivers = MessageService.GetEntityMessageReceivers(newMembers, null);

                    var paramData = new Dictionary<string, string>();
                    var userInfo = _accountRepository.GetUserInfoById(userNumber);
                    paramData.Add("operator", userInfo.UserName);

                    msg.TemplateKeyValue = paramData;

                    MessageService.WriteMessageAsyn(msg, userNumber);
                }
                catch (Exception ex)
                {

                }
            });
        }

        private void CheckCallBackService(DbTransaction transaction, OperatType operatType, ServicesJsonInfo servicesjson, Guid recid, Guid entityid, int userNum, string userName)
        {
            if (servicesjson == null) return;
            if (servicesjson.CallBackServices == null) return;
            if (servicesjson.CallBackServices.Count == 0) return;
            foreach (CallBackServiceModel item in servicesjson.CallBackServices)
            {
                if (item.OperatType == operatType)
                {
                    DoCallBack(transaction, operatType, item, recid, entityid, userNum, userName);
                }
            }
        }
        private void DoCallBack(DbTransaction transaction, OperatType operatType, CallBackServiceModel item, Guid recid, Guid entityid, int userNum, string userName)
        {
            string className = "";
            string methodName = "";
            if (item.MethodFullName == null || item.MethodFullName.Length == 0) return;
            int tmpIndex = item.MethodFullName.LastIndexOf('.');
            if (tmpIndex < 0) return;
            className = item.MethodFullName.Substring(0, tmpIndex);
            methodName = item.MethodFullName.Substring(tmpIndex + 1);
            if (className == null || className.Length == 0 || methodName == null || methodName.Length == 0) return;

            List<Dictionary<string, object>> param = new List<Dictionary<string, object>>();
            Dictionary<string, object> itemParam = new Dictionary<string, object>();
            itemParam.Add("recid", recid);
            itemParam.Add("entityid", entityid);
            itemParam.Add("opertype", operatType);
            if (item.ServiceType == CallBackServiceModel_ServiceType.ServiceType_InnerService)
            {
                object obj = dynamicCreateService(className, true);
                if (obj == null) return;
                Type t = obj.GetType();
                System.Reflection.MethodInfo methodInfo = t.GetMethod(methodName,
                    new Type[] { typeof(DbTransaction), typeof(Dictionary<string, object>), typeof(List<Dictionary<string, object>>), typeof(int), typeof(string) });
                if (methodInfo == null) return;
                methodInfo.Invoke(obj, new object[] { transaction, itemParam, param, userNum, userName });

            }
            else if (item.ServiceType == CallBackServiceModel_ServiceType.ServiceType_CommonMethod)
            {

                Assembly assembly = Assembly.GetEntryAssembly();
                object obj = assembly.CreateInstance(className);
                Type t = obj.GetType();
                System.Reflection.MethodInfo methodInfo = t.GetMethod(methodName,
                    new Type[] { typeof(DbTransaction), typeof(Dictionary<string, object>), typeof(List<Dictionary<string, object>>), typeof(int), typeof(string) });
                if (methodInfo == null) return;
                methodInfo.Invoke(obj, new object[] { transaction, itemParam, param, userNum, userName });
            }
        }
        /// <summary>
        /// 新增实体数据时写消息逻辑
        /// </summary>
        private void WriteEntityAddMessage(Guid typeId, Guid bussinessId, Guid relbussinessId, int userNumber)
        {
            Task.Run(() =>
            {
                var entityInfotemp = _entityProRepository.GetEntityInfo(typeId);
                string funccode = "EntityDataAdd";

                string msgpParam = null;

                var detailMapper = new DynamicEntityDetailtMapper()
                {
                    EntityId = entityInfotemp.EntityId,
                    RecId = bussinessId,
                    NeedPower = 0
                };
                IDictionary<string, object> detail;
                EntityMemberModel newMembers = null;
                if (entityInfotemp.ModelType == EntityModelType.Dynamic)
                {
                    funccode = "EntityDynamicAdd";
                    msgpParam = _dynamicRepository.GetDynamicTemplateData(bussinessId, entityInfotemp.EntityId, entityInfotemp.CategoryId, userNumber);
                    detailMapper.EntityId = entityInfotemp.RelEntityId.Value;
                    detailMapper.RecId = relbussinessId;

                    detail = _dynamicEntityRepository.Detail(detailMapper, userNumber);
                    newMembers = MessageService.GetEntityMember(detail as Dictionary<string, object>);
                }
                else
                {
                    detail = _dynamicEntityRepository.Detail(detailMapper, userNumber);
                    newMembers = MessageService.GetEntityMember(detail as Dictionary<string, object>);

                }


                //日报和周计划和周总结
                if (entityInfotemp.EntityId == new Guid("601cb738-a829-4a7b-a3d9-f8914a5d90f2") ||
                    entityInfotemp.EntityId == new Guid("0b81d536-3817-4cbc-b882-bc3e935db845") ||
                    entityInfotemp.EntityId == new Guid("fcc648ae-8817-48b7-b1d7-49ed4c24316b"))
                {
                    //获取模板数据
                    msgpParam = _dynamicRepository.GetDynamicTemplateData(bussinessId, entityInfotemp.EntityId, entityInfotemp.CategoryId, userNumber);
                    //周总结
                    if (entityInfotemp.EntityId == new Guid("fcc648ae-8817-48b7-b1d7-49ed4c24316b"))
                    {
                        funccode = "EntityDynamicAdd";
                    }
                    DateTime reportdate = DateTime.Parse(detail["reportdate"].ToString());
                    var msg = MessageService.GetDailyMsgParameter(reportdate, entityInfotemp, bussinessId, relbussinessId, funccode, userNumber, newMembers, null, msgpParam);
                    msg.ApprovalUsers = msg.Receivers[MessageUserType.DailyApprover];
                    msg.CopyUsers = msg.Receivers[MessageUserType.DailyCarbonCopyUser];
                    MessageService.WriteMessage(null, msg, userNumber);
                }

                else
                {
                    //编辑操作的消息
                    var addEntityMsg = MessageService.GetEntityMsgParameter(entityInfotemp, bussinessId, relbussinessId, funccode, userNumber, newMembers, null, msgpParam);
                    MessageService.WriteMessageAsyn(addEntityMsg, userNumber);
                }

                if (entityInfotemp.ModelType == EntityModelType.Independent)
                {

                    //生成添加相关人的离线消息参数数据
                    if (newMembers.ViewUsers.Count > 0)
                    {
                        var addViewusersMsg = MessageService.GetEntityMsgParameter(entityInfotemp, bussinessId, relbussinessId, "ViewUserAdd", userNumber, newMembers, null);
                        MessageService.WriteMessage(null, addViewusersMsg, userNumber);
                    }
                }
                //处理数据源控件需要触发消息的逻辑
                CheckServicesJson(OperatType.Insert, entityInfotemp.Servicesjson, detail as Dictionary<string, object>, userNumber);
            });
        }


        /// <summary>
        /// 用于执行实体的扩展函数，
        /// 实体扩展函数主要是给项目使用，产品请勿使用此功能。
        /// </summary>
        /// <param name="functionname"></param>
        /// <param name="paramInfo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> ExecuteExtFunction(string functionname, UKExtExecuteFunctionModel paramInfo, int userId)
        {
            DbTransaction tran = null;
            if (paramInfo.EntityId == null || paramInfo.EntityId == Guid.Empty) throw (new Exception("参数异常"));
            #region 检查职能权限
            if (GetUserData(userId, false).HasFunction(this.RoutePath, paramInfo.EntityId, this.DeviceClassic) == false) {
                throw (new Exception("权限项未配置或者您没有权限执行此方法"));
            }
            #endregion
            #region  暂不检查数据权限
            #endregion 
            Dictionary<string, object> entityInfo = this._dynamicEntityRepository.getEntityBaseInfoById(paramInfo.EntityId, userId);
            if (entityInfo == null || entityInfo.Count == 0) throw (new Exception("实体信息异常"));
            //
            EntityExtFunctionInfo extFuncInfo = this._dynamicEntityRepository.getExtFunctionByFunctionName(paramInfo.EntityId, functionname);
            if (extFuncInfo == null) throw (new Exception("实体函数异常"));
            if (extFuncInfo.RecStatus != 1) {
                throw (new Exception("该函数已经被停用了"));
            }
            try
            {

                object retObj = this._dynamicEntityRepository.ExecuteExtFunction(extFuncInfo, paramInfo.RecIds, paramInfo.OtherParams, userId);
                return new OutputResult<object>(retObj);
            }
            catch (Exception ex) {
                if (ex.InnerException != null) {
                    return new OutputResult<object>("", ex.InnerException.Message, -1);
                }
                else
                {
                    return new OutputResult<object>("", ex.Message, -1);
                }
            }
        }

        /// <summary>
        /// 写引用实体的消息通知
        /// </summary>
        /// <param name="sourecdetail"></param>
        /// <param name="noticeModel"></param>
        /// <param name="userNumber"></param>
        private void WriteReferEntityMessage(Dictionary<string, object> sourecdetail, NoticeServiceModel noticeModel, int userNumber)
        {

            var detailMapper = new DynamicEntityDetailtListMapper()
            {
                EntityId = noticeModel.EntityId,
                NeedPower = 0
            };
            var entityInfo = _entityProRepository.GetEntityInfo(noticeModel.EntityId, userNumber);
            var modeltype = (EntityModelType)entityInfo.modeltype;
            //如果是独立实体
            if (modeltype == EntityModelType.Independent)
            {
                var fieldData = sourecdetail[noticeModel.FieldName];
                if (sourecdetail.ContainsKey(noticeModel.FieldName) && fieldData != null && !string.IsNullOrEmpty(fieldData.ToString()))
                {
                    var fieldValue = JObject.Parse(fieldData.ToString());
                    if (fieldValue["id"] != null)
                    {
                        detailMapper.RecIds = fieldValue["id"].ToString();
                        var detailList = _dynamicEntityRepository.DetailList(detailMapper, userNumber);
                        var entityInfotemp = _entityProRepository.GetEntityInfo(noticeModel.EntityId);
                        foreach (var detail in detailList)
                        {
                            var relbussinessId = Guid.Empty;
                            var bussinessId = new Guid(detail["recid"].ToString());
                            entityInfotemp.CategoryId = new Guid(detail["rectype"].ToString()); ;
                            var members = MessageService.GetEntityMember(detail as Dictionary<string, object>);
                            //编辑操作的消息
                            var addEntityMsg = MessageService.GetEntityMsgParameter(entityInfotemp, bussinessId, relbussinessId, noticeModel.MsgConfigFuncCode, userNumber, members, null, null);
                            MessageService.WriteMessageAsyn(addEntityMsg, userNumber);
                        }
                    }
                }
            }
        }

        #region --处理ServicesJson的逻辑--
        private void CheckServicesJson(OperatType operatType, ServicesJsonInfo servicesjson, Dictionary<string, object> detail, int userNumber)
        {
            //ServicesJsonInfo servicesJsonInfo = null;
            if (servicesjson != null)
            {
                //servicesJsonInfo = JsonConvert.DeserializeObject<ServicesJsonInfo>(servicesjson);
                if (servicesjson != null && servicesjson.NoticeService != null && servicesjson.NoticeService.Count > 0)
                {
                    foreach (var notice in servicesjson.NoticeService)
                    {
                        if (notice != null && notice.IsValidData() && notice.OperatType == operatType)
                        {
                            WriteSourceEntityMessage(detail as Dictionary<string, object>, notice, userNumber);
                            //Type serviceType = Type.GetType(notice.ServiceName);
                            //object service = _serviceProvider.GetService(serviceType);
                            //MethodInfo methodInfo = serviceType.GetMethod(notice.MethorName, new Type[] { typeof(Dictionary<string, object>), typeof(Dictionary<string, string>), typeof(int), typeof(int), typeof(int) });
                            //var result = methodInfo.Invoke(service, new object[] { });
                        }
                    }
                }

            }
        }

        /// <summary>
        /// 写属于关联独立实体的消息通知
        /// </summary>
        /// <param name="sourecdetail"></param>
        /// <param name="noticeModel"></param>
        /// <param name="userNumber"></param>
        private void WriteSourceEntityMessage(Dictionary<string, object> sourecdetail, NoticeServiceModel noticeModel, int userNumber)
        {

            var detailMapper = new DynamicEntityDetailtListMapper()
            {
                EntityId = noticeModel.EntityId,
                NeedPower = 0
            };
            var entityInfo = _entityProRepository.GetEntityInfo(noticeModel.EntityId, userNumber);
            var modeltype = (EntityModelType)entityInfo.modeltype;
            //如果是独立实体
            if (modeltype == EntityModelType.Independent)
            {
                var fieldData = sourecdetail[noticeModel.FieldName];
                if (sourecdetail.ContainsKey(noticeModel.FieldName) && fieldData != null && !string.IsNullOrEmpty(fieldData.ToString()))
                {
                    var fieldValue = JObject.Parse(fieldData.ToString());
                    if (fieldValue["id"] != null)
                    {
                        detailMapper.RecIds = fieldValue["id"].ToString();
                        var detailList = _dynamicEntityRepository.DetailList(detailMapper, userNumber);
                        var entityInfotemp = _entityProRepository.GetEntityInfo(noticeModel.EntityId);
                        foreach (var detail in detailList)
                        {
                            var relbussinessId = Guid.Empty;
                            var bussinessId = new Guid(detail["recid"].ToString());
                            entityInfotemp.CategoryId = new Guid(detail["rectype"].ToString()); ;
                            var members = MessageService.GetEntityMember(detail as Dictionary<string, object>);
                            //编辑操作的消息
                            var addEntityMsg = MessageService.GetEntityMsgParameter(entityInfotemp, bussinessId, relbussinessId, noticeModel.MsgConfigFuncCode, userNumber, members, null, null);
                            MessageService.WriteMessageAsyn(addEntityMsg, userNumber);
                        }
                    }
                }
            }
        }
        #endregion

        #region --暂时不使用该代码--
        public FieldValueModel FormatFieldValue(EntityFieldControlType controlType, object sourceFieldValue)
        {
            FieldValueModel fieldValue = new FieldValueModel();
            fieldValue.FieldValue = sourceFieldValue;
            if (sourceFieldValue == null)
            {
                return fieldValue;
            }
            var fieldValueTemp = sourceFieldValue.ToString();
            if (!string.IsNullOrEmpty(fieldValueTemp))
            {

                switch (controlType)
                {
                    case EntityFieldControlType.RecId://记录ID
                    case EntityFieldControlType.RecItemid://明细ID
                    case EntityFieldControlType.AreaGroup://分组
                    case EntityFieldControlType.TreeSingle://树形
                    case EntityFieldControlType.TreeMulti://树形多选
                    case EntityFieldControlType.QuoteControl://引用控件
                    case EntityFieldControlType.FileAttach://附件
                    case EntityFieldControlType.LinkeTable://表格控件
                        break;
                    case EntityFieldControlType.Text:
                    case EntityFieldControlType.TipText:
                    case EntityFieldControlType.TextArea:
                    case EntityFieldControlType.PhoneNum:
                    case EntityFieldControlType.EmailAddr:
                    case EntityFieldControlType.Telephone:
                        break;
                    case EntityFieldControlType.NumberInt:
                    case EntityFieldControlType.NumberDecimal:
                        break;

                    case EntityFieldControlType.TimeDate:
                        {
                            var dt = DateTime.Parse(fieldValueTemp);
                            fieldValue.FieldValue = string.Format("{0:d}", dt);
                            break;
                        }
                    case EntityFieldControlType.TimeStamp:
                    case EntityFieldControlType.RecCreated:
                    case EntityFieldControlType.RecUpdated:
                    case EntityFieldControlType.RecOnlive://活动时间
                        {
                            var dt = DateTime.Parse(fieldValueTemp);
                            fieldValue.FieldValue = string.Format("{0:G}", dt);
                            break;
                        }


                    case EntityFieldControlType.Address:
                    case EntityFieldControlType.Location:
                        {
                            var addressData = JObject.Parse(fieldValueTemp);
                            fieldValue.FieldValueName = addressData["address"].ToString();
                            break;
                        }
                    case EntityFieldControlType.HeadPhoto:
                    case EntityFieldControlType.TakePhoto:
                        break;

                    case EntityFieldControlType.SelectSingle://单选
                    case EntityFieldControlType.SelectMulti://多选
                        break;
                    case EntityFieldControlType.PersonSelectSingle:
                    case EntityFieldControlType.PersonSelectMulti:
                        {
                            var temp = fieldValueTemp.Split(',').Distinct().Select(m => int.Parse(m)).ToList();
                            var userInfos = _accountRepository.GetUserInfoList(temp);
                            fieldValue.FieldValueName = string.Join(",", userInfos.Select(m => m.UserName));
                            break;
                        }
                    case EntityFieldControlType.AreaRegion://行政区域
                        {
                            break;
                        }
                    case EntityFieldControlType.Department://部门
                        {
                            break;
                        }
                    case EntityFieldControlType.DataSourceSingle:
                    case EntityFieldControlType.DataSourceMulti:
                        {
                            break;
                        }
                    case EntityFieldControlType.RecCreator:
                    case EntityFieldControlType.RecUpdator:
                    case EntityFieldControlType.RecManager:
                        {
                            int temp = int.Parse(fieldValueTemp);
                            var userInfo = _accountRepository.GetUserInfoById(temp);
                            fieldValue.FieldValueName = userInfo.UserName;
                        }
                        break;
                    case EntityFieldControlType.RecAudits://审批状态
                        {
                            int temp = int.Parse(fieldValueTemp);
                            switch (temp)
                            {
                                case 0:
                                    fieldValue.FieldValueName = "审批中";
                                    break;
                                case 1:
                                    fieldValue.FieldValueName = "通过";
                                    break;
                                case 2:
                                    fieldValue.FieldValueName = "不通过";
                                    break;
                                case 3:
                                    fieldValue.FieldValueName = "发起审批";
                                    break;
                            }
                        }
                        break;
                    case EntityFieldControlType.RecStatus://记录状态
                        {
                            int temp = int.Parse(fieldValueTemp);
                            switch (temp)
                            {
                                case 0:
                                    fieldValue.FieldValueName = "停用";
                                    break;
                                case 1:
                                    fieldValue.FieldValueName = "启用";
                                    break;
                                case 2:
                                    fieldValue.FieldValueName = "删除";
                                    break;
                            }
                        }
                        break;


                    case EntityFieldControlType.RecType://记录类型
                        {
                            break;
                        }
                    case EntityFieldControlType.SalesStage://销售阶段
                        {
                            break;
                        }
                    case EntityFieldControlType.Product://产品
                        {
                            break;
                        }
                    case EntityFieldControlType.ProductSet://产品系列
                        {
                            break;
                        }

                }

            }
            return fieldValue;
        }

        private Dictionary<string, object> GetDynamicTempData(Guid entityId, Guid typeid, IDictionary<string, object> detail)
        {
            var dynamicAbs = _dynamicRepository.GetDynamicAbstract(entityId, typeid);
            var msgpParamDic = new Dictionary<string, object>();
            foreach (var fieldItem in dynamicAbs)
            {
                if (detail.ContainsKey(fieldItem.FieldName))
                {
                    var fieldValue = detail[fieldItem.FieldName];

                    msgpParamDic.Add(fieldItem.FieldName, FormatFieldValue(fieldItem.ControlType, fieldValue));
                }
            }
            return msgpParamDic;
        }
        #endregion

        public OutputResult<object> GetRelTabEntity(RelTabListModel dynamicModel, int userNumber)
        {
            var dynamicEntity = _mapper.Map<RelTabListModel, RelTabListMapper>(dynamicModel);

            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }
            var result = _dynamicEntityRepository.GetRelTabEntity(dynamicEntity, userNumber);
            return new OutputResult<object>(result);

        }

        public OutputResult<object> GetRelEntityFields(GetEntityFieldsModel entity, int userNumber)
        {
            var dynamicEntity = new GetEntityFieldsMapper()
            {
                EntityId = entity.EntityId,
                RelEntityId = entity.RelEntityId
            };
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }
            var result = _dynamicEntityRepository.GetRelEntityFields(dynamicEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> AddList(DynamicEntityAddListModel dynamicModel, AnalyseHeader header, int userNumber)
        {

            var isMobile = header.Device.ToLower().Contains("android")
                           || header.Device.ToLower().Contains("ios");
            var dataList = new List<DynamicEntityAddListMapper>();
            foreach (var temdata in dynamicModel.EntityFields)
            {
                var dynamicEntity = new DynamicEntityAddListMapper()
                {
                    TypeId = temdata.TypeId,
                    FieldData = temdata.FieldData,
                    ExtraData = temdata.ExtraData

                };


                //获取该实体分类的字段
                var fields = GetTypeFields(temdata.TypeId, DynamicProtocolOperateType.Add, userNumber);

                if (fields.Count == 0)
                {
                    return ShowError<object>("该实体分类没有配置相应字段");
                }


                //验证字段
                var validResults = DynamicProtocolHelper.ValidData(fields, temdata.FieldData, DynamicProtocolOperateType.Add, isMobile);

                //check if valid ok
                var data = new Dictionary<string, object>();
                var validTips = new List<string>();
                foreach (DynamicProtocolValidResult validResult in validResults.Values)
                {
                    if (!validResult.IsValid)
                    {
                        validTips.Add(validResult.Tips);
                    }
                    data.Add(validResult.FieldName, validResult.FieldData);
                }

                if (validTips.Count > 0)
                {
                    return ShowError<object>(string.Join(";", validTips));
                }

                if (data.Count == 0)
                {
                    return ShowError<object>("未找到相关的键值属性");
                }
                //根据实体设置的查重来校验查重

                //额外字段
                if (temdata.ExtraData != null && temdata.ExtraData.Count > 0)
                {
                    foreach (KeyValuePair<string, object> pair in temdata.ExtraData)
                    {
                        data.Add(pair.Key, pair.Value);
                    }
                }

                //额外数据处理
                var extraDic = ProcessExtraDataHandle(dynamicEntity.TypeId, data);
                foreach (KeyValuePair<string, object> pair in extraDic)
                {
                    data.Add(pair.Key, pair.Value);
                }
                dynamicEntity.FieldData = data;
                dataList.Add(dynamicEntity);
            }
            //验证通过后，插入数据
            try
            {
                _dynamicEntityRepository.DynamicAddList(dataList, userNumber);
            }
            catch (Exception ex)
            {
                return ShowError<object>(ex.Message);
            }

            return new OutputResult<object>();
        }

        public Dictionary<string, object> ProcessExtraDataHandle(Guid entityid, Dictionary<string, object> data)
        {
            var extraDic = new Dictionary<string, object>();
            switch (entityid.ToString())
            {
                case "e450bfd7-ff17-4b29-a2db-7ddaf1e79342":
                    {
                        //联系人，需要增加拼音字段
                        if (data.ContainsKey("recname"))
                        {
                            var name = data["recname"] as string;
                            var pinYin = PinYinConvert.ToChinese(name, true);
                            if (!string.IsNullOrWhiteSpace(pinYin))
                            {
                                extraDic.Add("namepinyin", pinYin);
                            }
                        }
                        break;
                    }
            }

            return extraDic;
        }

        public OutputResult<object> GeneralProtocol(DynamicEntityGeneralModel dynamicModel, int userNumber)
        {
            //获取该实体分类的字段
            var fields = GetTypeFields(dynamicModel.TypeId, (DynamicProtocolOperateType)dynamicModel.OperateType, userNumber);

            if (fields.Count == 0)
            {
                return ShowError<object>("该实体分类没有配置相应字段");
            }

            return new OutputResult<object>(fields);
        }

        /***
         * 用于WEB查询grid的
         * */
        public OutputResult<object> GeneralGridProtocol(DynamicGridEntityGeneralModel dynamicModel, int userNumber)
        {
            Guid relTypeId = _dynamicEntityRepository.getGridTypeByMainType(dynamicModel.TypeId, dynamicModel.EntityId);
            if (relTypeId == null || relTypeId == Guid.Empty) return new OutputResult<object>(null, "参数错误", 1);
            //获取该实体分类的字段

            var fields = GetTypeFields(relTypeId, (DynamicProtocolOperateType)dynamicModel.OperateType, userNumber);

            if (fields.Count == 0)
            {
                return ShowError<object>("该实体分类没有配置相应字段");
            }

            return new OutputResult<object>(fields);
        }

        public OutputResult<object> ListViewColumns(DynamicEntityGeneralModel dynamicModel, int userNumber)
        {
            //获取该实体分类的字段
            var fields = GetWebFields(dynamicModel.TypeId, (DynamicProtocolOperateType)dynamicModel.OperateType, userNumber);

            if (fields.Count == 0)
            {
                return ShowError<object>("该实体没有配置列表字段");
            }

            return new OutputResult<object>(fields);
        }

        public OutputResult<object> GeneralDictionary(DynamicEntityGeneralDicModel dynamicModel, int userNumber)
        {
            //获取该实体分类的字段
            var result = _dynamicEntityRepository.GetDicItemByKeys(dynamicModel.DicKeys);

            var dataDic = new Dictionary<int, List<GeneralDicItem>>();
            foreach (GeneralDicItem item in result)
            {
                if (!dataDic.ContainsKey(item.DicTypeId))
                {
                    dataDic[item.DicTypeId] = new List<GeneralDicItem>();
                }

                dataDic[item.DicTypeId].Add(item);
            }

            return new OutputResult<object>(new { DicData = dataDic });
        }

        public OutputResult<object> Edit(DynamicEntityEditModel dynamicModel, AnalyseHeader header, int userNumber)
        {
            var dynamicEntity = _mapper.Map<DynamicEntityEditModel, DynamicEntityEditMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }

            //获取该实体分类的字段
            var fields = GetTypeFields(dynamicModel.TypeId, DynamicProtocolOperateType.Edit, userNumber);

            if (fields.Count == 0)
            {
                return ShowError<object>("该实体分类没有配置相应字段");
            }

            var isMobile = header.Device.ToLower().Contains("android")
                  || header.Device.ToLower().Contains("ios");

            //验证字段
            var validResults = DynamicProtocolHelper.ValidData(fields, dynamicModel.FieldData, DynamicProtocolOperateType.Edit, isMobile);

            //check if valid ok
            var data = new Dictionary<string, object>();
            var validTips = new List<string>();
            foreach (DynamicProtocolValidResult validResult in validResults.Values)
            {
                if (!validResult.IsValid)
                {
                    validTips.Add(validResult.Tips);
                }
                data.Add(validResult.FieldName, validResult.FieldData);
            }

            if (validTips.Count > 0)
            {
                return ShowError<object>(string.Join(";", validTips));
            }

            //额外数据处理
            var extraDic = ProcessExtraDataHandle(dynamicEntity.TypeId, data);
            foreach (KeyValuePair<string, object> pair in extraDic)
            {
                data.Add(pair.Key, pair.Value);
            }
            var entityInfo = _entityProRepository.GetEntityInfo(dynamicEntity.TypeId, userNumber);
            if (entityInfo != null)
            {
                return ExcuteUpdateAction((transaction, arg, userData) =>
                {
                    var entityid = (Guid)entityInfo.entityid;
                    var relentityid = (Guid?)entityInfo.relentityid;
                    var bussinessId = dynamicEntity.RecId;
                    var typeid = dynamicEntity.TypeId;


                    DynamicEntityDetailtMapper detailMapper = new DynamicEntityDetailtMapper()
                    {
                        EntityId = entityid,
                        RecId = bussinessId,
                        NeedPower = 0
                    };
                    var oldDetail = _dynamicEntityRepository.Detail(detailMapper, userNumber) as Dictionary<string, object>;

                    var relbussinessId = oldDetail.ContainsKey("recrelateid") ? new Guid(oldDetail["recrelateid"].ToString()) : Guid.Empty;

                    //验证通过后，插入数据
                    var result = _dynamicEntityRepository.DynamicEdit(transaction, dynamicEntity.TypeId, dynamicEntity.RecId, data, userNumber);
                    if (result.Flag == 1)
                    {
                        var entityInfotemp = _entityProRepository.GetEntityInfo(dynamicEntity.TypeId);
                        CheckCallBackService(transaction, OperatType.Update, entityInfotemp.Servicesjson, bussinessId, (Guid)entityInfo.entityid, userNumber, "");
                    }
                    Task.Run(() =>
                    {
                        if (result.Flag == 1)
                        {

                            List<MessageParameter> msgparams = new List<MessageParameter>();

                            //日报和周计划时，需要获取抄送人和批阅人
                            if (entityid == new Guid("601cb738-a829-4a7b-a3d9-f8914a5d90f2") ||
                                entityid == new Guid("0b81d536-3817-4cbc-b882-bc3e935db845") ||
                                entityid == new Guid("fcc648ae-8817-48b7-b1d7-49ed4c24316b"))
                            {
                                var entityInfotemp = _entityProRepository.GetEntityInfo(typeid);
                                //获取模板数据
                                var msgpParam = _dynamicRepository.GetDynamicTemplateData(bussinessId, entityInfotemp.EntityId, entityInfotemp.CategoryId, userNumber);
                                Dictionary<string, object> detail;

                                EntityMemberModel newMembers = null;
                                EntityMemberModel oldMembers = null;
                                DateTime reportdate;
                                //周总结
                                if (entityid == new Guid("fcc648ae-8817-48b7-b1d7-49ed4c24316b"))
                                {
                                    detailMapper.EntityId = entityInfotemp.RelEntityId.GetValueOrDefault();
                                    detailMapper.RecId = relbussinessId;
                                    detail = _dynamicEntityRepository.Detail(detailMapper, userNumber) as Dictionary<string, object>;
                                    newMembers = MessageService.GetEntityMember(detail);
                                    reportdate = DateTime.Parse(detail["reportdate"].ToString());
                                }
                                else
                                {
                                    detail = _dynamicEntityRepository.Detail(detailMapper, userNumber) as Dictionary<string, object>;
                                    reportdate = DateTime.Parse(detail["reportdate"].ToString());
                                    oldMembers = MessageService.GetEntityMember(oldDetail);
                                    newMembers = MessageService.GetEntityMember(detail);

                                    var receivers = MessageService.GetDailyMessageReceivers(newMembers, oldMembers);
                                    //新增批阅人和抄送人
                                    if ((receivers.ContainsKey(MessageUserType.DailyNewApprover) && receivers[MessageUserType.DailyNewApprover].Count > 0) ||
                                        (receivers.ContainsKey(MessageUserType.DailyNewCopyUser) && receivers[MessageUserType.DailyNewCopyUser].Count > 0))
                                    {
                                        var msg = MessageService.GetDailyMsgParameter(reportdate, entityInfotemp, bussinessId, relbussinessId, "DaliyRecevierAdd", userNumber, newMembers, oldMembers, msgpParam);
                                        msg.ApprovalUsers = msg.Receivers[MessageUserType.DailyNewApprover];
                                        msg.CopyUsers = msg.Receivers[MessageUserType.DailyNewCopyUser];
                                        msgparams.Add(msg);
                                    }
                                    //移除批阅人和抄送人
                                    if ((receivers.ContainsKey(MessageUserType.DailyCopyUserDeleted) && receivers[MessageUserType.DailyCopyUserDeleted].Count > 0) ||
                                        (receivers.ContainsKey(MessageUserType.DailyApproverDeleted) && receivers[MessageUserType.DailyApproverDeleted].Count > 0))
                                    {
                                        var msg = MessageService.GetDailyMsgParameter(reportdate, entityInfotemp, bussinessId, relbussinessId, "EntityDataDelete", userNumber, newMembers, oldMembers, msgpParam);
                                        //msg.ApprovalUsers = msg.Receivers[MessageUserType.DailyApproverDeleted];
                                        //msg.CopyUsers = msg.Receivers[MessageUserType.DailyCopyUserDeleted];
                                        msgparams.Add(msg);
                                    }
                                }

                                var editmsg = MessageService.GetDailyMsgParameter(reportdate, entityInfotemp, bussinessId, relbussinessId, "EntityDataEdit", userNumber, newMembers, oldMembers, msgpParam);
                                editmsg.ApprovalUsers = editmsg.Receivers[MessageUserType.DailyApprover];
                                editmsg.CopyUsers = editmsg.Receivers[MessageUserType.DailyCarbonCopyUser];
                                msgparams.Add(editmsg);
                            }
                            else
                            {
                                var detail = _dynamicEntityRepository.Detail(detailMapper, userNumber) as Dictionary<string, object>;
                                msgparams = GetEditMessageParameter(oldDetail, detail, typeid, bussinessId, relbussinessId, userNumber);
                            }
                            foreach (var msg in msgparams)
                            {
                                MessageService.WriteMessage(null, msg, userNumber, null);
                            }
                            if (entityInfo.servicesjson != null)
                            {
                                //处理数据源控件需要触发消息的逻辑
                                var servicesJsonInfo = JsonConvert.DeserializeObject<ServicesJsonInfo>(entityInfo.servicesjson);
                                CheckServicesJson(OperatType.Update, servicesJsonInfo, data, userNumber);
                            }
                        }
                    });
                    return HandleResult(result);

                }, dynamicModel, (Guid)entityInfo.entityid, userNumber, new List<Guid>() { dynamicEntity.RecId });
            }
            else
            {
                throw new Exception("获取实体Id失败");
            }
        }



        /// <summary>
        /// 获取编辑时，写离线消息的参数数据
        /// </summary>
        private List<MessageParameter> GetEditMessageParameter(Dictionary<string, object> oldDetail, Dictionary<string, object> newDetail, Guid typeid, Guid bussinessId, Guid relbussinessId, int userNumber)
        {



            var oldMembers = MessageService.GetEntityMember(oldDetail);
            var newMembers = MessageService.GetEntityMember(newDetail);


            //新增的相关人
            List<int> addViewusers = newMembers.ViewUsers.Except(oldMembers.ViewUsers).ToList();
            //删除的相关人
            List<int> deleteViewusers = oldMembers.ViewUsers.Except(newMembers.ViewUsers).ToList();


            var entityInfotemp = _entityProRepository.GetEntityInfo(typeid);
            var msgParams = new List<MessageParameter>();
            //生成添加相关人的离线消息参数数据
            if (addViewusers.Count > 0)
            {
                var addViewusersMsg = MessageService.GetEntityMsgParameter(entityInfotemp, bussinessId, relbussinessId, "ViewUserAdd", userNumber, newMembers, oldMembers);
                msgParams.Add(addViewusersMsg);
            }

            //生成移除相关人的离线消息参数数据
            if (deleteViewusers.Count > 0)
            {
                var removeViewusersMsg = MessageService.GetEntityMsgParameter(entityInfotemp, bussinessId, relbussinessId, "ViewUserDelete", userNumber, newMembers, oldMembers);
                msgParams.Add(removeViewusersMsg);
            }

            var editEntityMsg = MessageService.GetEntityMsgParameter(entityInfotemp, bussinessId, relbussinessId, "EntityDataEdit", userNumber, newMembers, oldMembers);
            msgParams.Add(editEntityMsg);
            return msgParams;

        }





        public OutputResult<object> DataList(DynamicEntityListModel dynamicModel, bool isAdvanceQuery, int userNumber)
        {
            var dynamicEntity = _mapper.Map<DynamicEntityListModel, DynamicEntityListMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }

            var pageParam = new PageParam { PageIndex = dynamicModel.PageIndex, PageSize = dynamicModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }



            if (dynamicModel.SearchData != null && dynamicModel.SearchData.Count > 0)
            {
                //生成查询语句
                var searchFields = isAdvanceQuery ? GetSearchFields(dynamicEntity.EntityId, userNumber) : GetEntityFields(dynamicEntity.EntityId, userNumber);

                if (searchFields == null)
                {
                    return ShowError<object>("未查询到相应的查询参数");
                }


                var validResults = isAdvanceQuery ? DynamicProtocolHelper.AdvanceQuery(searchFields, dynamicModel.SearchData) : DynamicProtocolHelper.SimpleQuery(searchFields, dynamicModel.SearchData);
                var validTips = new List<string>();
                var data = new Dictionary<string, string>();



                foreach (DynamicProtocolValidResult validResult in validResults.Values)
                {
                    if (!validResult.IsValid)
                    {
                        validTips.Add(validResult.Tips);
                    }
                    data.Add(validResult.FieldName, validResult.FieldData.ToString());

                }

                if (validTips.Count > 0)
                {
                    return ShowError<object>(string.Join(";", validTips));
                }

                if (data.Count > 0)
                {
                    dynamicEntity.SearchQuery = " AND " + string.Join(" AND ", data.Values.ToArray());
                }
            }

            if (dynamicModel.SearchDataXOR != null && dynamicModel.SearchDataXOR.Count > 0)
            {
                //生成查询语句
                var searchFields = GetEntityFields(dynamicEntity.EntityId, userNumber);

                if (searchFields == null)
                {
                    return ShowError<object>("未查询到相应的查询参数");
                }


                var validResults = DynamicProtocolHelper.SimpleQuery(searchFields, dynamicModel.SearchDataXOR);
                var validTips = new List<string>();
                var data = new Dictionary<string, string>();



                foreach (DynamicProtocolValidResult validResult in validResults.Values)
                {
                    if (!validResult.IsValid)
                    {
                        validTips.Add(validResult.Tips);
                    }
                    data.Add(validResult.FieldName, validResult.FieldData.ToString());

                }

                if (validTips.Count > 0)
                {
                    return ShowError<object>(string.Join(";", validTips));
                }

                if (data.Count > 0)
                {
                    dynamicEntity.SearchQuery = dynamicEntity.SearchQuery + " AND (" + string.Join(" OR ", data.Values.ToArray()) + " ) ";
                }
            }



            //处理排序语句
            if (string.IsNullOrWhiteSpace(dynamicEntity.SearchOrder))
            {
                dynamicEntity.SearchOrder = "";
            }

            var result = _dynamicEntityRepository.DataList(pageParam, dynamicEntity.ExtraData, dynamicEntity, userNumber);


            return new OutputResult<object>(result);
        }
        public OutputResult<object> CommonDataList(DynamicEntityListMapper dynamicEntity, PageParam pageParam, bool isAdvanceQuery, int userNumber)
        {
            DbTransaction tran = null;


            #region 检查并处理有特殊列表函数的情况
            string SpecFuncName = _dynamicEntityRepository.CheckDataListSpecFunction(dynamicEntity.EntityId);
            if (SpecFuncName != null)
            {
                var innerResult = _dynamicEntityRepository.DataListUseFunc(SpecFuncName, pageParam, dynamicEntity.ExtraData, dynamicEntity, userNumber);
                return new OutputResult<object>(innerResult);
            }
            #endregion

            Dictionary<string, object> EntityInfo = this._dynamicEntityRepository.getEntityBaseInfoById(dynamicEntity.EntityId, userNumber);
            if (EntityInfo == null) throw (new Exception("异常"));
            List<DynamicEntityFieldSearch> fieldList = this._dynamicEntityRepository.GetEntityFields(dynamicEntity.EntityId, userNumber);
            Dictionary<string, string> fieldNameMapDataSource = new Dictionary<string, string>();
            Dictionary<string, string> dataSources = new Dictionary<string, string>();
            Dictionary<string, string> fieldNameDictType = new Dictionary<string, string>();
            //获取所有的datasource信息
            foreach (DynamicEntityFieldSearch fieldInfo in fieldList)
            {
                if (fieldInfo.ControlType == 18)
                {//数据源
                    if (fieldInfo.FieldConfig != null || fieldInfo.FieldConfig.Length > 0)
                    {
                        Dictionary<string, object> fieldConfigDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(fieldInfo.FieldConfig);
                        if (fieldConfigDict.ContainsKey("dataSource"))
                        {
                            Dictionary<string, object> datasourceInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                Newtonsoft.Json.JsonConvert.SerializeObject(fieldConfigDict["dataSource"])
                                );
                            if (datasourceInfo.ContainsKey("sourceId") && datasourceInfo["sourceId"] != null)
                            {
                                string tmp = datasourceInfo["sourceId"].ToString();
                                fieldNameMapDataSource.Add(fieldInfo.FieldName, tmp);
                                if (dataSources.ContainsKey(tmp) == false) dataSources.Add(tmp, tmp);

                            }
                        }
                    }
                }
                else if (fieldInfo.ControlType == 3 || fieldInfo.ControlType == 4)
                {
                    if (fieldInfo.FieldConfig != null || fieldInfo.FieldConfig.Length > 0)
                    {
                        Dictionary<string, object> fieldConfigDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(fieldInfo.FieldConfig);
                        if (fieldConfigDict.ContainsKey("dataSource"))
                        {
                            Dictionary<string, object> datasourceInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                Newtonsoft.Json.JsonConvert.SerializeObject(fieldConfigDict["dataSource"])
                                );
                            if (datasourceInfo.ContainsKey("sourceId") && datasourceInfo["sourceId"] != null)
                            {
                                string tmp = datasourceInfo["sourceId"].ToString();
                                fieldNameDictType.Add(fieldInfo.FieldName, tmp);

                            }
                        }
                    }
                }
            }
            //根据datasource获取entity(table)信息
            Dictionary<string, object> DataSourceEntityList = this._dynamicEntityRepository.GetAllDataSourceDefine(tran);
            string[] dskeys = dataSources.Keys.ToArray();
            foreach (string key in dskeys)
            {
                if (DataSourceEntityList.ContainsKey(key))
                {
                    dataSources[key] = (string)DataSourceEntityList[key];
                }
                else
                {
                    throw (new Exception("实体配置异常"));
                }
            }

            #region 开始拼装脚本
            string selectClause = "";
            string fromClause = " ";
            string outerSelectClause = "";
            string MainTable = (string)EntityInfo["entitytable"];
            #region 检查是否需要校验权限，如果需要，则检验缺陷
            if (dynamicEntity.NeedPower == 0 && (dynamicEntity.MenuId == null || dynamicEntity.MenuId.Length == 0))
            {
                //这里啥都不做
            }
            else
            {
                if (dynamicEntity.NeedPower != 0)
                {
                    string sql = string.Format("Select crm_func_role_rule_fetch_sql('{0}',{1}) as sql  ", dynamicEntity.EntityId.ToString(), userNumber);
                    MainTable = (string)this._dynamicEntityRepository.ExecuteQuery(sql, tran).FirstOrDefault()["sql"];
                }
                else
                {
                    MainTable = string.Format("select * from {0} where 1=1 ", (string)EntityInfo["entitytable"]);
                }
                //处理menuid
                if (dynamicEntity.MenuId != null && dynamicEntity.MenuId.Length > 0)
                {
                    string sql = string.Format(@"select menu.*,spec.rulesql  
                                                from crm_sys_entity_menu menu 
	                                                left outer join  crm_sys_entity_special_menu spec on spec.specialmenuid = menu.menuid
	                                                left outer join crm_sys_userinfo_role_relate relate on relate.roleid = spec.roleid 
                                                where menu.menuid = '{0}' and relate.userid = {1}", dynamicEntity.MenuId, userNumber);
                    Dictionary<string, object> menuItem = this._dynamicEntityRepository.ExecuteQuery(sql, tran).FirstOrDefault();
                    if (menuItem == null)
                    {
                        sql = string.Format(@"select menu.*,'' rulesql  
                                                from crm_sys_entity_menu menu 
                                                where menu.menuid = '{0}' ", dynamicEntity.MenuId);
                        menuItem = this._dynamicEntityRepository.ExecuteQuery(sql, tran).FirstOrDefault();
                    }
                    if (menuItem != null)
                    {
                        int menutype = 0;
                        string menuspecsql = "";
                        string menuroleid = "";
                        if (menuItem.ContainsKey("menutype") && menuItem["menutype"] != null)
                        {
                            int.TryParse(menuItem["menutype"].ToString(), out menutype);
                        }
                        if (menuItem.ContainsKey("rulesql") && menuItem["rulesql"] != null)
                        {
                            menuspecsql = menuItem["rulesql"].ToString();
                        }
                        if (menuItem.ContainsKey("ruleid") && menuItem["ruleid"] != null)
                        {
                            menuroleid = menuItem["ruleid"].ToString();
                        }
                        if (menutype == 1 && menuspecsql != null && menuspecsql.Length > 0)
                        {
                            MainTable = MainTable + " And " + menuspecsql;
                        }
                        else
                        {
                            sql = string.Format("select crm_func_role_rule_fetch_single_sql('{0}',{1}) as sql ", menuroleid, userNumber);
                            object menusql = this._dynamicEntityRepository.ExecuteQuery(sql, tran).FirstOrDefault()["sql"];
                            if (menusql != null)
                            {
                                MainTable = MainTable + " And (" + menusql + ")";
                            }

                        }
                    }
                }
                #region 格式化脚本

                MainTable = RuleSqlHelper.FormatRuleSql(MainTable, GetUserData(userNumber).AccountUserInfo.UserId, GetUserData(userNumber).AccountUserInfo.DepartmentId);
                MainTable = "(" + MainTable + ")";
                #endregion 
            }

            #endregion
            fromClause = " " + MainTable + " as e  LEFT JOIN crm_sys_userinfo AS u ON u.userid = e.reccreator";
            selectClause = "e.*,u.usericon ";
            outerSelectClause = "outersql.*";
            foreach (DynamicEntityFieldSearch fieldInfo in fieldList)
            {
                EntityFieldControlType controlType = (EntityFieldControlType)fieldInfo.ControlType;
                switch (controlType)
                {
                    case EntityFieldControlType.Address://31,地址
                    case EntityFieldControlType.Location://14定位
                        selectClause = selectClause + ",e." + fieldInfo.FieldName + "->>'address' as " + fieldInfo.FieldName + "_name";
                        break;
                    case EntityFieldControlType.RecCreator://1002创建人
                    case EntityFieldControlType.RecUpdator://1003更新人
                    case EntityFieldControlType.RecManager://1006负责人
                        fromClause = string.Format(@"{0} left outer join crm_sys_userinfo  as {1}_t on e.{1} = {1}_t.userid ", fromClause, fieldInfo.FieldName);
                        selectClause = string.Format(@"{0},{1}_t.username as {1}_name", selectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.AreaGroup://20分组，不处理
                        break;
                    case EntityFieldControlType.AreaRegion://16 行政区域
                        fromClause = string.Format(@"{0} left outer join crm_sys_region  as {1}_t on e.{1} = {1}_t.regionid ", fromClause, fieldInfo.FieldName);
                        selectClause = string.Format(@"{0},{1}_t.fullname as {1}_name", selectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.DataSourceMulti://19数据源多选
                                                                //目前这个没有使用
                        break;
                    case EntityFieldControlType.DataSourceSingle://18数据源单选
                        string tmpDataSourceId = fieldNameMapDataSource[fieldInfo.FieldName];
                        string tablename = dataSources[tmpDataSourceId];
                        fromClause = string.Format(@"{0} left outer join {1} as {2}_t on jsonb_extract_path_text(e.{2},'id') = {2}_t.recid::text ", fromClause, tablename, fieldInfo.FieldName);
                        selectClause = string.Format(@"{0},{1}_t.recname as {1}_name", selectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.RecType://1009记录类型
                        fromClause = string.Format(@"{0} left outer join crm_sys_entity_category  as {1}_t on e.{1} = {1}_t.categoryid ", fromClause, fieldInfo.FieldName);
                        selectClause = string.Format(@"{0},{1}_t.categoryname as {1}_name", selectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.SelectMulti://4本地字典多选
                        break;
                    case EntityFieldControlType.SelectSingle://3本地字典单选
                        fromClause = string.Format(@"{0} left outer join crm_sys_dictionary  as {1}_t on e.{1} = {1}_t.dataid and {1}_t.dictypeid={2} ", fromClause, fieldInfo.FieldName, fieldNameDictType[fieldInfo.FieldName]);
                        selectClause = string.Format(@"{0},{1}_t.dataval as {1}_name", selectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.SalesStage://销售阶段
                        fromClause = string.Format(@"{0} left outer join crm_sys_salesstage_setting  as {1}_t on e.{1} = {1}_t.salesstageid ", fromClause, fieldInfo.FieldName);
                        selectClause = string.Format(@"{0},{1}_t.stagename as {1}_name", selectClause, fieldInfo.FieldName);
                        break;

                    case EntityFieldControlType.RecCreated://1004创建时间
                    case EntityFieldControlType.RecUpdated://
                        selectClause = string.Format(@"{0},TO_CHAR(e.{1},'YYYY-MM-DD HH24:MI:SS') {1}_name", selectClause, fieldInfo.FieldName);
                        break;

                    case EntityFieldControlType.TimeDate://8
                        selectClause = string.Format(@"{0},TO_CHAR(e.{1},'YYYY-MM-DD') {1}_name", selectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.TimeStamp://9
                        selectClause = string.Format(@"{0},TO_CHAR(e.{1},'YYYY-MM-DD HH24:MI:SS') {1}_name", selectClause, fieldInfo.FieldName);
                        break;

                    case EntityFieldControlType.QuoteControl://31引用(分三种情况，)
                        if (fieldInfo.FieldName.ToUpper().Equals("predeptgroup".ToUpper()))
                        {
                            string subTable = @"SELECT
	                                            relate.userid,
	                                            parentdept.deptname
                                            FROM
	                                            crm_sys_department tmpa
                                            INNER JOIN crm_sys_account_userinfo_relate relate ON relate.deptid = tmpa.deptid
                                            inner join crm_sys_department parentdept on parentdept.deptid = tmpa.pdeptid
                                            WHERE
	                                            relate.recstatus = 1";
                            fromClause = string.Format(@"{0} left outer join ({1}) as {2}_t on e.recmanager = {2}_t.userid ", fromClause, subTable, fieldInfo.FieldName);
                            selectClause = string.Format(@"{0},{1}_t.deptname as {1}_name", selectClause, fieldInfo.FieldName);

                        }
                        else if (fieldInfo.FieldName.ToUpper().Equals("deptgroup".ToUpper()))
                        {
                            string subTable = @"select  relate.userid,tmpa.deptname
                                                from crm_sys_department tmpa
	                                                inner join crm_sys_account_userinfo_relate relate on relate.deptid = tmpa.deptid
                                                where  relate.recstatus = 1 ";
                            fromClause = string.Format(@"{0} left outer join ({1}) as {2}_t on e.recmanager = {2}_t.userid ", fromClause, subTable, fieldInfo.FieldName);
                            selectClause = string.Format(@"{0},{1}_t.deptname as {1}_name", selectClause, fieldInfo.FieldName);

                        }
                        else
                        {
                            outerSelectClause = string.Format(@"{0},crm_func_entity_protocol_format_quote_control(row_to_json(outersql)::json,'{1}','{2}') as {3}_name", outerSelectClause, dynamicEntity.EntityId.ToString(), fieldInfo.FieldId, fieldInfo.FieldName);
                        }
                        break;
                    case EntityFieldControlType.Department://17部门
                        outerSelectClause = string.Format(@"{0},crm_func_entity_protocol_format_dept_multi(outersql.{1}::text) as {1}_name", outerSelectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.EmailAddr://11email地址
                    case EntityFieldControlType.FileAttach://23 文件地址
                    case EntityFieldControlType.HeadPhoto://15 头像
                    case EntityFieldControlType.LinkeTable://24关联表格
                    case EntityFieldControlType.NumberDecimal:
                    case EntityFieldControlType.NumberInt:
                    case EntityFieldControlType.PhoneNum://
                                                         //不需要处理
                        break;
                    case EntityFieldControlType.PersonSelectMulti://26人员多选
                        outerSelectClause = string.Format(@"{0},crm_func_entity_protocol_format_userinfo_multi(outersql.{1}) as {1}_name", outerSelectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.PersonSelectSingle://25人员单选
                        outerSelectClause = string.Format(@"{0},crm_func_entity_protocol_format_userinfo_multi(outersql.{1}) as {1}_name", outerSelectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.Product://28产品
                        outerSelectClause = string.Format(@"{0},crm_func_entity_protocol_format_product_multi(outersql.{1}) as {1}_name", outerSelectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.ProductSet://29产品系列
                        outerSelectClause = string.Format(@"{0},crm_func_entity_protocol_format_productserial_multi(outersql.{1}) as {1}_name", outerSelectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.RecAudits://1007记录状态
                        outerSelectClause = string.Format(@"{0},crm_func_entity_protocol_format_workflow_auditstatus(outersql.{1}) as {1}_name", outerSelectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.RecId://1001
                        break;
                    case EntityFieldControlType.RecItemid://1010明细ID
                        break;
                        break;
                    case EntityFieldControlType.RecName://1012记录名称
                        break;
                    case EntityFieldControlType.RecOnlive://1011 活动时间
                        break;
                    case EntityFieldControlType.RecStatus://1008 记录状态
                        break;
                    case EntityFieldControlType.TakePhoto://
                        break;
                    case EntityFieldControlType.Telephone:
                        break;
                    case EntityFieldControlType.Text:
                        break;
                    case EntityFieldControlType.TextArea:
                        break;
                    case EntityFieldControlType.TipText:
                        break;
                    case EntityFieldControlType.TreeMulti://27树形多选
                        break;
                    case EntityFieldControlType.TreeSingle://21树形
                        break;
                }
            }
            #endregion
            string WhereSQL = "1=1";
            string OrderBySQL = " reccreated desc ";
            if (dynamicEntity.SearchQuery != null && dynamicEntity.SearchQuery.Length > 0)
            {
                WhereSQL = " 1=1    " + dynamicEntity.SearchQuery;
            }
            string innerSQL = string.Format(@"select {0} from {1}  where  {2} order by {3} limit {4} offset {5}",
                selectClause, fromClause, WhereSQL, OrderBySQL, pageParam.PageSize, (pageParam.PageIndex - 1) * pageParam.PageSize);
            string strSQL = string.Format(@"Select {0} from ({1}) as outersql", outerSelectClause, innerSQL);
            string CountSQL = string.Format(@"select total,(total-1)/{2}+1 as page from (select count(*)  AS total  from {0} where  {1} ) as k", fromClause, WhereSQL, pageParam.PageSize);
            List<Dictionary<string, object>> datas = this._dynamicEntityRepository.ExecuteQuery(strSQL, tran);
            List<Dictionary<string, object>> page = this._dynamicEntityRepository.ExecuteQuery(CountSQL, tran);
            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();
            retData.Add("PageData", datas);
            retData.Add("PageCount", page);

            return new OutputResult<object>(retData);
        }
        public OutputResult<object> DataList2(DynamicEntityListModel dynamicModel, bool isAdvanceQuery, int userNumber)
        {

            string SpecFuncName = _dynamicEntityRepository.CheckDataListSpecFunction(dynamicModel.EntityId);
            
            var dynamicEntity = _mapper.Map<DynamicEntityListModel, DynamicEntityListMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }

            var pageParam = new PageParam { PageIndex = dynamicModel.PageIndex, PageSize = dynamicModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }



            if (dynamicModel.SearchData != null && dynamicModel.SearchData.Count > 0)
            {
                //生成查询语句
                var searchFields = isAdvanceQuery ? GetSearchFields(dynamicEntity.EntityId, userNumber) : GetEntityFields(dynamicEntity.EntityId, userNumber);

                if (searchFields == null)
                {
                    return ShowError<object>("未查询到相应的查询参数");
                }


                var validResults = isAdvanceQuery ? DynamicProtocolHelper.AdvanceQuery2(searchFields, dynamicModel.SearchData) : DynamicProtocolHelper.SimpleQuery(searchFields, dynamicModel.SearchData);
                if (SpecFuncName != null) {
                    validResults = isAdvanceQuery ? DynamicProtocolHelper.AdvanceQuery(searchFields, dynamicModel.SearchData) : DynamicProtocolHelper.SimpleQuery(searchFields, dynamicModel.SearchData);
                }
                
                var validTips = new List<string>();
                var data = new Dictionary<string, string>();



                foreach (DynamicProtocolValidResult validResult in validResults.Values)
                {
                    if (!validResult.IsValid)
                    {
                        validTips.Add(validResult.Tips);
                    }
                    data.Add(validResult.FieldName, validResult.FieldData.ToString());

                }

                if (validTips.Count > 0)
                {
                    return ShowError<object>(string.Join(";", validTips));
                }

                if (data.Count > 0)
                {
                    dynamicEntity.SearchQuery = " AND " + string.Join(" AND ", data.Values.ToArray());
                }
            }

            if (dynamicModel.SearchDataXOR != null && dynamicModel.SearchDataXOR.Count > 0)
            {
                //生成查询语句
                var searchFields = GetEntityFields(dynamicEntity.EntityId, userNumber);

                if (searchFields == null)
                {
                    return ShowError<object>("未查询到相应的查询参数");
                }


                var validResults = DynamicProtocolHelper.SimpleQuery(searchFields, dynamicModel.SearchDataXOR);
                var validTips = new List<string>();
                var data = new Dictionary<string, string>();



                foreach (DynamicProtocolValidResult validResult in validResults.Values)
                {
                    if (!validResult.IsValid)
                    {
                        validTips.Add(validResult.Tips);
                    }
                    data.Add(validResult.FieldName, validResult.FieldData.ToString());

                }

                if (validTips.Count > 0)
                {
                    return ShowError<object>(string.Join(";", validTips));
                }

                if (data.Count > 0)
                {
                    dynamicEntity.SearchQuery = dynamicEntity.SearchQuery + " AND (" + string.Join(" OR ", data.Values.ToArray()) + " ) ";
                }
            }



            //处理排序语句
            if (string.IsNullOrWhiteSpace(dynamicEntity.SearchOrder))
            {
                dynamicEntity.SearchOrder = "";
            }
            return this.CommonDataList(dynamicEntity, pageParam, isAdvanceQuery, userNumber);
        }

        public OutputResult<object> Detail(DynamicEntityDetailModel dynamicModel, int userNumber)
        {
            var dynamicEntity = _mapper.Map<DynamicEntityDetailModel, DynamicEntityDetailtMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }

            var result = _dynamicEntityRepository.DetailMulti(dynamicEntity, userNumber);
            // IEnumerable<DynamicEntityFieldSearch> linkTableFields = new List<DynamicEntityFieldSearch>();

            Guid entityid = dynamicEntity.EntityId;
            if (dynamicEntity.EntityId == new Guid("00000000-0000-0000-0000-000000000001"))
            {
                var workflowcaseInfo = _workFlowRepository.GetWorkFlowCaseInfo(null, dynamicModel.RecId);
                if (workflowcaseInfo != null)
                {
                    var workflowInfo = _workFlowRepository.GetWorkFlowInfo(null, workflowcaseInfo.FlowId);
                    if (workflowInfo != null)
                    {
                        entityid = workflowInfo.Entityid;

                        result["entitydetail"] = DealLinkTableFields(result["entitydetail"], entityid, userNumber);
                        var relatedetail = result["relatedetail"];
                        if (relatedetail.Count > 0)
                        {
                            relatedetail = DealLinkTableFields(relatedetail, workflowInfo.RelEntityId, userNumber);
                        }
                    }
                }
            }
            else
            {
                if (result.ContainsKey("Detail") == false
                    || result["Detail"] == null
                    || result["Detail"].Count == 0)
                {
                    return new OutputResult<object>(null, "权限不足或者数据已经被删除", -1);
                }

                result["Detail"] = DealLinkTableFields(result["Detail"], entityid, userNumber);
            }

            if (result.Count == 1)
            {
                var singleResult = new Dictionary<string, object>();
                singleResult.Add(result.Keys.First(), result.Values.First().FirstOrDefault());
                return new OutputResult<object>(singleResult);
            }
            else return new OutputResult<object>(result);
        }

        private List<IDictionary<string, object>> DealLinkTableFields(List<IDictionary<string, object>> data, Guid entityid, int userNumber)
        {
            var searchFields = GetEntityFields(entityid, userNumber);
            var linkTableFields = searchFields.Where(m => (DynamicProtocolControlType)m.ControlType == DynamicProtocolControlType.LinkeTable).ToList();
            foreach (var filed in linkTableFields)
            {
                var fieldConfig = JObject.Parse(filed.FieldConfig);
                var linketable_entityid = new Guid(fieldConfig["entityId"].ToString());


                foreach (var row in data)
                {
                    if (row.ContainsKey(filed.FieldName))
                    {
                        var linketableRecids = row[filed.FieldName] == null ? "" : row[filed.FieldName].ToString();
                        if (string.IsNullOrEmpty(linketableRecids))
                            continue;
                        DynamicEntityDetailtListMapper modeltemp = new DynamicEntityDetailtListMapper()
                        {
                            EntityId = linketable_entityid,
                            RecIds = linketableRecids,
                            NeedPower = 0
                        };

                        row[filed.FieldName] = _dynamicEntityRepository.DetailList(modeltemp, userNumber);
                    }
                }

            }
            return data;
        }




        public OutputResult<object> DetailList(DynamicEntityDetaillistModel dynamicModel, int userNumber)
        {
            var dynamicEntity = new DynamicEntityDetailtListMapper()
            {
                EntityId = dynamicModel.EntityId,
                NeedPower = dynamicModel.NeedPower,
                RecIds = dynamicModel.RecIds
            };
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }

            var result = _dynamicEntityRepository.DetailList(dynamicEntity, userNumber);

            return new OutputResult<object>(result);
        }

        /// <summary>
        /// 获取功能按钮列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> GetFunctionBtns(FunctionBtnsModel dynamicModel, int userNumber)
        {
            List<FunctionBtnInfo> funcBtns = new List<FunctionBtnInfo>();
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null)
                return new OutputResult<object>(funcBtns);
            //获取个人用户数据
            UserData userData = GetUserData(userNumber);
            List<FunctionInfo> funcs = new List<FunctionInfo>();
            if (userData != null && userData.Vocations != null)
            {

                foreach (var m in userData.Vocations)
                {
                    funcs.AddRange(m.Functions.Where(a => a.EntityId == dynamicModel.EntityId && a.DeviceType == (int)DeviceClassic));
                }
            }
            foreach (var btn in info.FuncBtns)
            {
                //if(btn.FunctionId==Guid.Empty|| funcs.Exists(m=>m.FuncId==btn.FunctionId))
                if (!string.IsNullOrEmpty(btn.RoutePath) && funcs.Exists(m => m.RoutePath == btn.RoutePath && m.DeviceType == (int)DeviceClassic))
                {
                    switch (btn.SelectType)
                    {
                        case DataSelectType.Single:
                            if (dynamicModel.RecIds != null && dynamicModel.RecIds.Count == 1)
                            {
                                var checkAccess = userData.HasDataAccess(null, btn.RoutePath, dynamicModel.EntityId, DeviceClassic, dynamicModel.RecIds);
                                if (checkAccess)
                                    funcBtns.Add(btn);
                            }
                            break;
                        case DataSelectType.Multiple:
                            if (dynamicModel.RecIds != null && dynamicModel.RecIds.Count >= 1)
                            {
                                var checkAccess = userData.HasDataAccess(null, btn.RoutePath, dynamicModel.EntityId, DeviceClassic, dynamicModel.RecIds);
                                if (checkAccess)
                                    funcBtns.Add(btn);
                            }
                            break;
                        default:
                            funcBtns.Add(btn);
                            break;
                    }

                }
            }

            return new OutputResult<object>(funcBtns);
        }



        public OutputResult<object> PluginCheckVisible(DynamicPluginVisibleModel dynamicModel, int userNumber)
        {
            var dynamicEntity = _mapper.Map<DynamicPluginVisibleModel, DynamicPluginVisibleMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }
            dynamicEntity.DeviceClassic = (int)DeviceClassic;

            var result = _dynamicEntityRepository.PluginVisible(dynamicEntity, userNumber);

            return new OutputResult<object>(result);
        }


        public OutputResult<object> PageCheckVisible(DynamicPageVisibleModel dynamicModel, int userNumber)
        {
            var dynamicEntity = _mapper.Map<DynamicPageVisibleModel, DynamicPageVisibleMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }

            var result = _dynamicEntityRepository.PageVisible(dynamicEntity, userNumber);

            return new OutputResult<object>(result);
        }

        public List<DynamicEntityDataFieldMapper> GetTypeFields(Guid typeId, DynamicProtocolOperateType operateType, int userNumber)
        {
            return _dynamicEntityRepository.GetTypeFields(typeId, (int)operateType, userNumber);
        }
        public List<DynamicEntityDataFieldMapper> GetGridTypeFields(Guid typeId, Guid entityId, DynamicProtocolOperateType operateType, int userNumber)
        {
            return _dynamicEntityRepository.GetTypeFields(typeId, (int)operateType, userNumber);
        }

        public List<DynamicEntityWebFieldMapper> GetWebFields(Guid typeId, DynamicProtocolOperateType operateType, int userNumber)
        {
            return _dynamicEntityRepository.GetWebFields(typeId, (int)operateType, userNumber);
        }

        public List<DynamicEntityFieldSearch> GetSearchFields(Guid entityId, int userNumber)
        {
            return _dynamicEntityRepository.GetSearchFields(entityId, userNumber);
        }

        public List<DynamicEntityFieldSearch> GetEntityFields(Guid entityId, int userNumber)
        {
            return _dynamicEntityRepository.GetEntityFields(entityId, userNumber);
        }

        public OutputResult<object> Transfer(DynamicEntityTransferModel dynamicModel, int userNumber)
        {
            var dynamicEntity = _mapper.Map<DynamicEntityTransferModel, DynamicEntityTransferMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }
            var entityId = dynamicEntity.EntityId;
            string[] recids = dynamicEntity.RecId.Split(',');
            List<Guid> lstGuid = new List<Guid>();
            var oldDetailList = new Dictionary<Guid, IDictionary<string, object>>();
            recids.ToList().ForEach(t =>
            {
                var recid = Guid.Parse(t);
                var detailMapper = new DynamicEntityDetailtMapper()
                {
                    EntityId = entityId,
                    RecId = recid,
                    NeedPower = 0
                };
                if (!oldDetailList.ContainsKey(recid))
                    oldDetailList.Add(recid, _dynamicEntityRepository.Detail(detailMapper, userNumber));
                lstGuid.Add(recid);
            });


            return ExcuteDeleteAction((transaction, arg, userData) =>
                {
                    var result = _dynamicEntityRepository.Transfer(dynamicEntity, userNumber);
                    if (result.Flag == 1)
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                var entityInfotemp = _entityProRepository.GetEntityInfo(dynamicEntity.EntityId);
                                foreach (var recid in recids)
                                {
                                    var bussinessId = new Guid(recid);
                                    var oldDetail = oldDetailList[bussinessId];

                                    DynamicEntityDetailtMapper detailMapper = new DynamicEntityDetailtMapper()
                                    {
                                        EntityId = entityId,
                                        RecId = bussinessId,
                                        NeedPower = 0
                                    };

                                    //var typeid = entityId;
                                    var relbussinessId = Guid.Empty;

                                    var newDetail = _dynamicEntityRepository.Detail(detailMapper, userNumber);
                                    var newMembers = MessageService.GetEntityMember(newDetail as Dictionary<string, object>);
                                    var oldMembers = MessageService.GetEntityMember(oldDetail as Dictionary<string, object>);

                                    //编辑操作的消息
                                    var msg = MessageService.GetEntityMsgParameter(entityInfotemp, bussinessId, relbussinessId, "EntityDataTransfer", userNumber, newMembers, oldMembers);
                                    MessageService.WriteMessageAsyn(msg, userNumber);
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                    }
                    return HandleResult(result);
                }, dynamicEntity, entityId, userNumber, lstGuid);
        }

        public OutputResult<object> Delete(DynamicEntityDeleteModel dynamicModel, int userNumber)
        {
            if (dynamicModel?.EntityId == null)
            {
                return ShowError<object>("实体ID不能为空");
            }

            if (string.IsNullOrWhiteSpace(dynamicModel?.RecId))
            {
                return ShowError<object>("记录ID不能为空");
            }
            var entityId = dynamicModel.EntityId;
            var typeid = dynamicModel.TypeId;
            var relbussinessId = dynamicModel.RelRecId;

            var recList = new List<Guid>();

            string[] recids = dynamicModel.RecId.Split(',');

            foreach (var recid in recids)
            {
                recList.Add(new Guid(recid));
            }

            return ExcuteDeleteAction((transaction, arg, userData) =>
            {

                DynamicEntityDetailtListMapper detailMapper = new DynamicEntityDetailtListMapper()
                {
                    EntityId = entityId,
                    RecIds = dynamicModel.RecId,
                    NeedPower = 0
                };
                //先获取详情再删除数据
                var details = _dynamicEntityRepository.DetailList(detailMapper, userNumber);

                //删除数据
                var result = _dynamicEntityRepository.Delete(dynamicModel.EntityId, dynamicModel.RecId, dynamicModel.PageType, dynamicModel.PageCode, userNumber);
                if (result.Flag == 1)
                {
                    Task.Run(() =>
                    {
                        foreach (var detail in details)
                        {
                            typeid = Guid.Parse(detail["rectype"].ToString());
                            var entityInfotemp = _entityProRepository.GetEntityInfo(typeid);
                            //var entityInfo = _entityProRepository.GetEntityInfo(dynamicModel.EntityId, userNumber);
                            var newMembers = MessageService.GetEntityMember(detail as Dictionary<string, object>);

                            var entityMsg = MessageService.GetEntityMsgParameter(entityInfotemp, new Guid(detail["recid"].ToString()), relbussinessId, "EntityDataDelete", userNumber, newMembers, null, null);
                            MessageService.WriteMessageAsyn(entityMsg, userNumber);
                            //处理数据源控件需要触发消息的逻辑
                            CheckServicesJson(OperatType.Delete, entityInfotemp.Servicesjson, detail as Dictionary<string, object>, userNumber);
                        }
                    });
                }
                return HandleResult(result);
            }, dynamicModel, (Guid)dynamicModel.EntityId, userNumber, recList);

        }
        public OutputResult<object> DeleteDataSrcRelation(DataSrcDeleteRelationModel dynamicModel, int userNumber)
        {
            var entity = _mapper.Map<DataSrcDeleteRelationModel, DataSrcDeleteRelationMapper>(dynamicModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            //先获取详情再删除数据
            var details = _dynamicEntityRepository.DeleteDataSrcRelation(entity, userNumber);

            return HandleResult(details);
        }
        public OutputResult<object> AddConnect(DynamicEntityAddConnectModel connectModel, int userNumber)
        {
            var connectEntity = _mapper.Map<DynamicEntityAddConnectModel, DynamicEntityAddConnectMapper>(connectModel);
            if (connectEntity == null || !connectEntity.IsValid())
            {
                return HandleValid(connectEntity);
            }

            var result = _dynamicEntityRepository.AddConnect(connectEntity, userNumber);

            return HandleResult(result);
        }

        public OutputResult<object> EditConnect(DynamicEntityEditConnectModel connectModel, int userNumber)
        {
            var connectEntity = _mapper.Map<DynamicEntityEditConnectModel, DynamicEntityEditConnectMapper>(connectModel);
            if (connectEntity == null || !connectEntity.IsValid())
            {
                return HandleValid(connectEntity);
            }

            var result = _dynamicEntityRepository.EditConnect(connectEntity, userNumber);

            return HandleResult(result);
        }

        public OutputResult<object> DeleteConnect(DynamicEntityDeleteConnectModel connectModel, int userNumber)
        {
            if (connectModel?.ConnectId == null)
            {
                return ShowError<object>("关系ID不能为空");
            }

            var result = _dynamicEntityRepository.DeleteConnect(connectModel.ConnectId, userNumber);

            return HandleResult(result);
        }

        public OutputResult<object> ConnectList(DynamicEntityConnectListModel listModel, int userNumber)
        {
            var connectEntity = _mapper.Map<DynamicEntityConnectListModel, DynamicEntityConnectListMapper>(listModel);
            if (connectEntity == null || !connectEntity.IsValid())
            {
                return HandleValid(connectEntity);
            }

            var result = _dynamicEntityRepository.ConnectList(connectEntity.EntityId, connectEntity.RecId, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> EntitySearchList(DynamicEntitySearchListModel listModel, int userNumber)
        {
            if (listModel?.ModelType == null)
            {
                return ShowError<object>("模型类型不能为空");
            }
            var result = _dynamicEntityRepository.EntitySearchList(listModel.ModelType, listModel.SearchData, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> EntitySearchRepeat(DynamicEntitySearchRepeatModel listModel, int userNumber)
        {
            if (listModel?.EntityId == null)
            {
                return ShowError<object>("实体ID不能为空");
            }

            if (string.IsNullOrWhiteSpace(listModel?.CheckName))
            {
                return ShowError<object>("查重名称不能为空");
            }
            var result = _dynamicEntityRepository.EntitySearchRepeat(listModel.EntityId, listModel.CheckField, listModel.CheckName, listModel.Exact, listModel.SearchData, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> RelTabList(DynamicRelTabModel tabModel, int userNumber)
        {
            string SpecFuncName = _dynamicEntityRepository.CheckDataListSpecFunction(tabModel.RelEntityId);
            string sqlWhere = _dynamicEntityRepository.ReturnRelTabSql(tabModel.RelId, tabModel.RecId, userNumber);
            if (sqlWhere != null && sqlWhere.StartsWith("and recid"))
            {//兼容历史
                if (SpecFuncName != null)
                {
                    sqlWhere = sqlWhere.Replace("and recid", "and t.recid");
                }
                else
                {
                    sqlWhere = sqlWhere.Replace("and recid", "and e.recid");
                }
            }
            DynamicEntityListMapper mapper = new DynamicEntityListMapper
            {
                EntityId = tabModel.RelEntityId,
                MenuId = null,
                ViewType = tabModel.ViewType,
                SearchOrder = string.Empty
            };
            //生成查询语句
            var searchFields = GetEntityFields(tabModel.RelEntityId, userNumber);
            if (searchFields == null)
            {
                mapper.SearchQuery = sqlWhere;
            }
            else
            {
                //支持简单查询
                if (searchFields.Count > 0 && !string.IsNullOrEmpty(tabModel.keyWord))
                {
                    Dictionary<string, object> searchData = new Dictionary<string, object>();
                    Dictionary<string, List<IDictionary<string, object>>> searchMap = _entityProRepository.EntityFieldFilterQuery(tabModel.RelEntityId.ToString(), userNumber);
                    if (searchMap["simple"] != null && searchMap["simple"].Count > 0)
                    {
                        Dictionary<string, object> simpleobj = new Dictionary<string, object>(searchMap["simple"][0]);
                        searchData.Add(simpleobj["fieldname"].ToString(), tabModel.keyWord);
                        var validResults = DynamicProtocolHelper.SimpleQuery(searchFields, searchData);
                        var validTips = new List<string>();
                        var data = new Dictionary<string, string>();

                        foreach (DynamicProtocolValidResult validResult in validResults.Values)
                        {
                            if (!validResult.IsValid)
                            {
                                validTips.Add(validResult.Tips);
                            }
                            data.Add(validResult.FieldName, validResult.FieldData.ToString());

                        }

                        if (validTips.Count > 0)
                        {
                            return ShowError<object>(string.Join(";", validTips));
                        }

                        if (data.Count > 0)
                        {
                            mapper.SearchQuery = sqlWhere + " AND " + string.Join(" AND ", data.Values.ToArray());
                        }
                    }
                    else
                    {
                        mapper.SearchQuery = sqlWhere;
                    }
                }
                else
                {
                    mapper.SearchQuery = sqlWhere;
                }

            }

            var pageParam = new PageParam { PageIndex = tabModel.PageIndex, PageSize = tabModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }
            //var result = _dynamicEntityRepository.DataList(pageParam, null, mapper, userNumber);

            //return new OutputResult<object>(result);
            return this.CommonDataList(mapper, pageParam, false, userNumber);
        }

        public OutputResult<object> RelTabSrcSqlList(DynamicRelTabModel tabModel, int userNumber)
        {
            RelTabSrcMapper srcSqlInfo = _dynamicEntityRepository.ReturnRelTabSrcSql(tabModel.RelId);

            var result = _dynamicEntityRepository.GetSrcSqlDataList(tabModel.RecId, srcSqlInfo.SrcSql, string.Empty, userNumber);

            return new OutputResult<object>(result);
        }

        public OutputResult<object> RelTabListQuery(RelTabListModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<RelTabListModel, RelTabListMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            Dictionary<string, List<IDictionary<string, object>>> result = _dynamicEntityRepository.RelTabListQuery(entity, userNumber);
            var relTabList = result["RelTabList"];
            if (relTabList.Count == 0)
            {
                //初始化默认标签
                //initDefaultTab(entity, userNumber);
                //result = _dynamicEntityRepository.RelTabListQuery(entity, userNumber);
            }

            return new OutputResult<object>(result);
        }


        public OutputResult<object> RelTabListQueryByRole(RelTabListModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<RelTabListModel, RelTabListMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            Dictionary<string, List<IDictionary<string, object>>> result = _dynamicEntityRepository.RelTabListQuery(entity, userNumber);
            var relTabList = result["RelTabList"];
            if (relTabList.Count == 0)
            {
                //初始化默认标签
                //initDefaultTab(entity, userNumber);
                //result = _dynamicEntityRepository.RelTabListQuery(entity, userNumber);
            }
            else
            {
                List<IDictionary<string, object>> lst = new List<IDictionary<string, object>>();

                if (DeviceClassic == DeviceClassic.WEB)
                {
                    relTabList = relTabList.Where(a => a["web"].ToString() == "1").ToList();
                }
                else
                {
                    relTabList = relTabList.Where(a => a["mob"].ToString() == "1").ToList();
                }
                GetUserData(userNumber).Vocations.ForEach(t =>
                {
                    foreach (var tmp in relTabList)
                    {
                        string uuid = tmp["relid"].ToString();

                        if (!string.IsNullOrEmpty(uuid))
                        {
                            var functions = t.Functions.Where(b => b.EntityId == entity.EntityId && b.RecType == FunctionType.EntityTab && b.DeviceType == (int)DeviceClassic);//拿子节点对上一级 的父节点的id
                            if (functions.Count() != 1) throw new Exception("职能权限数据异常");
                            var function = functions.SingleOrDefault();
                            functions = t.Functions.Where(a => a.ParentId == function.FuncId);
                            if (functions.Any(a => a.RelationValue == uuid))
                            {
                                if (!lst.Contains(tmp))
                                    lst.Add(tmp);
                            }
                        }
                    }
                });
                result["reltablist"] = lst;
            }

            return new OutputResult<object>(result);
        }

        public OutputResult<object> initDefaultTab(RelTabListMapper entity, int userNumber)
        {
            var result = _dynamicEntityRepository.initDefaultTab(entity, userNumber);
            return new OutputResult<object>(result);
        }


        public OutputResult<object> RelTabInfoQuery(RelTabInfoModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<RelTabInfoModel, RelTabInfoMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _dynamicEntityRepository.RelTabInfoQuery(entity, userNumber);
            return new OutputResult<object>(result);
        }



        public OutputResult<object> AddRelTab(AddRelTabModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AddRelTabModel, AddRelTabMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _dynamicEntityRepository.AddRelTab(entity, userNumber);
            if (result.Flag == 1)
            {
                //GetCommonCacheData(userNumber, true);
                //RemoveUserDataCache(userNumber);
                RemoveCommonCache();
                RemoveAllUserCache();
                IncreaseDataVersion(DataVersionType.EntityData);
                IncreaseDataVersion(DataVersionType.PowerData);
            }


            return HandleResult(result);
        }



        public OutputResult<object> UpdateRelTab(UpdateRelTabModel entityModel, int userNumber)
        {
            if (entityModel.type == 0)
            {
                var entity = _mapper.Map<UpdateRelTabModel, UpdateRelTabMapper>(entityModel);
                if (entity == null || !entity.IsValid())
                {
                    return HandleValid(entity);
                }

                var result = _dynamicEntityRepository.UpdateRelTab(entity, userNumber);
                if (result.Flag == 1)
                {
                    RemoveCommonCache();
                    //RemoveAllUserCache();
                    IncreaseDataVersion(DataVersionType.EntityData);
                    //IncreaseDataVersion(DataVersionType.PowerData);
                }
                return HandleResult(result);
            }
            else
            {
                UpdateRelTabMapper entity = new UpdateRelTabMapper
                {
                    RelId = entityModel.RelId,
                    RelName = entityModel.RelName
                };
                var result = _dynamicEntityRepository.UpdateDefaultRelTab(entity, userNumber);
                if (result > 0)
                {
                    RemoveCommonCache();
                    //RemoveAllUserCache();
                    IncreaseDataVersion(DataVersionType.EntityData);
                    //IncreaseDataVersion(DataVersionType.PowerData);
                }
                return new OutputResult<object>(result);
            }
        }


        public OutputResult<object> DisabledRelTab(DisabledRelTabModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<DisabledRelTabModel, DisabledRelTabMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _dynamicEntityRepository.DisabledRelTab(entity, userNumber);
            if (result.Flag == 1)
            {
                RemoveCommonCache();
                RemoveAllUserCache();
                IncreaseDataVersion(DataVersionType.EntityData);
                IncreaseDataVersion(DataVersionType.PowerData);
            }
            return HandleResult(result);
        }
        public OutputResult<object> OrderbyRelTab(OrderbyRelTabModel entityModel, int userNumber)
        {
            var entity = new OrderbyRelTabMapper()
            {
                RelIds = entityModel.RelIds
            };
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _dynamicEntityRepository.OrderbyRelTab(entity, userNumber);
            if (result.Flag == 1)
            {
                IncreaseDataVersion(DataVersionType.EntityData);
            }
            return HandleResult(result);
        }
        public OutputResult<object> AddRelTabRelationDataSrc(AddRelTabRelationDataSrcModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AddRelTabRelationDataSrcModel, AddRelTabRelationDataSrcMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _dynamicEntityRepository.AddRelTabRelationDataSrc(entity, userNumber);

            return HandleResult(result);
        }
        public OutputResult<object> IsHasPerssion(PermissionModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<PermissionModel, PermissionMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _dynamicEntityRepository.IsHasPerssion(entity, userNumber);
            return HandleResult(result);
        }

        public bool IsValidEntityPower(dynamic entityInfo)
        {
            //简单实体 没有关联实体 且走审批的就不校验权限
            if (entityInfo.modeltype == 2 && entityInfo.relentityid == null && entityInfo.relaudit == 1)
                return true;
            return false;
        }


        /// <summary>
        /// 用户关注的数据
        /// </summary>
        /// <param name="entityModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> FollowRecord(FollowModel entityModel, int userNumber)
        {
            var entity = new FollowRecordMapper()
            {
                RelId = entityModel.RecId,
                EntityId = entityModel.EntityId,
                IsFollow = entityModel.IsFollow
            };

            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _dynamicEntityRepository.FollowRecord(entity, userNumber);

            /*  //关注成功,发送消息
              if (result.Flag == 1&& entityModel.IsFollow)
              {
                  DynamicEntityDetailtListMapper detailMapper = new DynamicEntityDetailtListMapper()
                  {
                      EntityId = entityModel.EntityId,
                      RecIds = entityModel.RecId.ToString(),
                      NeedPower = 0
                  };

                  //获取详情
                  var detail = _dynamicEntityRepository.DetailList(detailMapper, userNumber).FirstOrDefault();

                  //发送消息
                  Task.Run(() =>
                  {
                      var entityInfotemp = _entityProRepository.GetEntityInfo(entityModel.RecType);
                      var entityInfo = _entityProRepository.GetEntityInfo(entityModel.EntityId, userNumber);
                      var newMembers = MessageService.GetEntityMember(detail as Dictionary<string, object>);

                      var entityMsg = MessageService.GetEntityMsgParameter(entityInfotemp, entityModel.RecId, Guid.Empty, "EntityDataFollow", userNumber, newMembers, null, null);
                      MessageService.WriteMessageAsyn(entityMsg, userNumber);

                  });
              }*/

            return HandleResult(result);
        }




        public OutputResult<object> MarkRecordComplete(Guid recId, int userNumber)
        {
            var result = _dynamicEntityRepository.MarkRecordComplete(recId, userNumber);
            return HandleResult(result);

        }


    }
}
