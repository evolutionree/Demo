using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Utility;
using UBeat.Crm.CoreApi.ZGQY.WJXModel;

namespace UBeat.Crm.CoreApi.ZGQY.Services
{
    public class EnterpriseWeChatServices : BasicBaseServices
    {
        private static readonly string OAuth2 = "https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_base&state=STATE#wechat_redirect";

        private static readonly string Access_Token = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}";
        private static readonly string UserAuth = "https://qyapi.weixin.qq.com/cgi-bin/user/getuserinfo?access_token={0}&code={1}";
		private static readonly string UserInfo = "https://qyapi.weixin.qq.com/cgi-bin/user/get?access_token={0}&userid={1}";
		private readonly IConfiguration _configurationRoot;
        private readonly IAccountRepository _accountRepository;
        public EnterpriseWeChatServices(IConfigurationRoot configurationRoot, IAccountRepository accountRepository)
        {
            _configurationRoot = configurationRoot;
            _accountRepository = accountRepository;
        }
        public OutputResult<object> GetAccountInfo(int userNumber)
        {
            var result = _accountRepository.GetAccountUserInfo(userNumber);
            return new OutputResult<object>(result);
        }
        public OutputResult<object> GetSSOCode(EnterpriseWeChatModel enterpriseWeChat, out int userId)
        {
            try
            {
                OperateResult result;
                var config = _configurationRoot.GetSection("WeChatConfig");
                string agentId = config.GetValue<string>("AgentId");
                string secret = config.GetValue<string>("Secret");
                string corpId = config.GetValue<string>("CorpId");
                var getToken = HttpLib.Get(string.Format(Access_Token, corpId, secret));
                var tokenObj = JObject.Parse(getToken);
                if (tokenObj["errcode"].ToString() != "0")
                {
                    result = new OperateResult
                    {
                        Flag = 0,
                        Msg = "企业微信单点登录失败"
                    };
                    userId = 0;
                    return HandleResult(result);
                }
                string access_token = tokenObj["access_token"].ToString();
                var getWCUser = HttpLib.Get(string.Format(UserAuth, access_token, enterpriseWeChat.Code));
                var getWCUserData = JObject.Parse(getWCUser);
                if (getWCUserData["errcode"].ToString() != "0")
                {
                    result = new OperateResult
                    {
                        Flag = 0,
                        Msg = "企业微信获取用户信息失败" + enterpriseWeChat.Code + getWCUser
                    };
                    userId = 0;
                    return HandleResult(result);
                }
                if (!getWCUserData.ContainsKey("UserId"))
                {
                    result = new OperateResult
                    {
                        Flag = 0,
                        Msg = "非企业微信授权用户"
                    };
                    userId = 0;
                    return HandleResult(result);
                }
                var wcUserId = getWCUserData["UserId"].ToString();
				var getWCUserInfo = HttpLib.Get(string.Format(UserInfo, access_token, wcUserId));
				AccountUserInfo userInfo = null;
				if (getWCUserInfo != null)
				{
					var getWCUserInfoData = JObject.Parse(getWCUserInfo);
					if (getWCUserInfoData.ContainsKey("mobile"))
					{
						var mobile = getWCUserInfoData["mobile"].ToString();
						userInfo = _accountRepository.GetWcAccountUserInfoByMobile(mobile);
					}
				}
				
                if (userInfo == null || userInfo.UserId <= 0)
                {
                    result = new OperateResult
                    {
                        Flag = 0,
                        Msg = "获取用户失败"
                    };
                    userId = 0;
                    return HandleResult(result);
                }
                DateTime expiration;
                List<Claim> claims = new List<Claim>();
                claims.Add(new Claim("uid", userInfo.UserId.ToString()));
                claims.Add(new Claim("username", userInfo.UserName));
                var token = JwtAuth.SignToken(claims, out expiration);
                result = new OperateResult
                {
                    Flag = 1,
                    Id = token,
                };
                userId = userInfo.UserId;
                return HandleResult(result);
            }
            catch (Exception ex)
            {
                userId = 0;
                //SoapHttpHelper.Log(new List<string> { "soapexceptionmsg", "finallyresult" }, new List<string> { ex.Message, ex.Source + ex.StackTrace }, 0, 1);
                return HandleResult(new OperateResult { Flag = 1, Msg = ex.Message });
            }
        }


    }
}
