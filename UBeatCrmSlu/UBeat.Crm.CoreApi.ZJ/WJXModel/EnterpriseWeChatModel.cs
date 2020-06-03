using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.ZJ.WJXModel
{
    public enum UrlTypeEnum
    {
        WorkFlow = 1,
        SmartReminder = 2
    }
    public class EnterpriseWeChatModel
    {
        public UrlTypeEnum UrlType { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}
