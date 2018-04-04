using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using NLog;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.Track;

namespace UBeat.Crm.CoreApi.Services.Utility
{
    public class BaiduTrackHelper
    {
        private static readonly string AK;
        private static readonly string SK;
        private static readonly string ServiceId;
        private static readonly int PageSize;

        private static System.Net.Http.HttpClient httpClient = null;

        static BaiduTrackHelper()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("BaiduTrackConfig");
            AK = config.GetValue<string>("AK");
            SK = config.GetValue<string>("SK");
            ServiceId = config.GetValue<string>("ServiceId");
            PageSize = config.GetValue<int>("PageSize");
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

        public static List<LocationDetailInfo> LocationSearch(string url, IDictionary<string, string> querystring_arrays, LocationSearchInfo searchInfo)
        {
            var _logger = LogManager.GetLogger(typeof(BaiduTrackHelper).FullName);
            querystring_arrays.Add("sortby", "loc_time:desc");
            querystring_arrays.Add("coord_type_output", "bd09ll");//该字段在国外无效，国外均返回 wgs84坐标
            querystring_arrays.Add("page_index", searchInfo.PageIndex.ToString());
            querystring_arrays.Add("page_size", searchInfo.PageSize.ToString());
            querystring_arrays.Add("ak", "5WbRjhj5kLKZLQZdF78vprTVjpNB4C5H");
            querystring_arrays.Add("service_id", "157572");
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

            _logger.Error("searchLocation.url:" + url);
            var response = HttpLib.Get(url);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            _logger.Error("searchLocation.result:" + response);
           
            if (searchResult.ContainsKey("status") && searchResult["status"].ToString() == "0")
            {
                return ((Newtonsoft.Json.Linq.JArray)searchResult["entities"]).ToObject<List<LocationDetailInfo>>();
            }
            return new List<LocationDetailInfo>();
        }

        public static string SearchAddressByLocationPoint(double latitude, double longitude)
        {
            string address = string.Empty;
            string url = "http://api.map.baidu.com/geocoder/v2/?callback=renderReverse&location={0},{1}&output=json&pois=1&ak={2}";
            url = string.Format(url, latitude, longitude, AK);
            var response = HttpLib.Get(url);
            var responseData = response.Substring(response.IndexOf("{"), response.LastIndexOf("}") - response.IndexOf("{") + 1);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseData);
            if(result["status"].ToString() == "0")
            {
                address = ((Newtonsoft.Json.Linq.JObject)result["result"])["formatted_address"].ToString();
            }
            return address;
        }

        public static TrackData GetTrackData(string url, IDictionary<string, string> querystring_arrays)
        {
            var _logger = LogManager.GetLogger(typeof(BaiduTrackHelper).FullName);
            querystring_arrays.Add("ak", "5WbRjhj5kLKZLQZdF78vprTVjpNB4C5H");
            querystring_arrays.Add("service_id", "157572");
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

            _logger.Error("GetTrackData.url:" + url);
            var response = HttpLib.Get(url);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject<TrackData>(response);
            _logger.Error("GetTrackData.result:" + response);
            if (searchResult != null && searchResult.status == 0)
            {
                return searchResult;
            }
            return new TrackData();
        }

        public static void TraceFileDownSave(string url, string savePath)
        {
            if (File.Exists(savePath))
            {
                return;
            }
            if (httpClient == null)
            {
                httpClient = new System.Net.Http.HttpClient();
            }
            var t = httpClient.GetByteArrayAsync(url);
            t.Wait();
            Stream responseStream = new MemoryStream(t.Result);
            Stream stream = new FileStream(savePath, FileMode.Create);
            byte[] bArr = new byte[1024];
            int size = responseStream.Read(bArr, 0, bArr.Length);
            while (size > 0)
            {
                stream.Write(bArr, 0, size);
                size = responseStream.Read(bArr, 0, bArr.Length);
            }
            stream.Close();
            responseStream.Close();
        }
    }
}
