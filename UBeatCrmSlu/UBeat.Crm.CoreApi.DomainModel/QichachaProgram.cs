using System;
using System.Net;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace UBeat.Crm.CoreApi.DomainModel
{
    public class QichachaProgram
    {
        private static readonly UTF8Encoding UTF8_ENCODING = new UTF8Encoding();
        

        /// <summary>
        /// Http Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headerValue"></param>
        /// <returns></returns>
        public static String httpGet(string url, String[] headerValue)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            // set header
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("Token", headerValue[0]);
            headers.Add("Timespan", headerValue[1]);
            request.UserAgent = null;
            request.Headers = headers;
            request.Method = "GET";

            // response deal
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var httpStatusCode = (int)response.StatusCode;
            Console.WriteLine("返回码为 {0}", httpStatusCode);
            if (httpStatusCode == 200)
            {
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                return retString;
            }
            else
            {   // todo 可以通过返回码判断处理
                Console.WriteLine("未返回数据 {0}", httpStatusCode);
                throw new Exception("no data response");
            }

        }

        /// <summary>
        /// 设置请求Header信息
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="secertKey"></param>
        /// <returns></returns>
        public static String[] getHeaderVals(String appkey, String secertKey)
        {
            String[] values = new string[2];
            var timeSpan = GetTimeStamp();
            var token = MD5Encrypt(appkey + timeSpan + secertKey, UTF8_ENCODING);
            values[0] = token.ToUpper();
            values[1] = timeSpan;
            return values;

        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">需要加密的字符串</param>
        /// <param name="encode">字符的编码</param>
        /// <returns></returns>
        public static string MD5Encrypt(string input, Encoding encode)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(encode.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            String startTime = ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString(); // 当地时区
            return Convert.ToString(startTime);
        }
    }
}

