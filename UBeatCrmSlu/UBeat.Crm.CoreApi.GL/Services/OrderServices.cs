using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

using System.Text;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility;
using System.Linq;
using UBeat.Crm.LicenseCore;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.GL.Repository;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.GL.Utility;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using NLog;
using IOrderRepository = UBeat.Crm.CoreApi.GL.Repository.IOrderRepository;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.GL.Services
{
    public class OrderServices : BasicBaseServices
    {
        private readonly Logger logger = LogManager.GetLogger("UBeat.Crm.CoreApi.GL.Services.OrderServices");
        private readonly IConfigurationRoot _configurationRoot;
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly IBaseDataRepository _baseDataRepository;
        private readonly BaseDataServices _baseDataServices;
        private readonly IOrderRepository _orderRepository;
        private readonly CacheServices _cacheService;

        public OrderServices(IConfigurationRoot configurationRoot, DynamicEntityServices dynamicEntityServices, CacheServices cacheService,
            BaseDataServices baseDataServices, IBaseDataRepository baseDataRepository, IOrderRepository orderRepository)
        {
            _configurationRoot = configurationRoot;
            _orderRepository = orderRepository;
            _dynamicEntityServices = dynamicEntityServices;
            _baseDataRepository = baseDataRepository;
            _baseDataServices = baseDataServices;
            _cacheService = cacheService;
        }

        public OperateResult InitOrdersData()
        {
            var total = 0;
            DateTime startDate = new DateTime(2018, 9, 30, 0, 0, 0);
            while (startDate<= DateTime.Now.Date)
            {
                SoOrderParamModel param = new SoOrderParamModel();
                param.ERDAT_FR = startDate.ToString("yyyy-MM-dd");
                param.ERDAT_TO = startDate.ToString("yyyy-MM-dd");
                var c = this.getOrders(param);
                if (c.Status == 0)
                {
                    total += int.Parse(c.DataBody.ToString());
                }
                startDate =startDate.AddDays(1);
            }
            return new OperateResult
            {
                Flag = 1,
                Msg = string.Format(@"SAP订单已同步条数：{0}", total)
            };

        }

        public OperateResult IncremOrdersData()
        {
            var total = 0;
            SoOrderParamModel param = new SoOrderParamModel();
            param.ERDAT_FR = DateTime.Now.ToString("yyyy-MM-dd");
            param.ERDAT_TO = DateTime.Now.ToString("yyyy-MM-dd");
            var c = this.getOrders(param);
            if (c.Status == 0)
            {
                total += int.Parse(c.DataBody.ToString());
            }
            return new OperateResult
            {
                Flag = 1,
                Msg = string.Format(@"SAP订单已同步条数：{0}", total)
            };

        }

        public OutputResult<Object> getOrders(SoOrderParamModel param, int userId = 1)
        {
            var header = new Dictionary<String, string>();
            header.Add("Transaction_ID", "SO_LIST");
            var postData = new Dictionary<String, string>();
            postData.Add("REQDATE", "");
            postData.Add("ORDERID", "");
            postData.Add("ERDAT_FR", "");
            postData.Add("ERDAT_TO", "");
            if (!string.IsNullOrEmpty(param.ReqDate))
            {
                //查询单条
                postData["REQDATE"] = param.ReqDate;
            }
            if (!string.IsNullOrEmpty(param.OrderId))
            {
                //查询单条
                postData["ORDERID"] = param.OrderId;
            }
            else if (!string.IsNullOrEmpty(param.ERDAT_FR) && !string.IsNullOrEmpty(param.ERDAT_TO))
            {
                //查询时间段
                postData["ERDAT_FR"] = param.ERDAT_FR;
                postData["ERDAT_TO"] = param.ERDAT_TO;

            }
            logger.Info(string.Concat("获取SAP订单请求参数：", JsonHelper.ToJson(postData)));
            String result = CallAPIHelper.ApiPostData(postData, header);
            if (!string.IsNullOrEmpty(result))
            {
                var objResult = JsonConvert.DeserializeObject<SoOrderModel>(result);
                if (objResult.TYPE == "S")
                {
                    int syncCount = 0;
                    List<IGrouping<string, SoOrderDataModel>> groupData = new List<IGrouping<string, SoOrderDataModel>>();
                    var data = objResult.DATA["LIST"];
                    try
                    {
                        groupData = data.GroupBy(t => t.VBELN).ToList();
                        syncCount =saveOrders(data, userId);
                    }
                    catch (Exception ex)
                    {
                        logger.Info(string.Concat("获取销售订单列表失败：", ex.Message));
                    }
                    logger.Log(LogLevel.Info, $"获取销售订单列表成功,读取数：{ groupData.Count },处理数：{ syncCount}");
                    return new OutputResult<object>(syncCount, message: $"获取销售订单列表成功,读取数：{ groupData.Count },处理数：{ syncCount}");
                }
                else
                {
                    logger.Log(LogLevel.Error, $"获取SAP订单接口异常报错：{objResult.MESSAGE}");
                    return new OutputResult<object>(null, message: "获取销售订单列表失败", status: 1);
                }
            }
            return new OutputResult<object>(null, message: "获取销售订单列表失败", status: 1);
        }

        private int saveOrders(List<SoOrderDataModel> orders, int userId)
        {
            int insertcount = 0;
            var groupData = orders.GroupBy(t => t.VBELN).ToList();
            var allDicData = _baseDataRepository.GetDicData();
            var orderTypeDicData = allDicData.Where(t => t.DicTypeId == 69);
            var salesOrgDicData = allDicData.Where(t => t.DicTypeId == 63);
            var salesChannelDicData = allDicData.Where(t => t.DicTypeId == 62);
            var productDicData = allDicData.Where(t => t.DicTypeId == 65);
            var salesDeptDicData = allDicData.Where(t => t.DicTypeId == 64);

            var orderReasonDicData = allDicData.Where(t => t.DicTypeId == 60);

            var salesTerritoryDicData = allDicData.Where(t => t.DicTypeId == 67);
            var currencyDicData = allDicData.Where(t => t.DicTypeId == 54);
            var factoryDicData = allDicData.Where(t => t.DicTypeId == 66);
            var custData = _baseDataRepository.GetCustData();
            var contractData = _baseDataRepository.GetContractData();
            var userData = _baseDataRepository.GetUserData();
            var products = _baseDataRepository.GetProductData();
            var crmOrders = _baseDataRepository.GetOrderData();
            IDynamicEntityRepository _iDynamicEntityRepository = ServiceLocator.Current.GetInstance<IDynamicEntityRepository>();
            groupData.ForEach(t =>
            {
                string saporder = t.Key;
                try {
                    Dictionary<String, object> fieldData = new Dictionary<string, object>();
                    bool isAdd = false;
                    Guid recId = Guid.Empty;
                    var collection = orders.Where(p => p.VBELN == t.Key);

                    decimal totalamount = 0;
                    int sequencenumber = 0;

                    if (collection.Count() > 0)
                    {
                        var mainData = collection.FirstOrDefault();
                        var crmOrder = crmOrders.FirstOrDefault(t1 => t1.code == mainData.VBELN);
                        if (crmOrder == null)
                            isAdd = true;
                        else
                        {
                            if (crmOrder.datasources == 2) {
                                ///crm创建不处理
                                return;
                            }
                            fieldData.Add("recid", crmOrder.id);
                            recId = crmOrder.id;
                        }
                        fieldData.Add("orderid", mainData.VBELN);
                        var orderType = orderTypeDicData.FirstOrDefault(t1 => t1.ExtField1 == mainData.AUART);
                        fieldData.Add("ordertype", orderType == null ? 0 : orderType.DataId);
                        var salesOffices = salesOrgDicData.FirstOrDefault(t1 => t1.ExtField1 == mainData.VKORG);
                        fieldData.Add("salesoffices", salesOffices == null ? 0 : salesOffices.DataId);
                        var salesChannel = salesChannelDicData.FirstOrDefault(t1 => t1.ExtField1 == mainData.VTWEG);
                        fieldData.Add("distributionchanne", salesChannel == null ? 0 : salesChannel.DataId);
                        var product = productDicData.FirstOrDefault(t1 => t1.ExtField1 == mainData.SPART);
                        fieldData.Add("productteam", product == null ? 0 : product.DataId);
                        var salesDept = salesDeptDicData.FirstOrDefault(t1 => t1.ExtField1 == mainData.VKBUR);
                        fieldData.Add("salesdepartments", salesDept == null ? 0 : salesDept.DataId);

                        //负责人
                        var salerMan = userData.FirstOrDefault(t1 => t1.username == mainData.LNAME1);
                        fieldData.Add("recmanager", salerMan == null ? 1 : salerMan.userid);

                        var cust = custData.FirstOrDefault(t1 => t1.code == mainData.KUNNR);
                        fieldData.Add("customer", cust == null ? null : "{\"id\":\"" + cust.id.ToString() + "\",\"name\":\"" + cust.name + "\"}");
                        var contract = contractData.FirstOrDefault(t1 => t1.code == mainData.BSTKD);
                        fieldData.Add("contractcode", contract == null ? null : "{\"id\":\"" + contract.id.ToString() + "\",\"name\":\"" + contract.name + "\"}");

                        //fieldData.Add("deliveredamount", mainData.KUKLA);
                        //fieldData.Add("undeliveredamount", mainData.KUKLA);
                        //fieldData.Add("invoiceamount", mainData.KUKLA);
                        //fieldData.Add("uninvoiceamount", mainData.KUKLA);
                        fieldData.Add("custrefernum", mainData.BSTKD);//客户参考号
                        fieldData.Add("flowstatus", 3);//sap同步过来默认审核通过
                        fieldData.Add("issynchrosap", 1);//是否已同步
                        //sap创建
                        fieldData.Add("datasources", 1);
                        var orderReason = orderReasonDicData.FirstOrDefault(t1 => t1.ExtField1 == mainData.AUGRU);
                        fieldData.Add("orderreason", orderReason == null ? 0 : orderReason.DataId);

                        try
                        {
                            //订单销售日期
                            if (!string.IsNullOrEmpty(mainData.ERDAT) && mainData.ERDAT != "0000-00-00")
                            {
                                fieldData.Add("orderdate", DateTime.Parse(mainData.ERDAT));
                            }
                            if (!string.IsNullOrEmpty(mainData.VDATU1) && mainData.VDATU1 != "0000-00-00")
                            {
                                fieldData.Add("deliverydate", DateTime.Parse(mainData.VDATU1));
                            }
                        }
                        catch (Exception ex1)
                        {
                            throw new Exception("订单转换日期异常");
                        }
                    }
                    List<Dictionary<String, object>> listDetail = new List<Dictionary<string, object>>();
                    collection.ToList().ForEach(t1 =>
                    {
                        sequencenumber++;
                        Dictionary<String, object> dicDetail = new Dictionary<string, object>();
                        Dictionary<String, object> dicFieldData = new Dictionary<string, object>();
                        dicDetail.Add("TypeId", "a1010450-2c42-423f-a248-55433b706581");
                        var product = products.FirstOrDefault(t2 => t2.productcode == t1.MATNR.Substring(8));
                        dicFieldData.Add("productname", product == null ? "" : product.productid.ToString());

                        dicFieldData.Add("productunit", 1); //t1.KMEIN2
                        dicFieldData.Add("quantity", t1.KWMENG);
                        dicFieldData.Add("totalmoney", t1.KZWI2);


                        dicFieldData.Add("productcode", product == null ? "" : product.productcode.ToString());
                        dicFieldData.Add("totalnetweight", t1.KWMENG);
                        dicFieldData.Add("packingway", t1.ZBZFS);
                        dicFieldData.Add("waterglaze", t1.ZBINGYI);
                        dicFieldData.Add("branchesnumber", t1.ZTIAOSHU);
                        dicFieldData.Add("specification", t1.ZGUIGE);
                        dicFieldData.Add("price", t1.ZHSDJKG);
                        //sap创建
                        dicFieldData.Add("datasource", 1);
                        dicFieldData.Add("synchronoustatus", 1);

                        var currency = currencyDicData.FirstOrDefault(t2 => t2.ExtField1 == t1.SPART);
                        dicFieldData.Add("currency", currency == null ? 0 : currency.DataId);

                        var factory = factoryDicData.FirstOrDefault(t2 => t2.ExtField1 == t1.WERKS);
                        dicFieldData.Add("factory", factory == null ? 0 : factory.DataId);
                        dicFieldData.Add("linenumber", sequencenumber);

                        dicFieldData.Add("rownum", t1.POSNR);

                        totalamount += t1.KZWI2;

                        dicDetail.Add("FieldData", dicFieldData);
                        listDetail.Add(dicDetail);
                    });
                    fieldData.Add("totalamount", totalamount);
                    fieldData.Add("orderdetail", JsonConvert.SerializeObject(listDetail));
                    // fieldData.Add("totalweight",)
                    OperateResult result;
                    if (isAdd)
                        result = _iDynamicEntityRepository.DynamicAdd(null, Guid.Parse("6f12d7b0-9666-4f36-a9b4-cd9ca8117794"), fieldData, null, userId);
                    else
                        result = _iDynamicEntityRepository.DynamicEdit(null, Guid.Parse("6f12d7b0-9666-4f36-a9b4-cd9ca8117794"), recId, fieldData, userId);
                    insertcount++;
                }
                catch (Exception ex) {
                    logger.Info(string.Concat("获取销售订单列表保存失败："+ saporder+":", ex.Message));
                }
            });
            return insertcount;
        }

        #region -----订单创建、修改-----

        public SynResultModel SynSapOrderDataByHttp(Guid entityId, Guid recId, int UserId)
        {
            var sapResult = string.Empty;
            var optResult = new SynResultModel();
            if (UserId == 0)
                UserId = 1;
            optResult = this.SynSapOrderData(entityId, recId, UserId);
            return optResult;
        }
        public SynResultModel SynSapOrderData(Guid entityId, Guid recId, int UserId, DbTransaction tran = null)
        {
            var result = new SynResultModel();
            var detailData = _baseDataServices.GetEntityDetailData(tran, entityId, recId, UserId);
            if (detailData != null)
            {
                SynchrosapStatus isSyn = SynchrosapStatus.Yes;
                var sapno = string.Concat(detailData["orderid"]);
                var issynchrosap = string.Concat(detailData["issynchrosap"]);
                if (!string.IsNullOrEmpty(sapno) && (issynchrosap == "1" || issynchrosap == "4"))
                    isSyn = SynchrosapStatus.No;
                try
                {
                    if (isSyn == SynchrosapStatus.Yes)
                    {
                        result = SynSapAddOrderData(detailData, entityId, recId, UserId, tran);
                    }
                    else
                    {
                        result = SynSapAddOrderData(detailData, entityId, recId, UserId, tran);
                        //result = SynSapModifyOrderData(detailData, entityId, recId, UserId, tran);
                    }
                }
                catch (Exception ex)
                {
                    var str = "同步订单失败，请联系管理员";
                    logger.Info(string.Format(@"{0},{1}", str, ex.Message));
                    result.Message = str;
                }
            }
            else
            {
                result.Message = "同步失败，不存在订单记录";
            }

            return result;
        }

        private bool TryToGetActionLock(string actionKey)
        {
            bool IsGotKey = true;
            try
            {
                if (_cacheService.Repository.Exists(actionKey))
                {
                    string dt = _cacheService.Repository.Get<string>(actionKey);
                    //判断是否超时,如果超时了也算
                    if (dt != null && dt.Length >= 19)
                    {
                        dt = dt.Substring(0, 19);
                        DateTime t = DateTime.Parse(dt);
                        if ((System.DateTime.Now - t).TotalMinutes < 2.0)
                        {
                            IsGotKey = false;
                        }
                        else
                        {
                            _cacheService.Repository.Remove(actionKey);
                        }
                    }
                    else
                    {
                        _cacheService.Repository.Remove(actionKey);
                    }

                }
                if (IsGotKey == false)
                {
                    return false;
                }
                string sVal = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _cacheService.Repository.Add(actionKey, sVal);
                return true;
            }
            catch (Exception ex)
            {
            }
            return false;

        }
        private bool TryToReleaseActionLock(string actionKey)
        {
            try
            {
                _cacheService.Repository.Remove(actionKey);
            }
            catch (Exception ex)
            {

            }
            return true;
        }

        public SynResultModel SynSapAddOrderData(IDictionary<string, object> resultData, Guid entityId, Guid recId, Int32 userId, DbTransaction tran = null)
        {
            var serviceTime = DateTime.Now;
            var result = new SynResultModel();
            var sapResult = string.Empty;
            var lineDic = new Dictionary<string, string>();
            #region  读取并写入redis，作为唯一键
            string actionKey = "OrderServices_SynSapAddOrderData_" + recId.ToString();
            Console.WriteLine("正在获取锁");
            bool isLock = this.TryToGetActionLock(actionKey);
            if (isLock == false)
            {
                sapResult = "该订单正在同步，勿重复提交(Case 1)";
                result.Message = sapResult;
                return result;
            }
            #endregion
            List<SoOrderDataModel> soOrderList = new List<SoOrderDataModel>();

            try
            {
                #region detailitem
                dynamic linkDetail = resultData["orderdetail"];
                if (linkDetail != null)
                {
                    foreach (IDictionary<string, object> itemData in linkDetail)
                    {
                        SoOrderDataModel order = new SoOrderDataModel();
                        #region main
                        //ERNAM, AUART,BSTNK, KUNNR1, KUNNR2, KUNNR,WAERK
                        var recmanager = string.Concat(resultData["recmanager"]);//销售代表id 
                        var salerMan = _baseDataRepository.GetUserDataById(int.Parse(recmanager));
                        order.ERNAM = salerMan.username;//创建者账号
                        var auart = string.Concat(resultData["ordertype"]);//销售凭证类型
                        order.AUART = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.订单类型, auart).StringMax(0, 4);
                        order.BSTNK = string.Concat(resultData["reccode"]).StringMax(0, 20);//客户参考号
                        var customer = string.Concat(resultData["customer"]);//售达方编码
                        var kunnr=_baseDataRepository.GetCustomerCodeByDataSource(customer).StringMax(0, 10);//
                        order.KUNNR1 = kunnr;
                        var deliverycode = string.Concat(resultData["deliverycode"]);//送达方编码
                        var kunnr2 = _baseDataRepository.GetCustomerCodeByDataSource(deliverycode).StringMax(0, 10);//售达方编码
                        order.KUNNR2 = kunnr2;
                        order.KUNNR = kunnr;//付款方编码
                        order.WAERK = "CNY";//销售和分销凭证货币 默认CNY

                        //VKORG, VTWEG,SPART, VKBUR, VKGRP, LIFNR_YWY,AUGRU
                        var vkorg = string.Concat(resultData["salesoffices"]);//销售组织
                        order.VKORG = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.销售组织, vkorg);
                        var vtweg = string.Concat(resultData["distributionchanne"]);//分销渠道
                        order.VTWEG = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.分销渠道, vtweg);
                        var spart = string.Concat(resultData["productteam"]);//产品组
                        order.SPART = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.产品组, spart);
                        var vkbur = string.Concat(resultData["salesdepartments"]);//销售办事处
                        order.VKBUR = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.销售办事处, vkbur);
                        order.VKGRP = "";//销售组
                        order.LIFNR_YWY = salerMan.workcode;//业务员编号
                        var augru = string.Concat(resultData["orderreason"]);//订单原因（业务交易原因）
                        order.AUGRU = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.订单原因, augru);

                        //KOSTL,LIFNR_HYDL, AUDAT, VDATU, PRSDT
                        order.KOSTL = "";
                        order.LIFNR_HYDL = "";
                        order.AUDAT = DateTime.Now.ToString("yyyyMMdd");
                        var deliverydate = string.Concat(resultData["deliverydate"]);//请求交货日期
                        order.VDATU = deliverydate == null ? DateTime.Now.ToString("yyyyMMdd") : DateTime.Parse(deliverydate).ToString("yyyyMMdd");
                        order.PRSDT = "00000000";


                        #endregion

                        //MATNR, PSTYV,POSNR, KSCHL, KBETR_ZPSG, KPEIN,KMEIN
                        order.MATNR = string.Concat(itemData["productcode"]).StringMax(0, 40);//物料编号
                        order.PSTYV = "TAN";//项目类别,默认TAN
                        var posnr = string.Concat(itemData["rownum"]).StringMax(0, 6);//行号
                        if (!string.IsNullOrEmpty(posnr))
                        {
                            posnr = posnr.PadLeft(6, '0');
                            order.POSNR = int.Parse(posnr);
                        }
                        order.KSCHL = "ZPC1";//价格类型 默认
                        var price = string.Concat(itemData["price"]).StringMax(0, 15);//含税单价
                        decimal price_decimal = 0;
                        if (decimal.TryParse(price, out price_decimal))
                        {
                            order.KBETR_ZPSG = decimal.Round(price_decimal, 3);
                        }
                        order.KPEIN = 1;//定价单位 默认1
                        order.KMEIN = "KG";//计量单位 默认KG


                        //KWMENG, VRKME,EDATU, NTGEW, GEWEI, ZBZFS
                        var totalnetweight = string.Concat(itemData["totalnetweight"]).StringMax(0, 15);//订单数量
                        decimal totalnetweight_decimal = 0;
                        if (decimal.TryParse(totalnetweight, out totalnetweight_decimal))
                        {
                            order.KWMENG = decimal.Round(totalnetweight_decimal, 3);
                        }
                        order.VRKME = "KG";//销售单位
                        order.EDATU = "";//计划行日期
                        order.NTGEW = 0;//净重
                        order.GEWEI = "KG";//净重单位
                        order.ZBZFS = "";//包装方式
                                         //KBETR_GWYF, KBETR_GWBX,KBETR_YJ, WERKS, LGORT, ABGRU
                        order.KBETR_GWYF = 0;//国外运费预估
                        order.KBETR_GWBX = 0;//国外保险费预估
                        order.KBETR_YJ = 0;//佣金
                        var werks = string.Concat(itemData["factory"]);//工厂
                        order.WERKS = _baseDataRepository.GetSapCodeByTypeIdAndId((int)DicTypeEnum.工厂, werks).StringMax(0, 4);
                        order.ABGRU = "";//拒绝原因，行关闭使用
                        soOrderList.Add(order);
                    }
                }
                #endregion
                var logTime = DateTime.Now;
                var postData = new Dictionary<string, object>();
                var headData = new Dictionary<string, string>();
                headData.Add("Transaction_ID", "SO_CREATE");

                postData.Add("LIST", soOrderList);

                logger.Info(string.Concat("SAP订单创建接口请求参数：", JsonHelper.ToJson(postData)));
                var postResult = CallAPIHelper.ApiPostData(postData, headData);
                SapOrderCreateModelResult sapRequest = JsonConvert.DeserializeObject<SapOrderCreateModelResult>(postResult);

                if (sapRequest.TYPE == "S")
                {
                    var sapCode = sapRequest.VBELN;
                    sapResult = sapRequest.MESSAGE;
                    result.Result = true;
                    _baseDataRepository.UpdateSynStatus(entityId, recId, (int)SynchrosapStatus.Yes, tran);
                    _orderRepository.UpdateOrderSapCode(recId, sapCode, lineDic, tran);
                    if (!string.IsNullOrEmpty(sapResult))
                        sapResult = string.Format(@"同步创建SAP订单成功，返回SAP订单号：{0}，SAP提示返回：{1}", sapCode, sapResult);
                    else
                        sapResult = string.Format(@"同步创建SAP订单成功，返回SAP订单号：{0}", sapCode);
                    result.Message = sapResult;
                    _baseDataRepository.UpdateSynTipMsg(entityId, recId, sapResult, tran);
                }
                else
                {
                    logger.Log(NLog.LogLevel.Error, $"创建SAP订单接口异常报错：{sapRequest.MESSAGE}");
                    sapResult = sapRequest.MESSAGE;
                    if (!string.IsNullOrEmpty(sapResult))
                    {
                        sapResult = string.Concat("同步创建SA订单失败，SAP错误返回：", sapResult);
                    }
                    else
                    {
                        sapResult = "同步创建SAP订单失败，SAP返回无订单号";
                    }
                    result.Message = sapResult;
                    _baseDataRepository.UpdateSynTipMsg2(entityId, recId, sapResult, tran);
                }
            }
            catch (Exception ex)
            {
                sapResult = string.Concat("同步创建SAP订单失败，异常错误，请查看日志：", sapResult);
                logger.Info(string.Concat("同步创建SAP订单失败，异常错误：", ex.Message));
            }
            finally
            {
                if (isLock)
                {
                    TryToReleaseActionLock(actionKey);
                }
            }

            return result;
        }
        #endregion
    }
}
