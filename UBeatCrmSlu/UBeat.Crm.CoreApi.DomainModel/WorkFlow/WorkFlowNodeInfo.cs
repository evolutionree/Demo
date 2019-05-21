using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    public class WorkFlowNodeInfo
    {
        public Guid NodeId { set; get; }

        public string NodeName { set; get; }

        public Guid FlowId { set; get; }

        public int NodeNum { set; get; }
        /// <summary>
        /// 需要多少人进行审批
        /// </summary>
        public int AuditNum { set; get; }

        /// <summary>
        /// 0普通审批 1会审
        /// </summary>
        public NodeType NodeType { set; get; }

        public NodeStepType StepTypeId { set; get; }

        public NodeStepType StepCPTypeId { set; get; }
        public object RuleConfig { set; get; }

        public object ColumnConfig { set; get; }

        public  int VerNum { set; get; }
        /// <summary>
        /// 通过审批最少需要多少人同意
        /// </summary>
        public int AuditSucc { set; get; }


    }
}
