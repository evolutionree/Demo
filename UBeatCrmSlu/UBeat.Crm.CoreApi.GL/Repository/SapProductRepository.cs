using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper;
using Npgsql;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.GL.Repository
{
    public class SapProductRepository : RepositoryBase, ISapProductRepository
    {
        public DictionaryDataModel getProductLine(string productcode)
        {
            var sql = string.Format(@"select * from  crm_sys_dictionary d 
                INNER JOIN(SELECT defaultline line 
                from crm_fhsj_productline_contrast where defaultline is not null and recstatus=1  and productcode=@productcode  ) o on o.line = d.dataid  
                where  d.dictypeid=76 limit 1;");

            var updateParam = new DynamicParameters();

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productcode", productcode)

            };
            return ExecuteQuery<DictionaryDataModel>(sql, param).FirstOrDefault();
        }
        public dynamic getProductInfoById(Guid recId)
        {
            var sql = string.Format(@"
                select * from crm_sys_product where recstatus = 1 and recid = @recId;");

            var updateParam = new DynamicParameters();
            updateParam.Add("recId", recId);

            var result = DataBaseHelper.Query(sql, updateParam, CommandType.Text);
            return result;
        }

        public dynamic getProductInfoByIds(List<Guid> recIds)
        {
            var sql = string.Format(@"
                select * from crm_sys_product where 1 = 1 and recid in ('{0}');", string.Join("','", recIds));

            var updateParam = new DynamicParameters();
            updateParam.Add("recId", string.Concat("'", string.Join("','", recIds), "'"));

            var result = DataBaseHelper.Query(sql, updateParam, CommandType.Text);
            return result;
        }

        public ProductModel IsExitProduct(String productCode, int userId)
        {
            var sql = @"select *  from crm_sys_product where productcode=@productcode limit 1;";
            var param = new DynamicParameters();
            param.Add("productcode", productCode);
            return DataBaseHelper.QuerySingle<ProductModel>(sql, param);
        }

        public ProductSeries IsExitProductSeries(String productsetcode, int userId)
        {

            var sql = @"select productsetid,productsetcode as seriescode from crm_sys_products_series where productsetcode=@productsetcode and recstatus=1 limit 1;";
            var param = new DynamicParameters();
            param.Add("productsetcode", productsetcode);
            return DataBaseHelper.QuerySingle<ProductSeries>(sql, param);
        }

        public OperateResult EditProductSeries(DbTransaction trans, ProductSeries data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_productseries_update(@productsetid,@seriesname,@seriescode,@userno,@serieslanguage::jsonb)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productsetid", data.ProductsetId.ToString()),
                new NpgsqlParameter("seriesname", data.SeriesName),
                new NpgsqlParameter("seriescode", data.SeriesCode),
                new NpgsqlParameter("serieslanguage",null),
                new NpgsqlParameter("userno", userNumber),

            };

            return ExecuteQuery<OperateResult>(executeSql, param, trans).FirstOrDefault();

        }

        public OperateResult AddProductSeries(DbTransaction trans, ProductSeries data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_productseries_insert(@topseriesid,@seriesname,@seriescode,@userno,@serieslanguage::jsonb)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("topseriesid", data.TopSeriesId),
                new NpgsqlParameter("seriesname", data.SeriesName),
                new NpgsqlParameter("seriescode", data.SeriesCode),
                new NpgsqlParameter("serieslanguage", null),
                new NpgsqlParameter("userno", userNumber),

            };
            if (trans == null)
            {
                return DBHelper.ExecuteQuery<OperateResult>("", executeSql, param).FirstOrDefault();
            }
            return DBHelper.ExecuteQuery<OperateResult>(trans, executeSql, param).FirstOrDefault();

        }

        public void DelAllProductFactory( int userId) {
            var sql = @"TRUNCATE crm_fhsj_materialfactory_relate;";
            DataBaseHelper.ExecuteNonQuery(sql, null, CommandType.Text);
        }
        public void DelAllProductSaleOrg(int userId)
        {
            var sql = @"TRUNCATE crm_fhsj_materialsale_relate  ;";
            DataBaseHelper.ExecuteNonQuery(sql, null, CommandType.Text);
        }

        public void DelAllProductFactoryTemp(int userId)
        {
            var sql = @"TRUNCATE crm_fhsj_materialfactory_relate_temp;";
            DataBaseHelper.ExecuteNonQuery(sql, null, CommandType.Text);
        }
        public void DelAllProductSaleOrgTemp(int userId)
        {
            var sql = @"TRUNCATE crm_fhsj_materialsale_relate_temp  ;";
            DataBaseHelper.ExecuteNonQuery(sql, null, CommandType.Text);
        }
        public void ProductFactoryTempToFormal(int userId)
        {
            var sql = @"TRUNCATE crm_fhsj_materialfactory_relate;
                      insert into crm_fhsj_materialfactory_relate select * from crm_fhsj_materialfactory_relate_temp;";
            DataBaseHelper.ExecuteNonQuery(sql, null, CommandType.Text);
        }

        public void ProductSaleOrgTempToFormal(int userId)
        {
            var sql = @"TRUNCATE crm_fhsj_materialsale_relate;
                      insert into crm_fhsj_materialsale_relate select * from crm_fhsj_materialsale_relate_temp;";
            DataBaseHelper.ExecuteNonQuery(sql, null, CommandType.Text);
        }


        public List<Dictionary<string, object>> GetStockMapping(DbTransaction trans, int userId)
        {
            try
            {
                string strSQL = "select * from crm_sys_dictionary where dictypeid = 102";
                return ExecuteQuery(strSQL, new DbParameter[] { }, trans);
            }
            catch (Exception ex) {
                throw (ex);
            }
        }

        public List<Dictionary<string, object>> GetDeliveryStock(Guid productId)
        {
            var executeSql = @"	SELECT x.productid,x.inventory,SUM(COALESCE(x.account,0)) AS account	
                          FROM (SELECT d.productname as productid,d.account,d.inventory FROM crm_fhsj_shipment_order_detail d 
                          INNER JOIN (SELECT UNNEST (string_to_array(shipmentdetail, ',')) :: uuid AS detailid FROM crm_fhsj_shipment_order de 
                          WHERE recstatus = 1 and pickstatus=1 ) o ON o.detailid = d.recid WHERE d.productname='de945274-f207-4f12-b9b0-a87332417a05' and 
							    d.inventory IS NOT NULL) x GROUP BY x.productid,x.inventory";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productId", productId),
            };

            return ExecuteQuery(executeSql, param);

        }
        public List<Dictionary<string, object>> GetDeliveryStock(List<Guid> productIds)
        {
            var executeSql = @"	SELECT x.productid,x.inventory,SUM(COALESCE(x.account,0)) AS account	
                          FROM (SELECT d.productname as productid,d.account,d.inventory FROM crm_fhsj_shipment_order_detail d 
                          INNER JOIN (SELECT UNNEST (string_to_array(shipmentdetail, ',')) :: uuid AS detailid FROM crm_fhsj_shipment_order de 
                          WHERE recstatus = 1 and pickstatus=1 ) o ON o.detailid = d.recid WHERE d.productname::uuid =any(@productids)  and 
							    d.inventory IS NOT NULL) x GROUP BY x.productid,x.inventory";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("productids", productIds.ToArray()),
            };

            return ExecuteQuery(executeSql, param);

        }

        public dynamic getProductInfoId()
        {
            var sql = "select * from crm_sys_product where 1 = 1 ;";

            var result = DataBaseHelper.Query(sql, CommandType.Text);
            return result;
        }

        public bool delProductInfoMprice()
        {
            var sql = "delete from crm_fhsj_bussinesscentre_productprice where 1 = 1 ";

            var param = new DbParameter[]
            {
            };
            var result = ExecuteNonQuery(sql, param);
            return true;
        }
    }
}
