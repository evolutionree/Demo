using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class DJCloudHelper
    {
        private static readonly string AccountSid;
        private static readonly string AuthToken;
        private static readonly string RestURL;
        private static readonly string APPID;

        static DJCloudHelper()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("DJCloudConfig");
            AccountSid = config.GetValue<string>("AccountSid");
            AuthToken = config.GetValue<string>("AuthToken");
            RestURL = config.GetValue<string>("RestURL");
            APPID = config.GetValue<string>("APPID");
        }

        public static string Call(byte[] body)
        {
            string authOfHeader = EncodeBase64(AccountSid + ":" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            string plainText = AccountSid + AuthToken + DateTime.Now.ToString("yyyyMMddHHmmss");
            string sig = SecurityHash.GetHash1(plainText);
            //构建POST请求
            var url = string.Format(RestURL, sig);
            var response = HttpLib.Post(url, body, authOfHeader);
            return response;
        }

        public static string EncodeBase64(string code)
        {
            string encode = "";
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(code);
            try
            {
                encode = Convert.ToBase64String(bytes);
            }
            catch
            {
                encode = code;
            }
            return encode;
        }

    }
}
