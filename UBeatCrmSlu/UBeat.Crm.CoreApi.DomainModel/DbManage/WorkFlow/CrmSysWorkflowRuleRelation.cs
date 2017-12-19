using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowRuleRelation
    {
        public Guid FlowId { set; get; }

        public Guid RuleId { set; get; }

        public DbRuleInfo RuleInfo { set; get; }

    }
}
