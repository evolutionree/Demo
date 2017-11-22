using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkFlow
{
    public class NextNodeDataInfo
    {
        public Guid? NodeId { set; get; }

        public string NodeName { set; get; }

        /// <summary>
        /// 0普通审批 1会审
        /// </summary>
        public NodeType NodeType { set; get; }


        public int NodeNum { set; get; }
        /// <summary>
        /// 节点状态：-1=流程已结束，0=可进入下一步，1=等待其他人会审
        /// </summary>
        public int NodeState { set; get; }

        /// <summary>
        /// 当前节点审批人类型
        /// </summary>
        public NodeStepType StepTypeId { set; get; }

        /// <summary>
        ///  节点审批通过还需多少人同意
        /// </summary>
        public int NeedSuccAuditCount { set; get; } = 1;

    }


}
