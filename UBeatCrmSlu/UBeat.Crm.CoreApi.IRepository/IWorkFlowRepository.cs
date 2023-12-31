﻿using System;
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

        Guid  AddWorkflowCase(DbTransaction tran, WorkFlowInfo workflowinfo, WorkFlowAddCaseMapper caseMapper, int userNumber);
        List<NextNodeDataInfo> GetNodeDataInfo(Guid flowid, Guid nodeid, int vernum, DbTransaction trans = null);
        List<NextNodeDataInfo> GetCurNodeDataInfo(Guid flowid, Guid nodeid, int vernum, DbTransaction trans = null);


        OperateResult AddCaseItem(WorkFlowAddCaseItemMapper caseItemMapper, int userNumber);

        OperateResult AuditCaseItem(WorkFlowAuditCaseItemMapper caseItemMapper, int userNumber);

        List<Dictionary<string, object>> CaseItemList(Guid caseId, int userNumber, int skipnode = -1, DbTransaction tran = null, string currentHost = "");

        Dictionary<string, List<IDictionary<string, object>>> NodeLineInfo(Guid flowId, int userNumber);

        Dictionary<string, List<Dictionary<string, object>>> GetNodeLinesInfo(Guid flowId, int userNumber, DbTransaction trans = null, int versionNum = -1);
        OperateResult NodeLinesConfig(WorkFlowNodeLinesConfigMapper nodeLineConfig, int userNumber);

        void SaveNodeLinesConfig(WorkFlowNodeLinesConfigMapper nodeLineConfig, int userNumber);

        void SaveNodeEvents(Guid flowId, List<WorkFlowNodeMapper> nodes, DbTransaction tran = null);

        dynamic GetFreeFlowNodeEvents(Guid flowId, DbTransaction tran = null);

        Dictionary<string, List<IDictionary<string, object>>> FlowList(PageParam pageParam, int flowstatus, string searchName, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> Detail(Guid flowId, int userNumber);

        OperateResult AddFlow(WorkFlowAddMapper flowMapper, int userNumber);
        OperateResult UpdateFlow(WorkFlowUpdateMapper flowMapper, int userNumber);

        OperateResult DeleteFlow(string flowIds, int userNumber);
        OperateResult UnDeleteFlow(string flowIds, int userNumber);

        List<WorkFlowSign> GetWorkFlowSign(Guid caseItemId, int userId);
        WorkFlowCaseInfo GetWorkFlowCaseInfo(DbTransaction trans, Guid caseid);
        WorkFlowInfo GetWorkFlowInfo(DbTransaction trans, Guid flowid);

        WorkFlowNodeInfo GetWorkFlowNodeInfo(DbTransaction trans, Guid nodeid);

        /// <summary>
        /// 获取前一节点
        /// </summary>
        WorkFlowNodeInfo GetPreviousWorkFlowNodeInfo(DbTransaction trans, Guid flowid, int vernum, Guid nodeid);

        List<WorkFlowNodeInfo> GetNextNodeInfoList(DbTransaction trans, Guid flowid, int vernum, Guid fromnodeid);
        List<WorkFlowNodeInfo> GetNodeInfoList(DbTransaction trans, Guid flowid, int vernum);

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
        List<ApproverInfo> GetFlowNodeCPUser(Guid caseId, Guid nodeid, int userNumber, WorkFlowType flowtype, DbTransaction trans = null, int auditStatus = -10);
        string GetRuleConfigInfo(string path, string json);
        /// <summary>
        /// 获取流程下一节点连线上的ruleid
        /// </summary>
        /// <param name="flowid"></param>
        /// <param name="endnode"></param>
        /// <param name="vernum"></param>
        /// <returns></returns>
        Guid GetNextNodeRuleId(Guid flowid, Guid fromnodeid, Guid tonodeid, int vernum, DbTransaction trans = null);
        
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
        bool RebackWorkFlowCaseItem(WorkFlowCaseInfo caseinfo, WorkFlowCaseItemInfo caseitem, int userNumber, DbTransaction trans = null);

        /// <summary>
        /// 重新发起流程
        /// </summary>
        bool ReOpenWorkFlowCase(Guid caseid, Guid caseitemid, int userNumber, DbTransaction trans = null);


        /// <summary>
        /// 审批已经到达了最后一步,添加最后节点
        /// </summary>
        Object EndWorkFlowCaseItem(Guid caseid, Guid nodeid, int stepnum, int userNumber, DbTransaction trans = null);
        List<int> GetSubscriber(Guid caseItemId, int auditStatus, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo nodeInfo, DbTransaction tran, int userId);
        IDictionary<Guid, List<int>> GetInformer(Guid flowId, int auditStatus, WorkFlowCaseInfo caseInfo, WorkFlowNodeInfo nodeInfo, DbTransaction tran, int userId);
        void AuditWorkFlowCaseData(WorkFlowAuditCaseItemMapper auditdata, WorkFlowCaseItemInfo caseitem, int userNumber, DbTransaction trans = null);
        void AddEndWorkFlowCaseItemCPUser(DbTransaction tran, Guid caseItemId, List<int> cpUserId);
        /// <summary>
        /// 获取流程event函数
        /// </summary>
        /// <param name="flowid">流程id</param>
        /// <param name="nodeid">event关联的节点nodeid，固定流程的节点id，若为自由流程，则uuid值为0作为流程起点，值为1作为流程终点</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        WorkFlowEventInfo GetWorkFlowEvent(Guid flowid, Guid nodeid, DbTransaction trans = null);

        /// <summary>
        /// 执行流程扩展函数
        /// </summary>
        void ExecuteWorkFlowEvent(WorkFlowEventInfo eventInfo, Guid caseid, int nodenum, int choicestatus, int userno, DbTransaction trans = null);

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
        /// <summary>
        /// 获取工作流对应的ruleid
        /// </summary>
        /// <param name="flowId"></param>
        /// <param name="userId"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        Guid getWorkflowRuleId(Guid flowId, int userId, DbTransaction tran);
        void SaveWorkflowRuleRelation(string id, Guid workflowId, int userId, DbTransaction tran);

        /// <summary>
        /// 获取流程审批的抄送人
        /// </summary>
        /// <param name="caseid"></param>
        /// <returns></returns>
        List<DomainModel.Account.UserInfo> GetWorkFlowCopyUser(Guid caseid, DbTransaction trans = null);

        void SetWorkFlowCaseItemReaded(DbTransaction trans, Guid caseid, int nodenum, int userNumber, int stepnum = -1);
        string SaveTitleConfig(Guid flowId, string titleConfig, int userId);
        void TerminateCase(DbTransaction tran, Guid caseId);
        List<WorkFlowCaseItemInfo> GetWorkflowCaseWaitingDealItems(DbTransaction tran, Guid caseId);
        List<WorkFlowCaseInfo> GetExpiredWorkflowCaseList(DbTransaction tran, int userId);
        Dictionary<string, object> GetWorkflowByEntityId(DbTransaction p, Guid entityId, int userId);

        OperateResult InsertSpecialJointComment(DbTransaction tran, CaseItemJoint joint, int userId);
        void UpdateWorkFlowNodeScheduled(DbTransaction trans, WorkFlowNodeScheduled scheduled, int userId);
        List<WorkFlowNodeScheduledList> GetWorkFlowNodeScheduled(DbTransaction trans, int userId);
        List<Dictionary<string, object>> GetSpecialJointCommentDetail(DbTransaction tran, Guid caseItemId, int userId, string currentHost = "");
        OperateResult InsertTransfer(DbTransaction tran, CaseItemJointTransfer transfer, int userId);
        List<Dictionary<string, object>> GetWorkFlowCaseTransferAtt(DbTransaction tran, Guid caseItemId, int userId, string currentHost = "");
        OperateResult InsertCaseItemAttach(DbTransaction tran, CaseItemFileAttach attach, int userId);
        OperateResult TransferToOther(DbTransaction tran, CaseItemTransferMapper transfer, int userId);
        void CheckIsTransfer(DbTransaction tran, CaseItemJointTransfer transfer, int userId);
        OperateResult NeedToRepeatApprove(DbTransaction tran, WorkFlowRepeatApprove workFlow, int userId);
        Guid GetLastestCaseId(DbTransaction tran, WorkFlowRepeatApprove workFlow, int useId);
        OperateResult CheckIfExistNeedToRepeatApprove(DbTransaction tran, WorkFlowRepeatApprove workFlow, int userId);
        List<Dictionary<string, object>> GetWorkFlowCaseAtt(DbTransaction tran, Guid caseItemId, int userId, string currentHost = "");
        void SaveWorkFlowGlobalEditJs(DbTransaction tran, WorkFlowGlobalJsMapper js, int userId);
        OperateResult SaveWorkflowInformer(DbTransaction tran, InformerRuleMapper informer, int userId);
        OperateResult UpdateWorkflowInformerStatus(DbTransaction tran, InformerRuleMapper informer, int userId);
        List<InformerRuleMapper> GetInformerRules(DbTransaction tran, InformerRuleMapper informer, int userId);

        OperateResult RejectToOrginalNode(DbTransaction trans, RejectToOrginalNode reject, int userId);

        List<WorkFlowInfo> GetWorkFlowInfoByCaseItemId(DbTransaction trans, Guid caseitemid);



        bool WithDrawkWorkFlowByCreator(DbTransaction trans, Guid caseid, int userid);
        List<WorkFlowCaseItemTransfer> GetWorkFlowCaseItemTransfer(DbTransaction trans, Guid caseitemid);

        int DeleteWorkFlowCaseItemTransfer(DbTransaction trans, Guid caseitemid, int userid);
        int UpdateWorkFlowCaseitemHandler(DbTransaction trans, Guid caseitemid, int userid);

        List<WorkFlowCaseItemInfo> GetWorkFlowCaseItemOfCase(DbTransaction trans, Guid caseid);
        int GetWorkFlowCaseItemCout(DbTransaction trans, Guid caseid, Guid caseitemid);
        int DeleteWorkFlowCaseItems(DbTransaction trans, Guid caseid, Guid caseitemid);
        int UpdateWorkFlowCaseitemChoicestatus(DbTransaction trans, Guid caseitemid, int choicestatus, int casestatus);

        int UpdateWorkFlowCaseNodeNum(DbTransaction trans, Guid caseid, Guid caseitemid);

        int DeleteWorkFlowCaseItems(DbTransaction trans, Guid caseitemid);

        int UpdateWorkFlowCaseNodeNumNew(DbTransaction trans, Guid caseid, int nodenum);

    }
}

