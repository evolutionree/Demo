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
        /// 当前节点审批人类型
        /// </summary>
        public NodeStepType StepTypeId { set; get; }

        public object ColumnConfig { set; get; }

        public int AllowMulti { set; get; }

        public int Stoped { set; get; }

        public int AllowNext { set; get; }

        public WorkFlowType FlowType { set; get; }
    }
}
