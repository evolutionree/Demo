﻿using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Products;
using UBeat.Crm.CoreApi.DomainModel.Products;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using Microsoft.International.Converters.PinYinConverter;
using System.Data.Common;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Utility;
using Dapper;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ProductsServices : BasicBaseServices
    {
        private IProductsRepository _repository;
        private DynamicEntityServices _dynamicEntityServices;
        private IEntityProRepository _entityProRepository;

        private readonly Guid productEntityId = new Guid("59cf141c-4d74-44da-bca8-3ccf8582a1f2");//固定值



        #region  国联新增对接接口方法
        public ProductModel IsExitProduct(String productCode, int userId)
        {
            var sql = @"select *  from crm_sys_product where productcode=@productcode limit 1;";
            var param = new DynamicParameters();
            param.Add("productcode", productCode);
            return DataBaseHelper.QuerySingle<ProductModel>(sql, param);
        }
        #endregion

        public ProductsServices(IProductsRepository repository,
            DynamicEntityServices dynamicEntityServices,
            IEntityProRepository entityProRepository)
        {
            _repository = repository;
            _dynamicEntityServices = dynamicEntityServices;
            _entityProRepository = entityProRepository;
        }


        /// <summary>
        /// 添加产品系列
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> AddProductSeries(ProductSeriesAddModel body, int userNumber)
        {
            var crmData = new ProductSeriesInsert()
            {
                TopSeriesId = body.TopSeriesId,
                SeriesName = body.SeriesName,
                SeriesCode = body.SeriesCode,
                SeriesLanguage = body.SeriesLanguage
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            var res = ExcuteAction((transaction, arg, userData) =>
            {

                return HandleResult(_repository.AddProductSeries(transaction, crmData, userNumber));

            }, body, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;
        }




        /// <summary>
        /// 编辑产品系列
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> EditProductSeries(ProductSeriesEditModel body, int userNumber)
        {
            var crmData = new ProductSeriesEdit()
            {
                ProductsetId = body.ProductsetId,
                SeriesName = body.SeriesName,
                SeriesCode = body.SeriesCode,
                SeriesLanguage = body.SeroesLanguage
            };

            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            var res = ExcuteAction((transaction, arg, userData) =>
            {

                return HandleResult(_repository.EditProductSeries(transaction, crmData, userNumber));

            }, body, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;
        }


        /// <summary>
        /// 删除产品系列
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> DeleteProductSeries(ProductSeriesDeleteModel body, int userNumber)
        {


            var res = ExcuteAction((transaction, arg, userData) =>
            {
                return HandleResult(_repository.DeleteProductSeries(transaction, body.ProductsetId, userNumber));
            }, body, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;

        }

        //启用产品系列
        public OutputResult<object> ToEnableProductSeries(ProductSeriesDeleteModel body, int userNumber)
        {


            var res = ExcuteAction((transaction, arg, userData) =>
            {
                return HandleResult(_repository.ToEnableProductSeries(transaction, body.ProductsetId, userNumber));
            }, body, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;

        }


        /// <summary>
        /// 根据id获取产品系列详情，不包含子系列
        /// </summary>
        /// <param name="productsetId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> GetProductSeriesDetail(Guid productsetId, int userId)
        {
            var res = ExcuteAction((transaction, arg, userData) =>
            {
                return new OutputResult<object>(_repository.GetProductSeriesDetail(transaction, productsetId, userId));
            }, productsetId, userId);
            return res;
        }



        /// <summary>
        /// 获取产品系列树
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> GetProductSeries(ProductSeriesListModel body, int userNumber)
        {

            return ExcuteAction((transaction, arg, userData) =>
            {
                return new OutputResult<object>(_repository.GetProductSeries(transaction, body.ProductsetId, body.Direction, body.IsGetDisable, userNumber));
            }, body, userNumber);
        }






        /// <summary>
        /// 删除产品
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> DeleteProduct(string productIds, int userNumber)
        {

            var res = ExcuteAction((transaction, arg, userData) =>
            {
                if (string.IsNullOrEmpty(productIds))
                {
                    throw new Exception("productIds不可为空");
                }
                var productIdsArray = productIds.Split(',');
                var recids = new List<Guid>();
                foreach (var pid in productIdsArray)
                {
                    recids.Add(new Guid(pid));
                }

                //判断某个实体的业务是否有权限新增文档，此处是判断实体业务表的id
                if (!userData.HasDataAccess(transaction, RoutePath, productEntityId, DeviceClassic, recids))
                {
                    throw new Exception("您没有权限停用该数据");
                }
                return HandleResult(_repository.DeleteProduct(transaction, productIds, userNumber));
            }, productIds, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;
        }

        public OutputResult<object> ProductDetail(ProductDetailModel paramInfo, int userId)
        {
            try
            {
                if (paramInfo.ProductOrSet == 1)
                {
                    List<Guid> setids = new List<Guid>();
                    string[] sids = paramInfo.recids.Split(',');
                    foreach (string id in sids)
                    {
                        setids.Add(Guid.Parse(id));
                    }
                    return new OutputResult<object>(this._repository.GetProductSeriesDetail(null, setids, userId));
                }
                else
                {

                    DynamicEntityDetaillistModel model = new DynamicEntityDetaillistModel();
                    model.EntityId = Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2");
                    model.NeedPower = 0;
                    model.RecIds = paramInfo.recids;
                    return this._dynamicEntityServices.DetailList(model, userId);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// 启用产品
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> ToEnableProduct(string productIds, int userNumber)
        {

            var res = ExcuteAction((transaction, arg, userData) =>
            {
                if (string.IsNullOrEmpty(productIds))
                {
                    throw new Exception("productIds不可为空");
                }
                var productIdsArray = productIds.Split(',');
                var recids = new List<Guid>();
                foreach (var pid in productIdsArray)
                {
                    recids.Add(new Guid(pid));
                }

                //判断某个实体的业务是否有权限新增文档，此处是判断实体业务表的id
                if (!userData.HasDataAccess(transaction, RoutePath, productEntityId, DeviceClassic, recids))
                {
                    throw new Exception("您没有权限启用该数据");
                }
                return HandleResult(_repository.ToEnableProduct(transaction, productIds, userNumber));
            }, productIds, userNumber);
            IncreaseDataVersion(DataVersionType.ProductData, null);
            return res;
        }

        /// <summary>
        /// 获取产品
        /// </summary>
        /// <param name="body"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> GetProducts(ProductListModel body, int userNumber)
        {
            var crmData = new ProductList()
            {
                ProductSeriesId = body.ProductSeriesId,
                IsAllProduct = body.IncludeChild,
                RecVersion = body.RecVersion,
                RecStatus = body.RecStatus
            };


            if (!crmData.IsValid())
            {
                return HandleValid(crmData);
            }

            var page = new PageParam()
            {
                PageIndex = body.PageIndex,
                PageSize = body.PageSize
            };

            if (!page.IsValid())
            {
                return HandleValid(page);
            }
            string SearchQuery = string.Empty;
            if (body.ColumnFilter != null && body.ColumnFilter.Count > 0)
            {

                var searchFields = _dynamicEntityServices.GetEntityFields(Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2"), userNumber);
                foreach (DynamicEntityFieldSearch field in searchFields)
                {
                    if (field.ControlType == (int)DynamicProtocolControlType.SelectSingle
                        /*|| field.ControlType == (int)DynamicProtocolControlType.SelectMulti //暂时把多选也设置为模糊*/)
                    {
                        field.IsLike = 0;
                    }
                    else
                    {
                        field.IsLike = 1;//把除字典类型所有的字段都设成模糊搜索
                    }

                }
                Dictionary<string, object> fieldDatas = new Dictionary<string, object>();
                foreach (string key in body.ColumnFilter.Keys)
                {
                    DynamicEntityFieldSearch fieldInfo = searchFields.FirstOrDefault(t => t.FieldName.ToString() == key);
                    if (fieldInfo == null) continue;
                    fieldDatas.Add(fieldInfo.FieldName, body.ColumnFilter[key]);

                }
                var validResults = DynamicProtocolHelper.AdvanceQuery2(searchFields, fieldDatas);

                var validTips = new List<string>();
                var data = new Dictionary<string, string>();
                foreach (DynamicProtocolValidResult validResult in validResults.Values)
                {
                    if (!validResult.IsValid)
                    {
                        validTips.Add(validResult.Tips);
                    }
                    data.Add(validResult.FieldName, validResult.FieldData.ToString());

                }

                if (validTips.Count > 0)
                {
                    return ShowError<object>(string.Join(";", validTips));
                }

                if (data.Count > 0)
                {
                    crmData.SearchQuery = crmData.SearchQuery + " AND (" + string.Join(" AND ", data.Values.ToArray()) + ")";
                }
                else
                    crmData.SearchQuery = string.Empty;

            }
            return ExcuteAction((transaction, arg, userData) =>
            {
                string ruleSql = userData.RuleSqlFormat(RoutePath, productEntityId, DeviceClassic);
                return new OutputResult<object>(_repository.GetProducts(transaction, ruleSql, page, crmData, body.SearchKey, userNumber));
            }, body, userNumber);

        }

        public OutputResult<object> SearchProductAndSeries(ProductSearchModel paramInfo, bool IsForWeb, int userNumber)
        {
            Guid productGuid = Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2");
            #region 处理参数信息
            if (paramInfo.IncludeFilter == null) paramInfo.IncludeFilter = "";
            if (paramInfo.ExcludeFilter == null) paramInfo.ExcludeFilter = "";
            paramInfo.IncludeFilter = paramInfo.IncludeFilter.ToLower();
            paramInfo.ExcludeFilter = paramInfo.ExcludeFilter.ToLower();
            paramInfo.IncludeFilters = paramInfo.IncludeFilter.Split(",");
            paramInfo.ExcludeFilters = paramInfo.ExcludeFilter.Split(",");
            string[] searchkeys = null;
            if (paramInfo.SearchKey != null)
            {
                searchkeys = paramInfo.SearchKey.Split(' ');
            }
            else
            {
                searchkeys = new string[] { };
            }
            #endregion

            Dictionary<string, object> retDict = new Dictionary<string, object>();
            List<ProductSetsSearchInfo> ret = new List<ProductSetsSearchInfo>();

            ProductSetPackageInfo ProductSet = ProductSetsUtils.getInstance().getCurrentPackage(_entityProRepository, _repository, _dynamicEntityServices, paramInfo.IncludeFilter, userNumber);
            #region
            if (paramInfo.IsTopSet == -1 && IsForWeb)
            {
                ProductSetsSearchInfo newRoot = ProductSetsUtils.getInstance().generateTree(paramInfo, ProductSet.RootProductSet);
                if (newRoot != null)
                    SearchProductForProductSet(newRoot, ret);

            }
            else if (paramInfo.IsTopSet == 1)
            {
                //这种情况非搜索，直接处理获取根节点，并判断根节点是否满足要求，以及获取符合要求的子节点的名称
                if (IsForWeb)
                {
                    if (paramInfo.SearchKey == null) paramInfo.SearchKey = "";
                    if (paramInfo.SearchKey != null)
                    {
                        ProductSetsSearchInfo newRoot = ProductSetsUtils.getInstance().generateTree(paramInfo, ProductSet.RootProductSet);
                        if (newRoot != null)
                        {
                            #region 开始处理searchkey过滤
                            SearchProductForSearchKey(searchkeys, IsForWeb, paramInfo.IsProductSetOnly != 0, newRoot, ret, ProductSet.VisibleFieldList);
                            #endregion
                        }
                    }
                }
                else
                {
                    foreach (ProductSetsSearchInfo child in ProductSet.RootProductSet.Children)
                    {

                        if (ProductSetsUtils.getInstance().CheckNodeWith(paramInfo, child))
                        {
                            if (paramInfo.IsProductSetOnly != 0)
                            {
                                if (child.SetOrProduct == 1)
                                {
                                    ret.Add(child.CopyToNew());
                                }
                            }
                            else
                            {
                                ret.Add(child.CopyToNew());
                            }
                        }
                    }
                }

            }
            else if (paramInfo.PSetId != null && paramInfo.PSetId.Length > 0)
            {
                //这种情况是非搜索情况，直接处理js即可.
                if (ProductSet.ProductSetDict.ContainsKey(paramInfo.PSetId))
                {
                    ProductSetsSearchInfo parentSet = ProductSet.ProductSetDict[paramInfo.PSetId];
                    if (IsForWeb)
                    {
                        if (paramInfo.SearchKey == null) paramInfo.SearchKey = "";
                        if (paramInfo.SearchKey != null)
                        {
                            ProductSetsSearchInfo newRoot = ProductSetsUtils.getInstance().generateTree(paramInfo, parentSet);
                            if (newRoot != null)
                            {
                                #region 开始处理searchkey过滤
                                SearchProductForSearchKey(searchkeys, IsForWeb, paramInfo.IsProductSetOnly != 0, newRoot, ret, ProductSet.VisibleFieldList);
                                #endregion
                            }
                        }
                    }
                    else
                    {
                        foreach (ProductSetsSearchInfo child in parentSet.Children)
                        {
                            if (ProductSetsUtils.getInstance().CheckNodeWith(paramInfo, child))
                            {
                                if (paramInfo.IsProductSetOnly != 0)
                                {
                                    if (child.SetOrProduct == 1)
                                    {
                                        ret.Add(child.CopyToNew());
                                    }
                                }
                                else
                                {
                                    ret.Add(child.CopyToNew());
                                }
                            }
                        }
                    }

                }
            }
            else if (paramInfo.SearchKey != null && paramInfo.SearchKey.Length > 0)
            {
                ProductSetsSearchInfo newRoot = ProductSetsUtils.getInstance().generateTree(paramInfo, ProductSet.RootProductSet);
                if (newRoot != null)
                {
                    #region 开始处理searchkey过滤
                    SearchProductForSearchKey(searchkeys, IsForWeb, paramInfo.IsProductSetOnly != 0, newRoot, ret, ProductSet.VisibleFieldList);
                    #endregion
                }
            }
            else
            {
                //这里可能要抛出错误
                ret = new List<ProductSetsSearchInfo>();
            }
            #endregion
            #region 开始处理分页问题
            SortList(ret);
            List<ProductSetsSearchInfo> countList = new List<ProductSetsSearchInfo>();

            Dictionary<string, object> pageCount = new Dictionary<string, object>();
            if (paramInfo.IsTopSet != -1)
            {


                if (paramInfo.PageIndex < 1) paramInfo.PageIndex = 1;
                if (paramInfo.PageCount <= 0) paramInfo.PageCount = 10;
                int startIndex = (paramInfo.PageIndex - 1) * paramInfo.PageCount;
                int endIndex = startIndex + paramInfo.PageCount;
                int totalCount = ret.Count;
                for (int i = startIndex; i < totalCount && i < endIndex; i++)
                {
                    countList.Add(ret[i]);
                }

                pageCount.Add("page", totalCount % paramInfo.PageCount == 0 ? totalCount / paramInfo.PageCount : totalCount / paramInfo.PageCount + 1);
                pageCount.Add("total", ret.Count);
            }
            else
            {
                pageCount.Add("page", 1);
                pageCount.Add("total", ret.Count);
                countList.AddRange(ret);
            }
            CalcNodepath(countList);
            retDict.Add("pagecount", pageCount);
            retDict.Add("pagedata", countList);
            #endregion
            return new OutputResult<object>(retDict);
        }
        private void SortList(List<ProductSetsSearchInfo> list)
        {
            list.Sort((ProductSetsSearchInfo o1, ProductSetsSearchInfo o2) =>
            {
                if (o1.SetOrProduct < o2.SetOrProduct)
                {
                    return -11;
                }
                else if (o1.SetOrProduct > o2.SetOrProduct)
                {
                    return 1;
                }
                else
                {
                    return o1.ProductSetName.CompareTo(o2.ProductSetName);
                }
            });
        }
        private void CalcNodepath(List<ProductSetsSearchInfo> list)
        {
            foreach (ProductSetsSearchInfo item in list)
            {
                char[] chs = item.FullPathId.ToCharArray();    
                int count = 0;
                foreach (char ch in chs)
                {
                    if (ch == '.') count++;
                }
                item.Nodepath = count;
            }
        }
        private void SearchProductForProductSet(ProductSetsSearchInfo item, List<ProductSetsSearchInfo> list)
        {
            if (item.SetOrProduct == 1)
                list.Add(item);
            if (item.Children != null)
            {
                foreach (ProductSetsSearchInfo subItem in item.Children)
                {
                    SearchProductForProductSet(subItem, list);
                }
            }
            item.Children = new List<ProductSetsSearchInfo>();
        }

        private void SearchProductForSearchKey(string[] searchkeys, bool isForWeb, bool isSetOnly, ProductSetsSearchInfo item, List<ProductSetsSearchInfo> list, List<IDictionary<string, object>> fieldsVisible)
        {
            int index = 0;
            bool isMatch = true;
            string productname = item.ProductSetName.ToLower();
            if (searchkeys == null || searchkeys.Length == 0) isMatch = true;
            else
            {
                foreach (string s in searchkeys)
                {
                    isMatch = productname.IndexOf(s) >= 0;
                    if (isMatch == true) break;
                }
            }
            //foreach (char ch in searchkey) {
            //    index = productname.IndexOf(ch, index);
            //    if (index < 0) {
            //        isMatch = false;
            //        break;
            //    }
            //}
            //if (isMatch == false) {
            //    index = 0;
            //    isMatch = true;
            //    if (item.ProductSetName_Pinyin != null) {
            //        foreach (char ch in searchkey)
            //        {
            //            index = item.ProductSetName_Pinyin.IndexOf(ch, index);
            //            if (index < 0)
            //            {
            //                isMatch = false;
            //                break;
            //            }
            //            index++;
            //        }
            //    }   
            //}
            if (isMatch == false && item.SetOrProduct == 2 && item.ProductDetail != null)
            {
                foreach (IDictionary<string, object> field in fieldsVisible)
                {
                    string fieldname = field["fieldname"].ToString();
                    if (fieldname.Equals("productname")) continue;
                    if (item.ProductDetail.ContainsKey(fieldname + "_name"))
                    {
                        fieldname = fieldname + "_name";
                    }
                    if (item.ProductDetail[fieldname] == null) continue;
                    string fieldValue = item.ProductDetail[fieldname].ToString();
                    index = 0;
                    isMatch = true;
                    fieldValue = fieldValue.ToLower();
                    foreach (string s in searchkeys)
                    {
                        isMatch = fieldValue.IndexOf(s) >= 0;
                        if (isMatch == true) break;
                    }
                    //foreach (char ch in searchkey)
                    //{
                    //    index = fieldValue.IndexOf(ch, index);
                    //    if (index < 0)
                    //    {
                    //        isMatch = false;
                    //        break;
                    //    }
                    //}
                    if (isMatch) break;
                }
            }
            if (isMatch)
            {
                if (isForWeb)
                {
                    if (item.SetOrProduct == 2)
                    {
                        list.Add(item.CopyToNew());
                    }
                }
                else
                {
                    if (!isSetOnly || item.SetOrProduct == 1)
                        list.Add(item.CopyToNew());
                }
            }
            if (item.Children != null)
            {
                foreach (ProductSetsSearchInfo subitem in item.Children)
                {
                    SearchProductForSearchKey(searchkeys, isForWeb, isSetOnly, subitem, list, fieldsVisible);
                }
            }
        }

        public OutputResult<object> ChangeProductSet(DynamicEntityEditModel dynamicModel, int userNumber)
        {
            //验证通过后，插入数据
            var result = _dynamicEntityServices.SimpleEdit(dynamicModel, userNumber);

            return result;
        }




    }
    /// <summary>
    /// 记录产品及产品系列的包
    /// </summary>
    public class ProductSetPackageInfo
    {
        public Dictionary<string, ProductSetsSearchInfo> ProductSetDict { get; set; }
        public List<ProductSetsSearchInfo> ListProductSet { get; set; }
        public ProductSetsSearchInfo RootProductSet { get; set; }
        public List<IDictionary<string, object>> VisibleFieldList { get; set; }
        public long MaxSetVersion { get; set; }
        public long MaxProductVersion { get; set; }
        public long MaxFieldVersion { get; set; }
    }
    public class ProductSetsUtils
    {

        private ProductSetPackageInfo ProductSetCached = null;
        private static object lockObj = new object();
        private static object lockWorkObj = new object();
        private static ProductSetsUtils instance = null;
        public static ProductSetsUtils getInstance()
        {
            lock (lockObj)
            {
                if (instance == null) instance = new ProductSetsUtils();
                return instance;
            }
        }
        public ProductSetPackageInfo getCurrentPackage(IEntityProRepository _entityProRepository, IProductsRepository _repository, DynamicEntityServices _dynamicEntityServices, string filterKey, int userNumber)
        {
            lock (lockWorkObj)
            {
                bool isNeedUpdateProduct = false;
                bool isNeedUpdateField = false;
                if (ProductSetCached == null)
                {
                    isNeedUpdateProduct = true;
                    isNeedUpdateField = true;
                }
                else
                {
                    long productVersion;
                    long setVersion;
                    long fieldVersion;
                    this.getProductAndSetVersion(_repository, _dynamicEntityServices, userNumber, out productVersion, out setVersion, out fieldVersion);
                    if (productVersion > ProductSetCached.MaxProductVersion
                        || setVersion > ProductSetCached.MaxSetVersion)
                    {
                        isNeedUpdateProduct = true;
                    }
                    if (fieldVersion > ProductSetCached.MaxFieldVersion)
                    {
                        isNeedUpdateField = true;
                    }
                }
                if (isNeedUpdateProduct)
                {
                    Console.WriteLine("需要获取产品详情");
                    List<ProductSetsSearchInfo> list = this.getList(_repository, _dynamicEntityServices, filterKey, userNumber);
                    ProductSetPackageInfo tmp = this.generatePackage(list);
                    if (ProductSetCached != null)
                    {
                        tmp.MaxFieldVersion = ProductSetCached.MaxFieldVersion;
                        tmp.VisibleFieldList = ProductSetCached.VisibleFieldList;
                    }
                    ProductSetCached = tmp;
                }
                if (isNeedUpdateField)
                {
                    Guid productGuid = Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2");
                    List<IDictionary<string, object>> VisibleFieldList = _entityProRepository.FieldMOBVisibleQuery(productGuid.ToString(), userNumber)["FieldVisible"];
                    ProductSetCached.VisibleFieldList = VisibleFieldList;
                    long tmpversion = 0;
                    long maxversion = 0;
                    foreach (IDictionary<string, object> item in VisibleFieldList)
                    {
                        if (item.ContainsKey("recversion") && item["recversion"] != null)
                        {
                            if (long.TryParse(item["recversion"].ToString(), out tmpversion))
                            {
                                if (maxversion < tmpversion) maxversion = tmpversion;
                            }
                        }
                    }
                    ProductSetCached.MaxFieldVersion = maxversion;
                }
                return ProductSetCached;
            }

        }
        private void getProductAndSetVersion(IProductsRepository _repository, DynamicEntityServices _dynamicEntityServices, int userNumber,
                out long ProductVersion, out long SetVersion, out long FieldVersion)
        {
            DbTransaction tran = null;
            _repository.GetProductAndSetVersion(tran, out ProductVersion, out SetVersion, out FieldVersion, userNumber);
        }
        public List<ProductSetsSearchInfo> getList(IProductsRepository _repository, DynamicEntityServices _dynamicEntityServices, string filterKey, int userNumber)
        {
            ///检查并获取最新的产品系列列表
            Guid productGuid = Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2");
            List<Dictionary<string, object>> tmplist = _repository.getProductAndSet(null, userNumber);
            List<ProductSetsSearchInfo> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductSetsSearchInfo>>(Newtonsoft.Json.JsonConvert.SerializeObject(tmplist));
            //DynamicEntityListModel dynamicEntity = new DynamicEntityListModel()
            //{
            //    EntityId = productGuid,
            //    MenuId = null,
            //    ViewType = 4,
            //    NeedPower = 0,
            //    PageIndex = 1,
            //    PageSize = 1000000,
            //    IsAdvanceQuery = 0
            //};
            //OutputResult<object> tmp = _dynamicEntityServices.DataList2(dynamicEntity, false, 1);
            dynamic tmp = _repository.GetProducts(null, "", new PageParam { PageIndex = 1, PageSize = Int32.MaxValue }, new ProductList
            {
                SearchQuery="1",//表示启动存储过程advancequery 逻辑
                IsAllProduct = true,
                ProductSeriesId = Guid.Parse("7f74192d-b937-403f-ac2a-8be34714278b"),
                RecStatus = 1,
                RecVersion = 0,
                ExitField = 3
            }, filterKey, userNumber);;
            var pagedata = tmp.GetType().GetProperty("pagedata").GetValue(tmp, null);
            List<Dictionary<string, object>> products = (List<Dictionary<string, object>>)pagedata;
            foreach (Dictionary<string, object> item in products)
            {
                ProductSetsSearchInfo product = new ProductSetsSearchInfo();
                product.Children = new List<ProductSetsSearchInfo>();
                product.SetOrProduct = 2;
                if (item.ContainsKey("recid") && item["recid"] != null)
                {
                    product.ProductSetId = Guid.Parse(item["recid"].ToString());
                }
                else
                {
                    product.ProductSetId = Guid.Empty;
                }
                if (item.ContainsKey("productcode") && item["productcode"] != null)
                {
                    product.ProductSetCode = item["productcode"].ToString();
                }
                else
                {
                    product.ProductSetCode = "";
                }
                if (item.ContainsKey("productname") && item["productname"] != null)
                {
                    product.ProductSetName = item["productname"].ToString();
                }
                else
                {
                    product.ProductSetName = "";
                }
                if (item.ContainsKey("productsetid") && item["productsetid"] != null)
                {
                    product.PProductSetId = Guid.Parse(item["productsetid"].ToString());
                }
                else
                {
                    product.PProductSetId = Guid.Empty;
                }
                if (item.ContainsKey("recid") && item["recid"] != null)
                {
                    product.RecId = Guid.Parse(item["recid"].ToString());
                }
                else
                {
                    product.RecId = Guid.Empty;
                }
                if (item.ContainsKey("recorder") && item["recorder"] != null)
                {
                    product.RecOrder = int.Parse(item["recorder"].ToString());
                }
                else
                {
                    product.RecOrder = 0;
                }
                if (item.ContainsKey("recversion") && item["recversion"] != null)
                {
                    long tmpversion = 0;
                    long.TryParse(item["recversion"].ToString(), out tmpversion);
                    product.RecVersion = tmpversion;
                }
                else
                {
                    product.RecVersion = 0;
                }
                product.ProductDetail = item;
                list.Add(product);
            }
            return list;
        }
        public ProductSetPackageInfo generatePackage(List<ProductSetsSearchInfo> list)
        {
            ProductSetPackageInfo ret = new ProductSetPackageInfo();
            ret.ListProductSet = list;
            ret.ProductSetDict = new Dictionary<string, ProductSetsSearchInfo>();
            long maxProductVersion = 0;
            long maxSetVersion = 0;
            foreach (ProductSetsSearchInfo item in list)
            {
                if (item.SetOrProduct == 2 && maxProductVersion < item.RecVersion)
                {
                    maxProductVersion = item.RecVersion;

                }
                else if (item.SetOrProduct == 1 && maxSetVersion < item.RecVersion)
                {
                    maxSetVersion = item.RecVersion;

                }
                ret.ProductSetDict.Add(item.ProductSetId.ToString(), item);
            }

            foreach (ProductSetsSearchInfo item in list)
            {
                //  item.ProductSetName_Pinyin = PingYinHelper.ConvertToAllSpell(item.ProductSetName);
                if (item.ProductSetName_Pinyin == null) item.ProductSetName_Pinyin = "";
                item.ProductSetName_Pinyin = item.ProductSetName_Pinyin.ToLower();
                if (item.PProductSetId != null)
                {
                    if (item.PProductSetId == Guid.Empty && ret.RootProductSet == null && item.SetOrProduct == 1)
                    {
                        ret.RootProductSet = item;
                    }
                    string tid = item.PProductSetId.ToString();
                    if (ret.ProductSetDict.ContainsKey(tid))
                    {
                        if (ret.ProductSetDict[tid].Children == null)
                        {
                            ret.ProductSetDict[tid].Children = new List<ProductSetsSearchInfo>();
                        }
                        ret.ProductSetDict[tid].Children.Add(item);
                    }
                }
            }
            if (ret.RootProductSet != null)
            {
                this.GenerateFullName(ret.RootProductSet, "", "");
                CalcChildrenCount(ret.RootProductSet);
            }
            ret.MaxProductVersion = maxProductVersion;
            ret.MaxSetVersion = maxSetVersion;
            return ret;
        }
        public void CalcChildrenCount(ProductSetsSearchInfo item)
        {
            if (item.Children != null)
            {
                item.ChildrenCount = item.Children.Count;
                foreach (ProductSetsSearchInfo subItem in item.Children)
                {
                    CalcChildrenCount(subItem);
                }
            }
            else
            {
                item.ChildrenCount = 0;
            }
        }
        private void GenerateFullName(ProductSetsSearchInfo root, string prefix, string prefixId)
        {
            root.FullPathId = prefixId + root.ProductSetId;
            root.FullPathName = prefix + root.ProductSetName;
            if (root.Children != null)
            {
                foreach (ProductSetsSearchInfo item in root.Children)
                {
                    GenerateFullName(item, root.FullPathName + ".", root.FullPathId + ".");
                }
            }
        }

        public bool CheckNodeWith(ProductSearchModel paramInfo, ProductSetsSearchInfo root)
        {
            if (root.Children != null)
            {
                foreach (ProductSetsSearchInfo item in root.Children)
                {
                    if (CheckNodeWith(paramInfo, item)) return true;
                }
            }
            return CheckSingleNode(paramInfo, root);
        }
        public ProductSetsSearchInfo generateTree(ProductSearchModel paramInfo, ProductSetsSearchInfo root)
        {
            ProductSetsSearchInfo ret = root.CopyToNew();
            ret.Children = new List<ProductSetsSearchInfo>();
            bool isOK = false;
            if (root.Children != null && root.Children.Count > 0)
            {
                foreach (ProductSetsSearchInfo item in root.Children)
                {
                    ProductSetsSearchInfo tmp = generateTree(paramInfo, item);
                    if (tmp != null)
                    {
                        isOK = true;
                        ret.Children.Add(tmp);
                    }
                }
            }
            if (isOK == false)
            {
                isOK = CheckSingleNode(paramInfo, root);
            }
            if (isOK)
                return ret;
            return null;
        }

        private bool CheckSingleNode(ProductSearchModel paramInfo, ProductSetsSearchInfo root)
        {
            bool isMatch = false;
            bool isCheck = false;
            ////foreach (string item in paramInfo.IncludeFilters)
            //{
            //    if (item.Length == 0) continue;
            //    isCheck = true;
            //    if (root.FullPathName.ToLower().IndexOf(item) >= 0)
            //    {
            //        isMatch = true;
            //        break;
            //    }
            //}
            if (root.SetOrProduct == 1) return true;
            var filter = paramInfo.IncludeFilters.Where(t => !string.IsNullOrEmpty(t)).FirstOrDefault(t => t == root.RecId.ToString());
            if (filter != null)
            {
                isCheck = true; isMatch = true;
            }

            if (paramInfo.IncludeFilters.FirstOrDefault(t => !string.IsNullOrEmpty(t)) != null) isCheck = true;

            if (isMatch == false && isCheck == true) return false;
            var exFilter = paramInfo.ExcludeFilters.Where(t => !string.IsNullOrEmpty(t)).FirstOrDefault(t => t == root.RecId.ToString());
            if (exFilter != null)
            {
                return false;
            }
            return true;
            //foreach (string item in paramInfo.ExcludeFilters)
            //{
            //    if (item.Length == 0) continue;
            //    if (root.FullPathName.ToLower().IndexOf(item) >= 0)
            //    {
            //        return false;
            //    }
            //}
            //return true;
        }
    }

    public class ProductSetsSearchInfo
    {
        public Guid RecId { get; set; }
        public Guid ProductSetId { get; set; }
        public string ProductSetName { get; set; }
        public string ProductSetName_Pinyin { get; set; }
        public Guid PProductSetId { get; set; }
        public string ProductSetCode { get; set; }
        public int RecOrder { get; set; }
        public string FullPathName { get; set; }
        public string FullPathId { get; set; }
        public long RecVersion { get; set; }
        public int SetOrProduct { get; set; }
        public int Nodepath { get; set; }
        public List<ProductSetsSearchInfo> Children { get; set; }
        public int ChildrenCount { get; set; }
        public Dictionary<string, object> ProductDetail { get; set; }
        public ProductSetsSearchInfo()
        {
            Children = new List<ProductSetsSearchInfo>();
        }
        public ProductSetsSearchInfo CopyToNew()
        {
            ProductSetsSearchInfo ret = new ProductSetsSearchInfo();
            ret.ProductSetId = this.ProductSetId;
            ret.ProductSetName = this.ProductSetName;
            ret.PProductSetId = this.PProductSetId;
            ret.ProductSetCode = this.ProductSetCode;
            ret.RecOrder = this.RecOrder;
            ret.FullPathName = this.FullPathName;
            ret.FullPathId = this.FullPathId;
            ret.SetOrProduct = this.SetOrProduct;
            ret.ProductSetName_Pinyin = this.ProductSetName_Pinyin;
            ret.ProductDetail = ProductDetail;
            ret.ChildrenCount = this.ChildrenCount;
            return ret;
        }

    }
    public class PingYinHelper
    {
        private static Encoding gb2312 = Encoding.GetEncoding("GB2312");

        /// <summary>
        /// 汉字转全拼
        /// </summary>
        /// <param name="strChinese"></param>
        /// <returns></returns>
        public static string ConvertToAllSpell(string strChinese)
        {
            try
            {
                if (strChinese.Length != 0)
                {
                    StringBuilder fullSpell = new StringBuilder();
                    for (int i = 0; i < strChinese.Length; i++)
                    {
                        var chr = strChinese[i];
                        fullSpell.Append(GetSpell(chr));
                    }

                    return fullSpell.ToString().ToUpper();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("全拼转化出错！" + e.Message);
            }

            return string.Empty;
        }

        public static string ConvertFirstChar(string strChinese)
        {
            try
            {
                if (strChinese.Length != 0)
                {
                    StringBuilder fullSpell = new StringBuilder();
                    for (int i = 0; i < strChinese.Length; i++)
                    {
                        var chr = strChinese[i];
                        fullSpell.Append(GetSpell(chr));
                    }

                    return fullSpell.ToString().ToUpper();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("全拼转化出错！" + e.Message);
            }

            return string.Empty;
        }
        /// <summary>
        /// 汉字转首字母
        /// </summary>
        /// <param name="strChinese"></param>
        /// <returns></returns>
        public static string GetFirstSpell(string strChinese)
        {
            //NPinyin.Pinyin.GetInitials(strChinese)  有Bug  洺无法识别
            //return NPinyin.Pinyin.GetInitials(strChinese);

            try
            {
                if (strChinese.Length != 0)
                {
                    StringBuilder fullSpell = new StringBuilder();
                    for (int i = 0; i < strChinese.Length; i++)
                    {
                        var chr = strChinese[i];
                        fullSpell.Append(GetSpellFirstChar(chr));
                    }

                    return fullSpell.ToString().ToUpper();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("首字母转化出错！" + e.Message);
            }

            return string.Empty;
        }

        private static string GetSpell(char chr)
        {
            var coverchr = NPinyin.Pinyin.GetPinyin(chr);

            bool isChineses = ChineseChar.IsValidChar(coverchr[0]);
            if (isChineses)
            {
                ChineseChar chineseChar = new ChineseChar(coverchr[0]);
                foreach (string value in chineseChar.Pinyins)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value.Remove(value.Length - 1, 1);
                    }
                }
            }

            return coverchr.Substring(0, 1);

        }
        private static string GetSpellFirstChar(char chr)
        {
            var coverchr = NPinyin.Pinyin.GetPinyin(chr);

            bool isChineses = ChineseChar.IsValidChar(coverchr[0]);
            if (isChineses)
            {
                ChineseChar chineseChar = new ChineseChar(coverchr[0]);
                foreach (string value in chineseChar.Pinyins)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value.Substring(0, 1);
                    }
                }
            }

            return "";

        }
    }
}

