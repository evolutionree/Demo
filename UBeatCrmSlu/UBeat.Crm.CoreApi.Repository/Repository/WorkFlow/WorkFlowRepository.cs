using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.ReportRelation;
using UBeat.Crm.CoreApi.DomainModel.Utility;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.WorkFlow
{
    public class WorkFlowRepository : RepositoryBase, IWorkFlowRepository
    {
        public OperateResult AddCase(DbTransaction tran, WorkFlowAddCaseMapper caseMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_workflow_case_add(@flowid, @entityid, @recid,@relEntityId,@relRecId,@caseData,@userno)
            ";
            var param = new DbParameter[]
            {
                    new NpgsqlParameter("flowid", caseMapper.FlowId),
                    new NpgsqlParameter("entityid", caseMapper.EntityId),
                    new NpgsqlParameter("recid", caseMapper.RecId),
                    new NpgsqlParameter("relEntityId", caseMapper.RelEntityId.HasValue ? caseMapper.RelEntityId.Value.ToString() : ""),
                    new NpgsqlParameter("relRecId", caseMapper.RelRecId.HasValue ? caseMapper.RelRecId.Value.ToString() : ""),
                    new NpgsqlParameter("caseData",JsonHelper.ToJson(caseMapper.CaseData)),
                    new NpgsqlParameter("userno", userNumber)
            };

            return ExecuteQuery<OperateResult>(sql, param, tran).FirstOrDefault();

        }

        public Guid AddWorkflowCase(DbTransaction tran, WorkFlowInfo workflowinfo, WorkFlowAddCaseMapper caseMapper, int userNumber)
        {
            Guid caseid = Guid.NewGuid();
            var executeSql = @"INSERT INTO crm_sys_workflow_case (caseid,flowid, recid, title,vernum, reccreator, recupdator,auditstatus) 
                               VALUES (@caseid,@flowid, @recid,@title, @flowvernum, @userno, @userno,3)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("caseid", caseid),
                new NpgsqlParameter("flowid", caseMapper.FlowId),
                new NpgsqlParameter("recid", caseMapper.RecId),
                new NpgsqlParameter("title", caseMapper.Title),
                new NpgsqlParameter("flowvernum", workflowinfo.VerNum),
                new NpgsqlParameter("userno", userNumber),
            };
            int rows = ExecuteNonQuery(executeSql, param, tran);

            if (rows > 0)
            {
                if (caseMapper.RelEntityId.HasValue && caseMapper.RelRecId.HasValue)
                {
                    var tempSql = @"INSERT INTO crm_sys_workflow_case_entity_relation(caseid,relentityid,relrecid)
                               VALUES (@caseid,@relentityid,@relrecid) ";
                    var tempParam = new DbParameter[]
                    {
                        new NpgsqlParameter("caseid", caseid),
                        new NpgsqlParameter("relentityid", caseMapper.RelEntityId.Value),
                        new NpgsqlParameter("relrecid", caseMapper.RelRecId.Value),

                    };
                    rows = ExecuteNonQuery(tempSql, tempParam, tran);
                }
                return caseid;
            }

            return Guid.Empty;

        }


        public Dictionary<string, List<IDictionary<string, object>>> NextNodeData(Guid caseId, int userNumber)
        {
            var procName =
                "SELECT crm_func_workflow_nextnode_data(@caseId,@userNo)";

            var dataNames = new List<string>();

            var param = new
            {
                CaseId = caseId,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult AddCaseItem(WorkFlowAddCaseItemMapper caseItemMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_workflow_caseitem_add(@caseId,@nodeNum,@handleUser,@caseData,@copyUser,@remark, @userno)
            ";

            var caseDataJson = JsonHelper.ToJson(caseItemMapper.CaseData);

            var param = new
            {
                CaseId = caseItemMapper.CaseId,
                NodeNum = caseItemMapper.NodeNum,
                HandleUser = caseItemMapper.HandleUser,
                CaseData = caseDataJson,
                CopyUser = caseItemMapper.CopyUser,
                Remark = caseItemMapper.Remark,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult AuditCaseItem(WorkFlowAuditCaseItemMapper caseItemMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_workflow_caseitem_audit(@caseId,@nodeNum,@choiceStatus,@suggest,@caseData, @userno)
            ";

            var caseDataJson = JsonHelper.ToJson(caseItemMapper.CaseData);

            var param = new
            {
                CaseId = caseItemMapper.CaseId,
                NodeNum = caseItemMapper.NodeNum,
                ChoiceStatus = caseItemMapper.ChoiceStatus,
                Suggest = caseItemMapper.Suggest,
                CaseData = caseDataJson,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public List<Dictionary<string, object>> CaseItemList(Guid caseId, int userNumber)
        {
            //var procName =
            //  "SELECT crm_func_workflow_caseitem_list(@caseId,@userno)";

            //var param = new
            //{
            //    CaseId = caseId,
            //    UserNo = userNumber
            //};

            //var result = DataBaseHelper.QueryStoredProcCursor(procName, param, CommandType.Text);
            //return result;

            var executeSql = @" SELECT i.nodenum,i.stepnum::TEXT AS itemcode, 
                                (CASE WHEN  i.nodeid = '00000000-0000-0000-0000-000000000000' THEN '发起审批'
	                                  WHEN  i.nodeid = '00000000-0000-0000-0000-000000000001' THEN '结束审批'
	                                  WHEN  i.nodeid = '00000000-0000-0000-0000-000000000002' THEN '自选审批'
                                      ELSE n.nodename END) nodename,
                                crm_func_entity_protocol_format_workflow_casestatus(i.casestatus,i.choicestatus) AS casestatus,
                                i.suggest,i.handleuser,i.recupdated,u.username,u.usericon,n.nodetype,n.auditnum,n.auditsucc,
                                (CASE WHEN n.nodetype = 1 THEN format('当前步骤为会审,需要%s人同意才能通过',n.auditsucc) ELSE NULL END) AS tipsmsg
			                        FROM crm_sys_workflow_case_item AS i
			                        LEFT JOIN crm_sys_workflow_case AS c ON i.caseid = c.caseid
			                        LEFT JOIN crm_sys_workflow_node AS n ON i.nodeid = n.nodeid 
			                        LEFT JOIN crm_sys_userinfo AS u ON u.userid = i.handleuser 
			                        WHERE i.caseid = @caseid
			                        ORDER BY i.stepnum ASC";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("caseid", caseId),

            };

            return ExecuteQuery(executeSql, param);

        }

        public Dictionary<string, List<IDictionary<string, object>>> NodeLineInfo(Guid flowId, int userNumber)
        {
            var procName =
            "SELECT crm_func_workflow_data_node_line_detail(@flowId,@userNo)";

            var dataNames = new List<string>();

            var param = new
            {
                FlowId = flowId,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public Dictionary<string, List<Dictionary<string, object>>> GetNodeLinesInfo(Guid flowId, int userNumber, DbTransaction trans = null)
        {
            var vernumSql = @"SELECT vernum FROM crm_sys_workflow WHERE flowid = @flowid LIMIT 1";
            var vernumSqlParameters = new List<DbParameter>();
            vernumSqlParameters.Add(new NpgsqlParameter("flowid", flowId));
            var vernumResult = ExecuteScalar(vernumSql, vernumSqlParameters.ToArray(), trans);
            int vernum = 0;
            if (vernumResult != null)
                int.TryParse(vernumResult.ToString(), out vernum);


            var executeSql = @" SELECT n.nodeid,n.nodename,n.auditnum,n.nodetype,n.steptypeid,n.stepcptypeid,n.ruleconfig,n.columnconfig,n.auditsucc,n.nodeconfig ,e.funcname,n.notfound
                                FROM crm_sys_workflow_node AS n
                                LEFT JOIN crm_sys_workflow_func_event AS e ON e.flowid=n.flowid AND e.nodeid=n.nodeid
                                WHERE n.flowid = @flowid AND n.vernum = @vernum ;
                                SELECT lineid,fromnodeid,tonodeid,ruleid,lineconfig from crm_sys_workflow_node_line WHERE flowid = @flowid AND vernum = @vernum;
                                SELECT w.entityid,
                                    (select entityname from crm_sys_entity e where e.entityid = w.entityid limit 1) as entityname,
                                    (select relentityid from crm_sys_entity e where e.entityid = w.entityid limit 1) as relentityid, 
                                    (select entityname from crm_sys_entity where entityid in(select relentityid from crm_sys_entity e where e.entityid = w.entityid limit 1) limit 1) as relentityname,
                                flowid,flowname,remark,vernum,flowname_lang FROM crm_sys_workflow w WHERE flowid = @flowid LIMIT 1;";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("flowid", flowId),
                new NpgsqlParameter("vernum", vernum),
            };
            var tabels = ExecuteQueryMultiple(executeSql, param, trans);
            var result = new Dictionary<string, List<Dictionary<string, object>>>();
            result.Add("nodes", tabels[0]);
            result.Add("lines", tabels[1]);
            result.Add("flow", tabels[2]);
            return result;
        }


        public void SaveNodeLinesConfig(WorkFlowNodeLinesConfigMapper nodeLineConfig, int userNumber)
        {
            if (nodeLineConfig == null)
                throw new Exception("参数不可为null");
            if (nodeLineConfig.FlowId == Guid.Empty)
                throw new Exception("流程ID不能为空");
            if (nodeLineConfig.Nodes == null || nodeLineConfig.Nodes.Count == 0)
                throw new Exception("流程节点数据错误");
            if (nodeLineConfig.Lines == null || nodeLineConfig.Lines.Count == 0)
                throw new Exception("节点连线数据错误");

            using (DbConnection conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();

                try
                {
                    //生成新的版本号
                    var versionParam = new DbParameter[]
                    {
                        new NpgsqlParameter("flowid", nodeLineConfig.FlowId),
                    };
                    var versionObj = ExecuteScalar("SELECT crm_func_workflow_autoversion(@flowid)", versionParam, tran);
                    int versionValue = 0;
                    int.TryParse(versionObj.ToString(), out versionValue);

                    #region --插入node节点--
                    var workflow_node_sql = @"INSERT INTO crm_sys_workflow_node(nodeid,nodename,flowid,auditnum,nodetype,steptypeid,stepcptypeid,ruleconfig,columnconfig,vernum,auditsucc,nodeconfig,notfound)
                                              VALUES(@nodeid,@nodename,@flowid,@auditnum,@nodetype,@steptypeid,@stepcptypeid,@ruleconfig,@columnconfig,@vernum,@auditsucc,@nodeconfig,@notfound)";
                    List<DbParameter[]> workflow_node_params = new List<DbParameter[]>();
                    List<DbParameter[]> node_eve_params = new List<DbParameter[]>();
                    foreach (var node in nodeLineConfig.Nodes)
                    {
                        workflow_node_params.Add(new DbParameter[]
                        {
                            new NpgsqlParameter("nodeid", node.NodeId),
                            new NpgsqlParameter("nodename", node.NodeName),
                            new NpgsqlParameter("flowid", nodeLineConfig.FlowId),
                            new NpgsqlParameter("auditnum", node.AuditNum),
                            new NpgsqlParameter("nodetype",node.NodeType),
                            new NpgsqlParameter("steptypeid", node.StepTypeId),
                            new NpgsqlParameter("stepcptypeid", node.StepCPTypeId),
                            new NpgsqlParameter("ruleconfig", JsonConvert.SerializeObject(node.RuleConfig)){ NpgsqlDbType= NpgsqlTypes.NpgsqlDbType.Jsonb },
                            new NpgsqlParameter("columnconfig", JsonConvert.SerializeObject(node.ColumnConfig)){ NpgsqlDbType= NpgsqlTypes.NpgsqlDbType.Jsonb },
                            new NpgsqlParameter("vernum", versionValue),
                            new NpgsqlParameter("auditsucc", node.AuditSucc),
                            new NpgsqlParameter("notfound", node.NotFound),
                            new NpgsqlParameter("nodeconfig", JsonConvert.SerializeObject(node.NodeConfig)){ NpgsqlDbType= NpgsqlTypes.NpgsqlDbType.Jsonb },
                        });

                        if (!string.IsNullOrEmpty(node.NodeEvent))
                        {
                            node_eve_params.Add(new DbParameter[]
                            {
                            new NpgsqlParameter("nodeid", node.NodeId),
                            new NpgsqlParameter("flowid", nodeLineConfig.FlowId),
                            new NpgsqlParameter("funcname", node.NodeEvent),
                            new NpgsqlParameter("steptype", node.StepTypeId==0?0:1)
                            });
                        }
                    }
                    ExecuteNonQueryMultiple(workflow_node_sql, workflow_node_params, tran);
                    #endregion

                    #region --插入nodeevent--
                    if (node_eve_params.Count > 0)
                    {
                        var node_event_sql = @"INSERT INTO crm_sys_workflow_func_event(flowid,funcname,nodeid,steptype)
                                              VALUES(@flowid,@funcname,@nodeid,@steptype)";
                        ExecuteNonQueryMultiple(node_event_sql, node_eve_params, tran);
                    }
                    #endregion

                    #region --插入nodeline--
                    //crm_sys_workflow_node_line
                    var node_line_sql = @"INSERT INTO crm_sys_workflow_node_line(flowid,fromnodeid,tonodeid,ruleid,vernum,lineconfig)
                                          VALUES(@flowid,@fromnodeid,@tonodeid,@ruleid,@vernum,@lineconfig)";
                    List<DbParameter[]> node_line_params = new List<DbParameter[]>();
                    foreach (var line in nodeLineConfig.Lines)
                    {
                        node_line_params.Add(new DbParameter[]
                        {
                            new NpgsqlParameter("flowid", nodeLineConfig.FlowId),
                            new NpgsqlParameter("fromnodeid", line.FromNodeId),
                            new NpgsqlParameter("tonodeid",line.ToNodeId),
                            new NpgsqlParameter("ruleid", line.RuleId.GetValueOrDefault()),
                            new NpgsqlParameter("vernum", versionValue),
                            new NpgsqlParameter("lineconfig", JsonConvert.SerializeObject(line.LineConfig)){ NpgsqlDbType= NpgsqlTypes.NpgsqlDbType.Jsonb },
                        });
                    }
                    ExecuteNonQueryMultiple(node_line_sql, node_line_params, tran);
                    #endregion

                    #region --更新主流程版本号--
                    //更新主流程版本号
                    var workflow_sql = @"UPDATE crm_sys_workflow SET vernum = @vernum WHERE flowid = @flowid;";

                    var workflow_params = new DbParameter[]
                        {
                            new NpgsqlParameter("flowid", nodeLineConfig.FlowId),
                            new NpgsqlParameter("vernum", versionValue)
                        };
                    ExecuteNonQuery(workflow_sql, workflow_params, tran);
                    #endregion

                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }

            }
        }
        public dynamic GetFreeFlowNodeEvents(Guid flowId, DbTransaction tran = null)
        {
            var executeSql = @"(SELECT e.funcname,e.steptype FROM crm_sys_workflow_func_event e
                                INNER JOIN crm_sys_workflow w ON w.flowid=e.flowid 
                                WHERE w.flowtype=0 AND e.steptype=0 AND e.flowid=@flowid LIMIT 1 
                                )
                                UNION
                                (SELECT e.funcname,e.steptype FROM crm_sys_workflow_func_event e
                                INNER JOIN crm_sys_workflow w ON w.flowid=e.flowid
                                WHERE w.flowtype=0 AND e.steptype=1 AND e.flowid=@flowid LIMIT 1 
                                )";
            var param = new DbParameter[]
           {
                new NpgsqlParameter("flowid", flowId),

           };

            return ExecuteQuery(executeSql, param);
        }

        public void SaveNodeEvents(Guid flowId, List<WorkFlowNodeMapper> nodes, DbTransaction tran = null)
        {


            List<DbParameter[]> node_eve_params = new List<DbParameter[]>();
            foreach (var node in nodes)
            {
                node_eve_params.Add(new DbParameter[]
                    {
                            new NpgsqlParameter("nodeid", node.NodeId),
                            new NpgsqlParameter("flowid", flowId),
                            new NpgsqlParameter("funcname", node.NodeEvent),
                            new NpgsqlParameter("steptype", node.StepTypeId==0?0:1)
                    });
            }
            if (node_eve_params.Count > 0)
            {
                var node_event_sql = @"
DELETE FROM crm_sys_workflow_func_event WHERE flowid=@flowid AND steptype=@steptype ;
INSERT INTO crm_sys_workflow_func_event(flowid,funcname,nodeid,steptype)
                                              VALUES(@flowid,@funcname,@nodeid,@steptype)";
                ExecuteNonQueryMultiple(node_event_sql, node_eve_params, tran);
            }
        }

        public OperateResult NodeLinesConfig(WorkFlowNodeLinesConfigMapper nodeLineConfig, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_workflow_data_node_line_insert(@flowId,@nodesdata,@linesdata, @userno)
            ";

            var param = new
            {
                FlowId = nodeLineConfig.FlowId,
                NodesData = JsonHelper.ToJson(nodeLineConfig.Nodes),
                LinesData = JsonHelper.ToJson(nodeLineConfig.Lines),
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }


        public Dictionary<string, List<IDictionary<string, object>>> FlowList(PageParam pageParam, int flowstatus, string searchName, int userNumber)
        {
            var procName =
              "SELECT crm_func_workflow_list(@flowstatus,@searchname,@pageIndex,@pageSize,@userno)";

            var dataNames = new List<string>();

            var param = new
            {
                FlowStatus = flowstatus,
                SearchName = searchName,
                PageIndex = pageParam.PageIndex,
                PageSize = pageParam.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> Detail(Guid flowId, int userNumber)
        {
            var procName =
           "SELECT crm_func_workflow_detail(@flowId,@userNo)";

            var param = new
            {
                FlowId = flowId,
                UserNo = userNumber
            };

            var dataNames = new List<string>();

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult AddFlow(WorkFlowAddMapper flowMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_workflow_add(@entityId,@flowName,@flowType,@backFlag,@resetFlag,@expireDay,@remark,@skipFlag, @userno,@flowlanguage::jsonb, @config::jsonb)
            ";

            var param = new
            {
                EntityId = flowMapper.EntityId,
                FlowName = flowMapper.FlowName,
                FlowType = flowMapper.FlowType,
                BackFlag = flowMapper.BackFlag,
                ResetFlag = flowMapper.ResetFlag,
                ExpireDay = flowMapper.ExpireDay,
                Remark = flowMapper.Remark,
                SkipFlag = flowMapper.SkipFlag,
                UserNo = userNumber,
                FlowLanguage = JsonConvert.SerializeObject(flowMapper.FlowName_Lang),
                Config = JsonConvert.SerializeObject(flowMapper.Config)
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateFlow(WorkFlowUpdateMapper flowMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_workflow_update(@flowId,@flowName,@backFlag,@resetFlag,@expireDay,@remark,@skipFlag, @userno,@flowlanguage::jsonb, @config::jsonb)
            ";

            var param = new
            {
                FlowId = flowMapper.FlowId,
                FlowName = flowMapper.FlowName,
                BackFlag = flowMapper.BackFlag,
                ResetFlag = flowMapper.ResetFlag,
                ExpireDay = flowMapper.ExpireDay,
                Remark = flowMapper.Remark,
                SkipFlag = flowMapper.SkipFlag,
                UserNo = userNumber,
                flowlanguage = JsonConvert.SerializeObject(flowMapper.FlowName_Lang),
                Config = JsonConvert.SerializeObject(flowMapper.Config)
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DeleteFlow(string flowIds, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_workflow_delete(@flowIds, @userno)
            ";

            var param = new
            {
                FlowIds = flowIds,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult UnDeleteFlow(string flowIds, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_workflow_undelete(@flowIds, @userno)
            ";

            var param = new
            {
                FlowIds = flowIds,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }


        public WorkFlowInfo GetWorkFlowInfo(DbTransaction trans, Guid flowid)
        {
            var executeSql = @"SELECT w.*,e.relentityid,u.username AS RecCreator_name FROM crm_sys_workflow  AS w
                               LEFT JOIN crm_sys_entity AS e ON e.entityid = w.entityid 
                               LEFT JOIN crm_sys_userinfo AS u ON u.userid = w.reccreator
                               WHERE flowid=@flowid";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("flowid", flowid),

            };
            if (trans == null)
            {
                return DBHelper.ExecuteQuery<WorkFlowInfo>("", executeSql, param).FirstOrDefault();
            }
            return DBHelper.ExecuteQuery<WorkFlowInfo>(trans, executeSql, param).FirstOrDefault();
        }

        public WorkFlowNodeInfo GetWorkFlowNodeInfo(DbTransaction trans, Guid nodeid)
        {
            var executeSql = @"SELECT * FROM crm_sys_workflow_node
                               WHERE nodeid = @nodeid LIMIT 1";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("nodeid", nodeid),
            };
            if (trans == null)
            {
                return DBHelper.ExecuteQuery<WorkFlowNodeInfo>("", executeSql, param).FirstOrDefault();
            }
            return DBHelper.ExecuteQuery<WorkFlowNodeInfo>(trans, executeSql, param).FirstOrDefault();
        }

        /// <summary>
        /// 获取前一节点
        /// </summary>
        public WorkFlowNodeInfo GetPreviousWorkFlowNodeInfo(DbTransaction trans, Guid flowid, int vernum, Guid nodeid)
        {
            var executeSql = @"SELECT * FROM crm_sys_workflow_node
                               WHERE flowid = @flowid AND vernum = @vernum AND nodeid IN( 
                               SELECT tonodeid FROM crm_sys_workflow_node_line WHERE vernum=@vernum AND tonodeid=@tonodeid AND flowid = @flowid)  LIMIT 1";

            var param = new DbParameter[]
             {
                new NpgsqlParameter("flowid", flowid),
                new NpgsqlParameter("vernum", vernum),
                new NpgsqlParameter("tonodeid", nodeid),
             };
            return ExecuteQuery<WorkFlowNodeInfo>(executeSql, param, trans).FirstOrDefault();

        }


        public List<WorkFlowNodeInfo> GetNextNodeInfoList(DbTransaction trans, Guid flowid, int vernum, Guid fromnodeid)
        {
            var executeSql = @"SELECT * FROM crm_sys_workflow_node
                               WHERE flowid = @flowid AND vernum = @vernum AND nodeid IN( 
                               SELECT tonodeid FROM crm_sys_workflow_node_line WHERE vernum=@vernum AND fromnodeid=@fromnodeid AND flowid = @flowid )";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("flowid", flowid),
                new NpgsqlParameter("vernum", vernum),
                new NpgsqlParameter("fromnodeid", fromnodeid),
            };
            return ExecuteQuery<WorkFlowNodeInfo>(executeSql, param, trans);

        }

        public List<WorkFlowNodeInfo> GetNodeInfoList(DbTransaction trans, Guid flowid, int vernum)
        {
            var executeSql = @"SELECT * FROM crm_sys_workflow_node WHERE flowid = @flowid AND vernum = @vernum ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("flowid", flowid),
                new NpgsqlParameter("vernum", vernum),

            };
            return ExecuteQuery<WorkFlowNodeInfo>(executeSql, param, trans);
        }



        public WorkFlowCaseInfo GetWorkFlowCaseInfo(DbTransaction trans, Guid caseid)
        {
            var executeSql = @" SELECT c.*,w.entityid, er.relentityid,er.relrecid ,u.username AS RecCreator_Name
                                FROM crm_sys_workflow_case AS c
                                LEFT JOIN crm_sys_workflow_case_entity_relation AS er ON er.caseid = c.caseid
                                LEFT JOIN crm_sys_workflow AS w ON w.flowid = c.flowid
                                LEFT JOIN crm_sys_userinfo AS u ON u.userid = c.reccreator
                                WHERE c.caseid = @caseid";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("caseid", caseid),

            };
            return ExecuteQuery<WorkFlowCaseInfo>(executeSql, param, trans).FirstOrDefault();
        }


        public List<WorkFlowCaseItemInfo> GetWorkFlowCaseItemInfo(DbTransaction trans, Guid caseid, int nodenum, int stepnum = -1)
        {
            string executeSql = string.Empty;
            if (stepnum <= 0)
            {
                executeSql = @" SELECT  wci.* ,u.username AS HandleUserName
                                FROM crm_sys_workflow_case_item AS wci
                                LEFT JOIN crm_sys_userinfo AS u ON u.userid = wci.handleuser
                                WHERE wci.recstatus=1 AND wci.nodenum=@nodenum AND wci.caseid=@caseid 
                                AND wci.stepnum=(SELECT MAX(stepnum) FROM crm_sys_workflow_case_item WHERE recstatus=1 AND nodenum=@nodenum AND caseid=@caseid )";
            }
            else
            {
                executeSql = @" SELECT  wci.* ,u.username AS HandleUserName
                                FROM crm_sys_workflow_case_item AS wci
                                LEFT JOIN crm_sys_userinfo AS u ON u.userid = wci.handleuser
                                WHERE wci.recstatus=1 AND wci.nodenum=@nodenum AND wci.caseid=@caseid  AND wci.stepnum=@stepnum";
            }

            var param = new DbParameter[]
            {
                new NpgsqlParameter("caseid", caseid),
                new NpgsqlParameter("nodenum", nodenum),
                new NpgsqlParameter("stepnum", stepnum),
            };
            return ExecuteQuery<WorkFlowCaseItemInfo>(executeSql, param, trans);
        }
        public List<WorkFlowCaseInfo> getWorkFlowCaseListByRecId(DbTransaction trans, string recid, int userNum)
        {
            var executeSql = @" SELECT c.*,w.entityid, er.relentityid,er.relrecid 
                                FROM crm_sys_workflow_case AS c
                                LEFT JOIN crm_sys_workflow_case_entity_relation AS er ON er.caseid = c.caseid
                                LEFT JOIN crm_sys_workflow AS w ON w.flowid = c.flowid
                                where c.recid = @recid";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("recid", Guid.Parse(recid)),

            };
            return ExecuteQuery<WorkFlowCaseInfo>(executeSql, param, trans);
        }
        public List<WorkFlowCaseInfo> getWorkFlowCaseListByRecIds(DbTransaction trans, List<string> recids, int userNum)
        {

            List<Guid> inids = new List<Guid>();
            string ids = "";
            foreach (string recid in recids)
            {
                ids = ids + ",'" + recid + "'";
            }
            if (ids.Length > 0)
            {
                ids = ids.Substring(1);
            }
            var executeSql = string.Format(@" SELECT c.*,w.entityid, er.relentityid,er.relrecid 
                                FROM crm_sys_workflow_case AS c
                                LEFT JOIN crm_sys_workflow_case_entity_relation AS er ON er.caseid = c.caseid
                                LEFT JOIN crm_sys_workflow AS w ON w.flowid = c.flowid
                                where c.recid::text  in ({0})", ids);
            return ExecuteQuery<WorkFlowCaseInfo>(executeSql, new DbParameter[] { }, trans);
        }

        public int getWorkFlowCountByStageId(DbTransaction trans, string stageid, int userNumber)
        {
            try
            {
                string cmdText = string.Format(@"select count(distinct f.caseid)  from crm_sys_workflow_case f inner join crm_sys_workflow_case_item e  on f.caseid = e.caseid 
                                                where  f.auditstatus = 0 and  jsonb_extract_path_text(e.casedata,'salesstageids')  ilike	 '%{0}'
                                                    ", stageid.Replace("'", "'"));
                object obj = ExecuteScalar(cmdText, new DbParameter[] { }, trans);
                if (obj == null) return 0;
                return int.Parse(obj.ToString());
            }
            catch (Exception e)
            {

            }
            return -1;
        }

        /// <summary>
        /// 获取当前流程执行到哪个节点
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="userNumber"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public int GetWorkFlowNowNodeNumber(Guid caseid, int userNumber, DbTransaction trans = null)
        {
            int nowNodeNum = -1;
            string cmdText = @"select crm_func_workflow_calculate_nownode(@caseid,@userno)";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("caseid", caseid),
                new NpgsqlParameter("userno", userNumber),
            };
            object obj = ExecuteScalar(cmdText, param, trans);
            if (obj != null)
            {
                int.TryParse(obj.ToString(), out nowNodeNum);
            }

            return nowNodeNum;
        }

        public int GetWorkFlowNextNodeNumber(Guid caseid, int nodenum, int userNumber, DbTransaction trans = null)
        {

            int nextNodeNum = -1;
            string cmdText = @"select crm_func_workflow_calculate_nextnode(@caseid,@nodenum,@userno)";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("caseid", caseid),
                new NpgsqlParameter("nodenum", nodenum),
                new NpgsqlParameter("userno", userNumber),
            };
            object obj = ExecuteScalar(cmdText, param, trans);
            if (obj != null)
            {
                int.TryParse(obj.ToString(), out nextNodeNum);
            }

            return nextNodeNum;
        }

        public List<NextNodeDataInfo> GetNextNodeDataInfoList(Guid flowid, Guid fromnodeid, int vernum, DbTransaction trans = null)
        {
            var executeSql = @" SELECT n.nodeid,n.nodename,n.nodetype,n.nodenum,n.steptypeid,n.stepcptypeid,n.notfound,wf.flowtype
								FROM crm_sys_workflow_node AS n
                                INNER JOIN crm_sys_workflow_node_line AS nl ON nl.vernum=@vernum AND nl.fromnodeid=@fromnodeid AND nl.flowid = @flowid 
								LEFT JOIN crm_sys_workflow_steptype AS s ON s.steptypeid = n.steptypeid
                                 LEFT JOIN crm_sys_workflow AS wf ON wf.flowid =@flowid 
								WHERE n.nodeid = nl.tonodeid ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("flowid", flowid),
                new NpgsqlParameter("fromnodeid", fromnodeid),
                new NpgsqlParameter("vernum", vernum),
            };
            return ExecuteQuery<NextNodeDataInfo>(executeSql, param, trans);
        }
        public List<NextNodeDataInfo> GetNodeDataInfo(Guid flowid, Guid nodeid, int vernum, DbTransaction trans = null)
        {
            var executeSql = @" SELECT n.nodeid,n.nodename,n.nodetype,n.nodenum,n.steptypeid,n.stepcptypeid,n.notfound,wf.flowtype
								FROM crm_sys_workflow_node AS n
                                INNER JOIN crm_sys_workflow_node_line AS nl ON nl.vernum=@vernum AND nl.fromnodeid=@fromnodeid AND nl.flowid = @flowid 
								LEFT JOIN crm_sys_workflow_steptype AS s ON s.steptypeid = n.steptypeid
                                 LEFT JOIN crm_sys_workflow AS wf ON wf.flowid =@flowid 
								WHERE n.nodeid = nl.fromnodeid ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("flowid", flowid),
                new NpgsqlParameter("fromnodeid", nodeid),
                new NpgsqlParameter("vernum", vernum),
            };
            return ExecuteQuery<NextNodeDataInfo>(executeSql, param, trans);
        }
        public List<NextNodeDataInfo> GetCurNodeDataInfo(Guid flowid, Guid nodeid, int vernum, DbTransaction trans = null)
        {
            var executeSql = @" SELECT n.nodeid,n.nodename,n.nodetype,n.nodenum,n.steptypeid,n.stepcptypeid,n.notfound,wf.flowtype
								FROM crm_sys_workflow_node AS n
								LEFT JOIN crm_sys_workflow_steptype AS s ON s.steptypeid = n.steptypeid
                                 LEFT JOIN crm_sys_workflow AS wf ON wf.flowid =@flowid 
								WHERE n.nodeid = @nodeid ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("flowid", flowid),
                new NpgsqlParameter("nodeid", nodeid),
                new NpgsqlParameter("vernum", vernum),
            };
            return ExecuteQuery<NextNodeDataInfo>(executeSql, param, trans);
        }
        #region --获取下一步节点的审批人列表--
        public List<ApproverInfo> GetFlowNodeApprovers(Guid caseId, Guid nodeid, int userNumber, WorkFlowType flowtype, DbTransaction trans = null)
        {
            string cmdText = null;
            var result = new List<ApproverInfo>();
            if (flowtype == WorkFlowType.FreeFlow)//自由流程
            {
                cmdText = @"SELECT u.userid,u.username,u.usericon,u.namepinyin,ur.deptid,d.deptname 
                            FROM crm_sys_userinfo AS u
			                LEFT JOIN crm_sys_account_userinfo_relate AS ur ON u.userid = ur.userid AND ur.recstatus = 1
			                LEFT JOIN crm_sys_department AS d ON ur.deptid = d.deptid
			                WHERE u.recstatus = 1";

                result = ExecuteQuery<ApproverInfo>(cmdText, null, trans);

            }
            else
            {
                cmdText = @"SELECT u.userid,u.username,u.usericon,u.namepinyin,ur.deptid,d.deptname FROM crm_sys_userinfo AS u
                           LEFT JOIN crm_sys_account_userinfo_relate AS ur ON u.userid = ur.userid AND ur.recstatus = 1
                           LEFT JOIN crm_sys_department AS d ON ur.deptid = d.deptid
                           WHERE u.recstatus = 1";


                var caseInfo = GetWorkFlowCaseInfo(trans, caseId);
                if (caseInfo == null)
                    throw new Exception("未找到相关联的流程");
                var flowNodeInfo = GetWorkFlowNodeInfo(trans, nodeid);
                if (flowNodeInfo == null)
                    throw new Exception("未找到相关联的流程节点");

                List<DbParameter> param = new List<DbParameter>();

                switch (flowNodeInfo.StepTypeId)
                {
                    case NodeStepType.Launch://0:发起审批,
                        break;
                    case NodeStepType.SelectByUser:// 1:让用户自己选择审批人

                        break;
                    case NodeStepType.SpecifyApprover:// 2:指定审批人, 
                        cmdText += @" AND u.userid IN (
													SELECT userid::INT from (
																SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'userid'), ',')) AS userid 
													) as r WHERE r.userid!=''
                                       )";
                        break;
                    case NodeStepType.Joint:// 3:会审,
                        cmdText += @" AND u.userid IN (SELECT userid::INT from (
																SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'userid'), ',')) AS userid
                                                    ) as r WHERE r.userid!=''
                                       )";
                        break;
                    case NodeStepType.SpecifyRole://4:指定审批人的角色(特定),
                        cmdText += @" AND EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate AS ro WHERE ro.userid = u.userid AND ro.roleid IN (
                                      SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb, 'roleid'), ',')) AS roleid )as r) LIMIT 1)";
                        break;
                    case NodeStepType.SpecifyDepartment://5:指定审批人所在团队(特定部门),
                        cmdText += @"  AND ur.deptid in (
 SELECT deptid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'deptid')
,',')) as deptid) as r)   " + (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader");
                        break;
                    case NodeStepType.SpecifyDepartment_Role://6:指定审批人所在团队及角色(特定),
                        cmdText += @" AND ur.deptid = jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'deptid')::uuid 
                                      AND EXISTS(
                                          SELECT 1 FROM crm_sys_userinfo_role_relate AS ro WHERE ro.userid = u.userid AND ro.roleid IN (
                                                SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'roleid'), ',')) AS roleid )as r
                                                ) LIMIT 1
                                          ) ";
                        break;
                    case NodeStepType.WorkFlowCreator://7:流程发起人,
                        cmdText += @" AND u.userid = @casecreator";
                        break;


                    #region --指定审批人所在团队--

                    #region --8XX 用户所在部门--
                    case NodeStepType.ApproverDept://8:指定审批人所在团队-用户所在部门-上一步处理人,
                        cmdText += @"  AND ur.deptid =  (SELECT deptid FROM crm_sys_account_userinfo_relate WHERE userid = @userno AND recstatus = 1 LIMIT 1) "+(string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader");
                        break;
                    case NodeStepType.ApproverDept_Launcher://801:指定审批人所在团队-用户所在部门-流程发起人,
                        cmdText += @"  AND ur.deptid =  (SELECT deptid FROM crm_sys_account_userinfo_relate WHERE userid = @casecreator AND recstatus = 1 LIMIT 1) " + (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader");
                        break;
                    case NodeStepType.ApproverDept_Select://802:指定审批人所在团队-用户所在部门-表单中选人控件,
                                                          //{"fieldname": "recmanager"}
                        if (flowNodeInfo.RuleConfig != null)
                        {
                            string fieldname, entityTableName;
                            GetEntityField(trans, caseInfo, flowNodeInfo, out fieldname, out entityTableName);
                            cmdText += string.Format(@"  AND ur.deptid IN  (SELECT deptid FROM crm_sys_account_userinfo_relate WHERE userid IN (
													SELECT userid::INT from (
																SELECT UNNEST( string_to_array((SELECT {0}::text FROM {1} WHERE recid=@recid LIMIT 1), ',')) AS userid 
													) as r WHERE r.userid!=''
                                       )
                                    AND recstatus = 1 )  " + (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader"), fieldname, entityTableName);
                        }
                        break;
                    #endregion

                    #region --11X 用户所在部门的上级部门--
                    case NodeStepType.ApproverPreDepatrment://11:指定审批人所在团队-用户所在部门的上级部门-上一步处理人,
                        cmdText += @"  AND ur.deptid =  (SELECT s.pdeptid FROM crm_sys_department AS s WHERE s.deptid = (
															    SELECT e.deptid FROM crm_sys_account_userinfo_relate AS e WHERE e.userid = @userno AND e.recstatus = 1 LIMIT 1
                                                                )
                                                            )   " + (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader");
                        break;
                    case NodeStepType.ApproverPreDepatrment_Launcher://111:指定审批人所在团队-用户所在部门的上级部门-流程发起人,
                        cmdText += @"  AND ur.deptid =  (SELECT s.pdeptid FROM crm_sys_department AS s WHERE s.deptid = (
															    SELECT e.deptid FROM crm_sys_account_userinfo_relate AS e WHERE e.userid = @casecreator AND e.recstatus = 1 LIMIT 1
                                                                )
                                                            )  " + (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader");
                        break;


                    case NodeStepType.ApproverPreDepatrment_Select://112:指定审批人所在团队-用户所在部门的上级部门-表单中选人控件,
                                                                   //{"fieldname": "recmanager"}
                        if (flowNodeInfo.RuleConfig != null)
                        {
                            string fieldname, entityTableName;
                            GetEntityField(trans, caseInfo, flowNodeInfo, out fieldname, out entityTableName);
                            cmdText += string.Format(@"  AND ur.deptid IN  (SELECT s.pdeptid FROM crm_sys_department AS s WHERE s.deptid = (
															    SELECT e.deptid FROM crm_sys_account_userinfo_relate AS e WHERE e.userid IN (
													                            SELECT userid::INT from (
																                            SELECT UNNEST( string_to_array((SELECT {0}::text FROM {1} WHERE recid=@recid LIMIT 1), ',')) AS userid 
													                            ) as r WHERE r.userid!=''
                                                                   ) AND e.recstatus = 1 
                                                                )
                                                            )   and" + (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader"), fieldname, entityTableName);
                        }
                        break;
                    #endregion

                    #region --12X 用户管辖部门--
                    case NodeStepType.ApproverDeptWidthChild://12:指定审批人所在团队-用户管辖部门-上一步处理人,
                        break;
                    case NodeStepType.ApproverDeptWidthChild_Launcher://121:指定审批人所在团队-用户管辖部门-流程发起人,
                        break;
                    case NodeStepType.ApproverDeptWidthChild_Select://122:指定审批人所在团队-用户管辖部门-表单中选人控件,
                        break;
                    #endregion

                    #endregion

                    #region --指定审批人所在团队及角色--

                    #region --9XX 用户所在部门--
                    case NodeStepType.ApproverDept_Role://9:指定审批人所在团队及角色-用户所在部门-上一步处理人,
                        cmdText += @"  AND ur.deptid =  (SELECT deptid FROM crm_sys_account_userinfo_relate WHERE userid = @userno AND recstatus = 1 LIMIT 1)
                                           AND EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate AS ro WHERE ro.userid = u.userid AND ro.roleid IN (
                                                      SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'roleid'), ',')) AS roleid )as r
                                                      ) LIMIT 1
                                               )";

                        break;

                    case NodeStepType.ApproverDept_Role_Launcher://901:指定审批人所在团队及角色-用户所在部门-流程发起人,
                        cmdText += @"  AND ur.deptid =  (SELECT deptid FROM crm_sys_account_userinfo_relate WHERE userid = @casecreator AND recstatus = 1 LIMIT 1)
                                           AND EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate AS ro WHERE ro.userid = u.userid AND ro.roleid IN (
                                                      SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'roleid'), ',')) AS roleid )as r
                                                      ) LIMIT 1
                                               )";

                        break;
                    case NodeStepType.ApproverDept_Role_Select://902:指定审批人所在团队及角色-用户所在部门-表单中选人控件,
                                                               //{"fieldname": "recmanager"}
                        if (flowNodeInfo.RuleConfig != null)
                        {
                            string fieldname, entityTableName;
                            GetEntityField(trans, caseInfo, flowNodeInfo, out fieldname, out entityTableName);
                            cmdText += string.Format(@" {2} AND ur.deptid IN  (SELECT deptid FROM crm_sys_account_userinfo_relate WHERE userid IN (
													                            SELECT userid::INT from (
																                            SELECT UNNEST( string_to_array((SELECT {0}::text FROM {1} WHERE recid=@recid LIMIT 1), ',')) AS userid 
													                            ) as r WHERE r.userid!=''
                                                                        ) AND recstatus = 1 )
                                                        AND EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate AS ro WHERE ro.userid = u.userid AND ro.roleid IN (
                                                                  SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'roleid'), ',')) AS roleid )as r
                                                                  ) LIMIT 1
                                                           )", fieldname, entityTableName, (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader"));
                        }
                        break;
                    #endregion

                    #region --10X 用户所在部门的上级部门--
                    case NodeStepType.ApproverPreDept_Role://10:指定审批人所在团队及角色-用户所在部门的上级部门-上一步处理人,
                        cmdText += @"  AND ur.deptid = (SELECT s.pdeptid  FROM crm_sys_department AS s WHERE s.deptid = (
									                            SELECT e.deptid FROM crm_sys_account_userinfo_relate AS e WHERE e.userid = @userno AND e.recstatus = 1 LIMIT 1
                                                                )
                                                            )
                                           AND EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate AS ro WHERE ro.userid = u.userid AND ro.roleid IN (
                                                    SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'roleid'), ',')) AS roleid )as r
                                                    ) LIMIT 1
                                               )
                                       ";
                        break;
                    case NodeStepType.ApproverPreDept_Role_Launcher://101:指定审批人所在团队及角色-用户所在部门的上级部门-流程发起人,
                        cmdText += @"  AND ur.deptid = (SELECT s.pdeptid  FROM crm_sys_department AS s WHERE s.deptid = (
									                            SELECT e.deptid FROM crm_sys_account_userinfo_relate AS e WHERE e.userid = @casecreator AND e.recstatus = 1 LIMIT 1
                                                                )
                                                            )
                                           AND EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate AS ro WHERE ro.userid = u.userid AND ro.roleid IN (
                                                    SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'roleid'), ',')) AS roleid )as r
                                                    ) LIMIT 1
                                               )
                                       ";
                        break;
                    case NodeStepType.ApproverPreDept_Role_Select://102:指定审批人所在团队及角色-用户所在部门的上级部门-表单中选人控件,
                                                                  //{"fieldname": "recmanager"}
                        if (flowNodeInfo.RuleConfig != null)
                        {
                            string fieldname, entityTableName;
                            GetEntityField(trans, caseInfo, flowNodeInfo, out fieldname, out entityTableName);
                            cmdText += string.Format(@"   AND ur.deptid IN (SELECT s.pdeptid  FROM crm_sys_department AS s WHERE s.deptid = (
									                            SELECT e.deptid FROM crm_sys_account_userinfo_relate AS e WHERE e.userid IN (
													                            SELECT userid::INT from (
																                            SELECT UNNEST( string_to_array((SELECT {0}::text FROM {1} WHERE recid=@recid LIMIT 1), ',')) AS userid 
													                            ) as r WHERE r.userid!=''
                                                                        ) AND e.recstatus = 1 
                                                                )
                                                            )
                                                       AND EXISTS(SELECT 1 FROM crm_sys_userinfo_role_relate AS ro WHERE ro.userid = u.userid AND ro.roleid IN (
                                                                SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'roleid'), ',')) AS roleid )as r
                                                                ) LIMIT 1
                                                           )
                                                   ", fieldname, entityTableName, (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader"));
                        }
                        break;
                    #endregion


                    #region --13X 用户管辖部门--
                    case NodeStepType.ApproverDeptWidthChild_Role://13:指定审批人所在团队及角色-用户管辖部门-上一步处理人
                        break;
                    case NodeStepType.ApproverDeptWidthChild_Role_Launcher://131:指定审批人所在团队及角色-用户管辖部门-流程发起人,
                        break;
                    case NodeStepType.ApproverDeptWidthChild_Role_Select://132:指定审批人所在团队及角色-用户管辖部门-表单中选人控件,
                        break;
                    #endregion
                    #endregion
                    #region 14X 指定表单中的团队字段
                    case NodeStepType.FormDeptGroup:
                        string f1, e1;
                        GetEntityFieldBasicRuleConfig(trans, caseInfo, flowNodeInfo, out f1, out e1);
                        if (GetRuleConfigInfo("entityid", flowNodeInfo) == caseInfo.RelEntityId.ToString())//判断是否是主实体还是关联实体
                        {
                            cmdText += string.Format(@" AND u.userid in (
                            SELECT u1.userid FROM (
                            select * from crm_sys_account_userinfo_relate where deptid in (
                            select regexp_split_to_table({0}::text,',')::uuid as deptid from {1} where recid=@relrecid 
                            ) AND recstatus=1
                            ) as tmp 
                            LEFT JOIN crm_sys_userinfo u1 on u1.userid=tmp.userid 
                            LEFT JOIN crm_sys_department d on tmp.deptid=d.deptid AND d.recstatus=1 {2}
                            )", f1, e1, (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " where u1.isleader=@isleader"));
                        }
                        else
                        {
                            cmdText += string.Format(@" AND u.userid in (
                            SELECT u1.userid FROM (
                            select * from crm_sys_account_userinfo_relate where deptid in (
                            select  regexp_split_to_table({0}::text,',')::uuid as deptid from {1} where recid=@recid
                            ) AND recstatus=1
                            ) as tmp 
                            LEFT JOIN crm_sys_userinfo u1 on u1.userid=tmp.userid 
                            LEFT JOIN crm_sys_department d on tmp.deptid=d.deptid AND d.recstatus=1 {2}
                            )", f1, e1, (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " where u1.isleader=@isleader"));
                        }
                        break;
                    case NodeStepType.FormDeptGroupForRole:
                        string f2, e2;
                        GetEntityFieldBasicRuleConfig(trans, caseInfo, flowNodeInfo, out f2, out e2);
                        if (GetRuleConfigInfo("entityid", flowNodeInfo) == caseInfo.RelEntityId.ToString())//判断是否是主实体还是关联实体
                        {
                            cmdText += string.Format(@"  AND u.userid in (
     select userid from crm_sys_account_userinfo_relate as ur where deptid in (
                            select regexp_split_to_table({0}::text,',')::uuid as deptid from {1} where recid=@relrecid
                            )
                            )  and EXISTS(select 1 from crm_sys_userinfo_role_relate where userid=ur.userid and  roleid in (SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'roleid'), ',')) AS roleid )as r  ))", f2, e2);
                        }
                        else
                        {
                            cmdText += string.Format(@"   AND u.userid in (
     select userid from crm_sys_account_userinfo_relate as ur where deptid in (
                            select regexp_split_to_table({0}::text,',')::uuid as deptid from {1} where recid=@recid)
                            )   and EXISTS(select 1 from crm_sys_userinfo_role_relate where userid=ur.userid and  roleid in (SELECT roleid::uuid from (SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'roleid'), ',')) AS roleid )as r ))
                            ", f2, e2);
                        }
                        break;
                    #endregion


                    #region 15X 汇报关系
                    case NodeStepType.ReportRelation:
                        JToken reportRelation = GetRuleConfigObject("reportrelation", flowNodeInfo);
                        if (reportRelation != null)
                        {
                            var jo = JObject.Parse(reportRelation.ToString());
                            Guid id = Guid.Parse(jo["id"].ToString());
                            int type = Convert.ToInt32(jo["type"].ToString());
                            var reportRelationRepository = ServiceLocator.Current.GetInstance<IReportRelationRepository>();
                            QueryReportRelDetailMapper model = new QueryReportRelDetailMapper();
                            switch (type)
                            {
                                case 1://流程发起人
                                    var l1 = CaseItemList(caseInfo.CaseId, userNumber);
                                    model = new QueryReportRelDetailMapper
                                    {
                                        ReportRelationId = id,
                                        UserId = l1.Count == 0 ? caseInfo.RecCreator : Convert.ToInt32(l1.FirstOrDefault()["handleuser"].ToString())
                                    };
                                    var d1 = reportRelationRepository.GetReportRelDetail(model, trans, userNumber);
                                    cmdText += @" AND u.userid in (" + string.Join(",", (d1.Count == 0 ? new string[] { "-1" } : d1.Select(t => t.ReportLeader))) + ")";
                                    break;
                                case 2://上一步骤处理人
                                    var l2 = CaseItemList(caseInfo.CaseId, userNumber);
                                    if (l2.Count == 0)
                                    {
                                        model = new QueryReportRelDetailMapper
                                        {
                                            ReportRelationId = id,
                                            UserId = l2.Count == 0 ? caseInfo.RecCreator : Convert.ToInt32(l2.FirstOrDefault()["handleuser"].ToString())
                                        };
                                        var d2 = reportRelationRepository.GetReportRelDetail(model, trans, userNumber);
                                        cmdText += @" AND u.userid in (" + string.Join(",", (d2.Count == 0 ? new string[] { "-1" } : d2.Select(t => t.ReportLeader))) + ")";
                                    }
                                    else
                                    {
                                        int index = l2.Count - 1;
                                        string userid = string.Empty;
                                        if (index > 0)
                                        {
                                            userid = l2[index]["handleuser"].ToString();
                                            model = new QueryReportRelDetailMapper
                                            {
                                                ReportRelationId = id,
                                                UserId = Convert.ToInt32(userid)
                                            };
                                            var d2 = reportRelationRepository.GetReportRelDetail(model, trans, userNumber);
                                            cmdText += @" AND u.userid in (" + string.Join(",", (d2.Count == 0 ? new string[] { "-1" } : d2.Select(t => t.ReportLeader))) + ")";
                                        }
                                    }
                                    break;
                                case 3://表单中的人员
                                    string f3, e3;
                                    GetEntityFieldBasicRuleConfig(trans, caseInfo, flowNodeInfo, out f3, out e3);
                                    if (GetRuleConfigInfo("entityid", flowNodeInfo) == caseInfo.RelEntityId.ToString())//判断是否是主实体还是关联实体
                                    {
                                        cmdText += string.Format(@" AND u.userid in  (select t1.userid from(
                                        SELECT regexp_split_to_table(reportleader,',')::int4 as userid from crm_sys_reportreldetail 
                                        where  reportrelationid='{2}'::uuid and 
                                        EXISTS (
                                        select 1 from (
                                        select regexp_split_to_table({0}::text,',')::int4 as userid from {1} where recid=@relrecid
                                        INTERSECT
                                        select  regexp_split_to_table(reportuser,',') ::int4
                                        ) as t2)   ) as t1 GROUP BY t1.userid
                                         )  ", f3, e3, id.ToString());
                                    }
                                    else
                                    {
                                        cmdText += string.Format(@" AND u.userid in  (select t1.userid from(
                                        SELECT regexp_split_to_table(reportleader,',')::int4 as userid from crm_sys_reportreldetail 
                                        where  reportrelationid='{2}'::uuid and 
                                        EXISTS (
                                        select 1 from (
                                        select regexp_split_to_table({0}::text,',')::int4 as userid from {1} where recid=@recid
                                        INTERSECT
                                        select  regexp_split_to_table(reportuser,',') ::int4
                                        ) as t2)   ) as t1 GROUP BY t1.userid
                                         )  ", f3, e3, id.ToString());
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    #endregion

                    #region 指定函数
                    case NodeStepType.Function:
                        JToken funcname = GetRuleConfigObject("funcname", flowNodeInfo);
                        param.Add(new NpgsqlParameter("ruleconfig", flowNodeInfo.RuleConfig) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });
                        param.Add(new NpgsqlParameter("userno", userNumber));
                        param.Add(new NpgsqlParameter("recid", caseInfo.RecId));
                        param.Add(new NpgsqlParameter("relrecid", caseInfo.RelRecId));
                        param.Add(new NpgsqlParameter("isleader", GetRuleConfigInfo("isleader", flowNodeInfo) == null ? 0 : Convert.ToInt32(GetRuleConfigInfo("isleader", flowNodeInfo))));
                        param.Add(new NpgsqlParameter("caseid", caseId));
                        result = ExecuteQuery<ApproverInfo>("select * from " + funcname.ToString() + "(@caseid,@ruleconfig,@recid,@relrecid,@userno);", param.ToArray(), trans);
                        return result;
                        break;

                    #endregion

                    default:
                        cmdText += " AND 1=2";
                        break;
                }
                param.Add(new NpgsqlParameter("ruleconfig", flowNodeInfo.RuleConfig) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });
                param.Add(new NpgsqlParameter("userno", userNumber));
                param.Add(new NpgsqlParameter("recid", caseInfo.RecId));
                param.Add(new NpgsqlParameter("relrecid", caseInfo.RelRecId));
                param.Add(new NpgsqlParameter("caseid", caseId));
                param.Add(new NpgsqlParameter("isleader", string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? 0 : Convert.ToInt32(GetRuleConfigInfo("isleader", flowNodeInfo))));
                param.Add(new NpgsqlParameter("casecreator", caseInfo.RecCreator));

                result = ExecuteQuery<ApproverInfo>(cmdText, param.ToArray(), trans);
            }
            return result;

        }

        public List<ApproverInfo> GetFlowNodeCPUser(Guid caseId, Guid nodeid, int userNumber, WorkFlowType flowtype, DbTransaction trans = null)
        {
            string cmdText = null;
            var result = new List<ApproverInfo>();
            if (flowtype == WorkFlowType.FreeFlow)//自由流程
            {
                cmdText = @"SELECT u.userid,u.username,u.usericon,u.namepinyin,ur.deptid,d.deptname 
                            FROM crm_sys_userinfo AS u
			                LEFT JOIN crm_sys_account_userinfo_relate AS ur ON u.userid = ur.userid AND ur.recstatus = 1
			                LEFT JOIN crm_sys_department AS d ON ur.deptid = d.deptid
			                WHERE u.recstatus = 1";

                result = ExecuteQuery<ApproverInfo>(cmdText, null, trans);

            }
            else
            {
                cmdText = @"SELECT u.userid,u.username,u.usericon,u.namepinyin,ur.deptid,d.deptname FROM crm_sys_userinfo AS u
                           LEFT JOIN crm_sys_account_userinfo_relate AS ur ON u.userid = ur.userid AND ur.recstatus = 1
                           LEFT JOIN crm_sys_department AS d ON ur.deptid = d.deptid
                           WHERE u.recstatus = 1";


                var caseInfo = GetWorkFlowCaseInfo(trans, caseId);
                if (caseInfo == null)
                    throw new Exception("未找到相关联的流程");
                var flowNodeInfo = GetWorkFlowNodeInfo(trans, nodeid);
                if (flowNodeInfo == null)
                    throw new Exception("未找到相关联的流程节点");

                List<DbParameter> param = new List<DbParameter>();

                switch (flowNodeInfo.StepCPTypeId)
                {
                    case NodeStepType.CPUser:// 2:指定审批人, 
                        cmdText += @" AND u.userid IN (
													SELECT userid::INT from (
																SELECT UNNEST( string_to_array(jsonb_extract_path_text(LOWER(@ruleconfig::TEXT)::jsonb,'cpuserid'), ',')) AS userid 
													) as r WHERE r.userid!=''
                                       )";
                        break;
                    #region 指定函数
                    case NodeStepType.Function:
                        JToken funcname = GetRuleConfigObject("cpfuncname", flowNodeInfo);
                        param.Add(new NpgsqlParameter("ruleconfig", flowNodeInfo.RuleConfig) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });
                        param.Add(new NpgsqlParameter("userno", userNumber));
                        param.Add(new NpgsqlParameter("recid", caseInfo.RecId));
                        param.Add(new NpgsqlParameter("relrecid", caseInfo.RelRecId));
                        param.Add(new NpgsqlParameter("caseid", caseId));
                        result = ExecuteQuery<ApproverInfo>("select * from " + funcname.ToString() + "(@caseid,@ruleconfig,@recid,@relrecid,@userno);", param.ToArray(), trans);
                        return result;
                        break;

                    #endregion
                    default:
                        cmdText += " AND 1=2";
                        break;
                }
                param.Add(new NpgsqlParameter("ruleconfig", flowNodeInfo.RuleConfig) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });
                param.Add(new NpgsqlParameter("userno", userNumber));
                param.Add(new NpgsqlParameter("recid", caseInfo.RecId));
                param.Add(new NpgsqlParameter("relrecid", caseInfo.RelRecId));
                param.Add(new NpgsqlParameter("caseid", caseId));
                param.Add(new NpgsqlParameter("casecreator", caseInfo.RecCreator));
                param.Add(new NpgsqlParameter("isleader", string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? 0 : Convert.ToInt32(GetRuleConfigInfo("isleader", flowNodeInfo))));
                result = ExecuteQuery<ApproverInfo>(cmdText, param.ToArray(), trans);
            }
            return result;

        }
        private void GetEntityField(DbTransaction trans, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo flowNodeInfo, out string fieldname, out string entityTableName)
        {
            var ruleConfig = JObject.Parse(flowNodeInfo.RuleConfig.ToString());
            fieldname = ruleConfig["fieldname"] != null ? ruleConfig["fieldname"].ToString() : null;
            var workflowInfo = GetWorkFlowInfo(trans, caseInfo.FlowId);
            var entitySql = @"SELECT entitytable FROM crm_sys_entity WHERE entityid = @entityid LIMIT 1";
            var entityparam = new DbParameter[] { new NpgsqlParameter("entityid", workflowInfo.Entityid) };
            object entitytable = ExecuteScalar(entitySql, entityparam, trans);

            entityTableName = entitytable == null ? null : entitytable.ToString();
            if (entityTableName == null)
                throw new Exception("流程关联实体表不存在");
        }
        private void GetEntityFieldBasicRuleConfig(DbTransaction trans, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo flowNodeInfo, out string fieldname, out string entityTableName)
        {
            var ruleConfig = JObject.Parse(flowNodeInfo.RuleConfig.ToString());
            fieldname = ruleConfig["fieldname"] != null ? ruleConfig["fieldname"].ToString() : null;
            if (String.IsNullOrEmpty(fieldname))
            {
                fieldname = ruleConfig["fieldtem"] != null ? ruleConfig["fieldtem"].ToString() : null;
            }
            string entityId = ruleConfig["entityid"] != null ? ruleConfig["entityid"].ToString() : null;
            var entitySql = @"SELECT entitytable FROM crm_sys_entity WHERE entityid = @entityid LIMIT 1";
            var entityparam = new DbParameter[] { new NpgsqlParameter("entityid", Guid.Parse(entityId)) };
            object entitytable = ExecuteScalar(entitySql, entityparam, trans);

            entityTableName = entitytable == null ? null : entitytable.ToString();
            if (entityTableName == null)
                throw new Exception("实体表不存在");
        }
        private string GetRuleConfigInfo(string key, WorkFlowNodeInfo flowNodeInfo)
        {
            var ruleConfig = JObject.Parse(flowNodeInfo.RuleConfig.ToString());
            return ruleConfig[key] != null ? ruleConfig[key].ToString() : null;
        }
        private JToken GetRuleConfigObject(string key, WorkFlowNodeInfo flowNodeInfo)
        {
            var ruleConfig = JObject.Parse(flowNodeInfo.RuleConfig.ToString());
            return ruleConfig[key] != null ? ruleConfig[key] : null;
        }
        #endregion


        /// <summary>
        /// 获取流程下一节点连线上的ruleid
        /// </summary>
        /// <param name="flowid"></param>
        /// <param name="endnode"></param>
        /// <param name="vernum"></param>
        /// <returns></returns>
        public Guid GetNextNodeRuleId(Guid flowid, Guid fromnodeid, Guid tonodeid, int vernum, DbTransaction trans = null)
        {
            var executeSql = @" SELECT ruleid FROM crm_sys_workflow_node_line WHERE flowid=@flowid AND fromnodeid=@fromnodeid AND tonodeid=@tonodeid AND vernum=@vernum LIMIT 1;";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("flowid", flowid),
                new NpgsqlParameter("fromnodeid", fromnodeid),
                new NpgsqlParameter("tonodeid", tonodeid),
                new NpgsqlParameter("vernum", vernum),
            };
            object obj = ExecuteScalar(executeSql, param, trans);
            Guid ruleid;
            if (obj != null)
            {
                Guid.TryParse(obj.ToString(), out ruleid);
            }

            return ruleid;
        }
        /// <summary>
        /// 获取流程当前节点id
        /// </summary>
        /// <param name="flowid"></param>
        /// <param name="endnode"></param>
        /// <param name="vernum"></param>
        /// <returns></returns>
        public int GetNowNodeId(Guid flowid, int nextNodeId, int vernum, DbTransaction trans = null)
        {
            var executeSql = @" SELECT fromnode FROM crm_sys_workflow_node_line WHERE flowid=@flowid AND endnode=@endnode AND vernum=@vernum LIMIT 1;";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("flowid", flowid),
                new NpgsqlParameter("endnode", nextNodeId),
                new NpgsqlParameter("vernum", vernum),
            };
            object obj = ExecuteScalar(executeSql, param, trans);
            int nodeid = -1;
            if (obj != null)
            {
                int.TryParse(obj.ToString(), out nodeid);
            }

            return nodeid;
        }



        public bool ValidateNextNodeRule(WorkFlowCaseInfo caseInfo, string ruleSql, int userNumber, DbTransaction trans = null)
        {
            #region --获取实体表名--
            var entitySql = @"SELECT (SELECT entitytable FROM crm_sys_entity WHERE entityid=@entityid) AS entitytable,
                                     (SELECT entitytable FROM crm_sys_entity WHERE entityid=@relentityid) AS relentitytable";
            var entitySqlParameters = new List<DbParameter>();
            entitySqlParameters.Add(new NpgsqlParameter("entityid", caseInfo.EntityId));
            entitySqlParameters.Add(new NpgsqlParameter("relentityid", caseInfo.RelEntityId));
            var entitytableResult = ExecuteQuery(entitySql, entitySqlParameters.ToArray(), trans).FirstOrDefault();

            if (entitytableResult == null || entitytableResult.Count == 0 || entitytableResult["entitytable"] == null)
            {
                throw new Exception("该实体不存在有效的业务表");
            }
            string entityTableName = entitytableResult["entitytable"].ToString();
            string relEntityTableName = entitytableResult["relentitytable"] == null ? null : entitytableResult["relentitytable"].ToString();
            #endregion

            #region --获取rel实体的查询字段和left join语句
            string relentitySql = string.Empty;
            string relFieldSql = string.Empty;
            if (!string.IsNullOrEmpty(relEntityTableName))
            {
                var relentityFieldSql = @"  SELECT array_to_string(ARRAY(
                                            SELECT 'rel.'||ef.fieldname ||' AS rel'||ef.fieldname
                                            FROM pg_attribute AS a 
                                            LEFT JOIN pg_attrdef f ON f.adrelid = a.attrelid  AND f.adnum = a.attnum
                                            INNER JOIN crm_sys_entity_fields ef ON ef.fieldname=a.attname 
                                            WHERE  a.attrelid = @relEntityTableName::regclass	AND ef.entityid=@entityid AND ef.recstatus=1),',')";
                var relfieldParameters = new List<DbParameter>();
                relfieldParameters.Add(new NpgsqlParameter("relEntityTableName", relEntityTableName));
                relfieldParameters.Add(new NpgsqlParameter("entityid", caseInfo.RelEntityId));


                var relfieldResult = ExecuteScalar(relentityFieldSql, relfieldParameters.ToArray(), trans);
                if (relfieldResult != null)
                    relFieldSql = relfieldResult.ToString() + ",";
                relentitySql = string.Format(@" LEFT JOIN {0} AS rel ON rel.recid=cer.relrecid", relEntityTableName);
            }
            #endregion

            string whereSql = string.IsNullOrEmpty(ruleSql) ? "1=1" : ruleSql;


            string detailSql = string.Format(@"SELECT c.caseid,c.flowid,c.auditstatus,c.vernum,c.reccode AS flowreccode,c.nodenum,c.reccreated AS flowcasecreated,
                                                      c.recupdated AS flowupdated, c.recupdator AS flowupdator, 
                                                      e.*,
                                                      {0}
                                                      c.reccreator AS flowluancher,
                                                      aur.deptid AS flowluancherdeptid,
                                                      d.pdeptid AS flowluancherpredeptid,
                                                      urr.roleid AS flowluancherroleid,
                                                      u.isleader AS flowluancherisleader
                                               FROM crm_sys_workflow_case AS c
                                               LEFT JOIN crm_sys_account_userinfo_relate AS aur ON aur.userid=c.reccreator
                                               LEFT JOIN crm_sys_department AS d ON d.deptid=aur.deptid
                                               LEFT JOIN crm_sys_userinfo AS u ON u.userid=c.reccreator
                                               LEFT JOIN crm_sys_userinfo_role_relate AS urr ON urr.userid=c.reccreator
                                               LEFT JOIN crm_sys_workflow_case_entity_relation AS cer ON cer.caseid=c.caseid and cer.relentityid=@relentityid
                                               LEFT JOIN {1} AS e ON e.recid=c.recid                                               
                                               {2}
                                               WHERE c.caseid=@caseid AND c.recid = @recid AND c.recstatus = 1
                                               ", relFieldSql, entityTableName, relentitySql);

            string sql = string.Format(@"SELECT COUNT(1) FROM ({0}) AS e  
                                         WHERE  {1}", detailSql, whereSql);

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("recid", caseInfo.RecId));
            sqlParameters.Add(new NpgsqlParameter("caseid", caseInfo.CaseId));
            sqlParameters.Add(new NpgsqlParameter("relentityid", caseInfo.RelEntityId));

            object result = null;
            result = ExecuteScalar(sql, sqlParameters.ToArray(), trans);
            int isAccess = 0;
            if (result != null)
                int.TryParse(result.ToString(), out isAccess);
            return isAccess > 0;
        }


        /// <summary>
        /// 退回流程节点
        /// </summary>
        public bool RebackWorkFlowCaseItem(WorkFlowCaseInfo caseinfo, WorkFlowCaseItemInfo caseitem, int userNumber, DbTransaction trans = null)
        {
            var nodeidSql = @"SELECT nodeid FROM crm_sys_workflow_node WHERE flowid=@flowid AND vernum=@vernum AND steptypeid=0";
            var entitySqlParameters = new List<DbParameter>();
            entitySqlParameters.Add(new NpgsqlParameter("flowid", caseinfo.FlowId));
            entitySqlParameters.Add(new NpgsqlParameter("vernum", caseinfo.VerNum));
            var nodeidResult = ExecuteScalar(nodeidSql, entitySqlParameters.ToArray(), trans);
            Guid nodeid = Guid.Empty;
            if (nodeidResult != null)
                Guid.TryParse(nodeidResult.ToString(), out nodeid);
            int handleuser = caseinfo.RecCreator;
            int stepnum = caseitem.StepNum + 1;
            string sql = string.Format(@"INSERT INTO crm_sys_workflow_case_item (caseid, nodenum,choicestatus,handleuser, casestatus, reccreator, recupdator,stepnum,nodeid) 
                                         VALUES (@caseid, 0 , 4,@handleuser,  0, @userno,@userno,@stepnum,@nodeid);
                                         UPDATE crm_sys_workflow_case SET nodenum = 0,recupdator = @userno,recupdated=@recupdated WHERE caseid = @caseid;");

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("caseid", caseitem.CaseId));
            sqlParameters.Add(new NpgsqlParameter("handleuser", handleuser));
            sqlParameters.Add(new NpgsqlParameter("nodeid", nodeid));
            sqlParameters.Add(new NpgsqlParameter("userno", userNumber));
            sqlParameters.Add(new NpgsqlParameter("stepnum", stepnum));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));

            var result = ExecuteNonQuery(sql, sqlParameters.ToArray(), trans);

            return result == 2;
        }


        /// <summary>
        /// 审批流程节点
        /// </summary>
        public bool AuditWorkFlowCaseItem(WorkFlowAuditCaseItemMapper auditdata, WorkFlowCaseItemInfo caseitem, int userNumber, DbTransaction trans = null)
        {
            string sql = string.Empty;
            var sqlParameters = new List<DbParameter>();
            if (caseitem.NodeNum == 0 && caseitem.StepNum == 0)
            {
                sql = string.Format(@" UPDATE crm_sys_workflow_case_item SET choicestatus = @choicestatus,suggest = COALESCE(@suggest,''), casestatus = 2,recupdator = @userno ,recupdated=@recupdated
                                          WHERE caseitemid = @caseitemid;");
            }
            else
            {
                sql = string.Format(@" UPDATE crm_sys_workflow_case_item SET choicestatus = @choicestatus,suggest = COALESCE(@suggest,''), casedata = @casedata, casestatus = 2,recupdator = @userno ,recupdated=@recupdated WHERE caseitemid = @caseitemid;");
                sqlParameters.Add(new NpgsqlParameter("casedata", JsonConvert.SerializeObject(auditdata.CaseData)) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });
            }

            sqlParameters.Add(new NpgsqlParameter("choicestatus", auditdata.ChoiceStatus));
            sqlParameters.Add(new NpgsqlParameter("suggest", auditdata.Suggest ?? ""));
            sqlParameters.Add(new NpgsqlParameter("userno", userNumber));
            sqlParameters.Add(new NpgsqlParameter("caseitemid", caseitem.CaseItemId));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));
            var result = ExecuteNonQuery(sql, sqlParameters.ToArray(), trans);

            return result > 0;
        }

        /// <summary>
        /// 审批流程时更新流程数据
        /// </summary>
        public bool AuditWorkFlowCase(Guid caseid, AuditStatusType auditstatus, int nodenum, int userNumber, DbTransaction trans = null)
        {
            string sql = string.Format(@" UPDATE crm_sys_workflow_case SET auditstatus = @auditstatus,nodenum = @nodenum,recupdator = @userno,recupdated=@recupdated WHERE caseid = @caseid;");

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("auditstatus", (int)auditstatus));
            sqlParameters.Add(new NpgsqlParameter("nodenum", nodenum));
            sqlParameters.Add(new NpgsqlParameter("userno", userNumber));
            sqlParameters.Add(new NpgsqlParameter("caseid", caseid));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));

            var result = ExecuteNonQuery(sql, sqlParameters.ToArray(), trans);

            return result > 0;
        }


        /// <summary>
        /// 重新发起流程
        /// </summary>
        public bool ReOpenWorkFlowCase(Guid caseid, Guid caseitemid, int userNumber, DbTransaction trans = null)
        {
            string sql = string.Format(@" UPDATE crm_sys_workflow_case SET nodenum = 0,recupdator = @userno,auditstatus=3,recupdated=@recupdated WHERE caseid = @caseid;
                                          UPDATE crm_sys_workflow_case_item SET choicestatus = 4,casestatus = 1,recupdated=@recupdated WHERE caseitemid = @caseitemid;");

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("caseitemid", caseitemid));
            sqlParameters.Add(new NpgsqlParameter("caseid", caseid));
            sqlParameters.Add(new NpgsqlParameter("userno", userNumber));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));

            var result = ExecuteNonQuery(sql, sqlParameters.ToArray(), trans);

            return result > 0;
        }
        /// <summary>
        /// 审批已经到达了最后一步,添加最后节点
        /// </summary>
        public bool EndWorkFlowCaseItem(Guid caseid, Guid nodeid, int stepnum, int userNumber, DbTransaction trans = null)
        {
            string sql = string.Format(@"INSERT INTO crm_sys_workflow_case_item (caseid,nodeid, nodenum,choicestatus,handleuser,suggest, casestatus, reccreator, recupdator,stepnum) 
                                         VALUES (@caseid,@nodeid, -1, 5,@userno, '', 2, @userno,@userno,@stepnum);");

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("stepnum", stepnum));
            sqlParameters.Add(new NpgsqlParameter("caseid", caseid));
            sqlParameters.Add(new NpgsqlParameter("nodeid", nodeid));
            sqlParameters.Add(new NpgsqlParameter("userno", userNumber));


            var result = ExecuteNonQuery(sql, sqlParameters.ToArray(), trans);

            return result > 0;
        }

        #region --获取流程event函数-- +string GetWorkFlowEvent(Guid flowid, Guid nodeid, int steptype, DbTransaction trans = null) 
        /// <summary>
        /// 获取流程event函数
        /// </summary>
        /// <param name="flowid">流程id</param>
        /// <param name="nodeid">event关联的节点nodeid，固定流程的节点id，若为自由流程，则uuid值为0作为流程起点，值为1作为流程终点</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public WorkFlowEventInfo GetWorkFlowEvent(Guid flowid, Guid nodeid, DbTransaction trans = null)
        {
            string sql = string.Format(@"SELECT funcname,steptype FROM crm_sys_workflow_func_event  WHERE flowid = @flowid AND nodeid = @nodeid ");

            var sqlParameters = new List<DbParameter>();

            sqlParameters.Add(new NpgsqlParameter("flowid", flowid));
            sqlParameters.Add(new NpgsqlParameter("nodeid", nodeid));


            var result = ExecuteQuery<WorkFlowEventInfo>(sql, sqlParameters.ToArray(), trans);

            return result.FirstOrDefault();
        }
        #endregion


        /// <summary>
        /// 执行流程扩展函数
        /// </summary>
        public void ExecuteWorkFlowEvent(WorkFlowEventInfo eventInfo, Guid caseid, int nodenum, int choicestatus, int userno, DbTransaction trans = null)
        {
            if (eventInfo == null || string.IsNullOrEmpty(eventInfo.FuncName))
                return;
            string sql = string.Empty;
            var sqlParameters = new List<DbParameter>();
            if (eventInfo.StepType == 0)
            {
                sql = string.Format(@"SELECT id,flag,msg,stacks,codes FROM {0}(@caseid,@nodenum,@userno)", eventInfo.FuncName);
            }
            else
            {
                sql = string.Format(@"SELECT id,flag,msg,stacks,codes FROM {0}(@caseid,@nodenum,@choicestatus,@userno)", eventInfo.FuncName);
                sqlParameters.Add(new NpgsqlParameter("choicestatus", choicestatus));
            }

            sqlParameters.Add(new NpgsqlParameter("nodenum", nodenum));
            sqlParameters.Add(new NpgsqlParameter("caseid", caseid));
            sqlParameters.Add(new NpgsqlParameter("userno", userno));


            var result = ExecuteQuery<OperateResult>(sql, sqlParameters.ToArray(), trans).FirstOrDefault();
            if (result.Flag != 1)
            {
                throw new Exception(result.Msg);
            }
        }

        public void ExecuteUpdateWorkFlowEntity(Guid caseid, int nodenum, int userno, DbTransaction trans = null)
        {
            string sql = string.Format(@"SELECT id,flag,msg,stacks,codes FROM crm_func_workflow_case_updatefunc(@caseid,@nodenum,@userno)");

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("nodenum", nodenum));
            sqlParameters.Add(new NpgsqlParameter("caseid", caseid));
            sqlParameters.Add(new NpgsqlParameter("userno", userno));


            var result = ExecuteQuery<OperateResult>(sql, sqlParameters.ToArray(), trans).FirstOrDefault();
            if (result.Flag != 1)
            {
                throw new Exception(result.Msg);
            }
        }


        /// <summary>
        /// 添加审批节点
        /// </summary>
        /// <returns></returns>
        public bool AddCaseItem(List<WorkFlowCaseItemInfo> caseitems, int userno, AuditStatusType auditstatus = AuditStatusType.Approving, DbTransaction trans = null)
        {
            if (caseitems == null || caseitems.Count == 0)
                throw new Exception("审批节点数据不可为空");
            var caseid = caseitems.FirstOrDefault().CaseId;
            var nodenum = caseitems.FirstOrDefault().NodeNum;

            var check_sql = @"SELECT 1 FROM crm_sys_workflow_case_item WHERE caseid = @caseid AND nodenum = @nodenum AND (casestatus = 0 OR casestatus = 1) ORDER BY stepnum DESC LIMIT 1;";
            var checkParameters = new List<DbParameter>();
            checkParameters.Add(new NpgsqlParameter("caseid", caseid));
            checkParameters.Add(new NpgsqlParameter("nodenum", nodenum));
            var chekResult = ExecuteScalar(check_sql, checkParameters.ToArray(), trans);
            if (chekResult != null && int.Parse(chekResult.ToString()) == 1)
            {
                throw new Exception("流程步骤不能重复提交");
            }

            #region 临时处理默认抄送问题,读取抄送规则,这是临时规则，在2018年8月-9月会更改为正式的规则
            string AdditionCopyUsers = "";
            try
            {
                string tmpsql = "select flowid from crm_sys_workflow_case where caseid = @caseid";
                Guid flowid = (Guid)ExecuteScalar(tmpsql, new DbParameter[] { new Npgsql.NpgsqlParameter("@caseid", caseid) }, trans);
                var config = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("workflowcopyusers.json")
                  .Build();
                IConfigurationSection it2 = config.GetSection(flowid.ToString());
                if (it2 != null && it2.Value != null)
                {
                    AdditionCopyUsers = it2.Value.ToString();
                }
            }
            catch (Exception ex)
            {
                AdditionCopyUsers = "";
            }
            #endregion
            string sql = string.Format(@"INSERT INTO crm_sys_workflow_case_item (caseitemid,caseid,nodeid, nodenum,stepnum,choicestatus,suggest, casestatus,casedata,remark,handleuser,copyuser, reccreator, recupdator) 
                                         VALUES (@caseitemid,@caseid,@nodeid, @nodenum,@stepnum,@choicestatus,@suggest, @casestatus,@casedata,@remark,@handleuser,@copyuser, @userno, @userno);");
            List<DbParameter[]> sqlParameters = new List<DbParameter[]>();

            foreach (var item in caseitems)
            {
                #region 临时处理默认抄送问题
                if (AdditionCopyUsers.Length > 0)
                {
                    if (item.CopyUser == null) item.CopyUser = AdditionCopyUsers;
                    else item.CopyUser = item.CopyUser + "," + AdditionCopyUsers;
                    item.CopyUser = string.Join(',', item.CopyUser.Split(',').Distinct());
                }
                #endregion
                var temparm = new List<DbParameter>();
                temparm.Add(new NpgsqlParameter("caseitemid", item.CaseItemId));
                temparm.Add(new NpgsqlParameter("caseid", item.CaseId));
                temparm.Add(new NpgsqlParameter("nodeid", item.NodeId));
                temparm.Add(new NpgsqlParameter("nodenum", item.NodeNum));
                temparm.Add(new NpgsqlParameter("stepnum", item.StepNum));
                temparm.Add(new NpgsqlParameter("choicestatus", (int)item.ChoiceStatus));
                temparm.Add(new NpgsqlParameter("suggest", item.Suggest ?? ""));
                temparm.Add(new NpgsqlParameter("casestatus", (int)item.CaseStatus));
                temparm.Add(new NpgsqlParameter("casedata", JsonConvert.SerializeObject(item.Casedata)) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });
                temparm.Add(new NpgsqlParameter("remark", item.Remark ?? ""));
                temparm.Add(new NpgsqlParameter("handleuser", item.HandleUser));
                temparm.Add(new NpgsqlParameter("copyuser", item.CopyUser ?? ""));
                temparm.Add(new NpgsqlParameter("userno", userno));
                sqlParameters.Add(temparm.ToArray());
            }
            ExecuteNonQueryMultiple(sql, sqlParameters, trans);

            var case_sql = @"UPDATE crm_sys_workflow_case SET auditstatus=@auditstatus, nodenum = @nodenum,recupdator = @userno WHERE caseid = @caseid;";
            var caseParameters = new List<DbParameter>();
            caseParameters.Add(new NpgsqlParameter("caseid", caseid));
            caseParameters.Add(new NpgsqlParameter("nodenum", nodenum));
            caseParameters.Add(new NpgsqlParameter("userno", userno));
            caseParameters.Add(new NpgsqlParameter("auditstatus", (int)auditstatus));
            var result = ExecuteNonQuery(case_sql, caseParameters.ToArray(), trans);

            return result > 0;
        }


        /// <summary>
        /// 判断是否允许编辑审批数据
        /// </summary>
        /// <returns></returns>
        public bool CanEditWorkFlowCase(WorkFlowInfo workflow, int userno, DbTransaction trans = null)
        {
            string sql = string.Format(@" SELECT e.modeltype,re.modeltype AS relmodeltype FROM crm_sys_entity AS e
                                          LEFT JOIN crm_sys_entity AS re ON re.entityid=e.relentityid
                                          WHERE e.entityid= @entityid;");

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("entityid", workflow.Entityid));

            var result = ExecuteQuery(sql, sqlParameters.ToArray(), trans).FirstOrDefault();
            //modeltype IS '实体模型类型0独立实体1嵌套实体2简单(应用)实体3动态实体'
            //如果审批关联的实体为简单实体且简单实体无关联独立实体时，则允许编辑审批信息重新提交
            //如果审批关联的实体为独立实体或关联的简单实体有关联的独立实体时，则不允许编辑审批信息
            var modeltypeobj = result["modeltype"];
            var relmodeltypeobj = result["relmodeltype"];
            int modeltype = -1;

            if (modeltypeobj != null && int.TryParse(modeltypeobj.ToString(), out modeltype))
            {
                int relmodeltype = -1;
                //现在的版本也开放独立实体可以编辑
                if ((modeltype == 0 || modeltype == 2) && (relmodeltypeobj == null || (int.TryParse(relmodeltypeobj.ToString(), out relmodeltype) && relmodeltype != 0)))
                {
                    return true;
                }
            }

            return false;
        }

        public Guid getWorkflowRuleId(Guid flowId, int userId, DbTransaction tran)
        {
            try
            {
                string strSQL = string.Format("select ruleid  from crm_sys_workflow_rule_relation where flowid =  '{0}'", flowId.ToString());
                Dictionary<string, object> item = ExecuteQuery(strSQL, new DbParameter[] { }, tran).FirstOrDefault();
                if (item != null) return (Guid)item["ruleid"];
            }
            catch (Exception ex)
            {
            }
            return Guid.Empty;
        }

        public void SaveWorkflowRuleRelation(string id, Guid workflowId, int userId, DbTransaction tran)
        {
            try
            {
                string sql = string.Format(@"update crm_sys_workflow_rule_relation set ruleid='{0}' where flowid='{1}' ", id, workflowId);
                ExecuteNonQuery(sql, new DbParameter[] { }, tran);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 获取流程审批的抄送人
        /// </summary>
        /// <param name="caseid"></param>
        /// <returns></returns>
        public List<DomainModel.Account.UserInfo> GetWorkFlowCopyUser(Guid caseid, DbTransaction trans = null)
        {

            string sql = string.Format(@" SELECT ci.copyuser FROM crm_sys_workflow_case_item ci
                                          WHERE ci.caseid=@caseid AND stepnum>= (SELECT MAX( stepnum) FROM crm_sys_workflow_case_item WHERE caseid=@caseid AND nodenum=0) 
                                          ");

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("caseid", caseid));

            var result = ExecuteQuery(sql, sqlParameters.ToArray(), trans);
            List<int> copyuserlist = new List<int>();
            foreach (var row in result)
            {
                string copyusertext = row["copyuser"] != null ? row["copyuser"].ToString() : null;

                if (!string.IsNullOrEmpty(copyusertext))
                {
                    var temps = copyusertext.Split(',');

                    foreach (var tem in temps)
                    {
                        int copyuser = 0;
                        if (int.TryParse(tem, out copyuser))
                        {
                            copyuserlist.Add(copyuser);
                        }
                    }
                }

            }

            var usersql = @"SELECT userid, username,namepinyin,usericon,usersex FROM crm_sys_userinfo WHERE userid =ANY(@userids)";

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("userids", copyuserlist.Distinct().ToArray()),
                    };
            return ExecuteQuery<DomainModel.Account.UserInfo>(usersql, param);

        }

        public void SetWorkFlowCaseItemReaded(DbTransaction trans, Guid caseid, int nodenum, int userNumber, int stepnum = -1)
        {
            try
            {
                string executeSql = string.Empty;
                if (stepnum <= 0)
                {
                    executeSql = @" update crm_sys_workflow_case_item set casestatus=1 where caseid=@caseid and nodenum=@nodenum and  handleuser=@handleuser and casestatus=0
                                and stepnum=(SELECT MAX(stepnum) FROM crm_sys_workflow_case_item WHERE recstatus=1 AND nodenum=@nodenum AND caseid=@caseid )";
                }
                else
                {
                    executeSql = @" update crm_sys_workflow_case_item set casestatus=1 where caseid=@caseid and nodenum=@nodenum and stepnum=@stepnum and  handleuser=@handleuser and casestatus=0";
                }

                ExecuteNonQuery(executeSql, new DbParameter[]
                {
                    new NpgsqlParameter("caseid", caseid),
                    new NpgsqlParameter("nodenum", nodenum),
                    new NpgsqlParameter("stepnum", stepnum),
                    new NpgsqlParameter("handleuser", userNumber),
                }, trans);
            }
            catch (Exception ex)
            {
            }
        }

        public string SaveTitleConfig(Guid flowId, string titleConfig, int userId)
        {
            try
            {
                string strSQL = "update crm_sys_workflow set titleconfig = @titleconfig where flowid = @flowid ";
                DbParameter[] p = new DbParameter[] {
                    new NpgsqlParameter("@titleconfig",titleConfig),
                    new NpgsqlParameter("@flowid",flowId)
                };
                ExecuteNonQuery(strSQL, p, null);
                return titleConfig;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void TerminateCase(DbTransaction tran, Guid caseId)
        {
            try
            {
                string strSQL = @"UPDATE crm_sys_workflow_case 
                        SET auditstatus = 2,
                            recupdated = now()
                        WHERE caseid = @caseid; ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@caseid",caseId)
                };
                ExecuteNonQuery(strSQL, p, tran);
                strSQL = @"UPDATE crm_sys_workflow_case_item 
			                SET choicestatus = 3,
			                    casestatus=2,
		                            suggest='超时自动中止',
			                    recupdated=now()
			                WHERE caseid =@caseid  and (choicestatus=6 or choicestatus = 4)";
                ExecuteNonQuery(strSQL, p, tran);
            }
            catch (Exception ex)
            {
            }
        }

        public List<WorkFlowCaseItemInfo> GetWorkflowCaseWaitingDealItems(DbTransaction tran, Guid caseId)
        {
            try
            {
                string strSQL = @"select * from  crm_sys_workflow_case_item 
                            where caseid = @caseid and choicestatus = 6";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@caseid",caseId)
                };
                return ExecuteQuery<WorkFlowCaseItemInfo>(strSQL, p, tran);
            }
            catch (Exception ex)
            {
            }
            return new List<WorkFlowCaseItemInfo>();
        }

        public List<WorkFlowCaseInfo> GetExpiredWorkflowCaseList(DbTransaction tran, int userId)
        {
            try
            {
                string strSQL = @"SELECT wc.*
		FROM crm_sys_workflow AS w
		INNER JOIN crm_sys_workflow_case AS wc ON w.flowid=wc.flowid AND wc.vernum=w.vernum AND (wc.auditstatus=0 OR wc.auditstatus=3)
		INNER JOIN crm_sys_workflow_case_item AS wci ON wci.caseid=wc.caseid AND (wci.casestatus=0 OR wci.casestatus=1)
		WHERE w.expireday>0 AND w.recstatus=1 AND (date_part('day',now()- wci.reccreated)>=w.expireday)";
                return ExecuteQuery<WorkFlowCaseInfo>(strSQL, new DbParameter[] { }, tran);
            }
            catch (Exception x)
            {
            }
            return new List<WorkFlowCaseInfo>();
        }

        public Dictionary<string, object> GetWorkflowByEntityId(DbTransaction tran, Guid entityId, int userId)
        {
            try
            {
                string strSQL = "select* from crm_sys_workflow  where entityid =@entityid and recstatus = 1 ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@entityid",entityId)
                };
                return ExecuteQuery(strSQL, p, tran).FirstOrDefault();

            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
