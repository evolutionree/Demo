using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    public class WorkFlowCaseItemInfo
    {
        public int? TransferUserId { get; set; }
        public int? SignUserId { get; set; }
        public int NowuserId { get; set; }
        public int NodeType { get; set; }
        /// <summary>
        /// 步骤明细ID
        /// </summary>
        public Guid CaseItemId { set; get; }
        /// <summary>
        /// 流程实例ID
        /// </summary>
        public Guid CaseId { set; get; }

        public Guid NodeId { set; get; }
        /// <summary>
        /// 当前审批步骤数，发起时为0   中间过程加1    结束为-1
        /// </summary>
        public int NodeNum { set; get; }
        /// <summary>
        /// 整个流程处理过程中的节点总数
        /// </summary>
        public int StepNum { set; get; }
        /// <summary>
        /// 操作选择 0拒绝 1通过 2退回 3中止 4编辑
        /// </summary>
        public ChoiceStatusType ChoiceStatus { set; get; }
        /// <summary>
        /// 改进建议
        /// </summary>
        public string Suggest { set; get; }
        /// <summary>
        /// 节点审批状态 0:未处理; 1:已读; 2:已处理 3作废
        /// </summary>
        public CaseStatusType CaseStatus { set; get; }
        /// <summary>
        /// 操作数据
        /// </summary>
        public object Casedata { set; get; }
        /// <summary>
        /// 个人备注
        /// </summary>
        public string Remark { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecCreated { set; get; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime RecUpdated { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int RecCreator { set; get; }
        /// <summary>
        /// 修改人
        /// </summary>
        public int RecUpdator { set; get; }
        /// <summary>
        /// 当前步骤处理人
        /// </summary>
        public int HandleUser { set; get; }

        /// <summary>
        /// 当前步骤处理人
        /// </summary>
        public string HandleUserName { set; get; }

        /// <summary>
        /// 当前步骤抄送人字符串
        /// </summary>
        public string CopyUser { set; get; }

       public int SkipNode { get; set; }


    }
    public class WorkFlowGlobalJsMapper : BaseEntity
    {
        public Guid NodeId { get; set; }
        public Guid FlowId { get; set; }
        public string Js { get; set; }
        public string Remark { get; set; }
        protected override IValidator GetValidator()
        {
            return new WorkFlowGlobalJsMapperValidator();
        }
        class WorkFlowGlobalJsMapperValidator : AbstractValidator<WorkFlowGlobalJsMapper>
        {
            public WorkFlowGlobalJsMapperValidator()
            {
                RuleFor(d => d.FlowId).NotNull().WithMessage("流程Id不能为空");
            }
        }

    }
    public class CaseItemFileAttach
    {
        public Guid CaseItemId { get; set; }
        public Guid FileId { get; set; }
        public string FileName { get; set; }
        public Guid RecId { get; set; }
    }

    public class CaseItemJoint
    {
        public Guid CaseId { get; set; }
        public Guid NodeId { get; set; }
        public Guid CaseItemid { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public int FlowStatus { get; set; }
    }
    public class WorkFlowNodeScheduled
    {
        public Guid CaseId { get; set; }
        public Guid NodeId { get; set; }
    }
    public class WorkFlowNodeScheduledList
    {
        public Guid CaseId { get; set; }
        public Guid NodeId { get; set; }
        public DateTime PointOfTime { get; set; }
        public int IsDone { get; set; }
    }
    public class CaseItemJointTransfer
    {
        public int SignStatus { get; set; }
        public int IsSignOrTransfer { get; set; }
        public Guid CaseItemid { get; set; }
        public int OrginUserId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; }
        public int FlowStatus { get; set; }
    }
    public class CaseItemTransferMapper
    {
        public int SignStatus { get; set; }
        public int ChoiceStatus { get; set; }
        public string Suggest { get; set; }
        public List<CaseItemFileAttach> Files { get; set; }
        public Guid CaseItemId { get; set; }
        public Guid CaseId { get; set; }
        public int UserId { get; set; }
        public int NodeNum { get; set; }
        public Guid NodeId { get; set; }
        public int IsSignOrTransfer { get; set; }
    }
    public class WorkFlowRepeatApprove
    {
        public WorkFlowEntityAddModel EntityModel { set; get; }
        public Guid RecId { get; set; }
        public Guid CaseId { get; set; }
        public Guid EntityId { get; set; }
        public Guid FlowId { get; set; }
        public int ModelType { get; set; }
    }
    public class WorkFlowEntityAddModel
    {
        public Guid TypeId { get; set; }
        public Dictionary<string, object> FieldData { get; set; }
        public Dictionary<string, object> ExtraData { get; set; }
        public List<Dictionary<string, object>> WriteBackData { get; set; }
        public Guid? FlowId { get; set; }
        public Guid? RelEntityId { get; set; }
        public Guid? RelRecId { get; set; }
        public Guid CacheId { get; set; }
    }
    public class InformerRuleMapper
    {
        public Guid RuleId { get; set; }
        public Guid FlowId { get; set; }
        public int InformerType { get; set; }
        public int RecStatus { get; set; }
        public string RuleConfig { get; set; }
        public int AuditStatus { get; set; } = -1;
    }
    public class InformerMapper
    {
        public Guid FlowId { get; set; }
        public List<InformerRuleMapper> InformerRules { get; set; }
    }


    public class WorkFlowCaseItemTransfer
    {

        public Guid caseitemid { get; set; }
        public int originuserid { get; set; }
        public int userid { get; set; }
        public int flowstatus { get; set; }
        public string comment { get; set; }
        public Guid recid { get; set; }
        public int operatetype { get; set; }

    }
}
