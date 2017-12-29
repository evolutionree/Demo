using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    /// <summary>
    /// 数据库流程审批信息对象
    /// </summary>
    public class DbWorkFlowInfo
    {
        public CrmSysWorkflow WorkFlow { set; get; }

        public List<CrmSysWorkflowNode> Nodes { set; get; }

        public List<CrmSysWorkflowNodeLine> NodeLines { set; get; }

        public List<CrmSysWorkflowFuncEvent> FuncEvents { set; get; }

        public List<CrmSysWorkflowRuleRelation> WorkFlowRuleRelations { set; get; }

        public List<DbRuleInfo> RuleInfos { set; get; }
    }

   
}
