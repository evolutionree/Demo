using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow
{
    public class CrmSysWorkflowComparer : IEqualityComparer<CrmSysWorkflow>
    {

        public bool Equals(CrmSysWorkflow x, CrmSysWorkflow y)
        {
            if (x.FlowId == y.FlowId)
                return true;
            else
                return false;
        }

        public int GetHashCode(CrmSysWorkflow obj)
        {
            return 0;
        }

    }

    public class CrmSysWorkflowNodeComparer : IEqualityComparer<CrmSysWorkflowNode>
    {

        public bool Equals(CrmSysWorkflowNode x, CrmSysWorkflowNode y)
        {
            if (x.NodeId == y.NodeId)
                return true;
            else
                return false;
        }

        public int GetHashCode(CrmSysWorkflowNode obj)
        {
            return 0;
        }

    }

    public class CrmSysWorkflowNodeLineComparer : IEqualityComparer<CrmSysWorkflowNodeLine>
    {

        public bool Equals(CrmSysWorkflowNodeLine x, CrmSysWorkflowNodeLine y)
        {
            if (x.LineId == y.LineId)
                return true;
            else
                return false;
        }

        public int GetHashCode(CrmSysWorkflowNodeLine obj)
        {
            return 0;
        }

    }

    public class CrmSysWorkflowFuncEventComparer : IEqualityComparer<CrmSysWorkflowFuncEvent>
    {

        public bool Equals(CrmSysWorkflowFuncEvent x, CrmSysWorkflowFuncEvent y)
        {
            if (x.FuncEventId == y.FuncEventId)
                return true;
            else
                return false;
        }

        public int GetHashCode(CrmSysWorkflowFuncEvent obj)
        {
            return 0;
        }

    }


    public class CrmSysWorkflowRuleRelationComparer : IEqualityComparer<CrmSysWorkflowRuleRelation>
    {

        public bool Equals(CrmSysWorkflowRuleRelation x, CrmSysWorkflowRuleRelation y)
        {
            if (x.FlowId == y.FlowId&&x.RuleId==y.RuleId)
                return true;
            else
                return false;
        }

        public int GetHashCode(CrmSysWorkflowRuleRelation obj)
        {
            return 0;
        }

    }
}
