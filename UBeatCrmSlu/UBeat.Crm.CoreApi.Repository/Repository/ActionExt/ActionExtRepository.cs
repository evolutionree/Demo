using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.ActionExt;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.ActionExt
{
    public class ActionExtRepository : RepositoryBase, IActionExtRepository
    {


        public List<ActionExtModel> GetActionExtData()
        {
            var sql = @"SELECT DISTINCT ON(routepath,implementtype) recid,routepath,implementtype,assemblyname,classtypename,funcname,operatetype,resulttype FROM crm_sys_actionext_config WHERE recstatus=1;
                       ";
            return DBHelper.ExecuteQuery<ActionExtModel>("", sql, null);
        }

        public dynamic ExcuteActionExt(DbTransaction transaction, string funcname, object basicParamData, int usernumber)
        {
            var sql = string.Format(@"SELECT {0}(@paramjson,@userno);", funcname);

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("paramjson", JsonHelper.ToJson(basicParamData)),
                        new NpgsqlParameter("userno", usernumber),

                    };
            return DBHelper.ExecuteQuery(transaction, sql, param);
        }


    }
}
