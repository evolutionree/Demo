using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class GetAppAccountTokensResponseData
    {
        [JsonProperty("tokens")]
        public List<string> Tokens { set; get; }
    }
}
