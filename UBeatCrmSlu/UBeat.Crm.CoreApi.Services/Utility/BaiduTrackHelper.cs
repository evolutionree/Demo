using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class BaiduTrackHelper
    {
        private static readonly string AK;
        private static readonly string SK;
        private static readonly string ServiceId;

        static BaiduTrackHelper()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("BaiduTrackConfig");
            AK = config.GetValue<string>("AK");
            SK = config.GetValue<string>("SK");
            ServiceId = config.GetValue<string>("ServiceId");
        }

        private static string MD5(string password)
        {
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(password);
            try
            {
                System.Security.Cryptography.MD5CryptoServiceProvider cryptHandler;
                cryptHandler = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] hash = cryptHandler.ComputeHash(textBytes);
                string ret = "";
                foreach (byte a in hash)
                {
                    ret += a.ToString("x");
                }
                return ret;
            }
            catch
            {
                throw;
            }
        }

        private static string UrlEncode(string str)
        {
            str = System.Web.HttpUtility.UrlEncode(str, Encoding.UTF8);
            byte[] buf = Encoding.ASCII.GetBytes(str);//等同于Encoding.ASCII.GetBytes(str)
            for (int i = 0; i < buf.Length; i++)
                if (buf[i] == '%')
                {
                    if (buf[i + 1] >= 'a') buf[i + 1] -= 32;
                    if (buf[i + 2] >= 'a') buf[i + 2] -= 32;
                    i += 2;
                }
            return Encoding.ASCII.GetString(buf);//同上，等同于Encoding.ASCII.GetString(buf)
        }

        private static string HttpBuildQuery(IDictionary<string, string> querystring_arrays)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in querystring_arrays)
            {
                sb.Append(UrlEncode(item.Key));
                sb.Append("=");
                sb.Append(UrlEncode(item.Value));
                sb.Append("&");
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        public static string CaculateAKSN(string url, IDictionary<string, string> querystring_arrays)
        {
            var queryString = HttpBuildQuery(querystring_arrays);

            var str = UrlEncode(url + queryString + SK);

            return MD5(str);
        }

        public static string LocationSearch(string url, IDictionary<string, string> querystring_arrays)
        {
            querystring_arrays.Add("ak", AK);
            querystring_arrays.Add("service_id", ServiceId);
            StringBuilder sb = new StringBuilder();
            foreach (var item in querystring_arrays)
            {
                sb.Append(item.Key);
                sb.Append("=");
                sb.Append(item.Value);
                sb.Append("&");
            }
            sb.Remove(sb.Length - 1, 1);
            url = string.Format("{0}?{1}", url, sb.ToString());

            var response = HttpLib.Get(url);

            return response;
        }
    }
}
