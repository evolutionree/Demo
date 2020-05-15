using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class WJXServices : BasicBaseServices
    {
        private readonly IConfigurationRoot _configurationRoot;
        public WJXServices(IConfigurationRoot configurationRoot)
        {
            _configurationRoot = configurationRoot;
        }
        public OutputResult<object> GetWJXSSO()
        {
            var config = _configurationRoot.GetSection("WJXConfig").Get<WJXSSOConfigModel>();
            var stamp = GetTimeStamp();
            string ssoUrl = string.Format(config.SSOUrl, config.AppId, config.APPkey, config.User, SignatureHelper.Sha1Signature(config.AppId +config.APPkey +config.User + stamp));
            return new OutputResult<object>(ssoUrl);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        string GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

    }
}
