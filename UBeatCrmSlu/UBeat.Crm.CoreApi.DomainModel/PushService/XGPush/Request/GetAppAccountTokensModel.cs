using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class GetAppAccountTokensModel:BaseRequestModel
    {
        /// <summary>
        /// 帐号
        /// </summary>
        public string account { set; get; }
    }
}
