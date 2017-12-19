using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo
{
    public class CrmSysRuleSet
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        public Guid RuleId { set; get; }
        /// <summary>
        /// 规则对应集合
        /// </summary>
        public string RuleSet { set; get; }
        /// <summary>
        /// 用户ID，如果为0则是实体的，不为0则为用户规则
        /// </summary>
        public int UserId { set; get; }
        /// <summary>
        /// 集合描述
        /// </summary>
        public string RuleFormat { set; get; }
    }
}
