using System;
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

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ProductsServices : BasicBaseServices
    {
        private IProductsRepository _repository;
        private DynamicEntityServices _dynamicEntityServices;

        private readonly Guid productEntityId = new Guid("59cf141c-4d74-44da-bca8-3ccf8582a1f2");//固定值



        public ProductsServices(IProductsRepository repository, DynamicEntityServices dynamicEntityServices)
        {
            _repository = repository;
            _dynamicEntityServices = dynamicEntityServices;
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
                SeriesCode = body.SeriesCode
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
                SeriesCode = body.SeriesCode

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
            return ExcuteAction((transaction, arg, userData) =>
            {
                string ruleSql = userData.RuleSqlFormat(RoutePath, productEntityId, DeviceClassic);
                return new OutputResult<object>(_repository.GetProducts(transaction, ruleSql, page, crmData, body.SearchKey, userNumber));
            }, body, userNumber);

        }

        public OutputResult<object> SearchProductAndSeries(ProductSearchModel paramInfo, int userNumber)
        {
            #region 处理参数信息
            if (paramInfo.IncludeFilter == null) paramInfo.IncludeFilter = "";
            paramInfo.IncludeFilters = paramInfo.IncludeFilter.Split(",");
            if (paramInfo.ExcludeFilter == null) paramInfo.ExcludeFilter = "";
            paramInfo.ExcludeFilters = paramInfo.ExcludeFilter.Split(",");
            #endregion
            Dictionary<string, object> retDict = new Dictionary<string, object>();
            List<ProductSetsSearchInfo> ret = new List<ProductSetsSearchInfo>();
            ///检查并获取最新的产品系列列表
            List< Dictionary<string,object>> tmplist = this._repository.getProductAndSet(null, userNumber);
            List<ProductSetsSearchInfo> list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductSetsSearchInfo>>(Newtonsoft.Json.JsonConvert.SerializeObject(tmplist));
            DynamicEntityListModel dynamicEntity = new DynamicEntityListModel()
            {
                EntityId = Guid.Parse("59cf141c-4d74-44da-bca8-3ccf8582a1f2"),
                MenuId = null,
                ViewType = 4,
                NeedPower = 0,
                PageIndex = 1,
                PageSize = 1000000,
                IsAdvanceQuery = 0
            };
            OutputResult<object>  tmp  = _dynamicEntityServices.DataList2(dynamicEntity, false, 1);
            
            List<Dictionary<string, object>> products = ((Dictionary<string, List<Dictionary<string, object>>>)tmp.DataBody)["PageData"];
            ProductSetPackageInfo ProductSet = ProductSetsUtils.getInstance().generatePackage(list);
            #region
            if (paramInfo.IsTopSet == 1)
            {
                //这种情况非搜索，直接处理获取根节点，并判断根节点是否满足要求，以及获取符合要求的子节点的名称
                foreach (ProductSetsSearchInfo child in ProductSet.RootProductSet.Children)
                {
                    if (ProductSetsUtils.getInstance().CheckNodeWith(paramInfo, child))
                    {
                        ret.Add(child.CopyToNew());
                    }
                }
            }
            else if (paramInfo.PSetId != null)
            {
                //这种情况是非搜索情况，直接处理js即可.
                if (ProductSet.ProductSetDict.ContainsKey(paramInfo.PSetId))
                {
                    ProductSetsSearchInfo parentSet = ProductSet.ProductSetDict[paramInfo.PSetId];
                    foreach (ProductSetsSearchInfo child in parentSet.Children)
                    {
                        if (ProductSetsUtils.getInstance().CheckNodeWith(paramInfo, child))
                        {
                            ret.Add(child.CopyToNew());
                        }
                    }

                }
                else
                {


                }
            }
            else if (paramInfo.SearchKey != null && paramInfo.SearchKey.Length > 0)
            {
                ProductSetsSearchInfo newRoot = ProductSetsUtils.getInstance().generateTree(paramInfo,ProductSet.RootProductSet);
                if (newRoot != null) {
                    #region 开始处理searchkey过滤
                    SearchProductForSearchKey(paramInfo.SearchKey.ToCharArray(), newRoot, ret);
                    #endregion
                }
            }
            else {
                //这里可能要抛出错误
            }
            #endregion
            #region 开始处理分页问题
            List<ProductSetsSearchInfo> countList = new List<ProductSetsSearchInfo>();

            if (paramInfo.PageIndex < 1) paramInfo.PageIndex = 1;
            if (paramInfo.PageCount <= 0) paramInfo.PageCount = 10;
            int startIndex = (paramInfo.PageIndex - 1) * paramInfo.PageCount;
            int endIndex = startIndex + paramInfo.PageCount;
            int totalCount = ret.Count;
            for (int i = startIndex; i < totalCount && i < endIndex; i++) {
                countList.Add(ret[i]);
            }
            Dictionary<string, object> pageCount = new Dictionary<string, object>();
            
            pageCount.Add("page",totalCount % paramInfo.PageCount ==0 ? totalCount /paramInfo.PageCount :totalCount/paramInfo.PageCount +1) ;
            pageCount.Add("total", ret.Count);
            retDict.Add("pagecount", pageCount);
            retDict.Add("pagedata", countList);
            #endregion
            return new OutputResult<object>(retDict);
        }
        private void SearchProductForSearchKey(char [] searchkey ,ProductSetsSearchInfo item,List<ProductSetsSearchInfo> list) {
            int index = 0;
            bool isMatch = true;
            foreach (char ch in searchkey) {
                index = item.ProductSetName.IndexOf(ch, index);
                if (index < 0) {
                    isMatch = false;
                    break;
                }
            }
            if (isMatch == false) {
                index = 0;
                isMatch = true;
                if (item.ProductSetName_Pinyin != null) {
                    foreach (char ch in searchkey)
                    {
                        index = item.ProductSetName_Pinyin.IndexOf(ch, index);
                        if (index < 0)
                        {
                            isMatch = false;
                            break;
                        }
                    }
                }
                
            }
            if (isMatch)
            {
                list.Add(item.CopyToNew());
            }
            if (item.Children != null) {
                foreach (ProductSetsSearchInfo subitem in item.Children) {
                    SearchProductForSearchKey(searchkey, subitem, list);
                }
            }
        }
    }
    public class ProductSetPackageInfo {
        public Dictionary<string, ProductSetsSearchInfo> ProductSetDict { get; set; }
        public List<ProductSetsSearchInfo> ListProductSet { get; set; }
        public ProductSetsSearchInfo RootProductSet { get; set; }
    }
    public class ProductSetsUtils {

        private ProductSetPackageInfo ProductSetCached = new ProductSetPackageInfo();
        public long MaxRecVersion = 0;
        private static  object lockObj = new object();
        private static ProductSetsUtils instance = null;
        public static ProductSetsUtils getInstance() {
            lock (lockObj)
            {
                if (instance == null) instance = new ProductSetsUtils();
                return instance;
            }
        }
        public ProductSetPackageInfo generatePackage(List<ProductSetsSearchInfo> list)
        {
            ProductSetPackageInfo ret = new ProductSetPackageInfo();
            ret.ListProductSet = list;
            ret.ProductSetDict = new Dictionary<string, ProductSetsSearchInfo>();
            foreach (ProductSetsSearchInfo item in list)
            {
                ret.ProductSetDict.Add(item.ProductSetId.ToString(), item);
            }

            foreach (ProductSetsSearchInfo item in list)
            {
                item.ProductSetName_Pinyin = PingYinHelper.ConvertToAllSpell(item.ProductSetName);
                if (item.ProductSetName_Pinyin == null) item.ProductSetName_Pinyin = "";
                item.ProductSetName_Pinyin = item.ProductSetName_Pinyin.ToLower();
                    if (item.PProductSetId != null)
                {
                    if (item.PProductSetId == Guid.Empty) {
                        ret.RootProductSet = item;
                    }
                    string tid = item.PProductSetId.ToString();
                    if (ret.ProductSetDict.ContainsKey(tid))
                    {
                        if (ret.ProductSetDict[tid].Children == null) {
                            ret.ProductSetDict[tid].Children = new List<ProductSetsSearchInfo>();
                        }
                        ret.ProductSetDict[tid].Children.Add(item);
                    }
                }
            }
            if (ret.RootProductSet != null) {
                this.GenerateFullName(ret.RootProductSet, "");
            }
            return ret;
        }
        private void GenerateFullName(ProductSetsSearchInfo root,string prefix) {
            root.FullPathName = prefix + root.ProductSetName;
            if (root.Children != null) {
                foreach (ProductSetsSearchInfo item in root.Children) {
                    GenerateFullName(item, root.FullPathName + ".");
                }
            }
        }

        public bool CheckNodeWith(ProductSearchModel paramInfo, ProductSetsSearchInfo root)
        {
            if (root.Children != null) {
                foreach (ProductSetsSearchInfo item in root.Children) {
                    if (CheckNodeWith(paramInfo, item)) return true;
                }
            }
            return CheckSingleNode(paramInfo, root);
        }
        public ProductSetsSearchInfo generateTree(ProductSearchModel paramInfo, ProductSetsSearchInfo root) {
            ProductSetsSearchInfo ret = root.CopyToNew();
            ret.Children = new List<ProductSetsSearchInfo>();
            bool isOK = false;
            if (root.Children != null && root.Children.Count > 0) {
                foreach (ProductSetsSearchInfo item in root.Children) {
                    ProductSetsSearchInfo tmp = generateTree(paramInfo, item);
                    if (tmp != null) {
                        isOK = true;
                        ret.Children.Add(tmp);
                    }
                }
            }
            if (isOK == false) {
                isOK = CheckSingleNode(paramInfo, root);
            }
            if (isOK )
                return ret;
            return null;
        }

        private bool CheckSingleNode(ProductSearchModel paramInfo, ProductSetsSearchInfo root) {
            bool isMatch = false;
            bool isCheck = false;
            foreach (string item in paramInfo.IncludeFilters) {
                if (item.Length == 0) continue;
                isCheck = true;
                if (root.FullPathName.ToLower().IndexOf(item) >= 0) {
                    isMatch = true;
                    break;
                }
            }
            if (isMatch == false && isCheck ==true) return false;
            foreach (string item in paramInfo.ExcludeFilters) {
                if (item.Length == 0) continue;
                if (root.FullPathName.ToLower().IndexOf(item) >= 0)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class ProductSetsSearchInfo
    {
        public Guid ProductSetId { get; set; }
        public string ProductSetName { get; set; }
        public string ProductSetName_Pinyin { get; set; }
        public Guid PProductSetId { get; set; }
        public string ProductSetCode { get; set; }
        public int RecOrder { get; set; }
        public string FullPathName { get; set; }

        public int SetOrProduct { get; set; }
        public List<ProductSetsSearchInfo> Children { get; set; }
        public ProductSetsSearchInfo() {
            Children = new List<ProductSetsSearchInfo>();
        }
        public ProductSetsSearchInfo CopyToNew() {
            ProductSetsSearchInfo ret = new ProductSetsSearchInfo();
            ret.ProductSetId = this.ProductSetId;
            ret.ProductSetName = this.ProductSetName;
            ret.PProductSetId = this.PProductSetId;
            ret.ProductSetCode = this.ProductSetCode;
            ret.RecOrder = this.RecOrder;
            ret.FullPathName = this.FullPathName;
            ret.SetOrProduct = this.SetOrProduct;
            ret.ProductSetName_Pinyin = this.ProductSetName_Pinyin;
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
                        fullSpell.Append(GetSpell(chr)[0]);
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

            return coverchr;

        }
    }
}

