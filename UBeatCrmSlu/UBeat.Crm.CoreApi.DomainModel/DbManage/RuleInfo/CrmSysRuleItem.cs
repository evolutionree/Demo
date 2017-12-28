using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage.RuleInfo
{
    public class CrmSysRuleItem
    {
        /// <summary>
        /// 规则明细ID
        /// </summary>
        public Guid ItemId { set; get; }
        /// <summary>
        /// 规则明细名称
        /// </summary>
        public string ItemName { set; get; }
        /// <summary>
        /// 字段ID
        /// </summary>
        public Guid? FieldId { set; get; }
        /// <summary>
        /// 操作符 + - *
        /// </summary>
        public string Operate { set; get; }
        /// <summary>
        /// 规则数据
        /// </summary>
        [SqlType(NpgsqlDbType.Json)]
        public object RuleData { set; get; }
        /// <summary>
        /// 规则类型 0范围规则 1字段规则 2语句规则
        /// </summary>
        public int RuleType { set; get; }
        /// <summary>
        /// 规则语句，最终生成
        /// </summary>
        public string RuleSql { set; get; }
        /// <summary>
        /// 0为实体规则 1为用户定义规则
        /// </summary>
        public int UseType { set; get; }
        /// <summary>
        /// 排序
        /// </summary>
        public int RecOrder { set; get; }

    }
}
