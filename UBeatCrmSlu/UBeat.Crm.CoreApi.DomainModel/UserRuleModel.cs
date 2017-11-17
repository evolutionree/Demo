using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel
{
    /// <summary>
    /// 用户规则对象，包含了角色和职能的rule
    /// </summary>
    public class UserRuleModel
    {
        /// <summary>
        /// 用户角色关联的rule,key为roleid，value为ruleID
        /// </summary>
        public Dictionary<Guid,Guid?> UserRoles { set; get; }

        /// <summary>
        /// 用户职能关联的rule,key为vocationid，value为ruleID
        /// </summary>
        public Dictionary<Guid, Guid?> UserVocations { set; get; }

        public string GetRuleSql()
        {
            return null;
        }

    }
}
