using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Products;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IProductsRepository
    {

        Object IsExistsProductSeries(DbTransaction trans, ProductSeriesEdit data, int userNumber);

        /// <summary>
        /// 添加产品系列
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult AddProductSeries(DbTransaction trans,ProductSeriesInsert data, int userNumber);


        /// <summary>
        /// 编辑产品系列
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult EditProductSeries(DbTransaction trans, ProductSeriesEdit data, int userNumber);


        /// <summary>
        /// 删除产品系列
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        OperateResult DeleteProductSeries(DbTransaction trans, Guid productSeriesId, int userNumber);

        //启用产品系列
        OperateResult ToEnableProductSeries(DbTransaction trans, Guid productSeriesId, int userNumber);

        /// <summary>
        /// 获取产品系列树
        /// </summary>
        /// <returns></returns>
        dynamic GetProductSeries(DbTransaction trans, Guid? productSetId, string direction,int isGetDisable, int userNumbe);




       
        /// <summary>
        /// 删除产品
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        OperateResult DeleteProduct(DbTransaction trans, string productIds, int userNumber);
        // 启用产品
        OperateResult ToEnableProduct(DbTransaction trans, string productIds, int userNumber);
        /// <summary>
        /// 获取产品
        /// </summary>
        /// <returns></returns>
        dynamic GetProducts(DbTransaction trans,string ruleSql, PageParam page, ProductList productData, string serachKey, int userNumber);
        Dictionary<string, List<Dictionary<string, object>>> GetNewProducts(DbTransaction trans, string ruleSql, PageParam page, ProductList productData, string serachKey, int userNumber);
        /// <summary>
        /// 根据产品系列id获取产品系列详情，不包含下级产品系列
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="productSetId"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        dynamic GetProductSeriesDetail(DbTransaction trans, Guid productSetId, int userNum);
        dynamic GetProductSeriesDetail(DbTransaction trans, List<Guid> productSetId, int userNum);

        List<Dictionary<string, object>> getProductAndSet(DbTransaction trans, int userNum);
        void GetProductAndSetVersion(DbTransaction tran, out long productVersion, out long setVersion, out long fieldVersion, int userNumber);
        object IsProductExists(DbTransaction trans, string cust, string productcode, string partnum, string partrev, string salespartrev, string customermodel, int userId);
        string GetProductLastUpdatedTime(DbTransaction trans, int userId);
    }
}
