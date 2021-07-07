using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Models;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.ZGQY.Services;
using UBeat.Crm.CoreApi.ZGQY.WJXModel;

namespace UBeat.Crm.CoreApi.ZGQY.Controllers
{
    [Route("api/[controller]")]
    public class EnterpriseWeChatController : BaseController
    {
        private readonly EnterpriseWeChatServices _enterpriseWeChatServices;
        private readonly AccountServices _accountServices;
        private readonly CacheServices _cacheService;
        public EnterpriseWeChatController(EnterpriseWeChatServices enterpriseWeChatServices)
        {
            _cacheService = new CacheServices();
            _enterpriseWeChatServices = enterpriseWeChatServices;
            _accountServices = ServiceLocator.Current.GetInstance<AccountServices>();
        }
        [HttpGet]
        [Route("getssocode")]
        [AllowAnonymous]
        public void GetMessageSSOCode()
        {
            Stream stream = Request.Body;
            Byte[] byteData = new Byte[stream.Length];
            stream.Read(byteData, 0, (Int32)stream.Length);
            string urltype = Request.Query["urltype"];
            string userId = Request.Query["userid"];
            string username = Request.Query["username"];
            string code = Request.Query["code"];
            EnterpriseWeChatModel enterpriseWeChat = new EnterpriseWeChatModel();
            enterpriseWeChat.Code = code;
            enterpriseWeChat.Data = new Dictionary<string, object>();
            string action = Request.Query["action"];
            if (urltype == "1")
            {
                string caseid = Request.Query["caseid"];
                enterpriseWeChat.UrlType = UrlTypeEnum.WorkFlow;
                enterpriseWeChat.Data.Add("caseid", caseid);
                enterpriseWeChat.Data.Add("userid", userId);
                enterpriseWeChat.Data.Add("username", username);
            }
            else
            {
                if (urltype == "3")
                    enterpriseWeChat.UrlType = UrlTypeEnum.EntityDynamic;
                else if (urltype == "4")
                    enterpriseWeChat.UrlType = UrlTypeEnum.Daily;
                else if (urltype == "5")
                    enterpriseWeChat.UrlType = UrlTypeEnum.Weekly;
                else
                    enterpriseWeChat.UrlType = UrlTypeEnum.SmartReminder;
                string recid = Request.Query["recid"];
                string typeid = Request.Query["typeid"];
                string entityid = Request.Query["entityid"];
                enterpriseWeChat.Data.Add("recid", recid);
                enterpriseWeChat.Data.Add("entityid", entityid);
                enterpriseWeChat.Data.Add("typeid", typeid);
                enterpriseWeChat.Data.Add("userid", userId);
                enterpriseWeChat.Data.Add("username", username);

            }
            int userNumber;
            var result = _enterpriseWeChatServices.GetSSOCode(enterpriseWeChat, out userNumber);
			if (result.Status == 1)
			{
				HttpContext.Response.ContentType = "text/plain; charset=utf-8";
				HttpContext.Response.WriteAsync("您未开通CRM账号，请联系管理员");
				return;
			}
			var account = _enterpriseWeChatServices.GetAccountInfo(Convert.ToInt32(userNumber));
            var userData = account.DataBody as AccountUserInfo;
			if (userData == null)
			{
				HttpContext.Response.ContentType = "text/plain;chartset=utf-8";
				HttpContext.Response.WriteAsync("您未开通CRM账号，请联系管理员");
				return;
			}
			var header = GetAnalyseHeader();
            var deviceId = header.DeviceId;
            if (header.DeviceId.Equals("UnKnown"))
            {
                //如果web没有传deviceid字段，则取token作为设备id
                deviceId = result.DataBody.ToString();
            }
            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            var webSession = config.GetSection("WebSession");
            TimeSpan webexpiration = new TimeSpan(0, 20, 0);
            if (webSession != null)
            {
                var seconds = webSession.GetValue<int>("Expiration");
                webexpiration = new TimeSpan(0, 0, seconds);
            }
            var loginSession = CacheService.Repository.Get<LoginSessionModel>("WebLoginSession_" + userNumber);
            if (loginSession != null) ClearExpiredSession(loginSession);//清除已经过期的session
            if (loginSession != null && loginSession.Sessions.ContainsKey(deviceId))
            {
                loginSession.Sessions.Remove(deviceId);
                CacheService.Repository.Replace("WebLoginSession_" + userNumber, loginSession, loginSession.Expiration);
            }
            SetLoginSession("WebLoginSession_" + userNumber, result.DataBody.ToString(), deviceId, webexpiration, 0, header.SysMark, header.Device, true);
            var cookie = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(120),
                Domain = config.GetValue<string>("Domain"),
                Path = "/"
            };
            HttpContext.Response.Cookies.Append("token", result.DataBody.ToString(), cookie);
            HttpContext.Response.Cookies.Append("account", userData.AccountName, cookie);
            HttpContext.Response.Cookies.Append("accountid", userData.AccountId.ToString(), cookie);
            HttpContext.Response.Headers.Add("Authorization", "Bearer " + result.DataBody.ToString());
            var enterpriseWeChatType = config.GetSection("EnterpriseWeChat").Get<EnterpriseWeChatTypeModel>();
            string actuallyUrl = string.Empty;
            if (enterpriseWeChat.UrlType == UrlTypeEnum.WorkFlow)
            {
                actuallyUrl = string.Format(enterpriseWeChatType.Workflow_EnterpriseWeChat, enterpriseWeChat.Data["caseid"]);
            }
            else if (enterpriseWeChat.UrlType == UrlTypeEnum.EntityDynamic)
            {
                actuallyUrl = string.Format(enterpriseWeChatType.EntityDynamic_EnterpriseWeChat, enterpriseWeChat.Data["entityid"], enterpriseWeChat.Data["typeid"], enterpriseWeChat.Data["recid"]);
            }
            else if (enterpriseWeChat.UrlType == UrlTypeEnum.Daily)
            {
                actuallyUrl = string.Format(enterpriseWeChatType.Daily_EnterpriseWeChat, enterpriseWeChat.Data["entityid"], enterpriseWeChat.Data["recid"]);
            }
            else if (enterpriseWeChat.UrlType == UrlTypeEnum.Weekly)
            {
                actuallyUrl = string.Format(enterpriseWeChatType.Weekly_EnterpriseWeChat, enterpriseWeChat.Data["entityid"], enterpriseWeChat.Data["recid"]);
            }
            else
            {
                actuallyUrl = string.Format(enterpriseWeChatType.SmartReminder_EnterpriseWeChat, enterpriseWeChat.Data["entityid"], enterpriseWeChat.Data["typeid"], enterpriseWeChat.Data["recid"]);
            }

            HttpContext.Response.WriteAsync("<script type='text/javascript'>location.href='" + actuallyUrl + "';</script>");
        }
        static string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
        [HttpPost]
        [Route("getsignature")]
        [AllowAnonymous]
        public OutputResult<Object> GetSignature([FromBody]EnterpriseWeChatSignatureModel signature)
        {
            var timestamp = GetTimeStamp();
            var ranCode = GetRandomString(12, false, true, false, false, "CRM");
            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("WeChatConfig");
            string secret = config.GetValue<string>("Secret");
            string corpId = config.GetValue<string>("CorpId");
            string agentId = config.GetValue<string>("AgentId");
            string enterpriseWeChatJsApi = config.GetValue<string>("EnterpriseWeChatJsApi");
            string enterpriseWeChatToken = config.GetValue<string>("EnterpriseWeChatToken");

            var token = _cacheService.Repository.Get("qiyeweixintoken");
            if (token == null || String.IsNullOrEmpty(token.ToString()))
            {
                token = HttpLib.Get(string.Format(enterpriseWeChatToken, corpId, secret));
                var tokenObj = JObject.Parse(token.ToString());
                if (tokenObj["errcode"].ToString() != "0")
                {
                    return new OutputResult<object>("获取企业Token异常");
                }
                TimeSpan expiration = DateTime.UtcNow.AddSeconds(3400) - DateTime.UtcNow;
                _cacheService.Repository.Add("qiyeweixintoken", tokenObj["access_token"].ToString(), expiration);//, 
                token = tokenObj["access_token"].ToString();
            }

            var ticket = _cacheService.Repository.Get("qiyeweixinticket");
            if (ticket == null || String.IsNullOrEmpty(ticket.ToString()))
            {
                ticket = HttpLib.Get(string.Format(enterpriseWeChatJsApi, token.ToString()));
                //jsapi_ticket=sM4AOVdWfPE4DxkXGEs8VMCPGGVi4C3VM0P37wVUCFvkVAy_90u5h9nbSlYy3-Sl-HhTdfl2fzFy1AOcHKP7qg&noncestr=Wm3WZYTPz0wzccnW&timestamp=1414587457&url=http://mp.weixin.qq.com?params=value
                var ticketObj = JObject.Parse(ticket.ToString());
                if (ticketObj["errcode"].ToString() != "0")
                {
                    return new OutputResult<object>("获取企业Ticket异常");
                }

                TimeSpan expiration = DateTime.UtcNow.AddSeconds(3400) - DateTime.UtcNow;
                _cacheService.Repository.Add("qiyeweixinticket", ticketObj["ticket"].ToString(), expiration);//, 
                ticket = ticketObj["ticket"].ToString();
            }
            var str = SignatureHelper.Sha1Signature("jsapi_ticket=" + ticket.ToString() + "&noncestr=" + ranCode + "&timestamp=" + timestamp + "&url=" + signature.Url);
            return new OutputResult<object>(new { agentid = agentId, appid = corpId, jsapi_ticket = ticket.ToString(), timestamp = timestamp, noncestr = ranCode, signature = str });
        }
        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        string GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();
        }
        private void ClearExpiredSession(LoginSessionModel sessions)
        {
            List<string> ExpiredSession = new List<string>();
            foreach (string key in sessions.Sessions.Keys)
            {
                if (sessions.Sessions[key].Expiration < System.DateTime.Now) ExpiredSession.Add(key);
            }
            foreach (string key in ExpiredSession)
            {
                sessions.Sessions.Remove(key);
            }
        }
        private void SetLoginSession(string sessionKey, string token, string deviceId, TimeSpan expiration, long requestTimeStamp,
                    string SysMark, string DeviceType,
                    bool isMultipleLogin = true)
        {
            LoginSessionModel loginSession = null;
            try
            {
                loginSession = CacheService.Repository.Get<LoginSessionModel>(sessionKey);
                if (loginSession != null) ClearExpiredSession(loginSession); //清除已经过期的session

            }
            catch { }
            bool isExist = loginSession == null;
            if (loginSession == null)
            {

                loginSession = new LoginSessionModel()
                {
                    IsMultipleLogin = isMultipleLogin,
                    Sessions = new Dictionary<string, TokenInfo>(),
                    Expiration = expiration

                };
            }
            loginSession.LatestSession = token;

            if (loginSession.Sessions.ContainsKey(deviceId))
            {
                loginSession.Sessions[deviceId] = new TokenInfo(token, DateTime.UtcNow + expiration, requestTimeStamp, deviceId, DeviceType, SysMark);
            }
            else loginSession.Sessions.Add(deviceId, new TokenInfo(token, DateTime.UtcNow + expiration, requestTimeStamp, deviceId, DeviceType, SysMark));

            if (isExist)
                CacheService.Repository.Replace(sessionKey, loginSession, expiration);
            else
                CacheService.Repository.Add(sessionKey, loginSession, expiration);
        }

        [HttpGet]
        [Route("getsso")]
        [AllowAnonymous]
        public void GetSSO()
        {
            Stream stream = Request.Body;
            Byte[] byteData = new Byte[stream.Length];
            stream.Read(byteData, 0, (Int32)stream.Length);
            string code = Request.Query["code"];
            string urlType =string.IsNullOrEmpty( Request.Query["urltype"])?"1" : Request.Query["urltype"].ToString();
            EnterpriseWeChatModel enterpriseWeChat = new EnterpriseWeChatModel();
            enterpriseWeChat.Code = code;
            enterpriseWeChat.Data = new Dictionary<string, object>();
            enterpriseWeChat.UrlType = UrlTypeEnum.SSO;

            int userNumber;
            var result = _enterpriseWeChatServices.GetSSOCode(enterpriseWeChat, out userNumber);
			if (result.Status == 1)
			{
				HttpContext.Response.ContentType = "text/plain; charset=utf-8";
				HttpContext.Response.WriteAsync("您未开通CRM账号，请联系管理员");
				return;
			}
			var account = _enterpriseWeChatServices.GetAccountInfo(Convert.ToInt32(userNumber));
            var userData = account.DataBody as AccountUserInfo;
			if (userData == null)
			{
				HttpContext.Response.ContentType = "text/plain;chartset=utf-8";
				HttpContext.Response.WriteAsync("您未开通CRM账号，请联系管理员");
				return;
			}
            var header = GetAnalyseHeader();
            var deviceId = header.DeviceId;
            if (header.DeviceId.Equals("UnKnown"))
            {
                //如果web没有传deviceid字段，则取token作为设备id
                deviceId = result.DataBody.ToString();
            }
            var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            var webSession = config.GetSection("WebSession");
            TimeSpan webexpiration = new TimeSpan(0, 20, 0);
            if (webSession != null)
            {
                var seconds = webSession.GetValue<int>("Expiration");
                webexpiration = new TimeSpan(0, 0, seconds);
            }
            var loginSession = CacheService.Repository.Get<LoginSessionModel>("WebLoginSession_" + userNumber);
            if (loginSession != null) ClearExpiredSession(loginSession);//清除已经过期的session
            if (loginSession != null && loginSession.Sessions.ContainsKey(deviceId))
            {
                loginSession.Sessions.Remove(deviceId);
                CacheService.Repository.Replace("WebLoginSession_" + userNumber, loginSession, loginSession.Expiration);
            }
            SetLoginSession("WebLoginSession_" + userNumber, result.DataBody.ToString(), deviceId, webexpiration, 0, header.SysMark, header.Device, true);
            var cookie = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(120),
                Domain = config.GetValue<string>("Domain"),
                Path = "/"
            };
            HttpContext.Response.Cookies.Append("token", result.DataBody.ToString(), cookie);
            HttpContext.Response.Cookies.Append("account", userData.AccountName, cookie);
            HttpContext.Response.Cookies.Append("accountid", userData.AccountId.ToString(), cookie);
            HttpContext.Response.Headers.Add("Authorization", "Bearer " + result.DataBody.ToString());
            var page = config.GetSection("WeChatConfig").GetValue<string>("EnterpriseWeChatMainPage");
            HttpContext.Response.WriteAsync("<script type='text/javascript'>location.href='" + page + "';</script>");
        }
        [HttpGet]
        [Route("geturl")]
        [AllowAnonymous]
        public string GetAbc()
        {
            Stream stream = Request.Body;
            Byte[] byteData = new Byte[stream.Length];
            stream.Read(byteData, 0, (Int32)stream.Length);
            string code = Request.Query["code"];
            return code;
        }

    }
}
