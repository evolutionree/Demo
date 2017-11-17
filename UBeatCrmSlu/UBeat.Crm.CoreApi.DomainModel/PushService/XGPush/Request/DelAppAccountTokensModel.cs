using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class DelAppAccountTokensModel : BaseRequestModel
    {
        /// <summary>
        /// 账号，可以是邮箱号、手机号、QQ号等任意形式的业务帐号
        /// </summary>
        public string account { set; get; }
        /// <summary>
        /// token，设备的唯一识别ID
        /// </summary>
        public string device_token { set; get; }

    }
}
