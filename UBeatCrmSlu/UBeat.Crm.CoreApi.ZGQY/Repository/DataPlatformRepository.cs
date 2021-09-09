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
	}
}
