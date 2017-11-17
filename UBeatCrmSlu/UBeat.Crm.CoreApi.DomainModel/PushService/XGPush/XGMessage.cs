using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public abstract class XGMessage
    {

        ///// <summary>
        ///// 消息类型，0为通知推送，1为聊天短消息，2为聊天长消息
        ///// </summary>
        //[JsonProperty("mtype")]
        //public int MessageType { set; get; } = 0;
        /// <summary>
        /// 用户自定义的key-value，选填
        /// </summary>
        [JsonProperty("custom_content")]
        public Dictionary<string, object> CustomContent { set; get; }
       
        public string SerializeToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

   
}
