using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper;
using Npgsql;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.GL.Repository
{
    public class BaseDataRepository: RepositoryBase, IBaseDataRepository
	{
        public bool HasDicTypeId(Int32 typeId)
        {
            string sql = @"select dictypeid from crm_sys_dictionary_type where dictypeid = @typeId";

            var param = new DynamicParameters();
            param.Add("typeId", typeId);

            return DataBaseHelper.ExecuteScalar<Int32>(sql, param, CommandType.Text) > 0;
        }

        public int GetNextDataId(Int32 dicTypeId)
        {
            try
            {
                string strSQL = "select max(dataid) dataid  from crm_sys_dictionary where dictypeid = @dictypeid";
                var param = new DynamicParameters();
                param.Add("@dictypeid", dicTypeId);

                object obj = DataBaseHelper.ExecuteScalar<object>(strSQL, param, CommandType.Text);
                if (obj == null) return 1;
                else return int.Parse(obj.ToString()) + 1;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        public List<SaveDicData> GetDicDataByTypeId(Int32 typeId)
        {
            string sql = @"select * from crm_sys_dictionary where dictypeid = @typeId";

            var param = new DynamicParameters();
            param.Add("typeId", typeId);

            return DataBaseHelper.Query<SaveDicData>(sql, param, CommandType.Text);
        }

        public bool AddDictionary(SaveDicData entity)
        {
            string sql = @"insert into crm_sys_dictionary(
                            dicid,dictypeid,dataid,dataval,recorder,
                            recstatus,reccreated,recupdated,reccreator,recupdator,
                            extfield1,extfield2,extfield3,extfield4)
                            values (
                            @dicid,@dictypeid::int4,@dataid,@dataval,@recorder,
                            @recstatus,@reccreated,@recupdated,@reccreator,@recupdator,
                            @extfield1,@extfield2,@extfield3,@extfield4)";
            var param = new DynamicParameters();
            param.Add("dicid", entity.DicId);
            param.Add("dictypeid", entity.DicTypeId);
            param.Add("dataid", entity.DataId);
            param.Add("dataval", entity.DataVal);
            param.Add("recorder", entity.RecOrder);

            param.Add("recstatus", entity.RecStatus);
            param.Add("reccreated", entity.RecCreated);
            param.Add("recupdated", entity.RecUpdated);
            param.Add("reccreator", entity.RecCreator);
            param.Add("recupdator", entity.RecUpdator);

            param.Add("extfield1", entity.ExtField1);
            param.Add("extfield2", entity.ExtField2);
            param.Add("extfield3", entity.ExtField3);
            param.Add("extfield4", entity.ExtField4);

            return DataBaseHelper.ExecuteNonQuery(sql, param, CommandType.Text) > 0;
        }

        public bool UpdateDictionary(SaveDicData entity)
        {
            string sql = @"update crm_sys_dictionary set
                            dataval = @dataval,
                            recstatus = @recstatus,
                            recupdated = @recupdated,
                            recupdator = @recupdator,

                            extfield1 = @extfield1,
                            extfield2 = @extfield2,
                            extfield3 = @extfield3,
                            extfield4 = @extfield4
                            where dicid = @dicid";

            var param = new DynamicParameters();
            param.Add("dataval", entity.DataVal);
            param.Add("recstatus", entity.RecStatus);
            param.Add("recupdated", entity.RecUpdated);
            param.Add("recupdator", entity.RecUpdator);

            param.Add("extfield1", entity.ExtField1);
            param.Add("extfield2", entity.ExtField2);
            param.Add("extfield3", entity.ExtField3);
            param.Add("extfield4", entity.ExtField4);
            param.Add("dicid", entity.DicId);

            return DataBaseHelper.ExecuteNonQuery(sql, param, CommandType.Text) > 0;

        }

        public string GetDicDataByTypeIdAndId(Int32 typeId, Int32 dataId)
        {
            string sql = @"select extfield1 from crm_sys_dictionary where dictypeid = @typeId and dataid = @dataId";

            var param = new DynamicParameters();
            param.Add("typeId", typeId);
            param.Add("dataId", dataId);

            return DataBaseHelper.ExecuteScalar<string>(sql, param, CommandType.Text);
        }

        public string GetDicDatavalByTypeIdAndId(Int32 typeId, Int32 dataId)
        {
            string sql = @"select dataval from crm_sys_dictionary where dictypeid = @typeId and dataid = @dataId";

            var param = new DynamicParameters();
            param.Add("typeId", typeId);
            param.Add("dataId", dataId);

            return DataBaseHelper.ExecuteScalar<string>(sql, param, CommandType.Text);
        }

        public string GetSapCodeByTypeIdAndId(Int32 typeId, string dataId)
        {
            string sql = @"select extfield1 from crm_sys_dictionary where dictypeid = @typeId and dataid::text = @dataId";

            var param = new DynamicParameters();
            param.Add("typeId", typeId);
            param.Add("dataId", dataId);

            var result = DataBaseHelper.ExecuteScalar<string>(sql, param, CommandType.Text);
            if (string.IsNullOrEmpty(result))
                result = string.Empty;
            return result;
        }

        public string GetDicDataByTypeIdAndExtField1(Int32 typeId, string extfield1)
        {
            string sql = @"select dataid from crm_sys_dictionary where dictypeid = @typeId and extfield1 = @extfield1";

            var param = new DynamicParameters();
            param.Add("typeId", typeId);
            param.Add("extfield1", extfield1);

            return DataBaseHelper.ExecuteScalar<string>(sql, param, CommandType.Text);
        }

        public List<SaveDicData> GetDicData()
        {
            string sql = @"select * from crm_sys_dictionary where recstatus = 1";

            var param = new DynamicParameters(); 

            return DataBaseHelper.Query<SaveDicData>(sql, param, CommandType.Text);
        }

        public int UpdateSynStatus(Guid entityId, Guid recId, int isSynchrosap, DbTransaction tran = null)
        {
            var result = 0;
            var selectSql = "select entitytable from crm_sys_entity where entityid = @entityId limit 1;";

            var selectParam = new DynamicParameters();
            selectParam.Add("entityId", entityId);
            var tableName = DataBaseHelper.ExecuteScalar<string>(selectSql, selectParam);
            if (!string.IsNullOrEmpty(tableName))
            {
                var updateSql = string.Format("update {0} set issynchrosap = @isSynchrosap, recupdated = now()  where recid = @recId;", tableName); 
				var param = new DbParameter[]
				{
					new NpgsqlParameter("recId",recId), 
					new NpgsqlParameter("isSynchrosap", isSynchrosap), 
				};

				if (tran == null)
					return DBHelper.ExecuteNonQuery("", updateSql, param);

				result = DBHelper.ExecuteNonQuery(tran, updateSql, param);
			}

            return result;
        }

        public int UpdateSynTipMsg(Guid entityId, Guid recId, string tipMsg, DbTransaction tran = null)
        {
            var result = 0;
            var selectSql = "select entitytable from crm_sys_entity where entityid = @entityId limit 1;";

            var selectParam = new DynamicParameters();
            selectParam.Add("entityId", entityId);
            var tableName = DataBaseHelper.ExecuteScalar<string>(selectSql, selectParam);
            if (!string.IsNullOrEmpty(tableName))
            {
                var updateSql = string.Format("update {0} set erpresult = @tipMsg, recupdated = now(),issynchrosap=1  where recid = @recId;", tableName); 
				var param = new DbParameter[]
				{
					new NpgsqlParameter("recId",recId),
					new NpgsqlParameter("tipMsg", tipMsg),
				};

				if (tran == null)
					return DBHelper.ExecuteNonQuery("", updateSql, param);

				result = DBHelper.ExecuteNonQuery(tran, updateSql, param);
			}

            return result;
        }

		public int UpdateSynTipMsg2(Guid entityId, Guid recId, string tipMsg, DbTransaction tran = null)
		{
			var result = 0;
			var selectSql = "select entitytable from crm_sys_entity where entityid = @entityId limit 1;";

			var selectParam = new DynamicParameters();
			selectParam.Add("entityId", entityId);
			var tableName = DataBaseHelper.ExecuteScalar<string>(selectSql, selectParam);
			if (!string.IsNullOrEmpty(tableName))
			{
				var updateSql = string.Format("update {0} set erpresult = @tipMsg, recupdated = now()  where recid = @recId;", tableName);
				var param = new DbParameter[]
				{
					new NpgsqlParameter("recId",recId),
					new NpgsqlParameter("tipMsg", tipMsg),
				};

				if (tran == null)
					return DBHelper.ExecuteNonQuery("", updateSql, param);

				result = DBHelper.ExecuteNonQuery(tran, updateSql, param);
			}

			return result;
		}

		public string GetRegionFullNameById(string regionId)
        {
            string sql = @"select replace(fullname, '.', '') as fullname from crm_sys_region where regionid::text = @regionId and recstatus = 1;";

            var param = new DynamicParameters();
            param.Add("regionId", regionId);

            var result = DataBaseHelper.ExecuteScalar<string>(sql, param, CommandType.Text);
            if (string.IsNullOrEmpty(result))
                result = string.Empty;
            return result;
        }

        public Int32 GetRegionIdByName(string regionName)
        {
            string sql = @"select regionid from crm_sys_region where (regionname = @regionName or replace(fullname,'.','') = @regionname) and recstatus = 1 and regiontype = 2;";

            var param = new DynamicParameters();
            param.Add("regionName", regionName);

            var result = DataBaseHelper.ExecuteScalar<Int32>(sql, param, CommandType.Text); 
            return result;
        }

        public DateTime GetLastDateTime(Guid entityId)
        {
            string sql = @"select lastdatetime from crm_fhsj_fetch_record where recstatus =1 and entityid = @entityId;";

            var param = new DynamicParameters();
            param.Add("entityId", entityId);

            var result = DataBaseHelper.ExecuteScalar<DateTime>(sql, param, CommandType.Text);
            return result;
        }

        public int UpdateLastDateTime(Guid entityId, DateTime dt)
        { 
            var updateSql = string.Format("update crm_fhsj_fetch_record set lastdatetime = @dt where entityid = @entityId;");

            var updateParam = new DynamicParameters();
            updateParam.Add("entityId", entityId);
            updateParam.Add("dt", dt);

            var result = DataBaseHelper.ExecuteNonQuery(updateSql, updateParam, CommandType.Text); 

            return result;
        }

        public string GetProductCodeById(string recId)
        {
            if (string.IsNullOrEmpty(recId)) return string.Empty;

            var recIdGuid = new Guid(recId);
            var sql = string.Format(@"
                select e.productcode from crm_sys_product e
where e.recid = @recId and e.recstatus = 1 limit 1;");

            var param = new
            {
                recId = recIdGuid
            };
            var result = DataBaseHelper.ExecuteScalar<string>(sql, param, CommandType.Text);
            return result;
        }

        public string getUserCodeById(string userId)
        {
            var sql = string.Format(@"
                select workcode from crm_sys_userinfo where 1 = 1 and userid::text = @userId limit 1;");

            var updateParam = new DynamicParameters();
            updateParam.Add("userId", userId);

            var result = DataBaseHelper.QuerySingle<string>(sql, updateParam, CommandType.Text);
            return result;
        }

        public string getUserIdByCode(string workcode)
        {
            var sql = string.Format(@"
                select userid from crm_sys_userinfo where 1= 1 and workcode = @workcode limit 1;");

            var updateParam = new DynamicParameters();
            updateParam.Add("workcode", workcode);

            var result = DataBaseHelper.QuerySingle<string>(sql, updateParam, CommandType.Text);
            return result;
        }

        public List<DataSourceInfo> GetCustomerData()
        {
            string sql = @"select recid as id, companyone as code, recname as name from crm_sys_customer where recstatus = 1;";

            var param = new DynamicParameters();  
            return DataBaseHelper.Query<DataSourceInfo>(sql, param, CommandType.Text);
        }

        public List<DataSourceInfo> GetOpporData()
        {
            string sql = @"select recid as id, recname as name, projectcode as code from crm_sys_opportunity where recstatus = 1;";

            var param = new DynamicParameters();
            return DataBaseHelper.Query<DataSourceInfo>(sql, param, CommandType.Text);
        }

        public List<DataSourceInfo> GetContractData()
        {
            string sql = @"select recid as id, recname as name,contractid as code from crm_sys_contract where recstatus = 1;";

            var param = new DynamicParameters();
            return DataBaseHelper.Query<DataSourceInfo>(sql, param, CommandType.Text);
        }

        public List<DataSourceInfo> GetOrderData()
        {
            string sql = @"select recid as id, orderid as name,orderid as code,datasources  from crm_sys_order where recstatus = 1;";

            var param = new DynamicParameters();
            return DataBaseHelper.Query<DataSourceInfo>(sql, param, CommandType.Text);
        }

        public List<DataSourceInfo> GetDeliData()
        {
            string sql = @"select recid as id, shipmentordercode as name,shipmentordercode as code  from crm_fhsj_shipment_order where recstatus = 1;";

            var param = new DynamicParameters();
            return DataBaseHelper.Query<DataSourceInfo>(sql, param, CommandType.Text);
        }

        public List<SimpleUserInfo> GetUserData()
        {
            //禁用也取
            string sql = @"select userid, username, workcode from crm_sys_userinfo where 1 = 1;";

            var param = new DynamicParameters();
            return DataBaseHelper.Query<SimpleUserInfo>(sql, param, CommandType.Text);
        }

        public List<SimpleProductnfo> GetProductData()
        {
            //禁用也取回来
            string sql = @"select recid as productid, productcode, productname productmodel
                            from crm_sys_product where 1 = 1;";

            var param = new DynamicParameters();
            return DataBaseHelper.Query<SimpleProductnfo>(sql, param, CommandType.Text);
        }

        public List<DataSourceInfo> GetCategoryDataByEntityId(Guid entityId)
        {
            string sql = @"select categoryid as id, categoryname as code from crm_sys_entity_category
                            where recstatus = 1 and entityid = @entityId;";

            var param = new DynamicParameters();
            param.Add("entityId", entityId);
            return DataBaseHelper.Query<DataSourceInfo>(sql, param, CommandType.Text);
        }

        public List<DataSourceInfo> GetBankData()
        {
            string sql = @"select recid as id, bankname as name, recname as code from crm_fhsj_bank_data where recstatus = 1;";

            var param = new DynamicParameters();
            return DataBaseHelper.Query<DataSourceInfo>(sql, param, CommandType.Text);
        }

        public List<DataSourceInfo> GetSapPayment()
        {
            string sql = @"select recid as id, sappaycode as name, sappaycode as code from crm_fhsj_sappayment where recstatus = 1;";

            var param = new DynamicParameters();
            return DataBaseHelper.Query<DataSourceInfo>(sql, param, CommandType.Text);
        }

        public string GetSapPaymentById(string recId)
        {
            if (string.IsNullOrEmpty(recId)) return string.Empty;

            var recIdGuid = new Guid(recId);
            var sql = string.Format(@"
                select e.sappaycode from crm_fhsj_sappayment e  where e.recid = @recId;");

            var param = new
            {
                recId = recIdGuid
            };
            var result = DataBaseHelper.ExecuteScalar<string>(sql, param, CommandType.Text);
            return result;
        }

        public Int32 GetProductLine(string productcode)
        {

            string sql = @"select COALESCE(d.dataid,0)::int as plineid  from crm_sys_product p inner join crm_sys_products_series s on p.productsetid=s.productsetid
                inner join crm_sys_dictionary d on d.extfield1=s.productsetcode and dictypeid=76  where s.recstatus=1 and d.recstatus=1 and p.productcode=@productcode limit 1";

            var updateParam = new DynamicParameters();
            updateParam.Add("productcode", productcode);

            var result = DataBaseHelper.QuerySingle<int>(sql, updateParam, CommandType.Text);
            return result;
        }
        public AutoSynSapModel GetEntityIdAndRecIdByCaseId(DbTransaction trans, Guid caseId,int userId)
        {
            string sql = @"select c.recid, e.entityid
                            from crm_sys_workflow_case c
                            left join crm_sys_workflow w on c.flowid = w.flowid
                            left join crm_sys_entity e on w.entityid = e.entityid
                            where c.caseid = @caseid;";

            //var param = new DynamicParameters();
            //param.Add("caseId", caseId);
            //return DataBaseHelper.QuerySingle<AutoSynSapModel>(sql, param, CommandType.Text);
            DbParameter[] p = new DbParameter[] {
                new NpgsqlParameter("@caseid",caseId)
            };
            return ExecuteQuery<AutoSynSapModel>(sql, p, trans).FirstOrDefault();
        }
        
        public dynamic ExcuteActionExt(DbTransaction transaction, string funcname, object basicParamData, object preActionResult, object actionResult, int usernumber)
        {
            var sql = string.Format(@"SELECT * from {0}(@paramjson,@preresultjson,@actionresultjson,@userno);", funcname);

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("paramjson", JsonHelper.ToJson(basicParamData)),
                        new NpgsqlParameter("preresultjson",JsonHelper.ToJson(preActionResult)),
                        new NpgsqlParameter("actionresultjson",JsonHelper.ToJson(actionResult)),
                        new NpgsqlParameter("userno", usernumber),

                    };
            return DBHelper.ExecuteQuery(transaction, sql, param);
        }
        public dynamic DoCloseCRMOrderRow(DbTransaction transaction, string _entityid, string _recids,string _inputstatus, int usernumber)
        {
            var sql = @"SELECT * from crm_fhsj_cancel_currencyfunc(@entityid,@recids,@inputstatus,@userno);";

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("entityid", _entityid),
                        new NpgsqlParameter("recids",_recids),
                        new NpgsqlParameter("inputstatus",_inputstatus),
                        new NpgsqlParameter("userno", usernumber+""),

                    };
            return DBHelper.ExecuteQuery(transaction, sql, param);
        }

        public List<RegionCityClass> GetRegionCityData()
        {
            string sql = @"select regionid,regionname,replace(fullname,'.','') as fullname from crm_sys_region where recstatus = 1 and regiontype = 2;";

            var param = new DynamicParameters();

            var result = DataBaseHelper.Query<RegionCityClass>(sql, param, CommandType.Text);
            return result;
        }

        public List<DataSourceInfo> GetCustData()
        {
            string sql = @"select recid as id, recname as name,erpcode as code  from crm_sys_customer where recstatus = 1;";

            var param = new DynamicParameters();
            return DataBaseHelper.Query<DataSourceInfo>(sql, param, CommandType.Text);
        }

    }
}
