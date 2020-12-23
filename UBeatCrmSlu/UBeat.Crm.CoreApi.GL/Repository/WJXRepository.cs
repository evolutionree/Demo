using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.GL.IRepository;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;

namespace UBeat.Crm.CoreApi.GL.Repository
{
    public class WJXRepository : RepositoryBase, IWJXRepository
    {
        public void SaveWXJAnswer(WJXCallBack callBack, int userId, DbTransaction tran = null)
        {
            var sql = " insert into crm_sys_wjxanswer (activity,name,joinid,submittime,sign,answer,question,sojumpparm) values (@activity,@name,@joinid,@submittime,@sign,@answer::jsonb,@question::jsonb,@sojumpparm)";

            var param = new DbParameter[] {
                new NpgsqlParameter("activity",callBack.Activity),
                new NpgsqlParameter("name",callBack.Name),
                new NpgsqlParameter("joinid",callBack.JoinId),
                new NpgsqlParameter("submittime",callBack.SubmitTime),
                new NpgsqlParameter("sign",callBack.SubmitTime),
                 new NpgsqlParameter("answer",JsonConvert.SerializeObject(callBack.Answer)),
                new NpgsqlParameter("question",callBack.Question),
                new NpgsqlParameter("sojumpparm", callBack.Sojumpparm)
            };
            ExecuteNonQuery(sql, param, tran);

        }
        public List<WJXCallBack> GetWXJAnswerList(WJXCallBack callBack, int userId, DbTransaction tran = null)
        {
            var sql = " select * from  crm_sys_wjxanswer  where sojumpparm=@sojumpparm;";

            var param = new DbParameter[] {
                new NpgsqlParameter("sojumpparm",callBack.Sojumpparm)
            };
            return ExecuteQuery<WJXCallBack>(sql, param, tran);
        }
    }
}
