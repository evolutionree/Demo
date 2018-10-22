using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using NLog;

namespace UBeat.Crm.CoreApi.DingTalk.Utils
{
    public class DingTalkHttpUtils
    {
        public static string HttpGet(string apiStr)
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiStr);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                return reader.ReadToEnd().ToString();
            }

        }


        public static string HttpPost(string serverUri, object postData)
        {

            string strData = JsonConvert.SerializeObject(postData);
            var url = new Uri(serverUri);

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";

            //TODO:use asyn method
            if (!string.IsNullOrEmpty(strData))
            {
                using (var streamWriter = new StreamWriter(request.GetRequestStreamAsync().GetAwaiter().GetResult()))
                {
                    streamWriter.Write(postData);
                    streamWriter.Flush();
                    //streamWriter.Close();
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


        private static ILogger logger = LogManager.GetCurrentClassLogger();
        public const string NetworkConnectError = "NETWORK_CONNECT_ERROR";

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




    }
}

