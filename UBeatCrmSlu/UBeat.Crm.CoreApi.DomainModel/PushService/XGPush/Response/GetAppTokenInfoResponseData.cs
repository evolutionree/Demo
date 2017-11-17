using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class GetAppTokenInfoResponseData
    {
        /// <summary>
        /// （1为token已注册，0为未注册）
        /// </summary>
        [JsonProperty("isReg")]
        public int IsReg { set; get; }

        /// <summary>
        /// 最新活跃时间戳
        /// </summary>
        [JsonProperty("connTimestamp")]
        public long ConnTimestamp { set; get; }
        /// <summary>
        /// 该应用的离线消息数
        /// </summary>
        [JsonProperty("msgsNum")]
        public int MsgsNum { set; get; }
    }
}
