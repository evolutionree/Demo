using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo;
using UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Repository.Repository.DbManage
{
    public class DbWorkFlowRepository : RepositoryBase, IDbWorkFlowRepository
    {

        public List<DbWorkFlowInfo> GetWorkFlowInfoList(List<Guid> flowids, DbTransaction trans = null)
        {

            var flowsSql = string.Empty;
            var sqlParameters = new List<DbParameter>();

            if (flowids != null && flowids.Count > 0)
            {
                flowsSql = " AND w.flowid=ANY(@flowids)";
                sqlParameters.Add(new NpgsqlParameter("flowids", flowids));
            }

            var executeSql = string.Format(@"
                                    (SELECT (row_to_json(w,true)) AS WorkFlow,
                                    NULL AS Nodes,NULL AS NodeLines,
                                    (SELECT array_to_json(array_agg(row_to_json(we,true))) FROM crm_sys_workflow_func_event we
                                    INNER JOIN crm_sys_workflow_node n ON n.flowid=we.flowid AND n.vernum=(
                                    SELECT MAX(vernum) FROM crm_sys_workflow_node WHERE flowid=n.flowid) WHERE we.flowid=w.flowid AND we.nodeid=n.nodeid) AS FuncEvents,
                                    (SELECT array_to_json(array_agg(row_to_json(r,true))) FROM crm_sys_workflow_rule_relation AS r WHERE r.flowid=w.flowid ) AS WorkFlowRuleRelations
                                    FROM crm_sys_workflow w
                                    WHERE w.recstatus=1 AND w.flowtype=0   {0})
                                    UNION ALL
                                    (SELECT (row_to_json(w,true)) AS WorkFlow,
                                    (SELECT array_to_json(array_agg(row_to_json(n,true))) FROM crm_sys_workflow_node AS n WHERE n.flowid=w.flowid AND n.vernum=(
                                    SELECT MAX(vernum) FROM crm_sys_workflow_node WHERE flowid=n.flowid)) AS Nodes,
                                    (SELECT array_to_json(array_agg(row_to_json(nl,true)))  FROM crm_sys_workflow_node_line AS nl WHERE nl.flowid=w.flowid AND nl.vernum=(
                                    SELECT MAX(vernum) FROM crm_sys_workflow_node WHERE flowid=nl.flowid)) AS NodeLines,
                                    (SELECT array_to_json(array_agg(row_to_json(we,true))) FROM crm_sys_workflow_func_event we
                                    INNER JOIN crm_sys_workflow_node n ON n.flowid=we.flowid AND n.vernum=(
                                    SELECT MAX(vernum) FROM crm_sys_workflow_node WHERE flowid=n.flowid) WHERE we.flowid=w.flowid AND we.nodeid=n.nodeid) AS FuncEvents,
                                    (SELECT array_to_json(array_agg(row_to_json(r,true))) FROM crm_sys_workflow_rule_relation AS r WHERE r.flowid=w.flowid ) AS WorkFlowRuleRelations
                                    FROM crm_sys_workflow w
                                    WHERE w.recstatus=1 AND w.flowtype=1   {0})", flowsSql);
            var workflows = ExecuteQuery<DbWorkFlowInfo>(executeSql, sqlParameters.ToArray(), trans);

            Dictionary<Guid, List<Guid>> flowRuleids = new Dictionary<Guid, List<Guid>>();
            var ruleids = new List<Guid>();
            foreach (var flow in workflows)
            {
                
                if (flow.NodeLines != null)
                {
                    ruleids.AddRange(flow.NodeLines.Select(m => m.RuleId));
                }
                if (flow.WorkFlowRuleRelations != null)
                {
                    ruleids.AddRange(flow.WorkFlowRuleRelations.Select(m => m.RuleId));
                }
                if (ruleids.Count > 0)
                    flowRuleids.Add(flow.WorkFlow.FlowId, ruleids);
            }
            ruleids = ruleids.Distinct().ToList();
            var rules =new DbRuleRepository().GetRuleInfoList(ruleids, trans);
            
            foreach(var item in flowRuleids)
            {
                var flow = workflows.Find(m => m.WorkFlow.FlowId == item.Key);
                flow.RuleInfos = rules.Where(m => item.Value.Contains(m.RuleInfo.RuleId)).ToList();
            }


            return workflows;
        }

       

        public void SaveWorkFlowInfoList(List<DbWorkFlowInfo> flowInfos, int userNum, DbTransaction trans = null)
        {
            var workFlows = new List<CrmSysWorkflow>();
            var nodes = new List<CrmSysWorkflowNode>();
            var nodeLines = new List<CrmSysWorkflowNodeLine>();
            var funcEvents = new List<CrmSysWorkflowFuncEvent>();
            var workFlowRuleRelations = new List<CrmSysWorkflowRuleRelation>();
            List<DbParameter[]> workflowParams = new List<DbParameter[]>();
            List<DbParameter[]> nodesParams = new List<DbParameter[]>();
            List<DbParameter[]> nodeLinesParams = new List<DbParameter[]>();
            List<DbParameter[]> funcEventsParams = new List<DbParameter[]>();
            List<DbParameter[]> workFlowRuleRelationsParams = new List<DbParameter[]>();

            List<DbRuleInfo> ruleInfos = new List<DbRuleInfo>();
            if (flowInfos == null || flowInfos.Count == 0)
            {
                throw new Exception("流程数据不可为空");
            }
            
            foreach (var flow in flowInfos)
            {
                if (flow.WorkFlow != null)
                    workflowParams.Add(GetDbParameters(flow.WorkFlow));
                if (flow.Nodes != null)
                    nodesParams.AddRange(GetDbParameters(flow.Nodes));
                if (flow.NodeLines != null)
                    nodeLinesParams.AddRange(GetDbParameters(flow.NodeLines));
                if (flow.FuncEvents != null)
                    funcEventsParams.AddRange(GetDbParameters(flow.FuncEvents));
                if (flow.WorkFlowRuleRelations != null)
                    workFlowRuleRelationsParams.AddRange(GetDbParameters(flow.WorkFlowRuleRelations));
                if (flow.RuleInfos != null)
                    ruleInfos.AddRange(flow.RuleInfos);
            }
            DbConnection conn = null;
            if (trans == null)
            {
                conn = DBHelper.GetDbConnect();
                conn.Open();
                trans = conn.BeginTransaction();
            }

            try
            {
                var workflowExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow(flowid,flowname,flowtype,backflag,resetflag,expireday,remark,entityid,vernum,skipflag,reccreator,recupdator)
                                   VALUES(@flowid,@flowname,@flowtype,@backflag,@resetflag,@expireday,@remark,@entityid,@vernum,@skipflag,{0},{0})", userNum);
                ExecuteNonQueryMultiple(workflowExecuteSql, workflowParams, trans);
                var nodesExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow_node(nodeid,nodename,flowid,auditnum,nodetype,steptypeid,ruleconfig,columnconfig,vernum,auditsucc,nodeconfig)
                                   VALUES(@nodeid,@nodename,@flowid,@auditnum,@nodetype,@steptypeid,@ruleconfig,@columnconfig,@vernum,@auditsucc,@nodeconfig)", userNum);
                ExecuteNonQueryMultiple(nodesExecuteSql, nodesParams, trans);
                var nodeLinesExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow_node_line(lineid,flowid,fromnodeid,tonodeid,ruleid,vernum,lineconfig)
                                   VALUES(@lineid,@flowid,@fromnodeid,@tonodeid,@ruleid,@vernum,@lineconfig)");
                ExecuteNonQueryMultiple(nodeLinesExecuteSql, nodeLinesParams, trans);
                var funcEventExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow_func_event(funceventid,flowid,nodeid,funcname,steptype)
                                   VALUES(@funceventid,@flowid,@nodeid,@funcname,@steptype)");
                ExecuteNonQueryMultiple(funcEventExecuteSql, funcEventsParams, trans);
                var workFlowRuleRelationsExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow_rule_relation(flowid,ruleid)
                                   VALUES(@flowid,@ruleid)");
                ExecuteNonQueryMultiple(workFlowRuleRelationsExecuteSql, workFlowRuleRelationsParams, trans);
                //保存rule数据
                new DbRuleRepository().SaveRuleInfoList(ruleInfos, userNum, trans);

                
                if (conn != null)
                    trans.Commit();
            }
            catch (Exception ex)
            {
                
                trans.Rollback();
                throw ex;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                    conn.Dispose();
                }

            }


        }

       
    }
}
