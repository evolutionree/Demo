using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class GetMessageStatusResponseData
    {
        [JsonProperty("list")]
        public List<MessageStatusResponseData> DataList { set; get; }
    }

    public class MessageStatusResponseData
    {
        [JsonProperty("push_id")]
        public string Push_id { set; get; }
        /// <summary>
        /// 0（未处理）/1（推送中）/2（推送完成）/3（推送失败）
        /// </summary>
       [JsonProperty("status")]
        public int Status { set; get; }
        /// <summary>
        /// 开始时间：year-mon-day hour:min:sec
        /// </summary>
        [JsonProperty("start_time")]
        public DateTime Start_time { set; get; }

        /// <summary>
        /// 已发送
        /// </summary>
        [JsonProperty("finished")]
        public string Finished { set; get; }
        /// <summary>
        /// 共需要发送
        /// </summary>
        [JsonProperty("total")]
        public string Total { set; get; }
    }
}
