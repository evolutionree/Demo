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
        public string IsExistsPackingShipOrder(string packingshipid)
        {
            var sql = " select recid from crm_cee_shippingorder where packingshipid=@packingshipid::int4";
            var param = new DbParameter[] {
                new NpgsqlParameter("packingshipid",packingshipid)
            };
            var result = ExecuteScalar(sql, param);
            return result != null ? result.ToString() : string.Empty;
        }
    }
}
