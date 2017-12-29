using Newtonsoft.Json;
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
        [JsonProperty("itemid")]
        public Guid ItemId { set; get; }
        /// <summary>
        /// 规则明细名称
        /// </summary>
        [JsonProperty("itemname")]
        public string ItemName { set; get; }
        /// <summary>
        /// 字段ID
        /// </summary>
        [JsonProperty("fieldid")]
        public Guid? FieldId { set; get; }
        /// <summary>
        /// 操作符 + - *
        /// </summary>
        [JsonProperty("operate")]
        public string Operate { set; get; }
        /// <summary>
        /// 规则数据
        /// </summary>
        [SqlType(NpgsqlDbType.Json)]
        [JsonProperty("ruledata")]
        public object RuleData { set; get; }
        /// <summary>
        /// 规则类型 0范围规则 1字段规则 2语句规则
        /// </summary>
        [JsonProperty("ruletype")]
        public int RuleType { set; get; }
        /// <summary>
        /// 规则语句，最终生成
        /// </summary>
        [JsonProperty("rulesql")]
        public string RuleSql { set; get; }
        /// <summary>
        /// 0为实体规则 1为用户定义规则
        /// </summary>
        [JsonProperty("usetype")]
        public int UseType { set; get; }
        /// <summary>
        /// 排序
        /// </summary>
        [JsonProperty("recorder")]
        public int RecOrder { set; get; }

    }
}
