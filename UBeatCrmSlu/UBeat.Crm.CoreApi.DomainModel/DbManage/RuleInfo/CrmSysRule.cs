using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo
{
    public class CrmSysRule
    {
        public Guid RuleId { set; get; }

        public string RuleName { set; get; }

        public Guid EntityId { set; get; }

        public string RuleSql { set; get; }

    }
}
