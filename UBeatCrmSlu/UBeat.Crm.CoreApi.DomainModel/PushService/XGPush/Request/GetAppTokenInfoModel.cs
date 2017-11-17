using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class GetAppTokenInfoModel:BaseRequestModel
    {
        public string device_token { set; get; }
    }
}
