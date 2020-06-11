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
        private bool IsRuning = false;
        public SoapServices()
        {
            _configurationRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            _dynamicEntityRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            _toERPRepository = ServiceLocator.Current.GetInstance<IToERPRepository>();
        }

        public OperateResult SyncEntityDataAfterApproved(Guid entityId, Guid caseId, Guid recId, int userId, DbTransaction trans = null)
        {
            IDictionary<string, object> detailData;
            var detail = _dynamicEntityRepository.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
            {
                RecId = recId,
                EntityId = entityId
            }, userId, trans);
            if (entityId == Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246"))
            {
                detailData = detail;
            }
            else
            {
                detailData = _dynamicEntityRepository.Detail(new DomainModel.DynamicEntity.DynamicEntityDetailtMapper
                {
                    RecId = Guid.Parse(JObject.Parse(detail["belongcust"].ToString())["id"].ToString()),
                    EntityId = Guid.Parse("f9db9d79-e94b-4678-a5cc-aa6e281c1246")
                }, userId, trans);
            }
            if (detailData["custype"] == null || detailData["custype"].ToString() == "1") return new OperateResult { Flag = 1, Msg = String.Empty };
            DomainModel.OperateResult result;
            Dictionary<string, object> dic = new Dictionary<string, object>();
            if (detailData["pkcode"] == null)
            {
                result = this.ToErpCustomer(detailData, "saveCustomerFromCrm", "新增客户", userId, trans);
            }
            else
                result = this.ToErpCustomer(detailData, "updateCustomerFromCrm", "编辑客户", userId, trans);
            dic.Add("ifsyn", result.Flag == 1 ? new Nullable<int>(1) : 2);
            dic.Add("syncinfo", result.Flag == 1 ? "成功" : result.Msg);
            var synTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var synId = string.Empty;
            var data = (result as SubOperateResult).Data;
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
        public OperateResult ToErpCustomer(IDictionary<string, object> detail, string filterKey, string orignalName, int userId, DbTransaction trans = null)
        {
            var _dynamicEntityServices = ServiceLocator.Current.GetInstance<DynamicEntityServices>();
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
                headers.Add("token", AuthToLoginERP(userId, trans));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl", "soapreqstatus" }, new List<string> { param, soapConfig.SoapUrl, "请求成功" }, 0, userId, trans: trans).ToString();
                var result = HttpLib.Post(soapConfig.SoapUrl, param, headers);
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
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { string.Empty, ex.Message }, isUpdate, userId, logId, trans: trans);
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
                var config = ValidConfig("ProductSoap", filterKey, orignalName);
                if (config.Flag == 0) return config;
                var productRepository = ServiceLocator.Current.GetInstance<IProductsRepository>();
                var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("token", AuthToLoginERP(userId));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl }, 0, userId).ToString();
                var startDate = productRepository.GetProductLastUpdatedTime(null, userId);
                var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate=" + startDate + "&endDate=" + DateTime.Now.AddDays(1).ToString("yyyyMMdd"), headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
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
                var config = ValidConfig("OrderSoap", filterKey, orignalName);
                if (config.Flag == 0) return config;
                var interfaces = (config.Data as SoapInterfacesCollection).Interfaces;
                var soapConfig = interfaces.FirstOrDefault(t => t.FunctionName == filterKey);
                WebHeaderCollection headers = new WebHeaderCollection();
                headers.Add("token", AuthToLoginERP(userId));
                logId = SoapHttpHelper.Log(new List<string> { "soapparam", "soapurl" }, new List<string> { string.Empty, soapConfig.SoapUrl }, 0, userId).ToString();
                var startDate = _toERPRepository.GetOrderLastUpdatedTime();
                //  var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate=" + startDate + "&endDate=" + DateTime.Now.AddDays(1).ToString("yyyyMMdd"), headers);
                var result = HttpLib.Get(soapConfig.SoapUrl + "?startDate=20190101&endDate=20190202", headers);
                SoapHttpHelper.Log(new List<string> { "soapresresult", "soapexceptionmsg" }, new List<string> { result, string.Empty }, 1, userId, logId.ToString());
                var subResult = ParseResult(result) as SubOperateResult;
                if (subResult.Flag == 0) return subResult;
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

        OperateResult ParseResult(string result)
        {
            JObject jObject = JObject.Parse(result);
            return new SubOperateResult { Flag = jObject["code"].ToString() == "200" ? 1 : 0, Msg = jObject["message"].ToString(), Data = jObject["data"].ToString() };
        }
        string AuthToLoginERP(int userId, DbTransaction trans = null)
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
