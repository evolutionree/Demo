using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.Repository.Repository;

namespace UBeat.Crm.CoreApi.ZGQY.Repository
{
    public class DelivnoteRepository : RepositoryBase, IDelivnoteRepository
    {
        public string GetOrderNoByRecId(string recid)
        {
            string sql = @"select orderid from crm_sys_order where recid = @recid::uuid";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("recid",recid)
            };
            return ExecuteScalar(sql, p)?.ToString();
        }

        public int UpdateDeliverySapCode(Guid recId, string sapCode, DbTransaction tran = null)
        {
            var updateSql = string.Format("update crm_glsc_deliveryorder set code = @sapCode, recupdated = now() where recid = @recId;");
            var param = new DbParameter[]
            {
                new NpgsqlParameter("recId",recId),
                new NpgsqlParameter("sapCode", sapCode),
            };

            if (tran == null)
                return DBHelper.ExecuteNonQuery("", updateSql, param);

            var result = DBHelper.ExecuteNonQuery(tran, updateSql, param);
            return result;
        }

        public Guid GetRecIdByCaseId(Guid caseId, DbTransaction tran)
        {
            string sql = @"select recid from crm_sys_workflow_case where caseid = @id";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("id",caseId)
            };
            var result = ExecuteScalar(sql, p, tran);
            Guid g;
            Guid.TryParse(result.ToString(), out g);
            return g;
        }
        public Dictionary<string, object> GetOrderInfo(string orderCode)
        {
            string sql = @"select customer,salesdepartments,salesterritory,jsonb_build_object('id',recid,'name',orderid) as orderjson,
                        recmanager
                        from crm_sys_order where recstatus = 1 and  orderid = @code ";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("code",orderCode)
            };
            return ExecuteQuery(sql, p).FirstOrDefault();
        }

        public string GetCrmProduct(string productCode)
        {
            string sql = @"select recid from crm_sys_product where productcode = @code and recstatus = 1";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("code",productCode)
            };
            return ExecuteScalar(sql, p)?.ToString();
        }

        public Guid IsExistsDelivnote(string code)
        {
            string sql = @"select recid from crm_glsc_deliveryorder 
                    where code = @code and recstatus = 1  limit 1";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("code",code)
            };
            var res = ExecuteScalar(sql, p);
            Guid g;
            Guid.TryParse(res?.ToString(), out g);
            return g;
        }
    }
}
