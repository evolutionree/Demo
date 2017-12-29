using Newtonsoft.Json;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowRuleRelation
    {
        [JsonProperty("flowid")]
        public Guid FlowId { set; get; }

        [JsonProperty("ruleid")]
        public Guid RuleId { set; get; }
        

    }
}
