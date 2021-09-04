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
	public class DataPlatformRepository: MiddleDbBaseRepository
	{
		public List<SaveProjectInfo> GetProejctData(DateTime date)
		{
			string sql = @"select * from cdmop_org_prj_project_contract_view_dd where CREATION_DATE::date >= @date";

			var param = new DynamicParameters();
			param.Add("date", date);

			return DataBaseHelper.Query<SaveProjectInfo>(sql, param, CommandType.Text, ConnectionString);
		}
	}
}
