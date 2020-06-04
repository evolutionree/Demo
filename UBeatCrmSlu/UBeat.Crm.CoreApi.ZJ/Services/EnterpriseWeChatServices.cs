using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Utility;
using UBeat.Crm.CoreApi.ZJ.WJXModel;

namespace UBeat.Crm.CoreApi.ZJ.Services
{
    public class EnterpriseWeChatServices : BasicBaseServices
    {
        private static readonly string OAuth2 = "https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_base&state=STATE#wechat_redirect";

        private static readonly string Access_Token = "https://qyapi.weixin.qq.com/cgi-bin/gettoken?corpid={0}&corpsecret={1}";
        private static readonly string UserAuth = "https://qyapi.weixin.qq.com/cgi-bin/user/getuserinfo?access_token={0}&code={1}";
        private readonly IConfiguration _configurationRoot;
        private readonly IAccountRepository _accountRepository;
        public EnterpriseWeChatServices(IConfigurationRoot configurationRoot, IAccountRepository accountRepository)
        {
            _configurationRoot = configurationRoot;
            _accountRepository = accountRepository;
        }
        public OutputResult<object> GetSSOCode(EnterpriseWeChatModel enterpriseWeChat)
        {
            OperateResult result;
            var config = _configurationRoot.GetSection("WeChatConfig");
            string agentId = config.GetValue<string>("AgentId");
            string secret = config.GetValue<string>("Secret");
            string corpId = config.GetValue<string>("CorpId");
            var getToken = HttpLib.Get(string.Format(Access_Token, corpId, secret));
            var tokenObj = JObject.Parse(getToken);
            if (tokenObj["errcode"].ToString() !="0") {
                result = new OperateResult
                {
                    Flag = 0,
                    Msg = "企业微信单点登录失败"
                };
                return HandleResult(result);
            }
            string access_token = tokenObj["access_token"].ToString();
            var getWCUser = HttpLib.Get(string.Format(UserAuth, access_token, enterpriseWeChat.Code));
            var getWCUserData = JObject.Parse(getWCUser);
            if (getWCUserData["errcode"].ToString() != "0") {
                result = new OperateResult
                {
                    Flag = 0,
                    Msg = "企业微信获取用户信息失败"
                };
                return HandleResult(result);
            }
            if (!getWCUserData.ContainsKey("UserId"))
            {
                result = new OperateResult
                {
                    Flag = 0,
                    Msg = "非企业微信授权用户"
                };
                return HandleResult(result);
            }
            var wcUserId = getWCUserData["UserId"].ToString();
            var userInfo = _accountRepository.GetWcAccountUserInfo(wcUserId);
            if (userInfo == null || string.IsNullOrEmpty(userInfo.WCUserid))
            {
                result = new OperateResult
                {
                    Flag = 0,
                    Msg = "获取用户失败"
                };
                return HandleResult(result);
            }
            var enterpriseWeChatType = _configurationRoot.GetSection("EnterpriseWeChat").Get<EnterpriseWeChatTypeModel>();
            string actuallyUrl = string.Empty;
            if (enterpriseWeChat.UrlType == UrlTypeEnum.WorkFlow)
            {
                actuallyUrl = string.Format(enterpriseWeChatType.Workflow_EnterpriseWeChat, enterpriseWeChat.Data["caseid"]);
            }
            else
                actuallyUrl = string.Format(enterpriseWeChatType.Workflow_EnterpriseWeChat, enterpriseWeChat.Data["entityid"], enterpriseWeChat.Data["recid"]);
            DateTime expiration;
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("uid", userInfo.UserId.ToString()));
            claims.Add(new Claim("username", userInfo.UserName));
            var token = JwtAuth.SignToken(claims, out expiration);
            result = new OperateResult
            {
                Flag = 1,
                Id = actuallyUrl + "?" + token
            };
            return HandleResult(result);
        }
    }
}
