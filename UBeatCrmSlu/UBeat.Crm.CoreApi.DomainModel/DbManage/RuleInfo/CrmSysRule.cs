using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo
{
    public class CrmSysRule
    {
        [JsonProperty("ruleid")]
        public Guid RuleId { set; get; }

        [JsonProperty("rulename")]
        public string RuleName { set; get; }

        [JsonProperty("entityid")]
        public Guid EntityId { set; get; }

        [JsonProperty("rulesql")]
        public string RuleSql { set; get; }

    }
}
