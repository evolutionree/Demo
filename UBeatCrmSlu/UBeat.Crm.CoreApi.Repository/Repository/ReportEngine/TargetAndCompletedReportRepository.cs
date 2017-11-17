using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;

using System.Data.Common;
namespace UBeat.Crm.CoreApi.Repository.Repository.ReportEngine
{
    public class TargetAndCompletedReportRepository : RepositoryBase, ITargetAndCompletedReportRepository
    {
        public Dictionary<string, List<Dictionary<string, object>>> DataList(string entityid, string searchsql, string orderby, int pageIndex, int pageSize, int userNumber)
        {
            try
            {
                string cmdText = string.Format("select crm_func_entity_protocol_data_list_forreport(@entityid,'{0}'::text,@sortedby1,@pageindex1,@pagesize2,@userno)", searchsql);
                var param = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("entityid",Guid.Parse(entityid)),
                    new Npgsql.NpgsqlParameter("sortedby1",orderby),
                    new Npgsql.NpgsqlParameter("pageindex1",pageIndex),
                    new Npgsql.NpgsqlParameter("pagesize2",pageSize),
                    new Npgsql.NpgsqlParameter("userno",userNumber)
                };
                Dictionary<string,List<Dictionary<string,object >>> retData = ExecuteQueryRefCursor(cmdText, param);
                return retData;
            }
            catch (Exception ex) {
            }
            return new Dictionary<string, List<Dictionary<string, object>>>();
        }
    }
}
