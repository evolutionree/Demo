using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Dapper;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.Customer;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Customer
{
    public class CustomerRepository : RepositoryBase, ICustomerRepository
    {
        public List<Dictionary<string, object>> SelectFailedCustomer(int userNumber)
        {
            string sql = @"select recid from crm_sys_customer where ifsyn=2 ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("reccreator",userNumber)
            };
            return ExecuteQuery(sql, param, null);
        }
        public dynamic QueryCustRelate(DbTransaction tran, Guid custId)
        {
            string executeSql = "WITH RECURSIVE T (ID, NAME, PARENT_ID, PATH, DEPTH,manager)  AS ( " +
"    SELECT recid AS ID, recname as NAME, (precustomer->>'id')::text PARENT_ID,ARRAY[recid::TEXT] AS PATH, 1 AS DEPTH,recmanager " +
"    FROM crm_sys_customer " +
"    WHERE    recid=@custid " +
"    UNION ALL " +
"    SELECT   D.recid AS ID, D.recname as NAME,  (D.precustomer->>'id')::text AS PARENT_ID,D.recid::TEXT||T.PATH , T.DEPTH + 1 AS DEPTH,D.recmanager  " +
"    FROM crm_sys_customer D " +
"    JOIN T ON D.recid::text = T.PARENT_ID  AND D.recstatus=1    " +
"    )" +
"    SELECT ID, NAME, PARENT_ID, PATH, DEPTH,u.username,dept.deptname FROM T " +
"    LEFT JOIN crm_sys_userinfo u on u.userid=T.manager " +
"    LEFT JOIN crm_sys_account_userinfo_relate acc on acc.userid=T.manager AND acc.recstatus=1 " +
"    LEFT JOIN crm_sys_department dept on dept.deptid=acc.deptid " +
"     " +
"    ORDER BY DEPTH DESC LIMIT 1; " +
"; ";

            var args = new
            {
                custid = custId
            };
            var dyn = DataBaseHelper.QuerySingle<dynamic>(executeSql, args);

            //var dyn = DBHelper.ExecuteQuery<dynamic>(tran,executeSql, dynsqlParameters.ToArray()).FirstOrDefault();

            executeSql = "WITH RECURSIVE T (ID, NAME, PARENT_ID, PATH, DEPTH,manager)  AS ( " +
    "    SELECT recid::TEXT AS ID, recname as NAME,'00000000-0000-0000-0000-000000000000'::TEXT PARENT_ID,ARRAY[recid::TEXT] AS PATH, 1 AS DEPTH,recmanager " +
    "    FROM crm_sys_customer " +
    "    WHERE   recid=@recid AND recstatus=1" +
    "    UNION ALL " +
    "    SELECT   D.recid::TEXT AS ID, D.recname as NAME,  (D.precustomer->>'id')::text AS PARENT_ID,D.recid::TEXT||T.PATH , T.DEPTH + 1 AS DEPTH,D.recmanager  " +
    "    FROM crm_sys_customer D  " +
    "    JOIN T ON (D.precustomer IS NOT  NULL OR (D.precustomer->>'id')::text<>'') AND (D.precustomer->>'id')::TEXT = T.ID   AND recstatus=1" +
    "    )" +
    "    SELECT ID, NAME, PARENT_ID, PATH, DEPTH,u.username,dept.deptname FROM T " +
    "    LEFT JOIN crm_sys_userinfo u on u.userid=T.manager " +
    "    LEFT JOIN crm_sys_account_userinfo_relate acc on acc.userid=T.manager AND acc.recstatus=1 " +
    "    LEFT JOIN crm_sys_department dept on dept.deptid=acc.deptid " +
    "    ORDER BY PATH" +
    "; ";
            if (dyn.id == null) throw new Exception("客户关系参数异常");
            var param = new
            {
                RecId = dyn.id
            };
            var dynList = DataBaseHelper.Query<dynamic>(executeSql, param);
            return dynList;
        }



        public PageDataInfo<MergeCustEntity> GetMergeCustomerList(DbTransaction tran, string wheresql, string searchkey, DbParameter[] sqlParameters, int pageIndex, int pageSize)
        {
            searchkey = searchkey.Replace("'", "''");
            List<MergeCustEntity> resutl = new List<MergeCustEntity>();
            var sql =string.Format( @"SELECT e.recid  AS CustId ,e.recname AS CustName,e.recmanager AS Manager,u.username AS ManagerName,e.custstatus,d.dataval AS custstatus_name
                        FROM crm_sys_customer AS e
                        LEFT JOIN crm_sys_userinfo AS u ON u.userid = e.recmanager
                        LEFT JOIN crm_sys_dictionary AS d ON d.dictypeid=12 AND d.dataid=e.custstatus
                        WHERE e.recstatus=1 
                            and (e.recname like '%{0}%' or u.username like '%{0}%')
                        ",searchkey);
            if (!string.IsNullOrEmpty(wheresql))
                sql += string.Format(" AND {0}", wheresql);
            if (pageSize <= 0) {
                pageSize = 10;
            }
            if (pageIndex <= 0) {
                pageIndex = 1;
            }
            string PageDataSQL = string.Format(@"{0} order by e.recname limit {1} offset {2}", sql, pageSize, (pageIndex - 1) * pageSize);
            string PageCountSQL = string.Format(@"select count(*) TotalCount from ({1}) totalsql ", pageSize, sql);

            List<MergeCustEntity> data = ExecuteQuery<MergeCustEntity>(PageDataSQL,  sqlParameters,tran);
            PageInfo pageInfo=  ExecuteQuery<PageInfo>(PageCountSQL, sqlParameters, tran).FirstOrDefault();
            pageInfo.PageSize = pageSize;
            PageDataInfo<MergeCustEntity> retData = new PageDataInfo<MergeCustEntity>();
            retData.PageInfo = pageInfo;
            retData.DataList = data;
            return retData;
        }

        public bool IsWorkFlowCustomer(List<Guid> custids, int usernumber)
        {
            List<MergeCustEntity> resutl = new List<MergeCustEntity>();
            var sql = @"SELECT cr.relrecid FROM
crm_sys_workflow_case AS wc
INNER JOIN crm_sys_workflow_case_entity_relation AS cr ON wc.caseid=cr.caseid
WHERE (wc.auditstatus=0 OR wc.auditstatus=3 ) AND cr.relrecid =ANY (@recids) 
                        ";
            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("recids", custids.ToArray()));

            var temp = DBHelper.ExecuteScalar("", sql, sqlParameters.ToArray());
            return temp != null;
        }

        public List<Custcommon_Customer_Model> IsCustomerExist(Dictionary<string, object> fieldData)
        {
            var sql = @"
                        SELECT commonid,custid,relateindex FROM crm_sys_custcommon_customer_relate
                        WHERE commonid= (SELECT * FROM crm_func_entity_check_repeat('ac051b46-7a20-4848-9072-3b108f1de9b0', @fieldData, NULL))";
            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("fieldData", JsonHelper.ToJson(fieldData)));

            var temp = DBHelper.ExecuteQuery<Custcommon_Customer_Model>("", sql, sqlParameters.ToArray());
            return temp;
        }

        public bool UpdateCustomer(DbTransaction tran, Guid custid, Dictionary<string, object> updateFileds, int usernumber)
        {
            var sql = @"UPDATE crm_sys_customer SET ";
            StringBuilder updateFieldSql = new StringBuilder();
            var sqlParameters = new List<DbParameter>();
            foreach (var m in updateFileds)
            {
                updateFieldSql.Append(string.Format("{0}=@{0},", m.Key));
                sqlParameters.Add(new NpgsqlParameter(m.Key, m.Value));
            }
            sql = string.Format("{0} WHERE recid=@recid ", sql + updateFieldSql.ToString().Trim(','));
            sqlParameters.Add(new NpgsqlParameter("recid", custid));

            var result = DBHelper.ExecuteNonQuery(tran, sql, sqlParameters.ToArray());
            return result > 0;
        }

        /// <summary>
        /// 更新客户关联实体的关联数据
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="custid"></param>
        /// <param name="oldCustid"></param>
        /// <param name="tableName"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        public bool UpdateCustomerRelateDynamicEntity(DbTransaction tran, Guid custid, List<Guid> oldCustid, string tableName, int usernumber)
        {

            var existSqlParameters = new List<DbParameter>();
            existSqlParameters.Add(new NpgsqlParameter("oldrecrelateid", oldCustid.ToArray()));
            if (DBHelper.GetCount(tran, tableName, " recrelateid=ANY (@oldrecrelateid) ", existSqlParameters.ToArray()) <= 0)
                return true;

            var sql = string.Format(@"UPDATE {0} SET recrelateid=@recrelateid,recupdator=@recupdator,recupdated=@recupdated 
                                      WHERE recrelateid=ANY (@oldrecrelateid)", tableName);
            StringBuilder updateFieldSql = new StringBuilder();
            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("recrelateid", custid));
            sqlParameters.Add(new NpgsqlParameter("oldrecrelateid", oldCustid.ToArray()));
            sqlParameters.Add(new NpgsqlParameter("recupdator", usernumber));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));

            var result = DBHelper.ExecuteNonQuery(tran, sql, sqlParameters.ToArray());
            return result > 0;
        }

        public List<CustRelFieldModel> GetCustomerRelateField(DbTransaction tran, string tableName, string updateFiledName, Guid custid)
        {
            List<CustRelFieldModel> resutl = new List<CustRelFieldModel>();
            var sql = string.Format("SELECT recid,{0}::text AS Data FROM {1} WHERE (select {0}::jsonb->> 'id') LIKE '%'||@custid||'%'", updateFiledName, tableName);
            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("custid", custid.ToString()));
            return DBHelper.ExecuteQuery<CustRelFieldModel>(tran, sql, sqlParameters.ToArray());
        }

        public bool UpdateCustomerRelateField(DbTransaction tran, string tableName, string updateFiledName, object filedValue, Guid recid, int usernumber)
        {
            var existSqlParameters = new List<DbParameter>();
            existSqlParameters.Add(new NpgsqlParameter("recid", recid));
            if (DBHelper.GetCount(tran, tableName, " recid=@recid ", existSqlParameters.ToArray()) <= 0)
                return true;


            var result = 0;

            var sql = string.Format(" UPDATE {0} SET {1}=@{1} WHERE recid=@recid", tableName, updateFiledName);

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("recid", recid));
            sqlParameters.Add(new NpgsqlParameter(updateFiledName, filedValue) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });

            result = DBHelper.ExecuteNonQuery(tran, sql.ToString(), sqlParameters.ToArray());

            return result > 0;
        }

        public bool DeleteBeMergeCustomer(DbTransaction tran, List<Guid> oldCustid, int usernumber)
        {
            var existSqlParameters = new List<DbParameter>();
            existSqlParameters.Add(new NpgsqlParameter("oldrecid", oldCustid.ToArray()));
            if (DBHelper.GetCount(tran, "crm_sys_customer", " recid=ANY (@oldrecid) ", existSqlParameters.ToArray()) <= 0)
                return true;


            var result = 0;

            var sql = string.Format(@" UPDATE crm_sys_customer SET recstatus=0,recupdator=@recupdator,recupdated=@recupdated  WHERE recid=ANY (@oldrecid);
                                       DELETE FROM crm_sys_custcommon_customer_relate WHERE custid=ANY (@oldrecid);
                                       UPDATE crm_sys_custcommon SET recstatus=0,recupdator=@recupdator,recupdated=@recupdated  WHERE recid not in(
                                              SELECT commonid FROM crm_sys_custcommon_customer_relate );");

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("oldrecid", oldCustid.ToArray()));
            sqlParameters.Add(new NpgsqlParameter("recupdator", usernumber));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));

            result = DBHelper.ExecuteNonQuery(tran, sql.ToString(), sqlParameters.ToArray());

            return result > 0;


        }

        public string getSaleClueIdFromCustomer(DbTransaction transaction, string recid, int userNum)
        {
            try
            {
                string cmdText = "select jsonb_extract_path_text(saleclue,'id') clueid from crm_sys_customer  where recid='" + recid + "'";
                return (string)this.ExecuteScalar(cmdText, new DbParameter[] { }, transaction);
            }
            catch (Exception ex) {

            }
            return null;
        }

        public string rewriteSaleClue(DbTransaction transaction, string recid, int userNum)
        {
            try
            {
                string cmdText = string.Format("update crm_sys_salesclues set followstatus=3 where recid = '{0}' and (followstatus= 1 or followstatus = 2 or followstatus = 6 or followstatus is null) ", recid);
                this.ExecuteNonQuery(cmdText, new DbParameter[] { }, transaction);
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public string tryCreateContact(DbTransaction transaction, string saleclueid, string customerid) {
            return ""; 
        }

        /// <summary>
        /// 检查是否需要新增联系人
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="custid"></param>
        /// <returns></returns>
        public bool checkNeedAddContact(DbTransaction transaction,string saleclueid, string custid)
        {
            try
            {
                string cmdText = string.Format(@"select recphone from crm_sys_salesclues   where recid ='{0}'", saleclueid);
                string phone = (string)ExecuteScalar(cmdText, new DbParameter[] { }, transaction);
                if (phone == null || phone.Length == 0)
                {
                    return false;
                }
                cmdText = string.Format(@"select recid from crm_sys_contact  where mobilephone ='{0}' ", phone.Replace("'", "''"));
                object id = ExecuteScalar(cmdText, new DbParameter[] { }, transaction);
                if (id == null) return true;
                return false;
            }
            catch (Exception ex)
            {
            }
            return true;
        }

        public string getCommonIdByCustId(DbTransaction tran, string custid, int userId)
        {
            try
            {
                string strSQL = "select commonid from crm_sys_custcommon_customer_relate where custid::text =@custid  limit 1 ";
                DbParameter[] param = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@custid",custid)
                };
                object obj  = ExecuteScalar(strSQL, param, tran);
                if (obj == null) return null;
                return obj.ToString();
            }
            catch (Exception ex) {
                return null;
            }
        }

        public Dictionary<string, object> SelectTodayIndex(int userNumber)
        {
            string sql = @"select remaintasknum + 5 as index,todaysale from crm_plu_salework  where reccreator = @reccreator  ORDER BY reccreated DESC LIMIT 1";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("reccreator",userNumber)
            };
            return ExecuteQuery(sql, param, null).FirstOrDefault();
        }


        public List<Dictionary<string, object>> SelectCustomerOfVisitPlan(string beginDate, string endDate, int userNumber, DbTransaction tran)
        {
            string sql = @"select  vp.custname, cs.xhhhqf ->>'address' as address,cs.target,cs.reccreated  from (
select  custname ->> 'id' as custid , custname ->> 'name' as custname 
from crm_sys_visit_plan 
where recstatus = 1 and reccreator = @reccreator and recupdated > @beginDate::timestamp and recupdated < @endDate::timestamp 
) as vp INNER JOIN crm_sys_customer_summary as cs on vp.custid::uuid = cs.recrelateid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("reccreator",userNumber),
                new NpgsqlParameter("beginDate",beginDate),
                new NpgsqlParameter("endDate",endDate)
            };
            return ExecuteQuery(sql, param, tran);
        }

        public bool DistributionCustomer(List<string> recids, int userid, int currentUserid, DbTransaction tran)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var id in recids)
            {
                var sql = string.Format("update crm_sys_customer set recmanager = {0}, followstatus = 3 where recid = '{1}'::uuid ; \n", userid, id);
                sb.Append(sql);
            }
            return ExecuteNonQuery(sb.ToString(), new DbParameter[] { }, tran) > 0;
        }

		public List<CustContactTreeItemInfo> GetCustContactTree(Guid custid, int usernumber)
		{
			var treeitems = new List<CustContactTreeItemInfo>();
			PostgreHelper pgHelper = new PostgreHelper();
			using (DbConnection conn = pgHelper.GetDbConnect())
			{
				conn.Open();
				var tran = conn.BeginTransaction();
				try
				{
					var executeSql = @" SELECT 
										c.recid,
										c.recname,
										dic.dataval AS custdept,
										c.position,
										'' as custphoto,
										c.customer,
										c.precontact as supcontact  
										FROM crm_sys_contact AS c
										left join (
										 select dataid, dataval from crm_sys_dictionary where dictypeid=4 and recstatus = 1
										) dic on dic.dataid = c.contacttype
										WHERE  c.recstatus=1 and c.customer->>'id' = @id::text;";
					var p = new DbParameter[]
					{
						new NpgsqlParameter("id",custid)
					};
					var allContacts = pgHelper.ExecuteQuery<CustContactTreeItemInfo>(tran, executeSql, p);
					tran.Commit();
					var custContacts = allContacts.Where(m => m.Customer != null && m.Customer.Id == custid);


					foreach (var contact in custContacts)
					{
						if (contact != null && !treeitems.Exists(m => m.RecId == contact.RecId))
						{
							var index = treeitems.FindIndex(m => m.SupContact != null && m.SupContact.Id == contact.RecId);
							index = index <= 0 ? 0 : index - 1;
							treeitems.Insert(index, contact);
							GetParentContacts(contact, treeitems, allContacts);
							GetChildContacts(contact, treeitems, allContacts);

						}
						else if (contact != null)
						{
							GetChildContacts(contact, treeitems, allContacts);
						}

					}

				}
				catch (Exception ex)
				{
					tran.Rollback();
					throw ex;
				}
				finally
				{
					conn.Close();
					conn.Dispose();
				}

			}
			treeitems.Insert(0, new CustContactTreeItemInfo()
			{
				RecId = new Guid("10000000-0000-0000-0000-000000000000"),
				RecName = "联系人",
				HeadIcon = null,
			});

			return treeitems;
		}
		
		public List<CustContactTreeItemInfo> insertCustomer(Dictionary<string, object> cusdata)
		{
			/*StringBuilder sql = new StringBuilder();


            var insertSql = string.Format(@"INSERT INTO crm_sys_custcommon(
											recid, recname, recstatus, reccreator, recupdator,
                                            recmanager, reccreated, recupdated, rectype) VALUES ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10});
                                            INSERT INTO crm_sys_custcommon_customer_relate(
                                            commonid, custid, relateindex) VALUES('{1}', '{2}', 1);
                                            ",
	            cusdata["recid"],cusdata["recname"],1,1,1,cusdata["recmanager"],DateTime.Now,DateTime.Now,"f9db9d79-e94b-4678-a5cc-aa6e281c1246");

            sql.Append(string.Format(@"DELETE FROM crm_sys_custcommon WHERE recid = '{0}';", item.id));
            sql.Append(string.Format(@"DELETE FROM crm_sys_custcommon_customer_relate WHERE custid = '{0}';", item.id));
            sql.Append(insertSql);
            DataBaseHelper.ExecuteNonQuery(sql, addParameters, CommandType.Text) > 0;
            return sql.ToString();*/
			return null;
		}
		
		

        public List<CustFrameProtocolModel> GetCustFrameProtocol(Guid custid, int usernumber) {
            var items = new List<CustFrameProtocolModel>();
            PostgreHelper pgHelper = new PostgreHelper();
            using (DbConnection conn = pgHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {//->>'id' startdate::text||'至'||enddate::text as validity
                    var executeSql = @"SELECT 
                                    	recname,customer->>'name' as customername,signdept, '' as validity,t2.username:: text AS recmanager
                                    FROM 
                                    	(select ral.*,(ral.commonid->>'id')::uuid as ralcommonid from crm_sys_contract ral) AS t0 
                                    	LEFT JOIN crm_sys_custcommon_customer_relate AS t1 ON t0.ralcommonid = t1.commonid
                                        LEFT JOIN crm_sys_userinfo t2 on t2.userid=t0.recmanager
                                    WHERE 1=1 
                                    	AND rectype = '64d4a584-abe8-4b3d-8afb-8d5b4bbf4bbd' 
                                    	AND custid  = @id::uuid";
                    var p = new DbParameter[]
                    {
                        new NpgsqlParameter("id",custid)
                    };
                    items = pgHelper.ExecuteQuery<CustFrameProtocolModel>(tran, executeSql, p);
                    tran.Commit();

                    return items;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }

            }
        }
        
        public List<CustomerTemp> GetCustomerId()
        {
	        var items = new List<CustomerTemp>();
	        PostgreHelper pgHelper = new PostgreHelper();
	        using (DbConnection conn = pgHelper.GetDbConnect())
	        {
		        conn.Open();
		        var tran = conn.BeginTransaction();
		        try
		        {//->>'id' startdate::text||'至'||enddate::text as validity
			        var executeSql = @"SELECT recid,recname
                                    FROM crm_sys_customer
                                    WHERE 1=1";
			        
			        var p = new DbParameter[]
			        {
				        new NpgsqlParameter()
			        };
			        items = pgHelper.ExecuteQuery<CustomerTemp>(tran, executeSql,null);
			        tran.Commit();
			        return items;
		        }
		        catch (Exception ex)
		        {
			        tran.Rollback();
			        throw ex;
		        }
		        finally
		        {
			        conn.Close();
			        conn.Dispose();
		        }
	        }
        }

        public void updateUCode(string recname,string ucode, DbTransaction tran = null)
        {
	        var updateBillSql = "update crm_sys_customer set ucode=@ucode where recname=@recname and ucode is  null and businesscode  is not null ";
	        var updateParam = new DbParameter[] {
		        new NpgsqlParameter("ucode",ucode),
		        new NpgsqlParameter("recname",recname)
	        };
	        ExecuteNonQuery(updateBillSql, updateParam, tran);
	        
	        updateBillSql = "update crm_sys_customer set ucode=@ucode where recname=@recname and ucode is  null and businesscode  is not null ";
	        ExecuteNonQuery(updateBillSql, updateParam, tran);
        }

        public List<CustomerTemp> GetCustomerTemp()
        {
	        var items = new List<CustomerTemp>();
	        PostgreHelper pgHelper = new PostgreHelper();
	        using (DbConnection conn = pgHelper.GetDbConnect())
	        {
		        conn.Open();
		        var tran = conn.BeginTransaction();
		        try
		        {//->>'id' startdate::text||'至'||enddate::text as validity
				        var executeSql = @"
						SELECT * FROM customer_temp WHERE 
                                  recname not in (select recname  from crm_sys_customer)  ";
				        //and recname not in (select beforename  from crm_sys_customer where beforename is not null)
			        // var executeSql = @"
			        // SELECT * FROM customer_temp1 WHERE businesscenter  like '%智能网联汽车测试研发中心%'";

			        
			        var p = new DbParameter[]
			        {
				        new NpgsqlParameter()
			        };
			        items = pgHelper.ExecuteQuery<CustomerTemp>(tran, executeSql,null);
			        tran.Commit();
			        return items;
		        }
		        catch (Exception ex)
		        {
			        tran.Rollback();
			        throw ex;
		        }
		        finally
		        {
			        conn.Close();
			        conn.Dispose();
		        }
	        }
        }
        
        public List<Dictionary<string, object>> ExecuteQueryTran(string sql, DbParameter[] param, DbTransaction trans = null)
        {
	        return ExecuteQuery(sql, param, trans);
        }
        
        public int ExecuteNonQuery(string sql, DbParameter[] param)
        {
	        return ExecuteNonQuery(sql, param, null);
        }
        
        public List<CustomerTemp> GetCustomerWithOutUCodeIsNull()
        {
	        var items = new List<CustomerTemp>();
	        PostgreHelper pgHelper = new PostgreHelper();
	        using (DbConnection conn = pgHelper.GetDbConnect())
	        {
		        conn.Open();
		        var tran = conn.BeginTransaction();
		        try
		        {//->>'id' startdate::text||'至'||enddate::text as validity
			        var executeSql = @"
						SELECT recname FROM crm_sys_customer WHERE 
						          ucode is  null and businesscode  is not null ";
			        //recname not in (select recname  from crm_sys_customer)  ";
			        //and recname not in (select beforename  from crm_sys_customer where beforename is not null)
			        // var executeSql = @"
			        // SELECT * FROM customer_temp1 WHERE businesscenter  like '%智能网联汽车测试研发中心%'";

			        
			        var p = new DbParameter[]
			        {
				        new NpgsqlParameter()
			        };
			        items = pgHelper.ExecuteQuery<CustomerTemp>(tran, executeSql,null);
			        tran.Commit();
			        return items;
		        }
		        catch (Exception ex)
		        {
			        tran.Rollback();
			        throw ex;
		        }
		        finally
		        {
			        conn.Close();
			        conn.Dispose();
		        }
	        }
        }
        

        public List<UserInfo> getUserInfo(string username)
        {
	        var items = new List<UserInfo>();
	        PostgreHelper pgHelper = new PostgreHelper();
	        using (DbConnection conn = pgHelper.GetDbConnect())
	        {
		        conn.Open();
		        var tran = conn.BeginTransaction();
		        try
		        {//->>'id' startdate::text||'至'||enddate::text as validity
			        var executeSql = @"SELECT userid
                                    FROM crm_sys_userinfo
                                    WHERE username=@username";
			        
			        var p = new DbParameter[]
			        {
				        new NpgsqlParameter("username",username)
			        };
			        items = pgHelper.ExecuteQuery<UserInfo>(tran, executeSql,p);
			        tran.Commit();
			        
		        }
		        catch (Exception ex)
		        {
			        tran.Rollback();
			        throw ex;
		        }
		        finally
		        {
			        conn.Close();
			        conn.Dispose();
		        }
		        return items;
	        }
        }

        private void GetParentContacts(CustContactTreeItemInfo contact, List<CustContactTreeItemInfo> treeitems, List<CustContactTreeItemInfo> allContacts)
		{
			if (contact == null)
				return;
			contact.ParantRecId = new Guid("10000000-0000-0000-0000-000000000000");
			if (contact.SupContact != null)
			{
				var index = treeitems.FindIndex(m => m.RecId == contact.RecId);
				var parentContact = allContacts.Find(m => m.RecId == contact.SupContact.Id);
				if (parentContact != null)
					contact.ParantRecId = parentContact.RecId;
				if (parentContact != null && !treeitems.Exists(m => m.RecId == parentContact.RecId))
				{
					var indextemp = index <= 0 ? 0 : index - 1;
					treeitems.Insert(indextemp, parentContact);
					GetParentContacts(parentContact, treeitems, allContacts);

				}

			}

		}
		private void GetChildContacts(CustContactTreeItemInfo contact, List<CustContactTreeItemInfo> treeitems, List<CustContactTreeItemInfo> allContacts)
		{
			if (contact == null)
				return;

			var childContacts = allContacts.Where(m => m.SupContact != null && contact.RecId == m.SupContact.Id);
			foreach (var childContact in childContacts)
			{
				var temItem = treeitems.FirstOrDefault(m => m.RecId == childContact.RecId);
				if (temItem == null)
				{
					childContact.ParantRecId = contact.RecId;
					treeitems.Insert(treeitems.Count, childContact);
				}
				if (childContact.IsSearchChildren == false)
				{
					childContact.IsSearchChildren = true;
					GetChildContacts(childContact, treeitems, allContacts);
				}

			}

		}
	}
}
