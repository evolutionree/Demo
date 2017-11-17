using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class SingleAccountPushModel: PushBaseModel
    {
        /// <summary>
        /// 针对某一账号推送，帐号可以是qq号，邮箱号，openid，手机号等各种类型
        /// </summary>
        public string account { set; get; }

        /// <summary>
        /// 指定推送时间，格式为year-mon-day hour:min:sec 若小于服务器当前时间，则会立即推送
        /// </summary>
        public string send_time { set; get; }

    }
}
