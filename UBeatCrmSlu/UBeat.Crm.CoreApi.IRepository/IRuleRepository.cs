using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainMapper.Rule;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Rule;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IRuleRepository
    {
        List<RuleQueryMapper> MenuRuleInfoQuery(string entityId, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> MenuRuleQuery(string entityId, int userNumber);
        OperateResult SaveRule(string ruleId, int typeId, string rule, string ruleItem, string ruleSet, string ruleRelation, int userId);

        List<RuleDataInfo> GetRule(Guid ruleid, int userNumber, DbTransaction tran = null);

        List<RoleRuleQueryMapper> RoleRuleInfoQuery(string roleId, string entityId, int userNumber);
        List<DynamicRuleQueryMapper> DynamicRuleInfoQuery(string entityId, int userNumber);
        List<WorkFlowRuleQueryMapper> WorkFlowRuleInfoQuery(string flowid, int userNumber);

        OperateResult DisabledEntityRule(string menuId, int userId);

        /// <summary>
        /// 判断是否有数据权限
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="ruleSql"></param>
        /// <param name="entityid"></param>
        /// <param name="recids"></param>
        /// <returns></returns>
        bool HasDataAccess(DbTransaction tran, string ruleSql, Guid entityid, List<Guid> recids, string recidFieldName = "recid");
        /// <summary>
        /// 单纯保存Rule信息，其他不保存
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="rule"></param>
        /// <param name="ruleItem"></param>
        /// <param name="ruleSet"></param>
        /// <returns></returns>
        OperateResult SaveRuleWithoutRelation(string Id, string rule, string ruleItem, string ruleSet, int userId);


    }
}

