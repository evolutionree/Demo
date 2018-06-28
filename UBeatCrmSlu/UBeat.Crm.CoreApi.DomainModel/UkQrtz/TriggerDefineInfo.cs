using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.UkQrtz
{
    /// <summary>
    /// 调度服务的定义
    /// </summary>
    public class TriggerDefineInfo
    {
        
        [JsonProperty("recid")]
        public Guid RecId { get; set; }
        [JsonProperty("recname")]
        public string RecName { get; set; }

        [JsonProperty("recstatus")]
        public int RecStatus { get; set; }

        [JsonProperty("triggertime")]
        public string TriggerTime { get; set; }

        [JsonProperty("actiontype")]
        public TriggerActionType ActionType { get; set; }

        [JsonProperty("actioncmd")]
        public string ActionCmd { get; set; }
        [JsonProperty("actionparameters")]
        public string ActionParameters { get; set; }

        [JsonProperty("singlerun")]
        public int SingleRun { get; set; }

        [JsonProperty("remark")]
        public string Remark { get; set; }

        [JsonProperty("inbusy")]
        public int InBusy { get; set; }

        [JsonProperty("runningserver")]
        public string RunningServer { get; set; }

        [JsonProperty("startruntime")]
        public DateTime StartRunTime { get; set; }

        [JsonProperty("endruntime")]
        public DateTime EndRunTime { get; set; }

        [JsonProperty("lasterrortime")]
        public DateTime LastErrorTime { get; set; }

        [JsonProperty("errorcount")]
        public int ErrorCount { get; set; }
        [JsonProperty("reclanguage")]
        public string RecLanguage { get; set; }
    }
    public enum TriggerActionType {
        ActionType_Service = 1,
        ActionType_DbFunc = 2
    }
}
