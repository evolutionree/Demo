using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Products;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Products
{
    public class ProductsRepository : RepositoryBase, IProductsRepository
    {
        public OperateResult AddProductSeries(DbTransaction trans, ProductSeriesInsert data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_productseries_insert(@topseriesid,@seriesname,@seriescode,@userno)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("topseriesid", data.TopSeriesId),
                new NpgsqlParameter("seriesname", data.SeriesName),
                new NpgsqlParameter("seriescode", data.SeriesCode),
                new NpgsqlParameter("userno", userNumber),

            };
            if (trans == null)
            {
                return DBHelper.ExecuteQuery<OperateResult>("", executeSql, param).FirstOrDefault();
            }
            return DBHelper.ExecuteQuery<OperateResult>(trans, executeSql, param).FirstOrDefault();

        }


        public OperateResult EditProductSeries(DbTransaction trans, ProductSeriesEdit data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_productseries_update(@productsetid,@seriesname,@seriescode,@userno)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productsetid", data.ProductsetId.ToString()),
                new NpgsqlParameter("seriesname", data.SeriesName),
                new NpgsqlParameter("seriescode", data.SeriesCode),
                new NpgsqlParameter("userno", userNumber),

            };

            return ExecuteQuery<OperateResult>(executeSql, param, trans).FirstOrDefault();


        }

        public OperateResult DeleteProductSeries(DbTransaction trans, Guid productSeriesId, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_productseries_delete(@productsetid,@userno)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productsetid", productSeriesId.ToString()),

                new NpgsqlParameter("userno", userNumber),

            };
            return ExecuteQuery<OperateResult>(executeSql, param, trans).FirstOrDefault();
        }



        public dynamic GetProductSeries(DbTransaction trans, Guid? productSetId, string direction, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_product_series_select(@productsetid,@direction,@userno)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productsetid", productSetId.ToString()),
                new NpgsqlParameter("direction", direction),
                new NpgsqlParameter("userno", userNumber),

            };
            return ExecuteQuery(executeSql, param, trans);

        }


        public OperateResult DeleteProduct(DbTransaction trans, string productIds, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_products_delete_improve(@productid,@userno)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productid", productIds),

                new NpgsqlParameter("userno", userNumber),

            };
            return ExecuteQuery<OperateResult>(executeSql, param, trans).FirstOrDefault();

        }


        public dynamic GetProducts(DbTransaction trans, string ruleSql, PageParam page, ProductList productData, string serachKey, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_products_select(@productseriesid,@includechild,@searchkey,@userno,@pageindex,@pagesize,@recversion,@recstatus)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productseriesid", productData.ProductSeriesId.ToString()),
                new NpgsqlParameter("includechild", productData.IsAllProduct == true ? 1 : 0),
                new NpgsqlParameter("searchkey", serachKey),
                new NpgsqlParameter("userno", userNumber),
                new NpgsqlParameter("pageindex", page.PageIndex),
                new NpgsqlParameter("pagesize",  page.PageSize),
                new NpgsqlParameter("recversion", productData.RecVersion),
                new NpgsqlParameter("recstatus", productData.RecStatus),

            };
            Dictionary<string, List<Dictionary<string, object>>> dataResult = null;

            dataResult = ExecuteQueryRefCursor(executeSql, param, trans);

            return new
            {
                pagecount = dataResult["page"],
                pagedata = dataResult["data"]
            };

        }

        

    }
}
