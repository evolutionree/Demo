using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class DelAppAccountTokensResponseData
    {
        /// <summary>
        /// 显示删除device_token后该account映射的剩余token
        /// </summary>
        [JsonProperty("tokens")]
        public List<string> tokens { set; get; }
    }
}
