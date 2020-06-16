using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.ZJ.WJXModel
{
    public enum UrlTypeEnum
    {
        SSO = 0,
        WorkFlow = 1,
        SmartReminder = 2
    }
    public class EnterpriseWeChatTypeModel
    {
        public string Workflow_EnterpriseWeChat { get; set; }
        public string SmartReminder_EnterpriseWeChat { get; set; }
    }
    public class EnterpriseWeChatModel
    {
        public UrlTypeEnum UrlType { get; set; }
        public string Code { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
    public class EnterpriseWeChatSignatureModel
    {
        public string Url { get; set; }
    }
}
