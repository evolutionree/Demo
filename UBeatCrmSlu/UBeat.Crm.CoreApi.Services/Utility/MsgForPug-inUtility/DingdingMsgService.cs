using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Services.Services;
using static UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility.EncodingStrOrByte;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public class DingdingMsgService : IMSGService
    {
        public const string NetworkConnectError = "NETWORK_CONNECT_ERROR";

        public static async Task<string> PostJson(string serverUri, object postData, HttpClientHandler webRequestHandler, Dictionary<string, string> headDic = null)
        {
            try
            {
                var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(postData));

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

            }

            return string.Empty;
        }

        public static async Task<string> GetJson(string serverUri, object postData, HttpClientHandler webRequestHandler, Dictionary<string, string> headDic = null)
        {
            try
            {
                var stringPayload = await Task.Run(() => JsonConvert.SerializeObject(postData));

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

                    var httpResponse = await httpClient.GetAsync(serverUri);

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

            }

            return string.Empty;
        }

        private static readonly String ACCESS_TOKEN_URL = "https://oapi.dingtalk.com/gettoken?corpid={0}&corpsecret={1}";
        private static readonly String SEND_MSG_URL = "https://oapi.dingtalk.com/message/send?access_token={0}";
        private static readonly String SEND_PIC_MSG_URL = "https://oapi.dingtalk.com/media/upload?access_token={0}&type=image";
        private static readonly string AGENTId;
        private static readonly string CORPSECRET;
        private static readonly string CORPID;
        private string _token;
        static DingdingMsgService()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("DingdingConfig");
            AGENTId = config.GetValue<string>("AgentId");
            CORPSECRET = config.GetValue<string>("CorpSecret");
            CORPID = config.GetValue<string>("CorpId");

            ACCESS_TOKEN_URL = String.Format(ACCESS_TOKEN_URL, CORPID, CORPSECRET);
            commonRequest = new Dictionary<string, object>();
            commonRequest.Add("toparty", "");
            commonRequest.Add("agentid", AGENTId);
        }
        private static readonly Dictionary<string, object> commonRequest;
        private static FileServices _fileServices;
        public DingdingMsgService()
        {
            _fileServices = new FileServices();
        }

        public string getToken()
        {
            try
            {
                Task<String> taskResult = GetJson(ACCESS_TOKEN_URL, null, null);
                taskResult.Wait();
                var result = taskResult.Result;
                var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
                string errorCode = searchResult["errcode"].ToString();
                if (errorCode == "0")
                {
                    string token = searchResult["access_token"].ToString();
                    _token = token;
                    return token;
                }
                return "";
            }
            catch (Exception ex)
            {
                throw new Exception("获取Token异常");
            }
        }

        public void updateToken(string token)
        {
            _token = token;
        }

        public bool sendTextMessage(Pug_inMsg msg)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("msgtype", "text");
            param.Add("touser", string.Join("|", msg.recevier));
            param.Add("text", new { content = msg.content });
            commonRequest.ToList().ForEach(x => param.Add(x.Key, x.Value));

            string realUrl = string.Format(SEND_MSG_URL, _token);
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(param);
            var response = HttpLib.Post(realUrl, body);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
            return true;
        }

        public bool sendTextCardMessage(Pug_inMsg msg)
        {
            /*
             
             {
    "msgtype": "action_card",
    "action_card": {
        "title": "是透出到会话列表和通知的文案",
        "markdown": "支持markdown格式的正文内容",
        "single_title": "查看详情",
        "single_url": "https://open.dingtalk.com"
    }
}
             */
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("msgtype", "action_card");
            param.Add("touser", string.Join("|", msg.recevier));
            param.Add("action_card", new { title = msg.title, markdown = msg.markdown ?? "", single_title = msg.single_title ?? "查看详情", single_url = msg.single_url ?? "URL", content = msg.content });
            commonRequest.ToList().ForEach(x => param.Add(x.Key, x.Value));

            string realUrl = string.Format(SEND_MSG_URL, _token);
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(param);
            var response = HttpLib.Post(realUrl, body);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
            return true;
        }

        public bool sendPictureMessage(Pug_inMsg msg)
        {
            String url = String.Format(SEND_PIC_MSG_URL, _token);
            String result = PostFile(url, msg.fileid??"d96e240d-7316-43a8-b7f9-473bc6370fd2");
            var imgResult = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);

            /*
             {
    "msgtype": "image",
    "image": {
        "media_id": "MEDIA_ID"
    }
}
             */
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("msgtype", "image");
            param.Add("touser", string.Join("|", msg.recevier));
            param.Add("image", new { media_id = imgResult.media_id });
            commonRequest.ToList().ForEach(x => param.Add(x.Key, x.Value));

            string realUrl = string.Format(SEND_MSG_URL, _token);
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(param);
            var response = HttpLib.Post(realUrl, body);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
            return true;
        }

        public bool sendPicTextMessage(Pug_inMsg msg)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("msgtype", "markdown");
            param.Add("touser", string.Join("|", msg.recevier));
            param.Add("markdown", new { title = msg.title, text=msg.content});
            //msg.imgurl?? "#  \n## 标题2  \n* 列表1 \n![alt 啊](http://news.online.sh.cn/news/gb/content/attachement/jpg/site1/20180926/IMGf48e3894467148793136079.jpg)"
            commonRequest.ToList().ForEach(x => param.Add(x.Key, x.Value));

            string realUrl = string.Format(SEND_MSG_URL, _token);
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(param);
            var response = HttpLib.Post(realUrl, body);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
            return true;
        }

        public static string PostFile(string url, string fileId)
        {
            var result = string.Empty;
            var request = (HttpWebRequest)WebRequest.Create(url);
            var boundary = "----------" + DateTime.Now.Ticks.ToString("x");
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            using (Stream requestStream = request.GetRequestStream())
            {
                byte[] boundarybytes = Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
                byte[] trailer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "–-\r\n");
                //        var filename = Path.GetFileName(fileWithPath);
                var fileListData = _fileServices.GetFileListData(string.Empty, new List<string>() { fileId });
                using (MemoryStream ms = new MemoryStream())
                {
                    var file = fileListData.FirstOrDefault();
                    if (file == null) return "";
                    ms.Write(file.Data, 0, file.Data.Length);
                    requestStream.Write(boundarybytes, 0, boundarybytes.Length);
                    var header = $"Content-Disposition:form-data;name=\"media\";filename=\"{file.FileName}\"\r\nfilelength=\"{file.Data.Length}\"\r\nContent-Type:application/octet-stream\r\n\r\n";
                    byte[] postHeaderBytes = Encoding.UTF8.GetBytes(header.ToString());
                    requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                    ms.Close();
                    requestStream.Write(file.Data, 0, file.Data.Length);
                    requestStream.Write(trailer, 0, trailer.Length);
                }
            }
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            using (MemoryStream stmMemory = new MemoryStream())
            {
                byte[] buffer = new byte[64 * 1024];
                int i;
                while ((i = responseStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stmMemory.Write(buffer, 0, i);
                }
                byte[] arraryByte = stmMemory.ToArray();
                stmMemory.Close();
                result = EncodingStrOrByte.GetString(arraryByte, EncodingType.UTF8);
            }
            return result;
        }


    }




    /// <summary>
    /// 处理编码字符串或字符串
    /// </summary>
    public static class EncodingStrOrByte
    {
        /// <summary>
        /// 编码方式
        /// </summary>
        public enum EncodingType { UTF7, UTF8, UTF32, Unicode, BigEndianUnicode, ASCII, GB2312 };
        /// <summary>
        /// 处理指定编码的字符串，转换字节数组
        /// </summary>
        /// <param name="str"></param>
        /// <param name="encodingType"></param>
        /// <returns></returns>
        public static byte[] GetBytes(string str, EncodingType encodingType)
        {
            byte[] bytes = null;
            switch (encodingType)
            {
                //将要加密的字符串转换为指定编码的字节数组
                case EncodingType.UTF7:
                    bytes = Encoding.UTF7.GetBytes(str);
                    break;
                case EncodingType.UTF8:
                    bytes = Encoding.UTF8.GetBytes(str);
                    break;
                case EncodingType.UTF32:
                    bytes = Encoding.UTF32.GetBytes(str);
                    break;
                case EncodingType.Unicode:
                    bytes = Encoding.Unicode.GetBytes(str);
                    break;
                case EncodingType.BigEndianUnicode:
                    bytes = Encoding.BigEndianUnicode.GetBytes(str);
                    break;
                case EncodingType.ASCII:
                    bytes = Encoding.ASCII.GetBytes(str);
                    break;
                case EncodingType.GB2312:
                    bytes = Encoding.Default.GetBytes(str);
                    break;
            }
            return bytes;
        }

        /// <summary>
        /// 处理指定编码的字节数组，转换字符串
        /// </summary>
        /// <param name="myByte"></param>
        /// <param name="encodingType"></param>
        /// <returns></returns>
        public static string GetString(byte[] myByte, EncodingType encodingType)
        {
            string str = null;
            switch (encodingType)
            {
                //将要加密的字符串转换为指定编码的字节数组
                case EncodingType.UTF7:
                    str = Encoding.UTF7.GetString(myByte);
                    break;
                case EncodingType.UTF8:
                    str = Encoding.UTF8.GetString(myByte);
                    break;
                case EncodingType.UTF32:
                    str = Encoding.UTF32.GetString(myByte);
                    break;
                case EncodingType.Unicode:
                    str = Encoding.Unicode.GetString(myByte);
                    break;
                case EncodingType.BigEndianUnicode:
                    str = Encoding.BigEndianUnicode.GetString(myByte);
                    break;
                case EncodingType.ASCII:
                    str = Encoding.ASCII.GetString(myByte);
                    break;
                case EncodingType.GB2312:
                    str = Encoding.Default.GetString(myByte);
                    break;
            }
            return str;
        }
    }
}
