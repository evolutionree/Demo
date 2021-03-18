using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Rule
{
    public class RuleDataInfo
    {
        public string FieldName { get; set; }
        public string BizDateFieldName { get; set; }
        public int CaculateType { get; set; }

        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string EntityId { get; set; }
        public string Rulesql { get; set; }

        public ICollection<RuleItemInfo> RuleItems { get; set; }
        public RuleSetInfo RuleSet { get; set; }

    }


    public class RuleItemInfo
    {
        public string ItemId { get; set; }
        public int ControlType { get; set; }
        public string ItemName { get; set; }
        public string EntityId { get; set; }
        public string FieldId { get; set; }
        public string Operate { get; set; }
        public string RuleData { get; set; }
        public int RuleType { get; set; }
        public int UseType { get; set; }
        public string RuleSql { get; set; }
        public RuleItemRelationInfo Relation { get; set; }
    }
    public class RuleItemRelationInfo
    {
        public string ItemId { get; set; }
        public string RuleId { get; set; }
        public int UserId { get; set; }
        public int RoleSub { get; set; }
        public int ParamIndex { get; set; }
    }

    public class RuleSetInfo
    {
        public string RuleId { get; set; }
        public string RuleSet { get; set; }
        public int UserId { get; set; }
        public string RuleFormat { get; set; }
    }
    public class EntityRule
    {
        public Guid EntityId { get; set; }
        public Guid PageId { get; set; }
        public RuleContent Rule { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }
    }
    public class RuleContent
    {
        public Guid EntityId { get; set; }
        public string RuleName { get; set; }
        public string RuleSql { get; set; }
        public Guid? RuleId { get; set; }
    }
}
