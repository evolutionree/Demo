using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Rule
{
    public class RuleListModel
    {
        public string EntityId { get; set; }
    }

    /// <summary>
    ///  规则
    /// </summary>
    public class RuleModel
    {
        public int TypeId { get; set; }//0 =代表角色 1 =代表菜单 2=代表动态实体 3=代表分支流程规则 ，4=流程可见规则，5=套打模板可见规则,6=二维码规则
        public string Id { get; set; }// 代表规则关联的某实体唯一id
        public string RoleId { get; set; }
        public string MenuName { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string EntityId { get; set; }
        public string FlowId { get; set; }
        public string RelEntityId { get; set; }
        public string Rulesql { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }
        public Dictionary<string,string> MenuLanguage { get; set; }
    }
    /// <summary>
    /// menu 规则
    /// </summary>
    public class MenuRuleModel
    {
        public string MenuId { get; set; }
        public string MenuName { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string EntityId { get; set; }
        public string Rulesql { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }
    }
    /// <summary>
    /// 角色规则
    /// </summary>
    public class RoleRuleModel
    {
        public string RoleId { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string EntityId { get; set; }
        public string Rulesql { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }
    }
    /// <summary>
    /// 动态
    /// </summary>
    public class DynamicRuleModel
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string EntityId { get; set; }
        public string Rulesql { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }
    }

    public class FlowRuleModel
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string FlowId { get; set; }
        public string Rulesql { get; set; }
        public ICollection<RuleItemModel> RuleItems { get; set; }
        public RuleSetModel RuleSet { get; set; }
    }
    public class RuleItemModel
    {
        public string ItemId { get; set; }
        public int ControlType { get; set; }
        public string ItemName { get; set; }
        public string EntityId { get; set; }
        public string FieldId { get; set; }
        public string Operate { get; set; }

        public string RuleData { get; set; }
        public int RuleType { get; set; }
        public string RuleSql { get; set; }
        public int UseType { get; set; }
        public RuleItemRelationModel Relation { get; set; }
    }

    public class RuleItemRelationModel
    {
        public string ItemId { get; set; }
        public string RuleId { get; set; }
        public int UserId { get; set; }
        public int RoleSub { get; set; }
        public int ParamIndex { get; set; }
    }

    public class RuleSetModel
    {
        public string RuleId { get; set; }
        public string RuleSet { get; set; }
        public int UserId { get; set; }
        public string RuleFormat { get; set; }
    }


    public class RuleInfoModel
    {
        public string MenuName { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string EntityId { get; set; }
        public string Rulesql { get; set; }
        public ICollection<RuleItemInfoModel> RuleItems { get; set; }
        public RuleSetInfoModel RuleSet { get; set; }
    }

    public class RoleRuleInfoModel
    {
        public string FieldName { get; set; }
        public string BizDateFieldName { get; set; }
        public int CaculateType { get; set; }

        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string EntityId { get; set; }
        public string Rulesql { get; set; }

        public ICollection<RuleItemInfoModel> RuleItems { get; set; }
        public RuleSetInfoModel RuleSet { get; set; }
    }

    public class DynamicRuleInfoModel
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string EntityId { get; set; }
        public string Rulesql { get; set; }
        public ICollection<RuleItemInfoModel> RuleItems { get; set; }
        public RuleSetInfoModel RuleSet { get; set; }
    }
    public class RuleItemInfoModel
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
        public RuleItemRelationInfoModel Relation { get; set; }
    }

    public class RuleItemRelationInfoModel
    {
        public string ItemId { get; set; }
        public string RuleId { get; set; }
        public int UserId { get; set; }
        public int RoleSub { get; set; }
        public int ParamIndex { get; set; }
    }

    public class RuleSetInfoModel
    {
        public string RuleId { get; set; }
        public string RuleSet { get; set; }
        public int UserId { get; set; }
        public string RuleFormat { get; set; }
    }


    public class GetRuleInfoModel
    {
        public string RuleId { get; set; }
    }

    public class EntityMenuOrderByModel {
        public Guid MenuId { get; set; }
        public int OrderBy { get; set; }
    }


}
