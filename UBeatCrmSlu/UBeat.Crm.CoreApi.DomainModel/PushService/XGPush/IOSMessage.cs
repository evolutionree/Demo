using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    /// <summary>
    /// IOS消息实体，参数为APNS规定的payload
    /// </summary>
    public class IOSMessage: XGMessage
    {
        [JsonProperty("aps")]
        public APS APS { set; get; }

        /// <summary>
        /// 允许推送给用户的时段，选填。accept_time不会占用payload容量
        /// </summary>
        [JsonProperty("accept_time")]
        public List<AcceptTime> AcceptTimes { set; get; }
        
        ///// <summary>
        /////  合法的自定义key-value，会传递给app
        ///// </summary>
        //[JsonProperty("custom1")]
        //public string custom1 { set; get; }

    }

    public class APS
    {
        [JsonProperty("alert")]
        public AlertInfo Alert { set; get; }

        //[JsonProperty("badge")]
        [JsonIgnore]
        public int Badge { set; get; } = 1;

        //[JsonProperty("category")]
        //public string Category { set; get; } 
    }

    public class AlertInfo
    {
        public AlertInfo()
        {

        }
        public AlertInfo(string body)
        {
            Body = body;
        }

        [JsonProperty("body")]
        public string Body { set; get; }

        //[JsonProperty("action-loc-key")]
        //public string ActionLocKey { set; get; } = "PLAY";
    }
}
