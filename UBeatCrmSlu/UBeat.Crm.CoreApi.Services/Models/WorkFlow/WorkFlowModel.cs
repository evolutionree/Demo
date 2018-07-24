using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Rule;
using UBeat.Crm.CoreApi.Services.Models.Vocation;

namespace UBeat.Crm.CoreApi.Services.Models.WorkFlow
{
    public class WorkFlowCaseAddModel
    {
        /// <summary>
        /// 数据类型：0=实体数据，1=流程数据
        /// </summary>
        public int DataType { set; get; }

        public DynamicEntityAddModel EntityModel { set; get; }

        public WorkFlowAddCaseModel CaseModel { set; get; }

        public Guid? NodeId { set; get; }
        public string HandleUser { get; set; }
        public string CopyUser { get; set; }
        public string CacheId { get; set; }

    }

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
        public Guid? NodeId { set; get; }
        public string HandleUser { get; set; }
        public string CopyUser { get; set; }
    }

    public class GetFreeFlowEventModel
    {
        public Guid FlowId { get; set; }
       

    }
    public class FreeFlowEventModel
    {
        public Guid FlowId { get; set; }
        /// <summary>
        /// 自由流程第一个节点的触发函数
        /// </summary>
        public string BeginNodeFunc { set; get; }
        /// <summary>
        /// 自由流程最后一个节点触发的函数
        /// </summary>
        public string EndNodeFunc { set; get; }

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

        public string NodeEvent { set; get; }
        /// <summary>
        /// 节点配置数据，如位置坐标
        /// </summary>
        public object NodeConfig { set; get; }
    }

    public class WorkFlowLineModel
    {
        public int FromNode { get; set; }//新设计不使用，保留为了暂时兼容
        public int EndNode { get; set; }//新设计不使用，保留为了暂时兼容
        public Guid FromNodeId { get; set; }
        public Guid ToNodeId { get; set; }
        public Guid? RuleId { get; set; }

        /// <summary>
        /// 节点连线配置数据，如位置坐标
        /// </summary>
        public object LineConfig { set; get; }
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

    public class WorkFlowTitleConfigModel {
        public Guid FlowId { get; set; }
        public string TitleConfig { get; set; }
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
        public string FlowLanguage { get; set; }
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
        public string FlowLanguage { get; set; }
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
    public class WorkFlowRuleQueryParamInfo {
        public Guid FlowId { get; set; }
    }
    public class WorkFlowRuleSaveParamInfo {
        public Guid WorkflowId { get; set; }
        public Guid EntityId { get; set; }

        public RuleContent Rule { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }
    }
    public class WorkflowIdByEntityIdParamInfo
    {
        public Guid EntityId { get; set; }
    }
}
