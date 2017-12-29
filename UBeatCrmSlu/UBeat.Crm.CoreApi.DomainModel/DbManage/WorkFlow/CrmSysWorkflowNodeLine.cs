using Newtonsoft.Json;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowNodeLine
    {
        [JsonProperty("lineid")]
        public Guid LineId { set; get; }

        [JsonProperty("flowid")]
        public Guid FlowId { set; get; }

        [JsonProperty("ruleid")]
        public Guid RuleId { set; get; }

        [JsonProperty("vernum")]
        public int VerNum { set; get; }

        [JsonProperty("fromnodeid")]
        public Guid FromNodeId { set; get; }
       
        [JsonProperty("tonodeid")]
        public Guid ToNodeId { set; get; }

        /// <summary>
        /// 线配置字段，如线的位置等
        /// </summary>
        [SqlType(NpgsqlDbType.Jsonb)]
        [JsonProperty("lineconfig")]
        public object LineConfig { set; get; }

        

    }
}
