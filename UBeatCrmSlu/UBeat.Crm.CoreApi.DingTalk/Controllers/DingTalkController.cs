using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DingTalk.Models;
using UBeat.Crm.CoreApi.DingTalk.Services;
using UBeat.Crm.CoreApi.DingTalk.Utils;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Models;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.DingTalk.Controllers
{
	[Route("api/[controller]")]
	public class DingTalkController: BaseController
	{
		private readonly DingTalkServices _dingTalkServices;
		private readonly CacheServices _cacheService;
		public DingTalkController(DingTalkServices services, CacheServices cacheService)
		{
			_dingTalkServices = services;
			_cacheService = cacheService;
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

			DingTalkModel talkModel = new DingTalkModel();
			talkModel.Code = code;
			talkModel.Data = new Dictionary<string, object>();

			int userNumber;
			var result = _dingTalkServices.GetSSOCode(talkModel, out userNumber);
			if (result.Status == 1)
			{
				return new OutputResult<object>(null, "您未开通CRM账号，请联系管理员", 1);
			}
			var token = string.Concat(result.DataBody);

			var account = _dingTalkServices.GetAccountInfo(Convert.ToInt32(userNumber));
			var userData = account.DataBody as AccountUserInfo;
			if (userData == null)
			{
				return new OutputResult<object>(null, "您未开通CRM账号，请联系管理员", 1);
			}
			var header = GetAnalyseHeader();
			var deviceId = header.DeviceId;
			if (header.DeviceId.Equals("UnKnown"))
			{
				//如果web没有传deviceid字段，则取token作为设备id
				deviceId = result.DataBody.ToString();
			}
			if (header.Device.Equals("UnKnown"))
			{
				header.Device = "H5";
				header.VerNum = "1.0.0";
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
			WriteOperateLog("钉钉登录系统", userData, userData.UserId, header.Device, header.VerNum);
			SetLoginSession("WebLoginSession_" + userNumber, result.DataBody.ToString(), deviceId, webexpiration, 0, header.SysMark, header.Device, true);

			var rusult = new
			{  
				account = userData.AccountName,
				access_token = token,
				usernumber = userData.UserId
			};
			return new OutputResult<object>(rusult);
		}

		[HttpPost]
		[Route("getsignature")]
		[AllowAnonymous]
		public OutputResult<Object> GetSignature([FromBody]DDSignatureModel signature)
		{
			var timestamp = GetTimeStamp();
			var ranCode = GetRandomString(12, false, true, false, false, "CRM");
			var config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("DingdingConfig");
			string corpId = config.GetValue<string>("CorpId");
			string agentId = config.GetValue<string>("AgentId");

			var cacheTokenKey = "dingtalktoken";
			var token = _cacheService.Repository.Get<string>(cacheTokenKey);
			if (string.IsNullOrEmpty(token))
			{
				token = DingTalkTokenUtils.GetAccessToken();
				if (string.IsNullOrEmpty(token))
				{
					return new OutputResult<object>("获取企业Token异常");
				}
				TimeSpan expiration = DateTime.UtcNow.AddSeconds(3400) - DateTime.UtcNow;
				_cacheService.Repository.Add(cacheTokenKey, token, expiration);
			}

			var cacheTicketKey = "dingtalkticket";
			var ticket = _cacheService.Repository.Get<string>(cacheTicketKey);
			if(string.IsNullOrEmpty(ticket))
			{
				ticket = DingTalkTokenUtils.GetTiket(token);
				if (string.IsNullOrEmpty(ticket))
				{
					return new OutputResult<object>("获取企业Ticket异常");
				}

				TimeSpan expiration = DateTime.UtcNow.AddSeconds(3400) - DateTime.UtcNow;
				_cacheService.Repository.Add(cacheTicketKey, ticket, expiration);
			}
			var str = getSign(ticket, ranCode, timestamp, signature.Url);
			return new OutputResult<object>(new {agentid = agentId, corpid = corpId, timestamp = timestamp, noncestr = ranCode, signature = str });
		}
		string GetTimeStamp()
		{
			TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
			return ((DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000).ToString();
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

		public string getSign(string ticket, string nonceStr, string timeStamp, string url)
		{
			String plain = string.Format("jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}", ticket, nonceStr, timeStamp, url);

			try
			{
				byte[] bytes = Encoding.UTF8.GetBytes(plain);
				byte[] digest = SHA1.Create().ComputeHash(bytes);
				string digestBytesString = BitConverter.ToString(digest).Replace("-", "");
				return digestBytesString.ToLower();
			}
			catch (Exception e)
			{
				throw;
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
	}
}
