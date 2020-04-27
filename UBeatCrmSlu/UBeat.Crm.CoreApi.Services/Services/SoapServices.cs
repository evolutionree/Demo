using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Models.SoapErp;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class SoapServices : EntityBaseServices
    {
        private string SOAPFUNCPARM = " " +
            "   <{0} xmlns=\"http://tempuri.org/\">" +
            "{1}" +
 "    </{0}> ";
        private string SOAPBODYPARAM = "<{0}>" +
            "{1}" +
      "</{0}> ";
        private string SOAPPROPERTY = " <{0}>{1}</{0}> ";

        private readonly IConfigurationRoot _configurationRoot;
        public SoapServices(IConfigurationRoot configurationRoot)
        {
            _configurationRoot = configurationRoot;
        }
        public OperateResult ToErpCustomer(Dictionary<string, object> detail, int userId)
        {
            string body = string.Empty;
            var soap = _configurationRoot.GetSection("ErpSoapInterfaces");
            if (soap == null) return new OperateResult { Msg = "没有配置Soap" };
            var soapConfig = soap.GetValue<SoapConfig>("CustomerSoap");
            if (soapConfig == null) return new OperateResult { Msg = "没有配置Soap接口" };
            var type = SoapHttpHelper.GetType(soapConfig.Params[0].ParamType);
            var kv = SoapHttpHelper.GetParamValueKV(type);
            foreach (var d in kv)
            {
                body += string.Format(SOAPPROPERTY, d.Key, detail[d.Value] ?? "");
            }
            string bodyParam = string.Format(SOAPBODYPARAM, soapConfig.Params[0].ParamName, body);
            string funcParam = string.Format(SOAPFUNCPARM, soapConfig.FunctionName, bodyParam);
            var result = SoapHttpHelper.SendSoapRequest(soapConfig.SoapUrl, funcParam);
            if (result.IsSuccess)
                return new OperateResult { Flag = 1, Msg = result.Msg };
            return new OperateResult { Flag = 0, Msg = result.Msg };
        }
    }
}
