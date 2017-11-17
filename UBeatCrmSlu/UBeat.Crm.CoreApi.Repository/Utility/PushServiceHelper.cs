using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace UBeat.Crm.CoreApi.Repository.Utility
{
    public class PushServiceHelper
    {
        public static string GetMD5HashToLower(String inputValue)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(inputValue));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "").ToLower();
            }
        }

        public static string GetMD5HashToUpper(String inputValue)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(inputValue));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "").ToUpper();
            }
        }


        public static string HttpGet(string baseUrl, string queryString, string host = null, Encoding encoding = null, int timeout = 60000)
        {
            try
            {
                encoding = encoding ?? Encoding.UTF8;
                if (!baseUrl.StartsWith("http://") && !baseUrl.StartsWith("https://"))
                    baseUrl = string.Format("http://{0}", baseUrl);
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(string.Format("{0}?{1}", baseUrl, queryString)));

                req.Method = "GET";
                req.ContinueTimeout = timeout;

                var rsp = (HttpWebResponse)req.GetResponseAsync().Result;
                var result = GetResponseAsString(rsp, encoding);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static T HttpGet<T>(string url, string data, string host = null, Encoding encoding = null)
        {
            try
            {
                encoding = encoding ?? Encoding.UTF8;
                var result = HttpGet(url, data, host, encoding);
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                throw ex;
                //return default(T);
            }
        }

        public static string HttpPost(string url, string data, string host = null, Encoding encoding = null, int timeout = 60000)
        {
            try
            {
                encoding = encoding ?? Encoding.UTF8;
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    url = string.Format("http://{0}", url);
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(url));

                req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                req.Method = "POST";
                req.Accept = "text/xml,text/javascript";
                req.ContinueTimeout = timeout;

                byte[] postData = encoding.GetBytes(data);
                Stream reqStream = req.GetRequestStreamAsync().Result;
                reqStream.Write(postData, 0, postData.Length);
                reqStream.Dispose();

                var rsp = (HttpWebResponse)req.GetResponseAsync().Result;
                var result = GetResponseAsString(rsp, encoding);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static T HttpPost<T>(string url, string data, string host = null, Encoding encoding = null)
        {
            try
            {
                encoding = encoding ?? Encoding.UTF8;
                var result = HttpPost(url, data, host, encoding);
                return JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                throw ex;
                //return default(T);
            }
        }
        public static string BuildQuery(IDictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return null;
            }

            StringBuilder query = new StringBuilder();
            bool hasParam = false;

            foreach (KeyValuePair<string, object> kv in parameters)
            {
                string name = kv.Key;
                string value = kv.Value == null ? string.Empty : kv.Value.ToString();
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        query.Append("&");
                    }

                    query.Append(name);
                    query.Append("=");
                    query.Append(WebUtility.UrlEncode(value));
                    hasParam = true;
                }
            }

            return query.ToString();
        }
        public static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);
                return WebUtility.UrlDecode(reader.ReadToEnd());
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Dispose();
                if (stream != null) stream.Dispose();
                if (rsp != null) rsp.Dispose();
            }
        }
    }
}
