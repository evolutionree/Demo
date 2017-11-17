using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class GetMessageStatusModel:BaseRequestModel
    {
        public string push_ids { set; get; }
    }

    public class PushIDModel
    {
        [JsonProperty("push_id")]
        public string PushId { set; get; }
    }
}
