using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowNode
    {
        public Guid NodeId { set; get; }

        public string NodeName { set; get; }

        public Guid FlowId { set; get; }

    }
}
