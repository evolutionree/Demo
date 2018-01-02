using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.UkQrtz
{
    /// <summary>
    /// 调度服务的运行实例
    /// </summary>
    public class TriggerInstanceInfo
    {

        [JsonProperty("recid")]
        public Guid RecId { get; set; }

        [JsonProperty("triggerid")]
        public Guid TriggerId { get; set; }

        [JsonProperty("begintime")]
        public DateTime BeginTime { get; set; }

        [JsonProperty("endtime")]
        public DateTime EndTime { get; set; }

        [JsonProperty("status")]
        public TriggerInstanceStatusEnum Status { get; set; }

        [JsonProperty("errormsg")]
        public string ErrorMsg { get; set; }

        [JsonProperty("runserver")]
        public string RunServer { get; set; }
    }
    public enum TriggerInstanceStatusEnum {
        Prepared =0,
        Running = 1,
        Completed = 2,
        CompletedWithError = -2
    }
}
