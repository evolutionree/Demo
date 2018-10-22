using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.Repository.Repository;

namespace UBeat.Crm.CoreApi.DingTalk.Repository
{
    public class H3YunRepository : RepositoryBase, IH3YunRepository
    {
        public string GetH3Code(string workflowid)
        {
            try
            {
                string strSQL = "Select h3code from crm_sys_workflow_h3conf where flowid = @flowid";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("flowid",Guid.Parse(workflowid))
                };
                return (string)ExecuteScalar(strSQL, p);
            }
            catch (Exception ex) {

            }
            return null;
        }
    }
}
