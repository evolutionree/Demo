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




        public WorkFlowServices(IMapper mapper, IWorkFlowRepository workFlowRepository, IRuleRepository ruleRepository, IEntityProRepository entityProRepository, IDynamicEntityRepository dynamicEntityRepository, IDynamicRepository dynamicRepository, DynamicEntityServices dynamicEntityServices)
        {
            _workFlowRepository = workFlowRepository;
            _entityProRepository = entityProRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _dynamicRepository = dynamicRepository;
            _mapper = mapper;
            _ruleRepository = ruleRepository;
            _dynamicEntityServices = dynamicEntityServices;
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
                        CopyUser = copyusersInfo
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
                    var lastCaseItem = _workFlowRepository.CaseItemList(caseInfo.CaseId, userNumber).LastOrDefault();
                    if (lastCaseItem != null)
                    {
                        var username = lastCaseItem.ContainsKey("username") ? lastCaseItem["username"] : "";
                        var casestatus = lastCaseItem.ContainsKey("casestatus") ? lastCaseItem["casestatus"] : "";
                        result.CaseItem.AuditStatus = string.Format("{0}{1}", username, casestatus);
                    }
                    var notfinishitems = caseitems.Where(m => m.CaseStatus == CaseStatusType.Readed || m.CaseStatus == CaseStatusType.WaitApproval);
                    if (notfinishitems.Count() > 0)
                    {
                        string temptext = notfinishitems.Count() > 2 ? string.Join(",", notfinishitems.Select(m => m.HandleUserName).ToArray(), 0, 2) + "等" : string.Join(",", notfinishitems.Select(m => m.HandleUserName).ToArray());
                        result.CaseItem.AuditStep = string.Format("等待{0}处理审批", temptext);
                    }

                    var nowcaseitem = caseitems.Find(m => m.HandleUser == userNumber);
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
                            }
                        }

                    }

                    #endregion



                    var dynamicEntityServices = dynamicCreateService("UBeat.Crm.CoreApi.Services.Services.DynamicEntityServices", false) as DynamicEntityServices;

                    #region --获取 entitydetail--
                    //获取 entitydetail
                    var detailMapper = new DynamicEntityDetailtMapper()
                    {
                        EntityId = caseInfo.EntityId,
                        RecId = caseInfo.RecId,
                        NeedPower = 0
                    };
                    var detail = _dynamicEntityRepository.Detail(detailMapper, userNumber, tran);
                    result.EntityDetail = dynamicEntityServices.DealLinkTableFields(new List<IDictionary<string, object>>() { detail }, detailMapper.EntityId, userNumber).FirstOrDefault();
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
                        result.RelateDetail = dynamicEntityServices.DealLinkTableFields(new List<IDictionary<string, object>>() { detailtemp }, reldetailMapper.EntityId, userNumber).FirstOrDefault();
                    }
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

        #region --AddCase--（new）

        #region --流程预提交--
        public OutputResult<object> PreAddWorkflowCase(WorkFlowCaseAddModel caseModel, UserInfo userinfo)
        {
            if (caseModel == null)
            {
                throw new Exception("参数不可为空");
            }
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
                    if (workflowInfo == null)
                        throw new Exception("流程配置不存在,请先配置审批节点");
                    WorkFlowNodeInfo firstNodeInfo = null;
                    var caseid = AddWorkFlowCase(true, tran, caseModel, workflowInfo, userinfo, out firstNodeInfo);
                    //走完审批所有操作，获取下一步数据
                    var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseid);

                    var result = GetNextNodeData(tran, caseInfo, workflowInfo, firstNodeInfo, userinfo);

                    return new OutputResult<object>(result);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    //这是预处理操作，获取到结果后不需要提交事务，直接全部回滚
                    tran.Rollback();
                    conn.Close();
                    conn.Dispose();
                }
            }
        }


        #endregion

        #region --流程发起提交--
        public OutputResult<object> AddWorkflowCase(WorkFlowCaseAddModel caseModel, UserInfo userinfo)
        {
            if (caseModel == null)
            {
                throw new Exception("参数不可为空");
            }

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
                    if (workflowInfo == null)
                        throw new Exception("流程配置不存在");
                    WorkFlowNodeInfo firstNodeInfo = null;
                    var caseid = AddWorkFlowCase(false, tran, caseModel, workflowInfo, userinfo, out firstNodeInfo);
                    tran.Commit();
                    return new OutputResult<object>(caseid);
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
        }

        #endregion

        #region --新增流程case数据--

        private Guid AddWorkFlowCase(bool ispresubmit, DbTransaction tran, WorkFlowCaseAddModel caseModel, WorkFlowInfo workflowInfo, UserInfo userinfo, out WorkFlowNodeInfo firstNodeInfo)
        {
            var caseEntity = _mapper.Map<WorkFlowAddCaseModel, WorkFlowAddCaseMapper>(caseModel.CaseModel);
            if (caseEntity == null || !caseEntity.IsValid())
            {
                throw new Exception(caseEntity.ValidationState.Errors.First());
            }

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
                    SubmitWorkFlowAudit(caseItemModel, userinfo, tran);
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
                    });
                }
            }

            return newcaseid;
        }

        #endregion

       

        #endregion

        public OutputResult<object> AddCase(WorkFlowAddCaseModel caseModel, int userNumber)
        {
            //获取该实体分类的字段
            var caseEntity = _mapper.Map<WorkFlowAddCaseModel, WorkFlowAddCaseMapper>(caseModel);
            if (caseEntity == null || !caseEntity.IsValid())
            {
                return HandleValid(caseEntity);
            }

            var result = AddCase(null, caseEntity, userNumber);

            return HandleResult(result);
        }

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

        public OperateResult AddCase(DbTransaction tran, WorkFlowAddCaseMapper caseEntity, int userNumber)
        {
            var entityInfo = _entityProRepository.GetEntityInfo(caseEntity.EntityId);
            var detailMapper = new DynamicEntityDetailtMapper()
            {
                EntityId = entityInfo.EntityId,
                RecId = caseEntity.RecId,
                NeedPower = 0
            };

            if (entityInfo.ModelType == EntityModelType.Dynamic)
            {
                detailMapper.EntityId = entityInfo.RelEntityId.GetValueOrDefault();
                detailMapper.RecId = caseEntity.RelRecId.GetValueOrDefault();
            }
            var olddetail = _dynamicEntityRepository.Detail(detailMapper, userNumber);
            var result = _workFlowRepository.AddCase(tran, caseEntity, userNumber);
            if (result.Flag == 1)
            {
                Task.Run(() =>
                {
                    var caseId = Guid.Parse(result.Id);
                    WriteAddCaseMessage(entityInfo, caseEntity.RecId, caseEntity.RelRecId.GetValueOrDefault(), caseEntity.FlowId, caseId, userNumber, olddetail);
                });
            }

            return result;
        }
        /// <summary>
        /// 写入添加流程的消息
        /// </summary>
        private void WriteAddCaseMessage(SimpleEntityInfo entityInfo, Guid bussinessId, Guid relbussinessId, Guid flowId, Guid caseId, int userNumber, IDictionary<string, object> olddetail)
        {

            //获取casedetail
            var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(null, caseId);
            if (caseInfo == null)
                return;
            var workflowInfo = _workFlowRepository.GetWorkFlowInfo(null, caseInfo.FlowId);
            if (workflowInfo == null)
                return;
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


        #region 旧代码
        public OutputResult<object> NextNodeData(WorkFlowNextNodeModel caseModel, int userNumber)
        {
            //获取该实体分类的字段
            if (caseModel?.CaseId == null)
            {
                return ShowError<object>("流程明细ID不能为空");
            }

            var result = _workFlowRepository.NextNodeData(caseModel.CaseId, userNumber);

            return new OutputResult<object>(result);

        }
        #endregion

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

        #region 旧代码
        public OutputResult<object> AddCaseItem(WorkFlowAddCaseItemModel caseItemModel, int userNumber)
        {
            //获取该实体分类的字段
            var caseItemEntity = _mapper.Map<WorkFlowAddCaseItemModel, WorkFlowAddCaseItemMapper>(caseItemModel);
            if (caseItemEntity == null || !caseItemEntity.IsValid())
            {
                return HandleValid(caseItemEntity);
            }

            var result = _workFlowRepository.AddCaseItem(caseItemEntity, userNumber);
            if (result.Flag == 1)
            {
                WriteCaseItemMessage(0, caseItemModel.CaseId, caseItemModel.NodeNum, userNumber);
            }

            return HandleResult(result);
        }

        /// <summary>
        /// 写入添加流程的消息
        /// </summary>
        /// <param name="operateType">操作类型：0=选择下一审批人，1=审批当前节点</param>
        /// <param name="caseId"></param>
        /// <param name="nodeNum"></param>
        /// <param name="userNumber"></param>
        public void WriteCaseItemMessage(int operateType, Guid caseId, int nodeNum, int userNumber)
        {
            Task.Run(() =>
            {
                List<int> approvers = new List<int>();
                List<int> copyusers = new List<int>();
                List<int> completedApprovers = new List<int>(); //暂时未空，预留字段
                string allApprovalSuggest = null;

                //获取casedetail
                var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(null, caseId);
                if (caseInfo == null)
                    return;
                var workflowInfo = _workFlowRepository.GetWorkFlowInfo(null, caseInfo.FlowId);
                if (workflowInfo == null)
                    return;


                var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(null, caseId, nodeNum);

                if (caseitems == null || caseitems.Count == 0)
                    return;
                caseitems = caseitems.OrderByDescending(m => m.StepNum).ToList();

                var caseItem = operateType == 0 ? caseitems.FirstOrDefault() : caseitems.FirstOrDefault(m => m.HandleUser == userNumber);
                if (caseItem == null)
                    return;

                approvers = caseitems.Select(m => m.HandleUser).Distinct().ToList();
                foreach (var item in caseitems)
                {
                    if (!string.IsNullOrEmpty(item.CopyUser))
                    {
                        var copyUserArray = item.CopyUser.Split(',');
                        foreach (var u in copyUserArray)
                        {
                            copyusers.Add(int.Parse(u));
                        }

                    }
                }
                copyusers = copyusers.Distinct().ToList();

                var entityInfotemp = _entityProRepository.GetEntityInfo(caseInfo.EntityId);
                if (entityInfotemp == null)
                    return;
                var msg = new MessageParameter();
                var nodeid = caseitems.FirstOrDefault().NodeId;
                var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(null, nodeid);
                WorkFlowNodeInfo previousFlowNodeInfo = null;//上一审批节点
                if (nodeNum > 1 && flowNodeInfo != null)
                    previousFlowNodeInfo = _workFlowRepository.GetPreviousWorkFlowNodeInfo(null, caseInfo.FlowId, caseInfo.VerNum, nodeid);

                bool isMultipleApproval = flowNodeInfo != null && flowNodeInfo.NodeType == NodeType.Joint;//是否会审（多人审批同一个节点）
                if (!isMultipleApproval) //普通审批和自由流程
                {

                    allApprovalSuggest = caseItem.Suggest;
                    switch (caseItem.ChoiceStatus)
                    {
                        case ChoiceStatusType.Edit:
                            if (caseInfo.AuditStatus == AuditStatusType.Begin || caseInfo.AuditStatus == AuditStatusType.Approving)//编辑操作
                            {
                                msg.FuncCode = "WorkFlowLaunch";

                            }
                            break;
                        case ChoiceStatusType.Approval://普通审批通过
                            if (caseInfo.AuditStatus == AuditStatusType.Finished)//审批完成
                            {
                                msg.FuncCode = "WorkFlowNodeFinish";
                            }
                            else
                            {
                                //由于先阶段，审批和选人是分开步骤，因此该情况不处理消息
                                return;
                            }
                            break;
                        case ChoiceStatusType.Reback: //普通审批退回
                            msg.FuncCode = "WorkFlowNodeFallback";
                            break;
                        case ChoiceStatusType.Refused://普通审批拒绝
                            msg.FuncCode = "WorkFlowNodeReject";
                            break;
                        case ChoiceStatusType.Stop:
                            msg.FuncCode = "WorkFlowNodeStop";
                            break;
                        case ChoiceStatusType.AddNode://选择下一步审批人时的消息
                            if (workflowInfo.FlowType == WorkFlowType.FreeFlow)//自由流程
                            {
                                if (caseItem.StepNum == 1)
                                {
                                    msg.FuncCode = "WorkFlowLaunch";
                                }
                                else msg.FuncCode = "WorkFlowNodeApproval";
                            }
                            else
                            {
                                if (caseItem.NodeNum == 1)
                                {
                                    msg.FuncCode = "WorkFlowLaunch";
                                }
                                else if (previousFlowNodeInfo.NodeType == NodeType.Joint)
                                    msg.FuncCode = "NextWorkFlowNodeJointApproval";
                                else msg.FuncCode = "WorkFlowNodeApproval";
                            }

                            break;
                        case ChoiceStatusType.EndPoint:
                            msg.FuncCode = "WorkFlowNodeFinish";
                            break;
                    }
                }
                else //会审审批
                {

                    if (caseInfo.AuditStatus == AuditStatusType.Finished)//完成审批
                    {
                        msg.FuncCode = "WorkFlowNodeFinish";
                    }

                    allApprovalSuggest = string.Join(";", caseitems.Select(m => m.Suggest));
                    //判断是否还有人未完成审批
                    bool hasNotApprovalFinish = caseitems.Exists(m => m.CaseStatus == CaseStatusType.WaitApproval || m.CaseStatus == CaseStatusType.Readed);
                    //会审审批通过的节点数
                    var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();

                    switch (caseItem.ChoiceStatus)
                    {
                        case ChoiceStatusType.Edit:
                            if (caseInfo.AuditStatus == AuditStatusType.Begin || caseInfo.AuditStatus == AuditStatusType.Approving)//编辑操作
                            {
                                msg.FuncCode = "WorkFlowLaunch";
                            }
                            break;
                        case ChoiceStatusType.Approval://某个会审人审批通过
                            if (aproval_success_count >= flowNodeInfo.AuditSucc)
                            {

                            }
                            else
                            {
                                msg.FuncCode = "WorkFlowNodeJointApproval";
                            }

                            break;
                        case ChoiceStatusType.Reback: //
                            if (caseInfo.NodeNum == 0)
                            {
                                msg.FuncCode = "FinishWorkFlowNodeJointFallback";//退回发起人
                            }
                            else msg.FuncCode = "WorkFlowNodeJointFallback";//某个会审人审批退回
                            break;
                        case ChoiceStatusType.Refused://审批拒绝
                            if (caseInfo.NodeNum == -1)
                            {
                                msg.FuncCode = "FinishWorkFlowNodeJointRejectk";//全部完成了，流程拒绝
                            }
                            else msg.FuncCode = "WorkFlowNodeJointReject";//其中有个会审人拒绝
                            break;
                        case ChoiceStatusType.Stop:
                            msg.FuncCode = "WorkFlowNodeStop";
                            break;
                        case ChoiceStatusType.AddNode://选择下一步审批人时的消息
                            if (caseItem.NodeNum == 1)
                            {
                                msg.FuncCode = "WorkFlowLaunch";
                            }
                            else if (previousFlowNodeInfo.NodeType == NodeType.Joint)
                                msg.FuncCode = "NextWorkFlowNodeJointApproval";
                            else msg.FuncCode = "WorkFlowNodeApproval";
                            break;
                        case ChoiceStatusType.EndPoint:
                            msg.FuncCode = "WorkFlowNodeFinish";
                            break;
                    }
                }

                msg.EntityId = entityInfotemp.EntityId;
                msg.EntityName = entityInfotemp.EntityName;
                msg.TypeId = entityInfotemp.CategoryId;
                msg.BusinessId = caseInfo.RecId;
                msg.RelEntityId = entityInfotemp.RelEntityId;
                msg.RelEntityName = entityInfotemp.RelEntityName;
                msg.RelBusinessId = caseInfo.RelRecId;
                msg.Receivers = MessageService.GetWorkFlowMessageReceivers(caseInfo.RecCreator, approvers, copyusers, completedApprovers);
                var msgParamData = new Dictionary<string, object>();
                msgParamData.Add("caseid", caseInfo.CaseId.ToString());
                msg.ParamData = JsonConvert.SerializeObject(msgParamData);

                var users = new List<int>();
                users.Add(userNumber);
                users.Add(caseInfo.RecCreator);
                users.AddRange(approvers);
                var userInfos = MessageService.GetUserInfoList(users.Distinct().ToList());

                var paramData = new Dictionary<string, string>();
                paramData.Add("operator", userInfos.FirstOrDefault(m => m.UserId == userNumber).UserName);
                paramData.Add("launchUser", userInfos.FirstOrDefault(m => m.UserId == caseInfo.RecCreator).UserName);
                paramData.Add("approvalUserNames", string.Join("、", userInfos.Where(m => approvers.Contains(m.UserId)).Select(m => m.UserName)));
                paramData.Add("workflowName", workflowInfo.FlowName);
                paramData.Add("reccode", caseInfo.RecCode);
                paramData.Add("approvalSuggest", caseItem.Suggest);
                paramData.Add("allApprovalSuggest", allApprovalSuggest);


                msg.TemplateKeyValue = paramData;
                msg.CopyUsers = copyusers;
                msg.ApprovalUsers = approvers;
                msg.FlowId = caseInfo.FlowId;
                //如果是动态实体，则需要发动态，
                //流程新增和结束时候需要发送动态

                if ((entityInfotemp.ModelType == EntityModelType.Dynamic || entityInfotemp.ModelType == EntityModelType.Independent)
                         && (msg.FuncCode == "WorkFlowLaunch" || caseInfo.AuditStatus != AuditStatusType.Approving))
                {
                    //先发流程的审批消息，再发关联动态的消息
                    MessageService.WriteMessage(null, msg, userNumber);

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

                    if (entityInfotemp.ModelType == EntityModelType.Dynamic && msg.FuncCode == "WorkFlowLaunch")
                    {
                        // 发布关联动态实体的动态消息
                        var dynamicMsgtemp = MessageService.GetEntityMsgParameter(entityInfotemp, msg.BusinessId, msg.RelBusinessId, "EntityDynamicAdd", userNumber, newMembers, null, msgpParam);

                        MessageService.WriteMessage(null, dynamicMsgtemp, userNumber, null);
                    }

                    string dynamicFuncode = msg.FuncCode + "Dynamic";
                    var dynamicMsg = MessageService.GetEntityMsgParameter(entityInfotemp, msg.BusinessId, msg.RelBusinessId, dynamicFuncode, userNumber, newMembers, null, msgpParam);

                    dynamicMsg.TemplateKeyValue = dynamicMsg.TemplateKeyValue.Union(paramData).ToLookup(t => t.Key, t => t.Value).ToDictionary(m => m.Key, m => m.First());
                    //发布审批消息到实体动态列表
                    MessageService.WriteMessage(null, dynamicMsg, userNumber, null, 2);


                }

                else MessageService.WriteMessage(null, msg, userNumber);
            });
        }




        public OutputResult<object> AuditCaseItem(WorkFlowAuditCaseItemModel caseItemModel, int userNumber)
        {
            //获取该实体分类的字段
            var caseItemEntity = _mapper.Map<WorkFlowAuditCaseItemModel, WorkFlowAuditCaseItemMapper>(caseItemModel);
            if (caseItemEntity == null || !caseItemEntity.IsValid())
            {
                return HandleValid(caseItemEntity);
            }

            var result = _workFlowRepository.AuditCaseItem(caseItemEntity, userNumber);
            if (result.Flag == 1)
            {
                WriteCaseItemMessage(1, caseItemModel.CaseId, caseItemModel.NodeNum, userNumber);
            }
            return HandleResult(result);
        }
        #endregion


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
        public OutputResult<object> SubmitPretreatAudit(WorkFlowAuditCaseItemModel caseItemModel, UserInfo userinfo)
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
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
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
                        else  //会审
                        {
                            var nowcaseitem = caseitems.Find(m => m.HandleUser == userinfo.UserId);
                            if (nowcaseitem == null)
                                throw new Exception("您没有审批当前节点的权限");
                            AuditJoinFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, flowNodeInfo, nowcaseitem, hasNextNode);
                        }
                    }
                    //判断是否有附加函数_event_func
                    var eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, nodeid, tran);
                    _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, caseInfo.NodeNum, caseItemEntity.ChoiceStatus, userinfo.UserId, tran);

                    //流程审批过程修改实体字段时，更新关联实体的字段数据
                    _workFlowRepository.ExecuteUpdateWorkFlowEntity(caseInfo.CaseId, caseInfo.NodeNum, userinfo.UserId, tran);

                    //走完审批所有操作，获取下一步数据
                    result = GetNextNodeData(tran, caseInfo, workflowInfo, flowNodeInfo, userinfo);
                    //这是预处理操作，获取到结果后不需要提交事务，直接全部回滚
                    tran.Rollback();
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
                if (newcaseInfo.NodeNum == -1)//预审批审批结束，表明到达最后节点
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
                }
                //检查下一点，获取下一节点信息
                if (nodetemp.NodeState == 0)
                {
                    //获取下一节点
                    var nextnodes = _workFlowRepository.GetNextNodeDataInfoList(caseInfo.FlowId, nodeid, caseInfo.VerNum, tran);
                    if (nextnodes == null || nextnodes.Count == 0)
                        throw new Exception("获取不到节点配置");

                    if (nextnodes.Count == 1)
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
                        users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, Guid.Empty, userinfo.UserId, WorkFlowType.FreeFlow, tran);
                    }
                    result = new NextNodeDataModel()
                    {
                        NodeInfo = nodetemp,
                        Approvers = users
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
        #endregion


        #region --提交审批--


        public OutputResult<object> SubmitWorkFlowAudit(WorkFlowAuditCaseItemModel caseItemModel, UserInfo userinfo, DbTransaction tran = null)
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
            Guid lastNodeId = Guid.Empty;
            bool canAddNextNode = true;
            bool isbranchFlow = false;
            WorkFlowNodeInfo nextnode = null;
            DbConnection conn = null;
            if (tran == null)
            {
                conn = GetDbConnect();
                conn.Open();
                tran = conn.BeginTransaction();
            }

            try
            {
                //判断流程类型，如果是分支流程，则检查所选分支是否正确
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

                stepnum = caseitems.FirstOrDefault().StepNum;


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
                        nodeid = freeFlowNodeId;
                    }
                }
                else //固定流程
                {
                    nodeid = caseitems.FirstOrDefault().NodeId;

                    isbranchFlow = AuditFixedFlow(nodeid, userinfo, caseItemEntity, ref casefinish, tran, caseInfo, caseitems, out nextnode, out canAddNextNode);
                    if (casefinish)
                        lastNodeId = nextnode.NodeId;
                }
                //判断是否有附加函数_event_func
                var eventInfo = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, nodeid, tran);
                _workFlowRepository.ExecuteWorkFlowEvent(eventInfo, caseInfo.CaseId, caseInfo.NodeNum, caseItemEntity.ChoiceStatus, userinfo.UserId, tran);

                //流程审批过程修改实体字段时，更新关联实体的字段数据
                _workFlowRepository.ExecuteUpdateWorkFlowEntity(caseInfo.CaseId, caseInfo.NodeNum, userinfo.UserId, tran);

                if (casefinish)//审批已经到达了最后一步
                {
                    _workFlowRepository.EndWorkFlowCaseItem(caseInfo.CaseId, lastNodeId, stepnum + 1, userinfo.UserId, tran);

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
                    AddCaseItem(nodeid, caseItemModel, workflowInfo, caseInfo, stepnum + 1, userinfo, tran);
                }
                if (conn != null)
                {
                    tran.Commit();
                }
                //写审批消息
                WriteCaseAuditMessage(caseInfo.CaseId, caseInfo.NodeNum, stepnum, userinfo.UserId);


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
        private bool AuditFixedFlow(Guid nodeid, UserInfo userinfo, WorkFlowAuditCaseItemMapper caseItemEntity, ref bool casefinish, DbTransaction tran, WorkFlowCaseInfo caseInfo, List<WorkFlowCaseItemInfo> caseitems, out WorkFlowNodeInfo nextnode, out bool canAddNextNode)
        {
            bool isbranchflow = false;
            bool hasNextNode = true;

            var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);
            if (flowNodeInfo == null)
                throw new Exception("流程配置不存在");
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
            if (nextnode != null && nextnode.StepTypeId == NodeStepType.End)
            {
                canAddNextNode = false;
                hasNextNode = false;
            }

            //执行审批逻辑
            if (flowNodeInfo.NodeType == NodeType.Normal)//普通审批
            {
                var nowcaseitem = caseitems.FirstOrDefault();

                canAddNextNode = AuditNormalFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, nowcaseitem, hasNextNode);
            }
            else  //会审
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
                    auditstatus = hasNextNode? AuditStatusType.Begin: AuditStatusType.Finished;
                    canAddNextNodeItem = hasNextNode;
                    casefinish = !hasNextNode;
                    _workFlowRepository.ReOpenWorkFlowCase(caseInfo.CaseId, nowcaseitem.CaseItemId, userinfo.UserId, tran);
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
            Guid nodeid = caseItemModel.NodeId;

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
                if (nodeid != Guid.Empty)
                    flowNodeInfo = flowNextNodeInfos.Find(m => m.NodeId == nodeid);
                else flowNodeInfo = flowNextNodeInfos.FirstOrDefault();
                if (flowNodeInfo == null)
                    throw new Exception("下一步节点不可为空");
                nodeid = flowNodeInfo.NodeId;
                if (flowNodeInfo.NodeType == NodeType.Joint)//会审
                {
                    //获取当前审批结果
                    var caseitemlist = _workFlowRepository.GetWorkFlowCaseItemInfo(trans, caseInfo.CaseId, caseInfo.NodeNum);
                    if (caseitemlist == null || caseitemlist.Count == 0)
                        throw new Exception("流程节点异常");
                    //caseitemlist = caseitemlist.OrderByDescending(m => m.StepNum).ToList();

                    //获取还有人未处理审批
                    //var aproval_notdeal_count = caseitemlist.Where(m => m.CaseStatus == CaseStatusType.WaitApproval || m.CaseStatus == CaseStatusType.Readed).Count();
                    //if (aproval_notdeal_count > 0)
                    //    throw new Exception("该节点其他人还在审批，不能进入下一步");
                }
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
                    CopyUser = caseItemModel.CopyUser
                };
                caseitems.Add(item);
            }
            var result = _workFlowRepository.AddCaseItem(caseitems, userinfo.UserId, AuditStatusType.Approving, trans);

        }
        #endregion

        #region --写入添加流程的消息--
        /// <summary>
        /// 写入添加流程的消息
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="nodeNum"></param>
        /// <param name="userNumber"></param>
        public void WriteCaseAuditMessage(Guid caseId, int nodeNum, int stepNum, int userNumber)
        {
            Task.Run(() =>
            {
                using (var conn = GetDbConnect())
                {
                    conn.Open();
                    var tran = conn.BeginTransaction();

                    try
                    {
                        List<int> completedApprovers = new List<int>(); //暂时为空，预留字段
                        string allApprovalSuggest = null;
                        bool isAddNextStep = false;//审批是否通过，并进入下一步审批人

                        //获取casedetail
                        var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(tran, caseId);
                        if (caseInfo == null)
                            return;
                        var workflowInfo = _workFlowRepository.GetWorkFlowInfo(tran, caseInfo.FlowId);
                        if (workflowInfo == null)
                            return;

                        var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseId, nodeNum, stepNum);
                        if (caseitems == null || caseitems.Count == 0)
                            return;
                        caseitems = caseitems.OrderByDescending(m => m.StepNum).ToList();

                        var myAuditCaseItem = caseitems.FirstOrDefault(m => m.HandleUser == userNumber);
                        if (myAuditCaseItem == null)
                            return;

                        var entityInfotemp = _entityProRepository.GetEntityInfo(caseInfo.EntityId);
                        if (entityInfotemp == null)
                            return;
                        var msg = new MessageParameter();
                        allApprovalSuggest = myAuditCaseItem.Suggest;
                        string funcode = null;

                        if (workflowInfo.FlowType == WorkFlowType.FreeFlow)//自由流程
                        {
                            funcode = GetFreeFlowMessageFuncode(myAuditCaseItem, caseInfo, tran, out isAddNextStep);

                        }
                        else   //固定流程
                        {
                            var nodeid = caseitems.FirstOrDefault().NodeId;
                            var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);
                            if (flowNodeInfo == null)
                            {
                                Logger.Error("流程节点不存在");
                                return;
                            }
                            if (flowNodeInfo.NodeType != NodeType.Joint)//普通审批
                            {
                                funcode = GetNormalFlowMessageFuncode(myAuditCaseItem, caseInfo, tran, out isAddNextStep);


                            }
                            else //会审
                            {
                                allApprovalSuggest = string.Join(";", caseitems.Select(m => m.Suggest));
                                funcode = GetJointFlowMessageFuncode(myAuditCaseItem, caseInfo, caseitems, flowNodeInfo, tran, out isAddNextStep);
                            }

                        }
                        if (string.IsNullOrEmpty(funcode))//没有有效的消息模板funcode
                        {
                            Logger.Error("没有有效的消息模板funcode");
                            return;
                        }
                        msg.FuncCode = funcode;

                        #region --获取审批人和抄送人--
                        List<int> approvers = new List<int>();//审批人
                        List<int> copyusers = new List<int>();//抄送人
                        List<WorkFlowCaseItemInfo> tempcaseitems = caseitems;
                        if (isAddNextStep)//如果审批通过，进入下一步审批，则获取下一步的审批人和抄送人
                        {
                            tempcaseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseId, caseInfo.NodeNum, stepNum + 1);

                            if (tempcaseitems == null || tempcaseitems.Count == 0)
                                tempcaseitems = caseitems;
                        }
                        approvers = tempcaseitems.Select(m => m.HandleUser).Distinct().ToList();

                        copyusers = _workFlowRepository.GetWorkFlowCopyUser(caseInfo.CaseId).Select(m => m.UserId).ToList();
                        #endregion


                        #region --封装MessageParameter--
                        msg.EntityId = entityInfotemp.EntityId;
                        msg.EntityName = entityInfotemp.EntityName;
                        msg.TypeId = entityInfotemp.CategoryId;
                        msg.BusinessId = caseInfo.RecId;
                        msg.RelEntityId = entityInfotemp.RelEntityId;
                        msg.RelEntityName = entityInfotemp.RelEntityName;
                        msg.RelBusinessId = caseInfo.RelRecId;
                        msg.Receivers = MessageService.GetWorkFlowMessageReceivers(caseInfo.RecCreator, approvers, copyusers, completedApprovers);
                        var msgParamData = new Dictionary<string, object>();
                        msgParamData.Add("caseid", caseInfo.CaseId.ToString());
                        msg.ParamData = JsonConvert.SerializeObject(msgParamData);

                        var users = new List<int>();
                        users.Add(userNumber);
                        users.Add(caseInfo.RecCreator);
                        users.AddRange(approvers);
                        var userInfos = MessageService.GetUserInfoList(users.Distinct().ToList());

                        var paramData = new Dictionary<string, string>();
                        paramData.Add("operator", userInfos.FirstOrDefault(m => m.UserId == userNumber).UserName);
                        paramData.Add("launchUser", userInfos.FirstOrDefault(m => m.UserId == caseInfo.RecCreator).UserName);
                        paramData.Add("approvalUserNames", string.Join("、", userInfos.Where(m => approvers.Contains(m.UserId)).Select(m => m.UserName)));
                        paramData.Add("workflowName", workflowInfo.FlowName);
                        paramData.Add("reccode", caseInfo.RecCode);
                        paramData.Add("approvalSuggest", myAuditCaseItem.Suggest);
                        paramData.Add("allApprovalSuggest", allApprovalSuggest);


                        msg.TemplateKeyValue = paramData;
                        msg.CopyUsers = copyusers;
                        msg.ApprovalUsers = approvers;
                        msg.FlowId = caseInfo.FlowId;
                        #endregion
                        //如果是动态实体，则需要发动态，
                        //流程新增和结束时候需要发送动态
                        #region --发送消息--
                        if ((entityInfotemp.ModelType == EntityModelType.Dynamic || entityInfotemp.ModelType == EntityModelType.Independent)
                                                 && (msg.FuncCode == "WorkFlowLaunch" || caseInfo.AuditStatus != AuditStatusType.Approving))
                        {
                            //先发流程的审批消息，再发关联动态的消息
                            MessageService.WriteMessage(tran, msg, userNumber);

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

                            if (entityInfotemp.ModelType == EntityModelType.Dynamic && msg.FuncCode == "WorkFlowLaunch")
                            {
                                // 发布关联动态实体的动态消息
                                var dynamicMsgtemp = MessageService.GetEntityMsgParameter(entityInfotemp, msg.BusinessId, msg.RelBusinessId, "EntityDynamicAdd", userNumber, newMembers, null, msgpParam);

                                MessageService.WriteMessage(tran, dynamicMsgtemp, userNumber, null);
                            }

                            string dynamicFuncode = msg.FuncCode + "Dynamic";
                            var dynamicMsg = MessageService.GetEntityMsgParameter(entityInfotemp, msg.BusinessId, msg.RelBusinessId, dynamicFuncode, userNumber, newMembers, null, msgpParam);

                            dynamicMsg.TemplateKeyValue = dynamicMsg.TemplateKeyValue.Union(paramData).ToLookup(t => t.Key, t => t.Value).ToDictionary(m => m.Key, m => m.First());
                            //发布审批消息到实体动态列表
                            MessageService.WriteMessage(tran, dynamicMsg, userNumber, null, 2);


                        }

                        else MessageService.WriteMessage(tran, msg, userNumber);
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

            var result = _workFlowRepository.CaseItemList(listModel.CaseId, userNumber);
            return new OutputResult<object>(result);
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

            var result = _workFlowRepository.GetNodeLinesInfo(nodeLineModel.FlowId, userNumber);
            return new OutputResult<object>(result);
        }

        #region --旧接口--
        public OutputResult<object> NodeLinesConfig(WorkFlowNodeLinesConfigModel configModel, int userNumber)
        {
            //获取该实体分类的字段
            var configEntity = _mapper.Map<WorkFlowNodeLinesConfigModel, WorkFlowNodeLinesConfigMapper>(configModel);
            if (configEntity == null || !configEntity.IsValid())
            {
                return HandleValid(configEntity);
            }

            var result = _workFlowRepository.NodeLinesConfig(configEntity, userNumber);
            IncreaseDataVersion(DataVersionType.FlowData, null);
            return new OutputResult<object>("");
        }
        #endregion

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

            var result= _workFlowRepository.GetFreeFlowNodeEvents(configModel.FlowId, null);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> SaveFreeFlowNodeEvents(FreeFlowEventModel configModel, int userNumber)
        {
            if (configModel == null)
            {
                return ShowError<object>("参数不可为空");
            }
            WorkFlowInfo workFlowInfo = this._workFlowRepository.GetWorkFlowInfo(null, configModel.FlowId);
            if(workFlowInfo==null)
                return ShowError<object>("流程不存在");
            else if(workFlowInfo.FlowType== WorkFlowType.FixedFlow)
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
            _workFlowRepository.SaveNodeEvents(configModel.FlowId,nodes);
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

        public OutputResult<object> AddMultipleCase(WorkFlowAddMultipleCaseModel caseModel, int userNumber)
        {
            var caseIds = new List<string>();
            WorkFlowAddMultipleCaseMapper caseEntity = new WorkFlowAddMultipleCaseMapper()
            {
                FlowId = caseModel.FlowId,
                EntityId = caseModel.EntityId,
                RecId = caseModel.RecId,
                RelEntityId = caseModel.RelEntityId,
                RelRecId = caseModel.RelRecId,
                CaseData = caseModel.CaseData
            };

            if (caseEntity == null || !caseEntity.IsValid())
            {
                return HandleValid(caseEntity);
            }

            //添加多个case,返回第一个case的user数据，作为所有case的user数据
            foreach (var id in caseEntity.RecId)
            {
                WorkFlowAddCaseModel caseMapper = new WorkFlowAddCaseModel()
                {
                    FlowId = caseEntity.FlowId,
                    EntityId = caseEntity.EntityId,
                    RecId = id,
                    RelEntityId = caseEntity.RelEntityId,
                    RelRecId = caseEntity.RelEntityId,
                    CaseData = caseEntity.CaseData
                };

                var restult = AddCase(caseMapper, userNumber);

                //记录caseid 
                if (restult.Status == 0)
                {
                    caseIds.Add(restult.DataBody.ToString());
                }

            }

            if (caseIds.Count > 0)
            {

                //获取下一个node
                var nextNodeResult = _workFlowRepository.NextNodeData(Guid.Parse(caseIds.FirstOrDefault()), userNumber);

                var node = nextNodeResult["node"];
                var user = nextNodeResult["user"];

                var finalResult = new
                {
                    node,
                    user,
                    caseIds
                };
                return new OutputResult<object>(finalResult);
            }
            else
            {
                return new OutputResult<object>()
                {
                    Status = 1,//失败
                    Message = "选择的数据有问题，请重新选择",

                };


            }
        }



        /// <summary>
        /// 添加多个case item
        /// </summary>
        /// <param name="caseModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> AddMultipleCaseItem(WorkFlowAddMultipleCaseItemModel caseModel, int userNumber)
        {
            WorkFlowAddMulpleCaseItemMapper _mapper = new WorkFlowAddMulpleCaseItemMapper()
            {
                CaseId = caseModel.CaseId,
                NodeNum = caseModel.NodeNum,
                HandleUser = caseModel.HandleUser,
                CopyUser = caseModel.CopyUser,
                Remark = caseModel.Remark,
                CaseData = caseModel.CaseData

            };

            if (_mapper == null || !_mapper.IsValid())
            {
                return HandleValid(_mapper);
            }

            foreach (var id in _mapper.CaseId)
            {
                WorkFlowAddCaseItemMapper caseItemEntity = new WorkFlowAddCaseItemMapper()
                {
                    CaseId = id,
                    NodeNum = caseModel.NodeNum,
                    HandleUser = caseModel.HandleUser,
                    CopyUser = caseModel.CopyUser,
                };

                var resultAddCaseItem = _workFlowRepository.AddCaseItem(caseItemEntity, userNumber);
                if (resultAddCaseItem.Flag == 1)
                {
                    WriteCaseItemMessage(0, id, caseModel.NodeNum, userNumber);
                }
            }


            return HandleResult(new OperateResult() { Flag = 1 });

        }

    }
}
