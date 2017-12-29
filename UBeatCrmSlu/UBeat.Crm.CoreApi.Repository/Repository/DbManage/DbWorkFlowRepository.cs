using Newtonsoft.Json;
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
            var rules = new DbRuleRepository().GetRuleInfoList(ruleids, trans);

            foreach (var item in flowRuleids)
            {
                var flow = workflows.Find(m => m.WorkFlow.FlowId == item.Key);
                flow.RuleInfos = rules.Where(m => item.Value.Contains(m.RuleInfo.RuleId)).ToList();
            }


            return workflows;
        }
        #region --获取数据库所有关于流程配置的数据--

        public List<CrmSysWorkflow> GetAllCrmSysWorkflowList(DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();

            var executeSql = @"SELECT * FROM crm_sys_workflow ";
            var result = ExecuteQuery<CrmSysWorkflow>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }
        public List<CrmSysWorkflowNode> GetAllCrmSysWorkflowNodeList(DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();

            var executeSql = @"SELECT * FROM crm_sys_workflow_node ";
            var result = ExecuteQuery<CrmSysWorkflowNode>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }
        public List<CrmSysWorkflowNodeLine> GetAllCrmSysWorkflowNodeLineList(DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();

            var executeSql = @"SELECT * FROM crm_sys_workflow_node_line ";
            var result = ExecuteQuery<CrmSysWorkflowNodeLine>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }
        public List<CrmSysWorkflowFuncEvent> GetAllCrmSysWorkflowFuncEventList(DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();

            var executeSql = @"SELECT * FROM crm_sys_workflow_func_event ";
            var result = ExecuteQuery<CrmSysWorkflowFuncEvent>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }
        public List<CrmSysWorkflowRuleRelation> GetAllCrmSysWorkflowRuleRelationList(DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();

            var executeSql = @"SELECT * FROM crm_sys_workflow_rule_relation ";
            var result = ExecuteQuery<CrmSysWorkflowRuleRelation>(executeSql, sqlParameters.ToArray(), trans);

            return result;
        }


        #endregion
        public void SaveWorkFlowInfoList(List<DbWorkFlowInfo> flowInfos, int userNum, DbTransaction trans = null)
        {
            var workFlows = new List<CrmSysWorkflow>();
            var updateWorkFlows = new List<CrmSysWorkflow>();
            var nodes = new List<CrmSysWorkflowNode>();
            var nodeLines = new List<CrmSysWorkflowNodeLine>();
            var funcEvents = new List<CrmSysWorkflowFuncEvent>();
            //var workFlowRuleRelations = new List<CrmSysWorkflowRuleRelation>();
            List<DbRuleInfo> ruleInfos = new List<DbRuleInfo>();
            if (flowInfos == null || flowInfos.Count == 0)
            {
                throw new Exception("流程数据不可为空");
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
                var allworkflow = GetAllCrmSysWorkflowList(trans);
                var allnodes = GetAllCrmSysWorkflowNodeList(trans);
                var allnodelines = GetAllCrmSysWorkflowNodeLineList(trans);


                //判断源数据是否存在目标数据库中，若存在，则需要把源版本号重置为目标数据库流程版本号+1的值
                foreach (var flow in flowInfos)
                {
                    if (flow.WorkFlow == null)
                        continue;
                    var existFlow = allworkflow.Find(m => m.FlowId == flow.WorkFlow.FlowId);
                    if (existFlow == null)
                    {
                        workFlows.Add(flow.WorkFlow);

                    }
                    else
                    {
                        var newVerNum = existFlow.VerNum + 1;
                        flow.WorkFlow.VerNum = newVerNum;

                        if (existFlow.FlowType == 1)//固定流程
                        {
                            if (flow.Nodes == null || flow.Nodes.Count == 0)//如果流程节点没配置，则不同步该条流程配置数据
                                throw new Exception("不可导入未生成节点的固定流程");
                            else
                            {
                                var firstnode = flow.Nodes.FirstOrDefault();
                                if (allnodes.Exists(m => m.NodeId == firstnode.NodeId))//如果节点id已经存在，说明该条流程数据已经存在目标数据库了，则不导入
                                {
                                    continue;
                                }
                                foreach (var o in flow.Nodes)
                                {
                                    if (o != null)
                                        o.VerNum = newVerNum;
                                }

                                if (flow.NodeLines == null || flow.NodeLines.Count == 0)//如果流程节点没配置，则不同步该条流程配置数据
                                    throw new Exception("不可导入未生成节点连线的固定流程");
                                foreach (var o in flow.NodeLines)
                                {
                                    if (o != null)
                                        o.VerNum = newVerNum;
                                }
                            }
                        }
                        updateWorkFlows.Add(flow.WorkFlow);
                    }
                    if (flow.Nodes != null)
                        nodes.AddRange(flow.Nodes);
                    if (flow.NodeLines != null)
                        nodeLines.AddRange(flow.NodeLines);
                    if (flow.FuncEvents != null)
                        funcEvents.AddRange(flow.FuncEvents);
                    //if (flow.WorkFlowRuleRelations != null)
                    //    workFlowRuleRelations.AddRange(flow.WorkFlowRuleRelations);
                    if (flow.RuleInfos != null)
                        ruleInfos.AddRange(flow.RuleInfos);
                }
                workFlows = workFlows.Distinct(new CrmSysWorkflowComparer()).ToList();
                nodes = nodes.Distinct(new CrmSysWorkflowNodeComparer()).ToList();
                nodeLines = nodeLines.Distinct(new CrmSysWorkflowNodeLineComparer()).ToList();
                funcEvents = funcEvents.Distinct(new CrmSysWorkflowFuncEventComparer()).ToList();
                //workFlowRuleRelations = workFlowRuleRelations.Distinct(new CrmSysWorkflowRuleRelationComparer()).ToList();

                var allfuncEvents = GetAllCrmSysWorkflowFuncEventList(trans);
                funcEvents.RemoveAll(m => allfuncEvents.Exists(a => a.FuncEventId == m.FuncEventId || (a.FlowId == m.FlowId && a.NodeId == m.NodeId)));

                ruleInfos = ruleInfos.Distinct(new DbRuleInfoComparer()).ToList();

                var updateWorkflowExecuteSql = string.Format(@"WITH t1 as(
                                                        SELECT flowid,flowname,flowtype,backflag,resetflag,expireday,remark,entityid,vernum,skipflag,{0},{0} 
                                                        FROM jsonb_populate_recordset(null::crm_sys_workflow,@flows)
                                                        )
                                                        UPDATE crm_sys_workflow AS bt SET(flowid,flowname,flowtype,backflag,resetflag,expireday,remark,entityid,vernum,skipflag,reccreator,recupdator)=(
                                                        SELECT * FROM t1  WHERE flowid =bt.flowid) WHERE bt.flowid in (SELECT flowid FROM t1);", userNum);
                DbParameter[] updateWorkflowparams = new DbParameter[] { new NpgsqlParameter("flows", JsonConvert.SerializeObject(updateWorkFlows)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                ExecuteNonQuery(updateWorkflowExecuteSql, updateWorkflowparams, trans);

                var workflowExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow(flowid,flowname,flowtype,backflag,resetflag,expireday,remark,entityid,vernum,skipflag,reccreator,recupdator)
                                   SELECT flowid,flowname,flowtype,backflag,resetflag,expireday,remark,entityid,vernum,skipflag,{0},{0} 
                                   FROM jsonb_populate_recordset(null::crm_sys_workflow,@flows)", userNum);
                DbParameter[] workflowparams = new DbParameter[] { new NpgsqlParameter("flows", JsonConvert.SerializeObject(workFlows)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                ExecuteNonQuery(workflowExecuteSql, workflowparams, trans);

                var nodesExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow_node(nodeid,nodename,flowid,auditnum,nodetype,steptypeid,ruleconfig,columnconfig,vernum,auditsucc,nodeconfig)
                                   SELECT nodeid,nodename,flowid,auditnum,nodetype,steptypeid,ruleconfig,columnconfig,vernum,auditsucc,nodeconfig 
                                   FROM jsonb_populate_recordset(null::crm_sys_workflow_node,@nodes)", userNum);
                DbParameter[] nodesparams = new DbParameter[] { new NpgsqlParameter("nodes", JsonConvert.SerializeObject(nodes)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                ExecuteNonQuery(nodesExecuteSql, nodesparams, trans);

                var nodeLinesExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow_node_line(lineid,flowid,fromnodeid,tonodeid,ruleid,vernum,lineconfig)
                                   SELECT lineid,flowid,fromnodeid,tonodeid,ruleid,vernum,lineconfig
                                   FROM jsonb_populate_recordset(null::crm_sys_workflow_node_line,@nodeLines)", userNum);
                DbParameter[] nodeLinesparams = new DbParameter[] { new NpgsqlParameter("nodeLines", JsonConvert.SerializeObject(nodeLines)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                ExecuteNonQuery(nodeLinesExecuteSql, nodeLinesparams, trans);

                var funcEventExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow_func_event(funceventid,flowid,nodeid,funcname,steptype)
                                   SELECT funceventid,flowid,nodeid,funcname,steptype
                                   FROM jsonb_populate_recordset(null::crm_sys_workflow_func_event,@funcEvent)", userNum);
                DbParameter[] funcEventparams = new DbParameter[] { new NpgsqlParameter("funcEvent", JsonConvert.SerializeObject(funcEvents)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                ExecuteNonQuery(funcEventExecuteSql, funcEventparams, trans);

                //暂时不处理crm_sys_workflow_rule_relation表，由其他使用该表数据的模块同步
                //var allworkFlowRuleRelations = GetAllCrmSysWorkflowRuleRelationList(trans);
                // workFlowRuleRelations.RemoveAll(m => allworkFlowRuleRelations.Exists(a => a.FlowId == m.FlowId && a.RuleId == m.RuleId));
                //var workFlowRuleRelationsExecuteSql = string.Format(@"INSERT INTO crm_sys_workflow_rule_relation(flowid,ruleid)
                //                   SELECT flowid,ruleid
                //                   FROM jsonb_populate_recordset(null::crm_sys_workflow_rule_relation,@workFlowRuleRelations)", userNum);
                //DbParameter[] workFlowRuleRelationsparams = new DbParameter[] { new NpgsqlParameter("workFlowRuleRelations", JsonConvert.SerializeObject(workFlowRuleRelations)) { NpgsqlDbType = NpgsqlDbType.Jsonb } };
                //ExecuteNonQuery(workFlowRuleRelationsExecuteSql, workFlowRuleRelationsparams, trans);

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
