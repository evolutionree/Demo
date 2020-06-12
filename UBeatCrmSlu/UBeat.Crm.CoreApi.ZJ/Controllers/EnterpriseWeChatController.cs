using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Models;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.ZJ.Services;
using UBeat.Crm.CoreApi.ZJ.WJXModel;

namespace UBeat.Crm.CoreApi.ZJ.Controllers
{
    [Route("api/[controller]")]
    public class EnterpriseWeChatController : BaseController
    {
        private readonly EnterpriseWeChatServices _enterpriseWeChatServices;
        private readonly AccountServices _accountServices;
        public EnterpriseWeChatController(EnterpriseWeChatServices enterpriseWeChatServices, AccountServices accountServices)
        {
            _enterpriseWeChatServices = enterpriseWeChatServices;
            _accountServices = accountServices;
        }
        [HttpGet]
        [Route("getssocode")]
        [AllowAnonymous]
        public void GetMessageSSOCode()
        {
            Stream stream = Request.Body;
            Byte[] byteData = new Byte[stream.Length];
            stream.Read(byteData, 0, (Int32)stream.Length);
            string userId = Request.Query["userid"];
            string username = Request.Query["username"];
            string caseid = Request.Query["caseid"];
            string action = Request.Query["action"];
            string code = Request.Query["code"];
            string urltype = Request.Query["urltype"];
            EnterpriseWeChatModel enterpriseWeChat = new EnterpriseWeChatModel();
            enterpriseWeChat.Code = code;
            enterpriseWeChat.Data = new Dictionary<string, object>();
            if (urltype == "1")
            {
                enterpriseWeChat.UrlType = UrlTypeEnum.WorkFlow;
                enterpriseWeChat.Data.Add("caseid", caseid);
                enterpriseWeChat.Data.Add("userid", userId);
                enterpriseWeChat.Data.Add("username", username);
            }
            else
            {
                enterpriseWeChat.UrlType = UrlTypeEnum.SmartReminder;
            }

            var result = _enterpriseWeChatServices.GetSSOCode(enterpriseWeChat);
            if (result.Status == 1) return;
            var account = _enterpriseWeChatServices.GetAccountInfo(Convert.ToInt32(userId));
            var userData = account.DataBody as AccountUserInfo;
            if (userData == null) return;
            long requestTimeStamp = 0;
            _accountServices.DecryptAccountPwd(userData.AccountPwd, out requestTimeStamp, true);
            var header = GetAnalyseHeader();
            var deviceId = header.DeviceId;
            if (header.DeviceId.Equals("UnKnown"))
            {
                //如果web没有传deviceid字段，则取token作为设备id
                deviceId = result.DataBody.ToString();
            }
            var Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("WebSession");
            TimeSpan webexpiration = new TimeSpan(0, 20, 0);
            if (Config != null)
            {
                var seconds = Config.GetValue<int>("Expiration");
                webexpiration = new TimeSpan(0, 0, seconds);
            }

            SetLoginSession(WebLoginSessionKey, result.DataBody.ToString(), deviceId, webexpiration, requestTimeStamp, header.SysMark, header.Device, true);
            var cookie = new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(120),
                Domain = "ltcwx.mos400.cn",
                Path = "/"
            };
            HttpContext.Response.Cookies.Append("token", result.DataBody.ToString(), cookie);
            HttpContext.Response.Cookies.Append("account", userData.AccountName, cookie);
            HttpContext.Response.Cookies.Append("accountid", userData.AccountId.ToString(), cookie);
            HttpContext.Response.Headers.Add("Authorization", "Bearer " + result.DataBody.ToString());
            HttpContext.Response.WriteAsync("<script type='text/javascript'>location.href='" + "http://ltcwx.mos400.cn:45290/dashboard" + "';</script>");
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
        public OutputResult<object> GetSSO()
        {
            Stream stream = Request.Body;
            Byte[] byteData = new Byte[stream.Length];
            stream.Read(byteData, 0, (Int32)stream.Length);
            string code = Request.Query["code"];

            EnterpriseWeChatModel enterpriseWeChat = new EnterpriseWeChatModel();
            enterpriseWeChat.Code = code;
            enterpriseWeChat.Data = new Dictionary<string, object>();
            enterpriseWeChat.UrlType = UrlTypeEnum.SSO;

            var result = _enterpriseWeChatServices.GetSSOCode(enterpriseWeChat);
            if (result.Status == 1) return result;
            HttpContext.Response.Cookies.Append("token", result.DataBody.ToString().Substring(result.DataBody.ToString().LastIndexOf("?") + 1), new CookieOptions
            {
                Expires = DateTime.Now.AddMinutes(120)
            });

            return result;
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
