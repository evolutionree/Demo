using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IWorkFlowRepository : IBaseRepository
    {
        OperateResult AddCase(DbTransaction tran, WorkFlowAddCaseMapper caseMapper, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> NextNodeData(Guid caseId, int userNumber);

        OperateResult AddCaseItem(WorkFlowAddCaseItemMapper caseItemMapper, int userNumber);

        OperateResult AuditCaseItem(WorkFlowAuditCaseItemMapper caseItemMapper, int userNumber);

        List<IDictionary<string, object>> CaseItemList(Guid caseId, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> NodeLineInfo(Guid flowId, int userNumber);

        Dictionary<string, List<Dictionary<string, object>>> GetNodeLinesInfo(Guid flowId, int userNumber, DbTransaction trans = null);
        OperateResult NodeLinesConfig(WorkFlowNodeLinesConfigMapper nodeLineConfig, int userNumber);

        void SaveNodeLinesConfig(WorkFlowNodeLinesConfigMapper nodeLineConfig, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> FlowList(PageParam pageParam, int flowstatus, string searchName, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> Detail(Guid flowId, int userNumber);

        OperateResult AddFlow(WorkFlowAddMapper flowMapper, int userNumber);
        OperateResult UpdateFlow(WorkFlowUpdateMapper flowMapper, int userNumber);

        OperateResult DeleteFlow(string flowIds, int userNumber);
        OperateResult UnDeleteFlow(string flowIds, int userNumber);


        WorkFlowCaseInfo GetWorkFlowCaseInfo(DbTransaction trans, Guid caseid);
        WorkFlowInfo GetWorkFlowInfo(DbTransaction trans, Guid flowid);

        WorkFlowNodeInfo GetWorkFlowNodeInfo(DbTransaction trans, Guid nodeid);

        /// <summary>
        /// 获取前一节点
        /// </summary>
        WorkFlowNodeInfo GetPreviousWorkFlowNodeInfo(DbTransaction trans, Guid flowid, int vernum, Guid nodeid);

        List<WorkFlowNodeInfo> GetNextNodeInfoList(DbTransaction trans, Guid flowid, int vernum, Guid fromnodeid);
       

        List<WorkFlowCaseItemInfo> GetWorkFlowCaseItemInfo(DbTransaction trans, Guid caseid, int nodenum,int stepnum=-1);
        List<WorkFlowCaseInfo> getWorkFlowCaseListByRecId(DbTransaction trans, string recid, int userNum);
        List<WorkFlowCaseInfo> getWorkFlowCaseListByRecIds(DbTransaction trans, List<string> recids, int userNum);

        int getWorkFlowCountByStageId(DbTransaction trans, string stageid, int userNumber);

        /// <summary>
        /// 获取当前流程执行到哪个节点
        /// </summary>
        /// <param name="caseId"></param>
        /// <param name="userNumber"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        int GetWorkFlowNowNodeNumber(Guid caseId, int userNumber, DbTransaction trans=null);
        int GetWorkFlowNextNodeNumber(Guid caseId,int nodenum, int userNumber, DbTransaction trans = null);

        List<NextNodeDataInfo> GetNextNodeDataInfoList(Guid flowid, Guid fromnodeid, int vernum,  DbTransaction trans = null);
        List<ApproverInfo> GetFlowNodeApprovers(Guid caseId,Guid nodeid, int userNumber, WorkFlowType flowtype, DbTransaction trans = null);

        /// <summary>
        /// 获取流程下一节点连线上的ruleid
        /// </summary>
        /// <param name="flowid"></param>
        /// <param name="endnode"></param>
        /// <param name="vernum"></param>
        /// <returns></returns>
        Guid GetNextNodeRuleId(Guid flowid, Guid tonodeid, int vernum, DbTransaction trans = null);
        
        /// <summary>
        /// 获取流程当前节点id
        /// </summary>
        int GetNowNodeId(Guid flowid, int nextNodeId, int vernum, DbTransaction trans = null);

        bool ValidateNextNodeRule(WorkFlowCaseInfo caseInfo, string ruleFormatSql, int userNumber, DbTransaction trans = null);

        /// <summary>
        /// 审批流程节点
        /// </summary>
        bool AuditWorkFlowCaseItem(WorkFlowAuditCaseItemMapper auditdata, WorkFlowCaseItemInfo caseitem,  int userNumber, DbTransaction trans = null);

        /// <summary>
        /// 审批流程时更新流程数据
        /// </summary>
        bool AuditWorkFlowCase(Guid caseid, AuditStatusType auditstatus, int nodenum, int userNumber, DbTransaction trans = null);


        /// <summary>
        /// 退回流程节点
        /// </summary>
        bool RebackWorkFlowCaseItem(Guid flowId, int vernum, WorkFlowCaseItemInfo caseitem, int userNumber, DbTransaction trans = null);

        /// <summary>
        /// 重新发起流程
        /// </summary>
        bool ReOpenWorkFlowCase(Guid caseid, Guid caseitemid, int userNumber, DbTransaction trans = null);


        /// <summary>
        /// 审批已经到达了最后一步,添加最后节点
        /// </summary>
        bool EndWorkFlowCaseItem(Guid caseid, int stepnum, int userNumber, DbTransaction trans = null);

        /// <summary>
        /// 获取流程event函数
        /// </summary>
        /// <param name="flowid">流程id</param>
        /// <param name="nodeid">event关联的节点nodeid，固定流程的节点id，若为自由流程，则uuid值为0作为流程起点，值为1作为流程终点</param>
        /// <param name="steptype">0为caseitemadd执行 1为caseitemaudit执行</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        string GetWorkFlowEvent(Guid flowid, Guid nodeid, int steptype, DbTransaction trans = null);

        void ExecuteWorkFlowEvent(string funcname, Guid caseid, int nodenum, int choicestatus, int userno, DbTransaction trans = null);

        void ExecuteUpdateWorkFlowEntity( Guid caseid, int nodenum, int userno, DbTransaction trans = null);

        /// <summary>
        /// 添加审批节点
        /// </summary>
        /// <returns></returns>
        bool AddCaseItem(List<WorkFlowCaseItemInfo> caseitems, int userno, AuditStatusType auditstatus = AuditStatusType.Approving, DbTransaction trans = null);




        /// <summary>
        /// 判断是否允许编辑审批数据
        /// </summary>
        /// <returns></returns>
        bool CanEditWorkFlowCase(WorkFlowInfo workflow, int userno, DbTransaction trans = null);
    }
}

