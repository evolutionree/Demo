using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo
{
    public class DbRuleInfo
    {
        public CrmSysRule RuleInfo { set; get; }

        public List<CrmSysRuleItem> RuleItems { set; get; }

        public List<CrmSysRuleItemRelation> RuleItemRelations { set; get; }

        public List<CrmSysRuleSet> RuleSet { set; get; }
    }
}
