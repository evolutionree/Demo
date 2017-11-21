using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models
{
    /// <summary>
    /// 设备类型：0=WEB，1=IOS，2=Android
    /// </summary>
    public enum DeviceType
    {
        WEB = 0,
        IOS = 1,
        Android = 2,
        OtherDevice = 3
    }

    public enum DeviceClassic
    {
        WEB = 0,
        Phone = 1
    }
}
