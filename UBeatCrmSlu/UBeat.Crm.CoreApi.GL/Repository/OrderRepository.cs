using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.GL.Repository
{
    public class OrderRepository : RepositoryBase, IOrderRepository
    {
        public int UpdateOrderSapCode(Guid recId, string sapCode, Dictionary<string, string> lineDic, DbTransaction tran = null)
        {
            var updateSql = @"update crm_fhsj_order set orderid = @sapCode, recupdated = now() where recid = @recId;
                    update crm_fhsj_product_detail set sapsynchstatus=2
                           where recid in (SELECT  d.recid
						                    FROM
							                    crm_fhsj_product_detail d
						                    INNER JOIN (
							                    SELECT
								                    UNNEST (
									                    string_to_array(productdetail, ',')
								                    ) :: uuid AS detailid
							                    FROM
								                    crm_fhsj_order de
							                    WHERE
								                    recstatus = 1 and  recId=@recId
						                    ) o ON o.detailid = d.recid)";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("recId",recId),
                new NpgsqlParameter("sapCode", sapCode),
            };

            var result = 0;
            if (tran == null)
                result = DBHelper.ExecuteNonQuery("", updateSql, param);
            else
                result = DBHelper.ExecuteNonQuery(tran, updateSql, param);
            if (result > 0 && lineDic.Count > 0)
                result = InsertOrderDetailSap(recId, lineDic);
            return result;
        }

        public Dictionary<string, object> GetContract(string contractno)
        {
            var sql = @"select * from crm_sys_contract where recstatus=1 and contractno = @contractno;";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("contractno",contractno)
            };
            List<Dictionary<string, object>> list = ExecuteQuery(sql, param);
            if (list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }

        public Dictionary<string, object> GetSapOrderByCode(string code)
        {
            var sql = @"select * from crm_fhsj_order a where recstatus=1 and orderid =lpad(@orderid, 10, '0');";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("orderid",code)
            };
            List<Dictionary<string, object>> list = ExecuteQuery(sql, param);
            if (list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }

        public List<Dictionary<string, object>> GetOrderDetailByCodeSortProdAndAccount(string code, DbTransaction tran = null)
        {
            var sql = @"select *,COALESCE(d.saleprice,0) price  from crm_fhsj_product_detail d inner join (
                select regexp_split_to_table(productdetail, ',')::uuid as itemid,recid orderid,saletocode,orderid ocode  from crm_fhsj_order  where   recstatus=1  and saletocode is not null and productdetail<>'' and orderid=lpad(@orderid, 10, '0') ) m
                  on m.itemid=d.recid  order by d.productname,(d.shippingaccount - d.returnaccount-d.salereturnaccount) desc ;";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("orderid",code)
            };
            List<Dictionary<string, object>> list = ExecuteQuery(sql, param, tran);

            return list;

        }

        public List<Dictionary<string, object>> GetDeliDetailByAccount(Guid returnId, DbTransaction tran = null)
        {
            var sql = @"select recid,account,rownum::int,orderrownum::int,orirecid,m.shipmentordercode,m.targetrecid from crm_fhsj_shipment_order_detail d  inner join (
					 select regexp_split_to_table(shipmentdetail,'','')::uuid itemid,recid targetrecid,shipmentordercode,(ordercode->>''id'')::uuid orirecid
						from crm_fhsj_shipment_order where  ordercode is not null and (ordercode->>''id'')::uuid in (select orirecid from crm_fhsj_order_return where returnid=@returnId )
					and recstatus=1 and shipmentdetail<>'''' and pickstatus=2) m
			 on  m.itemid=d.recid where d.orderrownum is not null  order by orirecid,orderrownum,account desc  ;";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("returnId",returnId)
            };
            List<Dictionary<string, object>> list = ExecuteQuery(sql, param, tran);

            return list;

        }
        public List<Dictionary<string, object>> GetOrderDetailByContractSortProdAndAccount(string code, DbTransaction tran = null)
        {
            var sql = @" select *,COALESCE(d.saleprice,0) price from crm_fhsj_product_detail d inner join (
				select regexp_split_to_table(productdetail, ',')::uuid as itemid,recid orderid,orderid ocode,saletocode,ordertype  from crm_fhsj_order where recstatus=1  and ordertype in (4,11)  and saletocode is not null  and productdetail<>'' 
                and contractno is not null and (contractno->>'id')::uuid in (select recid from crm_sys_contract where contractno=@contractno limit 1)  ) m
		        on m.itemid=d.recid order by ordertype,d.productname,(d.shippingaccount - d.returnaccount-d.salereturnaccount) desc  ;";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("contractno",code)
            };
            List<Dictionary<string, object>> list = ExecuteQuery(sql, param, tran);

            return list;

        }
        public List<Guid> GetOrderListInitReturnByBooking()
        {
            var sql = @"select recid from crm_fhsj_order where direction=-1 and recstatus=1 and  productdetail<>''
                and oldordercode is null  and perdormstatus in (1,2) and custrefernum ~'^预售\d'   and orderid is not null;";
            var param = new DynamicParameters();
            return DataBaseHelper.Query<Guid>(sql, param, CommandType.Text);

        }

        public List<Guid> GetOrderListInitReturn()
        {
            var sql = @"select recid from crm_fhsj_order where direction=-1 and recstatus=1 and  productdetail<>''
                and oldordercode is null  and perdormstatus in (1,2) and custrefernum ~'^\d'   and orderid is not null;";
            var param = new DynamicParameters();
            return DataBaseHelper.Query<Guid>(sql, param, CommandType.Text);

        }
        public List<Guid> GetOrderListInitCrmReturn()
        {
            var sql = @"select returnid from crm_fhsj_order_return where type in(0,1) group by returnid;";
            var param = new DynamicParameters();
            return DataBaseHelper.Query<Guid>(sql, param, CommandType.Text);

        }
        public List<Guid> GetOrderListInitReturnByContract()
        {
            var sql = @"select o.recid from  (
                select recid,custrefernum from crm_fhsj_order where direction=-1 and recstatus=1 and  productdetail<>''  and oldordercode is null and custrefernum ~ '^[a-zA-z]'   
                  and perdormstatus in (1,2) and orderid is not null and recid not in (select returnid from crm_fhsj_order_return group by returnid) ) o
                  inner join crm_sys_contract c on o.custrefernum=c.contractno and c.recstatus=1 ;";
            var param = new DynamicParameters();
            return DataBaseHelper.Query<Guid>(sql, param, CommandType.Text);

        }
        public List<Dictionary<string, object>> GetOrderListInitOccupy()
        {
            var sql = @"select (tuningincontract->>'id')::uuid contractid,tuningincontract->>'name' contractno,recid returnid from crm_fhsj_order where  recstatus=1 and tuningincontract is not null  
                       and isntincludecontract=1  and ordertype=11 ";
            var param = new DbParameter[]
            {
            };
            List<Dictionary<string, object>> list = ExecuteQuery(sql, param);

            return list;

        }

        public bool DropHasReturnOrderDetailById(Guid returnId, DbTransaction tran = null)
        {
            var sql = @"delete from crm_fhsj_order_return where type in (0,1) and returnid=@returnId;";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("returnId",returnId)
            };
            var result = ExecuteNonQuery(sql, param, tran) > 0;
            return result;

        }

        #region init data
        public List<string> getAddCode(List<string> codeList)
        {
            List<string> list = new List<string>();

            var sql = string.Format(@"
                select orderid from crm_fhsj_order where recstatus = 1 and orderid in ('{0}');", string.Join("','", codeList));

            var result = DataBaseHelper.Query(sql, null, CommandType.Text);
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    var code = string.Concat(item["orderid"]);
                    if (string.IsNullOrEmpty(code)) continue;

                    if (codeList.Contains(code))
                        codeList.Remove(code);
                }
            }

            return codeList;
        }

        public List<string> getModifyCode(List<string> codeList)
        {
            List<string> list = new List<string>();

            var sql = string.Format(@"
                select orderid from crm_fhsj_order where recstatus = 1 and orderid in ('{0}');", string.Join("','", codeList));

            var result = DataBaseHelper.Query(sql, null, CommandType.Text);
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    var code = string.Concat(item["orderid"]);
                    if (string.IsNullOrEmpty(code)) continue;

                    list.Add(string.Concat(code));
                }
            }

            return list;
        }

        public List<string> getDeleteCode(List<string> codeList)
        {
            List<string> list = new List<string>();

            var sql = string.Format(@"
                select orderid from crm_fhsj_order where recstatus = 1 and orderid in ('{0}');", string.Join("','", codeList), Guid.Empty);

            var result = DataBaseHelper.Query(sql, null, CommandType.Text);
            if (result.Count > 0)
            {
                foreach (var item in result)
                {
                    var code = string.Concat(item["orderid"]);
                    if (string.IsNullOrEmpty(code)) continue;

                    list.Add(code);
                }
            }

            return list;
        }

        public void calcOrderOccupyByContract(Guid returnid, string contractno, Guid contractid)
        {
            var strSql = @" SELECT * FROM crm_func_calcorder_occupy(@returnid,@contractno,@contractid) ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("returnid",returnid),
                new NpgsqlParameter("contractno",contractno),
                new NpgsqlParameter("contractid",contractid),
            };
            ExecuteQuery(strSql, param);
        }


        public bool DeleteList(List<string> codeList, int userId)
        {
            var result = false;

            StringBuilder sql = new StringBuilder();
            foreach (var item in codeList)
            {
                var setters = new List<string>();
                setters.Add(string.Format("{0} = {1}", "recstatus", 0));
                setters.Add(string.Format("{0} = {1}", "recupdator", userId));
                setters.Add(string.Format("{0} = '{1}'", "recupdated", DateTime.Now));
                setters.Add(string.Format("{0} = '{1}'", "reconlive", DateTime.Now));

                var updateSql = string.Format(@"update crm_fhsj_order set {0} where orderid = '{1}';", string.Join(",", setters), item);

                sql.Append(updateSql);
            }

            var finalSql = sql.ToString();
            if (!string.IsNullOrEmpty(finalSql))
            {
                result = DataBaseHelper.ExecuteNonQuery(finalSql, null, CommandType.Text) > 0;
            }
            return result;
        }
        #endregion

        public int InsertOrderDetailSap(Guid orderId, Dictionary<string, string> lineDic, DbTransaction tran = null)
        {
            var updateParam = new DynamicParameters();
            var sqlSB = new StringBuilder();
            foreach (var item in lineDic)
            {
                sqlSB.AppendFormat(@"INSERT INTO crm_fhsj_order_detail_reg(recid, orderid, recitemid, rownum, status, cttime, sapflag)
									VALUES('{0}', '{1}', '{2}', {3}, 1, now(), 1);", Guid.NewGuid().ToString(), orderId.ToString(), item.Value, item.Key);
            }
            var param = new DbParameter[]
            {
            };

            if (tran == null)
                return DBHelper.ExecuteNonQuery("", sqlSB.ToString(), param);

            var result = DBHelper.ExecuteNonQuery(tran, sqlSB.ToString(), param);
            return result;
        }

        public int DeleteOrderDetailSap(List<Guid> recItemIds, DbTransaction tran = null)
        {
            var updateParam = new DynamicParameters();
            var sqlSB = new StringBuilder();
            foreach (var item in recItemIds)
            {
                sqlSB.AppendFormat("delete from crm_fhsj_order_detail_reg where recitemid = '{0}';", item);
            }

            var param = new DbParameter[]
             {
             };

            if (tran == null)
                return DBHelper.ExecuteNonQuery("", sqlSB.ToString(), param);

            var result = DBHelper.ExecuteNonQuery(tran, sqlSB.ToString(), param);
            return result;
        }

        public int UpdateOrderDetailStatusSap(Guid orderId, int isSynchrosap, DbTransaction tran = null)
        {
            var sqlSB = @"update crm_fhsj_product_detail set sapsynchstatus = @isSynchrosap, recupdated = now() where recid in (select regexp_split_to_table(productdetail, ',')::uuid ids from crm_fhsj_order   where productdetail <> '' and recid = @orderId); ";

            var param = new DbParameter[]
             {
                  new NpgsqlParameter("isSynchrosap", isSynchrosap),
                  new NpgsqlParameter("orderId", orderId)
             };

            if (tran == null)
                return DBHelper.ExecuteNonQuery("", sqlSB.ToString(), param);

            var result = DBHelper.ExecuteNonQuery(tran, sqlSB.ToString(), param);
            return result;
        }

        public Dictionary<string, object> IsExistOrder(SoOrderDataModel sapOrder)
        {
            var sqlExist = @"select recid,datasources  from crm_sys_order where orderid=@orderid and recstatus=1 limit 1;";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("orderid",sapOrder.VBELN)
            };
            List<Dictionary<string, object>> list = ExecuteQuery(sqlExist, param);
            if (list.Count > 0)
            {
                return list[0];
            }
            else
            {
                return null;
            }
        }
    }
}
