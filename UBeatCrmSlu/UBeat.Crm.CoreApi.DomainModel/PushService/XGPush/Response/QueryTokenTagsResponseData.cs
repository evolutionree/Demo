using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class QueryTokenTagsResponseData
    {
        [JsonProperty("tags")]
        public List<string> Tags { set; get; }
    }
}
