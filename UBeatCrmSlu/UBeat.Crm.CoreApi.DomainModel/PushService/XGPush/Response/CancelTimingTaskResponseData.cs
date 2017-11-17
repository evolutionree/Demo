using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class CancelTimingTaskResponseData
    {
        /// <summary>
        /// 0为成功，其余为失败
        /// </summary>
        [JsonProperty("status")]
        public int Status { set; get; }
    }
}
