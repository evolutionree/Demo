using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.PushService
{
    public class PushApiConfig
    {
        /// <summary>
        /// url信息，如openapi.xg.qq.com/v2
        /// </summary>
        public string BaseUrl { set; get; }

        /// <summary>
        /// IOS环境类型，1表示推送生产环境；2表示推送开发环境,0为安卓平台
        /// </summary>
        public int EnvironmentType { set; get; }
    }
}
