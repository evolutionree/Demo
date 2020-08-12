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
        private static Hashtable ht = new Hashtable();
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IToERPRepository _toERPRepository;
        private static bool IsRuning = false;
        private static int isupdate = 0;
        private static object str = "synckey";
        private readonly static object str1 = "synckey1";
        private readonly static object str2 = "synckey2";
        public SoapServices()
        {
            _configurationRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            _dynamicEntityRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            _toERPRepository = ServiceLocator.Current.GetInstance<IToERPRepository>();
        }

        public OperateResult SyncEntityDataAfterApproved(Guid entityId, Guid caseId, Guid recId, int userId, DbTransaction trans = null)
        {
            IDictionary<string, object> detailData;
            if (entityId == Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"))
            {
                detailData = _dynamicEntityRepository.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
                {
                    RecId = recId,
                    EntityId = entityId
                }, userId, trans);
            }
            else
            {
                detailData = _dynamicEntityRepository.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
                {
                    RecId = recId,
                    EntityId = entityId,
                    NeedPower = 0
                }, userId, trans);
                if (detailData == null || detailData["belongcust"] == null) throw new Exception("同步客户异常");
                detailData = _dynamicEntityRepository.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
                {
                    RecId = Guid.Parse(JObject.Parse(detailData["belongcust"].ToString())["id"].ToString()),
                    EntityId = Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"),
                    NeedPower = 0
                }, userId, trans);
            }
            Dictionary<string, object> dic = new Dictionary<string, object>();
            DomainModel.OperateResult result;
            string synTime;
            var synId = string.Empty;
            object data;
            if (detailData["custype"] == null || detailData["custype"].ToString() == "1")
            {
                result = new OperateResult { Flag = 0, Msg = "该客户是潜在客户，不进行同步" };
                dic.Add("ifsyn", 2);
                dic.Add("syncinfo", result.Msg);
                synTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dic.Add("synctime", synTime);
                dic.Add("pkcode", synId);
                _dynamicEntityRepository.DynamicEdit(trans, Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"), Guid.Parse(detailData["recid"].ToString()), dic, userId);
                return result;
            }

            if (detailData["pkcode"] == null)
            {
                result = this.ToErpCustomer(detailData, "saveCustomerFromCrm", "新增客户", userId, trans);
            }
            else
                result = this.ToErpCustomer(detailData, "updateCustomerFromCrm", "编辑客户", userId, trans);
            dic.Add("ifsyn", result.Flag == 1 ? new Nullable<int>(1) : 2);
            dic.Add("syncinfo", result.Flag == 1 ? "成功" : result.Msg);
            synTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            synId = string.Empty;
            data = (result as SubOperateResult).Data;
            if (!string.IsNullOrEmpty(data.ToString()))
            {
                synTime = JObject.Parse(data.ToString())["time"].ToString();
                synId = JObject.Parse(data.ToString())["id"].ToString();
            }
            dic.Add("synctime", synTime);
            dic.Add("pkcode", synId);
            _dynamicEntityRepository.DynamicEdit(trans, Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"), Guid.Parse(detailData["recid"].ToString()), dic, userId);

            return result;
        }
        public void QrtSyncCustomer()
        {
            lock (str2)
            {
                var customer = ServiceLocator.Current.GetInstance<ICustomerRepository>();
                var list = customer.SelectFailedCustomer(1);
                foreach (var t in list)
                {
                    SyncEntityDataAfterApproved(Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"),
                        Guid.Empty, Guid.Parse(t["recid"].ToString()), 1, null);
                }
            }
        }
        public OperateResult ToErpCustomer(IDictionary<string, object> detail, string filterKey, string orignalName, int userId, DbTransaction trans = null)
        {
            var _dynamicEntityServices = ServiceLocator.Current.GetInstance<DynamicEntityServices>();
            string logId = string.Empty;
            string result = string.Empty;
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
                headers.Add("token", AuthToLoginERP(userId, trans));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl", "soapreqstatus" }, new List<string> { param, soapConfig.SoapUrl, "请求成功" }, 0, userId, trans: trans).ToString();
                result = HttpLib.Post(soapConfig.SoapUrl, param, headers);
                var pResult = ParseResult(result);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg", "soapresstatus" }, new List<string> { result, string.Empty, pResult.Flag == 0 ? pResult.Msg : "请求返回成功【" + pResult.Msg + "】" }, 1, userId, logId.ToString(), trans: trans);
                return pResult;
            }
            catch (Exception ex)
            {
                int isUpdate = 0;
                if (!string.IsNullOrEmpty(logId))
                    isUpdate = 1;
                else
                    isUpdate = 0;
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message + "[" + result + "]" }, isUpdate, userId, logId, trans: trans);
                return new OperateResult { Flag = 0, Msg = ex.Message };
            }
        }

        public OperateResult SyncErpProduct()
        {
            return this.FromErpProduct(null, "getSalesPartList", "同步产品", 1);
        }
        public OperateResult FromErpProduct(IDictionary<string, object> detail, string filterKey, string orignalName, int userId)
        {
            string logId = string.Empty;
            try
            {
                lock (str)
                {
                    var config = ValidConfig("ProductSoap", filterKey, orignalName);
                    if (config.Flag == 0) return config;
                    var productRepository = ServiceLocator.Current.GetInstance<IProductsRepository>();
                    var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                    var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                    WebHeaderCollection headers = new WebHeaderCollection();
                    headers.Add("token", AuthToLoginERP(userId));
                    logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl }, 0, userId).ToString();
                    var startDate = productRepository.GetProductLastUpdatedTime(null, userId);
                    if (string.IsNullOrEmpty(startDate)) startDate = soapConfig.Params.FirstOrDefault().DefaultValue;
                    var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate=" + startDate + "&endDate=" + DateTime.Now.AddDays(1).ToString("yyyyMMdd"), headers);
                    //   var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate=20200716&endDate=20200717", headers);

                    SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                    //var result = _dynamicEntityRepository.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
                    //{
                    //    EntityId = Guid.Parse("22e0700c-e829-4c1b-bb4a-ec5282d359b7"),
                    //    RecId = Guid.Parse("daced7a9-387a-41da-bfd9-4222ecca336f"),
                    //    NeedPower = 0
                    //}, userId);result["soapresresult"].ToString()

                    var subResult = ParseResult(result) as SubOperateResult;
                    if (subResult.Flag == 0) return subResult;
                    var dealData = SoapHttpHelper.PersistenceEntityData<FromProductSoap>(subResult.Data.ToString(), userId, logId);
                    if (dealData == null || dealData.Count == 0) return subResult;
                    OperateResult dataResult = new OperateResult();
                    var db = new PostgreHelper();
                    var conn = db.GetDbConnect();
                    conn.Open();
                    DbTransaction trans = conn.BeginTransaction();
                    try
                    {

                        var entityId = typeof(FromProductSoap).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                        foreach (var t in dealData)
                        {
                            if (t["cust"] == null) continue;
                            if (t["cust"] != null && t["cust"].ToString() == "{}") continue;
                            var recid = productRepository.IsProductExists(trans, t["cust"].ToString(), t["productcode"].ToString(), t["partnum"].ToString(), t["partrev"].ToString(), t["salespartrev"].ToString(), t["customermodel"].ToString(), userId);
                            t.Add("productname", t["productcode"]);
                            if (recid != null && !string.IsNullOrEmpty(recid.ToString()))
                            {
                                dataResult = _dynamicEntityRepository.DynamicEdit(trans, Guid.Parse(entityId), Guid.Parse(recid.ToString()), t, userId);
                            }
                            else
                            {
                                t.Add("productsetid", "7f74192d-b937-403f-ac2a-8be34714278b");
                                dataResult = _dynamicEntityRepository.DynamicAdd(trans, Guid.Parse(entityId), t, null, userId);
                            }
                            if (dataResult.Flag == 0)
                            {
                                trans.Rollback();
                                break;
                            }
                        }
                        trans.Commit();
                        IsRuning = false;
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

        public OperateResult SyncErpOrder()
        {
            return this.FromErpOrder(null, "getContractList", "同步订单单", 1);
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
                lock (str1)
                {
                    var config = ValidConfig("OrderSoap", filterKey, orignalName);
                    if (config.Flag == 0) return config;
                    var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                    var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                    WebHeaderCollection headers = new WebHeaderCollection();
                    headers.Add("token", AuthToLoginERP(userId));
                    var startDate = _toERPRepository.GetOrderLastUpdatedTime();
                    if (string.IsNullOrEmpty(startDate)) startDate = soapConfig.Params.FirstOrDefault().DefaultValue;
                    logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl + "?startDate=" + startDate + "&endDate=" + DateTime.Now.AddDays(1).ToString("yyyyMMdd") }, 0, userId).ToString();
                    // var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate="+startDate+"&endDate=" + DateTime.Now.AddDays(1).ToString("yyyyMMdd"), headers);//
                    // var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate=20200716&endDate=20200717", headers);//

                    //var result = "{\"code\":200,\"message\":\"success\",\"data\":[{\"recId\":291135,\"contractDate\":\"2020-06-11 00:00:00\",\"contractNo\":\"5905310723\",\"factoryCode\":\"惠州中京电子科技有限公司\",\"currency\":\"人民币\",\"taxRate\":\"0.1300\",\"exchangeRate\":1,\"contractItem\":[{\"recId\":291135,\"itemRecId\":315917,\"soNumber\":null,\"businessName\":\"中京电子HDI厂直销\",\"status\":\"生效\",\"approveStatus\":\"审批通过\",\"ifNew\":\"否\",\"ifForecast\":\"预测\",\"nonPlanned\":\"是\",\"qtyplanned\":0,\"qtyTobePlanned\":25000,\"qtyUndo\":0,\"qtyForecasted\":0,\"qtyOrder\":25000,\"qtyArray\":1563,\"qtyAssigned\":0,\"qtyGift\":0,\"qtyShipped\":0,\"qtyRepaired\":0,\"qtyOrderReturned\":0,\"qtyReplenishment\":0,\"qtyReturned\":0,\"requiredDate\":\"2020-08-11 00:00:00\",\"marketDate\":\"2020-08-11 00:00:00\",\"planDate\":\"2020-08-11 00:00:00\",\"price\":15.82,\"customer\":\"0841b\",\"customerModel\":\"N19002 主板\",\"productCode\":\"P2006111551154928742\",\"custProductCode\":\"12864747-00\",\"areaOrder\":56.4}]},{\"recId\":290322,\"contractDate\":\"2020-06-04 00:00:00\",\"contractNo\":\"#\",\"factoryCode\":\"惠州中京电子科技有限公司\",\"currency\":\"人民币\",\"taxRate\":\"0.1300\",\"exchangeRate\":1,\"contractItem\":[{\"recId\":290322,\"itemRecId\":314724,\"soNumber\":null,\"businessName\":\"中京电子多层厂直销\",\"status\":\"已计划\",\"approveStatus\":\"审批通过\",\"ifNew\":\"否\",\"ifForecast\":\"预测\",\"nonPlanned\":\"否\",\"qtyplanned\":500,\"qtyTobePlanned\":0,\"qtyUndo\":0,\"qtyForecasted\":0,\"qtyOrder\":500,\"qtyArray\":50,\"qtyAssigned\":0,\"qtyGift\":0,\"qtyShipped\":0,\"qtyRepaired\":0,\"qtyOrderReturned\":0,\"qtyReplenishment\":0,\"qtyReturned\":0,\"requiredDate\":\"2020-06-14 00:00:00\",\"marketDate\":\"2020-06-14 00:00:00\",\"planDate\":\"2020-06-14 00:00:00\",\"price\":0,\"customer\":\"0841b\",\"customerModel\":\"LPC118(1) 20200424\",\"productCode\":\"P2006041537033652061\",\"custProductCode\":\"\",\"areaOrder\":0.99}]},{\"recId\":291234,\"contractDate\":\"2020-07-04 00:00:00\",\"contractNo\":\"MN2L796J040031A\",\"factoryCode\":\"香港中京電子科技有限公司\",\"currency\":\"美金\",\"taxRate\":\"0.0000\",\"exchangeRate\":7.751799,\"contractItem\":[{\"recId\":291234,\"itemRecId\":316049,\"soNumber\":null,\"businessName\":\"客户-HK中京-惠州中京多层厂\",\"status\":\"活动\",\"approveStatus\":\"制作中\",\"ifNew\":\"否\",\"ifForecast\":\"预测\",\"nonPlanned\":\"否\",\"qtyplanned\":0,\"qtyTobePlanned\":400,\"qtyUndo\":0,\"qtyForecasted\":0,\"qtyOrder\":400,\"qtyArray\":400,\"qtyAssigned\":0,\"qtyGift\":0,\"qtyShipped\":0,\"qtyRepaired\":0,\"qtyOrderReturned\":0,\"qtyReplenishment\":0,\"qtyReturned\":0,\"requiredDate\":\"2020-07-04 00:00:00\",\"marketDate\":\"2020-07-04 00:00:00\",\"planDate\":\"2020-07-04 00:00:00\",\"price\":5.1,\"customer\":\"0841b\",\"customerModel\":\"03528 Rev F\",\"productCode\":\"P2007041401389352005\",\"custProductCode\":\"\",\"areaOrder\":18.04}]},{\"recId\":291239,\"contractDate\":\"2020-07-06 00:00:00\",\"contractNo\":\"N000702G040126A\",\"factoryCode\":\"惠州中京电子科技有限公司\",\"currency\":\"人民币\",\"taxRate\":\"0.1300\",\"exchangeRate\":1,\"contractItem\":[{\"recId\":291239,\"itemRecId\":316054,\"soNumber\":null,\"businessName\":\"中京电子惠城厂直销\",\"status\":\"生效\",\"approveStatus\":\"审批通过\",\"ifNew\":\"否\",\"ifForecast\":\"预测\",\"nonPlanned\":\"是\",\"qtyplanned\":0,\"qtyTobePlanned\":450,\"qtyUndo\":0,\"qtyForecasted\":0,\"qtyOrder\":450,\"qtyArray\":450,\"qtyAssigned\":0,\"qtyGift\":0,\"qtyShipped\":0,\"qtyRepaired\":0,\"qtyOrderReturned\":0,\"qtyReplenishment\":0,\"qtyReturned\":0,\"requiredDate\":\"2020-07-06 00:00:00\",\"marketDate\":\"2020-07-06 00:00:00\",\"planDate\":\"2020-07-06 00:00:00\",\"price\":76.68,\"customer\":\"0841b\",\"customerModel\":\"P4.6F1MR9AS5.0无LOGO\",\"productCode\":\"P2007061425274683010\",\"custProductCode\":\"AAEM46090023\",\"areaOrder\":55.67}]},{\"recId\":291235,\"contractDate\":\"2020-07-04 00:00:00\",\"contractNo\":\"H000342R040062\",\"factoryCode\":\"惠州中京电子科技有限公司\",\"currency\":\"人民币\",\"taxRate\":\"0.1300\",\"exchangeRate\":1,\"contractItem\":[{\"recId\":291235,\"itemRecId\":316050,\"soNumber\":null,\"businessName\":\"中京电子HDI厂直销\",\"status\":\"生效\",\"approveStatus\":\"审批通过\",\"ifNew\":\"否\",\"ifForecast\":\"预测\",\"nonPlanned\":\"否\",\"qtyplanned\":0,\"qtyTobePlanned\":10,\"qtyUndo\":0,\"qtyForecasted\":0,\"qtyOrder\":10,\"qtyArray\":10,\"qtyAssigned\":0,\"qtyGift\":0,\"qtyShipped\":0,\"qtyRepaired\":0,\"qtyOrderReturned\":0,\"qtyReplenishment\":0,\"qtyReturned\":0,\"requiredDate\":\"2020-07-04 00:00:00\",\"marketDate\":\"2020-07-04 00:00:00\",\"planDate\":\"2020-07-04 00:00:00\",\"price\":39.99159835,\"customer\":\"0342b\",\"customerModel\":\"E.QS.LED.2347C(19281)\",\"productCode\":\"P2007041547309745399\",\"custProductCode\":\"004.001.0013197\",\"areaOrder\":0.26},{\"recId\":291235,\"itemRecId\":316055,\"soNumber\":null,\"businessName\":\"中京电子HDI厂直销\",\"status\":\"生效\",\"approveStatus\":\"审批通过\",\"ifNew\":\"否\",\"ifForecast\":\"预测\",\"nonPlanned\":\"否\",\"qtyplanned\":0,\"qtyTobePlanned\":10,\"qtyUndo\":0,\"qtyForecasted\":0,\"qtyOrder\":10,\"qtyArray\":10,\"qtyAssigned\":0,\"qtyGift\":0,\"qtyShipped\":0,\"qtyRepaired\":0,\"qtyOrderReturned\":0,\"qtyReplenishment\":0,\"qtyReturned\":0,\"requiredDate\":\"2020-07-11 00:00:00\",\"marketDate\":\"2020-07-14 00:00:00\",\"planDate\":\"2020-07-14 00:00:00\",\"price\":39.99159835,\"customer\":\"0342b\",\"customerModel\":\"E.QS.LED.2347C(19281)\",\"productCode\":null,\"custProductCode\":\"004.001.0013197\",\"areaOrder\":0.26}]},{\"recId\":291238,\"contractDate\":\"2020-07-06 00:00:00\",\"contractNo\":\"非寄售改寄售\",\"factoryCode\":\"惠州中京电子科技有限公司\",\"currency\":\"人民币\",\"taxRate\":\"0.1300\",\"exchangeRate\":1,\"contractItem\":[{\"recId\":291238,\"itemRecId\":316053,\"soNumber\":null,\"businessName\":\"中京电子多层厂直销\",\"status\":\"生效\",\"approveStatus\":\"审批通过\",\"ifNew\":\"否\",\"ifForecast\":\"预测\",\"nonPlanned\":\"否\",\"qtyplanned\":0,\"qtyTobePlanned\":30,\"qtyUndo\":0,\"qtyForecasted\":0,\"qtyOrder\":30,\"qtyArray\":6,\"qtyAssigned\":0,\"qtyGift\":0,\"qtyShipped\":0,\"qtyRepaired\":0,\"qtyOrderReturned\":0,\"qtyReplenishment\":0,\"qtyReturned\":0,\"requiredDate\":\"2020-07-06 00:00:00\",\"marketDate\":\"2020-07-06 00:00:00\",\"planDate\":\"2020-07-06 00:00:00\",\"price\":0,\"customer\":\"0841b\",\"customerModel\":\"RH500_HDMI in mode \",\"productCode\":\"P2007061039004423788\",\"custProductCode\":\"\",\"areaOrder\":0.05}]}]}";
                    var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate=20200729&endDate=20200729", headers);
                    SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                    var subResult = ParseResult(result) as SubOperateResult;
                    if (subResult.Flag == 0) return subResult;
                    var ja = JArray.Parse(subResult.Data.ToString());
                    var sum = ja.Count();
                    var dealData = SoapHttpHelper.PersistenceEntityData<FromOrder, FromOrderDetail>(subResult.Data.ToString(), userId, logId);
                    if (dealData == null || dealData.Count == 0) return subResult;
                    OperateResult dataResult = new OperateResult();
                    var db = new PostgreHelper();
                    var conn = db.GetDbConnect();
                    conn.Open();
                    DbTransaction trans = conn.BeginTransaction();
                    try
                    {
                        var entityId = typeof(FromOrder).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                        var subEntityId = typeof(FromOrderDetail).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                        bool isSkip = false;
                        foreach (var t in dealData)
                        {
                            object cust = null;
                            if (subEntityId != null)
                            {
                                var subDetail = t["orderdetail"] as List<Dictionary<string, object>>;
                                var newSubDetail = new List<Dictionary<string, object>>();
                                foreach (var t1 in subDetail)
                                {
                                    if (t1["customer"] == null) { isSkip = true; break; }
                                    if (t1["customer"] != null && t1["customer"].ToString() == "{}") { isSkip = true; break; } else isSkip = false;
                                    var buidlerDetail = new Dictionary<string, object>();
                                    buidlerDetail.Add("TypeId", subEntityId);
                                    buidlerDetail.Add("FieldData", t1);
                                    newSubDetail.Add(buidlerDetail);
                                    cust = t1["customer"];
                                }
                                t["orderdetail"] = JsonConvert.SerializeObject(newSubDetail);
                                var subDetailFirst = subDetail.FirstOrDefault();

                                //订购数量
                                if (!t.ContainsKey("quantity") && subDetailFirst.ContainsKey("quantity"))
                                    t.Add("quantity", subDetailFirst["quantity"]);
                                else
                                    t["quantity"] = subDetailFirst["quantity"];
                                //交货板数
                                if (!t.ContainsKey("deliveredquantity") && subDetailFirst.ContainsKey("deliveredquantity"))
                                    t.Add("deliveredquantity", subDetailFirst["deliveredquantity"]);
                                else
                                    t["deliveredquantity"] = subDetailFirst["deliveredquantity"];
                                //客户交期
                                if (!t.ContainsKey("requireddate") && subDetailFirst.ContainsKey("requireddate"))
                                    t.Add("requireddate", subDetailFirst["requireddate"]);
                                else
                                    t["requireddate"] = subDetailFirst["requireddate"];
                                if (!t.ContainsKey("marketdate") && subDetailFirst.ContainsKey("marketdate"))
                                    t.Add("marketdate", subDetailFirst["marketdate"]);
                                else
                                    t["marketdate"] = subDetailFirst["marketdate"];
                                if (!t.ContainsKey("plandate") && subDetailFirst.ContainsKey("planDate"))
                                    t.Add("plandate", subDetailFirst["planDate"]);
                                else
                                    t["plandate"] = subDetailFirst["plandate"];
                                if (!t.ContainsKey("price") && subDetailFirst.ContainsKey("price"))
                                    t.Add("price", subDetailFirst["price"]);
                                else
                                    t["price"] = subDetailFirst["price"];
                                if (!t.ContainsKey("customer") && subDetailFirst.ContainsKey("customer"))
                                    t.Add("customer", subDetailFirst["customer"]);
                                else
                                    t["customer"] = subDetailFirst["customer"];
                                if (!t.ContainsKey("customermodel") && subDetailFirst.ContainsKey("customermodel"))
                                    t.Add("customermodel", subDetailFirst["customermodel"]);
                                else
                                    t["customermodel"] = subDetailFirst["customermodel"];
                                if (!t.ContainsKey("productcode") && subDetailFirst.ContainsKey("product"))
                                    t.Add("productcode", subDetailFirst["product"]);
                                else
                                    t["productcode"] = subDetailFirst["product"];
                                if (!t.ContainsKey("custproductcode") && subDetailFirst.ContainsKey("custproductcode"))
                                    t.Add("custproductcode", subDetailFirst["custproductcode"]);
                                else
                                    t["custproductcode"] = subDetailFirst["custproductcode"];
                                if (!t.ContainsKey("orderstatus") && subDetailFirst.ContainsKey("status"))
                                    t.Add("orderstatus", subDetailFirst["status"]);
                                else
                                    t["orderstatus"] = subDetailFirst["status"];
                            }
                            if (isSkip || cust == null)
                            {
                             //   _dynamicEntityRepository.inertproductlog(trans, "订单", JsonConvert.SerializeObject(t), "没有客户", string.Empty, string.Empty, string.Empty, userId);
                                continue;
                            }
                            if (!t.ContainsKey("customer"))
                                t.Add("customer", cust);
                            var recid = _toERPRepository.IsExistsOrder(t["orderid"].ToString());
                            if (!string.IsNullOrEmpty(recid))
                            {
                                dataResult = _dynamicEntityRepository.DynamicEdit(trans, Guid.Parse(entityId), Guid.Parse(recid), t, t["reccreator"] != null ? Convert.ToInt32(t["reccreator"]) : userId);
                              //  _dynamicEntityRepository.inertproductlog(trans, "订单", JsonConvert.SerializeObject(t), "订单重复", string.Empty, string.Empty, string.Empty, userId);
                            }
                            else
                                dataResult = _dynamicEntityRepository.DynamicAdd(trans, Guid.Parse(entityId), t, null, t["reccreator"] != null ? Convert.ToInt32(t["reccreator"]) : userId);
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
        public OperateResult SyncErpPackingShip()
        {
            return this.FromErpPackingShip(null, "getPackingSlipList", "同步发货单", 1);
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
                var startDate = _toERPRepository.GetShippingOrderLastUpdatedTime();
                var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate=" + startDate + "&endDate=" + DateTime.Now.AddDays(1).ToString("yyyyMMdd"), headers);
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
        public OperateResult SyncErpPackingShipCost()
        {
            return this.FromErpPackingShipCost(null, "getPackingSlipCost", "同步发货单", 1);
        }
        /// <summary>
        /// 发货单
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="filterKey"></param>
        /// <param name="orignalName"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OperateResult FromErpPackingShipCost(IDictionary<string, object> detail, string filterKey, string orignalName, int userId)
        {
            string logId = string.Empty;
            try
            {
                var config = ValidConfig("PackingShipCostSoap", filterKey, orignalName);
                if (config.Flag == 0) return config;
                var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("token", AuthToLoginERP(userId));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl }, 0, userId).ToString();
                var result = "{\"code\": 200, 	\"message\": \"success\", 	\"data\": [{ 		\"packingSlipId\": 38469, 		\"packingSlipItemId\": 5597, 		\"shipingNotesNumer\": \"DP19042401996\", 		\"salesPartNum\": \"MN10016G061723A\", 		\"salesPartName\": \"deco M5 REV1.0.0|1.1\", 		\"starting\": \"2019-04-01 00:00:00\", 		\"ending\": \"2019-04-30 00:00:00\", 		\"averageCostByMonth\": 0.0001 	}] }";
                //HttpLib.Get(soapConfig.SoapUrl + "?startDate=20200501&endDate=20200513", headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                var subResult = ParseResult(result) as SubOperateResult;
                var dealData = SoapHttpHelper.PersistenceEntityData<FromPackingShipPrimeCost>(subResult.Data.ToString(), userId, logId);
                OperateResult dataResult = new OperateResult();
                var db = new PostgreHelper();
                var conn = db.GetDbConnect();
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    var entityId = typeof(FromPackingShipPrimeCost).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                    foreach (var t in dealData)
                    {
                        var recid = _toERPRepository.IsExistsPackingShipOrder(t["packingshipid"].ToString());
                        if (string.IsNullOrEmpty(recid)) continue;
                        var _dynamicEntityServices = ServiceLocator.Current.GetInstance<DynamicEntityServices>();
                        var data = _dynamicEntityServices.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
                        {
                            EntityId = Guid.Parse(entityId),
                            NeedPower = 0,
                            RecId = Guid.Parse(recid)
                        }, userId);
                        var subDetail = data["Detail"].FirstOrDefault()["detail"] as List<IDictionary<string, object>>;
                        Dictionary<string, object> dic = new Dictionary<string, object>();
                        List<Dictionary<string, object>> subDetailData = new List<Dictionary<string, object>>();
                        Dictionary<string, object> subData = new Dictionary<string, object>();
                        if (subDetail == null) continue;
                        foreach (var t1 in subDetail)
                        {
                            if (t1.ContainsKey("productcode"))
                            {
                                t1["productcode"] = t["productcode"];
                            }
                            if (t1.ContainsKey("productname"))
                            {
                                t1["productname"] = t["productname"];
                            }
                            if (t1.ContainsKey("cost"))
                            {
                                t1["cost"] = t["cost"];
                            }
                            subData.Add("FieldData", t1);
                            subData.Add("TypeId", "658159ab-9ace-405b-ae06-00619230aa92");
                            subDetailData.Add(subData);
                        }
                        dic.Add("starting", t["Starting"]);
                        dic.Add("ending", t["Ending"]);
                        dic.Add("detail", JsonConvert.SerializeObject(subDetailData));

                        dataResult = _dynamicEntityRepository.DynamicEdit(trans, Guid.Parse(entityId), Guid.Parse(recid), dic, userId);
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
                    SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message
}, isUpdate, userId, logId);
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
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message
}, isUpdate, userId, logId);
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
        public OperateResult FromErpMakeCollectionsOrder(IDictionary<string, object> detail, string filterKey, string orignalName, int userId)
        {
            string logId = string.Empty;
            try
            {
                var config = ValidConfig("MakeCollectionOrderSoap", filterKey, orignalName);
                if (config.Flag == 0) return config;
                var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("token", AuthToLoginERP(userId));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl }, 0, userId).ToString();
                var result = "{\"code\": 200,   \"message\": \"success\",   \"data\": [     {       \"recId\": 2,       \"checkNumber\": \"RV20051600001\",       \"sourceId\": 1068,       \"totalAmount\": 120,       \"checkDate\": \"2020-05-16 00:00:00\",       \"currencyId\": \"RMB\"     }   ] }";
                //HttpLib.Get(soapConfig.SoapUrl + "?startDate=20200501&endDate=20200513", headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                var subResult = ParseResult(result) as SubOperateResult;
                var dealData = SoapHttpHelper.PersistenceEntityData<FromMakeCollectionsOrder>(subResult.Data.ToString(), userId, logId);
                OperateResult dataResult = new OperateResult();
                var db = new PostgreHelper();
                var conn = db.GetDbConnect();
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    var entityId = typeof(FromMakeCollectionsOrder).GetCustomAttribute<EntityInfoAttribute>().EntityId;
                    foreach (var t in dealData)
                    {
                        var recid = _toERPRepository.IsExistsMakeCollectionOrder(t["makecollectionsorderid"].ToString());
                        if (!string.IsNullOrEmpty(recid))
                        {
                            dataResult = _dynamicEntityRepository.DynamicEdit(trans, Guid.Parse(entityId), Guid.Parse(recid), t, userId);
                        }
                        else
                        {
                            dataResult = _dynamicEntityRepository.DynamicAdd(trans, Guid.Parse(entityId), t, null, userId);
                        }
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
                    SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message
}, isUpdate, userId, logId);
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
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message
}, isUpdate, userId, logId);
                return new OperateResult { Flag = 0, Msg = ex.Message };
            }
        }

        public OperateResult ParseResult(string result)
        {
            JObject jObject = JObject.Parse(result);
            return new SubOperateResult { Flag = jObject["code"].ToString() == "200" ? 1 : 0, Msg = jObject["message"].ToString(), Data = jObject["data"].ToString() };
        }
        public string AuthToLoginERP(int userId, DbTransaction trans = null)
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
                var result = AuthErp(userId, trans);
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

        public OperateResult AuthErp(int userId, DbTransaction trans = null)
        {
            var result = ValidConfig("AuthSoap", "getToken", "登录");
            if (result.Flag == 0) return result;
            var interfaces = (result.Data as SoapInterfacesCollection).Interfaces;
            string body = string.Empty;
            var soapConfig = interfaces.FirstOrDefault();
            if (soapConfig != null && soapConfig.IsSingleParam == 0)
            {
                var logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { JsonConvert.SerializeObject(soapConfig.Params), soapConfig.SoapUrl }, 0, userId, trans: trans);
                var soapResult = HttpLib.Get(soapConfig.SoapUrl + string.Format("?userId={0}&password={1}", soapConfig.Params[0].DefaultValue, soapConfig.Params[1].DefaultValue));
                var subResult = ParseResult(soapResult) as SubOperateResult;
                return subResult;
            }
            return new SubOperateResult { Flag = 0, Msg = "校验账号配置异常" };
        }
        public class SubOperateResult : OperateResult
        {
            public object Data { get; set; }
        }
        public SubOperateResult ValidConfig(string key, string filterKey, string orignalName)
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
