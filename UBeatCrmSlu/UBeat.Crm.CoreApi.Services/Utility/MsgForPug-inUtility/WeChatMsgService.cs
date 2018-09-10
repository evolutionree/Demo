using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Repository.Utility.Cache;
using UBeat.Crm.CoreApi.Core.Utility;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public class WeChatMsgService : IMSGService
    {
        private static readonly string getTokenURL = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}";
        private static readonly string snedMsgURL = "https://qyapi.weixin.qq.com/cgi-bin/message/send?access_token={0}";
        private string _token;

        private static readonly string AgentId;
        private static readonly string Secret;
        private static readonly string CorpId;

        private static readonly Dictionary<string, object> commonRequest;

        static WeChatMsgService()
        {
            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("WeChatConfig");
            AgentId = config.GetValue<string>("AgentId");
            Secret = config.GetValue<string>("Secret");
            CorpId = config.GetValue<string>("CorpId");

            commonRequest = new Dictionary<string, object>();
            commonRequest.Add("toparty", "");
            commonRequest.Add("totag", "");
            commonRequest.Add("safe", 0);
            commonRequest.Add("agentid", AgentId);
        }

        public string getToken()
        {
            string realUrl = string.Format(getTokenURL, CorpId, Secret);
            var response = HttpLib.Get(realUrl);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
            string errorCode = searchResult["errcode"].ToString();
            if (errorCode == "0")
            {
                string token = searchResult["access_token"].ToString();
                _token = token;
                return token;
            }
            return "";

        }

        public void updateToken(string token)
        {
            _token = token;
        }

        public bool sendTextMessage(Pug_inMsg msg)
        {
            /*
             {
   "touser" : "ZhaiQiuqiu",
   "toparty" : "",
   "totag" : "",
   "msgtype" : "text",
   "agentid" : 1000002,
   "text" : {
       "content" : "你的快递已到，请携带工卡前往邮件中心领取。\n出发前可查看<a href=\"http://work.weixin.qq.com\">邮件中心视频实况</a>，聪明避开排队。"
   },
   "safe":0
}
             */
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("msgtype", "text");
            param.Add("touser", string.Join("|", msg.recevier));
            param.Add("text", new { content  = msg.content});
            commonRequest.ToList().ForEach(x => param.Add(x.Key, x.Value));

            string realUrl = string.Format(snedMsgURL, _token);
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(param);
            var response = HttpLib.Post(realUrl, body);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
            return true;
        }

        public bool sendTextCardMessage(Pug_inMsg msg)
        {
            /*
             {
   "touser" : "HuangGuoChen",
   "toparty" : "",
   "totag" : "",
   "msgtype" : "textcard",
   "agentid" : 1000002,
   "textcard" : {
            "title" : "领奖通知",
            "description" : "<div class=\"gray\">2016年9月26日</div> <div class=\"normal\">恭喜你抽中iPhone 7一台，领奖码：xxxx</div><div class=\"highlight\">请于2016年10月10日前联系行政同事领取</div>",
            "url" : "URL",
            "btntxt":"更多"
   }
}
             */
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("msgtype", "textcard");
            param.Add("touser", string.Join("|", msg.recevier));
            param.Add("textcard", new { title = msg.title, url = msg.responseUrl ?? "URL", description = msg.content});
            commonRequest.ToList().ForEach(x => param.Add(x.Key, x.Value));

            string realUrl = string.Format(snedMsgURL, _token);
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(param);
            var response = HttpLib.Post(realUrl, body);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
            return true;
        }

        public bool sendPictureMessage(Pug_inMsg msg)
        {
            string realUrl = string.Format(snedMsgURL, _token);
            return true;
        }

        public bool sendPicTextMessage(Pug_inMsg msg)
        {
            string realUrl = string.Format(snedMsgURL, _token);
            return true;
        }

        private string ConvertToWebUser(List<int> userids)
        {
            //todo
            return "";
        }
    }
}
