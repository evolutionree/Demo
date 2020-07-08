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
            var executeSql = @"SELECT * FROM crm_func_productseries_insert(@topseriesid,@seriesname,@seriescode,@userno,@serieslanguage::jsonb)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("topseriesid", data.TopSeriesId),
                new NpgsqlParameter("seriesname", data.SeriesName),
                new NpgsqlParameter("seriescode", data.SeriesCode),
                new NpgsqlParameter("serieslanguage", data.SeriesLanguage),
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
            var executeSql = @"SELECT * FROM crm_func_productseries_update(@productsetid,@seriesname,@seriescode,@userno,@serieslanguage::jsonb)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productsetid", data.ProductsetId.ToString()),
                new NpgsqlParameter("seriesname", data.SeriesName),
                new NpgsqlParameter("seriescode", data.SeriesCode),
                new NpgsqlParameter("serieslanguage", data.SeriesLanguage),
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
        public dynamic GetProductSeriesDetail(DbTransaction trans, Guid productSetId, int userNum)
        {
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
	                    AND c.productsetid = '{0}'", productSetId);
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
        public Dictionary<string, List<Dictionary<string, object>>> GetNewProducts(DbTransaction trans, string ruleSql, PageParam page, ProductList productData, string serachKey, int userNumber)
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

            return dataResult;

        }

        public List<Dictionary<string, object>> getProductAndSet(DbTransaction trans, int userNum)
        {
            try
            {
                string strSQL = @"select * 
                                from (

                                select productsetid,productsetcode,productsetname,pproductsetid,recorder  ,1 SetOrProduct ,recversion from crm_sys_products_series  where recstatus =1 
                                )
                                 aa order by aa.SetOrProduct ,aa.productsetname";
                return ExecuteQuery(strSQL, new DbParameter[] { }, trans);
            }
            catch (Exception ex)
            {
                return new List<Dictionary<string, object>>();
            }
        }

        public void GetProductAndSetVersion(DbTransaction tran, out long productVersion, out long setVersion, out long fieldVersion, int userNumber)
        {
            try
            {
                productVersion = 0;
                fieldVersion = 0;
                setVersion = 0;
                string strSQL = @"select * 
                                    from (
                                    select max(recversion) productversion   from crm_sys_product  where recstatus = 1 ) product,
                                    (
                                    select max(recversion) setversion from crm_sys_products_series  where recstatus = 1  
                                    ) productset,
                                    (
                                    select  max(recversion) fieldversion from crm_sys_entity_listview_viewcolumn where viewtype = 1 and entityid ='59cf141c-4d74-44da-bca8-3ccf8582a1f2' 
                                    ) fields";
                List<Dictionary<string, object>> list = ExecuteQuery(strSQL, new DbParameter[] { }, tran);
                if (list != null || list.Count > 0)
                {
                    if (list[0]["productversion"] != null)
                    {
                        long.TryParse(list[0]["productversion"].ToString(), out productVersion);
                    }
                    if (list[0]["setversion"] != null)
                    {
                        long.TryParse(list[0]["setversion"].ToString(), out setVersion);
                    }
                    if (list[0]["fieldversion"] != null)
                    {
                        long.TryParse(list[0]["fieldversion"].ToString(), out fieldVersion);
                    }

                }
            }
            catch (Exception ex)
            {
                productVersion = 0;
                fieldVersion = 0;
                setVersion = 0;
            }

        }

        public dynamic GetProductSeriesDetail(DbTransaction trans, List<Guid> productSetId, int userNum)
        {

            string tmp = string.Join("','", productSetId);
            tmp = "'" + tmp + "'";
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
	                    AND c.productsetid in ({0})", tmp);
            return ExecuteQuery(sql, new DbParameter[] { }, trans);
        }

        public object IsProductExists(DbTransaction trans, string productcode, int userId)
        {
            var sql = " select recid from  crm_sys_product where productcode=@productcode";
            var dbParam = new DbParameter[] {
                new NpgsqlParameter("productcode",productcode)
            };
            var result = ExecuteScalar(sql, dbParam, trans);
            if (result != null)
                return result;
            return string.Empty;
        }

        public string GetProductLastUpdatedTime(DbTransaction trans, int userId)
        {
            var sql = " select recupdated from  crm_sys_product  order by recupdated desc limit 1   ";
            var result = ExecuteScalar(sql, null, trans);
            if (result != null)
                return Convert.ToDateTime(result).ToString("yyyyMMdd");
            return string.Empty;
        }
    }
}
