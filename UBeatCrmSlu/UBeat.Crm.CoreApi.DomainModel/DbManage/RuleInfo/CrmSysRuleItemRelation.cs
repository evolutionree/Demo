using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo
{
    public class CrmSysRuleItemRelation
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        public Guid RuleId { set; get; }
        /// <summary>
        /// 规则明细ID
        /// </summary>
        public Guid ItemId { set; get; }
        /// <summary>
        /// 用户ID，0为实体规则1为个人覆盖规则
        /// </summary>
        public int UserId { set; get; }
        /// <summary>
        /// 加减规则
        /// </summary>
        public int RoleSub { set; get; }
        /// <summary>
        /// 参数顺序
        /// </summary>
        public int ParamIndex { set; get; }
    }
}
