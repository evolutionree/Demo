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
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class WorkFlowServices : BaseServices
    {
        private readonly IWorkFlowRepository _workFlowRepository;
        private readonly IRuleRepository _ruleRepository;
        private readonly IMapper _mapper;
        private readonly IEntityProRepository _entityProRepository;
        private readonly IDynamicRepository _dynamicRepository;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;

        public WorkFlowServices(IMapper mapper, IWorkFlowRepository workFlowRepository, IRuleRepository ruleRepository, IEntityProRepository entityProRepository, IDynamicEntityRepository dynamicEntityRepository, IDynamicRepository dynamicRepository)
        {
            _workFlowRepository = workFlowRepository;
            _entityProRepository = entityProRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _dynamicRepository = dynamicRepository;
            _mapper = mapper;
            _ruleRepository = ruleRepository;
        }

        public OutputResult<object> CaseDetail(CaseDetailModel detailModel, int userNumber)
        {
            return null;
            //获取casedetail
            var caseInfo = _workFlowRepository.GetWorkFlowCaseInfo(null, detailModel.CaseId);



            //获取caseoperate
            //获取entitydetail
            //获取relatedetail
        }

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
            var result = new List<NodeDataModel>();

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
                    //获取当前流程走到哪个节点
                    //var nownodenum = _workFlowRepository.GetWorkFlowNowNodeNumber(caseModel.CaseId, userNumber, tran);
                    //var nextnodenum = _workFlowRepository.GetWorkFlowNextNodeNumber(caseInfo.CaseId, caseInfo.NodeNum, userNumber, tran);

                    //审批已经结束
                    if (caseInfo.NodeNum == -1)
                    {
                        nodetemp.NodeId = null;
                        nodetemp.NodeName = "流程结束";
                        nodetemp.NodeType = NodeType.Normal;
                        nodetemp.NodeNum = caseInfo.NodeNum;
                        nodetemp.StepTypeId = NodeStepType.Launch;
                        nodetemp.ColumnConfig = null;
                        nodetemp.AllowMulti = 0;
                        nodetemp.Stoped = 0;
                        nodetemp.AllowNext = 1;
                        nodetemp.FlowType = workflowInfo.FlowType;
                    }
                    //自由流程
                    else if (workflowInfo.FlowType == WorkFlowType.FreeFlow)
                    {
                        nodetemp.NodeId = null;
                        nodetemp.NodeName = "自由选择审批人";
                        nodetemp.NodeType = NodeType.Normal;
                        nodetemp.NodeNum = 1;
                        nodetemp.StepTypeId = NodeStepType.SelectByUser;
                        nodetemp.ColumnConfig = null;
                        nodetemp.AllowMulti = 0;
                        nodetemp.Stoped = 0;
                        nodetemp.AllowNext = 1;
                        nodetemp.FlowType = WorkFlowType.FreeFlow;
                        if (caseInfo.AuditStatus == AuditStatusType.Begin)
                        {
                            nodetemp.FlowType = WorkFlowType.FixedFlow;
                        }

                        var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, Guid.Empty, userNumber, workflowInfo.FlowType, tran);
                        result.Add(new NodeDataModel()
                        {
                            NodeInfo = nodetemp,
                            Approvers = users
                        });
                    }
                    //固定流程
                    else
                    {
                        //获取当前审批的实例item
                        var caseitems = _workFlowRepository.GetWorkFlowCaseItemInfo(tran, caseInfo.CaseId, caseInfo.NodeNum);
                        if (caseitems == null || caseitems.Count == 0)
                        {
                            throw new Exception("流程节点数据异常");
                        }
                        var nodeid = caseitems.FirstOrDefault().NodeId;
                        var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);


                        bool isCheckNextNode = false;

                        nodetemp.NodeId = flowNodeInfo.NodeId;
                        nodetemp.NodeNum = caseInfo.NodeNum;
                        nodetemp.StepTypeId = flowNodeInfo.StepTypeId;
                        nodetemp.ColumnConfig = flowNodeInfo.ColumnConfig;
                        nodetemp.AllowMulti = 0;
                        nodetemp.Stoped = 0;
                        nodetemp.AllowNext = 1;
                        nodetemp.FlowType = WorkFlowType.FixedFlow;

                        if (flowNodeInfo.NodeType == NodeType.Joint)//会审
                        {
                            //会审审批通过的节点数
                            var aproval_success_count = caseitems.Where(m => m.ChoiceStatus == ChoiceStatusType.Approval).Count();
                            if (aproval_success_count < flowNodeInfo.AuditSucc)//--说明当前节点，其他人还在审批，不能进入下一步
                            {
                                nodetemp.NodeName = "等待他人审批";
                                nodetemp.NodeType = NodeType.Joint;
                            }
                            else isCheckNextNode = true;
                        }
                        else
                        {
                            if (caseitems.Where(m => m.CaseStatus == CaseStatusType.WaitApproval || m.CaseStatus == CaseStatusType.Readed).Count() > 0)
                            {
                                nodetemp.NodeName = "等待审批";
                                nodetemp.NodeType = NodeType.Normal;
                            }
                            else isCheckNextNode = true;
                        }
                        //检查下一点，获取下一节点信息
                        if (isCheckNextNode)
                        {
                            //获取下一节点
                            var nextnodes = _workFlowRepository.GetNextNodeDataInfoList(caseInfo.FlowId, nodeid, caseInfo.VerNum, tran);
                            if (nextnodes == null || nextnodes.Count == 0)
                                throw new Exception("获取不到节点配置");

                            foreach (var m in nextnodes)
                            {
                                m.FlowType = WorkFlowType.FixedFlow;
                                var users = _workFlowRepository.GetFlowNodeApprovers(caseInfo.CaseId, m.NodeId.GetValueOrDefault(), userNumber, workflowInfo.FlowType, tran);
                                result.Add(new NodeDataModel()
                                {
                                    NodeInfo = m,
                                    Approvers = users
                                });
                            }
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

        public OutputResult<object> WorkFlowAudit(WorkFlowAuditCaseItemModel caseItemModel, AccountUserInfo userinfo)
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
            //如果没有下一步，表示流程审批走到最后节点
            bool hasNextNode = true;
            bool canAddNextNode = true;
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
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
                        AuditFreeFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, caseitems);
                        if (casefinish)
                        {
                            nodeid = new Guid("00000000-0000-0000-0000-000000000001");
                            hasNextNode = false;
                        }
                    }
                    else //固定流程
                    {
                        nodeid = AuditFixedFlow(userinfo, caseItemEntity, ref casefinish, ref hasNextNode, ref canAddNextNode, tran, caseInfo, caseitems);
                    }
                    //判断是否有附加函数_event_func
                    var eventfuncname = _workFlowRepository.GetWorkFlowEvent(workflowInfo.FlowId, nodeid, 1, tran);
                    if (!string.IsNullOrEmpty(eventfuncname))
                    {
                        _workFlowRepository.ExecuteWorkFlowEvent(eventfuncname, caseInfo.CaseId, caseInfo.NodeNum, caseItemEntity.ChoiceStatus, userinfo.UserId, tran);
                    }

                    //流程审批过程修改实体字段时，更新关联实体的字段数据
                    _workFlowRepository.ExecuteUpdateWorkFlowEntity(caseInfo.CaseId, caseInfo.NodeNum, userinfo.UserId, tran);

                    if (casefinish)//审批已经到达了最后一步
                    {
                        _workFlowRepository.EndWorkFlowCaseItem(caseInfo.CaseId, stepnum + 1, userinfo.UserId, tran);
                    }
                    else
                    {
                        if (canAddNextNode && (caseItemEntity.ChoiceStatus == 1 || caseItemEntity.ChoiceStatus == 4)) //如果是审批通过或者编辑重新发起操作，则需要添加下一步骤审批节点
                            AddCaseItem(nodeid, caseItemModel, workflowInfo, caseInfo, stepnum + 1, userinfo, tran);
                    }
                    //写审批消息
                    WriteCaseAuditMessage(caseInfo.CaseId, caseInfo.NodeNum, stepnum, userinfo.UserId);
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


            return new OutputResult<object>(null);
        }


        #region --审批固定流程--
        private Guid AuditFixedFlow(AccountUserInfo userinfo, WorkFlowAuditCaseItemMapper caseItemEntity, ref bool casefinish, ref bool hasNextNode, ref bool canAddNextNode, DbTransaction tran, WorkFlowCaseInfo caseInfo, List<WorkFlowCaseItemInfo> caseitems)
        {
            Guid nodeid = caseitems.FirstOrDefault().NodeId;
            var flowNodeInfo = _workFlowRepository.GetWorkFlowNodeInfo(tran, nodeid);
            if (flowNodeInfo == null)
                throw new Exception("流程配置不存在");
            //获取下一步节点，
            var flowNextNodeInfos = _workFlowRepository.GetNextNodeInfoList(tran, caseInfo.FlowId, caseInfo.VerNum, nodeid);

            if (flowNextNodeInfos == null || flowNextNodeInfos.Count == 0)
            {
                hasNextNode = false;
            }

            //如果配置节点有多个，属于分支流程
            if (flowNextNodeInfos.Count > 1)//分支流程,如果是分支流程，需要验证下一步审批人是否符合规则
            {
                //获取选中的分支
                var nextNode = flowNextNodeInfos.Find(m => m.NodeId == caseItemEntity.NodeId);
                //验证规则是否符合
                if (!ValidateNextNodeRule(caseInfo, nextNode, userinfo, tran))
                    throw new Exception("下一步审批人不符分支流程规则");
            }
            //验证通过，则执行审批逻辑
            if (caseitems.Count == 1)//普通审批
            {
                var nowcaseitem = caseitems.FirstOrDefault();
                AuditNormalFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, nowcaseitem, hasNextNode);
            }
            else  //会审
            {
                var nowcaseitem = caseitems.Find(m => m.HandleUser == userinfo.UserId);
                if (nowcaseitem == null)
                    throw new Exception("您没有审批当前节点的权限");
                canAddNextNode = AuditJoinFlow(userinfo, caseItemEntity, ref casefinish, tran, caseInfo, flowNodeInfo, nowcaseitem, hasNextNode);
            }

            return nodeid;
        }
        #endregion

        #region --自由流程审批--
        private void AuditFreeFlow(AccountUserInfo userinfo, WorkFlowAuditCaseItemMapper caseItemEntity, ref bool casefinish, DbTransaction tran, WorkFlowCaseInfo caseInfo, List<WorkFlowCaseItemInfo> caseitems)
        {
            int casenodenum = caseInfo.NodeNum;
            var nowcaseitem = caseitems.FirstOrDefault();
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
                        casefinish = true;
                    }
                    else
                    {
                        auditstatus = AuditStatusType.Approving;
                        casenodenum = 1;
                    }
                    break;
                case 2:       //2退回
                    var rebackResult = _workFlowRepository.RebackWorkFlowCaseItem(nowcaseitem, userinfo.UserId, tran);
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
                    break;
            }
            var auditcase = _workFlowRepository.AuditWorkFlowCase(caseInfo.CaseId, auditstatus, casenodenum, userinfo.UserId, tran);
        }
        #endregion

        #region --固定普通流程审批--
        private void AuditNormalFlow(AccountUserInfo userinfo, WorkFlowAuditCaseItemMapper caseItemEntity, ref bool casefinish, DbTransaction tran, WorkFlowCaseInfo caseInfo, WorkFlowCaseItemInfo nowcaseitem, bool hasNextNode)
        {

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
                        }
                        else
                        {
                            auditstatus = AuditStatusType.Finished;
                            casefinish = true;
                        }
                    }
                    break;
                case 2:       //2退回
                    var rebackResult = _workFlowRepository.RebackWorkFlowCaseItem(nowcaseitem, userinfo.UserId, tran);
                    casenodenum = 0;
                    break;
                case 3:       //3中止,中止一般是由审批发起人主动终止
                    casefinish = true;
                    auditstatus = AuditStatusType.NotAllowed;
                    break;
                case 4:       //4编辑
                    casenodenum = 0;
                    auditstatus = AuditStatusType.Begin;
                    _workFlowRepository.ReOpenWorkFlowCase(caseInfo.CaseId, nowcaseitem.CaseItemId, userinfo.UserId, tran);
                    break;
            }
            if (casefinish)
                casenodenum = -1;
            var auditcase = _workFlowRepository.AuditWorkFlowCase(caseInfo.CaseId, auditstatus, casenodenum, userinfo.UserId, tran);
        }
        #endregion

        #region --固定会审流程审批--
        private bool AuditJoinFlow(AccountUserInfo userinfo, WorkFlowAuditCaseItemMapper caseItemEntity, ref bool casefinish, DbTransaction tran, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo nodeInfo, WorkFlowCaseItemInfo nowcaseitem, bool hasNextNode)
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
                _workFlowRepository.ReOpenWorkFlowCase(caseInfo.CaseId, nowcaseitem.CaseItemId, userinfo.UserId, tran);
            }
            else
            {
                if (aproval_success_count >= nodeInfo.AuditSucc)//判断是否达到会审审批通过条件
                {
                    if (hasNextNode)
                    {
                        auditstatus = AuditStatusType.Approving;
                        canAddNextNodeItem = false;
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
                    if (caseitems.Exists(m => m.ChoiceStatus == ChoiceStatusType.Reback))//如果有人退回，则优先执行退回
                    {
                        var rebackResult = _workFlowRepository.RebackWorkFlowCaseItem(nowcaseitem, userinfo.UserId, tran);
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
        private void AddCaseItem(Guid fromNodeid, WorkFlowAuditCaseItemModel caseItemModel, WorkFlowInfo workFlowInfo, WorkFlowCaseInfo caseInfo, int stepnum, AccountUserInfo userinfo, DbTransaction trans = null)
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
                    caseitemlist = caseitemlist.OrderByDescending(m => m.StepNum).ToList();

                    //获取还有人未处理审批
                    var aproval_notdeal_count = caseitemlist.Where(m => m.CaseStatus == CaseStatusType.WaitApproval || m.CaseStatus == CaseStatusType.Readed).Count();
                    if (aproval_notdeal_count > 0)
                        throw new Exception("该节点其他人还在审批，不能进入下一步");
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
            var result = _workFlowRepository.AddCaseItem(caseitems, userinfo.UserId, trans);

            if (result)
            {
                WriteCaseItemMessage(0, caseInfo.CaseId, caseInfo.NodeNum + 1, userinfo.UserId);
            }
        }
        #endregion

        /// <summary>
        /// 写入添加流程的消息
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="nodeNum"></param>
        /// <param name="userNumber"></param>
        public void WriteCaseAuditMessage(Guid caseId, int nodeNum,int stepNum, int userNumber)
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

                        #region --获取审批人和抄送人--
                        List<int> approvers = new List<int>();//审批人
                        List<int> copyusers = new List<int>();//抄送人
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
                        #endregion

                        var entityInfotemp = _entityProRepository.GetEntityInfo(caseInfo.EntityId);
                        if (entityInfotemp == null)
                            return;
                        var msg = new MessageParameter();
                        allApprovalSuggest = myAuditCaseItem.Suggest;

                        if (workflowInfo.FlowType== WorkFlowType.FreeFlow)//自由流程
                        {

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
                            WorkFlowNodeInfo previousFlowNodeInfo = null;//上一审批节点
                            if (nodeNum > 1 && flowNodeInfo != null)
                                previousFlowNodeInfo = _workFlowRepository.GetPreviousWorkFlowNodeInfo(tran, caseInfo.FlowId, caseInfo.VerNum, nodeid);
                            if(flowNodeInfo.NodeType != NodeType.Joint)//普通审批
                            {
                                var funcode=GetNormalFlowMessageFuncode(myAuditCaseItem, caseInfo, previousFlowNodeInfo);
                                if(string.IsNullOrEmpty(funcode))
                                {
                                    Logger.Error("没有有效的消息模板funcode");
                                    return;
                                }
                                msg.FuncCode = funcode;

                            }
                            else //会审
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

                                switch (myAuditCaseItem.ChoiceStatus)
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
                                        if (myAuditCaseItem.NodeNum == 1)
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
                           
                        }

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


        #region --获取普通流程审批消息的funcode--
        private string GetNormalFlowMessageFuncode(WorkFlowCaseItemInfo auditCaseItem, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo previousFlowNodeInfo)
        {
            string funcCode = string.Empty;
            switch (auditCaseItem.ChoiceStatus)
            {
                case ChoiceStatusType.Edit:
                    if (caseInfo.AuditStatus == AuditStatusType.Begin || caseInfo.AuditStatus == AuditStatusType.Approving)//编辑重新发起
                    {
                        funcCode = "WorkFlowLaunch";
                    }
                    break;
                case ChoiceStatusType.Approval://普通审批通过
                    if (caseInfo.AuditStatus == AuditStatusType.Finished)//审批完成
                    {
                        funcCode = "WorkFlowNodeFinish";
                    }
                    else
                    {
                        //由于先阶段，审批和选人是分开步骤，因此该情况不处理消息
                        return null;
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
                case ChoiceStatusType.AddNode://选择下一步审批人时的消息
                    //if (workflowInfo.FlowType == WorkFlowType.FreeFlow)//自由流程
                    //{
                    //    if (auditCaseItem.StepNum == 1)
                    //    {
                    //        funcCode = "WorkFlowLaunch";
                    //    }
                    //    else funcCode = "WorkFlowNodeApproval";
                    //}
                    //else
                    //{
                    //    if (auditCaseItem.NodeNum == 1)
                    //    {
                    //        funcCode = "WorkFlowLaunch";
                    //    }
                    //    else if (previousFlowNodeInfo.NodeType == NodeType.Joint)
                    //        funcCode = "NextWorkFlowNodeJointApproval";
                    //    else funcCode = "WorkFlowNodeApproval";
                    //}

                    break;
                case ChoiceStatusType.EndPoint:
                    funcCode = "WorkFlowNodeFinish";
                    break;
            }
            return funcCode;
        }

        #endregion

        private bool ValidateNextNodeRule(WorkFlowCaseInfo caseinfo, WorkFlowNodeInfo node, AccountUserInfo userinfo, DbTransaction trans = null)
        {
            var ruleid = _workFlowRepository.GetNextNodeRuleId(node.FlowId, node.NodeNum, node.VerNum, trans);
            var ruleInfoList = _ruleRepository.GetRule(ruleid, userinfo.UserId);
            if (ruleInfoList != null && ruleInfoList.Count > 0)//如果存在合法的rulesql，则进行数据校验
            {
                var ruleInfo = ruleInfoList.FirstOrDefault();
                var temp = ruleInfo.Rulesql;
                var rulesql = string.IsNullOrEmpty(temp) ? "1=1" : string.Format("({0})", temp);

                var ruleFormatSql = RuleSqlHelper.FormatRuleSql(rulesql, userinfo.UserId, userinfo.DepartmentId);

                return _workFlowRepository.ValidateNextNodeRule(caseinfo, ruleFormatSql, userinfo.UserId, trans);
            }

            return true;
        }



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

        public OutputResult<object> NodeLinesConfig(WorkFlowNodeLinesConfigModel configModel, int userNumber)
        {
            //获取该实体分类的字段
            var configEntity = _mapper.Map<WorkFlowNodeLinesConfigModel, WorkFlowNodeLinesConfigMapper>(configModel);
            if (configEntity == null || !configEntity.IsValid())
            {
                return HandleValid(configEntity);
            }

            //var result = _workFlowRepository.NodeLinesConfig(configEntity, userNumber);
            _workFlowRepository.SaveNodeLinesConfig(configEntity, userNumber);
            IncreaseDataVersion(DataVersionType.FlowData, null);
            return new OutputResult<object>("");
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
