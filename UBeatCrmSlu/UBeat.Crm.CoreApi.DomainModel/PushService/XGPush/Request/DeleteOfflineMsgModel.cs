using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class DeleteOfflineMsgModel:BaseRequestModel 
    {
        public string push_id { set; get; }
    }
}
