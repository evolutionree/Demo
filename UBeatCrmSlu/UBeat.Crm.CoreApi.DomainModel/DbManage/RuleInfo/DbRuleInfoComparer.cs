using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo
{
    public class DbRuleInfoComparer : IEqualityComparer<DbRuleInfo>
    {

        public bool Equals(DbRuleInfo x, DbRuleInfo y)
        {
            if (x.RuleInfo != null && y.RuleInfo != null && x.RuleInfo.RuleId == y.RuleInfo.RuleId)
                return true;
            else
                return false;
        }

        public int GetHashCode(DbRuleInfo obj)
        {
            return 0;
        }

    }

    public class CrmSysRuleComparer : IEqualityComparer<CrmSysRule>
    {

        public bool Equals(CrmSysRule x, CrmSysRule y)
        {
            if (x.RuleId == y.RuleId)
                return true;
            else
                return false;
        }

        public int GetHashCode(CrmSysRule obj)
        {
            return 0;
        }

    }

    public class CrmSysRuleItemComparer : IEqualityComparer<CrmSysRuleItem>
    {

        public bool Equals(CrmSysRuleItem x, CrmSysRuleItem y)
        {
            if (x.ItemId == y.ItemId)
                return true;
            else
                return false;
        }

        public int GetHashCode(CrmSysRuleItem obj)
        {
            return 0;
        }

    }

    public class CrmSysRuleItemRelationComparer : IEqualityComparer<CrmSysRuleItemRelation>
    {

        public bool Equals(CrmSysRuleItemRelation x, CrmSysRuleItemRelation y)
        {
            if (x.RuleId == y.RuleId && x.ItemId == y.ItemId && x.UserId == y.UserId)
                return true;
            else
                return false;
        }

        public int GetHashCode(CrmSysRuleItemRelation obj)
        {
            return 0;
        }

    }

    public class CrmSysRuleSetComparer : IEqualityComparer<CrmSysRuleSet>
    {

        public bool Equals(CrmSysRuleSet x, CrmSysRuleSet y)
        {
            if (x.RuleId == y.RuleId && x.RuleSet == y.RuleSet && x.UserId == y.UserId)
                return true;
            else
                return false;
        }

        public int GetHashCode(CrmSysRuleSet obj)
        {
            return 0;
        }

    }
}
