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


namespace UBeat.Crm.CoreApi.GL.Services
{
    public class OrderServices : BasicBaseServices
    {
        private readonly Logger logger = LogManager.GetLogger("UBeat.Crm.CoreApi.GL.Services.OrderServices");
        private readonly IConfigurationRoot _configurationRoot;
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly IBaseDataRepository _baseDataRepository;

        public OrderServices(IConfigurationRoot configurationRoot, DynamicEntityServices dynamicEntityServices, IBaseDataRepository baseDataRepository)
        {
            _configurationRoot = configurationRoot;
            _dynamicEntityServices = dynamicEntityServices;
            _baseDataRepository = baseDataRepository;
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

                        fieldData.Add("flowstatus", 3);//sap同步过来默认审核通过
                        fieldData.Add("ifsap", 1);//是否已同步
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

                        dicFieldData.Add("price", t1.ZHSDJ);
                        dicFieldData.Add("productunit", 1); //t1.KMEIN2
                        dicFieldData.Add("quantity", t1.KWMENG);
                        dicFieldData.Add("subtotal", t1.KZWI2);


                        dicFieldData.Add("productcode", product == null ? "" : product.productcode.ToString());
                        dicFieldData.Add("kgnumber", t1.KWMENG);
                        dicFieldData.Add("packingway", t1.ZBZFS);
                        dicFieldData.Add("waterglaze", t1.ZBINGYI);
                        dicFieldData.Add("branchesnumber", t1.ZTIAOSHU);
                        dicFieldData.Add("specification", t1.ZGUIGE);
                        dicFieldData.Add("kgunitprice", t1.ZHSDJKG);
                        //sap创建
                        dicFieldData.Add("datasource", 1);
                        dicFieldData.Add("ifsap", 1);

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
 
    }
}
