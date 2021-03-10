using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.GL.Repository;
using System.Linq;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Products;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using NLog;
using UBeat.Crm.CoreApi.GL.Utility;
using Newtonsoft.Json;

namespace UBeat.Crm.CoreApi.GL.Services
{
    public class ProductServices
    {
        private readonly Logger logger = LogManager.GetLogger("UBeat.Crm.CoreApi.GL.Services.ProductServices");
        private readonly IBaseDataRepository _iBaseDataRepository;
        private readonly IDynamicEntityRepository _iDynamicEntityRepository;
        private readonly IProductsRepository _iProductsRepository;
        private readonly ISapProductRepository _iSapProductsRepository;
        private readonly ProductsServices _productsServices;
        public ProductServices(ProductsServices productsServices, IProductsRepository iProductsRepository, ISapProductRepository iSapProductsRepository, IDynamicEntityRepository iDynamicEntityRepository, IBaseDataRepository iBaseDataRepository)
        {
            _iBaseDataRepository = iBaseDataRepository;
            _iDynamicEntityRepository = iDynamicEntityRepository;
            _iProductsRepository = iProductsRepository;
            _productsServices = productsServices;
            _iSapProductsRepository = iSapProductsRepository;
        }


        public OutputResult<object> sapProductToCrmProduct(SAPProductInParam sapProduct, int userId)
        {
            //MATNR
            if (sapProduct != null && sapProduct.DATA != null)
            {
                List<String> keys = new List<string>();
                Dictionary<String, String> series = new Dictionary<String, String>();
                OperateResult result = new OperateResult();
                Guid topSetId = Guid.Parse("7f74192d-b937-403f-ac2a-8be34714278b");
                Guid setId = Guid.Parse("7f74192d-b937-403f-ac2a-8be34714278b");

                sapProduct.DATA.MARA.ForEach(t =>
                {
                    Dictionary<string, object> dic = new Dictionary<string, object>();
                    String MATNR = t["MATNR"] != null ? t["MATNR"].ToString().Substring(8) : "";
                    if (t["SPART"] != null)
                    {
                        List<SaveDicData> dicDatas = _iBaseDataRepository.GetDicDataByTypeId(65);
                        var dicData = dicDatas.FirstOrDefault(t1 => t1.ExtField1 == t["SPART"].ToString());
                        if (dicData != null)
                        {
                            var isExistProductSeries = _iProductsRepository.IsExistsProductSeries(null, new ProductSeriesEdit
                            {
                                SeriesCode = t["SPART"].ToString()
                            }, userId);
                            if (isExistProductSeries != null)
                            {
                                result = _iProductsRepository.EditProductSeries(null, new ProductSeriesEdit
                                {
                                    ProductsetId = Guid.Parse(isExistProductSeries.ToString()),
                                    SeriesCode = t["SPART"].ToString(),
                                    SeriesName = dicData.DataVal,
                                }, userId);
                                setId = Guid.Parse(isExistProductSeries.ToString());
                            }
                            else
                            {
                                ProductSeriesInsert crmData;
                                result = _iProductsRepository.AddProductSeries(null, new ProductSeriesInsert
                                {
                                    SeriesCode = t["SPART"].ToString(),
                                    TopSeriesId = topSetId,
                                    SeriesName = dicData.DataVal,
                                }, userId);
                                setId = result.Flag == 1 ? Guid.Parse(result.Id) : topSetId;
                                if (setId == topSetId)
                                {
                                    crmData = new ProductSeriesInsert()
                                    {
                                        TopSeriesId = setId,
                                        SeriesName = t["MATKL"],
                                        SeriesCode = t["EKWSL"],
                                        SeriesLanguage = null
                                    };
                                    result = _iProductsRepository.AddProductSeries(null, crmData, userId);
                                }
                            }
                        }
                    }
                    if (!keys.Contains(MATNR))
                    {
                        var isExistProduct = _productsServices.IsExitProduct(MATNR, userId);
                        if (isExistProduct == null)
                        {
                            dic.Add("productcode", MATNR);
                            dic.Add("productsetid", setId);
                            dic.Add("worker", 1);
                            String MAKTX = sapProduct.DATA.MAKT.FirstOrDefault(t2 => t2["MATNR"].ToString() == t["MATNR"].ToString())["MAKTX"];
                            if (!string.IsNullOrEmpty(MAKTX))
                            {
                                dic.Add("productname", MAKTX);
                                dic.Add("productdesciption", MAKTX);
                            }
                            _iDynamicEntityRepository.DynamicAdd(null, Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2"), dic, null, userId);
                            keys.Add(MATNR);
                        }
                        else
                        {
                            dic.Add("productcode", MATNR);
                            dic.Add("productsetid", setId);
                            dic.Add("worker", 1);
                            String MAKTX = sapProduct.DATA.MAKT.FirstOrDefault(t2 => t2["MATNR"].ToString() == t["MATNR"].ToString())["MAKTX"];
                            if (!string.IsNullOrEmpty(MAKTX))
                            {
                                dic.Add("productname", MAKTX);
                                dic.Add("productdesciption", MAKTX);
                            }
                            result = _iDynamicEntityRepository.DynamicEdit(null, Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2"), isExistProduct.recid, dic, userId);
                            keys.Add(MATNR);
                        }
                    }
                });
            }
            return new OutputResult<object>("同步成功");

        }

        #region 产品库存 
        public dynamic GetProductStockByIds(List<Guid> productIds)
        {
            var list = new List<ProductStockModel>();
            var dic = new Dictionary<string, ProductStockModel>();
            try {
                if (productIds.Count == 0)
                    return dic;
                var plist = _iSapProductsRepository.getProductInfoByIds(productIds) as List<IDictionary<string, object>>;


                List<ProductStockRequest> stockReqList = new List<ProductStockRequest>();
                if (plist != null && plist.Count > 0)
                {
                    foreach (var p in plist)
                    {
                        string productId = string.Concat(p["recid"]);
                        string productCode = string.Concat(p["productcode"]);
                        string productName = string.Concat(p["productname"]);
                        string productRemark = string.Concat(p["productdesciption"]);
                        string productModel = string.Concat(p["productmodel"]);

                        ProductStockModel model = new ProductStockModel();
                        model.ProductId = Guid.Parse(productId);
                        model.ProductCode = productCode;
                        model.ProductName = productName;
                        model.ProductRemark = productRemark;
                        model.ProductModel = productModel;

                        if (!dic.ContainsKey(productCode))
                            dic.Add(productCode, model);

                        //请求参数
                        ProductStockRequest stockRequest = new ProductStockRequest();
                        stockRequest.MATNR = productCode;
                        stockReqList.Add(stockRequest);
                    }

                    var headData = new Dictionary<String, string>();
                    headData.Add("Transaction_ID", "MATERIAL_STOCK");
                    var postData = new Dictionary<String, object>();

                    postData.Add("LIST", stockReqList);

                    logger.Info(string.Concat("获取物料库存请求参数：", JsonHelper.ToJson(postData)));
                    var postResult = CallAPIHelper.ApiPostData(postData, headData);
                    SapStockModelResult sapRequest = JsonConvert.DeserializeObject<SapStockModelResult>(postResult);

                    if (sapRequest.TYPE == "S")
                    {
                        var data = sapRequest.DATA["LIST"];
                        foreach (var item in data)
                        {
                            item.MATNR = int.Parse(item.MATNR).ToString();//去掉0
                            if (dic.ContainsKey(item.MATNR))
                            {
                                //物料只有一个，但返回值有个仓库，不能直接取引用
                                ProductStockModel model = new ProductStockModel();
                                model.ProductId = dic[item.MATNR].ProductId;
                                model.ProductName = dic[item.MATNR].ProductName;
                                model.ProductRemark = dic[item.MATNR].ProductRemark;
                                model.ProductModel = dic[item.MATNR].ProductModel;

                                model.ProductCode = item.MATNR;
                                model.Factory = item.MATNR;
                                model.StockAddress = item.LGORT;
                                model.Unit = item.MEINS;
                                model.enableSapStock = item.LABST;

                                if (item.LABST > 0)
                                    list.Add(model);
                            }
                        }

                    }
                }
            }
            catch (Exception ex) {
                logger.Log(NLog.LogLevel.Error, $"获取物料库存异常：{ex.Message}");
            }

            return list;
        }
        #endregion
    }
}
