using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Repository.Utility;
using UBeat.Crm.CoreApi.ZGQY.Model;

namespace UBeat.Crm.CoreApi.ZGQY.Repository
{
	public class DataPlatformRepository: MiddleDbBaseRepository, IDataPlatformRepository
	{

		public List<SaveProjectInfo> GetProejctData(DateTime date)
		{
			string sql = @"select * from cdmop_org_prj_project_contract_view_dd where CREATION_DATE::date >= @date";

			var param = new DynamicParameters();
			param.Add("date", date);

			return DataBaseHelper.Query<SaveProjectInfo>(sql, param, CommandType.Text, ConnectionString);
		}

		public QueryModelInfo GetContractByCode(string code)
		{
			string strSql = @" select recid, recname, recmanager from crm_sys_contract where recstatus = 1 and contractid = @code limit 1";
			var param = new DynamicParameters();
			param.Add("code", code);
			var result = DataBaseHelper.QuerySingle<QueryModelInfo>(strSql, param);

			return result;
		}

		public Guid IsExitProject(String code)
		{
			var sql = @"select recid from crm_sys_project where projectcode = @code and recstatus=1 limit 1;";
			var param = new DynamicParameters();
			param.Add("code", code);
			return DataBaseHelper.ExecuteScalar<Guid>(sql, param);
		}

		public List<SaveBillInfo> GetBillData(DateTime date)
		{
			string sql = @"select * from cdmop_prj_billing_information_dd where CREATION_DATE::date >= @date";

			var param = new DynamicParameters();
			param.Add("date", date);

			return DataBaseHelper.Query<SaveBillInfo>(sql, param, CommandType.Text, ConnectionString);
		}

		public Guid IsExitBill(String code)
		{
			var sql = @"select recid from crm_sys_invoicing where invoicingcode = @code and recstatus=1 limit 1;";
			var param = new DynamicParameters();
			param.Add("code", code);
			return DataBaseHelper.ExecuteScalar<Guid>(sql, param);
		}
		public QueryUserInfo GetUserIdByWorkcode(string code)
		{
			string strSql = @" select u.userid, ur.deptid from crm_sys_userinfo u
								left join crm_sys_account_userinfo_relate ur on u.userid = ur.userid
								where u.recstatus = 1 and ur.recstatus = 1 and u.workcode = @code limit 1";
			var param = new DynamicParameters();
			param.Add("code", code);
			var result = DataBaseHelper.QuerySingle<QueryUserInfo>(strSql, param);

			return result;
		}
		public List<SavePayInfo> GetPayData(DateTime date)
		{
			string sql = @"select * from cdmop_fee_contract_receive_register_dd where CREATION_DATE::date >= @date";

			var param = new DynamicParameters();
			param.Add("date", date);

			return DataBaseHelper.Query<SavePayInfo>(sql, param, CommandType.Text, ConnectionString);
		}

		public Guid IsExitPay(String code)
		{
			var sql = @"select recid from crm_sys_payments where recname = @code and recstatus=1 limit 1;";
			var param = new DynamicParameters();
			param.Add("code", code);
			return DataBaseHelper.ExecuteScalar<Guid>(sql, param);
		}

		public List<SaveZdInfo> GetZdData(DateTime date)
		{
			string sql = @"select * from ""ENTERTAIN_INFO"" where entertain_date::date >= @date";

			var param = new DynamicParameters();
			param.Add("date", date);

			return DataBaseHelper.Query<SaveZdInfo>(sql, param, CommandType.Text, ConnectionString);
		}

		public Guid IsExitZd(String code)
		{
			var sql = @"select recid from crm_sys_entertainment where encode = @code and recstatus=1 limit 1;";
			var param = new DynamicParameters();
			param.Add("code", code);
			return DataBaseHelper.ExecuteScalar<Guid>(sql, param);
		}
		public List<SaveClInfo> GetClData(DateTime date)
		{
			string sql = @"select * from ""TRAVEL_FEE_ACCOUNT_INFO"" where travel_date::date >= @date";

			var param = new DynamicParameters();
			param.Add("date", date);

			return DataBaseHelper.Query<SaveClInfo>(sql, param, CommandType.Text, ConnectionString);
		}

		public Guid IsExitCl(String code)
		{
			var sql = @"select recid from crm_sys_travel where travelcode = @code and recstatus=1 limit 1;";
			var param = new DynamicParameters();
			param.Add("code", code);
			return DataBaseHelper.ExecuteScalar<Guid>(sql, param);
		}
	}
}
