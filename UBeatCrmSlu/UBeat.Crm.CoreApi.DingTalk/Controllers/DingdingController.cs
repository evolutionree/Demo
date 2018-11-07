using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DingTalk.Utils;
using Microsoft.AspNetCore.Authorization;
using UBeat.Crm.CoreApi.DingTalk.Services;
using System.Security.Claims;
using UBeat.Crm.CoreApi.Utility;
using UBeat.Crm.CoreApi.Models;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DingTalk.Models;
using UBeat.Crm.CoreApi.Repository.Utility.Cache;
using UBeat.Crm.CoreApi.DomainModel.Account;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DingdingController : BaseController
    {
        private readonly DingTalkAdressBookServices _services;
        public DingdingController(DingTalkAdressBookServices services)
        {
            _services = services;
        }
        /// <summary>
        /// 用户H5客户端登陆使用
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("loginwithdingtalk")]
        public OutputResult<object> LoginWithDingTalkCode([FromBody] H5LoginWithCodeParamInfo paramInfo)
        {
            try
            {
                if (paramInfo == null || paramInfo.Code == null || paramInfo.Code.Length == 0)
                {
                    return ResponseError<object>("参数异常");
                }

                var userData = _services.LoginWithDingTalkCode(paramInfo.Code);
                if (userData == null)
                    return ResponseError<object>("未找到指定用户");
                long requestTimeStamp = 0;
                DateTime expiration;
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim("uid", userData.UserId.ToString()));
                claims.Add(new Claim("username", userData.UserName));
                var token = JwtAuth.SignToken(claims, out expiration);
                LoginUser.UserId = userData.UserId;
                var header = GetAnalyseHeader();
                var deviceId = header.DeviceId;
                if (header.DeviceId.Equals("UnKnown"))
                {
                    //如果web没有传deviceid字段，则取token作为设备id
                    deviceId = token;
                }
                var Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("WebSession");
                TimeSpan webexpiration = new TimeSpan(0, 20, 0);
                if (Config != null)
                {
                    var seconds = Config.GetValue<int>("Expiration");
                    webexpiration = new TimeSpan(0, 0, seconds);
                }
                SetLoginSession(WebLoginSessionKey, token, deviceId, webexpiration, requestTimeStamp, true);

                var rusult = new
                {
                    access_token = token,
                    AccessType = userData.AccessType,
                    usernumber = userData.UserId,
                    servertime = DateTime.Now
                };
                return new OutputResult<object>(rusult);
            }
            catch (Exception ex)
            {
                return ResponseError<object>(ex.Message);

            }

        }

        [AllowAnonymous]
        [HttpPost("loginwithdingtalkofuserid")]
        public OutputResult<object> LoginWithDingTalkUserId([FromBody] LoginWithUserIdModel body)
        {
            if (body == null || string.IsNullOrEmpty(body.UserId))
                return ResponseError<object>("参数异常");
            long requestTimeStamp = 0;
            DateTime expiration;
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("uid", body.UserId));
            var token = JwtAuth.SignToken(claims, out expiration);
            LoginUser.UserId = int.Parse(body.UserId);
            var header = GetAnalyseHeader();
            var deviceId = header.DeviceId;
            if (header.DeviceId.Equals("UnKnown"))
            {
                //如果web没有传deviceid字段，则取token作为设备id
                deviceId = token;
            }
            var Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("WebSession");
            TimeSpan webexpiration = new TimeSpan(0, 20, 0);
            if (Config != null)
            {
                var seconds = Config.GetValue<int>("Expiration");
                webexpiration = new TimeSpan(0, 0, seconds);
            }
            SetLoginSession(MobileLoginSessionKey, token, deviceId, webexpiration, requestTimeStamp, true);

            var rusult = new
            {
                access_token = token,
                //  AccessType = userData.AccessType,
                usernumber = body.UserId,
                servertime = DateTime.Now
            };
            return new OutputResult<object>(rusult);
        }

        [AllowAnonymous]
        [HttpPost("dingding")]
        public OutputResult<object> Dingding([FromBody] ParamModel body)
        {
            string url = "";
            if (body == null || string.IsNullOrEmpty(body?.url))
                url = "http://www.luckyweilai.com/test/";
            else
                url = body.url;
            string corpId = DingTalkConfig.getInstance().CorpId;
            string CorpSecret = DingTalkConfig.getInstance().CorpSecret;
            string tokenStr = DingTalkUrlUtils.GetTokenUrl() + "?corpid=" + corpId + "&corpsecret=" + CorpSecret;
            var tokenData = Get(tokenStr);

            var tokenJson = JsonConvert.DeserializeObject<TokenModel>(tokenData);
            var access_token = tokenJson.Access_Token;

            string ticketStr = DingTalkUrlUtils.Get_JSApi_Ticket_Url() + "?access_token=" + access_token;
            var ticketData = Get(ticketStr);
            var ticketJson = JsonConvert.DeserializeObject<TicketModel>(ticketData);
            var ticket = ticketJson.Ticket;

            var nonceStr = "123";
            var timeStamp = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            string plain = "jsapi_ticket=" + ticket + "&noncestr=" + nonceStr + "&timestamp=" + timeStamp
            + "&url=" + url;
            var signature = SHA1(plain);
            Dictionary<string, object> ruslut = new Dictionary<string, object>();
            ruslut.Add("signature", signature);
            ruslut.Add("nonstr", nonceStr);
            ruslut.Add("url", url);
            ruslut.Add("timeStamp", timeStamp);
            ruslut.Add("corpId", corpId);
            return new OutputResult<object>(ruslut, "返回成功", 0);
        }

        [AllowAnonymous]
        [HttpPost("accesstoken")]
        public OutputResult<object> GetAccessToken()
        {
            string tmp = DingTalkTokenUtils.GetAccessToken();
            return new OutputResult<object>(tmp);
        }
        [AllowAnonymous]
        [HttpPost("listdepts")]
        public OutputResult<object> ListDepts()
        {
            List<DingTalkDeptInfo> result = _services.ListDingTalkDepartments(DingTalkConfig.getInstance().RootDeptId);
            return new OutputResult<object>(result);
        }
        [AllowAnonymous]
        [HttpPost("listuserbydept")]
        public OutputResult<object> ListUserByDepts([FromBody] FetUserListByDeptIdParamInfo paramInfo)
        {
            if (paramInfo == null)
            {
                return new OutputResult<object>(_services.GetUsersByDept(1));
            }
            else
            {
                return new OutputResult<object>(_services.GetUsersByDept(paramInfo.DeptId));
            }
        }
        [AllowAnonymous]
        [HttpPost("dduserinfo")]
        public OutputResult<object> GetUserInfo([FromBody] GetUserInfoParamInfo paramInfo)
        {
            if (paramInfo == null)
            {
                return new OutputResult<object>(_services.GetUserInfo(""));
            }
            else
            {
                return new OutputResult<object>(_services.GetUserInfo(paramInfo.DDUserid));
            }
        }

        public string Get(string apiStr)
        {
            Encoding encoding = Encoding.UTF8;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiStr);
            request.Method = "GET";
            request.Accept = "text/html, application/xhtml+xml, */*";
            request.ContentType = "application/json";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream(), encoding))
            {
                return reader.ReadToEnd().ToString();
            }

        }

        public string SHA1(string enstr)
        {
            var strRes = Encoding.Default.GetBytes(enstr);
            HashAlgorithm iSha = new SHA1CryptoServiceProvider();
            strRes = iSha.ComputeHash(strRes);
            var enText = new StringBuilder();
            foreach (byte iByte in strRes)
            {
                enText.AppendFormat("{0:x2}", iByte);
            }
            return enText.ToString();
        }


        private void SetLoginSession(string sessionKey, string token, string deviceId, TimeSpan expiration, long requestTimeStamp, bool isMultipleLogin = true)
        {
            LoginSessionModel loginSession = null;
            try
            {
                loginSession = CacheService.Repository.Get<LoginSessionModel>(sessionKey);
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
                loginSession.Sessions[deviceId] = new TokenInfo(token, DateTime.UtcNow + expiration, requestTimeStamp);
            }
            else loginSession.Sessions.Add(deviceId, new TokenInfo(token, DateTime.UtcNow + expiration, requestTimeStamp));

            if (isExist)
                CacheService.Repository.Replace(sessionKey, loginSession, expiration);
            else
                CacheService.Repository.Add(sessionKey, loginSession, expiration);
        }

        [HttpPost("getentrance")]
        public OutputResult<object> GetEntranceList()
        {
            return _services.GetEntranceList();
        }

        [HttpPost("savecachemsg")]
        public OutputResult<object> SaveCacheMessage([FromBody] MessageModel body)
        {
            if (body == null) ResponseError<object>("参数格式有误");
            if (string.IsNullOrEmpty(body.RecId) || string.IsNullOrEmpty(body.EntityId))
                return ResponseError<object>("特定参数不允许为空");
            Guid g = Guid.NewGuid();
            string key = g.ToString("N");
            body.UserId = UserId;
            CacheService.Repository.Add(key, body);
            return new OutputResult<object>(key);
        }

        [AllowAnonymous]
        [HttpPost("getcachemsg")]
        public OutputResult<object> GetChacheMessage([FromBody] GetMessageModel body)
        {
            if (body == null) return ResponseError<object>("参数格式有误");
            if (string.IsNullOrEmpty(body.Key))
                return ResponseError<object>("Key不允许为空");
            var result = CacheService.Repository.Get<MessageModel>(body.Key);
            return new OutputResult<object>(result);
        }

        [AllowAnonymous]
        [HttpPost("getrolelist")]
        public OutputResult<object> GetRoleList([FromBody] GetMessageModel body)
        {
            var result = _services.GetRoleList(UserId);
            return new OutputResult<object>(result);
        }



        public class TokenModel
        {
            public string Access_Token { get; set; }
        }

        public class TicketModel
        {
            public string Ticket { get; set; }
        }

        public class ParamModel
        {
            public string url { get; set; }
        }

        public class FetUserListByDeptIdParamInfo
        {
            public long DeptId { get; set; }
        }
        public class GetUserInfoParamInfo
        {
            public string DDUserid { get; set; }
        }
        public class H5LoginWithCodeParamInfo
        {
            public string Code { get; set; }
        }

        public class LoginWithUserIdModel
        {
            public string UserId { get; set; }
        }

        public class MessageModel
        {
            public int UserId { get; set; }
            public string EntityId { get; set; }
            public string RecId { get; set; }
        }

        public class GetMessageModel
        {
            public string Key { get; set; }
        }
        /// <summary>
        /// SSO login 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("synchdeptwithdingtalk")]
        public OutputResult<object> SynchDeptWithDingtalk([FromBody] H5LoginWithCodeParamInfo paramInfo)
        {
            _services.SynDingTalkDepartment();
            return null;
        }

        /// <summary>
        /// SSO login 
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ssologinwithdingtalk")]
        public OutputResult<object> SSOLoginWithDingTalkCode([FromBody] H5LoginWithCodeParamInfo paramInfo)
        {
            try
            {
                if (paramInfo == null || paramInfo.Code == null || paramInfo.Code.Length == 0)
                {
                    return ResponseError<object>("参数异常");
                }

                var userData = _services.SSOLoginWithDingTalkCode(paramInfo.Code);
                if (userData == null)
                    return ResponseError<object>("未找到指定用户");
                long requestTimeStamp = 0;
                DateTime expiration;
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim("uid", userData.UserId.ToString()));
                claims.Add(new Claim("username", userData.UserName));
                var token = JwtAuth.SignToken(claims, out expiration);
                LoginUser.UserId = userData.UserId;
                var header = GetAnalyseHeader();
                var deviceId = header.DeviceId;
                if (header.DeviceId.Equals("UnKnown"))
                {
                    //如果web没有传deviceid字段，则取token作为设备id
                    deviceId = token;
                }
                var Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("WebSession");
                TimeSpan webexpiration = new TimeSpan(0, 20, 0);
                if (Config != null)
                {
                    var seconds = Config.GetValue<int>("Expiration");
                    webexpiration = new TimeSpan(0, 0, seconds);
                }
                SetLoginSession(WebLoginSessionKey, token, deviceId, webexpiration, requestTimeStamp, true);

                var rusult = new
                {
                    access_token = token,
                    AccessType = userData.AccessType,
                    usernumber = userData.UserId,
                    servertime = DateTime.Now
                };
                return new OutputResult<object>(rusult);
            }
            catch (Exception ex)
            {
                return ResponseError<object>(ex.Message);

            }

        }

        [AllowAnonymous]
        [HttpPost("syndingtalkdepartment")]
        public OutputResult<object> SynDingTalkDepartment()
        {
            _services.SynDingTalkDepartment();

            return new OutputResult<object>("syn success");

        }



        public class GetContactRelationParamInfo
        {
            public Guid ContactId { get; set; }
            public int Level { get; set; }
        }


    }
}
