using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using NLog;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class HttpLib
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(HttpLib).FullName);

        public static string Post(string url, string data)
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(url));
                req.ContentType = "application/json";
                req.Method = "POST";

                byte[] postData = Encoding.UTF8.GetBytes(data);
                Stream reqStream = req.GetRequestStreamAsync().Result;
                reqStream.Write(postData, 0, postData.Length);
                reqStream.Dispose();

                var rsp = (HttpWebResponse)req.GetResponseAsync().Result;
                var result = GetResponseAsString(rsp, Encoding.UTF8);
                watch.Stop();
                Logger.Error("推送返回内容:{0} 耗时:{1}", result, watch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "推送消息异常");
                return string.Empty;
            }
        }
        public static string Post(string url, string data, WebHeaderCollection headers)
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(url));
                req.ContentType = "application/json";
                req.Method = "POST";
                foreach (var t in headers)
                {
                    req.Headers.Add(t.ToString(), headers[t.ToString()]);
                }
                byte[] postData = Encoding.UTF8.GetBytes(data);
                Stream reqStream = req.GetRequestStreamAsync().Result;
                reqStream.Write(postData, 0, postData.Length);
                reqStream.Dispose();

                var rsp = (HttpWebResponse)req.GetResponseAsync().Result;
                var result = GetResponseAsString(rsp, Encoding.UTF8);
                watch.Stop();
                Logger.Error("推送返回内容:{0} 耗时:{1}", result, watch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "推送消息异常");
                return string.Empty;
            }
        }

        public static string Post(string url, byte[] postData, string requestHost, string contentType)
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(url));
                req.ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/json" : contentType;
                req.Method = "POST";
                if (!string.IsNullOrWhiteSpace(requestHost))
                {
                    req.Headers["Host"] = requestHost;
                }

                Stream reqStream = req.GetRequestStreamAsync().Result;
                reqStream.Write(postData, 0, postData.Length);
                reqStream.Dispose();

                var rsp = (HttpWebResponse)req.GetResponseAsync().Result;
                var result = GetResponseAsString(rsp, Encoding.UTF8);
                watch.Stop();
                Logger.Error("推送返回内容:{0} 耗时:{1}", result, watch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "推送消息异常");
                return string.Empty;
            }
        }

        public static string Post(string url, byte[] postData, string authOfHeader)
        {
            try
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                HttpWebRequest req = WebRequest.CreateHttp(new Uri(url));

                req.ContentType = "application/json;charset=utf-8;";
                req.Method = "POST";
                req.Headers["Authorization"] = authOfHeader;
                req.Headers["Accept"] = "application/json;";

                Stream reqStream = req.GetRequestStreamAsync().Result;
                reqStream.Write(postData, 0, postData.Length);
                reqStream.Dispose();

                var rsp = (HttpWebResponse)req.GetResponseAsync().Result;
                var result = GetResponseAsString(rsp, Encoding.UTF8);
                watch.Stop();
                Logger.Error("推送返回内容:{0} 耗时:{1}", result, watch.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "推送消息异常");
                return string.Empty;
            }
        }

        public static string Get(string url)
        {
            try
            {
                //Stopwatch watch = new Stopwatch();
                //watch.Start();
                //HttpWebRequest req = WebRequest.CreateHttp(new Uri(url));

                //req.ContentType = "application/json;charset=utf-8;";
                //req.Method = "GET";
                //req.Headers["Accept"] = "application/json;";

                //var rsp = (HttpWebResponse)req.GetResponseAsync().Result;
                //var result = GetResponseAsString(rsp, Encoding.UTF8);
                //watch.Stop();
                //Logger.Error("获取实时定位信息:{0} 耗时:{1}", result, watch.ElapsedMilliseconds);
                //return result;

                using (HttpClient client = new HttpClient())
                {
                    Byte[] resultBytes = client.GetByteArrayAsync(url).Result;
                    return Encoding.UTF8.GetString(resultBytes);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "GET请求异常");
                return string.Empty;
            }
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
