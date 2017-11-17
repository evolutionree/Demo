using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class GetAppDeviceNumResponseData
    {
        [JsonProperty("device_num")]
        public int DeviceNum { set; get; }
    }
}
