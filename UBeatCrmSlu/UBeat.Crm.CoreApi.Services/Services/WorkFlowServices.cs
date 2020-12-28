

using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.WorkFlow;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Version;
using System.Data.Common;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.DomainModel.Message;
using System.Linq;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.DomainModel.Rule;
using System.Text.RegularExpressions;
using UBeat.Crm.CoreApi.Core.Utility;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Services.Models.SoapErp;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class WorkFlowServices : EntityBaseServices
    {
        private readonly IWorkFlowRepository _workFlowRepository;
        private readonly IRuleRepository _ruleRepository;
        private readonly IMapper _mapper;
        private readonly IEntityProRepository _entityProRepository;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly RuleTranslatorServices _ruleTranslatorServices;
        private readonly IAccountRepository _accountRepository;
        private NLog.ILogger _logger = NLog.LogManager.GetLogger("UBeat.Crm.CoreApi.Services.Services.WorkFlowServices");
        public WorkFlowServices(IMapper mapper, IWorkFlowRepository workFlowRepository, IRuleRepository ruleRepository, IEntityProRepository entityProRepository, IDynamicEntityRepository dynamicEntityRepository, IDynamicRepository dynamicRepository, DynamicEntityServices dynamicEntityServices, IAccountRepository accountRepository, RuleTranslatorServices ruleTranslatorServices)
        {
            _workFlowRepository = workFlowRepository;
            _entityProRepository = entityProRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _dynamicRepository = dynamicRepository;
            _mapper = mapper;
            _ruleRepository = ruleRepository;
            _dynamicEntityServices = dynamicEntityServices;
            _accountRepository = accountRepository;
            _ruleTranslatorServices = ruleTranslatorServices;
        }

        public OutputResult<object> CaseDetail(CaseDetailModel detailModel, int userNumber)
        {
            var result = new CaseDetailDataModel();

            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    #region --获取 casedetail--
                    //获取流程数据信息
                    var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, detailModel.CaseId);
                    if (caseInfo == null)
                        throw new Exception("流程数据不存在");
                    //获取流程配置信息
                    var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
                    if (workflowInfo == null)
                        throw new Exception("流程配置不存在");
                    var copyusersInfo = _workFlowRepository.GetWorkFlowCopyUser(caseInfo.CaseId);
                    Guid RealEntityId = caseInfo.EntityId;
                    result.CaseDetail = new WorkFlowCaseInfoExt()
                    {
                        CaseId = caseInfo.CaseId,
                        FlowId = caseInfo.FlowId,
                        RecId = caseInfo.RecId,
                        EntityId = caseInfo.EntityId,
                        RelEntityId = caseInfo.RelEntityId,
                        RelRecId = caseInfo.RelRecId,
                        AuditStatus = caseInfo.AuditStatus,
                        RecCode = caseInfo.RecCode,
                        NodeNum = caseInfo.NodeNum,
                        RecCreated = caseInfo.RecCreated,
                        RecUpdated = caseInfo.RecUpdated,
                        RecCreator = caseInfo.RecCreator,
                        RecUpdator = caseInfo.RecUpdator,
                        Recstatus = caseInfo.Recstatus,
                        FlowName = workflowInfo.FlowName,
                        BackFlag = workflowInfo.BackFlag,
                        RecCreator_Name = caseInfo.RecCreator_Name,
                        CopyUser = copyusersInfo,
                        IsAllowTransfer = workflowInfo.IsAllowTransfer,
                        IsAllowSign = workflowInfo.IsAllowSign,
                        IsNeedToRepeatApprove = workflowInfo.IsNeedToRepeatApprove,
                        RecName = caseInfo.RecName,
                        VerNum = caseInfo.VerNum
                    };
                    #endregion

                    #region --获取 nodeauditinfo，包含 caseoperate--
                    //获取当前审批的实例item
                    var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
                    if (caseitems == null || caseitems.Count == 0)
                    {
                        throw new Exception("流程节点数据异常");
                    }

                    result.CaseItem = new CaseItemAuditInfo();
                    Guid caseItemId = Guid.Empty;
                    var caseItemList = _workFlowRepository.CaseItemList(caseInfo.CaseId, userNumber);
                    var lastCaseItem = _workFlowRepository.CaseItemList(caseInfo.CaseId, userNumber).LastOrDefault(t => t["handleuser"].ToString() == userNumber.ToString() && (t["choicestatus"].ToString() == "4" || t["choicestatus"].ToString() == "6"));
                    if (lastCaseItem != null)
                    {
                        var username = lastCaseItem.ContainsKey("username") ? lastCaseItem["username"] : "";
                        var casestatus = lastCaseItem.ContainsKey("casestatus") ? lastCaseItem["casestatus"] : "";
                        result.CaseItem.AuditStatus = string.Format("{0}{1}", username, caseItemList.LastOrDefault()["casestatus"]);
                        result.CaseItem.NodeType = Convert.ToInt32(lastCaseItem["nodetype"].ToString());
                        result.CaseItem.CaseItemId = Guid.Parse(lastCaseItem["caseitemid"].ToString());
                    }
                    else
                        caseItemId = caseitems.LastOrDefault().CaseItemId;
                    var notfinishitems = caseitems.Where(m => m.CaseStatus == CaseStatusType.Readed || m.CaseStatus == CaseStatusType.WaitApproval);
                    if (notfinishitems.Count() > 0)
                    {
                        string temptext = notfinishitems.Count() > 2 ? string.Join(",", notfinishitems.Select(m => m.HandleUserName).ToArray(), 0, 2) + "等" : string.Join(",", notfinishitems.Select(m => m.HandleUserName).ToArray());
                        result.CaseItem.AuditStep = string.Format("等待{0}处理审批", temptext);
                    }

                    var nowcaseitem = caseitems.Find(m => m.HandleUser == userNumber && (m.CaseStatus == CaseStatusType.WaitApproval || m.CaseStatus == CaseStatusType.Readed) && (m.ChoiceStatus == ChoiceStatusType.AddNode || m.ChoiceStatus == ChoiceStatusType.Edit));

                    if (nowcaseitem != null && (nowcaseitem.ChoiceStatus == ChoiceStatusType.Edit || nowcaseitem.ChoiceStatus == ChoiceStatusType.AddNode))
                    {
                        if (caseInfo.NodeNum == -1 || caseInfo.AuditStatus == AuditStatusType.Finished || caseInfo.AuditStatus == AuditStatusType.NotAllowed)
                        {
                            result.CaseItem.NodeName = "已完成审批";
                            if (caseInfo.AuditStatus == AuditStatusType.Finished)
                                result.CaseItem.AuditStep = "审批通过";
                            else if (caseInfo.AuditStatus == AuditStatusType.NotAllowed)
                                result.CaseItem.AuditStep = "审批不通过";
                        }
                        else
                        {

                            string nodeName = string.Empty;
                            if (workflowInfo.FlowType == WorkFlowType.FreeFlow)
                            {
                                nodeName = "自由流程";
                            }
                            else
                            {
                                var nodeid = caseitems.FirstOrDefault().NodeId;
                                var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);
                                if (flowNodeInfo == null)
                                    throw new Exception("不存在有效节点");
                                else nodeName = flowNodeInfo.NodeName;

                                result.CaseItem.NodeId = nodeid;
                                result.CaseItem.ColumnConfig = flowNodeInfo.ColumnConfig;
                            }

                            result.CaseItem.NodeName = nodeName;

                            if (caseInfo.NodeNum == 0)//如果处于第一个节点
                            {
                                var stepNum = caseitems.FirstOrDefault().StepNum;
                                result.CaseItem.IsCanLunch = 1;
                                //如果审批关联的实体为简单实体且简单实体无关联的独立实体时，则允许编辑审批信息重新提交或者中止审批
                                //如果审批关联的实体为独立实体或关联的简单实体有关联的独立实体时，则不允许编辑审批信息，只能中止审批
                                if (caseInfo.RecCreator == userNumber)
                                {
                                    result.CaseItem.IsCanTerminate = 1;
                                    result.CaseItem.IsCanEdit = _workFlowRepository.CanEditWorkFlowCase(workflowInfo, userNumber, tran) ? 1 : 0;
                                    if (stepNum > 1)
                                    {
                                        result.CaseItem.IsCanLunch = result.CaseItem.IsCanEdit;
                                    }
                                }

                            }
                            else
                            {
                                result.CaseItem.IsCanAllow = 1;
                                result.CaseItem.IsCanReject = 1;
                                result.CaseItem.IsCanReback = workflowInfo.BackFlag;
                                if (workflowInfo.Entrance == 1)
                                {
                                    result.CaseItem.IsCanLunch = 1;
                                }
                            }
                            var currentNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nowcaseitem.NodeId);
                            if (currentNodeInfo != null)
                            {
                                var json = JObject.Parse(currentNodeInfo.ColumnConfig.ToString());
                                if (json["stepfieldtype"] != null)
                                {
                                    if (json["stepfieldtype"].ToString() == "1" || json["stepfieldtype"].ToString() == "2")
                                    {
                                        result.CaseItem.IsCanEdit = 0;
                                    }
                                    else
                                    {
                                        result.CaseItem.IsCanEdit = 1;
                                    }
                                }
                            }
                        }
                    }
                    if (result.CaseItem.IsCanLunch == 1)
                    {
                        var items = _workFlowRepository.CaseItemList(caseInfo.CaseId, userNumber, tran: tran);
                        if (items.Count - 2 > 0 && items[items.Count - 2]["nodetype"].ToString() == "0")
                        {
                            if (items[items.Count - 2]["isrejectnode"] != null)
                            {
                                result.CaseItem.RejectCaseItemId = items[items.Count - 2]["isrejectnode"].ToString() == "1" ? Guid.Parse(items[items.Count - 2]["caseitemid"].ToString()) : new Nullable<Guid>();
                            }
                        }
                    }
                    if (result.CaseItem.CaseItemId != Guid.Empty)
                    {
                        var sign = _workFlowRepository.GetWorkFlowSign(result.CaseItem.CaseItemId, userNumber);
                        if (sign != null && sign.Count > 0)
                        {
                            if (sign.Count == 1)
                            {
                                result.CaseItem.OriginalUserId = sign.FirstOrDefault().OriginalUserId;
                                result.CaseItem.SignStatus = sign.FirstOrDefault().SignStatus;
                                result.CaseItem.SignCount = 1;
                            }
                            else
                            {
                                if (sign.FirstOrDefault().OriginalUserId == sign.LastOrDefault().UserId)
                                {
                                    result.CaseItem.OriginalUserId = sign.FirstOrDefault().OriginalUserId;
                                    result.CaseItem.SignStatus = 0;
                                    result.CaseItem.SignCount = 0;
                                }
                                else
                                {
                                    result.CaseItem.OriginalUserId = sign.FirstOrDefault().OriginalUserId;
                                    result.CaseItem.SignStatus = sign.LastOrDefault().SignStatus;
                                    result.CaseItem.SignCount = 1;
                                }
                            }
                        }
                    }
                    if (caseItemId != Guid.Empty)
                    {
                        var sign = _workFlowRepository.GetWorkFlowSign(caseItemId, userNumber);
                        if (sign != null && sign.Count > 0)
                        {
                            if (sign.Count == 1)
                            {
                                result.CaseItem.OriginalUserId = sign.FirstOrDefault().OriginalUserId;
                                result.CaseItem.SignStatus = sign.FirstOrDefault().SignStatus;
                                result.CaseItem.SignCount = 1;
                            }
                        }
                    }
                    if (caseItemId != Guid.Empty)
                    {
                        var sign = _workFlowRepository.GetWorkFlowSign(caseItemId, userNumber);
                        if (sign != null && sign.Count > 0)
                        {
                            if (sign.Count == 1)
                            {
                                result.CaseItem.OriginalUserId = sign.FirstOrDefault().OriginalUserId;
                                result.CaseItem.SignStatus = sign.FirstOrDefault().SignStatus;
                                result.CaseItem.SignCount = 1;
                            }
                            else
                            {
                                if (sign.FirstOrDefault().OriginalUserId == sign.LastOrDefault().UserId)
                                {
                                    result.CaseItem.OriginalUserId = sign.FirstOrDefault().OriginalUserId;
                                    result.CaseItem.SignStatus = sign.FirstOrDefault().SignStatus;
                                    result.CaseItem.SignCount = 0;
                                }
                                else
                                {
                                    result.CaseItem.OriginalUserId = sign.FirstOrDefault().OriginalUserId;
                                    result.CaseItem.SignStatus = 2;
                                    result.CaseItem.SignCount = 1;
                                }
                            }
                        }
                    }

                    #endregion

                    var dynamicEntityServices = dynamicCreateService("UBeat.Crm.CoreApi.Services.Services.DynamicEntityServices", false) as DynamicEntityServices;

                    #region --获取 entitydetail--
                    //获取 entitydetail
                    var detailMapper = new DynamicEntityDetailtMapper()
                    {
                        EntityId = RealEntityId,
                        RecId = caseInfo.RecId,
                        NeedPower = 0
                    };
                    var detail = _dynamicEntityRepository.Detail(detailMapper, userNumber, tran);
                    var fields = _dynamicEntityRepository.GetTypeFields(RealEntityId, (int)DynamicProtocolOperateType.Detail, userNumber);

                    #endregion

                    #region --获取 relatedetail--
                    //获取 relatedetail
                    if (caseInfo.RelEntityId != Guid.Empty && caseInfo.RelRecId != Guid.Empty)
                    {
                        var reldetailMapper = new DynamicEntityDetailtMapper()
                        {
                            EntityId = caseInfo.RelEntityId,
                            RecId = caseInfo.RelRecId,
                            NeedPower = 0
                        };
                        var detailtemp = _dynamicEntityRepository.Detail(reldetailMapper, userNumber, tran);
                        if (detailtemp != null)
                        {
                            var relEntityFields = _dynamicEntityRepository.GetEntityFields(caseInfo.RelEntityId, userNumber);
                            var entityInfo = _dynamicEntityRepository.getEntityBaseInfoById(caseInfo.EntityId, userNumber);
                            if (entityInfo["relfieldid"] != null)
                            {
                                var relField = relEntityFields.FirstOrDefault(t => t.FieldId == Guid.Parse(entityInfo["relfieldid"].ToString()));
                                if (relField != null && !string.IsNullOrEmpty(relField.FieldName) && detailtemp[relField.FieldName] != null && entityInfo["relfieldid"] != null && entityInfo["relfieldname"] != null)
                                {
                                    if (!detail.ContainsKey(entityInfo["relfieldname"].ToString()))
                                    {
                                        detail.Add(entityInfo["relfieldname"].ToString(), detailtemp[relField.FieldName]);
                                        if (detailtemp.ContainsKey(relField.FieldName + "_name"))
                                        {
                                            if (!detail.ContainsKey(entityInfo["relfieldname"].ToString() + "_name"))
                                            {
                                                detail.Add(entityInfo["relfieldname"].ToString() + "_name", detailtemp[relField.FieldName + "_name"]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        result.RelateDetail = dynamicEntityServices.DealLinkTableFields(new List<IDictionary<string, object>>() { detailtemp }, reldetailMapper.EntityId, userNumber, tran).FirstOrDefault();
                    }
                    result.EntityDetail = dynamicEntityServices.DealLinkTableFields(new List<IDictionary<string, object>>() { detail }, detailMapper.EntityId, userNumber,tran).FirstOrDefault();

                    #endregion

                    _workFlowRepository.SetWorkFlowCaseItemReaded(tran, caseInfo.CaseId, caseInfo.NodeNum, userNumber);

                    tran.Commit();
                }

                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return new OutputResult<object>(result);
        }

        public Dictionary<string, object> GetWorkFlowIdByEntityId(Guid entityId, int userId)
        {
            DbTransaction tran = null;
            Dictionary<string, object> ret = this._workFlowRepository.GetWorkflowByEntityId(null, entityId, userId);
            return ret;

        }

        public OutputResult<object> SaveTitleConfig(WorkFlowTitleConfigModel paramInfo, int userId)
        {
            return new OutputResult<object>(_workFlowRepository.SaveTitleConfig(paramInfo.FlowId, paramInfo.TitleConfig, userId));
        }

        public OutputResult<object> GetTitleFieldList(WorkFlowDetailModel paramInfo, int userId)
        {
            try
            {
                List<string> fields = new List<string>();

                GetWorkflowFieldForWorkflowTitle(fields);
                Dictionary<string, List<IDictionary<string, object>>> detailResult = _workFlowRepository.Detail(paramInfo.FlowId, userId);
                if (detailResult == null) return new OutputResult<object>("无法找到工作流定义", "无法找到工作流定义", -1);
                List<IDictionary<string, object>> workflowlist = detailResult["data"];
                if (workflowlist.Count == 0)
                {
                    return new OutputResult<object>("无法找到工作流定义", "无法找到工作流定义", -1);
                }
                IDictionary<string, object> workflowInfo = workflowlist[0];
                Guid entityid = Guid.Parse(workflowInfo["entityid"].ToString());
                if (entityid == Guid.Empty) return new OutputResult<object>("无法找到工作流定义", "无法找到工作流定义", -1);
                IDictionary<string, object> entityInfo = this._entityProRepository.GetEntityInfo(entityid, userId);
                if (entityInfo == null) return new OutputResult<object>("无法找到工作流绑定的实体定义信息", "无法找到工作流绑定的实体定义信息", -1);
                int modeltype = int.Parse(entityInfo["modeltype"].ToString());
                if (modeltype == 0)
                {
                    //独立实体,只需要考虑本实体信息
                    GetEntityFieldForWorkflowTitle(fields, entityid, entityInfo["entityname"].ToString(), userId);

                }
                else if (modeltype == 2)
                {
                    //简单实体，需要判断是否存在关联实体
                    GetEntityFieldForWorkflowTitle(fields, entityid, entityInfo["entityname"].ToString(), userId);
                    if (entityInfo.ContainsKey("relentityid") && entityInfo["relentityid"] != null)
                    {
                        Guid mainEntityId = Guid.Empty;
                        if (Guid.TryParse(entityInfo["relentityid"].ToString(), out mainEntityId))
                        {
                            IDictionary<string, object> mainEntityInfo = this._entityProRepository.GetEntityInfo(entityid, userId);
                            if (mainEntityInfo != null)
                            {
                                GetEntityFieldForWorkflowTitle(fields, mainEntityId, mainEntityInfo["entityname"].ToString(), userId);
                            }
                        }
                    }
                }
                else if (modeltype == 3)
                {
                    //动态实体 ,需要判断是否存在关联实体
                    GetEntityFieldForWorkflowTitle(fields, entityid, entityInfo["entityname"].ToString(), userId);
                    if (entityInfo.ContainsKey("relentityid") && entityInfo["relentityid"] != null)
                    {
                        Guid mainEntityId = Guid.Empty;
                        if (Guid.TryParse(entityInfo["relentityid"].ToString(), out mainEntityId))
                        {
                            IDictionary<string, object> mainEntityInfo = this._entityProRepository.GetEntityInfo(mainEntityId, userId);
                            if (mainEntityInfo != null)
                            {
                                GetEntityFieldForWorkflowTitle(fields, mainEntityId, mainEntityInfo["entityname"].ToString(), userId);
                            }
                        }
                    }
                }
                else
                {
                    return new OutputResult<object>("无法找到工作流绑定的实体定义信息", "无法找到工作流绑定的实体定义信息", -1);
                }
                return new OutputResult<object>(fields);
            }
            catch (Exception ex)
            {
                return new OutputResult<object>(ex.Message, ex.Message, -1);
            }
        }
        private void GetEntityFieldForWorkflowTitle(List<string> result, Guid entityid, string entityName, int userid)
        {

            Dictionary<string, List<IDictionary<string, object>>> fieldDetail = this._entityProRepository.EntityFieldProQuery(entityid.ToString(), userid);
            if (fieldDetail == null) throw (new Exception("获取字段名称失败"));
            List<IDictionary<string, object>> fields = fieldDetail["EntityFieldPros"];
            foreach (IDictionary<string, object> field in fields)
            {
                int controltype = 0;
                if (int.TryParse(field["controltype"].ToString(), out controltype) == false) continue;
                switch (controltype)
                {
                    case (int)DynamicProtocolControlType.Address:
                    case (int)DynamicProtocolControlType.DataSourceMulti:
                    case (int)DynamicProtocolControlType.DataSourceSingle:
                    case (int)DynamicProtocolControlType.Department:
                    case (int)DynamicProtocolControlType.EmailAddr:
                    case (int)DynamicProtocolControlType.Location:
                    case (int)DynamicProtocolControlType.NumberDecimal:
                    case (int)DynamicProtocolControlType.NumberInt:
                    case (int)DynamicProtocolControlType.PersonSelectMulti:
                    case (int)DynamicProtocolControlType.PersonSelectSingle:
                    case (int)DynamicProtocolControlType.PhoneNum:
                    case (int)DynamicProtocolControlType.Product:
                    case (int)DynamicProtocolControlType.ProductSet:
                    case (int)DynamicProtocolControlType.QuoteControl:
                    case (int)DynamicProtocolControlType.RecCreated:
                    case (int)DynamicProtocolControlType.RecCreator:
                    case (int)DynamicProtocolControlType.RecId:
                    case (int)DynamicProtocolControlType.RecManager:
                    case (int)DynamicProtocolControlType.RecOnlive:
                    case (int)DynamicProtocolControlType.RecName:
                    case (int)DynamicProtocolControlType.RecUpdated:
                    case (int)DynamicProtocolControlType.RecUpdator:
                    case (int)DynamicProtocolControlType.RelateControl:
                    case (int)DynamicProtocolControlType.SalesStage:
                    case (int)DynamicProtocolControlType.SelectMulti:
                    case (int)DynamicProtocolControlType.SelectSingle:
                    case (int)DynamicProtocolControlType.Text:
                    case (int)DynamicProtocolControlType.TimeDate:
                    case (int)DynamicProtocolControlType.TimeStamp:
                    case (int)DynamicProtocolControlType.AreaRegion:
                        result.Add(entityName + "." + field["displayname"].ToString());
                        break;

                }
            }
        }
        private void GetWorkflowFieldForWorkflowTitle(List<string> result)
        {
            result.Add("工作流.工作流名称");
            result.Add("工作流.发起时间");
            result.Add("工作流.发起人");
        }
        #region --AddCase--（new）

        #region --流程预提交--
        public OutputResult<object> PreAddWorkflowCase(WorkFlowCaseAddModel caseModel, UserInfo userinfo)
        {
            bool isCommit = false;
            if (caseModel == null)
            {
                throw new Exception("参数不可为空");
            }
            var result = new NextNodeDataModel();
            var caseInfo = new WorkFlowCaseInfo();
            var workflowInfo = new WorkFlowInfo();
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {

                    if (caseModel.DataType == 0)
                    {
                        var entityInfo = _entityProRepository.GetEntityInfo(caseModel.EntityModel.TypeId);
                        UserData userData = GetUserData(userinfo.UserId);

                        WorkFlowAddCaseModel workFlowAddCaseModel = null;
                        if (entityInfo.ModelType == EntityModelType.Dynamic)
                        {
                            if (!caseModel.EntityModel.FieldData.ContainsKey("recrelateid"))
                            {
                                caseModel.EntityModel.FieldData.Add("recrelateid", caseModel.EntityModel.RelRecId);
                            }
                        }
                        var entityResult = _dynamicEntityServices.AddEntityData(tran, userData, entityInfo, caseModel.EntityModel, header, userinfo.UserId, out workFlowAddCaseModel);
                        if (entityResult.Status != 0)
                        {
                            return entityResult;
                        }
                        caseModel.CaseModel = workFlowAddCaseModel;
                    }
                    if (caseModel.CaseModel == null)
                    {
                        throw new Exception("流程数据不可为空");
                    }
                    workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseModel.CaseModel.FlowId);
                    if (workflowInfo == null)
                        throw new Exception("流程配置不存在,请先配置审批节点");
                    WorkFlowNodeInfo firstNodeInfo = null;
                    var caseid = AddWorkFlowCase(true, tran, caseModel, workflowInfo, userinfo, out firstNodeInfo);
                    //走完审批所有操作，获取下一步数据
                    caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseid);
                    LoopInfo loopInfo;
                    var users = LoopApproveUsers(caseInfo, workflowInfo, firstNodeInfo, tran, userinfo, out loopInfo);
                    if (!(isCommit = loopInfo.IsNoneApproverUser))
                        result = GetNextNodeData(tran, caseInfo, workflowInfo, firstNodeInfo, userinfo);
                    while (isCommit)
                    {
                        var caseItemList = _workFlowRepository.CaseItemList(caseid, userinfo.UserId, tran: tran);
                        var data = SubmitPretreatAudit(new WorkFlowAuditCaseItemModel
                        {
                            CaseId = caseid,
                            ChoiceStatus = 1,
                            NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString())
                        }, userinfo, tran);
                        NextNodeDataModel model = data.DataBody as NextNodeDataModel;
                        if (model.NodeInfo.StepTypeId == NodeStepType.End)
                        {
                            result = model;
                            break;
                        }
                        if (!model.NodeInfo.IsSkip)
                            break;
                    }

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    //这是预处理操作，获取到结果后不需要提交事务，直接全部回滚
                    if (tran.Connection != null)
                        tran.Rollback();
                    conn.Close();
                    conn.Dispose();
                }
            }
            return new OutputResult<object>(result);
        }

        #endregion

        #region --跳过流程发起提交--
        public OutputResult<object> AddWorkflowCase(WorkFlowCaseAddModel caseModel, UserInfo userinfo)
        {
            if (caseModel == null)
            {
                throw new Exception("参数不可为空");
            }
            int isNeedToSendMsg = 0;//0代表不发消息 1代表是跳过流程的时候用handleuser来替换当前用户 2 当前用户
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    if (!string.IsNullOrEmpty(caseModel.CacheId))
                    {
                        Guid g = Guid.Parse(caseModel.CacheId);
                        if (!(_dynamicEntityRepository.ExistsData(g, userinfo.UserId, tran)))
                            _dynamicEntityRepository.DeleteTemporary(g, userinfo.UserId, tran);
                    }
                    if (caseModel.DataType == 0)
                    {
                        var entityInfo = _entityProRepository.GetEntityInfo(caseModel.EntityModel.TypeId);
                        UserData userData = GetUserData(userinfo.UserId);

                        WorkFlowAddCaseModel workFlowAddCaseModel = null;
                        var entityResult = _dynamicEntityServices.AddEntityData(tran, userData, entityInfo, caseModel.EntityModel, header, userinfo.UserId, out workFlowAddCaseModel);
                        if (entityResult.Status != 0)
                        {
                            return entityResult;
                        }
                        caseModel.CaseModel = workFlowAddCaseModel;
                    }
                    if (caseModel.CaseModel == null)
                    {
                        throw new Exception("流程数据不可为空");
                    }
                    var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseModel.CaseModel.FlowId);
                    WorkFlowNodeInfo firstNodeInfo = null;
                    Guid caseid;
                    if (caseModel.NodeId.HasValue)
                    {
                        var workFlowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, caseModel.NodeId.Value);
                        if (workFlowNodeInfo.StepTypeId == NodeStepType.End)
                        {
                            caseid = AddWorkFlowCase(false, tran, caseModel, workflowInfo, userinfo, out firstNodeInfo, isNeedToSendMsg);
                            _workFlowRepository.AuditWorkFlowCase(caseid, AuditStatusType.Finished, -1, userinfo.UserId, tran);
                            tran.Commit();
                            return new OutputResult<object>();
                        }
                    }
                    if (workflowInfo == null)
                        throw new Exception("流程配置不存在");
                    caseid = AddWorkFlowCase(false, tran, caseModel, workflowInfo, userinfo, out firstNodeInfo, isNeedToSendMsg);
                    tran.Commit();
                    canWriteCaseMessage = true;
                    WriteCaseAuditMessage(caseid, 0, 0, userinfo.UserId, type: -1);
                    return new OutputResult<object>(caseid);
                }
                catch (Exception ex)
                {
                    if (tran.Connection != null)
                        tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        #endregion

        #region 根据定义，获取工作流的标题的定义，并根据已有的数据，进行相关的替换
        private string GenerateWorkflowCaseTitle(DbTransaction tran, WorkFlowCaseAddModel caseModel, WorkFlowInfo workflowInfo, UserInfo userinfo)
        {
            if (workflowInfo == null || workflowInfo.TitleConfig == null || workflowInfo.TitleConfig.Length == 0) return workflowInfo.FlowName;///默认情况的处理
			Dictionary<string, string> ValueDict = new Dictionary<string, string>();
            GenerateWorkflowTitleValueDict(ValueDict, caseModel, workflowInfo, userinfo);
            GenerateEntityTitleValueDict(ValueDict, tran, caseModel.CaseModel.EntityId, caseModel.CaseModel.RecId, userinfo.UserId);
            if (caseModel.CaseModel.RelEntityId != null && caseModel.CaseModel.RelEntityId != Guid.Empty
                 && caseModel.CaseModel.RelRecId != null && caseModel.CaseModel.RelRecId != Guid.Empty && caseModel.CaseModel.RelEntityId != caseModel.CaseModel.EntityId)
            {
                GenerateEntityTitleValueDict(ValueDict, tran, (System.Guid)caseModel.CaseModel.RelEntityId, (System.Guid)caseModel.CaseModel.RelRecId, userinfo.UserId);
            }
            string title = workflowInfo.TitleConfig;
            foreach (string key in ValueDict.Keys)
            {
                title = title.Replace("{" + key + "}", ValueDict[key]);
            }
            //把没有在字典的值也清空
            string pattern = @"(?<=\{)[^}]*(?=\})";
            string replacement = "";
            title = Regex.Replace(title, pattern, replacement);
            return title;
        }
        private void GenerateWorkflowTitleValueDict(Dictionary<string, string> dict, WorkFlowCaseAddModel caseModel, WorkFlowInfo workflowInfo, UserInfo userinfo)
        {

            dict.Add("工作流.工作流名称", workflowInfo.FlowName);

            dict.Add("工作流.发起时间", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            dict.Add("工作流.发起人", userinfo.UserName);
        }
        private void GenerateEntityTitleValueDict(Dictionary<string, string> dict, DbTransaction tran, Guid entityId, Guid recid, int usserId)
        {

            IDictionary<string, object> entityInfo = this._entityProRepository.GetEntityInfo(entityId, usserId);
            string entityname = entityInfo["entityname"].ToString();
            Dictionary<string, List<IDictionary<string, object>>> fieldDetail = this._entityProRepository.EntityFieldProQuery(entityId.ToString(), usserId);
            List<IDictionary<string, object>> fields = fieldDetail["EntityFieldPros"];
            DynamicEntityDetailtMapper dynamicEntityDetailtMapper = new DynamicEntityDetailtMapper()
            {
                EntityId = entityId,
                RecId = recid
            };
            IDictionary<string, object> detailInfo = this._dynamicEntityRepository.Detail(dynamicEntityDetailtMapper, usserId, tran);
            if (detailInfo == null)
            {
                return;
            }
            foreach (IDictionary<string, object> field in fields)
            {
                int controltype = 0;
                if (int.TryParse(field["controltype"].ToString(), out controltype) == false) continue;
                string displayname = field["displayname"].ToString();
                string fieldname = field["fieldname"].ToString();
                string key = entityname + "." + displayname;
                bool found = false;
                switch (controltype)
                {
                    case (int)DynamicProtocolControlType.Address:
                    case (int)DynamicProtocolControlType.DataSourceMulti:
                    case (int)DynamicProtocolControlType.DataSourceSingle:
                    case (int)DynamicProtocolControlType.Department:
                    case (int)DynamicProtocolControlType.Location:
                    case (int)DynamicProtocolControlType.PersonSelectMulti:
                    case (int)DynamicProtocolControlType.PersonSelectSingle:
                    case (int)DynamicProtocolControlType.Product:
                    case (int)DynamicProtocolControlType.ProductSet:
                    case (int)DynamicProtocolControlType.RecCreator:
                    case (int)DynamicProtocolControlType.RecUpdator:
                    case (int)DynamicProtocolControlType.RelateControl:
                    case (int)DynamicProtocolControlType.SalesStage:
                    case (int)DynamicProtocolControlType.SelectMulti:
                    case (int)DynamicProtocolControlType.SelectSingle:
                    case (int)DynamicProtocolControlType.QuoteControl:
                    case (int)DynamicProtocolControlType.RecManager:
                    case (int)DynamicProtocolControlType.AreaRegion:
                        if (detailInfo.ContainsKey(fieldname + "_name") && detailInfo[fieldname + "_name"] != null)
                        {
                            dict.Add(key, detailInfo[fieldname + "_name"].ToString());
                            found = true;
                        }
                        break;
                    case (int)DynamicProtocolControlType.EmailAddr:
                    case (int)DynamicProtocolControlType.RecId:
                    case (int)DynamicProtocolControlType.PhoneNum:
                    case (int)DynamicProtocolControlType.RecName:
                    case (int)DynamicProtocolControlType.Text:

                        if (detailInfo.ContainsKey(fieldname) && detailInfo[fieldname] != null)
                        {
                            dict.Add(key, detailInfo[fieldname].ToString());
                            found = true;
                        }
                        break;
                    case (int)DynamicProtocolControlType.NumberDecimal:
                    case (int)DynamicProtocolControlType.NumberInt:
                        if (detailInfo.ContainsKey(fieldname) && detailInfo[fieldname] != null)
                        {
                            dict.Add(key, detailInfo[fieldname].ToString());
                            found = true;
                        }
                        break;
                    case (int)DynamicProtocolControlType.RecCreated:
                    case (int)DynamicProtocolControlType.RecOnlive:
                    case (int)DynamicProtocolControlType.RecUpdated:
                    case (int)DynamicProtocolControlType.TimeDate:
                    case (int)DynamicProtocolControlType.TimeStamp:
                        if (detailInfo.ContainsKey(fieldname) && detailInfo[fieldname] != null)
                        {
                            dict.Add(key, detailInfo[fieldname].ToString());
                            found = true;
                        }
                        break;

                }
                if (found == false) dict.Add(key, "");//未赋值的清空
            }
        }
        #endregion

        #region --新增流程case数据--

        private Guid AddWorkFlowCase(bool ispresubmit, DbTransaction tran, WorkFlowCaseAddModel caseModel, WorkFlowInfo workflowInfo, UserInfo userinfo, out WorkFlowNodeInfo firstNodeInfo, int isNeedToSendMsg = 0)
        {
            var caseEntity = _mapper.Map<WorkFlowAddCaseModel, WorkFlowAddCaseMapper>(caseModel.CaseModel);
            if (caseEntity == null || !caseEntity.IsValid())
            {
                throw new Exception(caseEntity.ValidationState.Errors.First());
            }
            caseEntity.Title = GenerateWorkflowCaseTitle(tran, caseModel, workflowInfo, userinfo);
            var newcaseid = _workFlowRepository.AddWorkflowCase(tran, workflowInfo, caseEntity, userinfo.UserId);
            if (newcaseid == Guid.Empty)
            {
                throw new Exception("流程新增失败");
            }
            Guid firstNodeId = Guid.Empty;
            Guid lastNodeId = Guid.Empty;
            firstNodeInfo = null;
            if (workflowInfo.FlowType == WorkFlowType.FreeFlow)
            {
                firstNodeId = freeFlowBeginNodeId;
                lastNodeId = freeFlowEndNodeId;
            }
            else
            {
                var nodes = _workFlowRepository.GetNodeInfoList(tran, workflowInfo.FlowId, workflowInfo.VerNum);
                if (nodes.Count == 0)
                    throw new Exception("该流程为固定流程，请先配置流程节点");
                firstNodeInfo = nodes.Find(m => m.StepTypeId == NodeStepType.Launch);
                if (firstNodeInfo == null)
                    throw new Exception("缺少发起流程节点");
                firstNodeId = firstNodeInfo.NodeId;
                if (workflowInfo.SkipFlag == 1)
                {
                    var lastNode = nodes.Find(m => m.StepTypeId == NodeStepType.End);
                    if (lastNode == null)
                        throw new Exception("缺少结束流程节点");
                    lastNodeId = lastNode.NodeId;
                }
            }
            //添加第一个审批节点item
            var item = new WorkFlowCaseItemInfo()
            {
                CaseItemId = Guid.NewGuid(),
                CaseId = newcaseid,
                NodeId = firstNodeId,
                NodeNum = 0,
                StepNum = 0,
                ChoiceStatus = ChoiceStatusType.Edit,
                CaseStatus = CaseStatusType.WaitApproval,
                HandleUser = userinfo.UserId,
                Casedata = caseEntity.CaseData
            };
            var success = _workFlowRepository.AddCaseItem(new List<WorkFlowCaseItemInfo>() { item }, userinfo.UserId, AuditStatusType.Begin, tran);
            if (!success)
            {
                throw new Exception("添加发起节点失败");
            }
            if (workflowInfo.SkipFlag == 0)
            {
                if (ispresubmit)
                {
                    //判断是否有附加函数_event_func
                    var eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, firstNodeId, tran);
                    _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, newcaseid, 0, 4, userinfo.UserId, tran);
                }
                else//如果不是预提交，则需要检查是否插入下一审批人节点
                {
                    var caseItemModel = new WorkFlowAuditCaseItemModel()
                    {
                        CaseId = newcaseid,
                        NodeNum = 0,
                        CaseData = caseEntity.CaseData,
                        ChoiceStatus = 4,
                        NodeId = caseModel.NodeId,
                        HandleUser = caseModel.HandleUser,
                        CopyUser = caseModel.CopyUser
                    };
                    SubmitWorkFlowAudit(caseItemModel, userinfo, tran, isNeedToSendMsg, isLaunchNode: firstNodeInfo.StepTypeId == NodeStepType.Launch ? 1 : 0);
                }
            }
            else //如果流程跳过标志为1，则直接跳转到结束
            {
                //判断是否有附加函数_event_func
                var eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, firstNodeId, tran);
                _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, newcaseid, 0, 4, userinfo.UserId, tran);
                //添加第一个审批节点item
                var lastitem = new WorkFlowCaseItemInfo()
                {
                    CaseItemId = Guid.NewGuid(),
                    CaseId = newcaseid,
                    NodeId = lastNodeId,
                    NodeNum = -1,
                    StepNum = 1,
                    ChoiceStatus = ChoiceStatusType.EndPoint,
                    CaseStatus = CaseStatusType.Approved,
                    HandleUser = userinfo.UserId
                };
                success = _workFlowRepository.AddCaseItem(new List<WorkFlowCaseItemInfo>() { lastitem }, userinfo.UserId, AuditStatusType.Finished, tran);
                if (!success)
                {
                    throw new Exception("添加跳过流程结束节点失败");
                }
                //判断是否有附加函数_event_func
                eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, lastNodeId, tran);
                _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, newcaseid, -1, 1, userinfo.UserId, tran);

                if (!ispresubmit)//如果正式提交成功，则发动态消息
                {
                    Task.Run(() =>
                    {
                        for (int i = 0; i < 20; i++)
                        {
                            if (canWriteCaseMessage) break;
                            try
                            {
                                System.Threading.Thread.Sleep(100);
                            }
                            catch (Exception ex)
                            {
                            }
                        }

                        var entityInfo = _entityProRepository.GetEntityInfo(caseEntity.EntityId);
                        var detailMapper = new DynamicEntityDetailtMapper()
                        {
                            EntityId = caseEntity.EntityId,
                            RecId = caseEntity.RecId,
                            NeedPower = 0
                        };

                        if (entityInfo.ModelType == EntityModelType.Dynamic)
                        {
                            detailMapper.EntityId = entityInfo.RelEntityId.GetValueOrDefault();
                            detailMapper.RecId = caseEntity.RelRecId.GetValueOrDefault();
                        }
                        var olddetail = _dynamicEntityRepository.Detail(detailMapper, userinfo.UserId);
                        WriteAddCaseMessage(entityInfo, caseEntity.RecId, caseEntity.RelRecId.GetValueOrDefault(), caseEntity.FlowId, newcaseid, userinfo.UserId, olddetail);
                        canWriteCaseMessage = false;
                    });
                }
            }

            return newcaseid;
        }

        #endregion



        #endregion



        public OutputResult<object> SaveWorkflowRule(WorkFlowRuleSaveParamInfo paramInfo, int userId)
        {
            DbTransaction tran = null;
            if (paramInfo.Rule == null)
            {
                paramInfo.Rule = new Models.Vocation.RuleContent();
            }
            paramInfo.Rule.EntityId = paramInfo.EntityId;
            string ruletext = Newtonsoft.Json.JsonConvert.SerializeObject(paramInfo.Rule);
            string ruleitemtext = Newtonsoft.Json.JsonConvert.SerializeObject(paramInfo.RuleItems);
            string ruleSetText = Newtonsoft.Json.JsonConvert.SerializeObject(paramInfo.RuleSet);
            if (paramInfo.Rule.RuleId == null || paramInfo.Rule.RuleId == Guid.Empty)
            {
                //走新增模式
                OperateResult result = this._ruleRepository.SaveRuleWithoutRelation(null, ruletext, ruleitemtext, ruleSetText, userId);
                if (result != null && result.Id != null && result.Id.Length > 0)
                {
                    this._workFlowRepository.SaveWorkflowRuleRelation(result.Id, paramInfo.WorkflowId, userId, tran);
                }
            }
            else
            {
                //走修改模式
                this._ruleRepository.SaveRuleWithoutRelation(paramInfo.Rule.RuleId.ToString(), ruletext, ruleitemtext, ruleSetText, userId);
            }
            return new OutputResult<object>("ok");
        }

        public OutputResult<object> GetRules(WorkFlowRuleQueryParamInfo paramInfo, int userId)
        {
            DbTransaction tran = null;
            WorkFlowInfo workFlowInfo = this._workFlowRepository.GetWorkFlowInfo(tran, paramInfo.FlowId);
            if (workFlowInfo == null || workFlowInfo.Entityid == null || workFlowInfo.Entityid == Guid.Empty)
            {
                return new OutputResult<object>(null, "无法配置", -1);
            }
            Guid ruleid = this._workFlowRepository.getWorkflowRuleId(paramInfo.FlowId, userId, tran);
            List<RuleDataInfo> rules = null;
            if (!(ruleid == null || ruleid == Guid.Empty))
            {
                rules = this._ruleRepository.GetRule(ruleid, userId, tran);
            }
            Dictionary<string, object> retData = new Dictionary<string, object>();
            retData.Add("ruleid", ruleid);
            retData.Add("flowinfo", workFlowInfo);
            if (rules != null && rules.Count > 0)
            {
                retData.Add("rulename", rules[0].RuleName);
                retData.Add("ruleitems", rules[0].RuleItems);
                retData.Add("ruleset", rules[0].RuleSet);
            }
            List<Dictionary<string, object>> retList = new List<Dictionary<string, object>>();
            retList.Add(retData);
            return new OutputResult<object>(retList);
        }


        /// <summary>
        /// 写入添加流程的消息
        /// </summary>
        private void WriteAddCaseMessage(SimpleEntityInfo entityInfo, Guid bussinessId, Guid relbussinessId, Guid flowId, Guid caseId, int userNumber, IDictionary<string, object> olddetail)
        {

            //获取casedetail
            WorkFlowCaseInfo caseInfo = null;
            for (int i = 0; i < 10; i++)
            {
                caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(null, caseId);
                if (caseInfo != null) break;
                try
                {
                    System.Threading.Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                }
            }
            if (caseInfo == null)
            {
                _logger.Error("写工作流动态及消息时发生异常：无法获取工作流节点的信息");
                return;
            }
            WorkFlowInfo workflowInfo = null;
            for (int i = 0; i < 20; i++)
            {
                workflowInfo = _workFlowRepository.GetWorkFlowInfo(null, caseInfo.FlowId);
                if (workflowInfo != null) break;
                try
                {
                    System.Threading.Thread.Sleep(200);
                }
                catch (Exception ex)
                {
                }
            }
            if (workflowInfo == null)
            {
                _logger.Error("写工作流动态及消息时发生异常：无法获取工作流的信息");
                return;
            }
            if (workflowInfo.SkipFlag == 1)
            {

                var detailMapper = new DynamicEntityDetailtMapper()
                {
                    EntityId = entityInfo.EntityId,
                    RecId = caseInfo.RecId,
                    NeedPower = 0
                };

                string msgpParam = null;
                if (entityInfo.ModelType == EntityModelType.Dynamic)
                {
                    detailMapper.EntityId = entityInfo.RelEntityId.GetValueOrDefault();
                    detailMapper.RecId = caseInfo.RelRecId;
                    msgpParam = _dynamicRepository.GetDynamicTemplateData(caseInfo.RecId, entityInfo.EntityId, entityInfo.CategoryId, userNumber);
                }

                var detail = _dynamicEntityRepository.Detail(detailMapper, userNumber);
                var newMembers = MessageService.GetEntityMember(detail as Dictionary<string, object>);
                EntityMemberModel oldMembers = null;
                if (olddetail != null)
                {
                    oldMembers = MessageService.GetEntityMember(olddetail as Dictionary<string, object>);
                }

                string dynamicFuncode = "EntityDynamicAdd";
                var dynamicMsg = MessageService.GetEntityMsgParameter(entityInfo, caseInfo.RecId, caseInfo.RelRecId, dynamicFuncode, userNumber, newMembers, oldMembers, msgpParam, flowId);

                MessageService.WriteMessage(null, dynamicMsg, userNumber);
            }
        }




        public OutputResult<object> GetNextNodeData(WorkFlowNextNodeModel caseModel, int userNumber)
        {
            //获取该实体分类的字段
            if (caseModel.CaseId == null)
            {
                return ShowError<object>("流程明细ID不能为空");
            }
            var result = new List<NextNodeDataModel>();
            //var users = new List<NextNodeApproverInfo>();
            NextNodeDataInfo nodetemp = new NextNodeDataInfo();
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    //获取流程数据信息
                    var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseModel.CaseId);
                    if (caseInfo == null)
                        throw new Exception("流程数据不存在");
                    //获取流程配置信息
                    var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
                    if (workflowInfo == null)
                        throw new Exception("流程配置不存在");

                    //自由流程
                    if (workflowInfo.FlowType == WorkFlowType.FreeFlow)
                    {
                        nodetemp.NodeId = null;
                        nodetemp.FlowType = WorkFlowType.FreeFlow;
                        nodetemp.NodeName = "自由选择审批人";
                        nodetemp.NodeType = NodeType.Normal;
                        nodetemp.NodeNum = 1;
                        nodetemp.NodeState = 0;
                        //nodetemp.StepTypeId = NodeStepType.SelectByUser;
                        if (caseInfo.NodeNum == -1)//审批已经结束
                        {
                            nodetemp.NodeNum = -1;
                            nodetemp.NodeState = -1;
                        }
                        var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, Guid.Empty, userNumber, workflowInfo.FlowType, tran);
                        result.Add(new NextNodeDataModel()
                        {
                            NodeInfo = nodetemp,
                            Approvers = users
                        });
                    }
                    else //固定流程
                    {
                        //获取当前审批的实例item
                        var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
                        if (caseitems == null || caseitems.Count == 0)
                        {
                            throw new Exception("流程节点数据异常");
                        }
                        var nodeid = caseitems.FirstOrDefault().NodeId;
                        var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);
                        if (flowNodeInfo == null)
                        {
                            throw new Exception("不存在有效节点");
                        }
                        nodetemp.NodeId = flowNodeInfo.NodeId;
                        nodetemp.FlowType = WorkFlowType.FixedFlow;
                        nodetemp.NodeName = flowNodeInfo.NodeName;
                        nodetemp.NodeType = flowNodeInfo.NodeType;
                        nodetemp.NodeNum = caseInfo.NodeNum;
                        nodetemp.NodeState = 0;
                        //nodetemp.StepTypeId = flowNodeInfo.StepTypeId;
                        if (caseInfo.NodeNum == -1)//审批已经结束
                        {
                            nodetemp.NodeState = -1;
                        }
                        else if (flowNodeInfo.NodeType == NodeType.Joint)//会审
                        {
                            //会审审批通过的节点数
                            var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();
                            nodetemp.NeedSuccAuditCount = flowNodeInfo.AuditSucc - aproval_success_count;
                            if (aproval_success_count < flowNodeInfo.AuditSucc)//--说明当前节点，其他人还在审批，不能进入下一步
                            {
                                nodetemp.NodeState = 1;
                            }
                        }
                        else if (flowNodeInfo.NodeType == NodeType.SpecialJoint)//特殊的会审 pxf
                        {
                            //会审审批通过的节点数
                            var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();
                            nodetemp.NeedSuccAuditCount = flowNodeInfo.AuditSucc - aproval_success_count;
                            if (aproval_success_count < flowNodeInfo.AuditSucc)//--说明当前节点，其他人还在审批，不能进入下一步
                            {
                                nodetemp.NodeState = 1;
                            }
                        }
                        //检查下一点，获取下一节点信息
                        if (nodetemp.NodeState == 0)
                        {
                            //获取下一节点
                            var nextnodes = _workFlowRepository.GetNextNodeDataInfoList(caseInfo.FlowId, nodeid, caseInfo.VerNum, tran);
                            if (nextnodes == null || nextnodes.Count == 0)
                                throw new Exception("获取不到节点配置");

                            foreach (var m in nextnodes)
                            {
                                m.NodeState = m.StepTypeId == NodeStepType.End ? 2 : 0; //如果下一节点为结束审批节点，说明当前审批节点到达审批的最后节点
                                m.NodeNum = caseInfo.NodeNum;
                                m.NeedSuccAuditCount = 1;
                                m.FlowType = WorkFlowType.FixedFlow;
                                var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, m.NodeId.GetValueOrDefault(), userNumber, workflowInfo.FlowType, tran);
                                result.Add(new NextNodeDataModel()
                                {
                                    NodeInfo = m,
                                    Approvers = users
                                });
                            }
                        }
                        else
                        {
                            result.Add(new NextNodeDataModel()
                            {
                                NodeInfo = nodetemp,

                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return new OutputResult<object>(result);
        }


        #region --审批预处理--
        //自由流程，uuid值为0作为流程起点，值为1作为流程终点'，值为2作为流程过程节点';
        Guid freeFlowBeginNodeId = new Guid("00000000-0000-0000-0000-000000000000");
        Guid freeFlowEndNodeId = new Guid("00000000-0000-0000-0000-000000000001");
        Guid freeFlowNodeId = new Guid("00000000-0000-0000-0000-000000000002");

        #region --提交审批预处理--
        /// <summary>
        /// 提交审批预处理
        /// </summary>
        /// <param name="caseItemModel"></param>
        /// <param name="userinfo"></param>
        /// <returns></returns>
        public OutputResult<object> SubmitPretreatAudit(WorkFlowAuditCaseItemModel caseItemModel, UserInfo userinfo, DbTransaction tran = null, int operatetype = 0)
        {
            bool isDisposed = false;
            NextNodeDataModel result = new NextNodeDataModel();
            //获取该实体分类的字段
            var caseItemEntity = _mapper.Map<WorkFlowAuditCaseItemModel, WorkFlowAuditCaseItemMapper>(caseItemModel);
            if (caseItemEntity == null || !caseItemEntity.IsValid())
            {
                return HandleValid(caseItemEntity);
            }
            string message = string.Empty;
            int status = 0;
            bool casefinish = false;
            int stepnum = 0;
            Guid nodeid = Guid.Empty;
            WorkFlowNodeInfo flowNodeInfo = null;
            DbConnection conn = null;
            if (tran != null)
                conn = tran.Connection;
            else
            {
                conn = GetDbConnect();
                conn.Open();
                tran = conn.BeginTransaction();
            }

            try
            {
                //获取casedetail
                var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseItemEntity.CaseId);
                if (caseInfo == null)
                    throw new Exception("流程表单数据不存在");
                var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
                if (workflowInfo == null)
                    throw new Exception("流程配置不存在");
                var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
                if (caseitems == null || caseitems.Count == 0)
                    throw new Exception("流程节点不存在");
                caseitems = caseitems.OrderByDescending(m => m.StepNum).ToList();

                if (caseItemEntity.ChoiceStatus == 2 && caseItemEntity.NodeNum == 0)
                {
                    throw new Exception("第一个节点不可退回");
                }
                if (caseitems.FirstOrDefault().NodeType == 2)
                {
                    caseItemModel.JointStatus = caseItemEntity.ChoiceStatus == -1 ? 1 : 0;
                    caseItemEntity.JointStatus = caseItemEntity.ChoiceStatus == -1 ? 1 : 0;
                    caseItemEntity.ChoiceStatus = 1;
                    caseItemModel.ChoiceStatus = 1;
                }

                stepnum = caseitems.FirstOrDefault().StepNum;

                //若非分支流程，则直接返回下一步处理人数据
                //若是分支流程，则通过预处理数据获取下一步分支节点，并返回下一步处理人数据
                if (workflowInfo.FlowType == WorkFlowType.FreeFlow)//自由流程
                {
                    bool canAddNextNode = false;
                    var nowcaseitem = caseitems.FirstOrDefault();
                    AuditFreeFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, nowcaseitem, out canAddNextNode);
                    if (casefinish)
                    {
                        //自由流程，uuid值为0作为流程起点，值为1作为流程终点'，值为2作为流程过程节点';
                        nodeid = freeFlowEndNodeId;
                    }
                    else if (caseItemEntity.ChoiceStatus == 2)//0拒绝 1通过 2退回 3中止 4编辑 5审批结束 6节点新增
                    {
                        nodeid = freeFlowBeginNodeId;
                        canAddNextNode = false;
                    }
                    else
                    {
                        nodeid = freeFlowNodeId;
                    }
                }
                else //固定流程
                {
                    nodeid = caseitems.FirstOrDefault().NodeId;
                    flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);
                    if (flowNodeInfo == null)
                        throw new Exception("流程配置不存在");

                    bool hasNextNode = true;
                    //获取下一步节点，
                    var flowNextNodeInfos = _workFlowRepository.GetNextNodeInfoList(tran, caseInfo.FlowId, caseInfo.VerNum, nodeid);
                    if (flowNextNodeInfos == null || flowNextNodeInfos.Count == 0 || (flowNextNodeInfos.Count == 1 && flowNextNodeInfos.FirstOrDefault().StepTypeId == NodeStepType.End))
                    {
                        hasNextNode = false;
                    }
                    //执行审批逻辑
                    if (flowNodeInfo.NodeType == NodeType.Normal)//普通审批
                    {
                        var nowcaseitem = caseitems.FirstOrDefault();
                        AuditNormalFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, nowcaseitem, hasNextNode);
                    }
                    else if (flowNodeInfo.NodeType == NodeType.Joint)  //会审
                    {
                        var nowcaseitem = caseitems.Find(m => m.HandleUser == userinfo.UserId);
                        if (nowcaseitem == null)
                            throw new Exception("您没有审批当前节点的权限");
                        AuditJoinFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, flowNodeInfo, nowcaseitem, hasNextNode);
                    }
                    else //特殊会审 pxf
                    {
                        var nowcaseitem = caseitems.Find(m => m.HandleUser == userinfo.UserId);
                        if (nowcaseitem == null)
                            throw new Exception("您没有审批当前节点的权限");
                        AuditJoinFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, flowNodeInfo, nowcaseitem, hasNextNode);
                    }
                }

                //流程审批过程修改实体字段时，更新关联实体的字段数据
                _workFlowRepository.ExecuteUpdateWorkFlowEntity(caseInfo.CaseId, caseInfo.NodeNum, userinfo.UserId, tran);

                //判断是否有附加函数_event_func
                var eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, nodeid, tran);
                _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, caseInfo.NodeNum, caseItemEntity.ChoiceStatus, userinfo.UserId, tran);

                //走完审批所有操作，获取下一步数据
                result = GetNextNodeData(tran, caseInfo, workflowInfo, flowNodeInfo, userinfo);
                //这是预处理操作，获取到结果后不需要提交事务，直接全部回滚
                if (result.NodeInfo.NodeType != NodeType.Joint && result.NodeInfo.NodeType != NodeType.SpecialJoint && result.NodeInfo.NodeState == 0 && (caseItemEntity.ChoiceStatus == 1 || caseItemEntity.ChoiceStatus == 4))
                {
                    if ((result.Approvers == null || result.Approvers.Count == 0) && result.NodeInfo.NotFound == 2)
                    {
                        try
                        {
                            while (true)
                            {
                                var caseItemList = _workFlowRepository.CaseItemList(caseInfo.CaseId, userinfo.UserId, -1, tran);
                                var data = SubmitPretreatAuditHelp(new WorkFlowAuditCaseItemModel
                                {
                                    CaseId = caseInfo.CaseId,
                                    ChoiceStatus = caseItemList[caseItemList.Count - 1]["nodenum"].ToString() == "-1" ? 5 : 1,
                                    NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString())
                                }, userinfo, tran);
                                NextNodeDataModel node = data.DataBody as NextNodeDataModel;
                                if (node.NotFound != 2 && node.Approvers != null && node.Approvers.Count > 0)
                                {
                                    result = node;
                                    break;
                                }
                                if (node.NodeInfo.StepTypeId == NodeStepType.End)
                                {
                                    result = node;
                                    break;
                                }
                                caseItemList = _workFlowRepository.CaseItemList(caseInfo.CaseId, userinfo.UserId, -1, tran);
                                SubmitWorkFlowAuditHelp(new WorkFlowAuditCaseItemModel
                                {
                                    CaseId = caseInfo.CaseId,
                                    ChoiceStatus = caseItemList[caseItemList.Count - 1]["nodenum"].ToString() == "-1" ? 5 : 1,
                                    CopyUser = caseItemModel.CopyUser,
                                    HandleUser = userinfo.UserId.ToString(),
                                    NodeId = node.NodeInfo.NodeId,
                                    NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString()),
                                    Suggest = "",
                                    SkipNode = 1
                                }, userinfo, tran, 0);
                            }
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                    }
                }
                bool isBreak = false;
                while (true && operatetype == 0 && (caseItemEntity.ChoiceStatus == 1 || caseItemEntity.ChoiceStatus == 4))
                {
                    this.PreInterceptWorkFlow(caseItemModel, caseInfo, workflowInfo.VerNum, workflowInfo.FlowId, tran, userinfo, out result, out isBreak);
                    if (isBreak)
                        break;
                }

                if (result.NodeInfo.StepTypeId == NodeStepType.End && caseItemEntity.ChoiceStatus != 0)
                {
                    if (caseItemEntity.ChoiceStatus == 1)
                    {
                        var type = typeof(SoapServices);
                        var instance = ServiceLocator.Current.GetInstance<SoapServices>();
                        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(m => !m.IsSpecialName);
                        if (methods != null)
                        {
                            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
                            var erpSync = config.GetSection("ERPSync").Get<List<ErpSyncFunc>>().FirstOrDefault(t => t.EntityId == workflowInfo.Entityid.ToString() && t.FlowId == workflowInfo.FlowId.ToString());
                            if (erpSync != null)
                            {
                                if (erpSync.IsFlow == 1)
                                {
                                    var method = methods.FirstOrDefault(t => t.Name == erpSync.FuncName);
                                    var data = method.Invoke(instance, new object[] { workflowInfo.Entityid, caseInfo.CaseId, caseInfo.RecId, userinfo.UserId, tran });
                                    var dataResult = data as OperateResult;
                                    if (dataResult.Flag == 0)
                                    {
                                        message = dataResult.Msg;
                                    }
                                }
                            }
                        }
                        MessageService.UpdateWorkflowNodeMessage(tran, caseInfo.RecId, caseInfo.CaseId, caseInfo.StepNum, caseItemEntity.ChoiceStatus, userinfo.UserId);
                        tran.Commit();
                        WriteCaseAuditMessage(caseInfo.CaseId, caseInfo.NodeNum, stepnum, userinfo.UserId, type: 5);
                        if (!string.IsNullOrEmpty(message))
                        {
                            var reminder = ServiceLocator.Current.GetInstance<ReminderServices>();
                            var detail = _dynamicEntityRepository.Detail(new DynamicEntityDetailtMapper { RecId = caseInfo.RecId, EntityId = caseInfo.EntityId, NeedPower = 0 }, userinfo.UserId, null);
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("reminderid", "392aba4a-2a31-4824-813d-e6c26d909e80");
                            dic.Add("busrecid", JObject.Parse(detail["belongcust"].ToString())["id"]);
                            dic.Add("message", message);
                            reminder.QrtExecuteTaskNow(dic);
                        }
                    }
                    if (operatetype == 0)
                    {
                        if (tran != null && tran.Connection != null)
                            tran.Rollback();
                    }
                    isDisposed = true;
                }
            }
            catch (Exception ex)
            {
                if (!isDisposed)
                    tran.Rollback();
                throw ex;
            }
            finally
            {
                if (operatetype == 0)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return new OutputResult<object>(result, message, status);
        }
        public OutputResult<object> SubmitPretreatAuditHelp(WorkFlowAuditCaseItemModel caseItemModel, UserInfo userinfo, DbTransaction trans)
        {
            NextNodeDataModel result = new NextNodeDataModel();
            //获取该实体分类的字段
            var caseItemEntity = _mapper.Map<WorkFlowAuditCaseItemModel, WorkFlowAuditCaseItemMapper>(caseItemModel);
            if (caseItemEntity == null || !caseItemEntity.IsValid())
            {
                return HandleValid(caseItemEntity);
            }
            bool casefinish = false;
            int stepnum = 0;
            Guid nodeid = Guid.Empty;
            WorkFlowNodeInfo flowNodeInfo = null;

            try
            {

                //获取casedetail
                var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(trans, caseItemEntity.CaseId);
                if (caseInfo == null)
                    throw new Exception("流程表单数据不存在");
                var workflowInfo = _workFlowRepository.GetWorkFlowInfo(trans, caseInfo.FlowId);
                if (workflowInfo == null)
                    throw new Exception("流程配置不存在");
                var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(trans, caseInfo.CaseId, caseInfo.NodeNum);
                if (caseitems == null || caseitems.Count == 0)
                    throw new Exception("流程节点不存在");
                caseitems = caseitems.OrderByDescending(m => m.StepNum).ToList();

                if (caseItemEntity.ChoiceStatus == 2 && caseItemEntity.NodeNum == 0)
                {
                    throw new Exception("第一个节点不可退回");
                }

                stepnum = caseitems.FirstOrDefault().StepNum;

                //若非分支流程，则直接返回下一步处理人数据
                //若是分支流程，则通过预处理数据获取下一步分支节点，并返回下一步处理人数据
                if (workflowInfo.FlowType == WorkFlowType.FreeFlow)//自由流程
                {
                    bool canAddNextNode = false;
                    var nowcaseitem = caseitems.FirstOrDefault();
                    AuditFreeFlow(userinfo, caseItemEntity, ref casefinish, trans, caseInfo, nowcaseitem, out canAddNextNode);
                    if (casefinish)
                    {
                        //自由流程，uuid值为0作为流程起点，值为1作为流程终点'，值为2作为流程过程节点';
                        nodeid = freeFlowEndNodeId;
                    }
                    else if (caseItemEntity.ChoiceStatus == 2)//0拒绝 1通过 2退回 3中止 4编辑 5审批结束 6节点新增
                    {
                        nodeid = freeFlowBeginNodeId;
                        canAddNextNode = false;
                    }
                    else
                    {
                        nodeid = freeFlowNodeId;
                    }
                }
                else //固定流程
                {
                    nodeid = caseitems.FirstOrDefault().NodeId;
                    flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(trans, nodeid);
                    if (flowNodeInfo == null)
                        throw new Exception("流程配置不存在");

                    bool hasNextNode = true;
                    //获取下一步节点，
                    var flowNextNodeInfos = _workFlowRepository.GetNextNodeInfoList(trans, caseInfo.FlowId, caseInfo.VerNum, nodeid);
                    if (flowNextNodeInfos == null || flowNextNodeInfos.Count == 0 || (flowNextNodeInfos.Count == 1 && flowNextNodeInfos.FirstOrDefault().StepTypeId == NodeStepType.End))
                    {
                        hasNextNode = false;
                    }
                    //执行审批逻辑
                    if (flowNodeInfo.NodeType == NodeType.Normal)//普通审批
                    {
                        var nowcaseitem = caseitems.FirstOrDefault();
                        AuditNormalFlow(userinfo, caseItemEntity, ref casefinish, trans, caseInfo, nowcaseitem, hasNextNode);
                    }
                    else if (flowNodeInfo.NodeType == NodeType.Joint)  //会审
                    {
                        var nowcaseitem = caseitems.Find(m => m.HandleUser == userinfo.UserId);
                        if (nowcaseitem == null)
                            throw new Exception("您没有审批当前节点的权限");
                        AuditJoinFlow(userinfo, caseItemEntity, ref casefinish, trans, caseInfo, flowNodeInfo, nowcaseitem, hasNextNode);
                    }
                    else  //特殊会审 pxf
                    {
                        var nowcaseitem = caseitems.Find(m => m.HandleUser == userinfo.UserId);
                        if (nowcaseitem == null)
                            throw new Exception("您没有审批当前节点的权限");
                        AuditJoinFlow(userinfo, caseItemEntity, ref casefinish, trans, caseInfo, flowNodeInfo, nowcaseitem, hasNextNode);
                    }
                }

                //流程审批过程修改实体字段时，更新关联实体的字段数据
                _workFlowRepository.ExecuteUpdateWorkFlowEntity(caseInfo.CaseId, caseInfo.NodeNum, userinfo.UserId, trans);

                //判断是否有附加函数_event_func
                var eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, nodeid, trans);
                _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, caseInfo.NodeNum, caseItemEntity.ChoiceStatus, userinfo.UserId, trans);

                //走完审批所有操作，获取下一步数据
                result = GetNextNodeData(trans, caseInfo, workflowInfo, flowNodeInfo, userinfo);
                //这是预处理操作，获取到结果后不需要提交事务，直接全部回滚
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return new OutputResult<object>(result);
        }
        #endregion

        #region --获取预处理后下一步审批人数据--
        public NextNodeDataModel GetNextNodeData(DbTransaction tran, WorkFlowCaseInfo caseInfo, WorkFlowInfo workflowInfo, WorkFlowNodeInfo flowNodeInfo, UserInfo userinfo)
        {
            var result = new NextNodeDataModel();

            if (workflowInfo.SkipFlag == 1)//如果是跳过流程，则直接返回
            {
                result.NodeInfo = new NextNodeDataInfo()
                {
                    NodeName = "跳过流程",
                    NodeNum = -1,
                    NodeState = -1
                };
                return result;
            }
            NextNodeDataInfo nodetemp = new NextNodeDataInfo();
            //获取流程数据信息
            var newcaseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseInfo.CaseId);
            var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
            if (caseitems == null || caseitems.Count == 0)
            {
                throw new Exception("流程节点数据异常");
            }
            var mycaseitems = caseitems.Find(m => m.HandleUser == userinfo.UserId);

            //自由流程
            if (workflowInfo.FlowType == WorkFlowType.FreeFlow)
            {
                nodetemp.NodeId = freeFlowNodeId;
                nodetemp.FlowType = WorkFlowType.FreeFlow;
                nodetemp.NodeName = "自由选择审批人";
                nodetemp.NodeType = NodeType.Normal;
                nodetemp.NodeNum = caseInfo.NodeNum == -1 ? -1 : 1;
                nodetemp.NodeState = caseInfo.NodeNum == -1 ? -1 : 0;
                nodetemp.StepTypeId = NodeStepType.SelectByUser;
                if (newcaseInfo.NodeNum == -1)//预审批审批结束，表明到达最后节点
                {
                    nodetemp.NodeState = 2;
                }
                else if (newcaseInfo.NodeNum == 0 && mycaseitems != null && mycaseitems.ChoiceStatus == ChoiceStatusType.Reback)//预审批审批回到第一节点，表明被退回
                {
                    nodetemp.NodeState = 3;
                }
                var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, Guid.Empty, userinfo.UserId, workflowInfo.FlowType, tran);
                result = new NextNodeDataModel()
                {
                    NodeInfo = nodetemp,
                    Approvers = users
                };
            }

            else //固定流程
            {

                var nodeid = caseitems.FirstOrDefault().NodeId;

                if (flowNodeInfo == null)
                {
                    throw new Exception("不存在有效节点");
                }
                nodetemp.NodeId = flowNodeInfo.NodeId;
                nodetemp.FlowType = WorkFlowType.FixedFlow;
                nodetemp.NodeName = flowNodeInfo.NodeName;
                nodetemp.NodeType = flowNodeInfo.NodeType;
                nodetemp.NodeNum = caseInfo.NodeNum == -1 ? -1 : caseInfo.NodeNum;
                nodetemp.NodeState = caseInfo.NodeNum == -1 ? -1 : 0;
                nodetemp.StepTypeId = flowNodeInfo.StepTypeId;
                nodetemp.StepCPTypeId = flowNodeInfo.StepCPTypeId;
                nodetemp.NotFound = flowNodeInfo.NotFound;
                if (newcaseInfo.NodeNum == -1 || flowNodeInfo.IsEnd)//预审批审批结束，表明到达最后节点
                {
                    nodetemp.NodeState = 2;
                }
                else if (newcaseInfo.NodeNum == 0 && mycaseitems != null && mycaseitems.ChoiceStatus == ChoiceStatusType.Reback)//预审批审批回到第一节点，表明被退回
                {
                    nodetemp.NodeState = 3;
                }
                else
                {

                    if (flowNodeInfo.NodeType == NodeType.Joint)//会审
                    {
                        //会审审批通过的节点数
                        var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();
                        nodetemp.NeedSuccAuditCount = flowNodeInfo.AuditSucc - aproval_success_count;
                        if (aproval_success_count < flowNodeInfo.AuditSucc)//--说明当前节点，其他人还在审批，不能进入下一步
                        {
                            nodetemp.NodeState = 1;
                        }
                    }
                    else if (flowNodeInfo.NodeType == NodeType.SpecialJoint)// 特殊会审 pxf
                    {
                        //会审审批通过的节点数
                        var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();
                        nodetemp.NeedSuccAuditCount = flowNodeInfo.AuditSucc - aproval_success_count;
                        if (aproval_success_count < flowNodeInfo.AuditSucc)//--说明当前节点，其他人还在审批，不能进入下一步
                        {
                            nodetemp.NodeState = 1;
                        }
                    }
                }
                //检查下一点，获取下一节点信息
                if (nodetemp.NodeState == 0)
                {
                    //获取下一节点
                    var nextnodes = _workFlowRepository.GetNextNodeDataInfoList(caseInfo.FlowId, nodeid, caseInfo.VerNum, tran);
                    if (nextnodes == null || nextnodes.Count == 0)
                        throw new Exception("获取不到节点配置");

                    if (nextnodes.Count == 1 && !flowNodeInfo.IsSkip)
                    {
                        nodetemp = nextnodes.FirstOrDefault();
                    }
                    else //分支流程则获取符合条件的下一步节点
                    {

                        List<NextNodeDataInfo> metConditionNodes = new List<NextNodeDataInfo>();//满足条件的分支节点
                        List<NextNodeDataInfo> defaultConditionNodes = new List<NextNodeDataInfo>();//默认不设置条件的分支节点，一般情况下只允许一条数据
                        foreach (var m in nextnodes)
                        {
                            bool isDefaultNode = false;
                            //验证规则是否符合
                            if (ValidateNextNodeRule(newcaseInfo, workflowInfo.FlowId, nodeid, m.NodeId.GetValueOrDefault(), caseInfo.VerNum, userinfo, out isDefaultNode, tran))
                            {
                                if (isDefaultNode)
                                {
                                    defaultConditionNodes.Add(m);
                                }
                                else metConditionNodes.Add(m);


                            }
                        }
                        if (defaultConditionNodes.Count > 1)
                            throw new Exception("每个流程只允许配置一条无过滤条件的分支");
                        else if (metConditionNodes.Count == 0 && defaultConditionNodes.Count == 0)
                        {
                            throw new Exception("没有符合分支流程规则的下一步审批人");
                        }
                        else
                        {
                            if (metConditionNodes.Count > 1)
                                throw new Exception("存在多条符合条件的分支，请重新配置流程后再发起审批");
                            else if (metConditionNodes.Count == 1)
                                nodetemp = metConditionNodes.FirstOrDefault();
                            else nodetemp = defaultConditionNodes.FirstOrDefault();
                        }
                    }

                    nodetemp.NodeState = nodetemp.StepTypeId == NodeStepType.End ? 2 : 0; //如果下一节点为结束审批节点，说明当前审批节点到达审批的最后节点
                    nodetemp.NodeNum = caseInfo.NodeNum;
                    nodetemp.NeedSuccAuditCount = 1;
                    nodetemp.FlowType = WorkFlowType.FixedFlow;
                    var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, nodetemp.NodeId.GetValueOrDefault(), userinfo.UserId, workflowInfo.FlowType, tran);
                    var cpUsers = new List<ApproverInfo>();
                    if (users == null || users.Count == 0 && nodetemp.NodeState == 0)//没有满足下一步审批人条件的选人列表,则获取与自由流程一样返回全公司人员
                    {
                        nodetemp.NodeState = 0;
                        var data = LoopWorkFlow(caseInfo, workflowInfo, flowNodeInfo, nodetemp, userinfo, tran);
                        if (data.Approvers != null && data.Approvers.Count > 0)
                        {
                            users = data.Approvers;
                            cpUsers = data.CPUsers;
                            if (data.NodeInfo != null)
                                nodetemp = data.NodeInfo;
                            nodetemp.IsSkip = true;
                            nodetemp.IsSkipNode = data.IsSkipNode;
                        }
                    }
                    else
                    {
                        nodetemp.IsSkip = false;
                    }

                    if (!nodetemp.IsSkipNode)
                    {
                        cpUsers = _workFlowRepository.GetFlowNodeCPUser(caseInfo.CaseId, nodetemp.NodeId.GetValueOrDefault(), userinfo.UserId, workflowInfo.FlowType, tran);
                        if (cpUsers == null || cpUsers.Count == 0 && nodetemp.NodeState == 0)//没有满足下一步审批人条件的选人列表,则获取与自由流程一样返回全公司人员
                        {
                            nodetemp.NodeState = 0;
                            // cpUsers = _workFlowRepository.GetFlowNodeCPUser(caseInfo.CaseId, Guid.Empty, userinfo.UserId, WorkFlowType.FreeFlow, tran);
                        }
                    }
                    result = new NextNodeDataModel()
                    {
                        NodeInfo = nodetemp,
                        Approvers = users,
                        CPUsers = cpUsers,
                        NotFound = nodetemp.NotFound
                    };
                }
                else
                {
                    result = new NextNodeDataModel()
                    {
                        NodeInfo = nodetemp,
                    };
                }

            }
            result.NodeInfo.NodeData = mycaseitems != null ? mycaseitems.Casedata : null;
            return result;
        }
        #endregion
        public bool CheckNodeData(DbTransaction tran, WorkFlowCaseInfo caseInfo, WorkFlowInfo workflowInfo, WorkFlowNodeInfo flowNodeInfo, UserInfo userinfo)
        {
            var result = new NextNodeDataModel();


            NextNodeDataInfo nodetemp = new NextNodeDataInfo();
            //获取流程数据信息
            var newcaseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseInfo.CaseId);
            var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
            if (caseitems == null || caseitems.Count == 0)
            {
                throw new Exception("流程节点数据异常");
            }
            var mycaseitems = caseitems.Find(m => m.HandleUser == userinfo.UserId);

            var nodeid = caseitems.FirstOrDefault().NodeId;

            if (flowNodeInfo == null)
            {
                throw new Exception("不存在有效节点");
            }
            nodetemp.NodeId = flowNodeInfo.NodeId;
            nodetemp.FlowType = WorkFlowType.FixedFlow;
            nodetemp.NodeName = flowNodeInfo.NodeName;
            nodetemp.NodeType = flowNodeInfo.NodeType;
            nodetemp.NodeNum = caseInfo.NodeNum == -1 ? -1 : caseInfo.NodeNum;
            nodetemp.NodeState = caseInfo.NodeNum == -1 ? -1 : 0;
            nodetemp.StepTypeId = flowNodeInfo.StepTypeId;
            nodetemp.StepCPTypeId = flowNodeInfo.StepCPTypeId;
            nodetemp.NotFound = flowNodeInfo.NotFound;
            if (newcaseInfo.NodeNum == -1 || flowNodeInfo.IsEnd)//预审批审批结束，表明到达最后节点
            {
                nodetemp.NodeState = 2;
            }
            else if (newcaseInfo.NodeNum == 0 && mycaseitems != null && mycaseitems.ChoiceStatus == ChoiceStatusType.Reback)//预审批审批回到第一节点，表明被退回
            {
                nodetemp.NodeState = 3;
            }
            else
            {

                if (flowNodeInfo.NodeType == NodeType.Joint)//会审
                {

                    //会审审批通过的节点数
                    var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();
                    nodetemp.NeedSuccAuditCount = flowNodeInfo.AuditSucc - aproval_success_count;
                    if (aproval_success_count < flowNodeInfo.AuditSucc)//--说明当前节点，其他人还在审批，不能进入下一步
                    {
                        nodetemp.NodeState = 1;
                    }
                }
                else if (flowNodeInfo.NodeType == NodeType.SpecialJoint)//特殊会审 pxf
                {

                    //会审审批通过的节点数
                    var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();
                    nodetemp.NeedSuccAuditCount = flowNodeInfo.AuditSucc - aproval_success_count;
                    if (aproval_success_count < flowNodeInfo.AuditSucc)//--说明当前节点，其他人还在审批，不能进入下一步
                    {
                        nodetemp.NodeState = 1;
                    }
                }
            }
            //检查下一点，获取下一节点信息
            if (nodetemp.NodeState == 0)
            {
                //获取下一节点
                var nextnodes = _workFlowRepository.GetNextNodeDataInfoList(caseInfo.FlowId, nodeid, caseInfo.VerNum, tran);
                if (nextnodes == null || nextnodes.Count == 0)
                    throw new Exception("获取不到节点配置");

                if (nextnodes.Count == 1 && !flowNodeInfo.IsSkip)
                {
                    nodetemp = nextnodes.FirstOrDefault();
                }
                else //分支流程则获取符合条件的下一步节点
                {

                    List<NextNodeDataInfo> metConditionNodes = new List<NextNodeDataInfo>();//满足条件的分支节点
                    List<NextNodeDataInfo> defaultConditionNodes = new List<NextNodeDataInfo>();//默认不设置条件的分支节点，一般情况下只允许一条数据
                    foreach (var m in nextnodes)
                    {
                        bool isDefaultNode = false;
                        //验证规则是否符合
                        if (ValidateNextNodeRule(newcaseInfo, workflowInfo.FlowId, nodeid, m.NodeId.GetValueOrDefault(), caseInfo.VerNum, userinfo, out isDefaultNode, tran))
                        {
                            if (isDefaultNode)
                            {
                                defaultConditionNodes.Add(m);
                            }
                            else metConditionNodes.Add(m);


                        }
                    }
                    if (defaultConditionNodes.Count > 1)
                        throw new Exception("每个流程只允许配置一条无过滤条件的分支");
                    else if (metConditionNodes.Count == 0 && defaultConditionNodes.Count == 0)
                    {
                        throw new Exception("没有符合分支流程规则的下一步审批人");
                    }
                    else
                    {
                        if (metConditionNodes.Count > 1)
                            throw new Exception("存在多条符合条件的分支，请重新配置流程后再发起审批");
                        else if (metConditionNodes.Count == 1)
                            nodetemp = metConditionNodes.FirstOrDefault();
                        else nodetemp = defaultConditionNodes.FirstOrDefault();
                    }
                }

                nodetemp.NodeState = nodetemp.StepTypeId == NodeStepType.End ? 2 : 0; //如果下一节点为结束审批节点，说明当前审批节点到达审批的最后节点
                nodetemp.NodeNum = caseInfo.NodeNum;
                nodetemp.NeedSuccAuditCount = 1;
                nodetemp.FlowType = WorkFlowType.FixedFlow;
                var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, nodetemp.NodeId.GetValueOrDefault(), userinfo.UserId, workflowInfo.FlowType, tran);
                if (users == null || users.Count == 0 && nodetemp.NodeState == 0)//没有满足下一步审批人条件的选人列表,则获取与自由流程一样返回全公司人员
                {
                    nodetemp.NodeState = 0;
                    var data = LoopWorkFlow(caseInfo, workflowInfo, flowNodeInfo, nodetemp, userinfo, tran);
                    if (data.Approvers != null && data.Approvers.Count > 0)
                    {
                        users = data.Approvers;
                        if (data.NodeInfo != null)
                            nodetemp = data.NodeInfo;
                        nodetemp.IsSkip = true;
                        nodetemp.IsSkipNode = data.IsSkipNode;
                        return true;
                    }
                    return false;
                }
                else
                    return true;
            }
            else if (nodetemp.StepTypeId == NodeStepType.End)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public NextNodeDataInfo GetNextNodeDataInfo(DbTransaction tran, WorkFlowCaseInfo caseInfo, WorkFlowInfo workflowInfo, WorkFlowNodeInfo flowNodeInfo, UserInfo userinfo)
        {
            var result = new NextNodeDataModel();


            NextNodeDataInfo nodetemp = new NextNodeDataInfo();
            //获取流程数据信息
            var newcaseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseInfo.CaseId);
            var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
            if (caseitems == null || caseitems.Count == 0)
            {
                throw new Exception("流程节点数据异常");
            }
            var mycaseitems = caseitems.Find(m => m.HandleUser == userinfo.UserId);

            var nodeid = flowNodeInfo.NodeId;

            if (flowNodeInfo == null)
            {
                throw new Exception("不存在有效节点");
            }
            nodetemp.NodeId = flowNodeInfo.NodeId;
            nodetemp.FlowType = WorkFlowType.FixedFlow;
            nodetemp.NodeName = flowNodeInfo.NodeName;
            nodetemp.NodeType = flowNodeInfo.NodeType;
            nodetemp.NodeNum = caseInfo.NodeNum == -1 ? -1 : caseInfo.NodeNum;
            nodetemp.NodeState = caseInfo.NodeNum == -1 ? -1 : 0;
            nodetemp.StepTypeId = flowNodeInfo.StepTypeId;
            nodetemp.StepCPTypeId = flowNodeInfo.StepCPTypeId;
            nodetemp.NotFound = flowNodeInfo.NotFound;
            if (newcaseInfo.NodeNum == -1 || flowNodeInfo.IsEnd)//预审批审批结束，表明到达最后节点
            {
                nodetemp.NodeState = 2;
            }
            else if (newcaseInfo.NodeNum == 0 && mycaseitems != null && mycaseitems.ChoiceStatus == ChoiceStatusType.Reback)//预审批审批回到第一节点，表明被退回
            {
                nodetemp.NodeState = 3;
            }
            else
            {

                if (flowNodeInfo.NodeType == NodeType.Joint)//会审
                {

                    //会审审批通过的节点数
                    var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();
                    nodetemp.NeedSuccAuditCount = flowNodeInfo.AuditSucc - aproval_success_count;
                    if (aproval_success_count < flowNodeInfo.AuditSucc)//--说明当前节点，其他人还在审批，不能进入下一步
                    {
                        nodetemp.NodeState = 1;
                    }
                }
                else if (flowNodeInfo.NodeType == NodeType.SpecialJoint)//会审
                {

                    //会审审批通过的节点数
                    var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();
                    nodetemp.NeedSuccAuditCount = flowNodeInfo.AuditSucc - aproval_success_count;
                    if (aproval_success_count < flowNodeInfo.AuditSucc)//--说明当前节点，其他人还在审批，不能进入下一步
                    {
                        nodetemp.NodeState = 1;
                    }
                }
            }
            //检查下一点，获取下一节点信息
            if (nodetemp.NodeState == 0)
            {
                //获取下一节点
                var nextnodes = _workFlowRepository.GetNextNodeDataInfoList(caseInfo.FlowId, nodeid, caseInfo.VerNum, tran);
                if (nextnodes == null || nextnodes.Count == 0)
                    throw new Exception("获取不到节点配置");

                if (nextnodes.Count == 1 && !flowNodeInfo.IsSkip)
                {
                    nodetemp = nextnodes.FirstOrDefault();
                }
                else //分支流程则获取符合条件的下一步节点
                {

                    List<NextNodeDataInfo> metConditionNodes = new List<NextNodeDataInfo>();//满足条件的分支节点
                    List<NextNodeDataInfo> defaultConditionNodes = new List<NextNodeDataInfo>();//默认不设置条件的分支节点，一般情况下只允许一条数据
                    foreach (var m in nextnodes)
                    {
                        bool isDefaultNode = false;
                        //验证规则是否符合
                        if (ValidateNextNodeRule(newcaseInfo, workflowInfo.FlowId, nodeid, m.NodeId.GetValueOrDefault(), caseInfo.VerNum, userinfo, out isDefaultNode, tran))
                        {
                            if (isDefaultNode)
                            {
                                defaultConditionNodes.Add(m);
                            }
                            else metConditionNodes.Add(m);


                        }
                    }
                    if (defaultConditionNodes.Count > 1)
                        throw new Exception("每个流程只允许配置一条无过滤条件的分支");
                    else if (metConditionNodes.Count == 0 && defaultConditionNodes.Count == 0)
                    {
                        throw new Exception("没有符合分支流程规则的下一步审批人");
                    }
                    else
                    {
                        if (metConditionNodes.Count > 1)
                            throw new Exception("存在多条符合条件的分支，请重新配置流程后再发起审批");
                        else if (metConditionNodes.Count == 1)
                            nodetemp = metConditionNodes.FirstOrDefault();
                        else nodetemp = defaultConditionNodes.FirstOrDefault();
                    }
                }

                nodetemp.NodeState = nodetemp.StepTypeId == NodeStepType.End ? 2 : 0; //如果下一节点为结束审批节点，说明当前审批节点到达审批的最后节点
                nodetemp.NodeNum = caseInfo.NodeNum;
                nodetemp.NeedSuccAuditCount = 1;
                nodetemp.FlowType = WorkFlowType.FixedFlow;
                return nodetemp;
            }
            else
            {
                return nodetemp;
            }
        }

        NextNodeDataModel LoopWorkFlow(WorkFlowCaseInfo caseInfo, WorkFlowInfo workflowInfo, WorkFlowNodeInfo flowNodeInfo, NextNodeDataInfo nodeDataInfo, UserInfo userinfo, DbTransaction tran)
        {
            NextNodeDataModel result = new NextNodeDataModel();
            switch (nodeDataInfo.NotFound)
            {
                case 1:
                    result.Approvers = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, Guid.Empty, userinfo.UserId, WorkFlowType.FreeFlow, tran);
                    result.CPUsers = _workFlowRepository.GetFlowNodeCPUser(caseInfo.CaseId, nodeDataInfo.NodeId.Value, userinfo.UserId, workflowInfo.FlowType, tran);
                    result.IsSkipNode = true;
                    return result;
                case 2:
                    LoopInfo loopInfo;
                    result.Approvers = LoopApproveUsers(caseInfo, workflowInfo, flowNodeInfo, tran, userinfo, out loopInfo);
                    if (loopInfo.IsBreak && !loopInfo.IsBreak)
                        return LoopWorkFlow(caseInfo, workflowInfo, flowNodeInfo, loopInfo.NodeDataInfo, userinfo, tran);
                    result.CPUsers = _workFlowRepository.GetFlowNodeCPUser(caseInfo.CaseId, loopInfo.NodeDataInfo.NodeId.Value, userinfo.UserId, workflowInfo.FlowType, tran);
                    result.IsSkipNode = true;
                    result.NodeInfo = loopInfo.NodeDataInfo;
                    return result;
                case 3:
                    throw new Exception("功能未开放");
                case 0:
                    return result;
                default:
                    throw new Exception("功能异常");
            }
        }

        List<ApproverInfo> LoopApproveUsers(WorkFlowCaseInfo caseInfo, WorkFlowInfo workflowInfo, WorkFlowNodeInfo flowNodeInfo, DbTransaction tran, UserInfo userinfo, out LoopInfo loopInfo)
        {
            loopInfo = new LoopInfo();
            //var nodes = _workFlowRepository.GetNextNodeInfoList(tran, workflowInfo.FlowId, flowNodeInfo.VerNum, flowNodeInfo.NodeId);
            //if ((nodes == null || nodes.Count == 0 || nodes.FirstOrDefault().StepTypeId == NodeStepType.End) && flowNodeInfo.StepTypeId == NodeStepType.End)
            //{
            //    return new List<ApproverInfo>();
            //}
            var node = GetNextNodeDataInfo(tran, caseInfo, workflowInfo, flowNodeInfo, userinfo);
            if ((node == null || node.StepTypeId == NodeStepType.End) && flowNodeInfo.StepTypeId == NodeStepType.End)
            {
                return new List<ApproverInfo>();
            }
            flowNodeInfo = _workFlowRepository.GetNodeInfoList(tran, workflowInfo.FlowId, flowNodeInfo.VerNum).FirstOrDefault(t => t.NodeId == node.NodeId.Value);
            var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, flowNodeInfo.NodeId, userinfo.UserId, workflowInfo.FlowType, tran);
            if (users.Count > 0)
            {
                var nodeInfo = _workFlowRepository.GetNodeDataInfo(caseInfo.FlowId, flowNodeInfo.NodeId, caseInfo.VerNum, tran).FirstOrDefault();
                loopInfo.NodeDataInfo = nodeInfo;
                return users;
            }
            else
            {
                if (flowNodeInfo.NotFound != 2)
                {
                    loopInfo.IsBreak = true;
                    loopInfo.NodeId = flowNodeInfo.NodeId;
                    var nodeInfo = _workFlowRepository.GetCurNodeDataInfo(caseInfo.FlowId, flowNodeInfo.NodeId, caseInfo.VerNum, tran).FirstOrDefault();
                    loopInfo.NodeDataInfo = nodeInfo;
                    loopInfo.IsNoneApproverUser = (flowNodeInfo.StepTypeId == NodeStepType.End) ? true : false;
                    if (!loopInfo.IsNoneApproverUser)
                    {
                        users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, Guid.Empty, userinfo.UserId, WorkFlowType.FreeFlow, tran);
                    }
                    return users;
                }
                return LoopApproveUsers(caseInfo, workflowInfo, flowNodeInfo, tran, userinfo, out loopInfo);
            }
        }


        #region --提交审批--


        public OutputResult<object> SubmitWorkFlowAudit(WorkFlowAuditCaseItemModel caseItemModel, UserInfo userinfo, DbTransaction tran = null, int isNeedToSendMsg = 0, int isLaunchNode = 0)
        {
            bool isSkip = false;
            //获取该实体分类的字段
            var caseItemEntity = _mapper.Map<WorkFlowAuditCaseItemModel, WorkFlowAuditCaseItemMapper>(caseItemModel);
            if (caseItemEntity == null || !caseItemEntity.IsValid())
            {
                return HandleValid(caseItemEntity);
            }
            bool casefinish = false;
            int stepnum = 0;
            Guid nodeid = Guid.Empty;
            Guid event_nodeid = Guid.Empty;
            Guid lastNodeId = Guid.Empty;
            int lastNodeNum = 0;
            bool canAddNextNode = true;
            bool isbranchFlow = false;
            WorkFlowNodeInfo nextnode = null;
            DbConnection conn = null;
            bool IsFinishAfterStart = false;
            string handlUser = string.Empty;
            string curUserIds = string.Empty;
            string message = string.Empty;
            int status = 0;
            if (tran == null || tran.Connection == null)
            {
                conn = GetDbConnect();
                conn.Open();
                tran = conn.BeginTransaction();
            }
            var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseItemEntity.CaseId);
            try
            {
                //判断流程类型，如果是分支流程，则检查所选分支是否正确
                //获取casedetail
                if (caseInfo == null)
                    throw new Exception("流程表单数据不存在");
                var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
                if (workflowInfo == null)
                    throw new Exception("流程配置不存在");
                var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
                if (caseitems == null || caseitems.Count == 0)
                    throw new Exception("流程节点不存在");
                caseitems = caseitems.OrderByDescending(m => m.StepNum).ToList();

                if (caseItemEntity.ChoiceStatus == 2 && caseItemEntity.NodeNum == 0)
                {
                    throw new Exception("第一个节点不可退回");
                }
                if (caseitems.FirstOrDefault().NodeType == 2)
                {
                    caseItemModel.JointStatus = caseItemEntity.ChoiceStatus == -1 ? 1 : 0;
                    caseItemEntity.JointStatus = caseItemEntity.ChoiceStatus == -1 ? 1 : 0;
                    caseItemEntity.ChoiceStatus = 1;
                    caseItemModel.ChoiceStatus = 1;
                }
                stepnum = caseitems.FirstOrDefault().StepNum;
                WorkFlowEventInfo eventInfo;

                if (workflowInfo.FlowType == WorkFlowType.FreeFlow)//自由流程
                {
                    var nowcaseitem = caseitems.FirstOrDefault();
                    AuditFreeFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, nowcaseitem, out canAddNextNode);
                    if (casefinish)
                    {
                        //自由流程，uuid值为0作为流程起点，值为1作为流程终点';
                        nodeid = freeFlowEndNodeId;
                        lastNodeId = freeFlowEndNodeId;
                        canAddNextNode = false;
                    }
                    else if (caseItemEntity.ChoiceStatus == 2)//0拒绝 1通过 2退回 3中止 4编辑 5审批结束 6节点新增
                    {
                        nodeid = freeFlowBeginNodeId;
                        canAddNextNode = false;
                    }
                    else
                    {
                        nodeid = caseItemEntity.NodeNum == 0 ? freeFlowBeginNodeId : freeFlowNodeId;

                    }
                    //流程审批过程修改实体字段时，更新关联实体的字段数据
                    _workFlowRepository.ExecuteUpdateWorkFlowEntity(caseInfo.CaseId, caseInfo.NodeNum, userinfo.UserId, tran);

                }
                else //固定流程
                {
                    nodeid = caseitems.FirstOrDefault().NodeId;

                    isbranchFlow = AuditFixedFlow(nodeid, userinfo, caseItemEntity, ref casefinish, tran, caseInfo, caseitems, userinfo.UserId, out nextnode, out canAddNextNode);
                    if (casefinish && nextnode != null)
                    {
                        lastNodeId = nextnode.NodeId;
                        lastNodeNum = nextnode.NodeNum;
                    }

                }

                //如果不是自由流程，或者自由流程的第一个节点，需要验证是否有附加函数
                if (workflowInfo.FlowType != WorkFlowType.FreeFlow || nodeid == freeFlowBeginNodeId)
                {
                    //判断是否有附加函数_event_func
                    eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, nodeid, tran);
                    if (eventInfo != null && !string.IsNullOrEmpty(eventInfo.FuncName))
                    {
                        _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, -1, caseItemEntity.ChoiceStatus, userinfo.UserId, tran);
                    }
                }

                if (casefinish)//审批已经到达了最后一步
                {
                    if (caseItemEntity.NodeNum == 0 && caseItemEntity.ChoiceStatus == 4)
                    {
                        //发起节点就立即结束，则默认为审批通过
                        caseItemEntity.ChoiceStatus = 1;
                        IsFinishAfterStart = true;
                    }
                    var caseItemList = _workFlowRepository.CaseItemList(caseInfo.CaseId, userinfo.UserId, tran: tran);

                    int casestatus = -2;
                    if (caseItemList.Count > 0)
                        casestatus = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString());
                    if (casestatus != -1)
                    {
                        var caseItemId = _workFlowRepository.EndWorkFlowCaseItem(caseInfo.CaseId, lastNodeId, stepnum + 1, userinfo.UserId, tran);
                        var subscriber = _workFlowRepository.GetSubscriber(Guid.Parse(caseItemId.ToString()), caseItemEntity.ChoiceStatus, caseInfo, nextnode, tran, userinfo.UserId);
                        var informer = _workFlowRepository.GetInformer(caseInfo.FlowId, caseItemEntity.ChoiceStatus, caseInfo, nextnode, tran, userinfo.UserId);
                        foreach (var t in informer)
                        {
                            bool result = this.ValidateInformerRule(caseInfo, t.Key, userinfo.UserId, tran);
                            if (result)
                            {
                                subscriber = subscriber.Union(t.Value).ToList();
                            }
                        }
                        _workFlowRepository.AddEndWorkFlowCaseItemCPUser(tran, Guid.Parse(caseItemId.ToString()), subscriber);
                        //   caseItemEntity.ChoiceStatus = 5;
                    }
                    if (caseItemEntity.ChoiceStatus == 1)
                    {
                        var type = typeof(SoapServices);
                        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).Where(m => !m.IsSpecialName);
                        if (methods != null)
                        {
                            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
                            var erpSync = config.GetSection("ERPSync").Get<List<ErpSyncFunc>>().FirstOrDefault(t => t.EntityId == workflowInfo.Entityid.ToString() && t.FlowId == workflowInfo.FlowId.ToString());
                            if (erpSync != null)
                            {
                                if (erpSync.IsFlow == 1)
                                {
                                    var method = methods.FirstOrDefault(t => t.Name == erpSync.FuncName);
                                    var data = method.Invoke(type, new object[4] { workflowInfo.Entityid, caseInfo.CaseId, caseInfo.RecId, userinfo.UserId });
                                    var result = data as OperateResult;
                                    if (result.Flag == 0) { message = result.Msg; status = 1; }
                                }
                            }
                        }
                    }
                    //判断是否有附加函数_event_func
                    eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, lastNodeId, tran);
                    _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, -1, caseItemEntity.ChoiceStatus, userinfo.UserId, tran);
                }
                else if (canAddNextNode) //添加下一步骤审批节点
                {
                    //完成了以上审批操作，如果是分支流程，需要验证下一步审批人是否符合规则
                    if (isbranchFlow && nextnode != null)
                    {
                        bool isDefaultNode = false;
                        //验证规则是否符合
                        if (!ValidateNextNodeRule(caseInfo, workflowInfo.FlowId, nodeid, nextnode.NodeId, caseInfo.VerNum, userinfo, out isDefaultNode, tran))
                            throw new Exception("下一步审批人不符合分支流程规则");
                    }
                    var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, nextnode.NodeId, userinfo.UserId, workflowInfo.FlowType, tran);
                    isSkip = (users.Count == 0 && nextnode.NotFound == 2);
                    if (string.IsNullOrEmpty(caseItemModel.ActHandleUser))
                        caseItemModel.ActHandleUser = caseItemModel.HandleUser;

                    if (isSkip)
                    {
                        curUserIds = userinfo.UserId.ToString();
                        caseItemModel.SkipNode = 1;
                        caseItemModel.HandleUser = curUserIds;
                    }
                    bool isRefresh = false;
                    bool isAdd = false;
                    NextNodeDataModel node = new NextNodeDataModel();
                    while (true && stepnum != 0)
                    {
                        InterceptWorkFlow(caseItemModel, caseInfo, caseInfo.VerNum, workflowInfo.FlowId, tran, userinfo, out node, out isRefresh, out isAdd, isNeedToSendMsg, isLaunchNode: isLaunchNode);
                        if (isRefresh)
                            break;
                    }
                    if (isRefresh || stepnum == 0)
                    {
                        if (node != null && node.Approvers != null)
                        { isSkip = (node.Approvers.Count == 0 && node.NotFound == 2); nodeid = node.NodeInfo.NodeId.Value; }
                        caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseInfo.CaseId);
                    }
                    if (isAdd || stepnum == 0)
                    {
                        if (stepnum != 0)
                        {
                            caseItemModel.HandleUser = caseItemModel.ActHandleUser;
                            stepnum = caseInfo.StepNum;
                        }
                        caseItemModel.SkipNode = isSkip ? 1 : 0;

                        AddCaseItem(nodeid, caseItemModel, workflowInfo, caseInfo, stepnum + 1, userinfo, tran);
                    }
                }
                if (conn != null)
                {
                    tran.Commit();
                    canWriteCaseMessage = true;
                }

                while (isSkip && (caseItemModel.ChoiceStatus == 1 || caseItemModel.ChoiceStatus == 4))
                {
                    var caseItemList = _workFlowRepository.CaseItemList(caseInfo.CaseId, userinfo.UserId, tran: (conn == null ? tran : null));
                    if (caseItemList.Count == 0) break;
                    var nodeDataInfo = _workFlowRepository.GetNodeDataInfo(caseInfo.FlowId, Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()), caseInfo.VerNum);
                    if (nodeDataInfo.Count == 0 && caseItemList[caseItemList.Count - 1]["nodenum"].ToString() == "-1")
                        break;
                    var branchNode = GetNextNodeBranch(caseInfo, Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()), userinfo, tran);
                    if (branchNode.NodeId == Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()))
                    {
                        break;
                    }
                    var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, branchNode.NodeId.GetValueOrDefault());
                    var nextNodeData = GetNextNodeData(tran, caseInfo, workflowInfo, flowNodeInfo, userinfo);
                    SubmitWorkFlowAudit(new WorkFlowAuditCaseItemModel
                    {
                        ActHandleUser = caseItemModel.ActHandleUser,
                        CaseId = caseInfo.CaseId,
                        ChoiceStatus = 1,
                        CopyUser = caseItemModel.CopyUser,
                        HandleUser = nextNodeData.Approvers != null && nextNodeData.Approvers.Count > 0 && branchNode.NodeId == caseItemEntity.NodeId ? (string.IsNullOrEmpty(caseItemModel.HandleUser) ? caseItemModel.ActHandleUser : caseItemModel.HandleUser) : curUserIds,
                        NodeId = branchNode.NodeId,
                        NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString()),
                        Suggest = "",
                        SkipNode = nextNodeData.Approvers != null && nextNodeData.Approvers.Count > 0 && branchNode.NodeId == caseItemEntity.NodeId ? 0 : caseItemModel.SkipNode
                    }, userinfo, tran: (conn == null ? tran : null), isNeedToSendMsg: isNeedToSendMsg, isLaunchNode: isLaunchNode);
                    if (!nextNodeData.IsSkipNode)
                        break;
                }
                if (isNeedToSendMsg != 0)
                {
                    if (!isSkip && isNeedToSendMsg != 3)
                    {
                        isNeedToSendMsg = 2;
                    }
                }
                //写审批消息
                if (isNeedToSendMsg == 1)//跳过流程的时候替换用户
                {
                    int userId = 0;
                    foreach (var u in caseItemModel.HandleUser.Split(","))
                    {
                        if (userinfo.UserId != Convert.ToInt32(u))
                        {
                            userId = Convert.ToInt32(u);
                        }
                        WriteCaseAuditMessage(caseInfo.CaseId, caseInfo.NodeNum, caseInfo.StepNum, userId, IsFinishAfterStart);
                        isNeedToSendMsg = 0;
                    }
                }
                else if (isNeedToSendMsg == 2 || isNeedToSendMsg == 3)
                {
                    WriteCaseAuditMessage(caseInfo.CaseId, caseInfo.NodeNum, caseInfo.StepNum, userinfo.UserId, IsFinishAfterStart);
                    isNeedToSendMsg = 0;
                }
            }
            catch (Exception ex)
            {
                if (conn != null)
                {
                    tran.Rollback();
                }
                StackTrace st = new StackTrace(ex, true);
                StackFrame[] frames = st.GetFrames();

                // Iterate over the frames extracting the information you need
                StringBuilder sb = new StringBuilder();
                foreach (StackFrame frame in frames)
                {
                    sb.Append(string.Format("{0}:{1}({2},{3})", frame.GetFileName(), frame.GetMethod().Name, frame.GetFileLineNumber(), frame.GetFileColumnNumber()));
                    _logger.Error(string.Format("{0}:{1}({2},{3})", frame.GetFileName(), frame.GetMethod().Name, frame.GetFileLineNumber(), frame.GetFileColumnNumber()));
                }
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            return new OutputResult<object>(null, message, status);
        }
        NextNodeDataInfo GetNextNodeBranch(WorkFlowCaseInfo caseInfo, Guid nodeId, UserInfo userInfo, DbTransaction tran)
        {
            var data = _workFlowRepository.GetNextNodeDataInfoList(caseInfo.FlowId, nodeId, caseInfo.VerNum, tran);
            NextNodeDataInfo nodetemp = new NextNodeDataInfo();
            NextNodeDataModel curNodeTemp = new NextNodeDataModel();
            List<NextNodeDataInfo> metConditionNodes = new List<NextNodeDataInfo>();//满足条件的分支节点
            List<NextNodeDataInfo> defaultConditionNodes = new List<NextNodeDataInfo>();//默认不设置条件的分支节点，一般情况下只允许一条数据
            foreach (var m in data)
            {
                bool isDefaultNode = false;
                //验证规则是否符合
                if (ValidateNextNodeRule(caseInfo, caseInfo.FlowId, nodeId, m.NodeId.GetValueOrDefault(), caseInfo.VerNum, userInfo, out isDefaultNode, tran))
                {
                    if (isDefaultNode)
                    {
                        defaultConditionNodes.Add(m);
                    }
                    else metConditionNodes.Add(m);
                }
            }
            if (defaultConditionNodes.Count > 1)
                throw new Exception("每个流程只允许配置一条无过滤条件的分支");
            else if (metConditionNodes.Count == 0 && defaultConditionNodes.Count == 0)
            {
                throw new Exception("没有符合分支流程规则的下一步审批人");
            }
            else
            {
                if (metConditionNodes.Count > 1)
                    throw new Exception("存在多条符合条件的分支，请重新配置流程后再发起审批");
                else if (metConditionNodes.Count == 1)
                    nodetemp = metConditionNodes.FirstOrDefault();
                else nodetemp = defaultConditionNodes.FirstOrDefault();
            }
            return nodetemp;
        }
        void PreInterceptWorkFlow(WorkFlowAuditCaseItemModel caseItemModel, WorkFlowCaseInfo caseInfo, int vernum, Guid flowId, DbTransaction tran, UserInfo userInfo, out NextNodeDataModel nodeTemp, out bool isBreak)
        {
            var caseItemList = _workFlowRepository.CaseItemList(caseInfo.CaseId, userInfo.UserId, tran: tran);
            var nextNodes = _workFlowRepository.GetNextNodeDataInfoList(flowId, Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()), Convert.ToInt32(caseItemList[caseItemList.Count - 1]["vernum"]), tran);
            isBreak = false;
            nodeTemp = new NextNodeDataModel();
            if (caseItemList[caseItemList.Count - 1]["nodenum"].ToString() == "-1")
            {
                isBreak = true;
                nodeTemp.NodeInfo = _workFlowRepository.GetCurNodeDataInfo(flowId, Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()), caseInfo.VerNum).FirstOrDefault();
                return;
            }
            if (nextNodes.Count == 0) throw new Exception("找不到流程分支");
            nodeTemp = new NextNodeDataModel();
            NextNodeDataInfo node = new NextNodeDataInfo();
            List<NextNodeDataInfo> metConditionNodes = new List<NextNodeDataInfo>();//满足条件的分支节点
            List<NextNodeDataInfo> defaultConditionNodes = new List<NextNodeDataInfo>();//默认不设置条件的分支节点，一般情况下只允许一条数据
            foreach (var m in nextNodes)
            {
                bool isDefaultNode = false;
                //验证规则是否符合
                if (ValidateNextNodeRule(caseInfo, flowId, Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()), m.NodeId.GetValueOrDefault(), caseInfo.VerNum, userInfo, out isDefaultNode, tran))
                {
                    if (isDefaultNode)
                    {
                        defaultConditionNodes.Add(m);
                    }
                    else metConditionNodes.Add(m);
                }
            }
            if (defaultConditionNodes.Count > 1)
                throw new Exception("每个流程只允许配置一条无过滤条件的分支");
            else if (metConditionNodes.Count == 0 && defaultConditionNodes.Count == 0)
            {
                throw new Exception("没有符合分支流程规则的下一步审批人");
            }
            else
            {
                if (metConditionNodes.Count > 1)
                    throw new Exception("存在多条符合条件的分支，请重新配置流程后再发起审批");
                else if (metConditionNodes.Count == 1)
                    node = metConditionNodes.FirstOrDefault();
                else node = defaultConditionNodes.FirstOrDefault();
            }
            var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
            var workflowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, node.NodeId.Value);
            var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, node.NodeId.Value, userInfo.UserId, workflowInfo.FlowType, tran);
            var cpusers = _workFlowRepository.GetFlowNodeCPUser(caseInfo.CaseId, node.NodeId.Value, userInfo.UserId, workflowInfo.FlowType, tran);

            if (caseItemList[caseItemList.Count - 1]["handleuser"].ToString() == string.Join(",", users.Select(t => t.UserId).Distinct().ToArray()) && caseItemList[caseItemList.Count - 1]["nodetype"].ToString() == "0")
            {
                SubmitWorkFlowAuditHelp(new WorkFlowAuditCaseItemModel
                {
                    CaseId = caseInfo.CaseId,
                    ChoiceStatus = caseItemList[caseItemList.Count - 1]["nodenum"].ToString() == "-1" ? 5 : 1,
                    CopyUser = string.Join(",", cpusers.Select(t => t.UserId).Distinct().ToArray()),
                    HandleUser = string.Join(",", users.Select(t => t.UserId).Distinct().ToArray()),
                    NodeId = node.NodeId,
                    NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString()),
                    Suggest = caseItemModel.Suggest,
                    SkipNode = 1
                }, userInfo, tran, 0);
                isBreak = false;
            }
            else if (node.NotFound == 2 && users.Count == 0)
            {
                SubmitWorkFlowAuditHelp(new WorkFlowAuditCaseItemModel
                {
                    CaseId = caseInfo.CaseId,
                    ChoiceStatus = caseItemList[caseItemList.Count - 1]["nodenum"].ToString() == "-1" ? 5 : 1,
                    CopyUser = string.Empty,
                    HandleUser = userInfo.UserId.ToString(),
                    NodeId = node.NodeId,
                    NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString()),
                    Suggest = "",
                    SkipNode = 1
                }, userInfo, tran, 0);
                if (caseItemList[caseItemList.Count - 1]["nodetype"].ToString() == "1" || caseItemList[caseItemList.Count - 1]["nodetype"].ToString() == "2")
                {
                    isBreak = true;
                }
            }
            else
            {
                isBreak = true;
            }
            if (node.StepTypeId != NodeStepType.End)
            {
                var result = SubmitPretreatAuditHelp(new WorkFlowAuditCaseItemModel
                {
                    CaseId = caseInfo.CaseId,
                    ChoiceStatus = caseItemList[caseItemList.Count - 1]["nodenum"].ToString() == "-1" ? 5 : 1,
                    NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString())
                }, userInfo, tran);
                nodeTemp = result.DataBody as NextNodeDataModel;
            }
            else
            {
                SubmitWorkFlowAuditHelp(new WorkFlowAuditCaseItemModel
                {
                    CaseId = caseInfo.CaseId,
                    ChoiceStatus = caseItemList[caseItemList.Count - 1]["nodenum"].ToString() == "-1" ? 5 : 1,
                    CopyUser = string.Empty,
                    HandleUser = userInfo.UserId.ToString(),
                    NodeId = node.NodeId,
                    NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString()),
                    Suggest = caseItemModel.Suggest,
                    SkipNode = 1
                }, userInfo, tran, 0);
                nodeTemp.NodeInfo = node;
            }
        }
        void InterceptWorkFlow(WorkFlowAuditCaseItemModel caseItemModel, WorkFlowCaseInfo caseInfo, int vernum, Guid flowId, DbTransaction tran, UserInfo userInfo, out NextNodeDataModel nodeTemp, out bool isRefresh, out bool isAdd, int isNeedToSendMsg = 0, int isLaunchNode = 0)
        {
            var caseItemList = _workFlowRepository.CaseItemList(caseInfo.CaseId, userInfo.UserId, tran: tran);
            var nextNodes = _workFlowRepository.GetNextNodeDataInfoList(flowId, Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()), Convert.ToInt32(caseItemList[caseItemList.Count - 1]["vernum"]), tran);
            isRefresh = false;
            isAdd = false;
            nodeTemp = new NextNodeDataModel();
            if (nextNodes.Count == 0) throw new Exception("找不到流程分支");
            NextNodeDataInfo nodetemp = new NextNodeDataInfo();
            List<NextNodeDataInfo> metConditionNodes = new List<NextNodeDataInfo>();//满足条件的分支节点
            List<NextNodeDataInfo> defaultConditionNodes = new List<NextNodeDataInfo>();//默认不设置条件的分支节点，一般情况下只允许一条数据
            foreach (var m in nextNodes)
            {
                bool isDefaultNode = false;
                //验证规则是否符合
                if (ValidateNextNodeRule(caseInfo, flowId, Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()), m.NodeId.GetValueOrDefault(), caseInfo.VerNum, userInfo, out isDefaultNode, tran))
                {
                    if (isDefaultNode)
                    {
                        defaultConditionNodes.Add(m);
                    }
                    else metConditionNodes.Add(m);
                }
            }
            if (defaultConditionNodes.Count > 1)
                throw new Exception("每个流程只允许配置一条无过滤条件的分支");
            else if (metConditionNodes.Count == 0 && defaultConditionNodes.Count == 0)
            {
                throw new Exception("没有符合分支流程规则的下一步审批人");
            }
            else
            {
                if (metConditionNodes.Count > 1)
                    throw new Exception("存在多条符合条件的分支，请重新配置流程后再发起审批");
                else if (metConditionNodes.Count == 1)
                    nodetemp = metConditionNodes.FirstOrDefault();
                else nodetemp = defaultConditionNodes.FirstOrDefault();
            }
            var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
            var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, nodetemp.NodeId.Value, userInfo.UserId, workflowInfo.FlowType, tran);
            var cpusers = _workFlowRepository.GetFlowNodeCPUser(caseInfo.CaseId, nodetemp.NodeId.Value, userInfo.UserId, workflowInfo.FlowType, tran);

            if (caseItemList[caseItemList.Count - 1]["handleuser"].ToString() == string.Join(",", users.Select(t => t.UserId).Distinct().ToArray()) && isLaunchNode != 1 && caseItemList[caseItemList.Count - 1]["nodetype"].ToString() == "0")
            {
                SubmitWorkFlowAuditHelp(new WorkFlowAuditCaseItemModel
                {
                    CaseId = caseInfo.CaseId,
                    ChoiceStatus = 1,
                    CopyUser = string.Join(",", cpusers.Select(t => t.UserId).Distinct().ToArray()),
                    HandleUser = string.Join(",", users.Select(t => t.UserId).Distinct().ToArray()),
                    NodeId = nodetemp.NodeId,
                    NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString()),
                    Suggest = caseItemModel.Suggest,
                    SkipNode = 1
                }, userInfo, tran, isNeedToSendMsg);
            }
            else
            {
                int skipNode = 0;
                if (nodetemp.NotFound == 2 && users.Count == 0)//跳过节点
                {
                    isRefresh = false;
                    skipNode = 1;
                }
                else
                    isRefresh = true;
                nodeTemp.NodeInfo = nodetemp;
                nodeTemp.Approvers = users;
                if (skipNode == 1)//跳过节点的情况直接跳过
                    SubmitWorkFlowAuditHelp(new WorkFlowAuditCaseItemModel
                    {
                        CaseId = caseInfo.CaseId,
                        ChoiceStatus = 1,
                        CopyUser = string.Join(",", cpusers.Select(t => t.UserId).Distinct().ToArray()),
                        HandleUser = !isRefresh ? userInfo.UserId.ToString() : string.Join(",", users.Select(t => t.UserId).Distinct().ToArray()),
                        NodeId = nodetemp.NodeId,
                        NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString()),
                        Suggest = caseItemModel.Suggest,
                        SkipNode = skipNode
                    }, userInfo, tran, isNeedToSendMsg);
                else if ((nodeTemp.Approvers.Count > 0 || nodeTemp.NotFound == 0) && isLaunchNode == 1)//发起后遇到一样发起人和审批人同一个的时候就跳出
                {
                    isRefresh = true;
                    isAdd = true;
                    SubmitWorkFlowAuditHelp(new WorkFlowAuditCaseItemModel
                    {
                        CaseId = caseInfo.CaseId,
                        ChoiceStatus = 1,
                        CopyUser = string.Empty,
                        HandleUser = userInfo.UserId.ToString(),
                        NodeId = nodetemp.NodeId,
                        NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString()),
                        Suggest = "",
                        SkipNode = skipNode
                    }, userInfo, tran, isNeedToSendMsg, 1);
                    //var curWorkFlowNode = _workFlowRepository.GetWorkFlowNodeInfo(tran, Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()));
                    //nodeTemp = this.GetNextNodeData(tran, caseInfo, workflowInfo, curWorkFlowNode, userInfo);
                    nodeTemp.NodeInfo = _workFlowRepository.GetCurNodeDataInfo(flowId, Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()), caseInfo.VerNum, tran).FirstOrDefault();
                }
                else if ((nodeTemp.Approvers.Count > 0 || nodeTemp.NodeInfo.NotFound == 1) && isLaunchNode != 1)//既不是跳过节点，也是不是发起的操作，遇到下个节点要审批的就直接把当前节点审批 然后跳出
                {
                    SubmitWorkFlowAuditHelp(new WorkFlowAuditCaseItemModel
                    {
                        Files = caseItemModel.Files,
                        CaseId = caseInfo.CaseId,
                        ChoiceStatus = 1,
                        CopyUser = caseItemModel.CopyUser,
                        HandleUser = caseItemModel.ActHandleUser,
                        NodeId = nodetemp.NodeId,
                        NodeNum = Convert.ToInt32(caseItemList[caseItemList.Count - 1]["nodenum"].ToString()),
                        Suggest = caseItemModel.Suggest,
                        SkipNode = skipNode
                    }, userInfo, tran, isNeedToSendMsg);
                    isRefresh = true;
                }
            }
        }
        public OutputResult<object> SubmitWorkFlowAuditHelp(WorkFlowAuditCaseItemModel caseItemModel, UserInfo userinfo, DbTransaction tran, int isNeedToSendMsg, int isAddCaseItem = 0)
        {
            //获取该实体分类的字段
            var caseItemEntity = _mapper.Map<WorkFlowAuditCaseItemModel, WorkFlowAuditCaseItemMapper>(caseItemModel);
            if (caseItemEntity == null || !caseItemEntity.IsValid())
            {
                return HandleValid(caseItemEntity);
            }
            bool casefinish = false;
            int stepnum = 0;
            Guid nodeid = Guid.Empty;
            Guid event_nodeid = Guid.Empty;
            Guid lastNodeId = Guid.Empty;
            int lastNodeNum = 0;
            bool canAddNextNode = true;
            bool isbranchFlow = false;
            WorkFlowNodeInfo nextnode = null;
            DbConnection conn = null;
            bool IsFinishAfterStart = false;
            if (tran == null)
            {
                conn = GetDbConnect();
                conn.Open();
                tran = conn.BeginTransaction();
            }
            var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseItemEntity.CaseId);
            try
            {
                //判断流程类型，如果是分支流程，则检查所选分支是否正确
                //获取casedetail
                if (caseInfo == null)
                    throw new Exception("流程表单数据不存在");
                var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
                if (workflowInfo == null)
                    throw new Exception("流程配置不存在");
                if (caseInfo.NodeNum != caseItemModel.NodeNum)
                {
                    caseInfo.NodeNum = caseItemModel.NodeNum;
                }
                var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
                if (caseitems == null || caseitems.Count == 0)
                    throw new Exception("流程节点不存在");
                caseitems = caseitems.OrderByDescending(m => m.StepNum).ToList();

                if (caseItemEntity.ChoiceStatus == 2 && caseItemEntity.NodeNum == 0)
                {
                    throw new Exception("第一个节点不可退回");
                }

                stepnum = caseitems.FirstOrDefault().StepNum;
                WorkFlowEventInfo eventInfo;

                if (workflowInfo.FlowType == WorkFlowType.FreeFlow)//自由流程
                {
                    var nowcaseitem = caseitems.FirstOrDefault();
                    AuditFreeFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, nowcaseitem, out canAddNextNode);
                    if (casefinish)
                    {
                        //自由流程，uuid值为0作为流程起点，值为1作为流程终点';
                        nodeid = freeFlowEndNodeId;
                        lastNodeId = freeFlowEndNodeId;
                        canAddNextNode = false;
                    }
                    else if (caseItemEntity.ChoiceStatus == 2)//0拒绝 1通过 2退回 3中止 4编辑 5审批结束 6节点新增
                    {
                        nodeid = freeFlowBeginNodeId;
                        canAddNextNode = false;
                    }
                    else
                    {
                        nodeid = caseItemEntity.NodeNum == 0 ? freeFlowBeginNodeId : freeFlowNodeId;

                    }
                }
                else //固定流程
                {
                    nodeid = caseitems.FirstOrDefault().NodeId;

                    isbranchFlow = AuditFixedFlow(nodeid, userinfo, caseItemEntity, ref casefinish, tran, caseInfo, caseitems, userinfo.UserId, out nextnode, out canAddNextNode);
                    if (casefinish)
                    {
                        lastNodeId = nextnode.NodeId;
                        lastNodeNum = nextnode.NodeNum;
                    }

                }
                //流程审批过程修改实体字段时，更新关联实体的字段数据
                _workFlowRepository.ExecuteUpdateWorkFlowEntity(caseInfo.CaseId, caseInfo.NodeNum, userinfo.UserId, tran);


                //如果不是自由流程，或者自由流程的第一个节点，需要验证是否有附加函数
                if (workflowInfo.FlowType != WorkFlowType.FreeFlow || nodeid == freeFlowBeginNodeId)
                {
                    //判断是否有附加函数_event_func
                    eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, nodeid, tran);
                    _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, caseInfo.NodeNum, caseItemEntity.ChoiceStatus, userinfo.UserId, tran);

                }

                if (casefinish)//审批已经到达了最后一步
                {
                    if (caseItemEntity.NodeNum == 0 && caseItemEntity.ChoiceStatus == 4)
                    {
                        //发起节点就立即结束，则默认为审批通过
                        caseItemEntity.ChoiceStatus = 1;
                        IsFinishAfterStart = true;
                    }
                    var caseItemId = _workFlowRepository.EndWorkFlowCaseItem(caseInfo.CaseId, lastNodeId, stepnum + 1, userinfo.UserId, tran);
                    var subscriber = _workFlowRepository.GetSubscriber(Guid.Parse(caseItemId.ToString()), caseItemEntity.ChoiceStatus, caseInfo, nextnode, tran, userinfo.UserId);
                    var informer = _workFlowRepository.GetInformer(caseInfo.FlowId, caseItemEntity.ChoiceStatus, caseInfo, nextnode, tran, userinfo.UserId);
                    foreach (var t in informer)
                    {
                        bool result = this.ValidateInformerRule(caseInfo, t.Key, userinfo.UserId, tran);
                        if (result)
                        {
                            subscriber = subscriber.Union(t.Value).ToList();
                        }
                    }
                    _workFlowRepository.AddEndWorkFlowCaseItemCPUser(tran, Guid.Parse(caseItemId.ToString()), subscriber.Distinct().ToList());

                    //判断是否有附加函数_event_func
                    eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, lastNodeId, tran);
                    if (eventInfo != null && !string.IsNullOrEmpty(eventInfo.FuncName))
                    {
                        _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, -1, caseItemEntity.ChoiceStatus, userinfo.UserId, tran);
                    }
                }
                else if (canAddNextNode) //添加下一步骤审批节点
                {
                    //完成了以上审批操作，如果是分支流程，需要验证下一步审批人是否符合规则
                    if (isbranchFlow && nextnode != null)
                    {
                        bool isDefaultNode = false;
                        //验证规则是否符合
                        if (!ValidateNextNodeRule(caseInfo, workflowInfo.FlowId, nodeid, nextnode.NodeId, caseInfo.VerNum, userinfo, out isDefaultNode, tran))
                            throw new Exception("下一步审批人不符合分支流程规则");
                    }
                    if (isAddCaseItem == 0)
                        AddCaseItem(nodeid, caseItemModel, workflowInfo, caseInfo, stepnum + 1, userinfo, tran);
                }
                if (conn != null)
                {
                    tran.Commit();
                    canWriteCaseMessage = true;
                }

            }
            catch (Exception ex)
            {
                if (conn != null)
                {
                    tran.Rollback();
                }
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            return new OutputResult<object>(null);
        }
        #endregion


        #region --审批固定流程--
        void LoopNode(WorkFlowCaseInfo caseInfo, Guid nodeId, Guid correctNodeId, int userId, DbTransaction tran, out Guid nextNodeId, List<Dictionary<string, object>> nodeInfos = null)
        {
            nodeInfos = nodeInfos ?? _workFlowRepository.GetNodeLinesInfo(caseInfo.FlowId, userId, tran)["lines"];
            if (nodeInfos.Count > 0)
            {
                int index = 0;
                List<Dictionary<string, object>> chooseNode = new List<Dictionary<string, object>>();
                while (true)
                {
                    if (index >= nodeInfos.Count) break;
                    var t = nodeInfos[index];
                    if (t["fromnodeid"].ToString() == nodeId.ToString())
                    {
                        if (Guid.Parse(t["tonodeid"].ToString()) == correctNodeId)
                        {
                            nextNodeId = Guid.Parse(t["fromnodeid"].ToString());
                            break;
                        }
                        else
                        {
                            LoopNode(caseInfo, Guid.Parse(t["tonodeid"].ToString()), correctNodeId, userId, tran, out nextNodeId, nodeInfos);
                            var sad = string.Empty;
                        }
                    }
                    index++;
                }
            }
        }
        private bool AuditFixedFlow(Guid nodeid, UserInfo userinfo, WorkFlowAuditCaseItemMapper caseItemEntity, ref bool casefinish, DbTransaction tran, WorkFlowCaseInfo caseInfo, List<WorkFlowCaseItemInfo> caseitems, int userId, out WorkFlowNodeInfo nextnode, out bool canAddNextNode)
        {
            bool isbranchflow = false;
            bool hasNextNode = true;

            var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);
            if (flowNodeInfo == null)
                throw new Exception("流程配置不存在");
            if (caseItemEntity.ChoiceStatus == 0 || caseItemEntity.ChoiceStatus == 3)//中止操作，需要获取结束节点
            {
                var nodelist = _workFlowRepository.GetNodeInfoList(tran, caseInfo.FlowId, caseInfo.VerNum);
                nextnode = nodelist.Find(m => m.StepTypeId == NodeStepType.End);
            }
            else
            {
                WorkFlowCaseItemInfo nowcaseitem = null;
                //执行审批逻辑
                if (flowNodeInfo.NodeType == NodeType.Normal)//普通审批
                {
                    nowcaseitem = caseitems.FirstOrDefault();
                }
                else if (flowNodeInfo.NodeType == NodeType.Joint || flowNodeInfo.NodeType == NodeType.SpecialJoint)  //会审 or 特殊会审 pxf
                {
                    nowcaseitem = caseitems.FirstOrDefault(t => t.HandleUser == userinfo.UserId && (t.ChoiceStatus == ChoiceStatusType.AddNode || t.ChoiceStatus == ChoiceStatusType.Edit));
                    if (nowcaseitem == null)
                    {
                        nowcaseitem = caseitems.FirstOrDefault(t => t.HandleUser == userId);
                    }
                }
                _workFlowRepository.AuditWorkFlowCaseData(caseItemEntity, nowcaseitem, userinfo.UserId, tran);
                //流程审批过程修改实体字段时，更新关联实体的字段数据
                _workFlowRepository.ExecuteUpdateWorkFlowEntity(caseInfo.CaseId, caseInfo.NodeNum, userinfo.UserId, tran);

                //获取下一步节点，
                var flowNextNodeInfos = _workFlowRepository.GetNextNodeInfoList(tran, caseInfo.FlowId, caseInfo.VerNum, nodeid);
                isbranchflow = flowNextNodeInfos != null && flowNextNodeInfos.Count > 1;//如果配置节点有多个，属于分支流程


                if (flowNextNodeInfos == null || flowNextNodeInfos.Count == 0 || (flowNextNodeInfos.Count == 1 && flowNextNodeInfos.FirstOrDefault().StepTypeId == NodeStepType.End))
                {
                    canAddNextNode = false;
                    hasNextNode = false;
                }
                nextnode = flowNextNodeInfos.Find(m => m.NodeId == caseItemEntity.NodeId);
                if (nextnode == null)
                    nextnode = flowNextNodeInfos.FirstOrDefault();
                if (caseItemEntity.ChoiceStatus == 0 || caseItemEntity.ChoiceStatus == 3)//中止操作，需要获取结束节点
                {
                    var nodelist = _workFlowRepository.GetNodeInfoList(tran, caseInfo.FlowId, caseInfo.VerNum);
                    nextnode = nodelist.Find(m => m.StepTypeId == NodeStepType.End);
                }

                if (nextnode != null && nextnode.StepTypeId == NodeStepType.End)
                {
                    canAddNextNode = false;
                    hasNextNode = false;
                }
                if (flowNextNodeInfos.Count > 1)
                {
                    List<NextNodeDataInfo> metConditionNodes = new List<NextNodeDataInfo>();//满足条件的分支节点
                    List<NextNodeDataInfo> defaultConditionNodes = new List<NextNodeDataInfo>();//默认不设置条件的分支节点，一般情况下只允许一条数据
                    foreach (var m in flowNextNodeInfos)
                    {
                        bool isDefaultNode = false;
                        //验证规则是否符合
                        if (ValidateNextNodeRule(caseInfo, caseInfo.FlowId, nodeid, m.NodeId, caseInfo.VerNum, userinfo, out isDefaultNode, tran))
                        {
                            var nodeDataInfo = _workFlowRepository.GetCurNodeDataInfo(caseInfo.FlowId, m.NodeId, caseInfo.VerNum, tran).FirstOrDefault();
                            if (isDefaultNode)
                            {
                                defaultConditionNodes.Add(nodeDataInfo);
                            }
                            else metConditionNodes.Add(nodeDataInfo);
                        }
                    }
                    if (defaultConditionNodes.Count > 1)
                        throw new Exception("每个流程只允许配置一条无过滤条件的分支");
                    else if (metConditionNodes.Count == 0 && defaultConditionNodes.Count == 0)
                    {
                        throw new Exception("没有符合分支流程规则的下一步审批人");
                    }
                    else
                    {
                        if (metConditionNodes.Count > 1)
                            throw new Exception("存在多条符合条件的分支，请重新配置流程后再发起审批");
                        else if (metConditionNodes.Count == 1)
                            nextnode = flowNextNodeInfos.FirstOrDefault(t => t.NodeId == metConditionNodes.FirstOrDefault().NodeId);
                        else nextnode = flowNextNodeInfos.FirstOrDefault(t => t.NodeId == defaultConditionNodes.FirstOrDefault().NodeId);
                    }
                }
                else if (flowNextNodeInfos.Count == 0)
                {
                    nextnode = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);
                }
                else
                    nextnode = flowNextNodeInfos.FirstOrDefault();
            }
            //执行审批逻辑
            if (flowNodeInfo.NodeType == NodeType.Normal)//普通审批
            {
                var nowcaseitem = caseitems.FirstOrDefault();

                canAddNextNode = AuditNormalFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, nowcaseitem, hasNextNode);
            }
            else if (flowNodeInfo.NodeType == NodeType.Joint)  //会审
            {
                var nowcaseitem = caseitems.Find(m => m.HandleUser == userinfo.UserId);
                if (nowcaseitem == null)
                    throw new Exception("您没有审批当前节点的权限");
                canAddNextNode = AuditJoinFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, flowNodeInfo, nowcaseitem, hasNextNode);
            }
            else//特殊会审 pxf
            {
                var nowcaseitem = caseitems.Find(m => m.HandleUser == userinfo.UserId);
                if (nowcaseitem == null)
                    throw new Exception("您没有审批当前节点的权限");
                canAddNextNode = AuditJoinFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, flowNodeInfo, nowcaseitem, hasNextNode);
            }

            return isbranchflow;
        }
        #endregion

        #region --自由流程审批--
        private void AuditFreeFlow(UserInfo userinfo, WorkFlowAuditCaseItemMapper caseItemEntity, ref bool casefinish, DbTransaction tran, WorkFlowCaseInfo caseInfo, WorkFlowCaseItemInfo nowcaseitem, out bool canAddNextNode)
        {
            int casenodenum = caseInfo.NodeNum;
            canAddNextNode = false;
            AuditStatusType auditstatus = AuditStatusType.Approving;


            var auditResult = _workFlowRepository.AuditWorkFlowCaseItem(caseItemEntity, nowcaseitem, userinfo.UserId, tran);

            MessageService.UpdateWorkflowNodeMessage(tran, caseInfo.RecId, caseInfo.CaseId, nowcaseitem.StepNum, (int)caseItemEntity.ChoiceStatus, userinfo.UserId);

            switch (caseItemEntity.ChoiceStatus)  //0拒绝 1通过 2退回 3中止 4编辑 5审批结束 6节点新增
            {
                case 0:        //0流程被拒绝而结束
                    casefinish = true;
                    casenodenum = -1;
                    auditstatus = AuditStatusType.NotAllowed;
                    break;
                case 1:        //1通过
                    if (caseItemEntity.NodeNum == -1)//通过并结束流程
                    {
                        auditstatus = AuditStatusType.Finished;
                        casenodenum = -1;
                        casefinish = true;
                    }
                    else
                    {
                        auditstatus = AuditStatusType.Approving;
                        casenodenum = 1;
                        canAddNextNode = true;
                    }
                    break;
                case 2:       //2退回
                    var rebackResult = _workFlowRepository.RebackWorkFlowCaseItem(caseInfo, nowcaseitem, userinfo.UserId, tran);
                    casenodenum = 0;
                    break;
                case 3:       //3中止,中止一般是由审批发起人主动终止
                    casefinish = true;
                    casenodenum = -1;
                    auditstatus = AuditStatusType.NotAllowed;
                    break;
                case 4:       //4编辑
                    casenodenum = 0;
                    auditstatus = AuditStatusType.Begin;
                    _workFlowRepository.ReOpenWorkFlowCase(caseInfo.CaseId, nowcaseitem.CaseItemId, userinfo.UserId, tran);
                    canAddNextNode = true;
                    break;
            }
            var auditcase = _workFlowRepository.AuditWorkFlowCase(caseInfo.CaseId, auditstatus, casenodenum, userinfo.UserId, tran);
        }
        #endregion

        #region --固定普通流程审批--
        private bool AuditNormalFlow(UserInfo userinfo, WorkFlowAuditCaseItemMapper caseItemEntity, ref bool casefinish, DbTransaction tran, WorkFlowCaseInfo caseInfo, WorkFlowCaseItemInfo nowcaseitem, bool hasNextNode)
        {
            bool canAddNextNodeItem = false;

            int casenodenum = caseInfo.NodeNum;
            AuditStatusType auditstatus = AuditStatusType.Approving;
            var auditResult = _workFlowRepository.AuditWorkFlowCaseItem(caseItemEntity, nowcaseitem, userinfo.UserId, tran);
            MessageService.UpdateWorkflowNodeMessage(tran, caseInfo.RecId, caseInfo.CaseId, nowcaseitem.StepNum, caseItemEntity.ChoiceStatus, userinfo.UserId);
            switch (caseItemEntity.ChoiceStatus)  //0拒绝 1通过 2退回 3中止 4编辑 5审批结束 6节点新增
            {
                case 0:        //0流程被拒绝而结束
                    casefinish = true;
                    auditstatus = AuditStatusType.NotAllowed;
                    break;
                case 1:        //1通过，计算下一步审批，如果没有下一步审批，则表示审批完成
                    {
                        if (hasNextNode)
                        {
                            auditstatus = AuditStatusType.Approving;
                            canAddNextNodeItem = true;
                        }
                        else
                        {
                            auditstatus = AuditStatusType.Finished;
                            casefinish = true;
                        }
                    }
                    break;
                case 2:       //2退回
                    var rebackResult = _workFlowRepository.RebackWorkFlowCaseItem(caseInfo, nowcaseitem, userinfo.UserId, tran);
                    casenodenum = 0;
                    break;
                case 3:       //3中止,中止一般是由审批发起人主动终止
                    casefinish = true;
                    auditstatus = AuditStatusType.NotAllowed;
                    break;
                case 4:       //4编辑
                    casenodenum = 0;
                    auditstatus = hasNextNode ? AuditStatusType.Begin : AuditStatusType.Finished;
                    canAddNextNodeItem = hasNextNode;
                    casefinish = !hasNextNode;
                    if (hasNextNode)
                    {
                        _workFlowRepository.ReOpenWorkFlowCase(caseInfo.CaseId, nowcaseitem.CaseItemId, userinfo.UserId, tran);
                    }
                    break;
            }
            if (casefinish)
                casenodenum = -1;
            var auditcase = _workFlowRepository.AuditWorkFlowCase(caseInfo.CaseId, auditstatus, casenodenum, userinfo.UserId, tran);

            return canAddNextNodeItem;
        }
        #endregion

        #region --固定会审流程审批--
        private bool AuditJoinFlow(UserInfo userinfo, WorkFlowAuditCaseItemMapper caseItemEntity, ref bool casefinish, DbTransaction tran, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo nodeInfo, WorkFlowCaseItemInfo nowcaseitem, bool hasNextNode)
        {
            bool canAddNextNodeItem = true;

            int casenodenum = caseInfo.NodeNum;
            AuditStatusType auditstatus = AuditStatusType.Approving;

            var auditResult = _workFlowRepository.AuditWorkFlowCaseItem(caseItemEntity, nowcaseitem, userinfo.UserId, tran);
            #region 处理当前节点的消息状态 
            MessageService.UpdateWorkflowNodeMessage(tran, caseInfo.RecId, caseItemEntity.CaseId,
                            nowcaseitem.StepNum, caseItemEntity.ChoiceStatus, userinfo.UserId);
            // MessageService.UpdateJointAuditMessage(tran, caseInfo.RecId, caseItemEntity.CaseId, nowcaseitem.CaseItemId, caseItemEntity.ChoiceStatus, userinfo.UserId);
            #endregion
            //获取当前审批结果
            var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
            if (caseitems == null || caseitems.Count == 0)
                throw new Exception("流程节点异常");
            caseitems = caseitems.OrderByDescending(m => m.StepNum).ToList();

            //获取还有人未处理审批
            var aproval_notdeal_count = caseitems.Where(m => m.CaseStatus == CaseStatusType.WaitApproval || m.CaseStatus == CaseStatusType.Readed).Count();
            //会审审批已经通过的节点数
            var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();


            //0拒绝 1通过 2退回 3中止 4编辑 5审批结束 6节点新增
            if (caseItemEntity.ChoiceStatus == 3) //3中止,中止一般是由审批发起人主动终止
            {
                casefinish = true;
                auditstatus = AuditStatusType.NotAllowed;
            }
            else if (caseItemEntity.ChoiceStatus == 4) //4编辑重新发起
            {
                casenodenum = 0;
                auditstatus = AuditStatusType.Begin;
                canAddNextNodeItem = hasNextNode;
                casefinish = !hasNextNode;
                _workFlowRepository.ReOpenWorkFlowCase(caseInfo.CaseId, nowcaseitem.CaseItemId, userinfo.UserId, tran);
            }
            else
            {
                if (aproval_success_count >= nodeInfo.AuditSucc)//判断是否达到会审审批通过条件
                {
                    if (hasNextNode)
                    {
                        auditstatus = AuditStatusType.Approving;
                        canAddNextNodeItem = true;
                    }
                    else//该审批流程已经完成
                    {
                        auditstatus = AuditStatusType.Finished;
                        casefinish = true;
                    }
                    //处理其他人的情况
                    MessageService.UpdateJointAuditMessage(tran, caseInfo.RecId, caseItemEntity.CaseId, nowcaseitem.StepNum, caseItemEntity.ChoiceStatus, userinfo.UserId);
                }
                else if (aproval_notdeal_count > 0) //是否还有人未处理会审审批,若有，则等待他人完成当前步骤审批
                {
                    auditstatus = AuditStatusType.Approving;
                    canAddNextNodeItem = false;
                }
                else //所有人都已审批，且不达到会审通过条件
                {
                    canAddNextNodeItem = false;
                    if (caseitems.Exists(m => m.ChoiceStatus == ChoiceStatusType.Reback))//如果有人退回，则优先执行退回
                    {
                        var rebackResult = _workFlowRepository.RebackWorkFlowCaseItem(caseInfo, nowcaseitem, userinfo.UserId, tran);
                        casenodenum = 0;
                    }
                    else //否则执行拒绝逻辑
                    {
                        casefinish = true;
                        auditstatus = AuditStatusType.NotAllowed;
                    }
                }
            }
            if (casefinish)
                casenodenum = -1;
            var auditcase = _workFlowRepository.AuditWorkFlowCase(caseInfo.CaseId, auditstatus, casenodenum, userinfo.UserId, tran);

            return canAddNextNodeItem;
        }
        #endregion

        #region --添加审批节点--
        private void AddCaseItem(Guid fromNodeid, WorkFlowAuditCaseItemModel caseItemModel, WorkFlowInfo workFlowInfo, WorkFlowCaseInfo caseInfo, int stepnum, UserInfo userinfo, DbTransaction trans = null)
        {
            if (caseItemModel == null || string.IsNullOrEmpty(caseItemModel.HandleUser))//如果没有审批人，则不添加审批节点
            {
                throw new Exception("步骤处理人不能为空");
            }
            Guid nodeid = caseItemModel.NodeId.GetValueOrDefault();
            var curNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(trans, fromNodeid);
            Guid preCaseItemId = Guid.Empty;
            List<Guid> skipNodes = new List<Guid>();
            var caseitemlist = _workFlowRepository.GetWorkFlowCaseItemInfo(trans, caseInfo.CaseId, caseInfo.NodeNum);
            if (workFlowInfo.FlowType == WorkFlowType.FixedFlow)//如果是固定流程，则获取对应的node
            {
                //获取下一步节点，
                var flowNextNodeInfos = _workFlowRepository.GetNextNodeInfoList(trans, caseInfo.FlowId, caseInfo.VerNum, fromNodeid);
                if (flowNextNodeInfos == null && flowNextNodeInfos.Count == 0)
                    throw new Exception("下一步节点不存在");
                else if (flowNextNodeInfos.Count > 1 && nodeid == Guid.Empty)
                {
                    throw new Exception("分支流程nodeid不可为空");
                }
                WorkFlowNodeInfo flowNodeInfo = null;
                bool isNull = false;

                if (nodeid != Guid.Empty)
                {
                    if (flowNextNodeInfos.Count > 1)
                    {
                        NextNodeDataInfo nodetemp = new NextNodeDataInfo();
                        List<NextNodeDataInfo> metConditionNodes = new List<NextNodeDataInfo>();//满足条件的分支节点
                        List<NextNodeDataInfo> defaultConditionNodes = new List<NextNodeDataInfo>();//默认不设置条件的分支节点，一般情况下只允许一条数据
                        foreach (var m in flowNextNodeInfos)
                        {
                            var node = _workFlowRepository.GetNodeDataInfo(caseInfo.FlowId, m.NodeId, caseInfo.VerNum, trans).FirstOrDefault();
                            bool isDefaultNode = false;
                            //验证规则是否符合
                            if (ValidateNextNodeRule(caseInfo, caseInfo.FlowId, fromNodeid, m.NodeId, caseInfo.VerNum, userinfo, out isDefaultNode, trans))
                            {
                                if (isDefaultNode)
                                {
                                    defaultConditionNodes.Add(node);
                                }
                                else metConditionNodes.Add(node);
                            }
                        }
                        if (defaultConditionNodes.Count > 1)
                            throw new Exception("每个流程只允许配置一条无过滤条件的分支");
                        else if (metConditionNodes.Count == 0 && defaultConditionNodes.Count == 0)
                        {
                            throw new Exception("没有符合分支流程规则的下一步审批人");
                        }
                        else
                        {
                            if (metConditionNodes.Count > 1)
                                throw new Exception("存在多条符合条件的分支，请重新配置流程后再发起审批");
                            else if (metConditionNodes.Count == 1)
                                nodetemp = metConditionNodes.FirstOrDefault();
                            else nodetemp = defaultConditionNodes.FirstOrDefault();
                        }
                        flowNodeInfo = flowNextNodeInfos.FirstOrDefault(t => t.NodeId == nodetemp.NodeId);
                    }
                    else
                        flowNodeInfo = flowNextNodeInfos.FirstOrDefault();
                }
                else flowNodeInfo = flowNextNodeInfos.FirstOrDefault();
                if (flowNodeInfo == null && !isNull)
                    throw new Exception("下一步节点不可为空");
                nodeid = flowNodeInfo.NodeId;
                if (flowNodeInfo.NodeType == NodeType.Joint || flowNodeInfo.NodeType == NodeType.SpecialJoint)//会审
                {
                    if (caseitemlist == null || caseitemlist.Count == 0)
                        throw new Exception("流程节点异常");
                }
                preCaseItemId = caseitemlist.OrderByDescending(t => t.StepNum).FirstOrDefault().CaseItemId;
            }

            var caseitems = new List<WorkFlowCaseItemInfo>();
            var handleusers = caseItemModel.HandleUser.Split(',');
            foreach (var handler in handleusers)
            {
                int handlerId = 0;
                if (!int.TryParse(handler, out handlerId))
                {
                    throw new Exception("步骤处理人字段格式错误");
                }
                var item = new WorkFlowCaseItemInfo()
                {
                    CaseItemId = Guid.NewGuid(),
                    CaseId = caseInfo.CaseId,
                    NodeId = nodeid,
                    NodeNum = caseInfo.NodeNum + 1,
                    StepNum = stepnum,
                    ChoiceStatus = ChoiceStatusType.AddNode,
                    CaseStatus = CaseStatusType.WaitApproval,
                    HandleUser = handlerId,
                    CopyUser = caseItemModel.CopyUser,
                    SkipNode = caseItemModel.SkipNode,
                    Suggest = string.Empty //为了适应驳回后重新发起的时候写入备注，这个需要跟第一次发起流程的时候区别开来
                };
                caseitems.Add(item);
            }
            var result = _workFlowRepository.AddCaseItem(caseitems, userinfo.UserId, AuditStatusType.Approving, trans);
            if (curNodeInfo.NodeType == NodeType.SpecialJoint)
            {
                _workFlowRepository.InsertSpecialJointComment(trans, new CaseItemJoint
                {
                    CaseItemid = preCaseItemId,
                    Comment = caseItemModel.Suggest,
                    UserId = userinfo.UserId,
                    FlowStatus = caseItemModel.JointStatus,
                }, userinfo.UserId);
            }
            //pxf 
            _workFlowRepository.CheckIsTransfer(trans, new CaseItemJointTransfer
            {
                CaseItemid = preCaseItemId,
                Comment = caseItemModel.Suggest,
                FlowStatus = caseItemModel.ChoiceStatus,
                UserId = userinfo.UserId,
                OrginUserId = userinfo.UserId
            }, userinfo.UserId);
        }

        int LookUpNextNodeData(DbTransaction trans, WorkFlowCaseInfo caseInfo, List<WorkFlowNodeInfo> flowNextNodeInfos, Guid nodeid, out bool isReturn, int parentIndex = -1)
        {
            isReturn = false;
            foreach (var tmp in flowNextNodeInfos)
            {
                if (tmp.NodeId == nodeid)//找到该nodeid证明是在某一分支下，然后返回该索引
                {
                    isReturn = true;
                    parentIndex++;
                    break;
                }
                var data = _workFlowRepository.GetNextNodeInfoList(trans, caseInfo.FlowId, caseInfo.VerNum, tmp.NodeId);
                if (data.Count > 0)
                {
                    LookUpNextNodeData(trans, caseInfo, data, nodeid, out isReturn, parentIndex);
                    if (isReturn) { parentIndex++; isReturn = true; break; }
                }
                parentIndex++;
            }
            return parentIndex;
        }
        #endregion

        #region --写入添加流程的消息--

        bool canWriteCaseMessage = false;

        /// <summary>
        /// 写入添加流程的消息
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="nodeNum"></param>
        /// <param name="userNumber"></param>
        /// <param name="type">代表1加签 3转办 4跳回驳回人等这些类型的消息特殊处理</param>
        public void WriteCaseAuditMessage(Guid caseId, int nodeNum, int stepNum, int userNumber, bool isFinishAfterStart = false, bool isAutoTerminal = false, int type = 0)
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    if (canWriteCaseMessage) break;
                    try
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                using (var conn = GetDbConnect())
                {
                    conn.Open();
                    var tran = conn.BeginTransaction();

                    try
                    {
                        List<Receiver> completedApprovers = new List<Receiver>(); //暂时为空，预留字段
                        string allApprovalSuggest = null;
                        bool isAddNextStep = false;//审批是否通过，并进入下一步审批人

                        //获取casedetail
                        WorkFlowCaseInfo caseInfo = null;
                        for (int i = 0; i < 10; i++)
                        {
                            caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseId);
                            if (caseInfo != null) break;
                            try
                            {
                                System.Threading.Thread.Sleep(100);
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                        if (caseInfo == null)
                        {
                            _logger.Error("写工作流消息:无法获取流程实例信息");
                            return;
                        }
                        if (type != 5)
                            stepNum = caseInfo.StepNum;
                        var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
                        if (workflowInfo == null)
                        {
                            _logger.Error("写工作流消息:无法获取流程定义信息");
                            return;
                        }
                        var caseItems = _workFlowRepository.CaseItemList(caseInfo.CaseId, userNumber, tran: tran);
                        bool isReOpenWorkFlow = false;
                        if (caseItems.Count > 2)
                        {
                            var tmpCaseItems = caseItems.OrderByDescending(t => Convert.ToInt32(t["stepnum"])).ToList();
                            var lastCaseItems = tmpCaseItems.Where(t => t["nodenum"] == tmpCaseItems[tmpCaseItems.Count() - 1]["nodenum"]);
                            int index = 0;
                            if (lastCaseItems.Count() == 1)
                            {
                                index = caseItems.Count - 2;
                            }
                            else if (lastCaseItems.Count() > 1)
                            {
                                index = caseItems.Count - 1 - lastCaseItems.Count();
                            }
                            isReOpenWorkFlow = caseItems[index]["nodenum"].ToString() == "0";
                        }
                        if (type != 1 && type != 3 && type != 4)
                        {
                            if (caseInfo.NodeNum != -1)
                            {
                                if (nodeNum == 0 || caseInfo.NodeNum == 0)
                                {
                                    nodeNum = 0;
                                }
                                else
                                {
                                    nodeNum = caseInfo.NodeNum - 1;
                                }

                                if (stepNum == 0 || caseInfo.StepNum == 0)
                                {
                                    stepNum = 0;
                                }
                                else if (caseInfo.StepNum == -1)
                                {
                                    stepNum = caseInfo.StepNum;
                                }
                                else
                                {
                                    stepNum = caseInfo.StepNum - 1;
                                }
                            }
                            else
                            {
                                ///发消息都是拿当前审批那条节点，然后通过状态判断去发消息，但是发起审批和拒绝审批的时候会产生两个节点需要区分
                                if (caseInfo.AuditStatus != AuditStatusType.NotAllowed)
                                {
                                    nodeNum = -1;
                                    stepNum = -1;
                                }
                                else
                                {
                                    var caseItem = caseItems.OrderByDescending(t => Convert.ToInt32(t["stepnum"])).FirstOrDefault(t => t["handleuser"].ToString() == userNumber.ToString() && t["choicestatus"].ToString() == "0");
                                    nodeNum = Convert.ToInt32(caseItem["nodenum"]);
                                    stepNum = Convert.ToInt32(caseItem["stepnum"]);
                                }
                            }
                        }
                        List<WorkFlowCaseItemInfo> caseitems = null;

                        if (nodeNum == 0 && stepNum > nodeNum)//处理退回消息
                        {
                            caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseId, Convert.ToInt32(caseItems[caseItems.Count - 2]["nodenum"]), Convert.ToInt32(caseItems[caseItems.Count - 2]["stepnum"]));
                        }
                        else
                        {
                            var items = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseId, caseInfo.NodeNum, caseInfo.StepNum);
                            if ((items.LastOrDefault().NodeType == 2 || items.LastOrDefault().NodeType == 1) && items.Count != items.Where(t => t.ChoiceStatus == ChoiceStatusType.AddNode).Count())
                            {
                                caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseId, caseInfo.NodeNum, caseInfo.StepNum);
                            }
                            else
                            {
                                caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseId, nodeNum, stepNum);
                            }
                        }
                        if (caseitems == null || caseitems.Count == 0)
                        {
                            _logger.Error("写工作流消息:无法获取流程实例节点信息");
                            return;
                        }
                        caseitems = caseitems.OrderByDescending(m => m.StepNum).ToList();

                        WorkFlowCaseItemInfo myAuditCaseItem;
                        if (isAutoTerminal == false)
                        {
                            if (type == 1)
                            {
                                myAuditCaseItem = caseitems.FirstOrDefault(m => m.TransferUserId == userNumber && m.NowuserId == m.HandleUser);
                            }
                            else if (type == 3)
                            {
                                myAuditCaseItem = caseitems.FirstOrDefault(m => m.SignUserId == userNumber && m.NowuserId == m.HandleUser);
                            }
                            else if (type == 2)
                            {
                                myAuditCaseItem = caseitems.LastOrDefault();
                            }
                            else
                            {
                                myAuditCaseItem = caseitems.FirstOrDefault(m => m.HandleUser == userNumber);
                                if (myAuditCaseItem == null)
                                {
                                    myAuditCaseItem = caseitems.LastOrDefault();
                                    if (myAuditCaseItem.ChoiceStatus == ChoiceStatusType.AddNode)
                                        myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Edit;
                                }
                            }
                        }
                        else
                        {
                            myAuditCaseItem = caseitems.FirstOrDefault();
                        }
                        if (myAuditCaseItem == null)
                        {
                            _logger.Error("写工作流消息:无法获取流程实例节点信息-我的信息");
                            return;
                        }

                        var entityInfotemp = _entityProRepository.GetEntityInfo(caseInfo.EntityId);
                        if (entityInfotemp == null)
                        {
                            _logger.Error("写工作流消息:无法获取流程关联的业务对象信息");
                            return;
                        }
                        var msg = new MessageParameter();
                        allApprovalSuggest = myAuditCaseItem.Suggest;
                        string funcode = null;
                        NodeType nodeType = NodeType.Normal;
                        if (workflowInfo.FlowType == WorkFlowType.FreeFlow)//自由流程
                        {
                            funcode = GetFreeFlowMessageFuncode(myAuditCaseItem, caseInfo, tran, out isAddNextStep);

                        }
                        else   //固定流程
                        {
                            var nodeid = caseitems.FirstOrDefault().NodeId;
                            var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);
                            nodeType = flowNodeInfo.NodeType;
                            if (flowNodeInfo == null)
                            {
                                _logger.Error("写工作流消息:流程节点不存在");
                                return;
                            }
                            if (flowNodeInfo.NodeType == NodeType.Normal)//普通审批
                            {
                                if (type == 1)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Transfer;
                                }
                                else if (type == 2 || type == -1)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Edit;
                                }
                                else if (type == 3)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Sign;
                                }
                                else if (type == 4)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.RejectNode;
                                }
                                funcode = GetNormalFlowMessageFuncode(myAuditCaseItem, caseInfo, tran, out isAddNextStep);
                            }
                            else if (flowNodeInfo.NodeType == NodeType.Joint) //会审
                            {
                                if (type == 1)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Transfer;
                                }
                                if (type == 2)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Edit;
                                }
                                else if (type == 3)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Sign;
                                }
                                allApprovalSuggest = string.Join(";", caseitems.Select(m => m.Suggest));
                                funcode = GetJointFlowMessageFuncode(myAuditCaseItem, caseInfo, caseitems, flowNodeInfo, tran, out isAddNextStep);
                            }
                            else //特殊会审 pxf
                            {
                                if (type == 1)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Transfer;
                                }
                                if (type == 2)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Edit;
                                }
                                else if (type == 3)
                                {
                                    myAuditCaseItem.ChoiceStatus = ChoiceStatusType.Sign;
                                }
                                allApprovalSuggest = string.Join(";", caseitems.Select(m => m.Suggest));
                                funcode = GetJointFlowMessageFuncode(myAuditCaseItem, caseInfo, caseitems, flowNodeInfo, tran, out isAddNextStep);
                            }
                        }
                        if (string.IsNullOrEmpty(funcode))//没有有效的消息模板funcode
                        {
                            _logger.Error("写工作流消息:没有有效的消息模板funcode");
                            return;
                        }
                        if (isReOpenWorkFlow && type == 4)
                        {
                            msg.FuncCode = "WorkFlowRepOpenLaunch";
                        }
                        else
                            msg.FuncCode = funcode;

                        #region --获取审批人和抄送人--
                        List<Receiver> approvers = new List<Receiver>();//审批人
                        List<Receiver> copyusers = new List<Receiver>();//抄送人
                        List<WorkFlowCaseItemInfo> tempcaseitems = caseitems;
                        if (isAddNextStep)//如果审批通过，进入下一步审批，则获取下一步的审批人和抄送人
                        {
                            if (caseInfo.NodeNum != nodeNum)
                            {
                                tempcaseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseId, caseInfo.NodeNum, caseInfo.StepNum);
                                tempcaseitems.ForEach(t =>
                                {
                                    if (t.ChoiceStatus == ChoiceStatusType.AddNode)
                                        t.ChoiceStatus = ChoiceStatusType.Edit;
                                });
                            }
                            else
                                tempcaseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseId, caseInfo.NodeNum, stepNum + 1);

                            if (tempcaseitems == null || tempcaseitems.Count == 0)
                                tempcaseitems = caseitems;
                        }

                        copyusers = _workFlowRepository.GetWorkFlowCopyUser(caseInfo.CaseId).Select(m => new Receiver
                        {
                            UserId = m.UserId,
                            ActRole = 11
                        }
                      ).ToList();
                        if (isAddNextStep == false && (funcode.Equals("WorkFlowNodeFinish") || funcode.Equals("WorkFlowNodeReject") ||
                                                        funcode.Equals("WorkFlowNodeStop") || funcode.Equals("WorkFlowNodeJointApproval")
                                                        || funcode.Equals("WorkFlowNodeJointReject")
                                                        || funcode.Equals("WorkFlowNodeJointFallback")))
                        {
                            if (myAuditCaseItem.NodeType == 0)
                            {
                                completedApprovers.AddRange(tempcaseitems.Select(m => new Receiver { UserId = m.HandleUser, ActRole = 10 }).Distinct().ToList());
                            }
                        }
                        else
                        {
                            if (type == 1)
                            {
                                approvers.Add(new Receiver
                                {
                                    UserId = myAuditCaseItem.HandleUser,
                                    ActRole = nodeType == NodeType.Normal ? 3 : 6
                                });
                                //approvers = approvers.Union(tempcaseitems.Where(t => t.HandleUser != myAuditCaseItem.HandleUser).Select(m => new Receiver
                                //{
                                //    UserId = m.HandleUser,
                                //    ActRole = nodeType == NodeType.Normal ? 1 : 4
                                //}).Distinct()).ToList();
                            }
                            else if (type == 3)
                            {
                                approvers.Add(new Receiver
                                {
                                    UserId = myAuditCaseItem.HandleUser,
                                    ActRole = nodeType == NodeType.Normal ? 2 : 5
                                });
                                //approvers = approvers.Union(tempcaseitems.Where(t => t.HandleUser != myAuditCaseItem.HandleUser).Select(m => new Receiver
                                //{
                                //    UserId = m.HandleUser,
                                //    ActRole = nodeType == NodeType.Normal ? 1 : 4
                                //}).Distinct()).ToList();
                            }
                            else
                                approvers = tempcaseitems.Select(m => new Receiver
                                {
                                    UserId = m.HandleUser,
                                    ActRole = nodeType == NodeType.Normal ? 1 : 4
                                }).Distinct().ToList();
                        }
                        //0实体消息推送，1审批 2 审批加签 3审批转办 4  会审 5会审加签 6会审转办 7 意见收集 8 意见收集加签 9 意见收集转办   10通知 11 抄送 12 传阅 13知会

                        #endregion


                        #region --封装MessageParameter--
                        msg.EntityId = entityInfotemp.EntityId;
                        msg.EntityName = entityInfotemp.EntityName;
                        msg.TypeId = entityInfotemp.CategoryId;
                        msg.BusinessId = caseInfo.RecId;
                        msg.RelEntityId = entityInfotemp.RelEntityId;
                        msg.RelEntityName = entityInfotemp.RelEntityName;
                        msg.RelBusinessId = caseInfo.RelRecId;
                        var caseItemList = _workFlowRepository.CaseItemList(caseId, userNumber, tran: tran);
                        var nodeInfos = _workFlowRepository.GetNodeInfoList(tran, caseInfo.FlowId, caseInfo.VerNum);
                        var nodeInfo = nodeInfos.FirstOrDefault(t => t.NodeId == Guid.Parse(caseItemList[caseItemList.Count - 1]["nodeid"].ToString()));
                        var subscriber = new List<Receiver>();
                        var informer = new List<Receiver>();
                        if (nodeInfo != null && nodeInfo.StepTypeId == NodeStepType.End)
                        {
                            subscriber = _workFlowRepository.GetSubscriber(myAuditCaseItem.CaseItemId, (int)caseInfo.AuditStatus, caseInfo, nodeInfo, tran, userNumber).Select(t => new Receiver
                            {
                                UserId = t,
                                ActRole = 12
                            }).ToList();
                            var informerDic = _workFlowRepository.GetInformer(caseInfo.FlowId, (int)caseInfo.AuditStatus == 2 ? 0 : 1, caseInfo, nodeInfo, tran, userNumber);
                            foreach (var t in informerDic)
                            {
                                bool result = this.ValidateInformerRule(caseInfo, t.Key, userNumber, tran);
                                if (result)
                                {
                                    informer = informer.Union(t.Value.Select(t1 => new Receiver { UserId = t1, ActRole = 13 })).ToList();
                                }
                            }
                            subscriber = subscriber.Where(t => !informer.Select(t1 => t1.UserId).ToList().Contains(t.UserId)).ToList();
                            copyusers.AddRange(subscriber);
                            copyusers = copyusers.Distinct().ToList();
                            copyusers = copyusers.Where(t => !informer.Select(t1 => t1.UserId).ToList().Contains(t.UserId)).ToList();
                            var caseitemList = _workFlowRepository.CaseItemList(caseInfo.CaseId, userNumber, tran: tran);
                        }
                        msg.NewReceivers = MessageService.GetWorkFlowMessageReceivers(caseInfo.RecCreator, approvers, copyusers, completedApprovers, informer, subscriber);
                        if (funcode.Equals("WorkFlowNodeJointApproval") || funcode.Equals("WorkFlowNodeJointReject") || funcode.Equals("WorkFlowNodeJointFallback") || funcode.Equals("WorkFlowNodeApproval"))
                        {
                            msg.NewReceivers[MessageUserType.WorkFlowCreateUser] = new List<Receiver>();
                        }
                        else if (funcode.Equals("WorkFlowNodeFallback"))
                        {
                            msg.NewReceivers[MessageUserType.WorkFlowCreateUser] = new List<Receiver> { new Receiver { UserId = caseInfo.RecCreator, ActRole = 1 } };
                        }
                        else if (funcode.Equals("WorkFlowNodeJointFallback") && myAuditCaseItem.NodeType == 1)
                        {
                            msg.NewReceivers[MessageUserType.WorkFlowCreateUser] = new List<Receiver> { new Receiver { UserId = caseInfo.RecCreator, ActRole = 4 } };
                        }
                        else if (funcode.Equals("WorkFlowNodeJointFallback") && myAuditCaseItem.NodeType == 2)
                        {
                            msg.NewReceivers[MessageUserType.WorkFlowCreateUser] = new List<Receiver> { new Receiver { UserId = caseInfo.RecCreator, ActRole = 4 } };
                        }
                        if (myAuditCaseItem.ChoiceStatus == ChoiceStatusType.Reback)
                        {
                            msg.NewReceivers[MessageUserType.WorkFlowNextApprover] = new List<Receiver>();
                        }
                        if (type == 1 || type == 3)
                        {
                            msg.NewReceivers[MessageUserType.WorkFlowCreateUser] = new List<Receiver>();
                        }
                        var msgParamData = new Dictionary<string, object>();
                        msgParamData.Add("caseid", caseInfo.CaseId.ToString());
                        msgParamData.Add("stepnum", (type == 1 || type == 3 || type == 4 ? stepNum : stepNum + 1));
                        msg.ParamData = JsonConvert.SerializeObject(msgParamData);

                        var users = new List<int>();
                        users.Add(userNumber);
                        users.Add(caseInfo.RecCreator);
                        users.AddRange(approvers.Select(t => t.UserId));
                        if (type == 1 || type == 3)
                        {
                            users.Add(myAuditCaseItem.NowuserId);
                        }
                        var userInfos = MessageService.GetUserInfoList(users.Distinct().ToList());


                        var paramData = new Dictionary<string, string>();
                        paramData.Add("operator", userInfos.FirstOrDefault(m => m.UserId == userNumber).UserName);
                        paramData.Add("launchUser", userInfos.FirstOrDefault(m => m.UserId == caseInfo.RecCreator).UserName);
                        paramData.Add("approvalUserNames", string.Join("、", userInfos.Where(m => approvers.Select(t => t.UserId).Contains(m.UserId)).Select(m => m.UserName)));
                        paramData.Add("workflowName", workflowInfo.FlowName);
                        paramData.Add("reccode", caseInfo.RecCode);
                        paramData.Add("approvalSuggest", myAuditCaseItem.Suggest);
                        paramData.Add("allApprovalSuggest", allApprovalSuggest);
                        paramData.Add("workflowtopic", caseInfo.Title);



                        msg.TemplateKeyValue = paramData;
                        msg.CopyUsers = copyusers.Select(t => t.UserId).ToList();
                        msg.ApprovalUsers = approvers.Select(t => t.UserId).ToList();
                        msg.FlowId = caseInfo.FlowId;
                        #endregion
                        //如果是动态实体，则需要发动态，
                        //流程新增和结束时候需要发送动态
                        #region --发送消息--
                        if ((entityInfotemp.ModelType == EntityModelType.Dynamic || entityInfotemp.ModelType == EntityModelType.Independent)
                                         && (msg.FuncCode == "WorkFlowLaunch" || caseInfo.AuditStatus != AuditStatusType.Approving))
                        {
                            //先发流程的审批消息，再发关联动态的消息
                            MessageService.WriteMessage(tran, msg, userNumber, isFlow: 1);

                            var detailMapper = new DynamicEntityDetailtMapper()
                            {
                                EntityId = msg.EntityId,
                                RecId = msg.BusinessId,
                                NeedPower = 0
                            };

                            string msgpParam = null;
                            if (entityInfotemp.ModelType == EntityModelType.Dynamic)
                            {
                                detailMapper.EntityId = msg.RelEntityId.GetValueOrDefault();
                                detailMapper.RecId = msg.RelBusinessId;
                                msgpParam = _dynamicRepository.GetDynamicTemplateData(msg.BusinessId, msg.EntityId, msg.TypeId, userNumber);
                            }

                            var detail = _dynamicEntityRepository.Detail(detailMapper, userNumber);
                            var newMembers = MessageService.GetEntityMember(detail as Dictionary<string, object>);



                            string dynamicFuncode = msg.FuncCode + "Dynamic";
                            var dynamicMsg = MessageService.GetEntityMsgParameter(entityInfotemp, msg.BusinessId, msg.RelBusinessId, dynamicFuncode, userNumber, newMembers, null, msgpParam);
                            foreach (var dmsg in dynamicMsg.TemplateKeyValue)
                            {
                                if (!paramData.ContainsKey(dmsg.Key))
                                {
                                    paramData.Add(dmsg.Key, dmsg.Value);
                                }
                            }

                            dynamicMsg.TemplateKeyValue = paramData;
                            //发布审批消息到实体动态列表
                            MessageService.WriteMessage(tran, dynamicMsg, userNumber, null);


                            if (entityInfotemp.ModelType == EntityModelType.Dynamic && (msg.FuncCode == "WorkFlowLaunch" || msg.FuncCode == "WorkFlowNodeTransfer"))
                            {
                                // 发布关联动态实体的动态消息
                                var dynamicMsgtemp = MessageService.GetEntityMsgParameter(entityInfotemp, msg.BusinessId, msg.RelBusinessId, "EntityDynamicAdd", userNumber, newMembers, null, msgpParam);

                                MessageService.WriteMessage(tran, dynamicMsgtemp, userNumber, null, isFlow: 1);

                                //发起流程时直接结束的场景
                                if (caseInfo.AuditStatus == AuditStatusType.Finished)
                                {
                                    msg.FuncCode = "WorkFlowNodeFinishDynamic";
                                    MessageService.WriteMessage(tran, msg, userNumber, isFlow: 1);
                                }
                            }


                        }

                        else MessageService.WriteMessage(tran, msg, userNumber, isFlow: 1);
                        #endregion
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                        conn.Dispose();
                    }
                }
                if (isFinishAfterStart)
                {
                    WriteCaseAuditMessage(caseId, -1, stepNum + 1, userNumber, false);//特殊处理，当且仅当固定流程，且启动后立即终止的情况
                }
                canWriteCaseMessage = false;
            });
        }
        #endregion

        #region --获取自由流程审批消息的funcode--
        private string GetFreeFlowMessageFuncode(WorkFlowCaseItemInfo auditCaseItem, WorkFlowCaseInfo caseInfo, DbTransaction trans, out bool isAddNextStep)
        {
            isAddNextStep = false;
            string funcCode = string.Empty;
            switch (auditCaseItem.ChoiceStatus)
            {

                case ChoiceStatusType.Edit:
                    if (caseInfo.AuditStatus == AuditStatusType.Begin || caseInfo.AuditStatus == AuditStatusType.Approving)//编辑重新发起
                    {
                        funcCode = "WorkFlowLaunch";
                        isAddNextStep = true;
                    }
                    break;
                case ChoiceStatusType.Approval://普通审批通过
                    if (caseInfo.AuditStatus == AuditStatusType.Finished)//审批完成
                    {
                        funcCode = "WorkFlowNodeFinish";
                    }
                    else
                    {
                        //获取下一步caseitem
                        var nextCaseItems = _workFlowRepository.GetWorkFlowCaseItemInfo(trans, caseInfo.CaseId, 1);

                        if (nextCaseItems == null || nextCaseItems.Count == 0)
                            return null;
                        funcCode = "WorkFlowNodeApproval";
                        isAddNextStep = true;
                    }
                    break;
                case ChoiceStatusType.Reback: //普通审批退回
                    funcCode = "WorkFlowNodeFallback";
                    break;
                case ChoiceStatusType.Refused://普通审批拒绝
                    funcCode = "WorkFlowNodeReject";
                    break;
                case ChoiceStatusType.Stop:
                    funcCode = "WorkFlowNodeStop";
                    break;
            }
            return funcCode;
        }

        #endregion

        #region --获取普通流程审批消息的funcode--
        private string GetNormalFlowMessageFuncode(WorkFlowCaseItemInfo auditCaseItem, WorkFlowCaseInfo caseInfo, DbTransaction trans, out bool isAddNextStep)
        {
            isAddNextStep = false;
            string funcCode = string.Empty;
            switch (auditCaseItem.ChoiceStatus)
            {
                case ChoiceStatusType.AddNode:
                case ChoiceStatusType.Edit:
                    if (caseInfo.AuditStatus == AuditStatusType.Begin || caseInfo.AuditStatus == AuditStatusType.Approving)//编辑重新发起
                    {
                        funcCode = "WorkFlowLaunch";
                        isAddNextStep = true;
                    }
                    if (caseInfo.AuditStatus == AuditStatusType.Finished)//审批完成
                    {
                        if (auditCaseItem.NodeNum == 0 && auditCaseItem.StepNum == 0)
                        {
                            funcCode = "WorkFlowLaunch";//这种情况是固定流程，但第一个节点结束后直接跳到最后一个节点，这时候应该发启动消息（动态），后面发结束消息
                        }
                        else
                        {

                            funcCode = "WorkFlowNodeFinish";
                        }
                    }
                    break;
                case ChoiceStatusType.Approval://普通审批通过
                    if (caseInfo.AuditStatus == AuditStatusType.Finished)//审批完成
                    {
                        funcCode = "WorkFlowNodeFinish";
                    }
                    else
                    {
                        //获取下一步caseitem
                        var nextCaseItems = _workFlowRepository.GetWorkFlowCaseItemInfo(trans, caseInfo.CaseId, caseInfo.NodeNum);

                        if (nextCaseItems == null || nextCaseItems.Count == 0)
                            return null;
                        funcCode = "WorkFlowNodeApproval";
                        isAddNextStep = true;
                    }
                    break;
                case ChoiceStatusType.Reback: //普通审批退回
                    funcCode = "WorkFlowNodeFallback";
                    break;
                case ChoiceStatusType.Refused://普通审批拒绝
                    funcCode = "WorkFlowNodeReject";
                    break;
                case ChoiceStatusType.Stop:
                    funcCode = "WorkFlowNodeStop";
                    break;
                case ChoiceStatusType.EndPoint:
                    funcCode = "WorkFlowNodeFinish";
                    break;
                case ChoiceStatusType.Transfer:
                    funcCode = "WorkFlowNodeTransfer";
                    break;
                case ChoiceStatusType.Sign:
                    funcCode = "WorkFlowNodeSign";
                    break;
                case ChoiceStatusType.WithDraw:
                    funcCode = "WorkFlowNodeWithDraw";
                    break;
                case ChoiceStatusType.RejectNode:
                    funcCode = "WorkFlowRejectNode";
                    break;

            }
            return funcCode;
        }

        #endregion

        #region --获取普通流程审批消息的funcode--
        private string GetJointFlowMessageFuncode(WorkFlowCaseItemInfo auditCaseItem, WorkFlowCaseInfo caseInfo, List<WorkFlowCaseItemInfo> caseitems, WorkFlowNodeInfo flowNodeInfo, DbTransaction trans, out bool isAddNextStep)
        {
            isAddNextStep = false;
            string funcCode = string.Empty;
            if (caseInfo.AuditStatus == AuditStatusType.Finished)//完成审批
            {
                funcCode = "WorkFlowNodeFinish";
                return funcCode;
            }

            //判断是否还有人未完成审批
            bool hasNotApprovalFinish = caseitems.Exists(m => m.CaseStatus == CaseStatusType.WaitApproval || m.CaseStatus == CaseStatusType.Readed);
            //会审审批通过的节点数
            var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();

            switch (auditCaseItem.ChoiceStatus)
            {
                case ChoiceStatusType.Edit:
                    if (caseInfo.AuditStatus == AuditStatusType.Begin || caseInfo.AuditStatus == AuditStatusType.Approving)//编辑操作
                    {
                        funcCode = "WorkFlowLaunch";
                        isAddNextStep = true;
                    }
                    if (caseInfo.AuditStatus == AuditStatusType.Finished)//审批完成
                    {
                        funcCode = "WorkFlowLaunch";
                    }
                    break;
                case ChoiceStatusType.Approval:
                    if (aproval_success_count >= flowNodeInfo.AuditSucc)//满足会审条件，审批通过
                    {
                        funcCode = "NextWorkFlowNodeJointApproval";
                        isAddNextStep = true;
                    }
                    else//某个会审人审批通过
                    {
                        funcCode = "WorkFlowNodeJointApproval";
                    }
                    break;
                case ChoiceStatusType.Reback: //
                    if (caseInfo.NodeNum == 0)
                    {
                        funcCode = "FinishWorkFlowNodeJointFallback";//退回发起人
                    }
                    else funcCode = "WorkFlowNodeJointFallback";//某个会审人审批退回
                    break;
                case ChoiceStatusType.Refused://审批拒绝
                    if (caseInfo.NodeNum == -1)
                    {
                        funcCode = "FinishWorkFlowNodeJointRejectk";//全部完成了，流程拒绝
                    }
                    else funcCode = "WorkFlowNodeJointReject";//其中某个会审人拒绝
                    break;
                case ChoiceStatusType.Stop:
                    funcCode = "WorkFlowNodeStop";
                    break;
                case ChoiceStatusType.Transfer:
                    funcCode = "WorkFlowNodeTransfer";
                    break;
                case ChoiceStatusType.Sign:
                    funcCode = "WorkFlowNodeSign";
                    break;

            }
            return funcCode;
        }

        #endregion

        #region --验证分支流程节点是否符合分支条件--
        private bool ValidateNextNodeRule(WorkFlowCaseInfo caseinfo, Guid flowid, Guid fromnodeid, Guid tonodeid, int vernum, UserInfo userinfo, out bool isDefaultNode, DbTransaction trans = null)
        {

            var ruleid = _workFlowRepository.GetNextNodeRuleId(flowid, fromnodeid, tonodeid, vernum, trans);
            isDefaultNode = ruleid == Guid.Empty;
            if (isDefaultNode)//如果是默认分支条件，则表示不设定过滤条件，全部通过
                return true;
            var ruleInfoList = _ruleRepository.GetRule(ruleid, userinfo.UserId);
            if (ruleInfoList != null && ruleInfoList.Count > 0)//如果存在合法的rulesql，则进行数据校验
            {
                var ruleInfo = ruleInfoList.FirstOrDefault();
                var temp = ruleInfo.Rulesql;
                var rulesql = string.IsNullOrEmpty(temp) ? "1=1" : string.Format("({0})", temp);
                var departid = GetUserData(userinfo.UserId).AccountUserInfo.DepartmentId;
                var ruleFormatSql = RuleSqlHelper.FormatRuleSql(rulesql, userinfo.UserId, departid);

                return _workFlowRepository.ValidateNextNodeRule(caseinfo, ruleFormatSql, userinfo.UserId, trans);
            }

            return true;
        }

        #endregion



        public OutputResult<object> CaseItemList(WorkFlowAuditCaseItemListModel listModel, int userNumber)
        {
            if (listModel?.CaseId == null)
            {
                return ShowError<object>("流程事件ID不能为空");
            }
            var result = _workFlowRepository.CaseItemList(listModel.CaseId, userNumber, listModel.IsSkipNode);
            List<Dictionary<string, object>> result_ext = new List<Dictionary<string, object>>();
            var dicCount = result.GroupBy(t => t["nodeid"]).Select(t => new
            {
                nodeid = t.Key,
                count = t.Count()
            });
            int index = 0;
            foreach (var t in result)
            {
                if (t["nodenum"].ToString() == "-1" || (t["nodenum"].ToString() == "0" && index == 0)) continue;
                if (t["nodetype"].ToString() == "2")
                {
                    var result_ext_comment = _workFlowRepository.GetSpecialJointCommentDetail(null, Guid.Parse(t["caseitemid"].ToString()), userNumber);
                    if (result_ext_comment != null && result_ext_comment.Count > 0)
                    {
                        result_ext_comment.ForEach(t1 =>
                        {
                            var data = t1["filejson"];
                            if (data != null)
                                t1["filejson"] = JArray.Parse(data.ToString());
                        });
                        result_ext = result_ext_comment.Union(result_ext).ToList();
                    }
                    else
                    {
                        var caseItemInfo = new Dictionary<string, object>();
                        caseItemInfo.Add("nodeid", t["nodeid"]);
                        caseItemInfo.Add("comment", string.Empty);
                        caseItemInfo.Add("filejson", null);
                        caseItemInfo.Add("flowstatus", t["casestatus"] == null ? string.Empty : t["casestatus"].ToString());
                        caseItemInfo.Add("reccreated", t["reccreated"]);
                        caseItemInfo.Add("username", t["username"]);
                        caseItemInfo.Add("caseitemid", t["caseitemid"]);
                        caseItemInfo.Add("usericon", t["usericon"]);
                        result_ext.Add(caseItemInfo);
                    }
                }

                var result_ext_joint = _workFlowRepository.GetWorkFlowCaseTransferAtt(null, Guid.Parse(t["caseitemid"].ToString()), userNumber);
                if (result_ext_joint != null && result_ext_joint.Count > 0)
                {
                    result_ext_joint.ForEach(t1 =>
                    {
                        var data = t1["filejson"];
                        if (data != null)
                            t1["filejson"] = JArray.Parse(data.ToString());
                    });
                    //var caseItemInfo = new Dictionary<string, object>();
                    //caseItemInfo.Add("nodeid", t["nodeid"]);
                    //caseItemInfo.Add("comment", string.Empty);
                    //caseItemInfo.Add("filejson", null);
                    //caseItemInfo.Add("flowstatus", t["casestatus"] == null ? string.Empty : t["casestatus"].ToString());
                    //caseItemInfo.Add("reccreated", t["reccreated"]);
                    //caseItemInfo.Add("username", t["username"]);
                    //caseItemInfo.Add("caseitemid", t["caseitemid"]);
                    //result_ext_joint.Add(caseItemInfo);
                    result_ext = result_ext_joint.Union(result_ext).ToList();
                }

                var result_ext_joint_1 = _workFlowRepository.GetWorkFlowCaseAtt(null, Guid.Parse(t["caseitemid"].ToString()), userNumber);
                if (result_ext_joint_1 != null && result_ext_joint_1.Count > 0)
                {
                    result_ext_joint_1.ForEach(t1 =>
                    {
                        var data = t1["filejson"];
                        if (data != null)
                            t1["filejson"] = JArray.Parse(data.ToString());
                    });
                    result_ext = result_ext.Union(result_ext_joint_1).ToList();
                }
                if (t["nodetype"].ToString() == "0" || t["nodetype"].ToString() == "1")
                {
                    var data = result_ext_joint_1.FirstOrDefault(t1 => t1["caseitemid"].ToString() == t["caseitemid"].ToString());
                    if (data != null)
                        t.Add("filejson", data["filejson"]);
                }
                if (index == 0)
                    index++;
            }
            var obj = new
            {
                result,
                result_ext
            };
            return new OutputResult<object>(obj);
        }

        public OutputResult<object> NodeLineInfo(WorkFlowNodeLinesInfoModel nodeLineModel, int userNumber)
        {
            if (nodeLineModel?.FlowId == null)
            {
                return ShowError<object>("流程ID不能为空");
            }

            var result = _workFlowRepository.NodeLineInfo(nodeLineModel.FlowId, userNumber);
            return new OutputResult<object>(result);
        }
        public OutputResult<object> GetNodeLinesInfo(WorkFlowNodeLinesInfoModel nodeLineModel, int userNumber)
        {
            if (nodeLineModel?.FlowId == null)
            {
                return ShowError<object>("流程ID不能为空");
            }
            var result = _workFlowRepository.GetNodeLinesInfo(nodeLineModel.FlowId, userNumber, versionNum: nodeLineModel.VersionNum);
            return new OutputResult<object>(result);
        }



        public OutputResult<object> SaveNodeLinesConfig(WorkFlowNodeLinesConfigModel configModel, int userNumber)
        {
            //获取该实体分类的字段
            var configEntity = _mapper.Map<WorkFlowNodeLinesConfigModel, WorkFlowNodeLinesConfigMapper>(configModel);
            if (configEntity == null || !configEntity.IsValid())
            {
                return HandleValid(configEntity);
            }
            _workFlowRepository.SaveNodeLinesConfig(configEntity, userNumber);
            IncreaseDataVersion(DataVersionType.FlowData, null);
            return new OutputResult<object>("");
        }
        public OutputResult<object> GetFreeFlowNodeEvents(GetFreeFlowEventModel configModel, int userNumber)
        {
            if (configModel == null)
            {
                return ShowError<object>("参数不可为空");
            }
            WorkFlowInfo workFlowInfo = this._workFlowRepository.GetWorkFlowInfo(null, configModel.FlowId);
            if (workFlowInfo == null)
                return ShowError<object>("流程不存在");
            else if (workFlowInfo.FlowType == WorkFlowType.FixedFlow)
                return ShowError<object>("该流程不是自由流程，无法获取Event函数");

            var result = _workFlowRepository.GetFreeFlowNodeEvents(configModel.FlowId, null);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> SaveFreeFlowNodeEvents(FreeFlowEventModel configModel, int userNumber)
        {
            if (configModel == null)
            {
                return ShowError<object>("参数不可为空");
            }
            WorkFlowInfo workFlowInfo = this._workFlowRepository.GetWorkFlowInfo(null, configModel.FlowId);
            if (workFlowInfo == null)
                return ShowError<object>("流程不存在");
            else if (workFlowInfo.FlowType == WorkFlowType.FixedFlow)
                return ShowError<object>("该流程不是自由流程，保存失败");
            List<WorkFlowNodeMapper> nodes = new List<WorkFlowNodeMapper>();
            nodes.Add(new WorkFlowNodeMapper()
            {
                NodeId = freeFlowBeginNodeId,
                NodeEvent = configModel.BeginNodeFunc,
                StepTypeId = 0,
            });
            nodes.Add(new WorkFlowNodeMapper()
            {
                NodeId = freeFlowEndNodeId,
                NodeEvent = configModel.EndNodeFunc,
                StepTypeId = -1,
            });
            _workFlowRepository.SaveNodeEvents(configModel.FlowId, nodes);
            IncreaseDataVersion(DataVersionType.FlowData, null);
            return new OutputResult<object>("OK");
        }


        public OutputResult<object> FlowList(WorkFlowListModel listModel, int userNumber)
        {
            if (listModel?.FlowStatus == null)
            {
                return ShowError<object>("流程状态不能为空");
            }

            var pageParam = new PageParam { PageIndex = listModel.PageIndex, PageSize = listModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }
            var result = _workFlowRepository.FlowList(pageParam, listModel.FlowStatus, listModel.SearchName, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> Detail(WorkFlowDetailModel detailModel, int userNumber)
        {
            if (detailModel?.FlowId == null)
            {
                return ShowError<object>("流程ID不能为空");
            }
            var result = _workFlowRepository.Detail(detailModel.FlowId, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> AddFlow(WorkFlowAddModel flowModel, int userNumber)
        {
            //获取该实体分类的字段
            var flowEntity = _mapper.Map<WorkFlowAddModel, WorkFlowAddMapper>(flowModel);
            string FlowName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(flowEntity.FlowName, flowEntity.FlowName_Lang, out FlowName);
            if (FlowName != null) flowEntity.FlowName = FlowName;
            if (flowEntity == null || !flowEntity.IsValid())
            {
                return HandleValid(flowEntity);
            }
            var result = _workFlowRepository.AddFlow(flowEntity, userNumber);
            RemoveCommonCache();
            RemoveAllUserCache();
            IncreaseDataVersion(DataVersionType.FlowData);
            IncreaseDataVersion(DataVersionType.PowerData);
            return HandleResult(result);
        }

        public OutputResult<object> UpdateFlow(WorkFlowUpdateModel flowModel, int userNumber)
        {
            //获取该实体分类的字段
            var flowEntity = _mapper.Map<WorkFlowUpdateModel, WorkFlowUpdateMapper>(flowModel);
            string FlowName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(flowEntity.FlowName, flowEntity.FlowName_Lang, out FlowName);
            if (FlowName != null) flowEntity.FlowName = FlowName;
            if (flowEntity == null || !flowEntity.IsValid())
            {
                return HandleValid(flowEntity);
            }
            var result = _workFlowRepository.UpdateFlow(flowEntity, userNumber);
            IncreaseDataVersion(DataVersionType.FlowData, null);
            return HandleResult(result);
        }

        public OutputResult<object> DeleteFlow(WorkFLowDeleteModel flowModel, int userNumber)
        {
            if (string.IsNullOrWhiteSpace(flowModel?.FlowIds))
            {
                return ShowError<object>("流程ID不能为空");
            }

            var result = _workFlowRepository.DeleteFlow(flowModel.FlowIds, userNumber);
            IncreaseDataVersion(DataVersionType.FlowData, null);
            return HandleResult(result);
        }
        public OutputResult<object> UnDeleteFlow(WorkFLowDeleteModel flowModel, int userNumber)
        {
            if (string.IsNullOrWhiteSpace(flowModel?.FlowIds))
            {
                return ShowError<object>("流程ID不能为空");
            }
            var result = _workFlowRepository.UnDeleteFlow(flowModel.FlowIds, userNumber);

            IncreaseDataVersion(DataVersionType.FlowData, null);
            return HandleResult(result);
        }

        public List<WorkFlowCaseInfo> getWorkFlowCaseListByRecId(string recid, int userNumber, DbTransaction transaction)
        {
            return _workFlowRepository.getWorkFlowCaseListByRecId(transaction, recid, userNumber);
        }



        /// <summary>
        /// 用于定时事务，计算需要终止的节点
        /// </summary>
        /// <param name="userId"></param>
        public void AutoTerminateWorkflowCases()
        {
            try
            {
                int userId = 1;
                DbTransaction tran = null;
                List<WorkFlowCaseInfo> cases = this._workFlowRepository.GetExpiredWorkflowCaseList(tran, userId);
                foreach (WorkFlowCaseInfo caseInfo in cases)
                {
                    TerminateWorkflowCase(caseInfo.CaseId, userId);
                    break;
                }
            }
            catch (Exception ex)
            {

            }

        }
        /// <summary>
        /// 终止流程
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="userId"></param>
        private void TerminateWorkflowCase(Guid caseId, int userId)
        {
            bool isOK = false;
            int stepnum = 0;
            int nodeNum = 0;
            using (var conn = GetDbConnect(null))
            {
                DbTransaction tran = null;
                try
                {
                    conn.Open();
                    tran = conn.BeginTransaction();
                    var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseId);
                    if (caseInfo == null)
                        throw new Exception("流程表单数据不存在");
                    var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
                    if (workflowInfo == null)
                        throw new Exception("流程配置不存在");
                    List<WorkFlowCaseItemInfo> caseitems = _workFlowRepository.GetWorkflowCaseWaitingDealItems(tran, caseId);
                    if (caseitems == null || caseitems.Count == 0)
                        throw new Exception("流程节点不存在");
                    //对所有节点进行更新处理

                    _workFlowRepository.TerminateCase(tran, caseId);
                    stepnum = caseitems.FirstOrDefault().StepNum;
                    nodeNum = caseitems.FirstOrDefault().NodeNum;
                    Guid nodeid = caseitems.FirstOrDefault().NodeId;
                    int ChoiceStatus = 3;
                    WorkFlowEventInfo eventInfo = null;
                    //如果不是自由流程，或者自由流程的第一个节点，需要验证是否有附加函数
                    #region 执行节点脚本
                    if (workflowInfo.FlowType != WorkFlowType.FreeFlow)
                    {
                        //判断是否有附加函数_event_func
                        eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, nodeid, tran);
                        _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, caseInfo.NodeNum, ChoiceStatus, userId, tran);
                    }
                    #endregion
                    #region 执行流程结束脚本
                    var nodelist = _workFlowRepository.GetNodeInfoList(tran, caseInfo.FlowId, caseInfo.VerNum);
                    WorkFlowNodeInfo endnode = nodelist.Find(m => m.StepTypeId == NodeStepType.End);
                    if (endnode != null)
                    {
                        _workFlowRepository.EndWorkFlowCaseItem(caseInfo.CaseId, endnode.NodeId, stepnum + 1, userId, tran);
                        eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, endnode.NodeId, tran);
                        _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, -1, ChoiceStatus, userId, tran);
                    }

                    #endregion
                    tran.Commit();
                    tran = null;
                    isOK = true;
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (tran != null)
                    {
                        tran.Rollback();
                        tran = null;
                    }
                }

            }
            #region 写终止消息
            if (isOK)
                WriteCaseAuditMessage(caseId, nodeNum, stepnum, userId, false, true);
            #endregion
        }
        public OutputResult<object> TransferToOther(CaseItemTransfer transfer, int userId, IServiceProvider service)
        {
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var transferEntity = _mapper.Map<CaseItemTransfer, CaseItemTransferMapper>(transfer);
                    var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(transaction, transferEntity.CaseId);
                    var workFlowCaseItem = _workFlowRepository.GetWorkFlowCaseItemInfo(transaction, transferEntity.CaseId, caseInfo.NodeNum);
                    var nowcaseitem = workFlowCaseItem.Find(m => m.HandleUser == userId);
                    if (caseInfo.NodeNum == 0 && nowcaseitem != null && (nowcaseitem.ChoiceStatus == ChoiceStatusType.Edit || nowcaseitem.ChoiceStatus == ChoiceStatusType.AddNode))
                    {
                        var userInfo = _accountRepository.GetUserInfoById(userId);
                        this.SubmitWorkFlowAudit(new WorkFlowAuditCaseItemModel
                        {
                            CaseId = transferEntity.CaseId,
                            NodeNum = transferEntity.NodeNum,
                            Suggest = transferEntity.Suggest,
                            CaseData = new Dictionary<string, object>(),
                            ChoiceStatus = 4,
                            NodeId = transferEntity.NodeId,
                            HandleUser = transferEntity.UserId.ToString(),
                            Files = transfer.Files
                        }, new UserInfo
                        {
                            UserId = userInfo.UserId,
                            UserName = userInfo.UserName
                        }, transaction);
                        caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(transaction, transferEntity.CaseId);
                        workFlowCaseItem = _workFlowRepository.GetWorkFlowCaseItemInfo(transaction, transferEntity.CaseId, caseInfo.NodeNum);
                        nowcaseitem = workFlowCaseItem.Find(m => m.HandleUser == transferEntity.UserId);
                        transferEntity.CaseItemId = nowcaseitem.CaseItemId;
                    }
                    if (nowcaseitem.NodeType == 2 && transferEntity.ChoiceStatus == 0)
                    {
                        transferEntity.ChoiceStatus = 4;
                    }
                    var data = _workFlowRepository.TransferToOther(transaction, transferEntity, userId);
                    var result = _workFlowRepository.InsertTransfer(transaction, new CaseItemJointTransfer
                    {
                        CaseItemid = transferEntity.CaseItemId,
                        OrginUserId = userId,
                        UserId = transferEntity.UserId,
                        Comment = transferEntity.Suggest,
                        IsSignOrTransfer = transferEntity.IsSignOrTransfer,
                        FlowStatus = transferEntity.IsSignOrTransfer == 1 ? (transferEntity.ChoiceStatus == 1 ? 17 : (transferEntity.ChoiceStatus == 0 ? 27 : 7)) : 7,
                        SignStatus = transfer.SignStatus//加签的情况下，17是同意，27是不同意
                    }, userId);
                    foreach (var t in transferEntity.Files)
                    {
                        _workFlowRepository.InsertCaseItemAttach(transaction, new CaseItemFileAttach
                        {
                            CaseItemId = transferEntity.CaseItemId,
                            FileId = t.FileId,
                            FileName = t.FileName,
                            RecId = Guid.Parse(result.Id)
                        }, userId);
                    }
                    MessageService.UpdateWorkflowNodeMessage(transaction, caseInfo.RecId, caseInfo.CaseId, nowcaseitem.StepNum, 4, userId);
                    transaction.Commit();

                    WriteCaseAuditMessage(caseInfo.CaseId, caseInfo.NodeNum, nowcaseitem.StepNum, userId, type: transferEntity.IsSignOrTransfer == 0 ? 1 : 3);
                    return HandleResult(data);
                }
                catch (Exception ex)
                {
                    if (transaction != null && transaction.Connection != null)
                        transaction.Rollback();
                    if (!string.IsNullOrEmpty(ex.Message))
                        throw new Exception(ex.Message);
                    throw new Exception("转办异常");
                }
            }

        }

        public OutputResult<object> NeedToRepeatApprove(WorkFlowRepeatApproveModel workFlow, int userId)
        {
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var workFlowEntity = _mapper.Map<WorkFlowRepeatApproveModel, WorkFlowRepeatApprove>(workFlow);
                    SimpleEntityInfo entityInfo = null;
                    if (workFlow.EntityModel.TypeId != Guid.Empty)
                        entityInfo = _entityProRepository.GetEntityInfo(workFlow.EntityModel.TypeId);
                    else
                        entityInfo = _entityProRepository.GetEntityInfo(workFlow.EntityId);

                    UserData userData = GetUserData(userId);

                    WorkFlowAddCaseModel workFlowAddCaseModel = null;
                    if (entityInfo.ModelType == EntityModelType.Dynamic)
                    {
                        if (!workFlow.EntityModel.FieldData.ContainsKey("recrelateid"))
                        {
                            workFlow.EntityModel.FieldData.Add("recrelateid", workFlow.EntityModel.RelRecId);
                        }
                        workFlow.EntityModel.FlowId = workFlow.FlowId;
                        workFlow.EntityModel.TypeId = workFlow.EntityId;
                        var entityResult = _dynamicEntityServices.AddEntityData(transaction, userData, entityInfo, workFlow.EntityModel, header, userId, out workFlowAddCaseModel);
                        if (entityResult.Status == 1) throw new Exception(entityResult.Message);
                        workFlowEntity.RecId = workFlowAddCaseModel.RecId;
                        workFlowEntity.ModelType = (int)EntityModelType.Dynamic;
                    }
                    else if (entityInfo.ModelType == EntityModelType.Simple || entityInfo.ModelType == EntityModelType.Independent)
                    {
                        _dynamicEntityRepository.DynamicEdit(transaction, workFlow.EntityModel.TypeId, workFlow.EntityModel.RelRecId.Value, workFlow.EntityModel.FieldData, userId);
                    }
                    workFlowEntity.CaseId = _workFlowRepository.GetLastestCaseId(transaction, workFlowEntity, userId);
                    var data = _workFlowRepository.NeedToRepeatApprove(transaction, workFlowEntity, userId);
                    if (data.Flag == 0)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(transaction, Guid.Parse(data.Id));
                        var workFlowCaseItem = _workFlowRepository.GetWorkFlowCaseItemInfo(transaction, Guid.Parse(data.Id), caseInfo.NodeNum);
                        var nowcaseitem = workFlowCaseItem.LastOrDefault(t => t.CaseStatus == CaseStatusType.WaitApproval || t.ChoiceStatus == ChoiceStatusType.AddNode);
                        transaction.Commit();
                        WriteCaseAuditMessage(caseInfo.CaseId, caseInfo.NodeNum, nowcaseitem.StepNum, userId, type: 2);
                    }

                    return HandleResult(data);
                }
                catch (Exception ex)
                {
                    if (transaction != null && transaction.Connection != null)
                        transaction.Rollback();
                    if (!string.IsNullOrEmpty(ex.Message))
                        throw new Exception(ex.Message);
                    throw new Exception("重新发起异常");
                }
            }
        }
        public OutputResult<object> CheckIfExistNeedToRepeatApprove(WorkFlowRepeatApproveModel workFlow, int userId)
        {
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var workFlowEntity = _mapper.Map<WorkFlowRepeatApproveModel, WorkFlowRepeatApprove>(workFlow);
                    var data = _workFlowRepository.CheckIfExistNeedToRepeatApprove(transaction, workFlowEntity, userId);
                    if (data.Flag == 0)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        transaction.Commit();
                    }
                    return HandleResult(data);
                }
                catch (Exception ex)
                {
                    if (transaction != null && transaction.Connection != null)
                        transaction.Rollback();
                    if (!string.IsNullOrEmpty(ex.Message))
                        throw new Exception(ex.Message);
                    throw new Exception("检查流程是否发起过异常");
                }
            }
        }

        public List<WorkFlowNodeScheduledList> GetWorkFlowNodeScheduled(DbTransaction trans, int userId)
        {
            return _workFlowRepository.GetWorkFlowNodeScheduled(trans, userId);
        }

        public OutputResult<object> SaveWorkflowInformer(InformerModel informerModel, int userId)
        {
            _ruleTranslatorServices.header = this.header;
            foreach (var t in informerModel.InformerRules)
            {
                var ruleResult = _ruleTranslatorServices.SaveRule(t.Rule, userId);
                if (ruleResult.Status == 1) { break; }
                t.RuleId = Guid.Parse(ruleResult.DataBody.ToString());
            }
            var informer = _mapper.Map<InformerModel, InformerMapper>(informerModel);
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    _workFlowRepository.UpdateWorkflowInformerStatus(transaction, new InformerRuleMapper
                    {
                        FlowId = informer.FlowId,
                        RecStatus = 0
                    }, userId);
                    foreach (var t in informer.InformerRules)
                    {
                        int auditstatus = string.IsNullOrEmpty(_workFlowRepository.GetRuleConfigInfo("endnodeconfig/allpass", t.RuleConfig)) ?
                            (string.IsNullOrEmpty(_workFlowRepository.GetRuleConfigInfo("endnodeconfig/failed", t.RuleConfig)) ?
                            (string.IsNullOrEmpty(_workFlowRepository.GetRuleConfigInfo("endnodeconfig/approve", t.RuleConfig)) ? -1 : 1) : 2) : 0;
                        var result = _workFlowRepository.SaveWorkflowInformer(transaction, new InformerRuleMapper
                        {
                            RuleId = t.RuleId,
                            FlowId = t.FlowId,
                            InformerType = t.InformerType,
                            AuditStatus = auditstatus,
                            RuleConfig = t.RuleConfig
                        }, userId);
                        if (result.Flag == 0)
                        {
                            transaction.Rollback();
                            break;
                        }
                    }
                    if (transaction != null && transaction.Connection != null)
                        transaction.Commit();
                    return HandleResult(new OperateResult { Flag = 1, Msg = "规则保存成功" });
                }
                catch (Exception ex)
                {
                    if (transaction != null && transaction.Connection != null)
                        transaction.Rollback();
                    if (!string.IsNullOrEmpty(ex.Message))
                        throw new Exception(ex.Message);
                    throw new Exception("规则失败");
                }
            }
        }
        public OutputResult<object> GetWorkflowInformerRules(InformerRuleModel informerRuleModel, int userId)
        {
            var informerRule = _mapper.Map<InformerRuleModel, InformerRuleMapper>(informerRuleModel);
            var informerRules = _workFlowRepository.GetInformerRules(null, informerRule, userId);
            List<dynamic> dynamics = new List<dynamic>();
            foreach (var t in informerRules)
            {
                var rule = _ruleRepository.GetRule(t.RuleId, userId);
                int informertype = 0;
                int type = 0;
                string value = _workFlowRepository.GetRuleConfigInfo(string.Format("endnodeconfig/{0}", "allpass"), t.RuleConfig);
                if (string.IsNullOrEmpty(value))
                {
                    value = _workFlowRepository.GetRuleConfigInfo(string.Format("endnodeconfig/{0}", "approve"), t.RuleConfig); informertype = 1;
                }
                if (string.IsNullOrEmpty(value))
                {
                    value = _workFlowRepository.GetRuleConfigInfo(string.Format("endnodeconfig/{0}", "failed"), t.RuleConfig); informertype = 2;
                }
                type = Convert.ToInt32(_workFlowRepository.GetRuleConfigInfo("type", value));
                string funcName = string.Empty;
                dynamic dynUser = null;
                dynamic dynReportRelation = null;
                switch (type)
                {
                    case 1:
                        dynUser = new
                        {
                            userids = _workFlowRepository.GetRuleConfigInfo("userids", value),
                            usernames = _workFlowRepository.GetRuleConfigInfo("usernames", value)
                        };
                        break;
                    case 2:
                        funcName = _workFlowRepository.GetRuleConfigInfo("spfuncname", value);
                        break;
                    case 3:
                        dynReportRelation = new
                        {
                            type = _workFlowRepository.GetRuleConfigInfo("reportrelation/type", value),
                            id = _workFlowRepository.GetRuleConfigInfo("reportrelation/id", value),
                            entityid = _workFlowRepository.GetRuleConfigInfo("reportrelation/entityid", value),
                            fieldname = _workFlowRepository.GetRuleConfigInfo("reportrelation/fieldname", value),
                            fieldlabel = _workFlowRepository.GetRuleConfigInfo("reportrelation/fieldlabel", value)
                        };
                        break;
                }
                dynamic dyn = new
                {
                    flowid = informerRule.FlowId,
                    ruleId = informerRule.RuleId,
                    informertype = informertype,
                    type = type,
                    rules = rule,
                    user = dynUser,
                    reportrelation = dynReportRelation,
                    funcname = funcName
                };
                dynamics.Add(dyn);
            }
            return new OutputResult<object>(dynamics);
        }
        private bool ValidateInformerRule(WorkFlowCaseInfo caseinfo, Guid ruleId, int userId, DbTransaction trans = null)
        {
            var ruleInfoList = _ruleRepository.GetRule(ruleId, userId, trans);
            if (ruleInfoList != null && ruleInfoList.Count > 0)//如果存在合法的rulesql，则进行数据校验
            {
                var ruleInfo = ruleInfoList.FirstOrDefault();
                var temp = ruleInfo.Rulesql;
                var rulesql = string.IsNullOrEmpty(temp) ? "1=1" : string.Format("({0})", temp);
                var departid = GetUserData(userId).AccountUserInfo.DepartmentId;
                var ruleFormatSql = RuleSqlHelper.FormatRuleSql(rulesql, userId, departid);

                return _workFlowRepository.ValidateNextNodeRule(caseinfo, ruleFormatSql, userId, trans);
            }
            return true;
        }
        public OutputResult<object> RejectToOrginalNode(RejectToOrginalNodeModel rejectToOrginalNodeModel, int userId)
        {
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {
                    var reject = _mapper.Map<RejectToOrginalNodeModel, RejectToOrginalNode>(rejectToOrginalNodeModel);
                    var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(transaction, reject.CaseId);
                    var caseItemList = _workFlowRepository.CaseItemList(caseInfo.CaseId, userId, tran: transaction);
                    reject.PreCaseItemId = Guid.Parse(caseItemList[caseItemList.Count - 1]["caseitemid"].ToString());
                    var nowCaseItem = _workFlowRepository.GetWorkFlowCaseItemInfo(transaction, reject.CaseId, Convert.ToInt32(caseItemList[caseItemList.Count - 2]["nodenum"]));
                    _workFlowRepository.AuditWorkFlowCaseData(new WorkFlowAuditCaseItemMapper
                    {
                        CaseItemId = reject.CaseItemId,
                        CaseData = reject.CaseData
                    }, nowCaseItem.FirstOrDefault(), userId, transaction);
                    //流程审批过程修改实体字段时，更新关联实体的字段数据
                    _workFlowRepository.ExecuteUpdateWorkFlowEntity(caseInfo.CaseId, Convert.ToInt32(caseItemList[caseItemList.Count - 2]["nodenum"]), Convert.ToInt32(caseItemList[caseItemList.Count - 2]["handleuser"]), transaction);
                    var result = _workFlowRepository.RejectToOrginalNode(transaction, reject, userId);
                    MessageService.UpdateWorkflowNodeMessage(transaction, caseInfo.RecId, caseInfo.CaseId, caseInfo.StepNum, 6, userId);
                    transaction.Commit();
                    caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(null, reject.CaseId);
                    WriteCaseAuditMessage(caseInfo.CaseId, caseInfo.NodeNum, caseInfo.StepNum, userId, type: 4);

                    result.Id = Guid.NewGuid().ToString();
                    return HandleResult(result);
                }
                catch (Exception ex)
                {
                    if (transaction != null && transaction.Connection != null)
                        transaction.Rollback();
                    if (!string.IsNullOrEmpty(ex.Message))
                        throw new Exception(ex.Message);
                    throw new Exception("跳到驳回人失败");
                }
            }
        }
        #endregion

        public OutputResult<object> WorkFlowWithDraw(WithDrawRequestModel withDraw, int userId)
        {
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                try
                {

                    var caseid = withDraw.CaseId;
                    WorkFlowCaseInfo _theCaseInfo = _workFlowRepository.GetWorkFlowCaseInfo(transaction, caseid);
                    if (_theCaseInfo != null)
                    {
                        //审批发起人，在任何时候都可以撤回审批
                        if (_theCaseInfo.RecCreator == userId)
                        {

                            //撤回发起人的审批
                            _workFlowRepository.WithDrawkWorkFlowByCreator(transaction, caseid, userId);

                            transaction.Commit();
                            return HandleResult(new OperateResult
                            {
                                Flag = 1,
                                Msg = "撤回成功"
                            });
                        }
                    }


                    WorkFlowCaseItemInfo caseitemInfo = null;
                    for (int i = 0; i < 10; i++)
                    {
                        bool _canWithDraw = CanWithDrawWorkFlowCaseItem(transaction, caseid, userId, out caseitemInfo);

                        if (_canWithDraw && caseitemInfo != null)
                        {
                            var caseitemid = caseitemInfo.CaseItemId;
                            var nodenum = caseitemInfo.NodeNum;

                            List<WorkFlowCaseItemTransfer> _transfers = _workFlowRepository.GetWorkFlowCaseItemTransfer(transaction, caseitemid);
                            WorkFlowCaseItemTransfer _theTransfer = _transfers.FirstOrDefault();
                            if (caseitemInfo.ChoiceStatus == ChoiceStatusType.AddNode && _transfers.Count > 0 && _theTransfer.originuserid == userId)
                            {
                                //转发或者加签
                                //delete transfer
                                _workFlowRepository.DeleteWorkFlowCaseItemTransfer(transaction, caseitemid, userId);
                                //update handler
                                _workFlowRepository.UpdateWorkFlowCaseitemHandler(transaction, caseitemid, userId);
                                WorkFlowCaseItemInfo caseitemInfo_sub = null;
                                bool _canWithDraw_sub = CanWithDrawWorkFlowCaseItem(transaction, caseid, userId, out caseitemInfo_sub);
                                if (_canWithDraw_sub && caseitemInfo_sub != null) { }
                                else
                                {
                                    transaction.Commit();
                                    return HandleResult(new OperateResult
                                    {
                                        Flag = 1,
                                        Msg = "撤回成功"
                                    });
                                }
                            }
                            else
                            {
                                List<WorkFlowCaseItemInfo> _caseItems = _workFlowRepository.GetWorkFlowCaseItemOfCase(transaction, caseid);
                                List<WorkFlowCaseItemInfo> _handlerCaseItems = _caseItems.Where(x => x.HandleUser == userId && x.NodeNum != -1 && x.NodeNum <= caseitemInfo.NodeNum).OrderByDescending(x => x.RecCreated).ToList();
                                WorkFlowCaseItemInfo _theCaseItem = _handlerCaseItems.FirstOrDefault();
                                int _theHandlerNodeNum = _theCaseItem.NodeNum;
                                WorkFlowCaseItemInfo _lastCaseItem = _caseItems.FirstOrDefault();

                                if (_lastCaseItem.NodeNum == -1)
                                {
                                    //int _theIndex = _caseItems.IndexOf(_theCaseItem);
                                    //if (_theIndex == 1)
                                    //{
                                    //    //delete 最后一个 case item 
                                    //    //修改倒数第二个caset item choicestatus=6,casestatus=1
                                    //    //修改case nodenum=4,auditstatus=0
                                    //    _workFlowRepository.DeleteWorkFlowCaseItems(transaction, _lastCaseItem.CaseItemId);
                                    //    _workFlowRepository.UpdateWorkFlowCaseItemStatus(transaction, _theCaseItem.CaseItemId, (int)ChoiceStatusType.AddNode,(int)CaseStatusType.Readed);
                                    //    _workFlowRepository.UpdateWorkFlowCaseStatus(transaction, _theCaseItem.CaseId,_theCaseItem.NodeNum,(int)AuditStatusType.Approving);
                                    //}
                                    //transaction.Commit();
                                    return HandleResult(new OperateResult
                                    {
                                        Flag = 0,
                                        Msg = "不能撤回"
                                    });

                                }
                                else
                                {
                                    if (_lastCaseItem.NodeNum > _theHandlerNodeNum)
                                    {
                                        //delete case item
                                        int _dealedCaseitemCount = _workFlowRepository.GetWorkFlowCaseItemCout(transaction, caseid, caseitemid);
                                        if (_dealedCaseitemCount == 0)
                                        {
                                            _workFlowRepository.DeleteWorkFlowCaseItems(transaction, caseid, caseitemid);

                                            //update case nodenum
                                            _workFlowRepository.UpdateWorkFlowCaseNodeNum(transaction, caseid, caseitemid);

                                            //更新处理状态为新加节点  
                                            _workFlowRepository.UpdateWorkFlowCaseitemChoicestatus(transaction, caseitemid, (int)ChoiceStatusType.AddNode, (int)CaseStatusType.WaitApproval);

                                            //发送撤回的消息，需要发给谁呢？
                                            //撤回之后该做些什么呢？ 该如何进行 下一步操作？
                                            //copy caseitem
                                            //_workFlowRepository.CopyWorkFlowCaseitem(transaction, caseid, caseitemid);


                                            //更新caseitem的状态为7,撤回状态
                                            //_workFlowRepository.UpdateWorkFlowCaseItem(transaction, caseitemid, 7);

                                            WorkFlowCaseItemInfo caseitemInfo_sub = null;
                                            bool _canWithDraw_sub = CanWithDrawWorkFlowCaseItem(transaction, caseid, userId, out caseitemInfo_sub);
                                            if (_canWithDraw_sub && caseitemInfo_sub != null) { }
                                            else
                                            {
                                                transaction.Commit();
                                                return HandleResult(new OperateResult
                                                {
                                                    Flag = 1,
                                                    Msg = "撤回成功"
                                                });
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("下一步流程已经处理,不可以撤回");
                                        }

                                    }
                                    else
                                    {
                                        if (_lastCaseItem != null)
                                        {
                                            _workFlowRepository.DeleteWorkFlowCaseItems(transaction, _lastCaseItem.CaseItemId);

                                            if (_theCaseItem != null)
                                            {
                                                _workFlowRepository.UpdateWorkFlowCaseitemChoicestatus(transaction, _theCaseItem.CaseItemId, (int)ChoiceStatusType.AddNode, (int)CaseStatusType.WaitApproval);
                                                _workFlowRepository.UpdateWorkFlowCaseNodeNumNew(transaction, _theCaseItem.CaseId, _theCaseItem.NodeNum);

                                            }
                                        }
                                        WorkFlowCaseItemInfo caseitemInfo_sub = null;
                                        bool _canWithDraw_sub = CanWithDrawWorkFlowCaseItem(transaction, caseid, userId, out caseitemInfo_sub);
                                        if (_canWithDraw_sub && caseitemInfo_sub != null) { }
                                        else
                                        {
                                            transaction.Commit();
                                            return HandleResult(new OperateResult
                                            {
                                                Flag = 1,
                                                Msg = "撤回成功"
                                            });
                                        }
                                    }

                                }
                            }

                        }
                        else
                        {
                            throw new Exception("不可以撤回");
                        }
                    }
                    throw new Exception("撤销过程中存在不可预知的问题，请联系管理员");
                }
                catch (Exception ex)
                {
                    if (transaction != null && transaction.Connection != null)
                        transaction.Rollback();
                    if (!string.IsNullOrEmpty(ex.Message))
                        throw new Exception(ex.Message);
                    throw new Exception("撤回失败");
                }
            }
        }

        public OutputResult<object> CanWorkFlowWithDraw(WithDrawRequestModel withDraw, int userId)
        {
            Guid caseid = withDraw.CaseId;
            WorkFlowCaseItemInfo caseitemInfo = null;
            bool _canWithDraw = CanWithDrawWorkFlowCaseItem(null, caseid, userId, out caseitemInfo);
            if (_canWithDraw)
            {
                var _can = new
                {
                    canwithdraw = true
                };
                return new OutputResult<object>(_can);
            }
            else
            {
                var _cannot = new
                {
                    canwithdraw = false
                };
                return new OutputResult<object>(_cannot);
            }
        }

        private bool CanWithDrawWorkFlowCaseItem(DbTransaction trans, Guid caseid, int userId, out WorkFlowCaseItemInfo caseitemInfo)
        {

            caseitemInfo = null;

            bool _canWithDraw = false;
            try
            {

                WorkFlowCaseInfo _theCaseInfo = _workFlowRepository.GetWorkFlowCaseInfo(trans, caseid);
                if (_theCaseInfo != null)
                {
                    //如果已经发起审批状态，不可以退回，避免重复退回
                    if (_theCaseInfo.NodeNum == 0)
                    {
                        _canWithDraw = false;
                        return _canWithDraw;
                    }
                }


                List<WorkFlowCaseItemInfo> _caseItems = _workFlowRepository.GetWorkFlowCaseItemOfCase(trans, caseid);
                WorkFlowCaseItemInfo _lastCaseItem = _caseItems.FirstOrDefault();
                if (_caseItems != null && _caseItems.Count > 0)
                {

                    //流程审批结束,不可以撤回
                    if (_lastCaseItem.ChoiceStatus == ChoiceStatusType.EndPoint)
                    {
                        _canWithDraw = false;
                        return _canWithDraw;
                    }


                    List<WorkFlowCaseItemInfo> _handlerCaseitems = _caseItems.Where(x => x.HandleUser == userId && x.NodeNum != -1 && x.NodeNum < _theCaseInfo.NodeNum).OrderByDescending(x => x.NodeNum).ToList();
                    if (_handlerCaseitems != null && _handlerCaseitems.Count > 0)
                    {
                        WorkFlowCaseItemInfo _caseItemInfo = _handlerCaseitems.FirstOrDefault();
                        int _theNodeNumber = _caseItemInfo.NodeNum;

                        if (_caseItemInfo.NodeType == (int)NodeType.Normal)
                        {
                            //最后一个为结束节点
                            if (_lastCaseItem.NodeNum == -1)
                            {
                                //int _theIndex = _caseItems.IndexOf(_caseItemInfo);
                                //if (_theIndex == 1)
                                //{
                                //    _canWithDraw = true;
                                //    caseitemInfo = _caseItemInfo;
                                //}

                                _canWithDraw = false;


                            }
                            else
                            {
                                //最后一个非结束节点
                                List<WorkFlowCaseItemInfo> _otherCaseItems = _caseItems.Where(x => x.NodeNum > _theNodeNumber).ToList();
                                int _theCount = _otherCaseItems.Count;
                                if (_theCount == 1)
                                {
                                    WorkFlowCaseItemInfo _theCaseItem = _otherCaseItems.FirstOrDefault();
                                    if (_theCaseItem.CaseStatus == CaseStatusType.WaitApproval)
                                    {
                                        _canWithDraw = true;
                                        caseitemInfo = _caseItemInfo;

                                    }
                                    else if (_theCaseItem.CaseStatus == CaseStatusType.Readed)
                                    {
                                        if (_theCaseItem.ChoiceStatus == ChoiceStatusType.AddNode)
                                        {
                                            _canWithDraw = true;
                                            caseitemInfo = _caseItemInfo;
                                        }
                                    }
                                }
                                else
                                {

                                    //撤回节点
                                    if (_lastCaseItem != null && _lastCaseItem.NodeNum < _theNodeNumber)
                                    {
                                        _canWithDraw = true;
                                        caseitemInfo = _caseItemInfo;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {

                        List<WorkFlowCaseItemInfo> _theCaseitems = _caseItems.OrderByDescending(x => x.NodeNum).ToList();
                        if (_theCaseitems != null && _theCaseitems.Count > 0)
                        {
                            WorkFlowCaseItemInfo _theCaseItem = _theCaseitems.FirstOrDefault();
                            if (_theCaseItem.ChoiceStatus == ChoiceStatusType.AddNode)
                            {
                                List<WorkFlowCaseItemTransfer> _transfers = _workFlowRepository.GetWorkFlowCaseItemTransfer(trans, _theCaseItem.CaseItemId);
                                WorkFlowCaseItemTransfer _theTransfer = _transfers.FirstOrDefault();
                                if (_theTransfer != null && _theTransfer.originuserid == userId)
                                {
                                    _canWithDraw = true;
                                    caseitemInfo = _theCaseItem;
                                }
                            }
                        }
                    }

                    //审批发起人，在任何时候都可以撤回审批
                    if (_theCaseInfo.RecCreator == userId)
                    {
                        _canWithDraw = true;
                        return _canWithDraw;
                    }

                }
            }
            catch (Exception)
            {
                _canWithDraw = false;
            }

            return _canWithDraw;
        }
    }
}
