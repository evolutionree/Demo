using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class DevicesPushResponseData
    {
        /// <summary>
        /// 表示给app下发的任务id，如果是循环任务，返回的是循环父任务id
        /// </summary>
        [JsonProperty("push_id")]
        public long PushId { set; get; }
    }
}
