using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.WorkFlow
{
    public class WorkFlowAddCaseModel
    {
        public Guid FlowId { get; set; }
        public Guid EntityId { get; set; }
        public Guid RecId { get; set; }
        public Guid? RelEntityId { get; set; }
        public Guid? RelRecId { get; set; }
        public Dictionary<string,object> CaseData { get; set; }
    }

    public class WorkFlowNextNodeModel
    {
        /// <summary>
        /// 流程实体，如果第一次提交流程，为空guid
        /// </summary>
        public Guid CaseId { get; set; }
        
    }

    public class WorkFlowAddCaseItemModel
    {
        public Guid CaseId { get; set; }
        public int NodeNum { get; set; }
        public string HandleUser { get; set; }
        public string CopyUser { get; set; }
        public string Remark { get; set; }
        public Dictionary<string,object> CaseData { get; set; }
    }

    public class WorkFlowAuditCaseItemModel
    {
        public Guid CaseId { get; set; }
        public int NodeNum { get; set; }
        public string Suggest { get; set; }
		public Dictionary<string, object> CaseData { get; set; }
		public int ChoiceStatus { get; set; }

        /// <summary>
        /// 分支流程时，选择的节点id
        /// </summary>
        public Guid NodeId { set; get; }
        public string HandleUser { get; set; }
        public string CopyUser { get; set; }
    }

    public class WorkFlowAuditCaseItemListModel
    {
        public Guid CaseId { get; set; }
    }

    public class WorkFlowNodeLinesInfoModel
    {
        public Guid FlowId { get; set; }
    }

    public class WorkFlowNodeLinesConfigModel
    {
        public Guid FlowId { get; set; }
        public List<WorkFlowNodeModel> Nodes { get; set; }
        public List<WorkFlowLineModel> Lines { get; set; }
    }

    public class WorkFlowNodeModel
    {
        public Guid NodeId { set; get; }
        public string NodeName { get; set; }
        public int NodeNum { get; set; }
        public int AuditNum { get; set; }
        public int NodeType { get; set; }
        public int StepTypeId { get; set; }
        public Dictionary<string,string> RuleConfig { get; set; }
        public Dictionary<string, object> ColumnConfig { get; set; }
        public int AuditSucc { get; set; }
    }

    public class WorkFlowLineModel
    {
        public int FromNode { get; set; }//新设计不使用，保留为了暂时兼容
        public int EndNode { get; set; }//新设计不使用，保留为了暂时兼容
        public Guid FromNodeId { get; set; }
        public Guid ToNodeId { get; set; }
        public Guid? RuleId { get; set; }
    }

    public class WorkFlowListModel
    {
        public int FlowStatus { get; set; }
        public string SearchName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class WorkFlowDetailModel
    {
        public Guid FlowId { get; set; }
    }

    public class WorkFlowAddModel
    {
        public string FlowName { get; set; }
        public int FlowType { get; set; }
        public int BackFlag { get; set; }
        public int ResetFlag { get; set; }
        public int ExpireDay { get; set; }
        public string Remark { get; set; }
        public Guid EntityId { get; set; }
        public int SkipFlag { get; set; }
    }

    public class WorkFlowUpdateModel
    {
        public Guid FlowId { get; set; }
        public string FlowName { get; set; }
        public int BackFlag { get; set; }
        public int ResetFlag { get; set; }
        public int ExpireDay { get; set; }
        public string Remark { get; set; }
        public int SkipFlag { get; set; }
    }

    public class WorkFLowDeleteModel
    {
        public string FlowIds { get; set; }
    }


    public class WorkFlowAddMultipleCaseModel
    {
        public Guid FlowId { get; set; }
        public Guid EntityId { get; set; }
        public List<Guid> RecId { get; set; }
        public Guid? RelEntityId { get; set; }
        public Guid? RelRecId { get; set; }
        public Dictionary<string, object> CaseData { get; set; }
    }


    public class WorkFlowAddMultipleCaseItemModel
    {
        public List<Guid> CaseId { get; set; }
        public int NodeNum { get; set; }
        public string HandleUser { get; set; }
        public string CopyUser { get; set; }
        public string Remark { get; set; }
        public Dictionary<string, object> CaseData { get; set; }
    }
}
