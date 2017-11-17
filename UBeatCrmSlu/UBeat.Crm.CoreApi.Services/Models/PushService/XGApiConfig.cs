using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.PushService;

namespace UBeat.Crm.CoreApi.Services.Models.PushService
{
    /// <summary>
    /// 信鸽推送接口的配置数据
    /// </summary>
    public class XGApiConfig: PushApiConfig
    {
        
        /// <summary>
        /// 应用的唯一标识符，在提交应用时管理系统返回。可在xg.qq.com管理台查看
        /// </summary>
        public long IOSAccessId { get; set; }
        /// <summary>
        /// 应用的唯一标识符，可在xg.qq.com管理台查看
        /// </summary>
        public string IOSSecretKey { get; set; }

        /// <summary>
        /// 应用的唯一标识符，在提交应用时管理系统返回。可在xg.qq.com管理台查看
        /// </summary>
        public long AndroidAccessId { get; set; }
        /// <summary>
        /// 应用的唯一标识符，可在xg.qq.com管理台查看
        /// </summary>
        public string AndroidSecretKey { get; set; }
        
    }
}

