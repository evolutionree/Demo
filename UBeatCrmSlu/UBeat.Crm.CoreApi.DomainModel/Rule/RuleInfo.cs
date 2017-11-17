using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Rule
{
    public class RuleInfo
    {

        /// <summary>
        /// 规则ID
        /// </summary>
        public Guid RuleId { set; get; }

        /// <summary>
        /// 规则名称
        /// </summary>
        public string RuleNames { set; get; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 规则生成后的语句
        /// </summary>
        public string Rulesql { set; get; }

    }
}
