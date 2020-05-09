using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Reminder;
using UBeat.Crm.CoreApi.Services.Models.SoapErp;
using UBeat.Crm.CoreApi.Services.Utility;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class SoapServices : BasicBaseServices
    {
        private string SOAPFUNCPARM = " " +
            "   <soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"  xmlns:ws=\"http://ws.service.ceews.ceepcb.com/\">   <soapenv:Header/> {0}{1}</soapenv:Envelope>";
        private string SOAPBODYPARAM = " <soap:Body xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
            "                <ws:{0}> {1}</ws:{0}>" +
 "    </soap:Body> ";
        private string SOAPPROPERTY = " <{0}>{1}</{0}> ";
        private string SOAPAUTHPARAM = "<soap:Header><auth><token>{0}</token></auth> </soap:Header>";
        private static Hashtable ht = new Hashtable();
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        public SoapServices()
        {
            _configurationRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            _dynamicEntityRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
        }

        public OperateResult ToErpCustomer(IDictionary<string, object> detail, string filterKey, string orignalName, int userId)
        {
            //{ "entityId":"f9db9d79-e94b-4678-a5cc-aa6e281c1246","recId":"0320535d-35e0-41c1-8ac4-0bb39c5e06c7","needPower":0}
            detail = _dynamicEntityRepository.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
            {
                EntityId = Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"),
                NeedPower = 0,
                RecId = Guid.Parse("0320535d-35e0-41c1-8ac4-0bb39c5e06c7")
            }, userId, null);
            string logId = string.Empty;
            try
            {
                var config = ValidConfig("CustomerSoap", filterKey, orignalName);
                if (config.Flag == 0) return config;
                var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                List<object> paramData = new List<object>();
                foreach (var t in soapConfig.Params)
                {
                    if (t.IsComplex == 1)
                    {
                        var type = SoapHttpHelper.GetType(t.ParamType);
                        var kv = SoapHttpHelper.GetParamValueKV(type);
                        foreach (var d in kv)
                        {
                            SoapHttpHelper.ValueConvert(d, detail, type);
                        }
                        var data = SoapHttpHelper.OutPutERPData<ToCustomerSoap>(detail);
                        paramData.Add(data);
                    }
                }
                var param = JsonConvert.SerializeObject(paramData.FirstOrDefault());
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("token", AuthToLoginERP(userId));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { param, soapConfig.SoapUrl }, 0, userId).ToString();
                var result = HttpLib.Post(soapConfig.SoapUrl, param, headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                return ParseResult(result);
            }
            catch (Exception ex)
            {
                int isUpdate = 0;
                if (!string.IsNullOrEmpty(logId))
                    isUpdate = 1;
                else
                    isUpdate = 0;
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message }, isUpdate, userId, logId);
                return new OperateResult { Flag = 0, Msg = ex.Message };
            }
        }

        public OperateResult FromErpProduct(IDictionary<string, object> detail, string filterKey, string orignalName, int userId)
        {
            string logId = string.Empty;
            try
            {
                var config = ValidConfig("ProductSoap", filterKey, orignalName);
                if (config.Flag == 0) return config;
                var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("token", AuthToLoginERP(userId));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl }, 0, userId).ToString();
                var result = HttpLib.Get(soapConfig.SoapUrl+ "?startDate=20191229&endDate=20200108", headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                var subResult = ParseResult(result) as SubOperateResult;
                var dealData = SoapHttpHelper.PersistenceEntityData<FromProductSoap>(subResult.Data.ToString(), userId,logId);
                return ParseResult(result);
            }
            catch (Exception ex)
            {
                int isUpdate = 0;
                if (!string.IsNullOrEmpty(logId))
                    isUpdate = 1;
                else
                    isUpdate = 0;
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message }, isUpdate, userId, logId);
                return new OperateResult { Flag = 0, Msg = ex.Message };
            }
        }


        OperateResult ParseResult(string result)
        {
            JObject jObject = JObject.Parse(result);
            return new SubOperateResult { Flag = jObject["code"].ToString() == "200" ? 1 : 0, Msg = jObject["message"].ToString(), Data = jObject["data"].ToString() };
        }
        string AuthToLoginERP(int userId)
        {
            bool isNeedToAuth = false;
            string token = string.Empty;
            if (ht.Count == 0)
            {
                isNeedToAuth = true;
            }
            else
            {
                ICollection key = ht.Keys;
                foreach (object k in key)
                {
                    System.TimeSpan t3 = DateTime.Now - DateTime.Parse(k.ToString());
                    if (t3.TotalSeconds >= 2 * 60 * 60)
                        isNeedToAuth = true;
                    else
                        token = ht[k].ToString();
                    break;
                }
            }
            if (isNeedToAuth)
            {
                var result = AuthErp(userId);
                if (result.Flag == 1)
                {
                    token = (result as SubOperateResult).Data.ToString();
                    ht.Add(DateTime.Now, token);
                    return token;
                }
                else
                    throw new Exception("token为空");
            }
            return token;
        }

        public OperateResult AuthErp(int userId)
        {
            var result = ValidConfig("AuthSoap", "getToken", "登录");
            if (result.Flag == 0) return result;
            var interfaces = (result.Data as SoapInterfacesCollection).Interfaces;
            string body = string.Empty;
            var soapConfig = interfaces.FirstOrDefault();
            if (soapConfig != null && soapConfig.IsSingleParam == 0)
            {
                foreach (var p in soapConfig.Params)
                {
                    if (p.IsComplex == 0)
                        body += string.Format(SOAPPROPERTY, p.ParamName, p.DefaultValue);
                }
                string bodyParam = string.Format(SOAPBODYPARAM, soapConfig.FunctionName, body);
                string funcParam = string.Format(SOAPFUNCPARM, string.Empty, bodyParam);
                var logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { funcParam, soapConfig.SoapUrl }, 0, userId);
                var soapResult = SoapHttpHelper.SendSoapRequest(soapConfig.SoapUrl, funcParam, "//soap:Envelope/soap:Body", logId.ToString(), userId);
                return new SubOperateResult { Data = (((soapResult as SoapRequestStatus).Data) as XmlNode).InnerText, Flag = 1 };
            }
            return new SubOperateResult { Flag = 0, Msg = "校验账号配置异常" };
        }
        class SubOperateResult : OperateResult
        {
            public object Data { get; set; }
        }
        SubOperateResult ValidConfig(string key, string filterKey, string orignalName)
        {
            string body = string.Empty;
            var soap = _configurationRoot.GetSection("ErpSoapInterfaces");
            if (soap == null) return new SubOperateResult { Msg = "没有配置ERP接口" };
            var soapConfig = soap.GetSection(key).Get<SoapInterfacesCollection>();
            if (soapConfig == null) return new SubOperateResult { Msg = "没有配置" + orignalName + "接口" };
            if (soapConfig.Interfaces.FirstOrDefault(t => t.FunctionName == filterKey) == null) if (soapConfig == null) return new SubOperateResult { Msg = "没有配置" + filterKey + "接口" };
            return new SubOperateResult { Flag = 1, Data = soapConfig };
        }
    }
}
