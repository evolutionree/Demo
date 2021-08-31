using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Dynamics;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Message;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.Rule;
using UBeat.Crm.CoreApi.Repository.Repository.Vocation;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.Services.Models.SoapErp;
using UBeat.Crm.CoreApi.Services.Models.WorkFlow;
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
        private readonly IDataSourceRepository _dataSourceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly JavaScriptUtilsServices _javaScriptUtilsServices;
        private readonly IEntityTransferRepository _entityTransferRepository = null;
        private readonly RuleTranslatorServices _translatorServices;
        private readonly RuleTranslatorServices _ruleServices;
        private Logger _logger = LogManager.GetLogger("UBeat.Crm.CoreApi.Services.Services.DynamicEntityServices");

        //private readonly WorkFlowServices _workflowService; 

        private readonly IMapper _mapper;

        public DynamicEntityServices(IMapper mapper, IDynamicEntityRepository dynamicEntityRepository, IEntityProRepository entityProRepository,
                IWorkFlowRepository workFlowRepository, IDynamicRepository dynamicRepository, IAccountRepository accountRepository,
                IDataSourceRepository dataSourceRepository,
                ICustomerRepository customerRepository,
                IEntityTransferRepository entityTransferRepository,
                               RuleTranslatorServices translatorServices,
                JavaScriptUtilsServices javaScriptUtilsServices)
        {
            _dynamicEntityRepository = dynamicEntityRepository;
            _entityProRepository = entityProRepository;
            _workFlowRepository = workFlowRepository;
            _mapper = mapper;
            _dynamicRepository = dynamicRepository;
            _accountRepository = accountRepository;
            _dataSourceRepository = dataSourceRepository;
            _customerRepository = customerRepository;
            _javaScriptUtilsServices = javaScriptUtilsServices;
            _entityTransferRepository = entityTransferRepository;
            _translatorServices = translatorServices;
            _ruleServices = translatorServices;
            //_workflowService = workflowService; 
        }

        public OutputResult<object> Add(DynamicEntityAddModel dynamicModel, AnalyseHeader header, int userNumber)
        {
            var entityInfo = _entityProRepository.GetEntityInfo(dynamicModel.TypeId);
            OutputResult<object> addRes = null;
            var res = ExcuteInsertAction((transaction, arg, userData) =>
            {
                if (dynamicModel.CacheId != Guid.Empty && !_dynamicEntityRepository.ExistsData(dynamicModel.CacheId, userNumber, transaction))
                    _dynamicEntityRepository.DeleteTemporary(dynamicModel.CacheId, userNumber, transaction);
                WorkFlowAddCaseModel workFlowAddCaseModel = null;
                return addRes = AddEntityData(transaction, userData, entityInfo, arg, header, userNumber, out workFlowAddCaseModel);

            }, dynamicModel, entityInfo.EntityId, userNumber);
            if (res.Status == 1)
                return res;
            if (addRes.Status == 0)
            {
                var bussinessId = Guid.Parse(addRes.DataBody.ToString());
                var relbussinessId = dynamicModel.RelRecId.GetValueOrDefault();
                if (!dynamicModel.FlowId.HasValue)
                {
                    //单据转换消息
                    if (dynamicModel.ExtraData != null && dynamicModel.ExtraData.ContainsKey("funccode") && dynamicModel.ExtraData["funccode"] != null
                        && dynamicModel.ExtraData.ContainsKey("entityId") && dynamicModel.ExtraData["entityId"] != null
                        && dynamicModel.ExtraData.ContainsKey("recordId") && dynamicModel.ExtraData["recordId"] != null)
                    {
                        SendMessage(new Guid(dynamicModel.ExtraData["recordId"].ToString()), userNumber, new Guid(dynamicModel.ExtraData["entityId"].ToString()), dynamicModel.ExtraData["funccode"].ToString());
                    }
                    else WriteEntityAddMessage(dynamicModel.TypeId, bussinessId, relbussinessId, userNumber, 1);
                }

            }
            return res;
        }

        public OutputResult<object> AddEntityData(DbTransaction transaction, UserData userData, SimpleEntityInfo entityInfo, DynamicEntityAddModel dynamicModel, AnalyseHeader header, int userNumber, out WorkFlowAddCaseModel workFlowAddCaseModel)
        {
            workFlowAddCaseModel = null;
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

            if (entityInfo.ModelType == EntityModelType.Dynamic && dynamicModel.FieldData.ContainsKey(entityInfo.RelFieldName))
            {
                if (!dynamicModel.FieldData.ContainsKey("recrelateid"))
                {
                    dynamicModel.FieldData.Add("recrelateid", dynamicModel.FieldData[entityInfo.RelFieldName]);
                    dynamicModel.FieldData.Remove(entityInfo.RelFieldName);
                }
            }
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


            if (entityInfo != null)
            {
                bool success = false;
                Guid bussinessId;

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
                    if (!IsValidEntityPower(entityInfo) && !dynamicEntity.FlowId.HasValue && !userData.HasDataAccess(transaction, RoutePath, entityInfo.EntityId, DeviceClassic, new List<Guid>() { bussinessId }))
                    {
                        throw new Exception("您没有权限新增该实体数据");
                    }


                    CheckCallBackService(transaction, OperatType.Insert, entityInfo.Servicesjson, bussinessId, entityInfo.EntityId, userNumber, "");

                    if (dynamicEntity.FlowId.HasValue)
                    {
                        //新增流程数据
                        workFlowAddCaseModel = new WorkFlowAddCaseModel()
                        {
                            EntityId = entityInfo.EntityId,
                            FlowId = dynamicEntity.FlowId.Value,
                            RecId = bussinessId,
                            RelEntityId = dynamicEntity.RelEntityId,
                            RelRecId = dynamicEntity.RelRecId,
                            CaseData = data
                        };
                    }
                }

                return HandleResult(entityResult);
            }
            else
            {
                throw new Exception("获取实体Id失败");
            }

        }



        public OutputResult<object> CalcMenuDataCount(Guid entityId, int userId)
        {
            List<EntityMenuInfo> menus = this._entityProRepository.GetEntityMenuInfoList(entityId);
            Dictionary<string, string> retData = new Dictionary<string, string>();
            foreach (EntityMenuInfo menu in menus)
            {
                if (menu.RecStatus != 1) continue;
                DynamicEntityListModel dynamicModel = new DynamicEntityListModel()
                {
                    EntityId = entityId,
                    MenuId = menu.MenuId.ToString(),
                    ViewType = 4,
                    SearchData = new Dictionary<string, object>(),
                    ExtraData = new Dictionary<string, object>(),
                    SearchDataXOR = new Dictionary<string, object>(),
                    SearchOrder = "",
                    PageIndex = 1,
                    PageSize = 1,
                    IsAdvanceQuery = 0,
                    NeedPower = 1

                };
                try
                {
                    OutputResult<object> tmp = this.DataList2(dynamicModel, false, userId, true);
                    if (tmp != null && tmp.DataBody != null)
                    {
                        Dictionary<string, List<Dictionary<string, object>>> tmpDict = (Dictionary<string, List<Dictionary<string, object>>>)tmp.DataBody;
                        if (tmpDict != null && tmpDict.ContainsKey("PageCount") && tmpDict["PageCount"] != null)
                        {
                            Dictionary<string, object> pageDict = tmpDict["PageCount"].FirstOrDefault();
                            if (pageDict.ContainsKey("total") && pageDict["total"] != null)
                            {
                                long ltmp = 0;
                                if (long.TryParse(pageDict["total"].ToString(), out ltmp))
                                {
                                    retData.Add(menu.MenuId.ToString(), ltmp.ToString());
                                }
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    //忽略所有错误
                }


            }
            return new OutputResult<object>(retData);
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
        private void WriteEntityAddMessage(Guid typeId, Guid bussinessId, Guid relbussinessId, int userNumber, int isFlow = 0)
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
                    detail = _dynamicEntityRepository.Detail(detailMapper, userNumber);

                    if (entityInfotemp.RelEntityId.HasValue && entityInfotemp.RelFieldId.HasValue && !string.IsNullOrEmpty(entityInfotemp.RelFieldName))
                    {
                        if (detail["recrelateid"] == null)
                            throw new Exception("动态实体缺少关联对象信息");
                        var relDetail = _dynamicEntityRepository.Detail(new DynamicEntityDetailtMapper
                        {
                            EntityId = entityInfotemp.RelEntityId.Value,
                            NeedPower = 1,
                            RecId = Guid.Parse(detail["recrelateid"].ToString())
                        }, userNumber);
                        var relEntityField = _dynamicEntityRepository.GetEntityFields(entityInfotemp.RelEntityId.Value, userNumber).FirstOrDefault(t => t.FieldId == entityInfotemp.RelFieldId.Value);
                        JObject jo = JObject.Parse(msgpParam);
                        StringBuilder sb = new StringBuilder();
                        JObject newJo = new JObject();
                        foreach (var tmp in jo)
                        {
                            if (jo[tmp.Key] != null && tmp.Key == "recrelateid")
                            {
                                object obj = "";
                                if (relDetail.ContainsKey(relEntityField.FieldName))
                                {
                                    if (relDetail[relEntityField.FieldName] != null)
                                        obj = relDetail[relEntityField.FieldName];
                                }
                                newJo.Add(entityInfotemp.RelFieldName, JToken.FromObject(obj));
                            }
                            else
                            {
                                newJo.Add(tmp.Key, tmp.Value);
                            }
                        }
                        msgpParam = newJo.ToString();
                    }
                    newMembers = MessageService.GetEntityMember(detail as Dictionary<string, object>);
                    #region 把主实体的人员信息
                    detailMapper.EntityId = entityInfotemp.RelEntityId.Value;
                    detailMapper.RecId = relbussinessId;
                    IDictionary<string, object> Rel_detail = _dynamicEntityRepository.Detail(detailMapper, userNumber);
                    EntityMemberModel MainMembers = MessageService.GetEntityMember(Rel_detail as Dictionary<string, object>);
                    if (MainMembers != null)
                    {
                        if (MainMembers.CopyUsers != null)
                            newMembers.CopyUsers.AddRange(MainMembers.CopyUsers);
                        if (MainMembers.FollowUsers != null)
                            newMembers.FollowUsers.AddRange(MainMembers.FollowUsers);
                        if (MainMembers.Members != null)
                            newMembers.Members.AddRange(MainMembers.Members);
                        if (MainMembers.ViewUsers != null)
                            newMembers.ViewUsers.AddRange(MainMembers.ViewUsers);
                        if (MainMembers.RecManager > 0)
                            newMembers.RecManager = MainMembers.RecManager;
                    }
                    #endregion
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
                    DateTime reportdate;
                    if (detail.ContainsKey("reportdate"))
                    {
                        reportdate = DateTime.Parse(detail["reportdate"].ToString());
                    }
                    else
                    {
                        reportdate = DateTime.Parse(detail["reccreated"].ToString());
                    }
                    var msg = MessageService.GetDailyMsgParameter(reportdate, entityInfotemp, bussinessId, relbussinessId, funccode, userNumber, newMembers, null, msgpParam);
                    msg.ApprovalUsers = msg.Receivers[MessageUserType.DailyApprover];
                    msg.CopyUsers = msg.Receivers[MessageUserType.DailyCarbonCopyUser];
                    MessageService.WriteMessage(null, msg, userNumber, isFlow: 1);
                }

                else
                {
                    //编辑操作的消息
                    var addEntityMsg = MessageService.GetEntityMsgParameter(entityInfotemp, bussinessId, relbussinessId, funccode, userNumber, newMembers, null, msgpParam);
                    MessageService.WriteMessageAsyn(addEntityMsg, userNumber, isFlow: isFlow);
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

        public OutputResult<object> QueryValueForRelTabAddNew(RelTabQueryDataSourceModel paramInfo, int userId)
        {
            dynamic tmp = this._entityProRepository.GetFieldInfo(paramInfo.FieldId, userId);
            string dstEntityid = "";
            Dictionary<string, object> fieldInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(tmp));
            if (fieldInfo == null)
            {
                throw (new Exception("字段信息不存在"));
            }
            int ControlType = int.Parse(fieldInfo["controltype"].ToString());
            if (ControlType != 18)
            {
                throw (new Exception("目标字段必须是数据源字段"));
            }

            Dictionary<string, object> fieldConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>((string)fieldInfo["fieldconfig"]);
            Dictionary<string, object> DataSource = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(fieldConfig["dataSource"]));

            string sourceId = DataSource["sourceId"].ToString();
            dynamic tmpsourceInfo = this._dataSourceRepository.GetDataSourceInfo(Guid.Parse(sourceId), userId);
            Dictionary<string, object> sourceInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(tmpsourceInfo));

            string recid = paramInfo.RecId.ToString();
            dstEntityid = sourceInfo["entityid"].ToString();
            if (!dstEntityid.Equals(paramInfo.EntityId.ToString()))
            {
                //这种情况主要是客户和客户基础资料的关系
                if (dstEntityid.Equals("ac051b46-7a20-4848-9072-3b108f1de9b0"))
                {
                    //这种情况要重新获取recid
                    string relrecid = this._customerRepository.getCommonIdByCustId(null, recid, userId);
                    if (relrecid != null) recid = relrecid;
                }
            }

            IDictionary<string, object> result = this._dataSourceRepository.DynamicDataSrcQueryDetail(sourceId, Guid.Parse(recid), userId);

            return new OutputResult<object>(result);
        }

        public void SavePersonalWebListColumnsSetting(SaveWebListColumnsForPersonalParamInfo paramInfo, int userId)
        {
            DbTransaction tran = null;
            Dictionary<string, object> detail = this._dynamicEntityRepository.GetPersonalWebListColumnsSetting(paramInfo.EntityId, userId, tran);
            if (detail == null || detail.ContainsKey("recid") == false || detail["recid"] == null)
            {
                this._dynamicEntityRepository.AddPersonalWebListColumnsSetting(paramInfo.EntityId, paramInfo.ViewConfig, userId, tran);
            }
            else
            {
                this._dynamicEntityRepository.UpdatePersonalWebListColumnsSetting(Guid.Parse(detail["recid"].ToString()), paramInfo.ViewConfig, userId, tran);

            }
        }

        /// <summary>
        /// 获取实体的web列表字段的个人设置
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public WebListPersonalViewSettingInfo GetPersonalWebListColumnsSettting(WebListColumnsForPersonalParamInfo paramInfo, int userId)
        {

            WebListPersonalViewSettingInfo view = null;
            DbTransaction tran = null;
            Dictionary<string, object> detail = this._dynamicEntityRepository.GetPersonalWebListColumnsSetting(paramInfo.EntityId, userId, tran);
            if (detail == null || detail.ContainsKey("viewconfig") == false || detail["viewconfig"] == null)
            {
                view = new WebListPersonalViewSettingInfo();
                view.FixedColumnCount = 0;
                view.Columns = new List<WebListPersonalViewColumnSettingInfo>();
                return view;
            }
            try
            {
                view = Newtonsoft.Json.JsonConvert.DeserializeObject<WebListPersonalViewSettingInfo>(detail["viewconfig"].ToString());
                return view;
            }
            catch (Exception ex)
            {
                view = new WebListPersonalViewSettingInfo();
                view.FixedColumnCount = 0;
                view.Columns = new List<WebListPersonalViewColumnSettingInfo>();
                return view;
            }
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
            //if (GetUserData(userId, false).HasFunction(this.RoutePath, paramInfo.EntityId, this.DeviceClassic) == false) {
            //    throw (new Exception("权限项未配置或者您没有权限执行此方法"));
            //}
            #endregion
            #region  暂不检查数据权限
            #endregion 
            Dictionary<string, object> entityInfo = this._dynamicEntityRepository.getEntityBaseInfoById(paramInfo.EntityId ?? Guid.Empty, userId);
            if (entityInfo == null || entityInfo.Count == 0) throw (new Exception("实体信息异常"));
            //
            EntityExtFunctionInfo extFuncInfo = this._dynamicEntityRepository.getExtFunctionByFunctionName(paramInfo.EntityId ?? Guid.Empty, functionname);
            if (extFuncInfo == null) throw (new Exception("实体扩展定义异常"));
            if (extFuncInfo.RecStatus != 1)
            {
                throw (new Exception("该扩展已经被停用了"));
            }
            try
            {

                object retObj = null;
                if (extFuncInfo.EngineType == EngineTypeEnum.SQLEngine)
                {
                    retObj = this._dynamicEntityRepository.ExecuteExtFunction(extFuncInfo, paramInfo.RecIds, paramInfo.OtherParams, userId);
                }
                else
                {
                    retObj = ExecuteUScript(extFuncInfo, paramInfo.RecIds, paramInfo.OtherParams, userId);
                }
                return new OutputResult<object>(retObj);
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    return new OutputResult<object>("", ex.InnerException.Message, -1);
                }
                else
                {
                    return new OutputResult<object>("", ex.Message, -1);
                }
            }
        }

        private object ExecuteUScript(EntityExtFunctionInfo extFuncInfo, string[] recIds, Dictionary<string, object> otherParams, int userId)
        {
            try
            {
                if (extFuncInfo.UScript == null || extFuncInfo.UScript.Length == 0)
                {
                    throw (new Exception("脚本定义失败"));
                }
                UKJSEngineUtils utils = new UKJSEngineUtils(this._javaScriptUtilsServices);
                Dictionary<string, object> param = new Dictionary<string, object>();
                param.Add("recids", recIds);
                param.Add("OtherParams", otherParams);
                utils.SetHostedObject("JsParam", param);
                long tick = System.DateTime.Now.Ticks;
                string jscode = "function ukejsengin_func_entity_" + tick + "(){" + extFuncInfo.UScript + @"};
                               ukejsengin_func_entity_" + tick + "();";
                return utils.Evaluate(jscode);
            }
            catch (Exception ex)
            {
                _logger.Error("执行引擎失败：" + ex.Message);
                throw (ex);
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
                        var detailList = _dynamicEntityRepository.DetailList(detailMapper, userNumber, null);
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
                        var detailList = _dynamicEntityRepository.DetailList(detailMapper, userNumber, null);
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
        public OutputResult<object> GetRelConfigEntity(RelTabListModel dynamicModel, int userNumber)
        {
            var dynamicEntity = _mapper.Map<RelTabListModel, RelTabListMapper>(dynamicModel);

            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }
            var result = _dynamicEntityRepository.GetRelConfigEntity(dynamicEntity, userNumber);
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


        public OutputResult<object> GetRelConfigFields(GetEntityFieldsModel entity, int userNumber)
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
            var result = _dynamicEntityRepository.GetRelConfigFields(dynamicEntity, userNumber);
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
            #region 需要对字段进行整理，并对数据源进行整理，增加补充entityid
            DataSourceListMapper dsListMapper = new DataSourceListMapper()
            {
                PageIndex = 1,
                DatasourceName = "",
                PageSize = 1000,
                RecStatus = 1
            };
            Dictionary<string, List<IDictionary<string, object>>> dataSourceListDict = this._dataSourceRepository.SelectDataSource(dsListMapper, userNumber);
            List<IDictionary<string, object>> allDataSourceList = dataSourceListDict["PageData"];
            Dictionary<string, IDictionary<string, object>> allDataSourceDict = new Dictionary<string, IDictionary<string, object>>();
            foreach (IDictionary<string, object> item in allDataSourceList)
            {
                string id = item["datasourceid"].ToString();
                allDataSourceDict[id] = item;
            }
            DynamicEntityDataFieldMapper tmpField = null;
            foreach (DynamicEntityDataFieldMapper field in fields)
            {
                if (field.ControlType == 18)//数据源控件才处理
                {
                    try
                    {
                        DynamicProtocolFieldConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicProtocolFieldConfig>(field.FieldConfig);
                        if (config.DataSource.SourceId != null && config.DataSource.SourceId.Length > 0)
                        {
                            if (allDataSourceDict.ContainsKey(config.DataSource.SourceId))
                            {
                                IDictionary<string, object> item = allDataSourceDict[config.DataSource.SourceId];
                                if (item.ContainsKey("entityid") && item["entityid"] != null)
                                {
                                    config.DataSource.EntityId = Guid.Parse(item["entityid"].ToString());
                                    JObject j1 = JObject.Parse(JsonConvert.SerializeObject(config));
                                    JObject j2 = JObject.Parse(field.FieldConfig);
                                    j1.Merge(j2);
                                    field.FieldConfig = j1.ToString();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else if (field.ControlType == 30)
                {
                    if (field.FieldName == "recrelateid")
                    {
                        tmpField = field;
                        tmpField.FieldName = _entityProRepository.GetEntityInfo(dynamicModel.TypeId).RelFieldName;
                    }
                }
            }
            #endregion 
            if (fields.Count == 0)
            {
                return ShowError<object>("该实体分类没有配置相应字段");
            }
            //if (tmpField != null)
            //    fields.Add(tmpField);
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
            #region 需要对字段进行整理，并对数据源进行整理，增加补充entityid
            DataSourceListMapper dsListMapper = new DataSourceListMapper()
            {
                PageIndex = 1,
                DatasourceName = "",
                PageSize = 1000,
                RecStatus = 1
            };
            Dictionary<string, List<IDictionary<string, object>>> dataSourceListDict = this._dataSourceRepository.SelectDataSource(dsListMapper, userNumber);
            List<IDictionary<string, object>> allDataSourceList = dataSourceListDict["PageData"];
            Dictionary<string, IDictionary<string, object>> allDataSourceDict = new Dictionary<string, IDictionary<string, object>>();
            foreach (IDictionary<string, object> item in allDataSourceList)
            {
                string id = item["datasourceid"].ToString();
                allDataSourceDict[id] = item;
            }
            foreach (DynamicEntityDataFieldMapper field in fields)
            {
                if (field.ControlType == 18)//数据源控件才处理
                {
                    try
                    {
                        DynamicProtocolFieldConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicProtocolFieldConfig>(field.FieldConfig);
                        if (config.DataSource.SourceId != null && config.DataSource.SourceId.Length > 0)
                        {
                            if (allDataSourceDict.ContainsKey(config.DataSource.SourceId))
                            {
                                IDictionary<string, object> item = allDataSourceDict[config.DataSource.SourceId];
                                if (item.ContainsKey("entityid") && item["entityid"] != null)
                                {
                                    config.DataSource.EntityId = Guid.Parse(item["entityid"].ToString());
                                    config.IsReadOnly = field.IsReadOnly ? 1 : 0;
                                    config.IsRequired = field.IsRequire ? 1 : 0;
                                    config.IsVisible = field.IsVisible ? 1 : 0;
                                    field.FieldConfig = JsonConvert.SerializeObject(config);

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            #endregion 
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
            #region 需要对字段进行整理，并对数据源进行整理，增加补充entityid
            DataSourceListMapper dsListMapper = new DataSourceListMapper()
            {
                PageIndex = 1,
                DatasourceName = "",
                PageSize = 1000,
                RecStatus = 1
            };
            Dictionary<string, List<IDictionary<string, object>>> dataSourceListDict = this._dataSourceRepository.SelectDataSource(dsListMapper, userNumber);
            List<IDictionary<string, object>> allDataSourceList = dataSourceListDict["PageData"];
            Dictionary<string, IDictionary<string, object>> allDataSourceDict = new Dictionary<string, IDictionary<string, object>>();
            foreach (IDictionary<string, object> item in allDataSourceList)
            {
                string id = item["datasourceid"].ToString();
                allDataSourceDict[id] = item;
            }
            foreach (DynamicEntityWebFieldMapper field in fields)
            {
                if (field.ControlType == 18)//数据源控件才处理
                {
                    try
                    {
                        DynamicProtocolFieldConfig config = Newtonsoft.Json.JsonConvert.DeserializeObject<DynamicProtocolFieldConfig>(field.FieldConfig);
                        if (config.DataSource.SourceId != null && config.DataSource.SourceId.Length > 0)
                        {
                            if (allDataSourceDict.ContainsKey(config.DataSource.SourceId))
                            {
                                IDictionary<string, object> item = allDataSourceDict[config.DataSource.SourceId];
                                if (item.ContainsKey("entityid") && item["entityid"] != null)
                                {
                                    config.DataSource.EntityId = Guid.Parse(item["entityid"].ToString());
                                    field.FieldConfig = Newtonsoft.Json.JsonConvert.SerializeObject(config);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            #endregion 
            if (fields.Count == 0)
            {
                return ShowError<object>("该实体没有配置列表字段");
            }

            return new OutputResult<object>(fields);
        }
        public OutputResult<object> DynamicListViewColumns(DynamicEntityGeneralModel dynamicModel, int userNumber)
        {
            //获取该实体分类的字段
            var fields = GetWebDynamicListFields(dynamicModel.TypeId, (DynamicProtocolOperateType)dynamicModel.OperateType, userNumber);

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

            //必填字段 修改记录
            var requireResults = DynamicProtocolHelper.ValidRequireData(validResults);

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
								if (detail.ContainsKey("viewusers") && data.ContainsKey("viewusers"))
								{
									detail["viewusers"] = data["viewusers"];
								}
                                msgparams = GetEditMessageParameter(oldDetail, detail, typeid, bussinessId, relbussinessId, userNumber);
                            }
                            //修改记录，并发动态
                            string modifyContent = ModifyRecord(oldDetail, requireResults, entityid, bussinessId, userNumber);
                            foreach (var msg in msgparams)
                            {
                                msg.TemplateKeyValue.Add("modifycontent", modifyContent);
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

        private Dictionary<String, object> FormatMenuParamSql(String menuSql, DynamicEntityListMapper dynamicEntity)
        {
            var param = new Dictionary<String, object>();
            foreach (var tmp in dynamicEntity.SearchData)
            {
                if (menuSql.IndexOf("@" + tmp.Key) > 0)
                {
                    param.Add(tmp.Key, tmp.Value);
                }
            }
            return param;
        }
        public OutputResult<object> CommonDataList(DynamicEntityListMapper dynamicEntity, PageParam pageParam, bool isAdvanceQuery, int userNumber, bool CalcCountOnly = false, bool isColumnFilter = false)
        {
            DbTransaction tran = null;



            #region 检查并处理有特殊列表函数的情况
            string SpecFuncName = _dynamicEntityRepository.CheckDataListSpecFunction(dynamicEntity.EntityId);
            if (!string.IsNullOrEmpty(SpecFuncName))
            {
                var innerResult = _dynamicEntityRepository.DataListUseFunc(SpecFuncName, pageParam, dynamicEntity.ExtraData, dynamicEntity, userNumber);
                return new OutputResult<object>(JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, object>>>>(JsonConvert.SerializeObject(innerResult)));
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
            var param = new Dictionary<String, object>();
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
                    MainTable = MainTable + " " + dynamicEntity.RelSql;
                }
                else
                {
                    MainTable = string.Format("select * from {0} where 1=1 {1}", (string)EntityInfo["entitytable"], dynamicEntity.RelSql);
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

                        //彭小锋
                        param = FormatMenuParamSql(MainTable, dynamicEntity);
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
                        if (fieldInfo.FieldConfig == null)
                        {
                            string tmpDataSourceId = fieldNameMapDataSource[fieldInfo.FieldName];
                            string tablename = dataSources[tmpDataSourceId];
                            fromClause = string.Format(@"{0} left outer join {1} as {2}_t on (e.{2}->>'id')::uuid = {2}_t.recid ", fromClause, tablename, fieldInfo.FieldName);
                            selectClause = string.Format(@"{0},{1}_t.recname as {1}_name", selectClause, fieldInfo.FieldName);
                        }
                        else
                        {
                            Dictionary<string, object> fieldConfigDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(fieldInfo.FieldConfig);
                            bool SaveNamed = false;
                            bool IsMulti = false;
                            if (fieldConfigDict.ContainsKey("dataSource")
                                && fieldConfigDict["dataSource"] != null)
                            {
                                Dictionary<string, object> datasourceInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                Newtonsoft.Json.JsonConvert.SerializeObject(fieldConfigDict["dataSource"]));
                                if (datasourceInfo != null && datasourceInfo.ContainsKey("namefrom") && datasourceInfo["namefrom"] != null)
                                {
                                    string namefrom = datasourceInfo["namefrom"].ToString();
                                    if (namefrom.Equals("1"))
                                    {
                                        SaveNamed = true;
                                    }
                                }
                            }
                            if (fieldConfigDict.ContainsKey("multiple") && fieldConfigDict["multiple"] != null)
                            {
                                int tmp = 0;
                                int.TryParse(fieldConfigDict["multiple"].ToString(), out tmp);
                                if (tmp != 0) IsMulti = true;
                            }
                            if (SaveNamed)
                            {
                                selectClause = string.Format(@"{0},jsonb_extract_path_text(e.{1},'name') as {1}_name", selectClause, fieldInfo.FieldName);
                            }
                            else
                            {
                                if (IsMulti)
                                {
                                    selectClause = string.Format(@"{0},jsonb_extract_path_text(e.{1},'name') as {1}_name", selectClause, fieldInfo.FieldName);
                                }
                                else
                                {
                                    string tmpDataSourceId = fieldNameMapDataSource[fieldInfo.FieldName];
                                    string tablename = dataSources[tmpDataSourceId];
                                    fromClause = string.Format(@"{0} left outer join {1} as {2}_t on (e.{2}->>'id')::uuid = {2}_t.recid ", fromClause, tablename, fieldInfo.FieldName);
                                    selectClause = string.Format(@"{0},{1}_t.recname as {1}_name", selectClause, fieldInfo.FieldName);
                                }
                            }
                        }
                        break;
                    case EntityFieldControlType.RecType://1009记录类型
                        fromClause = string.Format(@"{0} left outer join crm_sys_entity_category  as {1}_t on e.{1} = {1}_t.categoryid ", fromClause, fieldInfo.FieldName);
                        selectClause = string.Format(@"{0},{1}_t.categoryname as {1}_name", selectClause, fieldInfo.FieldName);
                        break;
                    case EntityFieldControlType.SelectMulti://4本地字典多选
                        selectClause = string.Format(@"{0},crm_func_entity_protocol_format_dictionary({2},e.{1}) {1}_name, crm_func_entity_protocol_format_dictionary_lang({2},e.{1})::jsonb {1}_lang", selectClause, fieldInfo.FieldName, fieldNameDictType[fieldInfo.FieldName]);
                        break;
                    case EntityFieldControlType.SelectSingle://3本地字典单选
                        fromClause = string.Format(@"{0} left outer join crm_sys_dictionary  as {1}_t on e.{1} = {1}_t.dataid and {1}_t.dictypeid={2} ", fromClause, fieldInfo.FieldName, fieldNameDictType[fieldInfo.FieldName]);
                        selectClause = string.Format(@"{0},{1}_t.dataval as {1}_name, {1}_t.dataval_lang as {1}_lang", selectClause, fieldInfo.FieldName);
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
                    case EntityFieldControlType.RelateControl://专供关联对象用
                        JObject jo = JObject.Parse(fieldInfo.FieldConfig);
                        if (jo["relentityid"] == null || jo["relfieldid"] == null || EntityInfo["relfieldname"] == null)
                            throw new Exception("关联对象信息配置异常");

                        var relEntityInfo = _entityProRepository.GetEntityInfo(Guid.Parse(jo["relentityid"].ToString()));
                        var relField = _entityProRepository.GetFieldInfo(Guid.Parse(jo["relfieldid"].ToString()), userNumber);
                        var selectField = DynamicProtocolHelper.tryParseFieldSearchString(new DynamicEntityFieldSearch
                        {
                            FieldId = relField.fieldid,
                            FieldName = relField.fieldname,
                            ControlType = relField.controltype,
                            FieldConfig = relField.fieldconfig
                        }, relEntityInfo.EntityTable + "_t");
                        var entityTable = relEntityInfo.EntityTable;
                        fromClause = string.Format(@"{0} left outer join {1} as {1}_t on {1}_t.recid = e.recrelateid ", fromClause, entityTable);
                        selectClause = string.Format(@"{0},{4} as {3}_name,{1} as {2}", selectClause, selectField, EntityInfo["relfieldname"].ToString(), EntityInfo["relfieldname"].ToString(), selectField);
                        break;
                }
            }
            #endregion
            string WhereSQL = "1=1";
            string OrderBySQL = " e.recversion desc ";
            if (dynamicEntity.SearchOrder != null && dynamicEntity.SearchOrder.Length > 0)
            {
                OrderBySQL = dynamicEntity.SearchOrder;
                string tmp = GenerateOrderBySQL(dynamicEntity.SearchOrder, fieldList);
                if (tmp != null && tmp.Length > 0)
                {
                    OrderBySQL = tmp;
                }
            }
            //if (isAdvanceQuery || isColumnFilter)
            //{
            if (dynamicEntity.SearchQuery != null && dynamicEntity.SearchQuery.Length > 0)
            {
                WhereSQL = " e.recstatus = 1   " + dynamicEntity.SearchQuery;
            }
            else
            {
                if (!string.IsNullOrEmpty(dynamicEntity.SearchQuery))
                    WhereSQL = " e.recstatus = 1    " + dynamicEntity.SearchQuery;
                else
                    WhereSQL = " e.recstatus = 1   ";
            }

            //}
            //else
            //    WhereSQL = " e.recstatus = 1   ";
            /*通用列表不要随便改了 改之前可以先问一下辉哥或者小锋， 这条东西加上去逻辑是不对 临时解决方案，完全方案可以看7.3.3_dingding分支(╯﹏╰)*/
            //if (dynamicEntity.SearchData != null &&  dynamicEntity.SearchData.Count() > 0)
            // WhereSQL += " and e." + dynamicEntity.SearchData.Keys.FirstOrDefault() + "  like '%" + dynamicEntity.SearchData.Values.FirstOrDefault() + "%'";
            string innerSQL = string.Format(@"select {0} from {1}  where  {2} order by {3} limit {4} offset {5}",
                selectClause, fromClause, WhereSQL, OrderBySQL, pageParam.PageSize, (pageParam.PageIndex - 1) * pageParam.PageSize);
            string strSQL = string.Format(@"Select {0} from ({1}) as outersql", outerSelectClause, innerSQL);
            string CountSQL = string.Format(@"select total,(total-1)/{2}+1 as page from (select count(*)  AS total  from {0} where  {1} ) as k", fromClause, WhereSQL, pageParam.PageSize);
            List<Dictionary<string, object>> datas = null;

            if (CalcCountOnly)
                datas = new List<Dictionary<string, object>>();
            else
            {
                if (param.Count > 0)
                {
                    DbParameter[] dbParmas = new DbParameter[param.Count];
                    int i = 0;
                    foreach (var tmp in param)
                    {
                        dbParmas[i] = new NpgsqlParameter(tmp.Key, tmp.Value);
                        i++;
                    }
                    datas = this._dynamicEntityRepository.ExecuteQuery(strSQL, dbParmas, tran);
                }
                else
                    datas = this._dynamicEntityRepository.ExecuteQuery(strSQL, tran);
            }
            List<Dictionary<string, object>> page = new List<Dictionary<string, object>>();
            if (param.Count > 0)
            {
                DbParameter[] dbParmas = new DbParameter[param.Count];
                int i = 0;
                foreach (var tmp in param)
                {
                    dbParmas[i] = new NpgsqlParameter(tmp.Key, tmp.Value);
                    i++;
                }
                page = this._dynamicEntityRepository.ExecuteQuery(CountSQL, dbParmas, tran);
            }
            else
                page = this._dynamicEntityRepository.ExecuteQuery(CountSQL, tran);

            Dictionary<string, List<Dictionary<string, object>>> retData = new Dictionary<string, List<Dictionary<string, object>>>();

            #region 数字类型整理,日期整理
            foreach (DynamicEntityFieldSearch fieldInfo in fieldList)
            {
                if (fieldInfo.ControlType == (int)EntityFieldControlType.NumberInt
                    || fieldInfo.ControlType == (int)EntityFieldControlType.NumberDecimal)
                {
                    DynamicProtocolHelper.FormatNumericFieldInList(datas, fieldInfo);
                }
                else if (fieldInfo.ControlType == (int)EntityFieldControlType.RecCreated
                   || fieldInfo.ControlType == (int)EntityFieldControlType.RecUpdated
                   || fieldInfo.ControlType == (int)EntityFieldControlType.TimeDate
                   || fieldInfo.ControlType == (int)EntityFieldControlType.TimeStamp)
                {

                }
            }
            #endregion
            retData.Add("PageData", datas);
            retData.Add("PageCount", page);

            return new OutputResult<object>(retData);
        }
        /// <summary>
        /// 处理orderby 的方法
        /// </summary>
        /// <param name="orderby"></param>
        /// <param name="fieldList"></param>
        /// <returns></returns>
        private string GenerateOrderBySQL(string orderby, List<DynamicEntityFieldSearch> fieldList)
        {
            string totalReturn = "";
            if (orderby == null || orderby.Length == 0)
            {
                return null;
            }
            string[] orders = orderby.Trim().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (orders == null || orders.Length == 0) return null;
            foreach (string orderitem in orders)
            {
                string curItemOrderBySQL = "";
                string tmpItem = orderitem.Trim();
                string[] orderStruct = tmpItem.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string orderFieldName = "";
                string orderType = "ASC";
                bool isNameField = false;
                if (orderStruct.Length == 1)
                {
                    orderFieldName = orderStruct[0];
                }
                else if (orderStruct.Length == 2)
                {
                    orderFieldName = orderStruct[0];
                    orderType = orderStruct[1].ToUpper();
                    if (orderType.Equals("DSC")) orderType = "DESC";
                }
                if (orderFieldName.EndsWith("_name"))
                {
                    isNameField = true;
                    orderFieldName = orderFieldName.Substring(0, orderFieldName.Length - "_name".Length);
                }
                DynamicEntityFieldSearch orderFieldInfo = null;
                foreach (DynamicEntityFieldSearch fieldInfo in fieldList)
                {
                    if (fieldInfo.FieldName.Equals(orderFieldName))
                    {
                        orderFieldInfo = fieldInfo;
                        break;
                    }
                }
                if (orderFieldInfo != null)
                {
                    EntityFieldControlType controlType = (EntityFieldControlType)orderFieldInfo.ControlType;
                    switch (controlType)
                    {
                        case EntityFieldControlType.Address://31,地址
                        case EntityFieldControlType.Location://14定位
                            curItemOrderBySQL = "e." + orderFieldInfo.FieldName + "->>'address' collate \"zh_CN\" " + orderType;
                            break;
                        case EntityFieldControlType.RecCreator://1002创建人
                        case EntityFieldControlType.RecUpdator://1003更新人
                        case EntityFieldControlType.RecManager://1006负责人
                            curItemOrderBySQL = string.Format("{0}_t.username collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.AreaGroup://20分组，不处理
                            break;
                        case EntityFieldControlType.AreaRegion://16 行政区域
                            curItemOrderBySQL = string.Format("{0}_t.fullname collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.DataSourceMulti://19数据源多选
                                                                    //目前这个没有使用
                            break;
                        case EntityFieldControlType.DataSourceSingle://18数据源单选
                            if (orderFieldInfo.FieldConfig == null)
                            {
                                curItemOrderBySQL = string.Format("{0}_t.recname collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            }
                            else
                            {
                                Dictionary<string, object> fieldConfigDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(orderFieldInfo.FieldConfig);
                                bool SaveNamed = false;
                                if (fieldConfigDict.ContainsKey("dataSource")
                                    && fieldConfigDict["dataSource"] != null)
                                {
                                    Dictionary<string, object> datasourceInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
                                    Newtonsoft.Json.JsonConvert.SerializeObject(fieldConfigDict["dataSource"]));
                                    if (datasourceInfo != null && datasourceInfo.ContainsKey("namefrom") && datasourceInfo["namefrom"] != null)
                                    {
                                        string namefrom = datasourceInfo["namefrom"].ToString();
                                        if (namefrom.Equals("1"))
                                        {
                                            SaveNamed = true;
                                        }
                                    }
                                }
                                if (SaveNamed)
                                {
                                    curItemOrderBySQL = string.Format("jsonb_extract_path_text(e.{0},'name') collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                                }
                                else
                                {
                                    curItemOrderBySQL = string.Format("{0}_t.recname collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                                }
                            }
                            break;
                        case EntityFieldControlType.RecType://1009记录类型
                            curItemOrderBySQL = string.Format("{0}_t.categoryname collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.SelectMulti://4本地字典多选
                            break;
                        case EntityFieldControlType.SelectSingle://3本地字典单选
                            curItemOrderBySQL = string.Format("{0}_t.dataval collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.SalesStage://销售阶段
                            curItemOrderBySQL = string.Format("{0}_t.stagename collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;

                        case EntityFieldControlType.RecCreated://1004创建时间
                        case EntityFieldControlType.RecUpdated://
                            curItemOrderBySQL = string.Format("TO_CHAR(e.{0},'YYYY-MM-DD HH24:MI:SS')  {1}", orderFieldInfo.FieldName, orderType);
                            break;

                        case EntityFieldControlType.TimeDate://8
                            curItemOrderBySQL = string.Format("TO_CHAR(e.{0},'YYYY-MM-DD')  {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.TimeStamp://9
                            curItemOrderBySQL = string.Format("TO_CHAR(e.{0},'YYYY-MM-DD HH24:MI:SS')  {1}", orderFieldInfo.FieldName, orderType);
                            break;

                        case EntityFieldControlType.QuoteControl://31引用(分三种情况，)
                            if (orderFieldInfo.FieldName.ToUpper().Equals("predeptgroup".ToUpper()))
                            {
                                curItemOrderBySQL = string.Format("{0}_t.deptname collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            }
                            else if (orderFieldInfo.FieldName.ToUpper().Equals("deptgroup".ToUpper()))
                            {
                                curItemOrderBySQL = string.Format("{0}_t.deptname collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            }
                            else
                            {
                                curItemOrderBySQL = string.Format("crm_func_entity_protocol_format_quote_control(row_to_json(outersql)::json,'{0}','{1}') collate \"zh_CN\" {2}", orderFieldInfo.FieldId, orderFieldInfo.FieldName, orderType);
                            }
                            break;
                        case EntityFieldControlType.Department://17部门
                            curItemOrderBySQL = string.Format("crm_func_entity_protocol_format_dept_multi(outersql.{0}::text)  collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            //outerSelectClause = string.Format(@"{0},crm_func_entity_protocol_format_dept_multi(outersql.{1}::text) as {1}_name", outerSelectClause, fieldInfo.FieldName);
                            break;
                        case EntityFieldControlType.EmailAddr://11email地址
                            curItemOrderBySQL = string.Format("e.{0} collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.FileAttach://23 文件地址
                            //此字段不能排序
                            break;
                        case EntityFieldControlType.HeadPhoto://15 头像
                            //此字段不能排序
                            break;
                        case EntityFieldControlType.LinkeTable://24关联表格
                            //此字段不能排序
                            break;
                        case EntityFieldControlType.NumberDecimal:
                        case EntityFieldControlType.NumberInt:
                        case EntityFieldControlType.PhoneNum://
                            curItemOrderBySQL = string.Format("e.{0} {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.PersonSelectMulti://26人员多选
                            curItemOrderBySQL = string.Format("crm_func_entity_protocol_format_userinfo_multi(e.{0}) collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.PersonSelectSingle://25人员单选
                            curItemOrderBySQL = string.Format("crm_func_entity_protocol_format_userinfo_multi(e.{0}) collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.Product://28产品
                            curItemOrderBySQL = string.Format("crm_func_entity_protocol_format_product_multi(e.{0}) collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.ProductSet://29产品系列
                            curItemOrderBySQL = string.Format("crm_func_entity_protocol_format_productserial_multi(e.{0}) collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.RecAudits://1007记录状态
                            curItemOrderBySQL = string.Format("crm_func_entity_protocol_format_workflow_auditstatus(e.{0}) collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.RecId://1001
                            curItemOrderBySQL = string.Format("e.{0} {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.RecItemid://1010明细ID
                            //不能排序
                            break;
                        case EntityFieldControlType.RecName://1012记录名称
                            curItemOrderBySQL = string.Format("e.{0} collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.RecOnlive://1011 活动时间
                            curItemOrderBySQL = string.Format("e.{0} {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.RecStatus://1008 记录状态
                            curItemOrderBySQL = string.Format("e.{0} {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.TakePhoto://
                            //不能排序
                            break;
                        case EntityFieldControlType.Telephone:
                        case EntityFieldControlType.Text:
                        case EntityFieldControlType.TextArea:
                        case EntityFieldControlType.TipText:
                            curItemOrderBySQL = string.Format("e.{0} collate \"zh_CN\" {1}", orderFieldInfo.FieldName, orderType);
                            break;
                        case EntityFieldControlType.TreeMulti://27树形多选
                            //不处理
                            break;
                        case EntityFieldControlType.TreeSingle://21树形
                            //不处理
                            break;
                    }
                    if (curItemOrderBySQL != null && curItemOrderBySQL.Length > 0)
                    {
                        if (totalReturn == null || totalReturn.Length == 0)
                        {
                            totalReturn = curItemOrderBySQL;
                        }
                        else
                        {
                            totalReturn += ("," + curItemOrderBySQL);
                        }
                    }
                }
            }
            return totalReturn;
        }

        public OutputResult<object> DataList2(DynamicEntityListModel dynamicModel, bool isAdvanceQuery, int userNumber, bool CalcCountOnly = false)
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


                var validResults = isAdvanceQuery ? DynamicProtocolHelper.AdvanceQuery2(searchFields, dynamicModel.SearchData) : DynamicProtocolHelper.SimpleQuery2(searchFields, dynamicModel.SearchData);
                if (SpecFuncName != null)
                {
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

            if (dynamicModel.RelInfo != null
                && dynamicModel.RelInfo.ContainsKey("recid") && dynamicModel.RelInfo["recid"] != null
                 && dynamicModel.RelInfo.ContainsKey("relid") && dynamicModel.RelInfo["relid"] != null)
            {
                string sqlWhere = _dynamicEntityRepository.ReturnRelTabSql(Guid.Parse(dynamicModel.RelInfo["relid"].ToString()), Guid.Parse(dynamicModel.RelInfo["recid"].ToString()), userNumber);
                if (sqlWhere != null && sqlWhere.StartsWith("and recid"))
                {//兼容历史
                    if (SpecFuncName != null)
                    {
                        sqlWhere = sqlWhere.Replace("and recid", "and t.recid");
                    }
                    else
                    {
                        sqlWhere = sqlWhere.Replace("and recid", "and e.recid");
                        dynamicEntity.RelSql = sqlWhere;
                        sqlWhere = " and 1=1 ";
                    }
                }
                dynamicEntity.SearchQuery = dynamicEntity.SearchQuery + sqlWhere;


            }
            bool isColumnFilter = false;
            #region 处理字段自由过滤
            if (dynamicModel.ColumnFilter != null && dynamicModel.ColumnFilter.Count > 0)
            {

                var searchFields = GetEntityFields(dynamicEntity.EntityId, userNumber);
                foreach (DynamicEntityFieldSearch field in searchFields)
                {
                    if (field.ControlType == (int)DynamicProtocolControlType.SelectSingle
                        /*|| field.ControlType == (int)DynamicProtocolControlType.SelectMulti //暂时把多选也设置为模糊*/)
                    {
                        field.IsLike = 0;
                    }
                    else
                    {
                        field.IsLike = 1;//把除字典类型所有的字段都设成模糊搜索
                    }

                }
                Dictionary<string, object> fieldDatas = new Dictionary<string, object>();
                foreach (string key in dynamicModel.ColumnFilter.Keys)
                {
                    DynamicEntityFieldSearch fieldInfo = searchFields.FirstOrDefault(t => t.FieldName.ToString() == key);
                    if (fieldInfo == null) continue;
                    fieldDatas.Add(fieldInfo.FieldName, dynamicModel.ColumnFilter[key]);

                }
                var validResults = DynamicProtocolHelper.AdvanceQuery2(searchFields, fieldDatas);
                if (SpecFuncName != null)
                {
                    validResults = DynamicProtocolHelper.AdvanceQuery(searchFields, fieldDatas);
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
                    dynamicEntity.SearchQuery = dynamicEntity.SearchQuery + " AND (" + string.Join(" AND ", data.Values.ToArray()) + ")";
                    isColumnFilter = true;
                }



            }

            #endregion
            #region 处理嵌套表格批量查询问题
            if (dynamicModel.MainIds != null && dynamicModel.MainIds.Count > 0)
            {
                string sqlMainId = "";
                if (SpecFuncName != null)
                {
                    sqlMainId = " and t.recid in ";
                }
                else
                {
                    sqlMainId = " and e.recid in ";
                }
                string sids = "";
                foreach (Guid id in dynamicModel.MainIds)
                {
                    sids = sids + ",'" + id.ToString() + "'";
                }
                sids = sids.Substring(1);
                sqlMainId = sqlMainId + "(" + sids + ") ";
                dynamicEntity.SearchQuery = dynamicEntity.SearchQuery + sqlMainId;
            }
            #endregion
            #region 处理精确帅选项
            if (dynamicModel.ExactFieldOrFilter != null)
            {
                string tmp = " and (1<>1  ";
                foreach (string item in dynamicModel.ExactFieldOrFilter.Keys)
                {
                    if (SpecFuncName != null)
                    {
                        tmp = tmp + string.Format("or t.{0} = '{1}' ", item, dynamicModel.ExactFieldOrFilter[item]);
                    }
                    else
                    {
                        tmp = tmp + string.Format("or e.{0} = '{1}' ", item, dynamicModel.ExactFieldOrFilter[item]);
                    }
                }
                tmp = tmp + ")";
                dynamicEntity.SearchQuery = dynamicEntity.SearchQuery + tmp;
            }
            #endregion 
            //处理排序语句
            if (string.IsNullOrWhiteSpace(dynamicEntity.SearchOrder))
            {
                if (SpecFuncName != null)
                {
                    dynamicEntity.SearchOrder = " t.recversion desc ";
                }
                else
                {
                    dynamicEntity.SearchOrder = " e.recversion desc ";

                }
            }
            return this.CommonDataList(dynamicEntity, pageParam, isAdvanceQuery, userNumber, CalcCountOnly, isColumnFilter);
        }

		public Dictionary<string, List<IDictionary<string, object>>> Detail(DynamicEntityDetailtMapper dynamicEntity, int userNumber, DbTransaction tran = null)
		{

			if (dynamicEntity == null || !dynamicEntity.IsValid())
			{
				var errorTips = dynamicEntity.ValidationState.Errors.First();
				throw new Exception(errorTips);
			}

			var result = _dynamicEntityRepository.DetailMulti(dynamicEntity, userNumber, tran);
			List<DynamicEntityFieldSearch> fieldList = _dynamicEntityRepository.GetEntityFields(dynamicEntity.EntityId, userNumber);

			List<IDictionary<string, object>> list = result["Detail"];
			foreach (DynamicEntityFieldSearch fieldInfo in fieldList)
			{
				if (fieldInfo.ControlType == (int)(DynamicProtocolControlType.DataSourceSingle))
				{
					foreach (IDictionary<string, object> item in list)
					{
						if (item.ContainsKey(fieldInfo.FieldName)
							&& item.ContainsKey(fieldInfo.FieldName + "_name")
							&& item[fieldInfo.FieldName] != null)
						{
							if (item[fieldInfo.FieldName] is IDictionary<string, object>)
							{
								IDictionary<string, object> obj = ((IDictionary<string, object>)item[fieldInfo.FieldName]);
								if (obj != null && obj.ContainsKey("id") && obj.ContainsKey("name"))
								{
									if (!((item[fieldInfo.FieldName + "_name"] == null && obj["name"] == null)
										|| obj["name"].Equals(item[fieldInfo.FieldName + "_name"])))
									{
										obj["name"] = item[fieldInfo.FieldName + "_name"];
										item[fieldInfo.FieldName] = obj;

									}
								}
							}
							else if (item[fieldInfo.FieldName] is Dictionary<string, object>)
							{
								Dictionary<string, object> obj = ((Dictionary<string, object>)item[fieldInfo.FieldName]);
								if (obj != null && obj.ContainsKey("id") && obj.ContainsKey("name"))
								{
									if (!((item[fieldInfo.FieldName + "_name"] == null && obj["name"] == null)
										|| obj["name"].Equals(item[fieldInfo.FieldName + "_name"])))
									{
										obj["name"] = item[fieldInfo.FieldName + "_name"];
										item[fieldInfo.FieldName] = obj;

									}
								}
							}
							else if (item[fieldInfo.FieldName] is string)
							{
								Dictionary<string, object> obj = JsonConvert.DeserializeObject<Dictionary<string, object>>((string)item[fieldInfo.FieldName]);
								if (obj != null && obj.ContainsKey("id") && obj.ContainsKey("name"))
								{
									Dictionary<string, object> fieldConfigDict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(fieldInfo.FieldConfig);
									bool SaveNamed = false;
									if (fieldConfigDict.ContainsKey("dataSource")
										&& fieldConfigDict["dataSource"] != null)
									{
										Dictionary<string, object> datasourceInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(
										Newtonsoft.Json.JsonConvert.SerializeObject(fieldConfigDict["dataSource"]));
										if (datasourceInfo != null && datasourceInfo.ContainsKey("namefrom") && datasourceInfo["namefrom"] != null)
										{
											string namefrom = datasourceInfo["namefrom"].ToString();
											if (namefrom.Equals("1"))
											{
												SaveNamed = true;
											}
										}
									}
									if (!((item[fieldInfo.FieldName + "_name"] == null && obj["name"] == null)
									   || obj["name"].Equals(item[fieldInfo.FieldName + "_name"])))
									{
										if (SaveNamed)
										{
											item[fieldInfo.FieldName + "_name"] = obj["name"];
										}
										else
										{
											obj["name"] = item[fieldInfo.FieldName + "_name"];
											item[fieldInfo.FieldName] = JsonConvert.SerializeObject(obj);
										}
									}
								}

							}
						}
					}
				}
			}

			var entityInfo = _dynamicEntityRepository.getEntityBaseInfoById(dynamicEntity.EntityId, userNumber, tran);
			if (entityInfo != null)
			{
				var modelType = Convert.ToInt32(entityInfo["modeltype"].ToString());
				if (modelType == 3)
				{
					if (entityInfo["relentityid"] == null || entityInfo["relfieldid"] == null)
						throw new Exception("动态列表配置异常");
					var detail = result["Detail"].FirstOrDefault();
					if (detail["recrelateid"] == null)
						throw new Exception("关联实体Id不能为空");

					var relResult = _dynamicEntityRepository.Detail(new DynamicEntityDetailtMapper
					{
						EntityId = Guid.Parse(entityInfo["relentityid"].ToString()),
						RecId = Guid.Parse(detail["recrelateid"].ToString()),
						NeedPower = 0
					}, userNumber, tran);
					if (relResult != null)
					{
						var relEntityFields = _dynamicEntityRepository.GetEntityFields(Guid.Parse(entityInfo["relentityid"].ToString()), userNumber, tran);
						var relField = relEntityFields.FirstOrDefault(t => t.FieldId == Guid.Parse(entityInfo["relfieldid"].ToString()));
						if (relField == null) throw (new Exception("动态表单的关联字段配置异常，请联系管理员"));
						if (relResult[relField.FieldName] != null && entityInfo["relfieldid"] != null && entityInfo["relfieldname"] != null)
						{
							if (!detail.ContainsKey(entityInfo["relfieldname"].ToString()))
							{
								detail.Add(entityInfo["relfieldname"].ToString(), relResult[relField.FieldName]);
								if (relResult.ContainsKey(relField.FieldName + "_name"))
								{
									if (!detail.ContainsKey(entityInfo["relfieldname"].ToString() + "_name"))
									{
										detail.Add(entityInfo["relfieldname"].ToString() + "_name", relResult[relField.FieldName + "_name"]);
									}
								}
								else
								{
									if (!detail.ContainsKey(entityInfo["relfieldname"].ToString() + "_name"))
									{
										detail.Add(entityInfo["relfieldname"].ToString() + "_name", relResult[relField.FieldName]);
									}
								}

							}

						}
					}
				}
			}

			Guid entityid = dynamicEntity.EntityId;
			if (dynamicEntity.EntityId == new Guid("00000000-0000-0000-0000-000000000001"))
			{
				var workflowcaseInfo = _workFlowRepository.GetWorkFlowCaseInfo(null, dynamicEntity.RecId);
				if (workflowcaseInfo != null)
				{
					var workflowInfo = _workFlowRepository.GetWorkFlowInfo(null, workflowcaseInfo.FlowId);
					if (workflowInfo != null)
					{
						entityid = workflowInfo.Entityid;

						result["entitydetail"] = DealLinkTableFields(result["entitydetail"], entityid, userNumber, tran);
						var relatedetail = result["relatedetail"];
						if (relatedetail.Count > 0)
						{
							relatedetail = DealLinkTableFields(relatedetail, workflowInfo.RelEntityId, userNumber, tran);
						}
					}
				}
			} 
			else
            {
                if (result.ContainsKey("Detail") == false || result["Detail"] == null || result["Detail"].Count == 0)
                {
                    throw new Exception("权限不足或者数据已经被删除");
                }
                var data = result["Detail"].FirstOrDefault();
                if (data.ContainsKey("recstatus"))
                {
                    if (data["recstatus"].ToString() == "0")
                    {
                        throw new Exception("数据已经被删除");
                    }
                }
                result["Detail"] = DealLinkTableFields(result["Detail"], entityid, userNumber, tran);
            }

			if (dynamicEntity.EntityId == new Guid("0b81d536-3817-4cbc-b882-bc3e935db845"))
			{
				var summary = result["summary"];
				if (summary != null)
				{
					entityid = new Guid("fcc648ae-8817-48b7-b1d7-49ed4c24316b");
					result["summary"] = DealLinkTableFields(summary, entityid, userNumber, tran);
				}
			}
			return result;
        }


        public List<IDictionary<string, object>> DealLinkTableFields(List<IDictionary<string, object>> data, Guid entityid, int userNumber, DbTransaction tran)
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

                        row[filed.FieldName] = _dynamicEntityRepository.DetailList(modeltemp, userNumber, tran);
                    }
                }

            }
            return data;
        }


        public OutputResult<object> Detail(DynamicEntityDetailModel dynamicModel, int userNumber, DbTransaction tran = null)
        {
            var dynamicEntity = _mapper.Map<DynamicEntityDetailModel, DynamicEntityDetailtMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }
            var result = Detail(dynamicEntity, userNumber, tran);

            if (result.Count == 1)
            {
                var singleResult = new Dictionary<string, object>();
                singleResult.Add(result.Keys.First(), result.Values.First().FirstOrDefault());
                return new OutputResult<object>(singleResult);
            }
            else return new OutputResult<object>(result);
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

            var result = _dynamicEntityRepository.DetailList(dynamicEntity, userNumber, null);

            return new OutputResult<object>(result);
        }
        public OutputResult<object> DetailRelList(DynamicEntityDetailRellistModel dynamicModel, int userNumber)
        {
            if (dynamicModel.FieldId == Guid.Empty || dynamicModel.RelEntityId == Guid.Empty || dynamicModel.RelRecId == Guid.Empty)
                return new OutputResult<object>
                {
                    Status = 1,
                    Message = "获取数据失败"
                };
            // 主实体数据，例如合同下的某个表格控件，对于表格控件合同就是主实体
            //IDictionary<string, object> relDetail = _dynamicEntityRepository.Detail(new DynamicEntityDetailtMapper
            //{
            //    EntityId = dynamicModel.RelEntityId,
            //    NeedPower = dynamicModel.NeedPower,
            //    RecId = dynamicModel.RelRecId,
            //}, userNumber);

            var protocols = this.GeneralProtocol(new DynamicEntityGeneralModel
            {
                OperateType = 2,
                TypeId = dynamicModel.EntityId
            }, userNumber).DataBody as List<DynamicEntityDataFieldMapper>;
            var nestedTablesProtocol = protocols.FirstOrDefault(t => t.FieldId == dynamicModel.FieldId && t.ControlType == 24);

            IDictionary<string, List<IDictionary<string, object>>> dicResult = new Dictionary<string, List<IDictionary<string, object>>>();

            var jo = JObject.Parse(nestedTablesProtocol.FieldConfig);
            var nesteds = jo["nested"].FirstOrDefault(t => t["sourcefieldid"].ToString() == dynamicModel.SourceFieldId.ToString() && t["nestedtablesfieldid"].ToString() == dynamicModel.NestedTablesFieldId.ToString());
            List<IDictionary<string, object>> result = null;

            if (nesteds["sourceentityid"] == null && nesteds["sourcefieldid"] == null)
                return HandleResult(new OperateResult { Flag = 0, Msg = "表格控件关联的数据源实体Id或字段id不能为空" });
            if (nesteds["nestedtablesentityid"] == null && nesteds["nestedtablesfieldid"] == null)
                return HandleResult(new OperateResult { Flag = 0, Msg = "表格控件关联的表格控件实体Id或字段id不能为空" });

            var sourceField = protocols.FirstOrDefault(t2 => t2.FieldId == Guid.Parse(nesteds["sourcefieldid"].ToString()));
            if (sourceField == null)
                throw new Exception("数据源关联异常");


            var r1 = this.Detail(new DynamicEntityDetailtMapper
            {
                EntityId = Guid.Parse(nesteds["sourceentityid"].ToString()),
                NeedPower = dynamicModel.NeedPower,
                RecId = dynamicModel.RelRecId
            }, userNumber);
            var r1Data = r1["Detail"].FirstOrDefault();
            if (r1Data != null)
            {
                var p1 = this.GeneralProtocol(new DynamicEntityGeneralModel
                {
                    OperateType = 2,
                    TypeId = Guid.Parse(r1["Detail"].FirstOrDefault()["rectype"].ToString())
                }, userNumber).DataBody as List<DynamicEntityDataFieldMapper>;
                var nestedTablesField = p1.FirstOrDefault(t2 => t2.FieldId == Guid.Parse(nesteds["nestedtablesfieldid"].ToString()));
                if (nestedTablesField == null)
                    throw new Exception("表格控件关联异常");
                var nestedTablesFieldValue = r1Data[nestedTablesField.FieldName];
                if (nestedTablesFieldValue != null && (r1Data[nestedTablesField.FieldName] is List<IDictionary<string, object>>))
                    result = (r1Data[nestedTablesField.FieldName] as List<IDictionary<string, object>>);
            }
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
            if (info.FuncBtns == null) return new OutputResult<object>(funcBtns);
            foreach (var btn in info.FuncBtns)
            {
                if (btn.ButtonCode == "AddEntityData" && (dynamicModel.RecIds == null || dynamicModel.RecIds.Count <= 0))
                {
                    funcBtns.Add(btn);
                }

                //if(btn.FunctionId==Guid.Empty|| funcs.Exists(m=>m.FuncId==btn.FunctionId))
                if (!string.IsNullOrEmpty(btn.RoutePath) && funcs.Exists(m => m.RoutePath == btn.RoutePath && m.DeviceType == (int)DeviceClassic))
                {
                    switch (btn.SelectType)
                    {
                        case DataSelectType.Single:
                            if (dynamicModel.RecIds != null && dynamicModel.RecIds.Count == 1)
                            {
                                var checkAccess = userData.HasDataAccess(null, btn.RoutePath, dynamicModel.EntityId, DeviceClassic, dynamicModel.RecIds);
                                if (checkAccess&&btn.ButtonCode!="AddEntityData")
                                    funcBtns.Add(btn);
                            }
                            break;
                        case DataSelectType.Multiple:
                            if (dynamicModel.RecIds != null && dynamicModel.RecIds.Count >= 1)
                            {
                                var checkAccess = userData.HasDataAccess(null, btn.RoutePath, dynamicModel.EntityId, DeviceClassic, dynamicModel.RecIds);
                                if (checkAccess&&btn.ButtonCode!="AddEntityData")
                                    funcBtns.Add(btn);
                            }
                            break;
                        default:
                            if (dynamicModel.RecIds != null && dynamicModel.RecIds.Count >= 1)
                            {
                                var checkAccess = userData.HasDataAccess(null, btn.RoutePath, dynamicModel.EntityId, DeviceClassic, dynamicModel.RecIds);
                                if (checkAccess&&btn.ButtonCode!="AddEntityData")
                                    funcBtns.Add(btn);
                            }
                            // funcBtns.Add(btn);
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
            List<DynamicEntityWebFieldMapper> listColumns = _dynamicEntityRepository.GetWebFields(typeId, (int)operateType, userNumber);
            CalcDefaultListViewColumnWidth(listColumns);
            return listColumns;
        }
        public List<DynamicEntityWebFieldMapper> GetWebDynamicListFields(Guid typeId, DynamicProtocolOperateType operateType, int userNumber)
        {
            List<DynamicEntityWebFieldMapper> listColumns = _dynamicEntityRepository.GetWebDynamicListFields(typeId, (int)operateType, userNumber);
            CalcDefaultListViewColumnWidth(listColumns);
            return listColumns;
        }
        /// <summary>
        /// 初始化默认的web列表宽度显示 
        /// </summary>
        /// <param name="listColumns"></param>
        private void CalcDefaultListViewColumnWidth(List<DynamicEntityWebFieldMapper> listColumns)
        {
            if (listColumns == null) return;
            foreach (DynamicEntityWebFieldMapper fieldInfo in listColumns)
            {
                if (fieldInfo.DefaultWidth <= 0)
                {
                    EntityFieldControlType controlType = (EntityFieldControlType)fieldInfo.ControlType;
                    switch (controlType)
                    {
                        case EntityFieldControlType.Address:
                            fieldInfo.DefaultWidth = 200;
                            break;
                        case EntityFieldControlType.AreaGroup:
                            break;
                        case EntityFieldControlType.AreaRegion:
                            fieldInfo.DefaultWidth = 200;
                            break;
                        case EntityFieldControlType.DataSourceMulti:
                            fieldInfo.DefaultWidth = 200;
                            break;
                        case EntityFieldControlType.DataSourceSingle:
                            fieldInfo.DefaultWidth = 200;
                            break;
                        case EntityFieldControlType.Department:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.EmailAddr:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.FileAttach:
                            fieldInfo.DefaultWidth = 140;
                            break;
                        case EntityFieldControlType.HeadPhoto:
                            fieldInfo.DefaultWidth = 130;
                            break;
                        case EntityFieldControlType.LinkeTable:
                            fieldInfo.DefaultWidth = 110;
                            break;
                        case EntityFieldControlType.Location:
                            fieldInfo.DefaultWidth = 200;
                            break;
                        case EntityFieldControlType.NumberDecimal:
                            fieldInfo.DefaultWidth = 100;
                            break;
                        case EntityFieldControlType.NumberInt:
                            fieldInfo.DefaultWidth = 100;
                            break;
                        case EntityFieldControlType.PersonSelectMulti:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.PersonSelectSingle:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.PhoneNum:
                            fieldInfo.DefaultWidth = 130;
                            break;
                        case EntityFieldControlType.Product:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.ProductSet:
                            fieldInfo.DefaultWidth = 150; ;
                            break;
                        case EntityFieldControlType.QuoteControl:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.RecAudits:
                            fieldInfo.DefaultWidth = 80;
                            break;
                        case EntityFieldControlType.RecCreated:
                            fieldInfo.DefaultWidth = 80;
                            break;
                        case EntityFieldControlType.RecCreator:
                            fieldInfo.DefaultWidth = 80;
                            break;
                        case EntityFieldControlType.RecId:
                            fieldInfo.DefaultWidth = 80;
                            break;
                        case EntityFieldControlType.RecItemid:
                            fieldInfo.DefaultWidth = 80;
                            break;
                        case EntityFieldControlType.RecManager:
                            fieldInfo.DefaultWidth = 80;
                            break;
                        case EntityFieldControlType.RecName:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.RecOnlive:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.RecStatus:
                            fieldInfo.DefaultWidth = 80;
                            break;
                        case EntityFieldControlType.RecType:
                            fieldInfo.DefaultWidth = 80;
                            break;
                        case EntityFieldControlType.RecUpdated:
                            fieldInfo.DefaultWidth = 100;
                            break;
                        case EntityFieldControlType.RecUpdator:
                            fieldInfo.DefaultWidth = 80;
                            break;
                        case EntityFieldControlType.SalesStage:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.SelectMulti:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.SelectSingle:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.TakePhoto:
                            fieldInfo.DefaultWidth = 100;
                            break;
                        case EntityFieldControlType.Telephone:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.Text:
                            fieldInfo.DefaultWidth = 200;
                            break;
                        case EntityFieldControlType.TextArea:
                            fieldInfo.DefaultWidth = 200;
                            break;
                        case EntityFieldControlType.TimeDate:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.TimeStamp:
                            fieldInfo.DefaultWidth = 150;
                            break;
                        case EntityFieldControlType.TipText:
                            fieldInfo.DefaultWidth = 200;
                            break;
                        case EntityFieldControlType.TreeMulti:
                            fieldInfo.DefaultWidth = 200;
                            break;
                        case EntityFieldControlType.TreeSingle:
                            fieldInfo.DefaultWidth = 200;
                            break;
                    }
                }
            }
        }

        public List<DynamicEntityFieldSearch> GetSearchFields(Guid entityId, int userNumber)
        {
            return _dynamicEntityRepository.GetSearchFields(entityId, userNumber);
        }

        public List<DynamicEntityFieldSearch> GetEntityFields(Guid entityId, int userNumber)
        {
            return _dynamicEntityRepository.GetEntityFields(entityId, userNumber);
        }
        public OutputResult<object> TransferPro(EntityTransferParamInfo paramInfo, int userId)
        {
            if (paramInfo.RecIds != null)
            {
                //如果是
                if (paramInfo.SchemeId != null && paramInfo.SchemeId != Guid.Empty)
                {
                    return TransferPro_RecIds_Scheme(paramInfo, userId);
                }
                else
                {
                    return TransferPro_RecIds_Common(paramInfo, userId);
                }
            }
            else if (paramInfo.DataFilter != null)
            {
                if (paramInfo.SchemeId != null && paramInfo.SchemeId != Guid.Empty)
                {
                    return TransferPro_Filter_Scheme(paramInfo, userId);
                }
                else
                {
                    return TransferPro_Filter_Common(paramInfo, userId);
                }
            }
            else
            {
                throw (new Exception("参数异常"));
            }
        }
        private OutputResult<object> TransferPro_RecIds_Scheme(EntityTransferParamInfo paramInfo, int userId)
        {
            return new OutputResult<object>(null, "暂时不支持方案转移", -1);
            //string[] ids = paramInfo.RecIds.Split(',');
            //foreach (string id in ids) {

            //}
            //return null;
        }
        private OutputResult<object> TransferPro_RecIds_Common(EntityTransferParamInfo paramInfo, int userId)
        {
            string[] ids = paramInfo.RecIds.Split(',');
            string entityTableName = "";
            string entityName = "";
            string newUserName = "";
            string oldUserName = "";
            Dictionary<string, object> EntityInfo = this._dynamicEntityRepository.getEntityBaseInfoById(paramInfo.EntityId, userId);
            entityName = EntityInfo["entityname"].ToString();
            entityTableName = EntityInfo["entitytable"].ToString();
            Dictionary<string, object> fieldInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Newtonsoft.Json.JsonConvert.SerializeObject(this._entityProRepository.GetFieldInfo(paramInfo.FieldId, userId)));
            string FieldName = fieldInfo["fieldname"].ToString();
            bool isMultiPerson = false;
            #region 检查是否多选用户
            int controltype = int.Parse(fieldInfo["controltype"].ToString());
            Dictionary<string, string> fieldConfig = JsonConvert.DeserializeObject<Dictionary<string, string>>(fieldInfo["fieldconfig"].ToString());
            if (fieldConfig.ContainsKey("multiple") && fieldConfig["multiple"] != null)
            {
                if (fieldConfig["multiple"].ToString().Equals("1"))
                {
                    isMultiPerson = true;
                }
            }
            #endregion 
            string FieldName_Display = fieldInfo["displayname"].ToString();
            if (fieldInfo == null) throw (new Exception("字段定义有误"));
            UserInfo newUserInfo = this._accountRepository.GetUserInfoById(paramInfo.NewUserId);
            if (newUserInfo == null) throw (new Exception("新的负责人不存在"));
            newUserName = newUserInfo.UserName;
            if (paramInfo.OUserId > 0)
            {
                UserInfo oldUserInfo = this._accountRepository.GetUserInfoById(paramInfo.OUserId);
                if (oldUserInfo != null)
                {
                    oldUserName = oldUserInfo.UserName;
                }
            }
            List<TransferTempInfo> retList = new List<TransferTempInfo>();
            foreach (string id in ids)
            {
                string newUserIds = "";
                DynamicEntityDetailtMapper p = new DynamicEntityDetailtMapper()
                {
                    EntityId = paramInfo.EntityId,
                    RecId = Guid.Parse(id),
                    NeedPower = 0
                };
                IDictionary<string, object> detail = this._dynamicEntityRepository.Detail(p, userId, null);
                bool needChanged = false;
                if (detail.ContainsKey(FieldName) == false || detail[FieldName] == null)
                {
                    needChanged = true;
                }
                else
                {
                    if (isMultiPerson)//多选的选人控件
                    {
                        if (paramInfo.OUserId > 0)
                        {
                            //需要检查人员匹配才增加
                            bool isNeed = false;
                            string[] oldPersons = detail[FieldName].ToString().Split(',');
                            foreach (string person in oldPersons)
                            {
                                if (person == paramInfo.OUserId.ToString())
                                {
                                    isNeed = true;
                                    break;
                                }
                            }
                            if (isNeed)
                            {
                                foreach (string person in oldPersons)
                                {
                                    if (person == paramInfo.NewUserId.ToString())
                                    {
                                        isNeed = false;
                                        break;
                                    }
                                }
                                if (isNeed) needChanged = true;
                            }
                            if (needChanged)
                            {
                                newUserIds = paramInfo.NewUserId.ToString();
                                foreach (string person in oldPersons)
                                {
                                    if (person != paramInfo.OUserId.ToString())
                                        newUserIds = newUserIds + "," + person;
                                }
                            }

                        }
                        else
                        {
                            //只要不存在，则新增此人
                            bool isNeed = true;
                            string[] oldPersons = detail[FieldName].ToString().Split(',');
                            foreach (string person in oldPersons)
                            {
                                if (person == paramInfo.NewUserId.ToString())
                                {
                                    isNeed = false;
                                    break;
                                }
                            }
                            if (isNeed) needChanged = true;
                            if (needChanged)
                            {
                                newUserIds = paramInfo.NewUserId.ToString();
                                foreach (string person in oldPersons)
                                {
                                    newUserIds = newUserIds + "," + person;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (detail[FieldName].ToString().Equals(paramInfo.NewUserId.ToString()) == false)
                        {
                            needChanged = true;
                        }
                    }


                }
                if (needChanged)
                {
                    TransferTempInfo msg = new TransferTempInfo()
                    {
                        Message = "",
                        DetailInfo = detail,
                        FieldNames = new List<string>() { FieldName },
                        NewUserId = paramInfo.NewUserId,
                        NewUserName = newUserName,
                        NewUserIds = newUserIds,
                        OldUserId = paramInfo.OUserId,
                        OldUserName = oldUserName,
                        IsMultiPersonField = isMultiPerson,
                        TableName = entityTableName,
                        EntityName = entityName,

                        EntityId = paramInfo.EntityId,
                        TypeId = detail.ContainsKey("rectype") && detail["rectype"] != null ? Guid.Parse(detail["rectype"].ToString()) : paramInfo.EntityId,
                        RecId = Guid.Parse(detail["recid"].ToString()),
                        FieldDisplayNames = FieldName_Display,
                        RecName = detail["recname"] == null ? "" : detail["recname"].ToString(),
                    };
                    retList.Add(msg);
                }

            }
            TransferData(retList, userId);
            int success = 0;
            int error = 0;
            foreach (TransferTempInfo item in retList)
            {
                if (item.IsSuccess) success++;
                else error++;
            }
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            retDict.Add("success", success);
            retDict.Add("error", error);
            retDict.Add("detail", retList);
            return new OutputResult<object>(retDict);
        }
        private OutputResult<object> TransferPro_Filter_Scheme(EntityTransferParamInfo paramInfo, int userId)
        {
            return new OutputResult<object>(null, "暂时不支持方案转移", -1);
        }
        private OutputResult<object> TransferPro_Filter_Common(EntityTransferParamInfo paramInfo, int userId)
        {
            string entityTableName = "";
            string entityName = "";
            string newUserName = "";
            string oldUserName = "";
            Dictionary<string, object> EntityInfo = this._dynamicEntityRepository.getEntityBaseInfoById(paramInfo.EntityId, userId);
            entityName = EntityInfo["entityname"].ToString();
            entityTableName = EntityInfo["entitytable"].ToString();
            Dictionary<string, object> fieldInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Newtonsoft.Json.JsonConvert.SerializeObject(this._entityProRepository.GetFieldInfo(paramInfo.FieldId, userId)));
            string FieldName = fieldInfo["fieldname"].ToString();
            string FieldName_Display = fieldInfo["displayname"].ToString();
            bool isMultiPerson = false;
            if (fieldInfo == null) throw (new Exception("字段定义有误"));
            UserInfo newUserInfo = this._accountRepository.GetUserInfoById(paramInfo.NewUserId);
            if (newUserInfo == null) throw (new Exception("新的负责人不存在"));
            newUserName = newUserInfo.UserName;
            if (paramInfo.OUserId > 0)
            {
                UserInfo oldUserInfo = this._accountRepository.GetUserInfoById(paramInfo.OUserId);
                if (oldUserInfo != null)
                {
                    oldUserName = oldUserInfo.UserName;
                }
            }
            List<TransferTempInfo> retList = new List<TransferTempInfo>();
            paramInfo.DataFilter.PageIndex = 1;
            paramInfo.DataFilter.PageSize = 100000;
            OutputResult<object> tmpResult = this.DataList2(paramInfo.DataFilter, false, userId);
            if (tmpResult == null || tmpResult.DataBody == null) return new OutputResult<object>(retList);
            List<Dictionary<string, object>> datas = ((Dictionary<string, List<Dictionary<string, object>>>)tmpResult.DataBody)["PageData"];
            List<TransferTempInfo> thisDealed = new List<TransferTempInfo>();
            foreach (IDictionary<string, object> rowData in datas)
            {
                string newUserIds = "";
                bool needChanged = false;
                if (rowData.ContainsKey(FieldName) == false || rowData[FieldName] == null)
                {
                    needChanged = true;
                }
                else
                {
                    if (isMultiPerson)//多选的选人控件
                    {
                        if (paramInfo.OUserId > 0)
                        {
                            //需要检查人员匹配才增加
                            bool isNeed = false;
                            string[] oldPersons = rowData[FieldName].ToString().Split(',');
                            foreach (string person in oldPersons)
                            {
                                if (person == paramInfo.OUserId.ToString())
                                {
                                    isNeed = true;
                                    break;
                                }
                            }
                            if (isNeed)
                            {
                                foreach (string person in oldPersons)
                                {
                                    if (person == paramInfo.NewUserId.ToString())
                                    {
                                        isNeed = false;
                                        break;
                                    }
                                }
                                if (isNeed) needChanged = true;
                            }
                            if (needChanged)
                            {
                                newUserIds = paramInfo.NewUserId.ToString();
                                foreach (string person in oldPersons)
                                {
                                    if (person != paramInfo.OUserId.ToString())
                                        newUserIds = newUserIds + "," + person;
                                }
                            }

                        }
                        else
                        {
                            //只要不存在，则新增此人
                            bool isNeed = true;
                            string[] oldPersons = rowData[FieldName].ToString().Split(',');
                            foreach (string person in oldPersons)
                            {
                                if (person == paramInfo.NewUserId.ToString())
                                {
                                    isNeed = false;
                                    break;
                                }
                            }
                            if (isNeed) needChanged = true;
                            if (needChanged)
                            {
                                newUserIds = paramInfo.NewUserId.ToString();
                                foreach (string person in oldPersons)
                                {
                                    newUserIds = newUserIds + "," + person;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (rowData[FieldName].ToString().Equals(paramInfo.NewUserId.ToString()) == false)
                        {
                            needChanged = true;
                        }
                    }

                }
                if (needChanged)
                {
                    TransferTempInfo msg = new TransferTempInfo()
                    {
                        Message = "",
                        DetailInfo = rowData,
                        FieldNames = new List<string>() { FieldName },
                        NewUserId = paramInfo.NewUserId,
                        NewUserName = newUserName,
                        NewUserIds = newUserIds,
                        OldUserId = paramInfo.OUserId,
                        OldUserName = oldUserName,
                        IsMultiPersonField = isMultiPerson,
                        TableName = entityTableName,
                        EntityName = entityName,
                        EntityId = paramInfo.EntityId,
                        TypeId = rowData.ContainsKey("rectype") && rowData["rectype"] != null ? Guid.Parse(rowData["rectype"].ToString()) : paramInfo.EntityId,
                        RecId = Guid.Parse(rowData["recid"].ToString()),
                        FieldDisplayNames = FieldName_Display,
                        RecName = rowData["recname"] == null ? "" : rowData["recname"].ToString(),
                    };
                    retList.Add(msg);
                }

            }
            TransferData(retList, userId);
            int success = 0;
            int error = 0;
            foreach (TransferTempInfo item in retList)
            {
                if (item.IsSuccess) success++;
                else error++;
            }
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            retDict.Add("success", success);
            retDict.Add("error", error);
            retDict.Add("detail", retList);
            return new OutputResult<object>(retDict);
        }
        public OutputResult<object> TransferUser2User(DynamicEntityTransferUser2UserModel paramInfo, int userId)
        {

            List<TransferTempInfo> AllDetailDatas = new List<TransferTempInfo>();
            string NewUserName = "";
            UserInfo newUserInfo = this._accountRepository.GetUserInfoById(paramInfo.NewUserId);
            if (newUserInfo == null) throw (new Exception("新的负责人不存在"));
            NewUserName = newUserInfo.UserName;
            foreach (DynamicEntityTransferUser2User_EntityFieldsModel item in paramInfo.Entities)
            {
                string entityTableName = "";
                string entityName = "";
                Dictionary<string, object> EntityInfo = this._dynamicEntityRepository.getEntityBaseInfoById(item.EntityId, userId);
                entityName = EntityInfo["entityname"].ToString();
                entityTableName = EntityInfo["entitytable"].ToString();
                DynamicEntityListModel model = new DynamicEntityListModel()
                {
                    EntityId = item.EntityId,
                    MenuId = null,
                    ViewType = 3,
                    PageIndex = 1,
                    PageSize = 10000//每次处理1万条
                };
                string[] fieldids = item.FieldIds.Split(',');
                List<DynamicEntityFieldSearch> searchFields = new List<DynamicEntityFieldSearch>();
                List<DynamicEntityFieldSearch> allEntityFields = this.GetEntityFields(item.EntityId, userId);
                foreach (DynamicEntityFieldSearch field in allEntityFields)
                {
                    bool isInFound = false;
                    foreach (string fieldid in fieldids)
                    {
                        if (field.FieldId.ToString().Equals(fieldid))
                        {
                            isInFound = true;
                            break;
                        }
                    }
                    if (isInFound)
                    {
                        searchFields.Add(field);
                    }
                }
                model.ExactFieldOrFilter = new Dictionary<string, object>();
                foreach (DynamicEntityFieldSearch field in searchFields)
                {
                    model.ExactFieldOrFilter.Add(field.FieldName, paramInfo.OldUserId);
                }
                OutputResult<object> tmpResult = this.DataList2(model, false, userId);
                if (tmpResult == null || tmpResult.DataBody == null) continue;
                List<Dictionary<string, object>> datas = ((Dictionary<string, List<Dictionary<string, object>>>)tmpResult.DataBody)["PageData"];
                List<TransferTempInfo> thisDealed = new List<TransferTempInfo>();
                foreach (IDictionary<string, object> rowData in datas)
                {
                    List<string> fieldNames = new List<string>();
                    List<string> displayNames = new List<string>();
                    foreach (DynamicEntityFieldSearch field in searchFields)
                    {
                        if (rowData.ContainsKey(field.FieldName) && rowData[field.FieldName] != null
                            && rowData[field.FieldName].ToString().Equals(paramInfo.OldUserId.ToString()))
                        {
                            //需要修改此字段
                            fieldNames.Add(field.FieldName);
                            displayNames.Add(field.DisplayName);
                        }
                    }
                    if (fieldNames.Count > 0)
                    {
                        TransferTempInfo msg = new TransferTempInfo()
                        {
                            Message = "",
                            DetailInfo = rowData,
                            FieldNames = fieldNames,
                            NewUserId = paramInfo.NewUserId,
                            NewUserName = NewUserName,
                            TableName = entityTableName,
                            EntityName = entityName,
                            EntityId = item.EntityId,
                            TypeId = rowData.ContainsKey("rectype") && rowData["rectype"] != null ? Guid.Parse(rowData["rectype"].ToString()) : item.EntityId,
                            RecId = Guid.Parse(rowData["recid"].ToString()),
                            FieldDisplayNames = string.Join('、', displayNames.ToArray()),
                            RecName = rowData["recname"] == null ? "" : rowData["recname"].ToString(),
                        };
                        thisDealed.Add(msg);
                    }
                }
                #region 更新并发送消息
                TransferData(thisDealed, userId);
                AllDetailDatas.AddRange(thisDealed);
                #endregion
            }
            int success = 0;
            int error = 0;
            foreach (TransferTempInfo item in AllDetailDatas)
            {
                if (item.IsSuccess) success++;
                else error++;
            }
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            retDict.Add("success", success);
            retDict.Add("error", error);
            retDict.Add("detail", AllDetailDatas);
            return new OutputResult<object>(retDict);
        }
        private string ModifyRecord(Dictionary<string, object> oldDetail, Dictionary<string, DynamicProtocolValidResult> requireData, Guid entityid, Guid bussinessId, int userId)
        {
            string msgContent = "";
            try
            {
                Dictionary<string, DynamicEntityModifyMapper> tempData = new Dictionary<string, DynamicEntityModifyMapper>();
                foreach (var requKey in requireData.Keys)
                {
                    DynamicProtocolValidResult requTarget = requireData[requKey];
                    foreach (var oldkey in oldDetail.Keys)
                    {
                        if (oldkey == requKey)
                        {
                            var oldTarget = oldDetail[oldkey].ToString();
                            var newTarget = requTarget.FieldData.ToString();
                            switch ((DynamicProtocolControlType)requTarget.ControlType)
                            {
                                case DynamicProtocolControlType.NumberInt:
                                    {
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldTarget, newTarget);
                                        break;
                                    }
                                case DynamicProtocolControlType.NumberDecimal:
                                    {
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldTarget, newTarget);
                                        break;
                                    }
                                case DynamicProtocolControlType.Text:
                                    {
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldTarget, newTarget);
                                        break;
                                    }
                                case DynamicProtocolControlType.SelectSingle:
                                    {

                                        if (oldTarget != newTarget)
                                        {
                                            if (!string.IsNullOrEmpty(requTarget.FieldConfig))
                                            {
                                                JObject jo = JObject.Parse(requTarget.FieldConfig);
                                                jo = JObject.Parse(jo["dataSource"].ToString());
                                                int dictdataid = Convert.ToInt32(jo["sourceId"].ToString());
                                                string dictOldValue = _entityTransferRepository.getDictByVal(dictdataid, oldTarget);
                                                string dictNewValue = _entityTransferRepository.getDictByVal(dictdataid, newTarget);
                                                var modifyRecord = new DynamicEntityModifyMapper
                                                {
                                                    keyName = requKey,
                                                    field = requTarget.FieldDisplay,
                                                    oldVal = dictOldValue,
                                                    newVal = dictNewValue
                                                };
                                                tempData.Add(requKey, modifyRecord);
                                            }
                                        }
                                        break;
                                    }
                                case DynamicProtocolControlType.SelectMulti:
                                    {

                                        if (oldTarget != newTarget)
                                        {
                                            if (!string.IsNullOrEmpty(requTarget.FieldConfig))
                                            {
                                                JObject jo = JObject.Parse(requTarget.FieldConfig);
                                                jo = JObject.Parse(jo["dataSource"].ToString());
                                                int dictdataid = Convert.ToInt32(jo["sourceId"].ToString());
                                                string dictOldValue = _entityTransferRepository.getDictByVal(dictdataid, oldTarget);
                                                string dictNewValue = _entityTransferRepository.getDictByVal(dictdataid, newTarget);
                                                var modifyRecord = new DynamicEntityModifyMapper
                                                {
                                                    keyName = requKey,
                                                    field = requTarget.FieldDisplay,
                                                    oldVal = dictOldValue,
                                                    newVal = dictNewValue
                                                };
                                                tempData.Add(requKey, modifyRecord);
                                            }
                                        }
                                        break;
                                    }
                                case DynamicProtocolControlType.PhoneNum:
                                    {
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldTarget, newTarget);
                                        break;
                                    }
                                case DynamicProtocolControlType.EmailAddr:
                                    {
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldTarget, newTarget);
                                        break;
                                    }
                                case DynamicProtocolControlType.Telephone:
                                    {
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldTarget, newTarget);
                                        break;
                                    }
                                case DynamicProtocolControlType.Address:
                                    {
                                        Dictionary<string, object> oldAddress = JsonHelper.ToObject<Dictionary<string, object>>(oldTarget);
                                        Dictionary<string, object> newAddress = JsonHelper.ToObject<Dictionary<string, object>>(newTarget);
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldAddress["address"].ToString(), newAddress["address"].ToString());
                                        break;
                                    }
                                case DynamicProtocolControlType.DataSourceSingle:
                                    {
                                        Dictionary<string, object> oldSource = JsonHelper.ToObject<Dictionary<string, object>>(oldTarget);
                                        Dictionary<string, object> newSource = JsonHelper.ToObject<Dictionary<string, object>>(newTarget);
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldSource["name"].ToString(), newSource["name"].ToString());
                                        break;
                                    }
                                case DynamicProtocolControlType.DataSourceMulti:
                                    {
                                        Dictionary<string, object> oldSource = JsonHelper.ToObject<Dictionary<string, object>>(oldTarget);
                                        Dictionary<string, object> newSource = JsonHelper.ToObject<Dictionary<string, object>>(newTarget);
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldSource["name"].ToString(), newSource["name"].ToString());
                                        break;
                                    }
                                case DynamicProtocolControlType.RecName:
                                    {
                                        tempData = AddModifyRecord(tempData, requKey, requTarget.FieldId.ToString(), requTarget.FieldDisplay, oldTarget, newTarget);
                                        break;
                                    }
                                case DynamicProtocolControlType.RecManager:
                                    {
                                        if (oldTarget != newTarget)
                                        {
                                            UserInfo newUserInfo = this._accountRepository.GetUserInfoById(int.Parse(newTarget));
                                            UserInfo oldUserInfo = this._accountRepository.GetUserInfoById(int.Parse(oldTarget));
                                            var modifyRecord = new DynamicEntityModifyMapper
                                            {
                                                keyName = requKey,
                                                field = requTarget.FieldDisplay,
                                                oldVal = oldUserInfo.UserName,
                                                newVal = newUserInfo.UserName
                                            };
                                            tempData.Add(requKey, modifyRecord);

                                        }
                                        break;
                                    }
                            }


                            //check the change of productdetail

                            /*if (requKey == "productdetail")
                            {
                                DynamicEntityDetailtListMapper modeltemp = new DynamicEntityDetailtListMapper()
                                {
                                    //order detail
                                    EntityId = Guid.Parse("261ff2e5-81e0-4595-83fe-868235325f43"),
                                    RecIds = oldTarget,
                                    NeedPower = 0
                                };

                                List<IDictionary<string, object>> _theOrderDetailResult = _dynamicEntityRepository.DetailList(modeltemp, userId);
                                List<string> _oldProductIds = new List<string>();
                                foreach (var item in _theOrderDetailResult)
                                {
                                    string _id = item["product"].ToString();
                                    _oldProductIds.Add(_id);
                                }


                                DynamicEntityDetailtListMapper modeltemp2 = new DynamicEntityDetailtListMapper()
                                {
                                    //order detail
                                    EntityId = Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2"),
                                    RecIds = string.Join(",", _oldProductIds.ToArray()),
                                    NeedPower = 0
                                };

                                List<IDictionary<string, object>> _theProductDetailResult = _dynamicEntityRepository.DetailList(modeltemp2, userId);

                                List<string> _finalMessageList = new List<string>();
                                JArray _productDetailJson = JArray.Parse(newTarget);
                                List<string> _newProductids = new List<string>();
                                for (int j = 0; j < _productDetailJson.Count; j++)
                                {
                                    string _productid = _productDetailJson[j]["FieldData"]["product"].ToString();
                                    _newProductids.Add(_productid);
                                }


                                List<string> _theDeletedProductList = new List<string>();
                                List<string> _theDeletedProductIds = new List<string>();
                                for (int i = 0; i < _oldProductIds.Count; i++)
                                {
                                    string _theProductid = _oldProductIds[i];
                                    if (!_newProductids.Contains(_theProductid))
                                    {
                                        _theDeletedProductIds.Add(_theProductid);
                                    }
                                }


                                foreach (var id in _theDeletedProductIds)
                                {
                                    foreach (var item in _theProductDetailResult)
                                    {
                                        if (item["recid"].ToString() == id)
                                        {
                                            _theDeletedProductList.Add(item["description"].ToString());
                                        }
                                    }
                                }


                                string _finalDeleteMessage = string.Empty;
                                if (_theDeletedProductList.Count > 0)
                                {
                                    _finalDeleteMessage = "删除了商品:" + string.Join(",", _theDeletedProductList.ToArray());
                                    _finalMessageList.Add(_finalDeleteMessage);
                                }


                                string _productName = "";
                                List<string> _newProductList = new List<string>();
                                foreach (string _id in _newProductids)
                                {
                                    if (!_oldProductIds.Contains(_id))
                                    {
                                        // the prodcut is new add
                                        for (int j = 0; j < _productDetailJson.Count; j++)
                                        {
                                            string _productid = _productDetailJson[j]["FieldData"]["product"].ToString();
                                            if (_id == _productid)
                                            {
                                                _productName = _productDetailJson[j]["FieldData"]["productcode"].ToString();
                                                _newProductList.Add(_productName);
                                            }
                                        }
                                    }
                                }

                                string _finalNewAddProduct = string.Empty;
                                if (_newProductList.Count > 0)
                                {
                                    _finalNewAddProduct = "新增了商品:" + string.Join(",", _newProductList.ToArray());
                                    _finalMessageList.Add(_finalNewAddProduct);
                                }


                                var modifyRecord = new DynamicEntityModifyMapper
                                {
                                    keyName = requKey,
                                    fieldId = requTarget.FieldId.ToString(),
                                    field = requTarget.FieldDisplay,
                                    oldVal = "",
                                    newVal = string.Join(";", _finalMessageList.ToArray())
                                };
                                tempData.Add(requKey, modifyRecord);

                            }*/
                        }
                    }
                }
                #region --泛海三江 联系人特殊逻辑，必须把名称和电话同时显示--
                bool isModLink = false;
                string linkname = "";
                string tel = "";
                #endregion
                if (tempData.Count > 0)
                {
                    foreach (var key in tempData.Keys)
                    {
                        DynamicEntityModifyMapper modifyRecord = tempData[key];
                        _dynamicEntityRepository.inertEntityModify(modifyRecord, entityid, bussinessId, userId);

                        if (key == "productdetail")
                        {
                            msgContent = msgContent + string.Format("\n\t{0}:{1}。", modifyRecord.field, modifyRecord.newVal);
                        }
                        else
                        {
                            //泛海三江 联系人特殊逻辑，必须把名称和电话同时显示
                            if (entityid == Guid.Parse("a1486d13-061b-4d92-990a-4d93cbe58694"))
                            {
                                if (modifyRecord.fieldId == "63766245-199e-491e-88db-5832d004550d")
                                {
                                    isModLink = true;
                                    linkname = string.Format("\n\t{0}:{1}->{2}。", modifyRecord.field, modifyRecord.oldVal, modifyRecord.newVal);
                                }
                                else if (modifyRecord.fieldId == "61c2fa69-2d58-40b8-b123-e5f49f27f34d")
                                {
                                    isModLink = true;
                                    tel = string.Format("\n\t{0}:{1}->{2}。", modifyRecord.field, modifyRecord.oldVal, modifyRecord.newVal);
                                }
                                else
                                {
                                    msgContent = msgContent + string.Format("\n\t{0}:{1}->{2}。", modifyRecord.field, modifyRecord.oldVal, modifyRecord.newVal);
                                }
                            }
                            else
                            {
                                msgContent = msgContent + string.Format("\n\t{0}:{1}->{2}。", modifyRecord.field, modifyRecord.oldVal, modifyRecord.newVal);
                            }
                        }
                    }
                }

                #region --泛海三江 联系人特殊逻辑，必须把名称和电话同时显示--
                if (isModLink)
                {
                    if (string.IsNullOrEmpty(linkname))
                    {
                        //没修改
                        linkname = string.Format("\n\t{0}:{1}->{2}。", requireData["recname"].FieldDisplay, oldDetail["recname"], oldDetail["recname"]);
                    }
                    if (string.IsNullOrEmpty(tel))
                    {
                        //没修改
                        tel = string.Format("\n\t{0}:{1}->{2}。", requireData["phone"].FieldDisplay, oldDetail["phone"], oldDetail["phone"]);
                    }
                }
                msgContent = linkname + tel + msgContent;
                #endregion
            }
            catch (Exception ex)
            {
                _logger.Error("修改痕迹转换异常：" + ex.Message);
            }
            return msgContent;
        }

        private Dictionary<string, DynamicEntityModifyMapper> AddModifyRecord(Dictionary<string, DynamicEntityModifyMapper> tempData, string requKey, string fieldId, string fieldName, string oldTarget, string newTarget)
        {
            //发生改变
            if (oldTarget != newTarget)
            {
                var modifyRecord = new DynamicEntityModifyMapper
                {
                    keyName = requKey,
                    fieldId = fieldId,
                    field = fieldName,
                    oldVal = oldTarget,
                    newVal = newTarget
                };
                tempData.Add(requKey, modifyRecord);
            }
            return tempData;
        }

        private void TransferData(List<TransferTempInfo> thisDealed, int userId)
        {
            foreach (TransferTempInfo updateitem in thisDealed)
            {
                DbTransaction tran = null;
                bool updateSuccess = false;
                if (updateitem.IsMultiPersonField)
                {
                    updateSuccess = _dynamicRepository.TransferEntityData(tran, updateitem.TableName, updateitem.FieldNames, updateitem.NewUserIds, updateitem.RecId, userId);

                }
                else
                {
                    updateSuccess = _dynamicRepository.TransferEntityData(tran, updateitem.TableName, updateitem.FieldNames, updateitem.NewUserId, updateitem.RecId, userId);

                }
                if (updateSuccess)
                {
                    string msgContent = "";
                    if (updateitem.IsMultiPersonField)
                    {
                        if (updateitem.OldUserId > 0)
                        {
                            msgContent = string.Format("{0} {1} 的 {2} 已经移除{3}、加入了{4}。", updateitem.EntityName, updateitem.RecName, updateitem.FieldDisplayNames, updateitem.OldUserName, updateitem.NewUserName);
                        }
                        else
                        {
                            msgContent = string.Format("{0} {1} 的 {2} 已经加入了{3}。", updateitem.EntityName, updateitem.RecName, updateitem.FieldDisplayNames, updateitem.NewUserName);
                        }
                    }
                    else
                    {
                        msgContent = string.Format("{0} {1} 的 {2} 已经变更为 {3}。", updateitem.EntityName, updateitem.RecName, updateitem.FieldDisplayNames, updateitem.NewUserName);
                    }

                    DynamicInsertInfo dynamicInfo = new DynamicInsertInfo()
                    {
                        DynamicType = DynamicType.System,
                        EntityId = updateitem.EntityId,
                        TypeId = updateitem.TypeId,
                        BusinessId = updateitem.RecId,
                        RelEntityId = Guid.Empty,
                        RelBusinessId = Guid.Empty,
                        Content = msgContent,
                        TemplateData = "{}",
                    };
                    MsgParamInfo tempData = null;
                    bool msgSuccess = _dynamicRepository.InsertDynamic(null, dynamicInfo, userId, out tempData);
                    updateitem.IsSuccess = true;
                }
                else
                {
                    updateitem.IsSuccess = false;
                }

                updateitem.DetailInfo = null;
            }
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
                var details = _dynamicEntityRepository.DetailList(detailMapper, userNumber, transaction);

                //删除数据
                var result = _dynamicEntityRepository.Delete(transaction, dynamicModel.EntityId, dynamicModel.RecId, dynamicModel.PageType, dynamicModel.PageCode, userNumber);
                if (result.Flag == 1)
                {
                    Task.Run(() =>
                    {
                        foreach (var detail in details)
                        {
                            object rectype = detail.ContainsKey("rectype") ? detail["rectype"] : entityId;
                            typeid = Guid.Parse(rectype.ToString());
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
            DynamicEntityListMapper mapper = new DynamicEntityListMapper
            {
                EntityId = tabModel.RelEntityId,
                MenuId = null,
                ViewType = tabModel.ViewType,
                SearchOrder = string.Empty
            };
            if (sqlWhere != null && sqlWhere.StartsWith("and recid"))
            {//兼容历史
                if (SpecFuncName != null)
                {
                    sqlWhere = sqlWhere.Replace("and recid", "and t.recid");
                }
                else
                {
                    sqlWhere = sqlWhere.Replace("and recid", "and e.recid");
                    mapper.RelSql = sqlWhere;
                    sqlWhere = " 1=1 ";
                }
            }
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
                IRuleRepository ruleRepository = new RuleRepository();
                IVocationRepository vocationRepository = new VocationRepository();
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
                    var relTabRelationList = vocationRepository.GetRelTabs();
                    foreach (var tmp in relTabList)
                    {
                        string uuid = tmp["relid"].ToString();

                        if (!string.IsNullOrEmpty(uuid))
                        {
                            var functions = t.Functions.Where(b => b.EntityId == entity.EntityId && b.RecType == FunctionType.EntityTab && b.DeviceType == (int)DeviceClassic);//拿子节点对上一级 的父节点的id
                            if (functions != null && functions.Count() > 0)
                            {
                                var function = functions.SingleOrDefault();
                                functions = t.Functions.Where(a => a.ParentId == function.FuncId);
                                if (functions.Any(a => a.RelationValue == uuid))
                                {
                                    //1.检查职能可见规则
                                    //2.检查页签可见规则
                                    var hasAccess = true;
                                    var recIds = new List<Guid>();
                                    recIds.Add(entityModel.RecId);

                                    var fun = functions.Where(a => a.RelationValue == uuid).FirstOrDefault();
                                    var userData = GetUserData(userNumber, false);
                                    var sql = userData.RuleSqlFormatForFunction(fun);
                                    if (!string.IsNullOrEmpty(sql))
                                    {
                                        hasAccess = ruleRepository.HasDataAccess(null, sql, entityModel.EntityId, recIds);
                                    }
                                    if (hasAccess == true)
                                    {
                                        if (relTabRelationList != null && relTabRelationList.Any(i => i.RelTabId.ToString() == uuid))
                                        {
                                            var tab = relTabRelationList.Where(i => i.RelTabId.ToString() == uuid).FirstOrDefault();
                                            sql = userData.RuleSqlFormatForSql(tab.RuleSql);
                                            hasAccess = ruleRepository.HasDataAccess(null, sql, entityModel.EntityId, recIds);
                                        }
                                    }

                                    if (hasAccess == true)
                                    {
                                        if (!lst.Contains(tmp))
                                            lst.Add(tmp);
                                    }
                                }
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
        public OutputResult<object> SaveRelConfig(SaveRelConfigModel entityModel, int userNumber)
        {
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                if (entityModel?.RelId == null || entityModel.RelId == new Guid("00000000-0000-0000-0000-000000000000"))
                {
                    return ShowError<object>("页签id不能为空");
                }

                var configs = entityModel.Configs;
                var configSets = entityModel.ConfigSets;
                foreach (var conf in configs)
                {
                    Guid uuid = Guid.NewGuid();
                    conf.RecId = uuid;
                    if (conf.EntityRule != null)
                    {
                        conf.EntityRule.PageId = uuid;
                        var entityRule = _mapper.Map<EntityRuleMapper, EntityRule>(conf.EntityRule);
                        _translatorServices.SaveEntityRule(entityRule, entityModel.RelId, userNumber, tran);
                    }
                }

   
                var configResult = _dynamicEntityRepository.SaveRelConfig(configs, entityModel.RelId, userNumber);
                var setResult = _dynamicEntityRepository.SaveRelConfigSet(configSets, entityModel.RelId, userNumber);
                if (configResult.Flag == 1 && setResult.Flag == 1)
                {
                    tran.Commit();
                    return new OutputResult<object>(new OperateResult()
                    {
                        Flag = 1,
                        Msg = "保存配置成功"
                    });
                }
                else
                {
                    tran.Rollback();
                    return new OutputResult<object>(new OperateResult()
                    {
                        Flag = 0,
                        Msg = "保存配置失败"
                    });
                }
            }
        }

        public OutputResult<object> queryDataForDataSource(IServiceProvider serviceProvider, RelQueryDataModel queryModel, int userNum)
        {
            int errorCode = 0;
            string errorMsg = "";
            Guid tmpEntityId = Guid.Empty;
            OutputResult<object> resujlt = ExcuteSelectAction((transaction, arg, userData) =>
            {
                RelConfigInfo relConfigInfo = _dynamicEntityRepository.GetRelConfig(queryModel.RelId, userNum);
                List<RelConfig> configList = relConfigInfo.Configs;
                //4个统计结果
                Dictionary<string, List<object>> statRet = new Dictionary<string, List<object>>();
                List<object> list = new List<object>();
                statRet.Add("data", list);

                //根据条件查询结果
                Dictionary<string, object> queryRet = new Dictionary<string, object>();
                foreach (var config in configList)
                {
                    //0配置1函数2服务
                    if (config.Type == 0)
                    {
                        var queryVal = _dynamicEntityRepository.queryDataForDataSource_CalcuteType(config, queryModel.RecId, userNum);
                        queryRet.Add("q" + config.Index, queryVal);
                    }
                    else if (config.Type == 1)
                    {
                        //函数返回直接返回decimal
                        var queryVal = _dynamicEntityRepository.queryDataForDataSource_funcType(config, queryModel.RecId, userNum);
                        queryRet.Add("q" + config.Index, queryVal);
                    }
                    else
                    {
                        //服务没实现
                    }
                }
                if (relConfigInfo.ConfigSets.Count > 0)
                {
                    var configSet = relConfigInfo.ConfigSets[0];
                    for (int i = 0; i < 4; i++)
                    {
                        string expressionStr = GetModelValue("ConfigSet" + (i + 1), configSet);
                        string expressionTitleStr = GetModelValue("title" + (i + 1), configSet);
                        if (!string.IsNullOrEmpty(expressionTitleStr))
                        {
                            Dictionary<string, string> listObj = new Dictionary<string, string>();
                            if (string.IsNullOrEmpty(expressionStr))
                            {
                                listObj.Add("title", expressionTitleStr);
                                listObj.Add("value", "0");
                                list.Add(listObj);
                            }
                            else
                            {
                                foreach (var queryVal in queryRet)
                                {
                                    expressionStr = expressionStr.Replace(queryVal.Key, queryVal.Value.ToString());
                                }
                                string calcSql = string.Format(@"select round({0},4) as result", expressionStr);
                                var calcRet = this._dynamicEntityRepository.ExecuteQuery(calcSql, null).FirstOrDefault();
                                listObj.Add("title", expressionTitleStr);
                                listObj.Add("value", calcRet["result"].ToString());
                                list.Add(listObj);
                            }

                        }
                    }
                }
                return new OutputResult<object>(statRet, errorMsg, errorCode);
            }, "", tmpEntityId, userNum);
            return resujlt;
        }

        public string GetModelValue(string FieldName, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                object o = Ts.GetProperty(FieldName).GetValue(obj, null);
                string Value = Convert.ToString(o);
                if (string.IsNullOrEmpty(Value)) return null;
                return Value;
            }
            catch
            {
                return null;
            }
        }

        public OutputResult<object> GetRelConfig(RelConfigModel entityModel, int userNumber)
        {
            if (entityModel?.RelId == null || entityModel.RelId == new Guid("00000000-0000-0000-0000-000000000000"))
            {
                return ShowError<object>("页签id不能为空");
            }
            var data = _dynamicEntityRepository.GetRelConfig(entityModel.RelId, userNumber);
            foreach (var conf in data.Configs)
            {
                if (_dynamicEntityRepository.ExistsRule(conf.RecId))
                    conf.EntityRule = _ruleServices.SelectEntityRule(conf.RecId, userNumber);
            }
            return new OutputResult<object>(data);

        }

        public OutputResult<object> AddRelTab(AddRelTabModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AddRelTabModel, AddRelTabMapper>(entityModel);
            string RelName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(entity.RelName, entity.RelName_Lang, out RelName);
            if (RelName != null) entity.RelName = RelName;
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
                string RelName = "";
                MultiLanguageUtils.GetDefaultLanguageValue(entity.RelName, entity.RelName_Lang, out RelName);
                if (RelName != null) entity.RelName = RelName;
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
                string RelName = "";
                MultiLanguageUtils.GetDefaultLanguageValue(entityModel.RelName, entityModel.RelName_Lang, out RelName);
                if (RelName != null) entityModel.RelName = RelName;
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

        public bool IsValidEntityPower(SimpleEntityInfo entityInfo)
        {
            //简单实体 没有关联实体 且走审批的就不校验权限
            if (entityInfo.ModelType == EntityModelType.Simple && entityInfo.RelEntityId == null && entityInfo.RelAudit == 1)
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

        /// <summary>
        /// 查重
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="userNumber">用户id</param>
        /// <returns></returns>
        public OutputResult<object> QueryEntityCondition(EntityCondition entity)
        {
            DbTransaction tran = null;
            DynamicEntityCondition dyEntity = new DynamicEntityCondition();
            dyEntity.EntityId = entity.EntityId;
            dyEntity.Functype = entity.FuncType;
            var result = _dynamicEntityRepository.QueryEntityCondition(dyEntity, tran);
            return new OutputResult<object>(result);
        }

        /// <summary>
        /// 查重修改
        /// </summary>
        /// <param name="entity">参数对象</param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> UpdateEntityCondition(EntityCondition entity, int userNumber)
        {
            DbTransaction tran = null;
            var fieldIds = entity.FieldIds.Split(',');
            List<DynamicEntityCondition> entityList = new List<DynamicEntityCondition>();
            if (!string.IsNullOrEmpty(entity.FieldIds))
            {
                for (int i = 0; i < fieldIds.Count(); i++)
                {
                    entityList.Add(new DynamicEntityCondition
                    {
                        EntityId = entity.EntityId,
                        Fieldid = new Guid(fieldIds[i]),
                        Functype = (int)FuncType.Repeat
                    });
                }
            }
            else
            {
                entityList.Add(new DynamicEntityCondition
                {
                    EntityId = entity.EntityId,
                    Fieldid = Guid.Empty,
                    Functype = (int)FuncType.Repeat
                });
            }
            var flag = _dynamicEntityRepository.UpdateEntityCondition(entityList, userNumber, tran);
            if (flag)
                return new OutputResult<object>(null, "修改成功！", 0);
            else
                return new OutputResult<object>(null, "修改失败！", -1);

        }


        public OutputResult<object> MarkRecordComplete(Guid recId, int userNumber)
        {
            var result = _dynamicEntityRepository.MarkRecordComplete(recId, userNumber);
            return HandleResult(result);

        }

        public OutputResult<object> TemporarySave(TemporarySaveModel model, int userNumber)
        {
            DbTransaction tran = null;
            var falg = false;
            var eneity = _mapper.Map<TemporarySaveModel, TemporarySaveMapper>(model);
            if (_dynamicEntityRepository.ExistsData(model.CacheId, userNumber, tran))
                falg = _dynamicEntityRepository.AddTemporaryData(eneity, userNumber, tran);
            else
                falg = _dynamicEntityRepository.UpdateTemporaryData(eneity, userNumber, tran);
            if (falg)
                return new OutputResult<object>(null, "保存成功");
            else
                return new OutputResult<object>(null, "保存失败", 1);
        }

        public OutputResult<object> SelectTemporaryDetails(Guid cacheId, int userNumber)
        {
            DbTransaction tran = null;
            var result = _dynamicEntityRepository.SelectTemporaryDetails(cacheId, userNumber, tran);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> DeleteTemporaryList(List<Guid> cacheIds, int userNumber)
        {
            DbTransaction tran = null;
            bool falg = false;
            falg = _dynamicEntityRepository.DeleteTemporaryList(cacheIds, userNumber, tran);
            if (falg)
                return new OutputResult<object>(null, "删除成功");
            else
                return new OutputResult<object>(null, "删除失败", 1);
        }

        public OutputResult<object> SimpleEdit(DynamicEntityEditModel dynamicModel, int userNumber)
        {
            var dynamicEntity = _mapper.Map<DynamicEntityEditModel, DynamicEntityEditMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }

            return ExcuteUpdateAction((transaction, arg, userData) =>
            {
                //验证通过后，插入数据
                var result = _dynamicEntityRepository.DynamicEdit(transaction, dynamicEntity.TypeId, dynamicEntity.RecId, dynamicEntity.FieldData, userNumber);
                return HandleResult(result);

            }, dynamicModel, dynamicEntity.TypeId, userNumber, new List<Guid>() { dynamicEntity.RecId });
        }

        public Dictionary<string, object> getEntityBaseInfoByTypeId(Guid typeId)
        {
            return _dynamicEntityRepository.getEntityBaseInfoByTypeId(typeId, 1, null);
        }
        public Dictionary<string, object> getEntityBaseInfoByEntityId(Guid entityId)
        {
            return _dynamicEntityRepository.getEntityBaseInfoById(entityId, 1, null);
        }
        public Dictionary<string, object> getEntityBaseInfoByCaseId(Guid entityId)
        {
            return _dynamicEntityRepository.getEntityBaseInfoByCaseId(entityId, 1, null);
        }
    }
    public class MuleSendParamInfo
    {
        public Guid[] RecIds { get; set; }

    }
    public class TransferTempInfo
    {
        public IDictionary<string, object> DetailInfo { get; set; }
        public Guid EntityId { get; set; }
        public Guid TypeId { get; set; }
        public Guid RecId { get; set; }
        public string TableName { get; set; }
        public string EntityName { get; set; }
        public string Message { get; set; }
        public int NewUserId { get; set; }
        public List<string> FieldNames { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string NewUserName { get; set; }
        public string NewUserIds { get; set; }

        public string FieldDisplayNames { get; set; }
        public string RecName { get; set; }
        public bool IsMultiPersonField { get; set; }
        public int OldUserId { get; set; }
        public string OldUserName { get; set; }


    }
}
