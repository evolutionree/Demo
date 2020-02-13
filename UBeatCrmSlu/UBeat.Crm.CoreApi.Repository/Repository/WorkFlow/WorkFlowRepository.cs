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
using UBeat.Crm.CoreApi.DomainModel.Department;
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

        public List<Dictionary<string, object>> CaseItemList(Guid caseId, int userNumber, int skipnode = -1, DbTransaction tran = null, string currentHost = "")
        {
            var executeSql = @" SELECT (case when (select 1 from crm_sys_workflow_case_item_transfer where caseitemid=i.caseitemid and operatetype=0 limit 1) is null then 0 else 1 end) as isallowtransfer,(case when (select 1 from crm_sys_workflow_case_item_transfer where caseitemid=i.caseitemid and operatetype=1 limit 1) is null then 0 else 1 end) as isallowsign,i.nodenum,i.stepnum::TEXT AS itemcode,i.choicestatus,isrejectnode,
                                (CASE WHEN  i.nodeid = '00000000-0000-0000-0000-000000000000' THEN '发起审批'
	                                  WHEN  i.nodeid = '00000000-0000-0000-0000-000000000001' THEN '结束审批'
	                                  WHEN  i.nodeid = '00000000-0000-0000-0000-000000000002' THEN '自选审批'
                                      ELSE n.nodename END) nodename,
                                crm_func_entity_protocol_format_workflow_casestatus(i.casestatus,i.choicestatus) AS casestatus,
                                i.suggest,i.handleuser,i.recupdated,u.username,u.usericon,n.nodetype,n.auditnum,n.auditsucc,
                                (CASE WHEN n.nodetype = 1 THEN format('当前步骤为会审,需要%s人同意才能通过',n.auditsucc) ELSE NULL END) AS tipsmsg,i.caseitemid,i.nodeid,i.reccreated,i.casestatus as itemstatus,c.vernum,i.stepnum,i.skipnode
			                        FROM crm_sys_workflow_case_item AS i
			                        LEFT JOIN crm_sys_workflow_case AS c ON i.caseid = c.caseid
			                        LEFT JOIN crm_sys_workflow_node AS n ON i.nodeid = n.nodeid 
			                        LEFT JOIN crm_sys_userinfo AS u ON u.userid = i.handleuser 
			                        WHERE i.caseid = @caseid {0}
			                        ORDER BY i.stepnum ASC";
            executeSql = string.Format(executeSql, skipnode == -1 ? " and i.skipnode!=@skipnode " : " and i.skipnode=@skipnode ");
            var param = new DbParameter[]
            {
                new NpgsqlParameter("caseid", caseId),
                new NpgsqlParameter("skipnode", skipnode)
            };

            return ExecuteQuery(executeSql, param, tran);

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
        public Dictionary<string, List<Dictionary<string, object>>> GetNodeLinesInfo(Guid flowId, int userNumber, DbTransaction trans = null, int versionNum = -1)
        {
            var vernumSql = @"SELECT vernum FROM crm_sys_workflow WHERE flowid = @flowid LIMIT 1";
            var vernumSqlParameters = new List<DbParameter>();
            vernumSqlParameters.Add(new NpgsqlParameter("flowid", flowId));
            if (versionNum == -1 || versionNum == 0)
            {
                var vernumResult = ExecuteScalar(vernumSql, vernumSqlParameters.ToArray(), trans);
                if (vernumResult != null)
                    int.TryParse(vernumResult.ToString(), out versionNum);
            }



            var executeSql = @" SELECT n.isscheduled,n.deadline,n.nodeid,n.nodename,n.auditnum,n.nodetype,n.steptypeid,n.stepcptypeid,n.ruleconfig,n.columnconfig,n.auditsucc,n.nodeconfig ,e.funcname,n.notfound
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
                new NpgsqlParameter("vernum", versionNum),
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
                            new NpgsqlParameter("deadline", node.DeadLine),
                            new NpgsqlParameter("isscheduled", node.IsScheduled),
                            new NpgsqlParameter("nodeconfig", JsonConvert.SerializeObject(node.NodeConfig)){ NpgsqlDbType= NpgsqlTypes.NpgsqlDbType.Jsonb },
                        });

                        if (!string.IsNullOrEmpty(node.NodeEvent))
                        {
                            node_eve_params.Add(new DbParameter[]
                            {
                            new NpgsqlParameter("nodeid", node.NodeId),
                            new NpgsqlParameter("flowid", nodeLineConfig.FlowId),
                            new NpgsqlParameter("funcname", node.NodeEvent),
                            new NpgsqlParameter("steptype", node.StepTypeId==0?0:1),
                            new NpgsqlParameter("ruleconfig", node.StepTypeId==0?0:1)
                            });
                        }
                        if (node.ColumnConfig.Keys.Contains("globaljs") && node.ColumnConfig.Keys.Contains("stepfieldtype") && node.ColumnConfig["globaljs"] != null && !string.IsNullOrEmpty(node.ColumnConfig["globaljs"].ToString()) && node.ColumnConfig["stepfieldtype"] != null && node.ColumnConfig["stepfieldtype"].ToString() == "3")
                        {
                            this.SaveWorkFlowGlobalEditJs(tran, new WorkFlowGlobalJsMapper
                            {
                                FlowId = nodeLineConfig.FlowId,
                                Js = node.ColumnConfig["globaljs"].ToString(),
                                NodeId = node.NodeId
                            }, userNumber);
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
                SELECT * FROM crm_func_workflow_add(@entityId,@flowName,@flowType,@backFlag,@resetFlag,@expireDay,@remark,@skipFlag, @userno,@flowlanguage::jsonb, @config::jsonb, @isallowtransfer, @isallowsign, @isneedtorepeatapprove)
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
                Config = JsonConvert.SerializeObject(flowMapper.Config),
                IsNeedToRepeatApprove = flowMapper.IsNeedToRepeatApprove,
                IsAllowTransfer = flowMapper.IsAllowTransfer,
                IsAllowSign = flowMapper.IsAllowSign
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateFlow(WorkFlowUpdateMapper flowMapper, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_workflow_update(@flowId,@flowName,@backFlag,@resetFlag,@expireDay,@remark,@skipFlag, @userno,@flowlanguage::jsonb, @config::jsonb,@isallowtransfer,@isallowsign,@isneedtorepeatapprove)
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
                Config = JsonConvert.SerializeObject(flowMapper.Config),
                IsAllowTransfer = flowMapper.IsAllowTransfer,
                IsNeedToRepeatApprove = flowMapper.IsNeedToRepeatApprove,
                IsAllowSign = flowMapper.IsAllowSign
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
            var executeSql = @"SELECT w.*,(w.config->>'entrance')::int4 as entrance,e.relentityid,u.username AS RecCreator_name FROM crm_sys_workflow  AS w
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
            var executeSql = @" SELECT c.*,w.entityid, er.relentityid,er.relrecid ,u.username AS RecCreator_Name,(select stepnum from crm_sys_workflow_case_item where caseid=c.caseid order by stepnum desc  limit 1) as stepnum
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
                executeSql = @" SELECT  wci.* ,u.username AS HandleUserName,nodeinfo.nodetype
                                FROM crm_sys_workflow_case_item AS wci
                                LEFT JOIN crm_sys_workflow_node AS nodeinfo ON nodeinfo.nodeid = wci.nodeid
                                LEFT JOIN crm_sys_userinfo AS u ON u.userid = wci.handleuser
                                WHERE wci.recstatus=1 AND wci.nodenum=@nodenum AND wci.caseid=@caseid 
                                AND wci.stepnum=(SELECT MAX(stepnum) FROM crm_sys_workflow_case_item WHERE recstatus=1 AND nodenum=@nodenum AND caseid=@caseid )";
            }
            else
            {
                executeSql = @" SELECT  wci.* ,u.username AS HandleUserName,nodeinfo.nodetype,(select originuserid from crm_sys_workflow_case_item_transfer where caseitemid=wci.caseitemid and operatetype=0 order by reccreated desc limit 1) as transferuserid,(select originuserid from crm_sys_workflow_case_item_transfer where caseitemid=wci.caseitemid and operatetype=1 order by reccreated desc limit 1) as signuserid,(select userid from crm_sys_workflow_case_item_transfer where caseitemid=wci.caseitemid order by reccreated desc limit 1) as nowuserid
                                FROM crm_sys_workflow_case_item AS wci
                                LEFT JOIN crm_sys_workflow_node AS nodeinfo ON nodeinfo.nodeid = wci.nodeid
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
                        cmdText += @"  AND ur.deptid =  (SELECT deptid FROM crm_sys_account_userinfo_relate WHERE userid = @userno AND recstatus = 1 LIMIT 1) " + (string.IsNullOrEmpty(GetRuleConfigInfo("isleader", flowNodeInfo)) ? "" : " and u.isleader=@isleader");
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
                                        if (index >= 0)
                                        {
                                            var n = l2[index] == null ? string.Empty : l2[index]["nodeid"].ToString();
                                            var userIds = l2.Where(t => t["nodeid"].ToString() == n).Select(t => t["handleuser"]).ToList();
                                            model = new QueryReportRelDetailMapper
                                            {
                                                ReportRelationId = id,
                                                UserIds = string.Join(",", userIds)
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

        public List<ApproverInfo> GetFlowNodeCPUser(Guid caseId, Guid nodeid, int userNumber, WorkFlowType flowtype, DbTransaction trans = null, int auditStatus = -10)
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
        public string GetRuleConfigInfo(string path, string json)
        {
            var ruleConfig = JObject.Parse(json);
            string[] jsonPath = path.Split("/");
            foreach (var t in jsonPath)
            {
                var j = ruleConfig[t];
                if (t != null && !string.IsNullOrEmpty(t))
                {
                    try
                    {
                        ruleConfig = JObject.Parse(j.ToString());
                    }
                    catch (Exception ex)
                    {
                        if (j == null) return string.Empty;
                        if (string.IsNullOrEmpty(j.ToString())) return string.Empty;
                        return j.ToString();
                    }
                }
                else
                    break;
            }
            return ruleConfig.ToString();
        }

        private void GetEntityFieldBasicRuleConfigPath(DbTransaction trans, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo flowNodeInfo, string path, out string fieldname, out string entityTableName)
        {
            var ruleConfig = JObject.Parse(GetRuleConfigInfo(path, flowNodeInfo.RuleConfig.ToString()));
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
            string sql = string.Format(@"update crm_sys_workflow_case_item set isrejectnode=1 where caseitemid=@caseitemid;
                 INSERT INTO crm_sys_workflow_case_item (caseid, nodenum,choicestatus,handleuser, casestatus, reccreator, recupdator,stepnum,nodeid) 
                                         VALUES (@caseid, 0 , 4,@handleuser,  0, @userno,@userno,@stepnum,@nodeid);
                                         UPDATE crm_sys_workflow_case SET nodenum = 0,recupdator = @userno,recupdated=@recupdated WHERE caseid = @caseid;");

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("caseid", caseitem.CaseId));
            sqlParameters.Add(new NpgsqlParameter("handleuser", handleuser));
            sqlParameters.Add(new NpgsqlParameter("nodeid", nodeid));
            sqlParameters.Add(new NpgsqlParameter("userno", userNumber));
            sqlParameters.Add(new NpgsqlParameter("stepnum", stepnum));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));
            sqlParameters.Add(new NpgsqlParameter("caseitemid", caseitem.CaseItemId));
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
                sql = string.Format(@" UPDATE crm_sys_workflow_case_item SET choicestatus = @choicestatus,suggest =(case when skipnode=1 then '' else COALESCE(@suggest,'') end), casedata = @casedata, casestatus = 2,recupdator = @userno ,recupdated=@recupdated WHERE caseitemid = @caseitemid;");
                sqlParameters.Add(new NpgsqlParameter("casedata", JsonConvert.SerializeObject(auditdata.CaseData)) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });

            }

            sqlParameters.Add(new NpgsqlParameter("choicestatus", auditdata.ChoiceStatus));
            sqlParameters.Add(new NpgsqlParameter("suggest", auditdata.Suggest ?? ""));
            sqlParameters.Add(new NpgsqlParameter("userno", userNumber));
            sqlParameters.Add(new NpgsqlParameter("caseitemid", caseitem.CaseItemId));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));
            var result = ExecuteNonQuery(sql, sqlParameters.ToArray(), trans);
            if (caseitem.NodeType == 2)
            {
                this.InsertSpecialJointComment(trans, new CaseItemJoint
                {
                    CaseItemid = caseitem.CaseItemId,
                    Comment = auditdata.Suggest ?? "",
                    UserId = userNumber,
                    NodeId = caseitem.NodeId,
                    CaseId = caseitem.CaseId,
                    FlowStatus = auditdata.JointStatus
                }, userNumber);
            }
            foreach (var t in auditdata.Files)// pxf 附件上传
            {
                InsertCaseItemAttach(trans, new CaseItemFileAttach { CaseItemId = caseitem.CaseItemId, FileId = t.FileId, FileName = t.FileName }, userNumber);
            }
            return result > 0;
        }

        public void AuditWorkFlowCaseData(WorkFlowAuditCaseItemMapper auditdata, WorkFlowCaseItemInfo caseitem, int userNumber, DbTransaction trans = null)
        {
            var sqlParameters = new List<DbParameter>();
            string sql = string.Format(@" UPDATE crm_sys_workflow_case_item SET casedata = @casedata WHERE caseitemid = @caseitemid;");
            sqlParameters.Add(new NpgsqlParameter("casedata", JsonConvert.SerializeObject(auditdata.CaseData)) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Jsonb });
            sqlParameters.Add(new NpgsqlParameter("caseitemid", caseitem.CaseItemId));
            ExecuteNonQuery(sql, sqlParameters.ToArray(), trans);
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
        public Object EndWorkFlowCaseItem(Guid caseid, Guid nodeid, int stepnum, int userNumber, DbTransaction trans = null)
        {
            string sql = string.Format(@"INSERT INTO crm_sys_workflow_case_item (caseid,nodeid, nodenum,choicestatus,handleuser,suggest, casestatus, reccreator, recupdator,stepnum) 
                                         VALUES (@caseid,@nodeid, -1, 5,@userno, '', 2, @userno,@userno,@stepnum) returning caseitemid;");
            string sqlStopScheduled = " update crm_sys_workflow_scheduled set isdone=1 where caseid=@caseid;";
            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("stepnum", stepnum));
            sqlParameters.Add(new NpgsqlParameter("caseid", caseid));
            sqlParameters.Add(new NpgsqlParameter("nodeid", nodeid));
            sqlParameters.Add(new NpgsqlParameter("userno", userNumber));


            var id = ExecuteScalar(sql, sqlParameters.ToArray(), trans);
            ExecuteScalar(sqlStopScheduled, sqlParameters.ToArray(), trans);
            return id;
        }
        #region pxf 特殊会审
        public List<int> GetSubscriber(Guid caseItemId, int auditStatus, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo nodeInfo, DbTransaction tran, int userId)
        {
            List<int> approveUserIds = new List<int>();
            List<int> failedUserIds = new List<int>();
            string key = (auditStatus == 1 ? "approve" : "failed");
            var value = GetRuleConfigInfo(string.Format("endnodeconfig/{0}", key), nodeInfo.RuleConfig.ToString());
            if (!string.IsNullOrEmpty(value))
            {
                var type = GetRuleConfigInfo(string.Format("endnodeconfig/{0}/type", key), nodeInfo.RuleConfig.ToString());
                switch (type)
                {
                    case "1":
                        var cpUser = GetRuleConfigInfo(string.Format("endnodeconfig/{0}/userids", key), nodeInfo.RuleConfig.ToString());
                        if (!string.IsNullOrEmpty(cpUser))
                        {
                            var userArr = cpUser.Split(",").Distinct().ToArray();
                            foreach (var t in userArr)
                            {
                                approveUserIds.Add(Convert.ToInt32(t));
                            }
                        }
                        break;
                    case "2":
                        JToken funcname = GetRuleConfigInfo(string.Format("endnodeconfig/{0}/spfuncname", key), nodeInfo.RuleConfig.ToString());
                        var param = new DbParameter[] {
                                            new NpgsqlParameter("ruleconfig", nodeInfo.RuleConfig.ToString()),
                                            new NpgsqlParameter("userno", userId),
                                            new NpgsqlParameter("recid", caseInfo.RecId),
                                            new NpgsqlParameter("relrecid", caseInfo.RelRecId),
                                            new NpgsqlParameter("caseid", caseInfo.CaseId),
                                };
                        var result = ExecuteQuery<ApproverInfo>("select * from " + funcname.ToString() + "(@caseid,@ruleconfig::jsonb,@recid,@relrecid,@userno);", param.ToArray(), tran);
                        result.ForEach(t =>
                        {
                            approveUserIds.Add(t.UserId);
                        });
                        break;
                    case "3":
                        string cmdText = "SELECT u.userid,u.username,u.usericon,u.namepinyin,ur.deptid,d.deptname   FROM crm_sys_userinfo AS u      LEFT JOIN crm_sys_account_userinfo_relate AS ur ON u.userid = ur.userid AND ur.recstatus = 1              LEFT JOIN crm_sys_department AS d ON ur.deptid = d.deptid   WHERE u.recstatus = 1";
                        JToken reportRelation = GetRuleConfigInfo(string.Format("endnodeconfig/{0}/reportrelation", key), nodeInfo.RuleConfig.ToString());
                        if (reportRelation != null)
                        {
                            var jo = JObject.Parse(reportRelation.ToString());
                            Guid id = Guid.Parse(jo["id"].ToString());
                            int relationType = Convert.ToInt32(jo["type"].ToString());
                            var reportRelationRepository = ServiceLocator.Current.GetInstance<IReportRelationRepository>();
                            QueryReportRelDetailMapper model = new QueryReportRelDetailMapper();
                            switch (relationType)
                            {
                                case 1://流程发起人
                                    var l1 = CaseItemList(caseInfo.CaseId, userId);
                                    model = new QueryReportRelDetailMapper
                                    {
                                        ReportRelationId = id,
                                        UserId = l1.Count == 0 ? caseInfo.RecCreator : Convert.ToInt32(l1.FirstOrDefault()["handleuser"].ToString())
                                    };
                                    var d1 = reportRelationRepository.GetReportRelDetail(model, tran, userId);
                                    cmdText += @" AND u.userid in (" + string.Join(",", (d1.Count == 0 ? new string[] { "-1" } : d1.Select(t => t.ReportLeader))) + ")";
                                    break;
                                case 2://上一步骤处理人
                                    var l2 = CaseItemList(caseInfo.CaseId, userId);
                                    if (l2.Count == 0)
                                    {
                                        model = new QueryReportRelDetailMapper
                                        {
                                            ReportRelationId = id,
                                            UserId = l2.Count == 0 ? caseInfo.RecCreator : Convert.ToInt32(l2.FirstOrDefault()["handleuser"].ToString())
                                        };
                                        var d2 = reportRelationRepository.GetReportRelDetail(model, tran, userId);
                                        cmdText += @" AND u.userid in (" + string.Join(",", (d2.Count == 0 ? new string[] { "-1" } : d2.Select(t => t.ReportLeader))) + ")";
                                    }
                                    else
                                    {
                                        int index = l2.Count - 1;
                                        if (index >= 0)
                                        {
                                            var n = l2[index] == null ? string.Empty : l2[index]["nodeid"].ToString();
                                            var userIds = l2.Where(t => t["nodeid"].ToString() == n).Select(t => t["handleuser"]).ToList();
                                            model = new QueryReportRelDetailMapper
                                            {
                                                ReportRelationId = id,
                                                UserIds = string.Join(",", userIds)
                                            };
                                            var d2 = reportRelationRepository.GetReportRelDetail(model, tran, userId);
                                            cmdText += @" AND u.userid in (" + string.Join(",", (d2.Count == 0 ? new string[] { "-1" } : d2.Select(t => t.ReportLeader))) + ")";
                                        }
                                    }
                                    break;
                                case 3://表单中的人员
                                    string f3, e3;
                                    GetEntityFieldBasicRuleConfigPath(tran, caseInfo, nodeInfo, string.Format("endnodeconfig/{0}", key), out f3, out e3);
                                    if (GetRuleConfigInfo("entityid", nodeInfo) == caseInfo.RelEntityId.ToString())//判断是否是主实体还是关联实体
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
                            }
                            param = new DbParameter[] {
                                         new NpgsqlParameter("recid",caseInfo.RecId)
                                    };
                            result = ExecuteQuery<ApproverInfo>(cmdText, param, tran);
                            result.ForEach(t =>
                            {
                                approveUserIds.Add(t.UserId);
                            });
                        }
                        break;
                }
            }
            return approveUserIds;
        }

        public void AddEndWorkFlowCaseItemCPUser(DbTransaction tran, Guid caseItemId, List<int> cpUserId)
        {
            foreach (var t in cpUserId)
            {
                var sql = " insert into crm_sys_workflow_case_item_receiver(caseitemid,userid) values (@caseitemid,@userid)";
                var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",caseItemId),
                new NpgsqlParameter("userid",t)
            };
                ExecuteNonQuery(sql, param, tran);
            }
        }

        #endregion

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

            string funcName = "";
            var temp = eventInfo.FuncName.IndexOf("&");
            if (temp == -1)
                funcName = eventInfo.FuncName;
            else
                funcName = eventInfo.FuncName.Substring(0, temp);

            string sql = string.Empty;
            var sqlParameters = new List<DbParameter>();
            if (eventInfo.StepType == 0)
            {
                sql = string.Format(@"SELECT id,flag,msg,stacks,codes FROM {0}(@caseid,@nodenum,@userno)", funcName);
            }
            else
            {
                sql = string.Format(@"SELECT id,flag,msg,stacks,codes FROM {0}(@caseid,@nodenum,@choicestatus,@userno)", funcName);
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
                var caseItemList = this.CaseItemList(caseid, userno, tran: trans);
                if (caseItemList != null && caseItemList.Count > 0)
                {
                    var caseItem = caseItemList[caseItemList.Count - 2];
                    if (!(caseItem["itemstatus"].ToString() == "2"))
                        throw new Exception("流程步骤不能重复提交");
                }
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
            string sql = string.Format(@"INSERT INTO crm_sys_workflow_case_item (caseitemid,caseid,nodeid, nodenum,stepnum,choicestatus,suggest, casestatus,casedata,remark,handleuser,copyuser, reccreator, recupdator,skipnode) 
                                         VALUES (@caseitemid,@caseid,@nodeid, @nodenum,@stepnum,@choicestatus,@suggest, @casestatus,@casedata,@remark,@handleuser,@copyuser, @userno, @userno, @skipnode);");
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
                temparm.Add(new NpgsqlParameter("skipnode", item.SkipNode));
                sqlParameters.Add(temparm.ToArray());
                var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",item.CaseItemId),
                new NpgsqlParameter("userid",userno),
                new NpgsqlParameter("comment",item.Suggest),
                new NpgsqlParameter("nodeid",item.NodeId),
                new NpgsqlParameter("caseid",item.CaseId)
                };
                if (item.NodeNum == 0 || item.NodeNum == -1) continue;
                var result1 = ExecuteQuery("select nodeid,deadline,isscheduled from crm_sys_workflow_node where nodeid=@nodeid limit 1", param, trans);
                var resultDetail = result1.FirstOrDefault();
                if (resultDetail != null)
                {
                    if (resultDetail["isscheduled"] != null && Convert.ToInt32(resultDetail["isscheduled"]) == 1)
                    {
                        if (ExecuteScalar("select 1 from crm_sys_workflow_scheduled where caseid=@caseid and nodeid=@nodeid limit 1", param, trans) == null)
                        {
                            param = new DbParameter[] {
                                new NpgsqlParameter("caseid",item.CaseId),
                                new NpgsqlParameter("nodeid",item.NodeId),
                                new NpgsqlParameter("pointoftime",DateTime.Now.AddHours(Convert.ToInt32(resultDetail["deadline"])))
                          };
                            ExecuteNonQuery("insert into crm_sys_workflow_scheduled(caseid,nodeid,pointoftime) values (@caseid,@nodeid,@pointoftime)", param, trans);
                        }
                    }
                }
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
                    executeSql = @" update crm_sys_workflow_case_item set casestatus=1,reccreated=now(),recupdated=now()  where caseid=@caseid and nodenum=@nodenum and  handleuser=@handleuser and casestatus=0
                                and stepnum=(SELECT MAX(stepnum) FROM crm_sys_workflow_case_item WHERE recstatus=1 AND nodenum=@nodenum AND caseid=@caseid )";
                }
                else
                {
                    executeSql = @" update crm_sys_workflow_case_item set casestatus=1,reccreated=now(),recupdated=now() where caseid=@caseid and nodenum=@nodenum and stepnum=@stepnum and  handleuser=@handleuser and casestatus=0";
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
        public OperateResult InsertSpecialJointComment(DbTransaction tran, CaseItemJoint joint, int userId)
        {
            var sqlExist = "select 1 from crm_sys_workflow_case_item_joint where caseitemid=@caseitemid;";
            var sql = " insert into crm_sys_workflow_case_item_joint(caseitemid,userid,comment,nodeid,flowstatus) values (@caseitemid,@userid,@comment,@nodeid,@flowstatus)";
            var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",joint.CaseItemid),
                new NpgsqlParameter("userid",joint.UserId),
                new NpgsqlParameter("comment",joint.Comment),
                new NpgsqlParameter("nodeid",joint.NodeId),
                new NpgsqlParameter("caseid",joint.CaseId),
                new NpgsqlParameter("flowstatus",joint.FlowStatus)
            };
            int result = 0;
            if (ExecuteScalar(sqlExist, param, tran) == null)
            {
                result = ExecuteNonQuery(sql, param, tran);
                var result1 = ExecuteQuery("select nodeid,deadline,isscheduled from crm_sys_workflow_node where nodeid=@nodeid limit 1", param, tran);
                var resultDetail = result1.FirstOrDefault();
                if (resultDetail != null)
                {
                    if (resultDetail["isscheduled"] != null && Convert.ToInt32(resultDetail["isscheduled"]) == 1)
                    {
                        if (ExecuteScalar("select 1 from crm_sys_workflow_scheduled where caseid=@caseid and nodeid=@nodeid limit 1", param, tran) == null)
                        {
                            param = new DbParameter[] {
                                new NpgsqlParameter("caseid",joint.CaseId),
                                new NpgsqlParameter("nodeid",joint.NodeId),
                                new NpgsqlParameter("pointoftime",DateTime.Now.AddHours(Convert.ToInt32(resultDetail["deadline"])))
                          };
                            ExecuteNonQuery("insert into crm_sys_workflow_scheduled(caseid,nodeid,pointoftime) values (@caseid,@nodeid,@pointoftime)", param, tran);
                        }
                    }
                }
            }
            if (result > 0)
                return new OperateResult
                {
                    Flag = 1
                };
            return new OperateResult
            {

            };
        }

        public void UpdateWorkFlowNodeScheduled(DbTransaction trans, WorkFlowNodeScheduled scheduled, int userId)
        {
            var sql = " update crm_sys_workflow_scheduled set isdone=1 where caseid=@caseid and nodeid=@nodeid;";
            List<string> a = new List<string>();
            a.Select(t => t.Split("")).ToList();
            var param = new DbParameter[] {
                new NpgsqlParameter("nodeid",scheduled.NodeId),
                new NpgsqlParameter("caseid",scheduled.CaseId)
            };
            ExecuteNonQuery(sql, param, trans);
        }
        public List<WorkFlowNodeScheduledList> GetWorkFlowNodeScheduled(DbTransaction trans, int userId)
        {
            var sql = " select * from crm_sys_workflow_scheduled where isdone=0;";
            return ExecuteQuery<WorkFlowNodeScheduledList>(sql, null, trans);
        }
        public List<Dictionary<string, object>> GetSpecialJointCommentDetail(DbTransaction tran, Guid caseItemId, int userId, string currentHost = "")
        {
            var _config = ServiceLocator.Current.GetInstance<Microsoft.Extensions.Configuration.IConfigurationRoot>().GetSection("FileServiceSetting");
            var lngfFilePlugin = _config.GetValue<string>("LngfFilePlugin");
            string fileUrl = string.Format(lngfFilePlugin, currentHost);
            var sql = "select tmp.nodeid,(case when caseitem1.casestatus=2 then caseitem1.recupdated else tmp.reccreated end ) as reccreated,tmp.caseitemid,tmp.comment,tmp.flowstatus,tmp.userid,att1.filejson,  tmp.username  from (\n" +
" select jt.nodeid,jt.reccreated,jt.caseitemid,jt.comment,case when jt.flowstatus=0 then '有意见' else '无意见' end as flowstatus,jt.userid,u.username,n.nodetype from \n" +
"crm_sys_workflow_case_item_joint jt LEFT JOIN crm_sys_workflow_case_attach \n" +
"as att on att.caseitemid = jt.caseitemid   LEFT JOIN crm_sys_workflow_node AS n ON jt.nodeid = n.nodeid  left join crm_sys_userinfo as u on u.userid = jt.userid where jt.caseitemid\n" +
" =@caseitemid \n" +
"group by jt.comment,jt.flowstatus,jt.caseitemid,jt.reccreated,u.username,jt.nodeid,n.nodetype,jt.userid  ORDER BY jt.reccreated ) as tmp\n" +
"LEFT JOIN ( \n" +
"select array_to_json(array_agg(row_to_json(t))) as filejson,t.caseitemid \n" +
"from ( \n" +
"  select  '' as fileurl,fileid, filename,caseitemid,recid FROM crm_sys_workflow_case_attach    where caseitemid =@caseitemid  \n" +
") as t GROUP BY t.caseitemid,recid \n" +
") as att1 on att1.caseitemid=tmp.caseitemid \n" +
" inner join crm_sys_workflow_case_item as caseitem1 on caseitem1.caseitemid=tmp.caseitemid " +
"LEFT JOIN crm_sys_workflow_node AS n ON tmp.nodeid = n.nodeid";

            var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",caseItemId)
            };
            var result = ExecuteQuery(sql, param, tran);
            result.ForEach(t =>
            {
                if (t["filejson"] != null)
                {
                    var fileJson = t["filejson"].ToString();
                    var files = JArray.Parse(fileJson);
                    foreach (var t1 in files)
                    {
                        var newFile = t1["fileid"] == null ? string.Empty : (fileUrl + t1["fileid"]);
                        t1["fileurl"] = newFile;
                    }
                    t["filejson"] = files.ToString();
                }
            });
            return result;
        }

        public OperateResult InsertTransfer(DbTransaction tran, CaseItemJointTransfer transfer, int userId)
        {
            // var sqlExist = " select count(1) from crm_sys_workflow_case_item_transfer where caseitemid=@caseitemid";
            var sql = " insert into crm_sys_workflow_case_item_transfer(caseitemid,originuserid,userid,comment,operatetype,flowstatus,signstatus) values (@caseitemid,@originuserid,@userid,@comment,@operatetype,@flowstatus,@signstatus) returning recid";
            var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",transfer.CaseItemid),
                new NpgsqlParameter("originuserid",transfer.OrginUserId),
                new NpgsqlParameter("userid",transfer.UserId),
                new NpgsqlParameter("comment",transfer.Comment),
                new NpgsqlParameter("operatetype",transfer.IsSignOrTransfer),
                new NpgsqlParameter("flowstatus",transfer.FlowStatus==4?new Nullable<int>():transfer.FlowStatus),
                               new NpgsqlParameter("signstatus",transfer.SignStatus)
            };
            //var count = ExecuteScalar(sqlExist, param, tran);
            //int num = 0;
            //if (count != null)
            //    num = Convert.ToInt32(count);
            //if (num == 1)
            //{
            //    ExecuteQuery(" update crm_sys_workflow_case_item_transfer set flowstatus=7 where  recid=(select recid from crm_sys_workflow_case_item_transfer where caseitemid=@caseitemid order by reccreated desc limit 1) ;", param, tran);
            //}
            var result = ExecuteScalar(sql, param, tran);
            if (result != null)
                return new OperateResult
                {
                    Flag = 1,
                    Id = result.ToString()
                };
            return new OperateResult
            {

            };
        }
        public OperateResult InsertCaseItemAttach(DbTransaction tran, CaseItemFileAttach attach, int userId)
        {
            var existsSql = " select 1 from crm_sys_workflow_case_attach where caseitemid=@caseitemid and fileid=@fileid;";
            var sql = " insert into crm_sys_workflow_case_attach(caseitemid,fileid,filename,recid,nodeid) values (@caseitemid,@fileid,@filename,@recid,(select nodeid from crm_sys_workflow_case_item where caseitemid=@caseitemid limit 1))";
            var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",attach.CaseItemId),
                new NpgsqlParameter("fileid",attach.FileId),
                new NpgsqlParameter("filename",attach.FileName),
                new NpgsqlParameter("recid",attach.RecId),
                new NpgsqlParameter("userno",userId)
            };
            if (ExecuteScalar(existsSql, param, tran) == null)
            {
                var result = ExecuteNonQuery(sql, param, tran);
                if (result > 0)
                    return new OperateResult
                    {
                        Flag = 1
                    };
            }
            return new OperateResult
            {

            };
        }

        public List<Dictionary<string, object>> GetWorkFlowCaseTransferAtt(DbTransaction tran, Guid caseItemId, int userId, string currentHost = "")
        {
            var _config = ServiceLocator.Current.GetInstance<Microsoft.Extensions.Configuration.IConfigurationRoot>().GetSection("FileServiceSetting");
            var lngfFilePlugin = _config.GetValue<string>("LngfFilePlugin");
            string fileUrl = string.Format(lngfFilePlugin, currentHost);
            var sql = "select (select nodeid from crm_sys_workflow_case_item where caseitemid=tmp.caseitemid limit 1) as nodeid,tmp.caseitemid,\n" +
"tmp.comment,tmp.flowstatus,tmp.recid,tmp.reccreated,att1.filejson,  tmp.username \n" +
" \n" +
" from (select tf.caseitemid,tf.comment, (case when operatetype=0 then '转办' when operatetype=1 then (case when flowstatus=7 then '同意加签' when flowstatus=17 then '同意' when flowstatus=27 then '不同意' else '未知' end) else '' end)  as flowstatus,u1.username,att.recid,tf.reccreated,tf.originuserid,n.nodetype\n" +
"from crm_sys_workflow_case_item_transfer tf LEFT JOIN crm_sys_workflow_case_attach as att on att.recid = tf.recid  \n" +
" left join crm_sys_userinfo as u1 on u1.userid = tf.originuserid\n" +
"left join crm_sys_workflow_case_item item on item.caseitemid=tf.caseitemid\n" +
"LEFT JOIN crm_sys_workflow_node n on n.nodeid=item.nodeid\n" +
" where tf.caseitemid =@caseitemid  and (tf.flowstatus=7 or tf.flowstatus=17 or tf.flowstatus=27) \n" +
" group by tf.comment,tf.flowstatus,tf.caseitemid,tf.reccreated,att.recid,u1.username,operatetype,n.nodetype,tf.originuserid ) as tmp\n" +
"LEFT JOIN (\n" +
"select array_to_json(array_agg(row_to_json(t))) as filejson,t.caseitemid,t.recid\n" +
"from (\n" +
"  select  '' as fileurl,fileid, filename,caseitemid,recid FROM crm_sys_workflow_case_attach    where caseitemid =@caseitemid \n" +
") as t GROUP BY t.caseitemid,recid\n" +
") as att1 on att1.caseitemid=tmp.caseitemid and att1.recid=tmp.recid  ORDER BY tmp.reccreated asc";

            var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",caseItemId),
            };
            var result = ExecuteQuery(sql, param, tran);
            result.ForEach(t =>
            {
                if (t["filejson"] != null)
                {
                    var fileJson = t["filejson"].ToString();
                    var files = JArray.Parse(fileJson);
                    foreach (var t1 in files)
                    {
                        var newFile = t1["fileid"] == null ? string.Empty : (fileUrl + t1["fileid"]);
                        t1["fileurl"] = newFile;
                    }
                    t["filejson"] = files.ToString();
                }
            });
            return result;
        }

        public List<Dictionary<string, object>> GetWorkFlowCaseAtt(DbTransaction tran, Guid caseItemId, int userId, string currentHost = "")
        {
            var _config = ServiceLocator.Current.GetInstance<Microsoft.Extensions.Configuration.IConfigurationRoot>().GetSection("FileServiceSetting");
            var lngfFilePlugin = _config.GetValue<string>("LngfFilePlugin");
            string fileUrl = string.Format(lngfFilePlugin, currentHost);
            var sqlExists = "select nodetype from crm_sys_workflow_node where nodeid=(select nodeid from crm_sys_workflow_case_item where caseitemid=@caseitemid);";
            var sql = "select (select nodeid from crm_sys_workflow_case_item where caseitemid = tmp.caseitemid limit 1) as nodeid,tmp.*\n" +
        " ,att1.filejson::json\n" +
        " from (select caseitem.caseitemid,caseitem.suggest as comment,(crm_func_entity_protocol_format_workflow_casestatus(caseitem.casestatus,caseitem.choicestatus)) as flowstatus, u.username ,att.recid,caseitem.reccreated,caseitem.handleuser \n" +
        "from crm_sys_workflow_case_item as caseitem \n" +
        "{0} JOIN  crm_sys_workflow_case_attach as att on caseitem.caseitemid=att.caseitemid AND  att.recid='00000000-0000-0000-0000-000000000000' \n" +
        "LEFT JOIN crm_sys_workflow_case as wfcase on wfcase.caseid=caseitem.caseid  LEFT JOIN crm_sys_workflow_node AS n ON caseitem.nodeid = n.nodeid  \n" +
        " left join crm_sys_userinfo as u on u.userid = caseitem.handleuser where caseitem.caseitemid =@caseitemid \n" +
        "AND NOT EXISTS(SELECT 1 FROM crm_sys_workflow_case_item_joint where caseitemid=@caseitemid ) \n" +
        " group by caseitem.choicestatus,u.username,caseitem.caseitemid,caseitem.reccreated,att.recid,caseitem.suggest,n.nodetype,caseitem.handleuser   ORDER BY caseitem.reccreated ) as tmp\n" +
        "LEFT JOIN (\n" +
        "select array_to_json(array_agg(row_to_json(t))) as filejson,t.caseitemid,t.recid\n" +
        "from (\n" +
        "  select '' as fileurl,fileid, filename,caseitemid,recid FROM crm_sys_workflow_case_attach  where caseitemid =@caseitemid  \n" +
        ") as t GROUP BY t.caseitemid,recid\n" +
        ") as att1 on att1.caseitemid=tmp.caseitemid and att1.recid=tmp.recid;";
            var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",caseItemId),
            };
            if (ExecuteScalar(sqlExists, param, tran).ToString() == "2")
            {
                sql = string.Format(sql, "INNER");
            }
            else
            {
                sql = string.Format(sql, "LEFT");
            }
            var result = ExecuteQuery(sql, param, tran);
            result.ForEach(t =>
            {
                if (t["filejson"] != null)
                {
                    var fileJson = t["filejson"].ToString();
                    var files = JArray.Parse(fileJson);
                    foreach (var t1 in files)
                    {
                        var newFile = t1["fileid"] == null ? string.Empty : (fileUrl + t1["fileid"]);
                        t1["fileurl"] = newFile;
                    }
                    t["filejson"] = files.ToString();
                }
            });
            return result;
        }
        public OperateResult TransferToOther(DbTransaction tran, CaseItemTransferMapper transfer, int userId)
        {
            var sql = "update crm_sys_workflow_case_item set handleuser=@handleuser,reccreator=@userno,recupdator=@userno,choicestatus=@choicestatus,casestatus=@casestatus,reccreated=now(),recupdated=now()  where caseitemid=@caseitemid;";
            var param = new DbParameter[] {
                new NpgsqlParameter("handleuser",transfer.UserId),
                new NpgsqlParameter("userno",userId),
                new NpgsqlParameter("caseitemid",transfer.CaseItemId),
                new NpgsqlParameter("choicestatus",6),
                new NpgsqlParameter("casestatus",(object)0)
            };
            ExecuteNonQuery(sql, param, tran);
            return new OperateResult
            {
                Flag = 1
            };

        }

        public void CheckIsTransfer(DbTransaction tran, CaseItemJointTransfer transfer, int userId)
        {
            var sqlExist = " select 1 from crm_sys_workflow_case_item_transfer where caseitemid=@caseitemid limit 1";
            var sql = " insert into crm_sys_workflow_case_item_transfer(caseitemid,originuserid,userid,flowstatus,operatetype) values (@caseitemid,@originuserid,@userid,@flowstatus,(select isallowtransfer from crm_sys_workflow where flowid=(select flowid from crm_sys_workflow_case where caseid=(select caseid from crm_sys_workflow_case_item where caseitemid=@caseitemid) LIMIT 1)))";
            var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",transfer.CaseItemid),
                new NpgsqlParameter("originuserid",transfer.OrginUserId),
                new NpgsqlParameter("userid",transfer.UserId),
                new NpgsqlParameter("flowstatus",transfer.FlowStatus)
            };
            if (ExecuteScalar(sqlExist, param, tran) != null)
            {
                ExecuteNonQuery(sql, param, tran);
            }
        }
        public Guid GetLastestCaseId(DbTransaction tran, WorkFlowRepeatApprove workFlow, int useId)
        {
            var sqlEntity = " select modeltype from crm_sys_entity where entityid=(select entityid from crm_sys_workflow where flowid=@flowid limit 1);";
            var sqlWorkFlow = " SELECT caseid FROM crm_sys_workflow_case where {0} reccreator=@userno and flowid =@flowid and auditstatus = 2 and caseid = (select caseid from crm_sys_workflow_case where flowid = @flowid order by reccreated desc limit 1)" +
                " {1} order by reccreated desc limit 1; ";
            var param = new DbParameter[] {
                new NpgsqlParameter("caseid",workFlow.CaseId),
                new NpgsqlParameter("flowid",workFlow.FlowId),
                new NpgsqlParameter("entityid",workFlow.EntityId),
                new NpgsqlParameter("relentityid",workFlow.EntityModel.RelEntityId),
                new NpgsqlParameter("relrecid",workFlow.EntityModel.RelRecId),
                new NpgsqlParameter("userno",useId),
                new NpgsqlParameter("recid",workFlow.EntityModel.RelRecId)
            };
            var modelType = ExecuteScalar(sqlEntity, param, tran);
            if (modelType != null)
            {
                if (modelType.ToString() == "3")
                {
                    sqlWorkFlow = string.Format(sqlWorkFlow, string.Empty, " and exists(select 1 from crm_sys_workflow_case_entity_relation where relentityid=@relentityid and relrecid=@relrecid) ");
                }
                else if (modelType.ToString() == "0" || modelType.ToString() == "2")
                {
                    sqlWorkFlow = string.Format(sqlWorkFlow, " recid=@recid AND ", string.Empty);
                }
            }
            var result = ExecuteScalar(sqlWorkFlow, param, tran);
            if (result == null)
                throw new Exception("找不到历史审批数据");
            else
                return Guid.Parse(result.ToString());
        }
        public OperateResult NeedToRepeatApprove(DbTransaction tran, WorkFlowRepeatApprove workFlow, int userId)
        {
            var sqlWorkFlow = " INSERT INTO crm_sys_workflow_case (caseid,flowid, recid, auditstatus, vernum, nodenum, recstatus, reccreator, recupdator, title) SELECT @caseid,flowid, @recid, 0, vernum, nodenum, recstatus, reccreator, recupdator, title FROM crm_sys_workflow_case where caseid =@precaseid ";
            var sqlWorkFlowCase = "INSERT INTO crm_sys_workflow_case_item (caseitemid,caseid, nodenum, handleuser, copyuser, choicestatus, suggest, casestatus, casedata, remark, recstatus, reccreator, recupdator, stepnum, nodeid, skipnode) SELECT @caseitemid,@caseid, nodenum, handleuser, copyuser, {0}, '系统重新发起跳过',{1}, casedata, remark, recstatus, reccreator, recupdator, stepnum, nodeid, skipnode FROM crm_sys_workflow_case_item WHERE caseitemid = @precaseitemid and nodenum<>-1 order by reccreated asc;";
            //var sqlWorkFlowReciver = " insert into crm_sys_workflow_case_item_receiver(caseitemid,userid) select @caseitemid,userid from crm_sys_workflow_case_item_receiver where caseitemid=@precaseitemid;";
            var sqlJoint = " insert into crm_sys_workflow_case_item_joint (caseitemid,userid,comment,flowstatus,nodeid) select @caseitemid,userid,'系统重新发起跳过',flowstatus,nodeid from crm_sys_workflow_case_item_joint where caseitemid=@precaseitemid;";
            var sqlTransfer = " insert into crm_sys_workflow_case_item_transfer (caseitemid,originuserid,userid,comment,flowstatus,recid,operatetype) select @caseitemid,originuserid,userid,'系统重新发起跳过',flowstatus,@recid from crm_sys_workflow_case_item_transfer,operatetype where caseitemid=@precaseitemid and recid=@prerecid;";
            var sqlAtt = " insert into crm_sys_workflow_case_attach (caseitemid,fileid,filename,recid,nodeid) select @caseitemid,fileid,filename,@recid,nodeid from crm_sys_workflow_case_attach where caseitemid=@precaseitemid and recid=@prerecid;";
            var sqlAtt1 = " insert into crm_sys_workflow_case_attach (caseitemid,fileid,filename,recid,nodeid) select @caseitemid,fileid,filename,recid,nodeid from crm_sys_workflow_case_attach where caseitemid=@precaseitemid and recid='00000000-0000-0000-0000-000000000000';";
            var sqlCaseEntity = " insert into crm_sys_workflow_case_entity_relation(caseid,relentityid,relrecid) select @caseid,relentityid,relrecid from crm_sys_workflow_case_entity_relation where caseid=@precaseid;";
            var caseId = Guid.NewGuid();
            var param = new DbParameter[] {
                new NpgsqlParameter("precaseid",workFlow.CaseId),
                new NpgsqlParameter("caseid",caseId),
                new NpgsqlParameter("recid",workFlow.ModelType==3?workFlow.RecId:workFlow.EntityModel.RelRecId)
            };
            var result = ExecuteNonQuery(sqlWorkFlow, param, tran);
            ExecuteNonQuery(sqlCaseEntity, param, tran);
            if (result > 0)
            {
                var data = ExecuteQuery("select caseitemid,choicestatus,casestatus,nodenum,nodeid from crm_sys_workflow_case_item as caseitem  where caseid=@precaseid and caseitem.nodenum<>-1 order by caseitem.stepnum asc;", param, tran);
                int index = 0;
                foreach (var t in data)
                {
                    var caseItemId = Guid.NewGuid();
                    param = new DbParameter[] {
                        new NpgsqlParameter("precaseitemid",Guid.Parse(t["caseitemid"].ToString())),
                        new NpgsqlParameter("caseitemid",caseItemId),
                        new NpgsqlParameter("nodenum",Convert.ToInt32(t["nodenum"])),
                        new NpgsqlParameter("caseid",caseId),
                        new NpgsqlParameter("nodeid",Guid.Parse(t["nodeid"].ToString()))
                    };
                    index++;
                    if (data.Count == index)
                    {
                        var nodeInfos = ExecuteQuery(" select nodetype from crm_sys_workflow_node where nodeid = @nodeid;", param, tran);
                        var nodeInfo = nodeInfos.FirstOrDefault();
                        if (nodeInfo["nodetype"].ToString() == "1")
                        {
                            ExecuteNonQuery("update crm_sys_workflow_case_item set casestatus=0,choicestatus=6,suggest='' where caseid=@caseid and nodeid=@nodeid", param, tran);
                        }
                        else
                        {
                            ExecuteNonQuery("update crm_sys_workflow_case_item set casestatus=0,choicestatus=6 where caseitemid=@caseitemid", param, tran);
                        }
                        ExecuteNonQuery(string.Format(sqlWorkFlowCase, 6, 0).Replace("系统重新发起跳过", string.Empty), param, tran);
                        ExecuteNonQuery("update crm_sys_workflow_case set  nodenum=@nodenum,auditstatus=0 where caseid=@caseid", param, tran);
                    }
                    else
                        ExecuteNonQuery(string.Format(sqlWorkFlowCase, t["choicestatus"].ToString(), t["casestatus"].ToString()), param, tran);
                    ExecuteNonQuery(sqlJoint, param, tran);

                    var transferData = ExecuteQuery("select caseitemid,recid,originuserid from crm_sys_workflow_case_item_transfer where caseitemid=@precaseitemid order by reccreated asc;", param, tran);
                    if (data.Count > index)
                    {
                        foreach (var t1 in transferData)
                        {
                            var recid = Guid.NewGuid();
                            param = new DbParameter[] {
                        new NpgsqlParameter("precaseitemid",Guid.Parse(t["caseitemid"].ToString())),
                        new NpgsqlParameter("recid",recid),
                        new NpgsqlParameter("prerecid",Guid.Parse(t1["recid"].ToString())),
                        new NpgsqlParameter("caseitemid",caseItemId)
                       };
                            ExecuteNonQuery(sqlTransfer, param, tran);
                            ExecuteNonQuery(sqlAtt, param, tran);
                        }
                    }
                    else
                    {
                        if (transferData.Count > 0)
                        {
                            string handleUser = transferData.FirstOrDefault()["originuserid"].ToString();
                            param = new DbParameter[] {
                                new NpgsqlParameter("handleuser",Convert.ToInt32(handleUser)),
                                  new NpgsqlParameter("caseitemid",caseItemId),
                               };
                            ExecuteNonQuery("update crm_sys_workflow_case_item set  handleuser=@handleuser where caseitemid=@caseitemid", param, tran);
                        }
                    }
                    param = new DbParameter[] {
                        new NpgsqlParameter("precaseitemid",Guid.Parse(t["caseitemid"].ToString())),
                          new NpgsqlParameter("caseitemid",caseItemId),
                       };
                    ExecuteNonQuery(sqlAtt1, param, tran);
                }
            }
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Id = caseId.ToString(),
                    Msg = "重新发起成功"
                };
            }
            else
                return new OperateResult
                {
                    Msg = "重新发起失败"
                };
        }
        public OperateResult CheckIfExistNeedToRepeatApprove(DbTransaction tran, WorkFlowRepeatApprove workFlow, int userId)
        {
            var sqlEntity = " select modeltype from crm_sys_entity where entityid=@entityid;";
            var sqlWorkFlow = " SELECT 1 FROM crm_sys_workflow_case where {0} reccreator=@userno and EXISTS(select 1 from crm_sys_workflow where flowid=@flowid and entityid=@entityid  and recstatus=1 limit 1) and flowid =@flowid and auditstatus = 2 and caseid = (select caseid from crm_sys_workflow_case where flowid = @flowid order by reccreated desc limit 1)" +
                " {1} order by reccreated desc limit 1; ";
            var param = new DbParameter[] {
                new NpgsqlParameter("caseid",workFlow.CaseId),
                new NpgsqlParameter("flowid",workFlow.FlowId),
                new NpgsqlParameter("entityid",workFlow.EntityId),
                new NpgsqlParameter("relentityid",workFlow.EntityModel.RelEntityId),
                new NpgsqlParameter("relrecid",workFlow.EntityModel.RelRecId),
                new NpgsqlParameter("userno",userId),
                new NpgsqlParameter("recid",workFlow.EntityModel.RelRecId)
            };
            var modelType = ExecuteScalar(sqlEntity, param, tran);
            if (modelType != null)
            {
                if (modelType.ToString() == "3")
                {
                    sqlWorkFlow = string.Format(sqlWorkFlow, string.Empty, " and exists(select 1 from crm_sys_workflow_case_entity_relation where relentityid=@relentityid and relrecid=@relrecid) ");
                }
                else if (modelType.ToString() == "0")
                {
                    sqlWorkFlow = string.Format(sqlWorkFlow, " recid=@recid AND ", string.Empty);
                }
            }
            var result = ExecuteScalar(sqlWorkFlow, param, tran);
            if (result != null)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Id = result.ToString()
                };
            }
            else
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "找不到可以重新发起的数据,请检查数据"
                };
        }

        public void SaveWorkFlowGlobalEditJs(DbTransaction tran, WorkFlowGlobalJsMapper js, int userId)
        {
            var sql = "   insert INTO crm_sys_ucode_history_log(codetype,recid,relrecid,recname,oldcode,newcode,reccreator,commitremark,commituserid,commitdate,commithistory)\n" +
                        "   VALUES('WorkFlowJS',@nodeid,@flowid,\n" +
                        "(select entityname||'关联流程全局编辑JS' from  crm_sys_entity where entityid=(select entityid from crm_sys_workflow where flowid=@flowid limit 1)),\n" +
                        "  (select newcode from crm_sys_ucode_history_log where codetype='WorkFlowJS' ORDER BY reccreated DESC LIMIT 1),\n" +
                        "   @jscode,\n" +
                        "   @userno,\n" +
                        "   @remark,\n" +
                        "   @userno,\n" +
                        "   now(),\n" +
                        "   '{}'::jsonb\n" +
                        "   );";
            var param = new DbParameter[] {
                new NpgsqlParameter("flowid",js.FlowId),
                new NpgsqlParameter("nodeid",js.NodeId),
                new NpgsqlParameter("jscode",js.Js),
                new NpgsqlParameter("userno",userId),
                new NpgsqlParameter("remark",js.Remark)
            };
            ExecuteNonQuery(sql, param, tran);
        }

        #region 知会人

        public OperateResult SaveWorkflowInformer(DbTransaction tran, InformerRuleMapper informer, int userId)
        {
            var sql = " INSERT INTO crm_sys_workflow_informer (flowid, ruleid,ruleconfig,auditstatus) VALUES (@flowid, @ruleid, @ruleconfig::jsonb,@auditstatus); ";
            var param = new DbParameter[] {
                new NpgsqlParameter("flowid",informer.FlowId),
                new NpgsqlParameter("ruleid",informer.RuleId),
                new NpgsqlParameter("ruleconfig",informer.RuleConfig),
                new NpgsqlParameter("auditstatus",informer.AuditStatus)
            };
            ExecuteNonQuery(sql, param, tran);
            return new OperateResult
            {
                Flag = 1,
                Msg = "保存规则成功"
            };
        }
        public OperateResult UpdateWorkflowInformerStatus(DbTransaction tran, InformerRuleMapper informer, int userId)
        {
            var sql = " update crm_sys_workflow_informer set recstatus=@recstatus where flowid=@flowid; ";
            var param = new DbParameter[] {
                new NpgsqlParameter("flowid",informer.FlowId),
                new NpgsqlParameter("recstatus",informer.RecStatus)
            };
            ExecuteNonQuery(sql, param, tran);
            return new OperateResult
            {
                Flag = 1,
                Msg = "停用规则成功"
            };
        }

        public List<InformerRuleMapper> GetInformerRules(DbTransaction tran, InformerRuleMapper informer, int userId)
        {
            var sql = "select * from crm_sys_workflow_informer where flowid=@flowid {0} and recstatus=1";
            var param = new DbParameter[] {
                new NpgsqlParameter("flowid",informer.FlowId),
                new NpgsqlParameter("auditstatus",informer.AuditStatus)
            };
            string condition = string.Empty;
            if (informer.AuditStatus != -1)
            {
                condition += " and (auditstatus=@auditstatus or auditstatus=0 ) ";
                sql = string.Format(sql, condition);
            }
            else
                sql = string.Format(sql, condition);
            var result = ExecuteQuery<InformerRuleMapper>(sql, param, tran);
            return result;
        }
        public IDictionary<Guid, List<int>> GetInformer(Guid flowId, int auditStatus, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo nodeInfo, DbTransaction tran, int userId)
        {
            IDictionary<Guid, List<int>> ruleUsers = new Dictionary<Guid, List<int>>();
            List<int> approveUserIds;
            var informerRules = this.GetInformerRules(tran, new InformerRuleMapper { FlowId = flowId, AuditStatus = (auditStatus == 0 ? 2 : 1) }, userId);
            string typeStatus = ((auditStatus == 0 ? 2 : 1) == 1 ? "approve" : "failed");
            foreach (var rule in informerRules)
            {
                approveUserIds = new List<int>();
                if (rule.AuditStatus != (auditStatus == 0 ? 2 : 1) && rule.AuditStatus != 0) continue;
                string key = rule.AuditStatus == 0 ? "allpass" : typeStatus;
                var value = GetRuleConfigInfo(string.Format("endnodeconfig/{0}", key), rule.RuleConfig);
                if (!string.IsNullOrEmpty(value))
                {
                    var type = GetRuleConfigInfo(string.Format("endnodeconfig/{0}/type", key), rule.RuleConfig);
                    switch (type)
                    {
                        case "1":
                            var cpUser = GetRuleConfigInfo(string.Format("endnodeconfig/{0}/userids", key), rule.RuleConfig);
                            if (!string.IsNullOrEmpty(cpUser))
                            {
                                var userArr = cpUser.Split(",").Distinct().ToArray();
                                foreach (var t in userArr)
                                {
                                    approveUserIds.Add(Convert.ToInt32(t));
                                }
                            }
                            if (approveUserIds.Count > 0)
                                ruleUsers.Add(rule.RuleId, approveUserIds);
                            break;
                        case "2":
                            JToken funcname = GetRuleConfigInfo(string.Format("endnodeconfig/{0}/spfuncname", key), rule.RuleConfig);
                            var param = new DbParameter[] {
                                            new NpgsqlParameter("ruleconfig", rule.RuleConfig),
                                            new NpgsqlParameter("userno", userId),
                                            new NpgsqlParameter("recid", caseInfo.RecId),
                                            new NpgsqlParameter("relrecid", caseInfo.RelRecId),
                                            new NpgsqlParameter("caseid", caseInfo.CaseId),
                                };
                            var result = ExecuteQuery<ApproverInfo>("select * from " + funcname.ToString() + "(@caseid,@ruleconfig::jsonb,@recid,@relrecid,@userno);", param.ToArray(), tran);
                            result.ForEach(t =>
                            {
                                approveUserIds.Add(t.UserId);
                            });
                            if (approveUserIds.Count > 0)
                                ruleUsers.Add(rule.RuleId, approveUserIds);
                            break;
                        case "3":
                            string cmdText = "SELECT u.userid,u.username,u.usericon,u.namepinyin,ur.deptid,d.deptname   FROM crm_sys_userinfo AS u      LEFT JOIN crm_sys_account_userinfo_relate AS ur ON u.userid = ur.userid AND ur.recstatus = 1              LEFT JOIN crm_sys_department AS d ON ur.deptid = d.deptid   WHERE u.recstatus = 1";
                            JToken reportRelation = GetRuleConfigInfo(string.Format("endnodeconfig/{0}/reportrelation", key), rule.RuleConfig);
                            if (reportRelation != null)
                            {
                                var jo = JObject.Parse(reportRelation.ToString());
                                Guid id = Guid.Parse(jo["id"].ToString());
                                int relationType = Convert.ToInt32(jo["type"].ToString());
                                var reportRelationRepository = ServiceLocator.Current.GetInstance<IReportRelationRepository>();
                                QueryReportRelDetailMapper model = new QueryReportRelDetailMapper();
                                switch (relationType)
                                {
                                    case 1://流程发起人
                                        var l1 = CaseItemList(caseInfo.CaseId, userId);
                                        model = new QueryReportRelDetailMapper
                                        {
                                            ReportRelationId = id,
                                            UserId = l1.Count == 0 ? caseInfo.RecCreator : Convert.ToInt32(l1.FirstOrDefault()["handleuser"].ToString())
                                        };
                                        var d1 = reportRelationRepository.GetReportRelDetail(model, tran, userId);
                                        cmdText += @" AND u.userid in (" + string.Join(",", (d1.Count == 0 ? new string[] { "-1" } : d1.Select(t => t.ReportLeader))) + ")";
                                        break;
                                    case 2://上一步骤处理人
                                        var l2 = CaseItemList(caseInfo.CaseId, userId);
                                        if (l2.Count == 0)
                                        {
                                            model = new QueryReportRelDetailMapper
                                            {
                                                ReportRelationId = id,
                                                UserId = l2.Count == 0 ? caseInfo.RecCreator : Convert.ToInt32(l2.FirstOrDefault()["handleuser"].ToString())
                                            };
                                            var d2 = reportRelationRepository.GetReportRelDetail(model, tran, userId);
                                            cmdText += @" AND u.userid in (" + string.Join(",", (d2.Count == 0 ? new string[] { "-1" } : d2.Select(t => t.ReportLeader))) + ")";
                                        }
                                        else
                                        {
                                            int index = l2.Count - 1;
                                            if (index >= 0)
                                            {
                                                var n = l2[index] == null ? string.Empty : l2[index]["nodeid"].ToString();
                                                var userIds = l2.Where(t => t["nodeid"].ToString() == n).Select(t => t["handleuser"]).ToList();
                                                model = new QueryReportRelDetailMapper
                                                {
                                                    ReportRelationId = id,
                                                    UserIds = string.Join(",", userIds)
                                                };
                                                var d2 = reportRelationRepository.GetReportRelDetail(model, tran, userId);
                                                cmdText += @" AND u.userid in (" + string.Join(",", (d2.Count == 0 ? new string[] { "-1" } : d2.Select(t => t.ReportLeader))) + ")";
                                            }
                                        }
                                        break;
                                    case 3://表单中的人员
                                        string f3, e3;
                                        GetEntityFieldBasicRuleConfigPath(tran, caseInfo, nodeInfo, string.Format("endnodeconfig/{0}", key), out f3, out e3);
                                        if (GetRuleConfigInfo("entityid", nodeInfo) == caseInfo.RelEntityId.ToString())//判断是否是主实体还是关联实体
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
                                }
                                param = new DbParameter[] {
                                         new NpgsqlParameter("recid",caseInfo.RecId)
                                    };
                                result = ExecuteQuery<ApproverInfo>(cmdText, param, tran);
                                result.ForEach(t =>
                                {
                                    approveUserIds.Add(t.UserId);
                                });
                                if (approveUserIds.Count > 0)
                                    ruleUsers.Add(rule.RuleId, approveUserIds);
                            }
                            break;
                    }
                }
            }
            return ruleUsers;
        }


        #endregion

        public OperateResult RejectToOrginalNode(DbTransaction trans, RejectToOrginalNode reject, int userId)
        {
            var sql = "insert into crm_sys_workflow_case_item(caseid, nodenum,choicestatus,handleuser, casestatus, reccreator, recupdator,stepnum,nodeid) select caseid,nodenum,6,handleuser,0,reccreator,recupdator,(select max(stepnum)+1 from crm_sys_workflow_case_item where caseid=item.caseid limit 1),nodeid from crm_sys_workflow_case_item as item where item.caseitemid=@caseitemid;";
            var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",reject.CaseItemId),
                new NpgsqlParameter("precaseitemid",reject.PreCaseItemId),
                new NpgsqlParameter("suggest",reject.Remark)
            };
            int count = ExecuteNonQuery(sql, param, trans);
            if (count > 0)
            {
                foreach (var t in reject.FileAttachs)
                {
                    t.CaseItemId = reject.PreCaseItemId;
                    this.InsertCaseItemAttach(trans, t, userId);
                }
                sql = " update crm_sys_workflow_case_item set suggest=@suggest where caseitemid=@precaseitemid; update crm_sys_workflow_case set nodenum=(select nodenum from crm_sys_workflow_case_item as item where item.caseitemid=@caseitemid) where caseid=(select caseid from crm_sys_workflow_case_item as item where item.caseitemid=@caseitemid);";
                ExecuteNonQuery(sql, param, trans);
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "驳回成功"
                };
            }
            else
                return new OperateResult
                {
                    Msg = "驳回失败"
                };
        }



        public List<WorkFlowInfo> GetWorkFlowInfoByCaseItemId(DbTransaction trans, Guid caseitemid)
        {
            var sql = @" select f.* from crm_sys_workflow_case_item ci
                                inner join crm_sys_workflow_case c on ci.caseid = c.caseid
                                inner join crm_sys_workflow f on f.flowid = c.flowid
                                where ci.caseitemid =@caseitemid ";

            var param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",caseitemid)
            };
            return ExecuteQuery<WorkFlowInfo>(sql, param, trans);
        }

        public List<DepartListMapper> GetUserDefaultDepartment(int userid)
        {
            var sql = @"       select d.deptid as departid,1 ismaster  from crm_sys_userinfo u 
                               inner join crm_sys_account_userinfo_relate aur on u.userid=aur.userid
                               inner join crm_sys_account a on aur.accountid=a.accountid
                               inner join crm_sys_department d on d.deptid=aur.deptid
                               where aur.recstatus=1 
                               and  u.userid=@userid
                               and a.recstatus=1
                               ";

            var param = new DbParameter[] {
                new NpgsqlParameter("userid",userid)
            };


            return ExecuteQuery<DepartListMapper>(sql, param);

        }



        public List<DepartmentEditMapper> GetUserDepartments(Guid departmentid)
        {
            var sql = @" select d.* from crm_sys_department_treepaths t
                inner join crm_sys_department d on t.ancestor=d.deptid
                where descendant=@departmentid
                order by nodepath desc ";

            var param = new DbParameter[] {
                new NpgsqlParameter("departmentid",departmentid)
            };

            return ExecuteQuery<DepartmentEditMapper>(sql, param);

        }
        public List<WorkFlowSign> GetWorkFlowSign(Guid caseItemId, int userId)
        {
            var sql = " select  caseitemid,signstatus,originuserid as originaluserid,userid from crm_sys_workflow_case_item_transfer  where caseitemid=@caseitemid  and operatetype=1 order by reccreated asc  ";

            DbParameter[] param = new DbParameter[] {
                new NpgsqlParameter("caseitemid",caseItemId)
            };
            var r = ExecuteQuery<WorkFlowSign>(sql, param);
            return r;
        }

        public bool WithDrawkWorkFlowByCreator(DbTransaction trans, Guid caseid, int userid)
        {
            WorkFlowCaseInfo caseinfo = GetWorkFlowCaseInfo(null, caseid);

            var nodeidSql = @"SELECT nodeid FROM crm_sys_workflow_node WHERE flowid=@flowid AND vernum=@vernum AND steptypeid=0";
            var entitySqlParameters = new List<DbParameter>();
            entitySqlParameters.Add(new NpgsqlParameter("flowid", caseinfo.FlowId));
            entitySqlParameters.Add(new NpgsqlParameter("vernum", caseinfo.VerNum));
            var nodeidResult = ExecuteScalar(nodeidSql, entitySqlParameters.ToArray(), trans);
            Guid nodeid = Guid.Empty;
            if (nodeidResult != null)
            {
                Guid.TryParse(nodeidResult.ToString(), out nodeid);
            }

            int _stepNum = GetWorkFlowCaseItemInfoMaxStepNum(trans, caseid);
            int _netStepNum = _stepNum + 1;
            int handleuser = userid;



            //更新最新的节点为退回
            UpdateWorkFlowCaseStatusByStepNum(trans, caseid, _stepNum);

            string sql = string.Format(@"INSERT INTO crm_sys_workflow_case_item (caseid, nodenum,choicestatus,handleuser, casestatus, reccreator, recupdator,stepnum,nodeid) 
                                         VALUES (@caseid,0,4,@handleuser,0,@userno,@userno,@stepnum,@nodeid);
                                         UPDATE crm_sys_workflow_case SET nodenum = 0,recupdator=@userno,recupdated=@recupdated WHERE caseid=@caseid;");


            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("caseid", caseid));
            sqlParameters.Add(new NpgsqlParameter("handleuser", handleuser));
            sqlParameters.Add(new NpgsqlParameter("nodeid", nodeid));
            sqlParameters.Add(new NpgsqlParameter("userno", userid));
            sqlParameters.Add(new NpgsqlParameter("stepnum", _netStepNum));
            sqlParameters.Add(new NpgsqlParameter("recupdated", DateTime.Now));

            var result = ExecuteNonQuery(sql, sqlParameters.ToArray(), trans);

            return result == 2;
        }


        public int GetWorkFlowCaseItemInfoMaxStepNum(DbTransaction trans, Guid caseid)
        {
            string executeSql = " select max(stepnum) from crm_sys_workflow_case_item  where caseid=@caseid  ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("caseid", caseid),

            };

            int _stepNum = 0;
            object obj = ExecuteScalar(executeSql, param, trans);
            if (obj != null)
            {
                _stepNum = int.Parse(obj.ToString());
            }

            return _stepNum;
        }

        public int UpdateWorkFlowCaseStatusByStepNum(DbTransaction trans, Guid caseid, int stepnum)
        {
            string _strSql = @" update crm_sys_workflow_case_item set choicestatus = 2, casestatus = 2 where caseid=@caseid and stepnum=@stepnum ";

            DbParameter[] param = new DbParameter[] {
                new NpgsqlParameter("@caseid",caseid),
                new NpgsqlParameter("@stepnum",stepnum),
                };
            int _result = ExecuteNonQuery(_strSql, param, trans);
            return _result;
        }


        public int DeleteWorkFlowCaseItemTransfer(DbTransaction trans, Guid caseitemid, int userid)
        {
            string _strSql = @" delete from  crm_sys_workflow_case_item_transfer  where caseitemid=@caseitemid and originuserid=@originuserid ";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseitemid",caseitemid),
                    new NpgsqlParameter("@originuserid",userid)
                };
            int _result = ExecuteNonQuery(_strSql, param, trans);
            return _result;
        }


        public int UpdateWorkFlowCaseitemHandler(DbTransaction trans, Guid caseitemid, int userid)
        {
            string _strSql = @" update crm_sys_workflow_case_item set handleuser=@handleuser where caseitemid=@caseitemid ";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseitemid",caseitemid),
                    new NpgsqlParameter("@handleuser",userid)
                };
            int _result = ExecuteNonQuery(_strSql, param, trans);
            return _result;

        }
        public List<WorkFlowCaseItemInfo> GetWorkFlowCaseItemOfCase(DbTransaction trans, Guid caseid)
        {
            string _strSql = @"  select * from crm_sys_workflow_case_item 
                                where caseid=@caseid order by reccreated desc,nodenum desc ";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseid",caseid)
                };

            return ExecuteQuery<WorkFlowCaseItemInfo>(_strSql, param, trans);


        }

        public int UpdateWorkFlowCaseNodeNum(DbTransaction trans, Guid caseid, Guid caseitemid)
        {
            string _strSql = @" update crm_sys_workflow_case set nodenum=(select nodenum from crm_sys_workflow_case_item where caseitemid=@caseitemid) where caseid=@caseid ";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseitemid",caseitemid),
                    new NpgsqlParameter("@caseid",caseid)
                };
            int _result = ExecuteNonQuery(_strSql, param, trans);
            return _result;

        }


        public int DeleteWorkFlowCaseItems(DbTransaction trans, Guid caseid, Guid caseitemid)
        {
            string _strSql = @"  delete
                                 from crm_sys_workflow_case_item
                                 where caseid =@casesid
                                 and nodenum> (select nodenum from crm_sys_workflow_case_item where caseitemid=@caseitemid)";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseitemid",caseitemid),
                    new NpgsqlParameter("@casesid",caseid)
                };
            int _result = ExecuteNonQuery(_strSql, param, trans);
            return _result;
        }


        public int GetWorkFlowCaseItemCout(DbTransaction trans, Guid caseid, Guid caseitemid)
        {
            string _strSql = @" select count(1) 
                                 from crm_sys_workflow_case_item
                                 where caseid =@casesid
                                 and casestatus> 1
                                 and nodenum> (select nodenum from crm_sys_workflow_case_item where caseitemid = @caseitemid)";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseitemid",caseitemid),
                    new NpgsqlParameter("@casesid",caseid)
                };
            object _result = ExecuteScalar(_strSql, param, trans);

            int _theCount = 0;
            int.TryParse(_result.ToString(), out _theCount);
            return _theCount;

        }


        public int UpdateWorkFlowCaseitemChoicestatus(DbTransaction trans, Guid caseitemid, int choicestatus, int casestatus)
        {
            string _strSql = @" update crm_sys_workflow_case_item set casestatus=@casestatus,choicestatus=@choicestatus,suggest='' where caseitemid=@caseitemid ";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseitemid",caseitemid),
                    new NpgsqlParameter("@choicestatus",choicestatus),
                    new NpgsqlParameter("@casestatus",casestatus)
                };
            int _result = ExecuteNonQuery(_strSql, param, trans);
            return _result;

        }

        public int UpdateWorkFlowCaseNodeNumNew(DbTransaction trans, Guid caseid, int nodenum)
        {
            string _strSql = @" update crm_sys_workflow_case set nodenum=@nodenum where caseid=@caseid ";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseid",caseid),
                    new NpgsqlParameter("@nodenum",nodenum)
                };
            int _result = ExecuteNonQuery(_strSql, param, trans);
            return _result;

        }


        public int DeleteWorkFlowCaseItems(DbTransaction trans, Guid caseitemid)
        {
            string _strSql = @"  delete
                                 from crm_sys_workflow_case_item
                                 where caseitemid =@caseitemid ";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseitemid",caseitemid)
                };
            int _result = ExecuteNonQuery(_strSql, param, trans);
            return _result;
        }

        public List<WorkFlowCaseItemTransfer> GetWorkFlowCaseItemTransfer(DbTransaction trans, Guid caseitemid)
        {
            string _strSql = @"  select * from crm_sys_workflow_case_item_transfer 
                                where caseitemid=@caseitemid order by reccreated desc ";

            DbParameter[] param = new DbParameter[] {
                    new NpgsqlParameter("@caseitemid",caseitemid)
                };

            return ExecuteQuery<WorkFlowCaseItemTransfer>(_strSql, param, trans);
        }


    }
}
