using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.DJCloud;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Data.Common;
using Npgsql;
using System.Linq;

namespace UBeat.Crm.CoreApi.Repository.Repository.DJCloud
{
    public class DJCloudRepository : RepositoryBase,IDJCloudRepository
    {
        public OperateResult AddDJCloudCallLog(DJCloudCallMapper data)
        {
            var executeSql = "select * from crm_func_cloudcall_add(@callid, @sessionid, @caller, @called, @isseccess, @failmsg, @calltime)";
            var param = new DbParameter[]
         {
                        new NpgsqlParameter("callid", data.CallId??""),
                        new NpgsqlParameter("sessionid", data.SessionId??""),
                        new NpgsqlParameter("caller", data.Caller),
                        new NpgsqlParameter("called", data.Called),
                        new NpgsqlParameter("isseccess", data.IsSeccess),
                        new NpgsqlParameter("failmsg", data.FailMsg??""),
                        new NpgsqlParameter("calltime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
         };
            var tempResult = ExecuteQuery<OperateResult>(executeSql, param).FirstOrDefault();

            return tempResult;
        }

        public string getCurrentLoginMobileNO(int userNumber)
        {
            var sql = @"SELECT userphone FROM public.crm_sys_userinfo where userid = @userid limit 1;";
            var param = new DbParameter[]
             {
                        new NpgsqlParameter("userid", userNumber),
             };
            var result = ExecuteQuery(sql, param).FirstOrDefault();

            if (result == null)
            {
                return "";
            }
            var temp = string.Empty;
            if (result["userphone"] != null)
                temp = result["userphone"].ToString();
            return temp;
        }
    }
}
