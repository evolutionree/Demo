using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Repository.Repository.DbManage
{
    public class DbWorkFlowRepository : RepositoryBase, IDbWorkFlowRepository
    {

        public List<DbWorkFlowInfo> GetWorkFlowInfoList(DbTransaction trans)
        {
            var infolist = new List<DbWorkFlowInfo>();

            var workflow_Sql = @" SELECT * FROM crm_sys_workflow WHERE recstatus=1 AND flowtype=0;
                                  SELECT * FROM crm_sys_workflow WHERE recstatus=1 AND flowtype=1;";
            var workflows = ExecuteQueryMultiple<CrmSysWorkflow>(workflow_Sql, null, trans);
            var fixWorkflows = workflows[1];
            if (fixWorkflows.Count > 0)
            {
                var executeSql = @"SELECT w.*,e.relentityid,u.username AS RecCreator_name FROM crm_sys_workflow  AS w
                               LEFT JOIN crm_sys_entity AS e ON e.entityid = w.entityid 
                               LEFT JOIN crm_sys_userinfo AS u ON u.userid = w.reccreator
                               WHERE flowid=@flowid";
                foreach (var wf in fixWorkflows)
                {

                }
            }

            var param = new DbParameter[]
            {
                //new NpgsqlParameter("flowid", flowid),
            };
            return ExecuteQuery<DbWorkFlowInfo>(workflow_Sql, param, trans);

        }
        public CrmSysWorkflow GetCrmSysWorkflow(DbTransaction trans)
        {
            var executeSql = @"SELECT w.*,e.relentityid,u.username AS RecCreator_name FROM crm_sys_workflow  AS w
                               LEFT JOIN crm_sys_entity AS e ON e.entityid = w.entityid 
                               LEFT JOIN crm_sys_userinfo AS u ON u.userid = w.reccreator
                               WHERE flowid=@flowid";

            var param = new DbParameter[]
            {
                //new NpgsqlParameter("flowid", flowid),
            };
            return ExecuteQuery<CrmSysWorkflow>(executeSql, param, trans).FirstOrDefault();
        }
        //public List<CrmSysWorkflowNode> GetNodes(DbTransaction trans)
        //{
        //    var executeSql = @"SELECT w.*,e.relentityid,u.username AS RecCreator_name FROM crm_sys_workflow  AS w
        //                       LEFT JOIN crm_sys_entity AS e ON e.entityid = w.entityid 
        //                       LEFT JOIN crm_sys_userinfo AS u ON u.userid = w.reccreator
        //                       WHERE flowid=@flowid";

        //    var param = new DbParameter[]
        //    {
        //        //new NpgsqlParameter("flowid", flowid),
        //    };
        //    return ExecuteQuery<CrmSysWorkflow>(executeSql, param, trans).FirstOrDefault();
        //}


    }
}
