using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using System.Security.Cryptography;

namespace UBeat.Crm.CoreApi.ZGQY.Utility
{
    public class CallAPIHelper
    {
        private static ILogger logger = LogManager.GetCurrentClassLogger();
        public const string NetworkConnectError = "NETWORK_CONNECT_ERROR";
        private string sapUrl;
        private static readonly UTF8Encoding UTF8_ENCODING = new UTF8Encoding();

        private static IConfigurationRoot _configRoot;

        public static async Task<string> PostJson(string serverUri, object postData, HttpClientHandler webRequestHandler, Dictionary<string, string> headDic = null)
        {
            try
            {
                var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(postData));

                logger.Info("发送数据:" + stringPayload);

                var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

                using (var httpClient = webRequestHandler == null ? new HttpClient() : new HttpClient(webRequestHandler))
                {
                    if (headDic != null)
                    {
                        foreach (var item in headDic.Keys)
                        {
                            httpClient.DefaultRequestHeaders.Add(item, headDic[item]);
                        }
                    }

                    var httpResponse = await httpClient.PostAsync(serverUri, httpContent);

                    if (httpResponse.Content != null)
                    {
                        var responseContent = await httpResponse.Content.ReadAsStringAsync();
                        return responseContent;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is WebException || ex is HttpRequestException)
                {
                    return NetworkConnectError;
                }
                logger.Error(ex);
            }

            return string.Empty;
        }

        public static string ApiSFPostData(string Url, string xml, string verifyCode)
        {
            string postData = string.Format("xml={0}&verifyCode={1}", xml, verifyCode);

            //请求
            WebRequest request = (HttpWebRequest)WebRequest.Create(Url);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
            request.ContentLength = Encoding.UTF8.GetByteCount(postData);
            byte[] postByte = Encoding.UTF8.GetBytes(postData);
            Stream reqStream = request.GetRequestStream();
            reqStream.Write(postByte, 0, postByte.Length);
            reqStream.Close();

            //读取
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }


        public static string ApiPostData(object postData, Dictionary<string, string> headDic = null, string contentType = "application/json")
        {
            return ApiPostData(null, postData, headDic, contentType);
        }
       public static string ApiPostData(string serverUri, object postData, Dictionary<string, string> headDic = null, string contentType = "application/json")
        {
            if (_configRoot == null) {
                _configRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            }
            IConfigurationSection config = _configRoot.GetSection("SapConfig");
            if (string.IsNullOrEmpty(serverUri)) {
                serverUri= config.GetValue<string>("SapUrl");
            }
            string strData = JsonConvert.SerializeObject(postData);
            var url = new Uri(serverUri);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            //超时时间设置
            request.Timeout = config.GetValue<int>("Timeout"); ;
            request.ReadWriteTimeout = config.GetValue<int>("ReadWriteTimeout"); ;
            request.ContinueTimeout = config.GetValue<int>("ContinueTimeout"); ;
            request.KeepAlive = true;
            if (headDic != null)
            {
                foreach (var item in headDic.Keys)
                {
                    request.Headers.Add(item, headDic[item]);
                }
            }

            if (!string.IsNullOrEmpty(strData))
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStreamAsync().GetAwaiter().GetResult()))
                {
                    streamWriter.Write(strData);
                    streamWriter.Flush();
                }
            }

            var response = (HttpWebResponse)request.GetResponseAsync().GetAwaiter().GetResult();
            var data = string.Empty;
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                }
            }

            return data;
        }


        public static string ApiGetData(string serverUri)
        {

            // string strData = JsonConvert.SerializeObject(postData);
            var url = new Uri(serverUri);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";

            //TODO:use asyn method
            /*  if (!string.IsNullOrEmpty(strData))
              {
                  using (var streamWriter = new StreamWriter(request.GetRequestStreamAsync().GetAwaiter().GetResult()))
                  {
                      streamWriter.Write(postData);
                      streamWriter.Flush();
                      //streamWriter.Close();
                  }
              }*/

            var response = (HttpWebResponse)request.GetResponseAsync().GetAwaiter().GetResult();
            var data = string.Empty;
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                }
            }

            return data;
        }


        public static string MessgaeSendPostData(string serverUri, object postData, Dictionary<string, string> headDic = null, string contentType = "application/json")
        {
            if (_configRoot == null)
            {
                _configRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            }
            IConfigurationSection config = _configRoot.GetSection("MessageSendConfig");
            if (string.IsNullOrEmpty(serverUri))
            {
                serverUri = config.GetValue<string>("Url");
            }
            string strData = JsonConvert.SerializeObject(postData);
            //string strtemp = "{\"batchName\":\"CRM审批提醒\",\"items\":[{\"to\":\"15177775716\",\"content\":\"测试短信\"}],\"msgType\":\"sms\",\"bizType\":\"100\"}";
            //string strtemp = @"{"batchName":"CRM审批提醒","items":"[{"to":"15177775716","content":"测试短信"}]","msgType":"sms","bizType":"100"}";
            var url = new Uri(serverUri);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = contentType;
            //超时时间设置
            request.Timeout = config.GetValue<int>("Timeout"); 
            request.ReadWriteTimeout = config.GetValue<int>("ReadWriteTimeout"); 
            request.ContinueTimeout = config.GetValue<int>("ContinueTimeout"); 
            request.KeepAlive = true;

            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Accept", "application/json");
            headers.Add("Content-Type", "application/json;charset=utf-8");
            headers.Add("Authorization", "cWN5anlAcWN5ank6MGYyMmU2Y2FkYTgyOTg4MWFlOTFiODRlMGQ4MTg4NDE=");
            //config.GetValue<string>("Account") + ":" + sBuilder.ToString();
            //headDic.Add("Content-Type", "application/json;charset=utf-8");
            //headDic.Add("Accept", "application/json");
            //headDic.Add("Authorization", "cWN5anlAcWN5ank6MGYyMmU2Y2FkYTgyOTg4MWFlOTFiODRlMGQ4MTg4NDE=");
            /*if (headDic != null)
            {
                foreach (var item in headDic.Keys)
                {
                    request.Headers.Add(item, headDic[item]);
                }
            }*/
            request.Headers = headers;
            if (!string.IsNullOrEmpty(strData))
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStreamAsync().GetAwaiter().GetResult()))
                {
                    streamWriter.Write(strData);
                    streamWriter.Flush();
                }
            }

            var response = (HttpWebResponse)request.GetResponseAsync().GetAwaiter().GetResult();
            var data = string.Empty;
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream != null)
                {
                    using (var streamReader = new StreamReader(responseStream))
                    {
                        data = streamReader.ReadToEnd();
                    }
                }
            }

            return data;
        }

    }
}
