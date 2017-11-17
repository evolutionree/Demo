using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class QueryTokenTagsModel:BaseRequestModel
    {
        public string device_token { set; get; }

    }
}
