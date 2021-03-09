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

namespace UBeat.Crm.CoreApi.GL.Services
{
    public class ProductServices
    {
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
            var plist = _iSapProductsRepository.getProductInfoByIds(productIds) as List<IDictionary<string, object>>;

            if (plist != null && plist.Count > 0)
            {
                /*List<ZppCrm004> mats = new List<ZppCrm004>();
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

                    ZppCrm004 mat = new ZppCrm004();
                    mat.Matnr = productCode;
                    mats.Add(mat);
                }

                var remoteAddress = new System.ServiceModel.EndpointAddress(string.Format("{0}/{1}", SapServer,
                    string.Format("sap/bc/srt/rfc/sap/zmm_crm_001/{0}/zmm_crm_001_s/zmm_crm_001_s", SapClientId)));
                var binding = InitBind(remoteAddress);

                ZMM_CRM_001Client client = new ZMM_CRM_001Client(binding, remoteAddress);
                AuthBasic(client.ClientCredentials.UserName, client.Endpoint, 1);

                var crmdata = new ZmmCrm0011();
                crmdata.ItMatnr = mats.ToArray();
                crmdata.ItLgort = new ZmmCrm001[] { };
                ZmmCrm001Request request = new ZmmCrm001Request();
                request.ZmmCrm001 = crmdata;

                logger.Info(string.Concat("SAP产品库存接口请求参数：", JsonHelper.ToJson(request)));
                var ret = client.ZmmCrm001Async(request).Result;
                logger.Info(string.Concat("SAP返回产品库存接口数据：", JsonHelper.ToJson(ret)));
                if (ret != null && ret.ZmmCrm001Response != null && ret.ZmmCrm001Response.ItLgort != null)
                {
                    foreach (var item in ret.ZmmCrm001Response.ItLgort)
                    {
                        if (!string.IsNullOrEmpty(item.Sobkz)) continue;
                        if (dic.ContainsKey(item.Matnr))
                        {
                            //物料只有一个，但返回值有个仓库，不能直接取引用
                            ProductStockModel model = new ProductStockModel();
                            model.ProductId = dic[item.Matnr].ProductId;
                            model.ProductName = dic[item.Matnr].ProductName;
                            model.ProductRemark = dic[item.Matnr].ProductRemark;
                            model.ProductModel = dic[item.Matnr].ProductModel;

                            model.ProductCode = item.Matnr;
                            model.Factory = item.Werks;
                            model.StockAddress = item.Lgort;
                            model.Unit = item.Meins;
                            model.LABST = item.Labst;
                            model.SpeciFlag = item.Sobkz;

                            if (item.Labst > 0)
                                list.Add(model);
                        }
                    }
                }*/
            }

            return list;
        }
        #endregion
    }
}
