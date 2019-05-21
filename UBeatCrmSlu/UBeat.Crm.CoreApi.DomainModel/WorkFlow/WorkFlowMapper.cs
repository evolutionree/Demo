using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    public class WorkFlowAddCaseMapper : BaseEntity
    {
        public Guid FlowId { get; set; }
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
        public Guid? RelEntityId { get; set; }
        public Guid? RelRecId { get; set; }
        public string Title { get; set; }
        public Dictionary<string, object> CaseData { get; set; }
        protected override IValidator GetValidator()
        {
            return new WorkFlowAddCaseMapperValidator();
        }
    }

    public class WorkFlowAddCaseMapperValidator : AbstractValidator<WorkFlowAddCaseMapper>
    {
        public WorkFlowAddCaseMapperValidator()
        {
            RuleFor(d => d.FlowId).NotNull().WithMessage("流程ID不能为空");
            RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            RuleFor(d => d.RecId).NotNull().WithMessage("数据ID不能为空");
            RuleFor(d => d).Must(x=>ValidRelation(x.RelEntityId,x.RelRecId)).WithMessage("关联实体数据必须同时有值");
        }

        public static bool ValidRelation(Guid? relEntityId,Guid? relRecId)
        {
            if (!relEntityId.HasValue && !relRecId.HasValue) return true;
            if (relEntityId.HasValue && relRecId.HasValue) return true;
            return false;
        }
    }

    public class WorkFlowAddCaseItemMapper : BaseEntity
    {
        public Guid CaseId { get; set; }
        public int NodeNum { get; set; }
        public string HandleUser { get; set; }
        public string CopyUser { get; set; }
        public string Remark { get; set; }
        public Dictionary<string, object> CaseData { get; set; }
        protected override IValidator GetValidator()
        {
            return new WorkFlowAddCaseItemMapperValidator();
        }
    }

    public class WorkFlowAddCaseItemMapperValidator : AbstractValidator<WorkFlowAddCaseItemMapper>
    {
        public WorkFlowAddCaseItemMapperValidator()
        {
            RuleFor(d => d.CaseId).NotNull().WithMessage("流程明细ID不能为空");
            RuleFor(d => d.NodeNum).NotNull().WithMessage("流程节点数不能为空");
            RuleFor(d => d.HandleUser).NotNull().WithMessage("步骤审批人不能为空");
        }
    }

    public class WorkFlowAuditCaseItemMapper : BaseEntity
    {
        public Guid CaseId { get; set; }
        public int NodeNum { get; set; }
        public string Suggest { get; set; }
		public Dictionary<string, object> CaseData { get; set; }
		public int ChoiceStatus { get; set; }

        /// <summary>
        /// 分支流程时，选择的下一节点id
        /// </summary>
        public Guid NodeId { set; get; }
        public string HandleUser { get; set; }
        public string CopyUser { get; set; }

        protected override IValidator GetValidator()
        {
            return new WorkFlowAuditCaseItemMapperValidator();
        }
    }

    public class WorkFlowAuditCaseItemMapperValidator : AbstractValidator<WorkFlowAuditCaseItemMapper>
    {
        public WorkFlowAuditCaseItemMapperValidator()
        {
            RuleFor(d => d.CaseId).NotNull().WithMessage("流程明细ID不能为空");
            RuleFor(d => d.NodeNum).NotNull().WithMessage("流程节点数不能为空");
            RuleFor(d => d.ChoiceStatus).NotNull().WithMessage("审批状态不能为空");
        }
    }

    public class WorkFlowNodeLinesConfigMapper : BaseEntity
    {
        public Guid FlowId { get; set; }
        public List<WorkFlowNodeMapper> Nodes { get; set; }
        public List<WorkFlowLineMapper> Lines { get; set; }

        protected override IValidator GetValidator()
        {
            return new WorkFlowNodeLinesConfigMapperValidator();
        }
    }

    public class WorkFlowNodeLinesConfigMapperValidator : AbstractValidator<WorkFlowNodeLinesConfigMapper>
    {
        public WorkFlowNodeLinesConfigMapperValidator()
        {
            RuleFor(d => d.FlowId).NotNull().WithMessage("流程ID不能为空");
            RuleFor(d => d.Nodes).Must(ValidateNodes).WithMessage("节点数据配置验证失败");
            RuleFor(d => d.Lines).Must(ValidateLines).WithMessage("连线数据配置验证失败");
        }

        public static bool ValidateNodes(List<WorkFlowNodeMapper> nodes)
        {
            if (nodes == null || nodes.Count < 2) return false;

            foreach (var node in nodes)
            {
                if (!node.IsValid())
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ValidateLines(List<WorkFlowLineMapper> lines)
        {
            if (lines == null || lines.Count < 1) return false;

            foreach (var line in lines)
            {
                if (!line.IsValid())
                {
                    return false;
                }
            }
            return true;
        }
    }


    public class WorkFlowNodeMapper : BaseEntity
    {
        public Guid NodeId { set; get; }
        public string NodeName { get; set; }
        public int NodeNum { get; set; }
        public int AuditNum { get; set; }
        public int NodeType { get; set; }
        public int StepTypeId { get; set; }
        public int StepCPTypeId { get; set; }
        public Dictionary<string, string> RuleConfig { get; set; }
        public Dictionary<string,object> ColumnConfig { get; set; }
        public int AuditSucc { get; set; }

        public string NodeEvent { set; get; }

        /// <summary>
        /// 节点配置数据，如位置坐标
        /// </summary>
        public Dictionary<string, object> NodeConfig { set; get; }

        protected override IValidator GetValidator()
        {
            return new WorkFlowNodeMapperValidator();
        }
    }

    public class WorkFlowNodeMapperValidator : AbstractValidator<WorkFlowNodeMapper>
    {
        public WorkFlowNodeMapperValidator()
        {
            RuleFor(d => d.NodeName).NotEmpty().WithMessage("节点名称不能为空");
            RuleFor(d => d.NodeNum).NotNull().WithMessage("节点数不能为空");
            RuleFor(d => d.AuditNum).NotNull().GreaterThanOrEqualTo(0).WithMessage("审批人数不能为空");
            RuleFor(d => d.NodeType).NotNull().GreaterThanOrEqualTo(0).WithMessage("节点类型不能为空");
            RuleFor(d => d.StepTypeId).NotNull().WithMessage("节点范围类型不能为空");
            RuleFor(d => d.AuditSucc).NotNull().GreaterThanOrEqualTo(0).WithMessage("审批同意数不能为空");
            RuleFor(d => d.RuleConfig).NotNull().WithMessage("规则配置不能为空");
            RuleFor(d => d.ColumnConfig).NotNull().WithMessage("字段配置不能为空");
        }
    }

    public class WorkFlowLineMapper : BaseEntity
    {
        public int FromNode { get; set; }//旧设计，已弃用
        public int EndNode { get; set; }//旧设计，已弃用
        public Guid FromNodeId { get; set; }
        public Guid ToNodeId { get; set; }

        public Guid? RuleId { get; set; }

        /// <summary>
        /// 节点连线配置数据，如位置坐标
        /// </summary>
        public object LineConfig { set; get; }

        protected override IValidator GetValidator()
        {
            return new WorkFlowLineMapperValidator();
        }
    }

    public class WorkFlowLineMapperValidator : AbstractValidator<WorkFlowLineMapper>
    {
        public WorkFlowLineMapperValidator()
        {
            RuleFor(d => d.FromNode).NotNull().WithMessage("开始节点不能为空");
            RuleFor(d => d.EndNode).NotNull().WithMessage("结束节点不能为空");
        }
    }

    public class WorkFlowAddMapper : BaseEntity
    {
        public string FlowName { get; set; }
        public int FlowType { get; set; }
        public int BackFlag { get; set; }
        public int ResetFlag { get; set; }
        public int ExpireDay { get; set; }
        public string Remark { get; set; }
        public Guid EntityId { get; set; }
        public int SkipFlag { get; set; }
        public Dictionary<string, string> FlowName_Lang { get; set; }
		public Dictionary<string, object> Config { get; set; }
		protected override IValidator GetValidator()
        {
            return new WorkFlowAddMapperValidator();
        }
    }

    public class WorkFlowAddMapperValidator : AbstractValidator<WorkFlowAddMapper>
    {
        public WorkFlowAddMapperValidator()
        {
            RuleFor(d => d.FlowName).NotNull().WithMessage("流程名称不能为空");
            RuleFor(d => d.FlowType).NotNull().WithMessage("流程类型不能为空");
            RuleFor(d => d.BackFlag).NotNull().WithMessage("退回标志不能为空");
            RuleFor(d => d.ResetFlag).NotNull().WithMessage("重置标志不能为空");
            RuleFor(d => d.ExpireDay).NotNull().GreaterThanOrEqualTo(0).WithMessage("过期天数不能为空");
            RuleFor(d => d.Remark).NotNull().WithMessage("备注不能为空");
            RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            RuleFor(d => d.SkipFlag).NotNull().WithMessage("跳过标志不能为空");
			RuleFor(d => d.Config).NotNull().WithMessage("入口标志不能为空");
		}
    }

    public class WorkFlowUpdateMapper : BaseEntity
    {
        public Guid FlowId { get; set; }
        public string FlowName { get; set; }
        public int BackFlag { get; set; }
        public int ResetFlag { get; set; }
        public int ExpireDay { get; set; }
        public string Remark { get; set; }
        public int SkipFlag { get; set; }
        public Dictionary<string,string> FlowName_Lang { get; set; }
		public Dictionary<string, object> Config { get; set; }
		protected override IValidator GetValidator()
        {
            return new WorkFlowUpdateMapperValidator();
        }
    }

    public class WorkFlowUpdateMapperValidator : AbstractValidator<WorkFlowUpdateMapper>
    {
        public WorkFlowUpdateMapperValidator()
        {
            RuleFor(d => d.FlowId).NotNull().WithMessage("流程ID不能为空");
            RuleFor(d => d.FlowName).NotNull().WithMessage("流程名称不能为空");
            RuleFor(d => d.BackFlag).NotNull().WithMessage("退回标志不能为空");
            RuleFor(d => d.ResetFlag).NotNull().WithMessage("重置标志不能为空");
            RuleFor(d => d.ExpireDay).NotNull().GreaterThanOrEqualTo(0).WithMessage("过期天数不能为空");
            RuleFor(d => d.Remark).NotNull().WithMessage("备注不能为空");
            RuleFor(d => d.SkipFlag).NotNull().WithMessage("跳过标志不能为空");
			RuleFor(d => d.Config).NotNull().WithMessage("入口标志不能为空");
		}
    }


    public class WorkFlowAddMultipleCaseMapper : BaseEntity
    {
        public Guid FlowId { get; set; }
        public Guid EntityId { get; set; }
        public List<Guid> RecId { get; set; }
        public Guid? RelEntityId { get; set; }
        public Guid? RelRecId { get; set; }
        public Dictionary<string, object> CaseData { get; set; }

        public string HandleUser { get; set; }
        public string CopyUser { get; set; }
        

        protected override IValidator GetValidator()
        {
            return new WorkFlowAddMultipleCaseMapperValidator();
        }
    }

    public class WorkFlowAddMultipleCaseMapperValidator : AbstractValidator<WorkFlowAddMultipleCaseMapper>
    {
        public WorkFlowAddMultipleCaseMapperValidator()
        {
            RuleFor(d => d.FlowId).NotNull().WithMessage("流程ID不能为空");
            RuleFor(d => d.EntityId).NotNull().WithMessage("实体ID不能为空");
            RuleFor(d => d.RecId).NotNull().WithMessage("数据ID不能为空");
            RuleFor(d => d).Must(x => ValidRelation(x.RelEntityId, x.RelRecId)).WithMessage("关联实体数据必须同时有值");
        }

        public static bool ValidRelation(Guid? relEntityId, Guid? relRecId)
        {
            if (!relEntityId.HasValue && !relRecId.HasValue) return true;
            if (relEntityId.HasValue && relRecId.HasValue) return true;
            return false;
        }
    }


    public class WorkFlowAddMulpleCaseItemMapper : BaseEntity
    {
        public List<Guid> CaseId { get; set; }
        public int NodeNum { get; set; }
        public string HandleUser { get; set; }
        public string CopyUser { get; set; }
        public string Remark { get; set; }
        public Dictionary<string, object> CaseData { get; set; }
        protected override IValidator GetValidator()
        {
            return new WorkFlowAddMultipleCaseItemMapperValidator();
        }
    }

    public class WorkFlowAddMultipleCaseItemMapperValidator : AbstractValidator<WorkFlowAddMulpleCaseItemMapper>
    {
        public WorkFlowAddMultipleCaseItemMapperValidator()
        {
            RuleFor(d => d.CaseId).NotNull().WithMessage("流程明细ID不能为空");
            RuleFor(d => d.NodeNum).NotNull().WithMessage("流程节点数不能为空");
            RuleFor(d => d.HandleUser).NotNull().WithMessage("步骤审批人不能为空");
        }
    }

}
