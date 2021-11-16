using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using UBeat.Crm.CoreApi.DingTalk.Models;
using UBeat.Crm.CoreApi.DingTalk.Repository;
using UBeat.Crm.CoreApi.DingTalk.Utils;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Utility;

namespace UBeat.Crm.CoreApi.DingTalk.Services
{
	public class DingTalkServices: EntityBaseServices
	{
		private readonly IDingTalkRepository _dingTalk;
		private ILogger _logger = LogManager.GetLogger("UBeat.Crm.CoreApi.DingTalk.Services.DingTalkServices");
		private readonly IConfiguration _configurationRoot;
		private readonly IAccountRepository _accountRepository;
		private readonly IDynamicEntityRepository _dynamicEntityRepository;
		private readonly DingTalkAdressBookServices _dingTalkAdressBookServices;

		public DingTalkServices(IDingTalkRepository dingTalk, IConfigurationRoot configurationRoot, IAccountRepository accountRepository, IDynamicEntityRepository dynamicEntityRepository, DingTalkAdressBookServices dingTalkAdressBookServices)
		{
			_dingTalk = dingTalk;
			_configurationRoot = configurationRoot;
			_accountRepository = accountRepository;
			_dynamicEntityRepository = dynamicEntityRepository;
			_dingTalkAdressBookServices = dingTalkAdressBookServices;
		}

		public OutputResult<object> GetSSOCode(DingTalkModel talkModel, out int userId)
		{
			try
			{
				OperateResult result;
				string Access_token = DingTalkTokenUtils.GetAccessToken();
				if (string.IsNullOrEmpty(Access_token))
				{
					result = new OperateResult
					{
						Flag = 0,
						Msg = "钉钉单点登录失败"
					};
					userId = 0;
					return HandleResult(result);
				}
				string ddUserUrl = DingTalkUrlUtils.GetUserId() + "?access_token=" + Access_token + "&code=" + talkModel.Code;
				string response = DingTalkHttpUtils.HttpGet(ddUserUrl);
				_logger.Log(LogLevel.Fatal, JsonConvert.SerializeObject(response));
				var ddUserInfo = JsonConvert.DeserializeObject<DingTalkUserInfo>(response);

				AccountUserInfo userInfo = null;
				if (ddUserInfo == null || string.IsNullOrEmpty(ddUserInfo.userid))
				{
					result = new OperateResult
					{
						Flag = 0,
						Msg = "钉钉获取用户信息失败" + talkModel.Code
					};
					userId = 0;
					return HandleResult(result);
				}
				else
				{
					var u = _dingTalkAdressBookServices.GetUserInfo(ddUserInfo.userid);
					if (u != null)
					{
						var mobile = u.Mobile;
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
				else
				{
					//保存钉钉的用户Id
					_accountRepository.SetUserInfoDdUserId(ddUserInfo.userid, userInfo.UserId);
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
				return HandleResult(new OperateResult { Flag = 1, Msg = ex.Message });
			}
		}

		public OutputResult<object> GetAccountInfo(int userNumber)
		{
			var result = _accountRepository.GetAccountUserInfo(userNumber);
			return new OutputResult<object>(result);
		}
	}
}
