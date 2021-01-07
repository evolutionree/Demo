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
        private readonly ProductsServices _productsServices;
        public ProductServices(ProductsServices productsServices, IProductsRepository iProductsRepository, IDynamicEntityRepository iDynamicEntityRepository, IBaseDataRepository iBaseDataRepository)
        {
            _iBaseDataRepository = iBaseDataRepository;
            _iDynamicEntityRepository = iDynamicEntityRepository;
            _iProductsRepository = iProductsRepository;
            _productsServices = productsServices;
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
                    String MATNR = t["MATNR"] != null ? t["MATNR"].ToString() : "";
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
                            foreach (var data in sapProduct.DATA.MARA)
                            {
                                String MAKTX = sapProduct.DATA.MAKT.FirstOrDefault(t2 => t2["MATNR"].ToString() == data["MATNR"].ToString())["MAKTX"];
                                if (!string.IsNullOrEmpty(MAKTX))
                                {
                                    dic.Add("productname", MAKTX);
                                    dic.Add("productdesciption", MAKTX);
                                    break;
                                }
                            }
                            _iDynamicEntityRepository.DynamicAdd(null, Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2"), dic, null, userId);
                            keys.Add(MATNR);
                        }
                        else
                        {
                            dic.Add("productcode", MATNR);
                            dic.Add("productsetid", setId);
                            dic.Add("worker", 1);
                            foreach (var data in sapProduct.DATA.MARA)
                            {
                                String MAKTX = sapProduct.DATA.MAKT.FirstOrDefault(t2 => t2["MATNR"].ToString() == data["MATNR"].ToString())["MAKTX"];
                                if (!string.IsNullOrEmpty(MAKTX))
                                {
                                    dic.Add("productname", MAKTX);
                                    dic.Add("productdesciption", MAKTX);
                                    break;
                                }
                            }
                              result = _iDynamicEntityRepository.DynamicEdit(null, Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2"), isExistProduct.recid, dic, userId);
                            keys.Add(MATNR);
                        }
                    }
                });
            }
            return new OutputResult<object>("同步成功");

        }
    }
}
