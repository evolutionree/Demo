using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models.SoapErp;
using UBeat.Crm.CoreApi.Services.Utility;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Data.Common;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Reflection;

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
        private readonly IToERPRepository _toERPRepository;
        public SoapServices()
        {
            _configurationRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            _dynamicEntityRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            _toERPRepository = ServiceLocator.Current.GetInstance<IToERPRepository>();
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
            var _dynamicEntityServices = ServiceLocator.Current.GetInstance<DynamicEntityServices>();
            Dictionary<string, object> relinfo = new Dictionary<string, object>();
            relinfo.Add("recid", detail["recid"]);
            relinfo.Add("relid", "0dc586b0-c721-4319-af6c-c7d4639638d7");
            var custaddr = _dynamicEntityServices.DataList(new Models.DynamicEntity.DynamicEntityListModel
            {
                EntityId = Guid.Parse("689bc59b-f60d-4084-b99d-b0a3e406e873"),
                MenuId = "f38b01b0-f072-471c-acbd-c8f890c9cab9",
                RelInfo = relinfo,
                PageIndex = 1,
                PageSize = int.MaxValue,
                ViewType = 0,
                SearchOrder = ""
            }, false, userId);
            var subdata = (custaddr.DataBody as Dictionary<string, List<IDictionary<string, object>>>)["PageData"];
            subdata.ForEach(t =>
            t.Add("custcode", detail["custcode"])
            );
            detail.Add("customerAddress".ToLower(), subdata);
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
                var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate=20191229&endDate=20200108", headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                var subResult = ParseResult(result) as SubOperateResult;
                var dealData = SoapHttpHelper.PersistenceEntityData<FromProductSoap>(subResult.Data.ToString(), userId, logId);
                OperateResult dataResult = new OperateResult();
                var db = new PostgreHelper();
                var conn = db.GetDbConnect();
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    var productRepository = ServiceLocator.Current.GetInstance<IProductsRepository>();
                    var entityId = typeof(FromProductSoap).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                    foreach (var t in dealData)
                    {
                        var recid = productRepository.IsProductExists(trans, t["productcode"].ToString(), userId);
                        if (recid != null && !string.IsNullOrEmpty(recid.ToString()))
                            dataResult = _dynamicEntityRepository.DynamicEdit(trans, Guid.Parse(entityId), Guid.Parse(recid.ToString()), t, userId);
                        else
                            dataResult = _dynamicEntityRepository.DynamicAdd(trans, Guid.Parse(entityId), t, null, userId);
                        if (dataResult.Flag == 0)
                        {
                            trans.Rollback();
                            break;
                        }
                    }
                    trans.Commit();
                    SoapHttpHelper.Log(new List<string> { "finallyresult" }, new List<string> { "erp产品同步到CRM成功" + JsonConvert.SerializeObject(dealData) }, 1, userId, logId);
                }
                catch (Exception ex)
                {
                    int isUpdate = 0;
                    if (!string.IsNullOrEmpty(logId))
                        isUpdate = 1;
                    else
                        isUpdate = 0;
                    SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message }, isUpdate, userId, logId);
                    trans.Rollback();
                }
                finally
                {
                    trans.Dispose();
                    conn.Close();
                }
                return dataResult;
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

        /// <summary>
        /// 订单
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="filterKey"></param>
        /// <param name="orignalName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OperateResult FromErpOrder(IDictionary<string, object> detail, string filterKey, string orignalName, int userId)
        {
            string logId = string.Empty;
            try
            {
                var config = ValidConfig("OrderSoap", filterKey, orignalName);
                if (config.Flag == 0) return config;
                var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("token", AuthToLoginERP(userId));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl }, 0, userId).ToString();
                var result = "{\"code\":200,\"message\":\"success\",\"data\":[{\"recId\":286947,\"contractDate\":\"2020-05-13 00:00:00\",\"contractNo\":\"0753\",\"factoryCode\":\"惠州中京电子科技有限公司\",\"contractItem\":[{\"contractId\":286947,\"quantity\":50,\"deliveredQuantity\":25,\"requiredDate\":\"2020-05-13 00:00:00\",\"marketDate\":\"2020-05-13 00:00:00\",\"planDate\":\"2020-05-13 00:00:00\",\"price\":56.71,\"customer\":\"820\",\"customerModel\":\"SN1962A-F_MB_V0.85\",\"productCode\":\"P2005131014451522253\",\"custProductCode\":\" YY19620085\"},{\"contractId\":286947,\"quantity\":50,\"deliveredQuantity\":25,\"requiredDate\":\"2020-06-13 00:00:00\",\"marketDate\":\"2020-05-07 00:00:00\",\"planDate\":\"2020-05-07 00:00:00\",\"price\":56.71,\"customer\":\"820\",\"customerModel\":\"SN1962A-F_MB_V0.85\",\"productCode\":null,\"custProductCode\":\" YY19620085\"}]},{\"recId\":286954,\"contractDate\":\"2020-05-13 00:00:00\",\"contractNo\":\"wewqes1122\",\"factoryCode\":\"惠州中京电子科技有限公司\",\"contractItem\":[{\"contractId\":286954,\"quantity\":1200,\"deliveredQuantity\":300,\"requiredDate\":\"2020-06-13 00:00:00\",\"marketDate\":\"2020-06-04 00:00:00\",\"planDate\":\"2020-06-04 00:00:00\",\"price\":39,\"customer\":\"1178\",\"customerModel\":\"test201\",\"productCode\":\"P2005131107290568185\",\"custProductCode\":\"\"},{\"contractId\":286954,\"quantity\":1200,\"deliveredQuantity\":1200,\"requiredDate\":\"2020-05-13 00:00:00\",\"marketDate\":\"2020-05-13 00:00:00\",\"planDate\":\"2020-05-13 00:00:00\",\"price\":10,\"customer\":\"1178\",\"customerModel\":\"test201-1\",\"productCode\":\"P2005131120393225107\",\"custProductCode\":\"test201-1\"}]},{\"recId\":286955,\"contractDate\":\"2020-05-13 00:00:00\",\"contractNo\":\"wewqes1122-a\",\"factoryCode\":\"惠州中京电子科技有限公司\",\"contractItem\":[]}]}";
                //HttpLib.Get(soapConfig.SoapUrl + "?startDate=20200501&endDate=20200513", headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                var subResult = ParseResult(result) as SubOperateResult;
                var dealData = SoapHttpHelper.PersistenceEntityData<FromOrder, FromOrderDetail>(subResult.Data.ToString(), userId, logId);
                OperateResult dataResult = new OperateResult();
                var db = new PostgreHelper();
                var conn = db.GetDbConnect();
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    var entityId = typeof(FromOrder).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                    var subEntityId = typeof(FromOrderDetail).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                    foreach (var t in dealData)
                    {
                        if (subEntityId != null)
                        {
                            var subDetail = t["detail"] as List<Dictionary<string, object>>;
                            var newSubDetail = new List<Dictionary<string, object>>();
                            foreach (var t1 in subDetail)
                            {
                                var buidlerDetail = new Dictionary<string, object>();
                                buidlerDetail.Add("TypeId", subEntityId);
                                buidlerDetail.Add("FieldData", t1);
                                newSubDetail.Add(buidlerDetail);
                            }
                            t["detail"] = JsonConvert.SerializeObject(newSubDetail);
                        }
                        var recid = _toERPRepository.IsExistsOrder(t["orderid"].ToString());
                        if (!string.IsNullOrEmpty(recid))
                            dataResult = _dynamicEntityRepository.DynamicEdit(trans, Guid.Parse(entityId), Guid.Parse(recid), t, userId);
                        else
                            dataResult = _dynamicEntityRepository.DynamicAdd(trans, Guid.Parse(entityId), t, null, userId);
                        if (dataResult.Flag == 0)
                        {
                            trans.Rollback();
                            break;
                        }
                    }
                    trans.Commit();
                    SoapHttpHelper.Log(new List<string> { "finallyresult" }, new List<string> { "erp产品同步到CRM成功" + JsonConvert.SerializeObject(dealData) }, 1, userId, logId);
                }
                catch (Exception ex)
                {
                    int isUpdate = 0;
                    if (!string.IsNullOrEmpty(logId))
                        isUpdate = 1;
                    else
                        isUpdate = 0;
                    SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message }, isUpdate, userId, logId);
                    trans.Rollback();
                }
                finally
                {
                    trans.Dispose();
                    conn.Close();
                }
                return dataResult;
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

        /// <summary>
        /// 发货单
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="filterKey"></param>
        /// <param name="orignalName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OperateResult FromErpPackingShip(IDictionary<string, object> detail, string filterKey, string orignalName, int userId)
        {
            string logId = string.Empty;
            try
            {
                var config = ValidConfig("PackingShipSoap", filterKey, orignalName);
                if (config.Flag == 0) return config;
                var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("token", AuthToLoginERP(userId));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl }, 0, userId).ToString();
                var result = "{\"code\": 200, 	\"message\": \"success\", 	\"data\": [{ 			\"recId\": 38469, 			\"shipingNotesNumer\": \"DP20050600004\", 			\"customerCode\": \"0692b\", 			\"customerName\": \"正鹏电子（昆山）有限公司\", 			\"shippingAddress\": \"江苏省昆山综合保税区新竹路88号\", 			\"shippedDate\": \"2020-05-06 12:00:00\", 			\"packingSlipItem\": [{ 				\"recId\": 104149, 				\"packingSlipId\": 38469, 				\"contractNumber\": \"SO20032101892\", 				\"soNumber\": \"SO2003210189201\", 				\"salesPartNum\": \"MN30692G040058B\", 				\"salesPartName\": \"19K-514-6901R\", 				\"qtyOfPcsAssigned\": 48, 				\"unit\": \"PCS\" 			}] 		}, 		{ 			\"recId\": 38470, 			\"shipingNotesNumer\": \"DP20050600005\", 			\"customerCode\": \"0692b\", 			\"customerName\": \"正鹏电子（昆山）有限公司\", 			\"shippingAddress\": \"江苏省昆山综合保税区新竹路88号\", 			\"shippedDate\": \"2020-05-11 14:45:26\", 			\"packingSlipItem\": [{ 				\"recId\": 104150, 				\"packingSlipId\": 38470, 				\"contractNumber\": \"190447-001\", 				\"soNumber\": \"190447-001\", 				\"salesPartNum\": \"MH30692J040122A\", 				\"salesPartName\": \"19K-516-5100R\", 				\"qtyOfPcsAssigned\": 0, 				\"unit\": \"PCS\" 			}] 		} 	] }";
                //HttpLib.Get(soapConfig.SoapUrl + "?startDate=20200501&endDate=20200513", headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                var subResult = ParseResult(result) as SubOperateResult;
                var dealData = SoapHttpHelper.PersistenceEntityData<FromPackingShip, FromPackingShipDetail>(subResult.Data.ToString(), userId, logId);
                OperateResult dataResult = new OperateResult();
                var db = new PostgreHelper();
                var conn = db.GetDbConnect();
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    var entityId = typeof(FromPackingShip).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                    var subEntityId = typeof(FromPackingShipDetail).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                    foreach (var t in dealData)
                    {
                        if (subEntityId != null)
                        {
                            var subDetail = t["detail"] as List<Dictionary<string, object>>;
                            var newSubDetail = new List<Dictionary<string, object>>();
                            foreach (var t1 in subDetail)
                            {
                                var buidlerDetail = new Dictionary<string, object>();
                                buidlerDetail.Add("TypeId", subEntityId);
                                buidlerDetail.Add("FieldData", t1);
                                newSubDetail.Add(buidlerDetail);
                            }
                            t["detail"] = JsonConvert.SerializeObject(newSubDetail);
                        }
                        var recid = _toERPRepository.IsExistsPackingShipOrder(t["packingshipid"].ToString());
                        if (!string.IsNullOrEmpty(recid))
                            dataResult = _dynamicEntityRepository.DynamicEdit(trans, Guid.Parse(entityId), Guid.Parse(recid), t, userId);
                        else
                            dataResult = _dynamicEntityRepository.DynamicAdd(trans, Guid.Parse(entityId), t, null, userId);
                        if (dataResult.Flag == 0)
                        {
                            trans.Rollback();
                            break;
                        }
                    }

                    trans.Commit();
                    SoapHttpHelper.Log(new List<string> { "finallyresult" }, new List<string> { "erp产品同步到CRM成功" + JsonConvert.SerializeObject(dealData) }, 1, userId, logId);
                }
                catch (Exception ex)
                {
                    int isUpdate = 0;
                    if (!string.IsNullOrEmpty(logId))
                        isUpdate = 1;
                    else
                        isUpdate = 0;
                    SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message }, isUpdate, userId, logId);
                    trans.Rollback();
                }
                finally
                {
                    trans.Dispose();
                    conn.Close();
                }
                return dataResult;
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
                var logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { JsonConvert.SerializeObject(soapConfig.Params), soapConfig.SoapUrl }, 0, userId);
                var soapResult = HttpLib.Get(soapConfig.SoapUrl+ string.Format("?userId={0}&password={1}", soapConfig.Params[0].DefaultValue, soapConfig.Params[1].DefaultValue));
                 var subResult = ParseResult(soapResult) as SubOperateResult;
                return subResult;
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
