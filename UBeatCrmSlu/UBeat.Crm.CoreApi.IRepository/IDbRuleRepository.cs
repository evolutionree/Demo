using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDbRuleRepository
    {
        List<DbRuleInfo> GetRuleInfoList(List<Guid> ruleids, DbTransaction trans = null);

        void SaveRuleInfoList(List<DbRuleInfo> ruleInfos, int userNum, DbTransaction trans = null);
    }
}
