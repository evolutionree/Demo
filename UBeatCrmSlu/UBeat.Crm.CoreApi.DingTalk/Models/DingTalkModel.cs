using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DingTalk.Models
{
	public class DingTalkModel
	{
		public UrlTypeEnum UrlType { get; set; }
		public string Code { get; set; }
		public Dictionary<string, object> Data { get; set; }
	}

	public enum UrlTypeEnum
	{
		SSO = 0,
		WorkFlow = 1,
		SmartReminder = 2,
		EntityDynamic = 3,
		Daily = 4,
		Weekly = 5
	}

	public class DDLoginWithCodeParamInfo
	{
		public string Code { get; set; }
	}
	public class DDSignatureModel
	{
		public string Url { get; set; }
	}
}
