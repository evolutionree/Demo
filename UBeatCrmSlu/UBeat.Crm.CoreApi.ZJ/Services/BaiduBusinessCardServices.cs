using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class BaiduBusinessCardServices : BasicBaseServices
    {
        private static String token;
        private static int tokenExpires = 30;
        private static DateTime tokenTime;

        private readonly IConfiguration _configurationRoot;

        public BaiduBusinessCardServices(IConfigurationRoot configurationRoot)
        {
            _configurationRoot = configurationRoot;
        }

        public string GetAccessToken()
        {
            if (!string.IsNullOrEmpty(token) && tokenTime != null && (DateTime.Now - tokenTime).TotalSeconds < (tokenExpires - 10))
            {
                return token;
            }
            else
            {
                var config = _configurationRoot.GetSection("ZJBusinessCardConfig");
                string clientId = config.GetValue<string>("clientId");
                string clientSecret = config.GetValue<string>("clientSecret");

                string url = "https://aip.baidubce.com/oauth/2.0/token?grant_type=client_credentials&client_id=" + clientId + "&client_secret=" + clientSecret;
                System.Net.WebHeaderCollection headers = new WebHeaderCollection();
                var result = HttpLib.Get(url, headers);
                var jsonData = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.ToString());
                token = jsonData["access_token"].ToString();
                tokenExpires = int.Parse(jsonData["expires_in"].ToString());
                tokenTime = DateTime.Now;

                return token;
            }
        }

        public string GetBusinessCardInfo(byte[] imgData)
        {
            GetAccessToken();
            string url = "https://aip.baidubce.com/rest/2.0/ocr/v1/business_card?access_token=" + token;
            Encoding encoding = Encoding.Default;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "post";
            request.KeepAlive = true;
            string imgBaser64Str = Convert.ToBase64String(imgData);
            string str = "image=" + HttpUtility.UrlEncode(imgBaser64Str);
            byte[] buffer = encoding.GetBytes(str);
            request.ContentLength = buffer.Length;
            request.GetRequestStream().Write(buffer, 0, buffer.Length);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
            string result = reader.ReadToEnd();
            return result;
        }

    }
}
