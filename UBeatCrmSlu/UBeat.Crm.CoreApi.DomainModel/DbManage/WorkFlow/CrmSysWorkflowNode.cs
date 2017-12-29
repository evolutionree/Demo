using Newtonsoft.Json;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowNode
    {
        [JsonProperty("nodeid")]
        public Guid NodeId { set; get; }

        [JsonProperty("nodename")]
        public string NodeName { set; get; }

        [JsonProperty("flowid")]
        public Guid FlowId { set; get; }

        [JsonProperty("auditnum")]
        public int AuditNum { set; get; }

        [JsonProperty("nodetype")]
        public int NodeType { set; get; }

        [JsonProperty("steptypeid")]
        public int StepTypeId { set; get; }
        
        [SqlType(NpgsqlDbType.Jsonb)]
        [JsonProperty("ruleconfig")]
        public object RuleConfig { set; get; }
      
        [SqlType(NpgsqlDbType.Jsonb)]
        [JsonProperty("columnconfig")]
        public object ColumnConfig { set; get; }

        [JsonProperty("vernum")]
        public int VerNum { set; get; }

        [JsonProperty("auditsucc")]
        public int AuditSucc { set; get; }
       
        [SqlType(NpgsqlDbType.Jsonb)]
        [JsonProperty("nodeconfig")]
        public object NodeConfig { set; get; }

    }
}
