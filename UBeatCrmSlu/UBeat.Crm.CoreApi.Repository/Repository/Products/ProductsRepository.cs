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
        public OperateResult ToEnableProductSeries(DbTransaction trans, Guid productSeriesId, int userNumber)
        {
            var executeSql = @"update crm_sys_products_series
	                           set recstatus=1,
	                               recupdator=@userno,
	                               recupdated=now()
	                           where productsetid=@productsetid;";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productsetid", productSeriesId),
                new NpgsqlParameter("userno", userNumber),
            };
            var resutl = new OperateResult();
            int res = ExecuteNonQuery(executeSql, param, trans);
            if (res > 0)
                resutl.Flag = 1;
            return resutl;
        }


        public dynamic GetProductSeries(DbTransaction trans, Guid? productSetId, string direction, int isGetDisable, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_product_series_select_improve(@productsetid,@direction,@isgetdisable,@userno)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productsetid", productSetId.ToString()),
                new NpgsqlParameter("direction", direction),
                new NpgsqlParameter("isgetdisable", isGetDisable),
                new NpgsqlParameter("userno", userNumber),

            };
            return ExecuteQuery(executeSql, param, trans);

        }
        /// <summary>
        /// 根据产品系列id，或者产品系列详情，不包括子产品系列
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="productSetId"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        public dynamic GetProductSeriesDetail(DbTransaction trans, Guid productSetId, int userNum) {
            var sql = string.Format(@"SELECT
	                    C .productsetid,
	                    C .pproductsetid,
	                    C .productsetname,
	                    C .productsetcode,
	                    C .recorder,
	                    C .reccreator,
	                    C .reccreated
                    FROM
	                    crm_sys_products_series AS C
                    WHERE
	                    C .recstatus = 1
	                    AND c.productsetid = '620a733c-14fe-4a62-8aa2-f22f8b2b9a83'", productSetId);
            return ExecuteQuery(sql, new DbParameter[] { }, trans);
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
        public OperateResult ToEnableProduct(DbTransaction trans, string productIds, int userNumber)
        {
            var executeSql = @" UPDATE crm_sys_product
			                    SET recstatus=1,
			                    recupdator=@userno,
			                    recupdated=now()
			                    WHERE recid::text IN (select * from regexp_split_to_table(@productids,','));";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productids", productIds),
                new NpgsqlParameter("userno", userNumber),
            };
            var resutl = new OperateResult();
            int res = ExecuteNonQuery(executeSql, param, trans);
            if (res > 0)
                resutl.Flag = 1;
            return resutl;
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

        public List<Dictionary<string, object>> getProductAndSet(DbTransaction trans, int userNum)
        {
            try
            {
                string strSQL = @"select * 
                                from (

                                select productsetid,productsetcode,productsetname,pproductsetid,recorder  ,1 SetOrProduct from crm_sys_products_series  where recstatus =1 
                                union all 
                                select recid prouctsetid ,productcode productsetcode,productname productsetname ,productsetid pproductsetid ,recorder  ,2 SetOrProduct 
                                from crm_sys_product
                                where recstatus = 1 )
                                 aa order by aa.SetOrProduct ,aa.productsetname";
                return ExecuteQuery(strSQL, new DbParameter[] { }, trans);
            }
            catch (Exception ex) {
                return new List<Dictionary<string, object>>();
            }
        }
    }
}
