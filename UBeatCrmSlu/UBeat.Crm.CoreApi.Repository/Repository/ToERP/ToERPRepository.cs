using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Repository.Repository.ToERP
{
    public class ToERPRepository : RepositoryBase, IToERPRepository
    {
        public string IsExistsOrder(string orderId)
        {
            var sql = " select recid from crm_sys_order where orderid=@orderid::int4";
            var param = new DbParameter[] {
                new NpgsqlParameter("orderid",orderId)
            };
            var result = ExecuteScalar(sql, param);
            return result != null ? result.ToString() : string.Empty;
        }
        public string GetOrderLastUpdatedTime()
        {
            var sql = " select recupdated from crm_sys_order order by recupdated desc limit 1";
            var result = ExecuteScalar(sql, null);
            if (result != null)
                return Convert.ToDateTime(result).ToString("yyyyMMdd");
            return "1990-01-01";
        }

        public string IsExistsPackingShipOrder(string packingshipid)
        {
            var sql = " select recid from crm_cee_shippingorder where packingshipid=@packingshipid::int4";
            var param = new DbParameter[] {
                new NpgsqlParameter("packingshipid",packingshipid)
            };
            var result = ExecuteScalar(sql, param);
            return result != null ? result.ToString() : string.Empty;
        }
        public string GetShippingOrderLastUpdatedTime()
        {
            var sql = " select recupdated from crm_cee_shippingorder order by recupdated desc limit 1";
            var result = ExecuteScalar(sql, null);
            if (result != null)
                return Convert.ToDateTime(result).ToString("yyyyMMdd");
            return "1990-01-01";
        }
        public string IsExistsMakeCollectionOrder(string makeColOrderId)
        {
            var sql = " select recid from crm_zj_receivables where makecollectionsorderid=@makecollectionsorderid::int4";
            var param = new DbParameter[] {
                new NpgsqlParameter("makecollectionsorderid",makeColOrderId)
            };
            var result = ExecuteScalar(sql, param);
            return result != null ? result.ToString() : string.Empty;
        }
    }
}
