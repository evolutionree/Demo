using FluentValidation;
using FluentValidation.Results;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.DomainMapper.Rule
{
    public class RuleListMapper : BaseEntity
    {
        public string EntityId { get; set; }
        protected override IValidator GetValidator()
        {
            return new RuleListMapperValidator();
        }
        class RuleListMapperValidator : AbstractValidator<RuleListMapper>
        {
            public RuleListMapperValidator()
            {
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("实体Id不能为null");
            }
        }
    }

    public class RuleMapper : BaseEntity
    {
        public int typeid { get; set; }
        public string id { get; set; }
        public string roleid { get; set; }
        public string menuname { get; set; }
        public string ruleid { get; set; }
        public string rulename { get; set; }
        public string entityid { get; set; }
        public string flowid { get; set; }
        public string rulesql { get; set; }
        public Dictionary<string, string> menuname_lang { get; set; }
        public Dictionary<string,string> rulename_lang { get; set; }

        [JsonIgnore]
        public ICollection<RuleItemMapper> RuleItems { get; set; }
        [JsonIgnore]
        public RuleSetMapper RuleSet { get; set; }

        protected override IValidator GetValidator()
        {
            return new RuleMapperValidator();
        }
        class RuleMapperValidator : AbstractValidator<RuleMapper>
        {
            public RuleMapperValidator()
            {
                Custom((d, validationContext) =>
                {
                    switch (d.typeid)
                    {
                        case 0:
                            if (String.IsNullOrEmpty(d.roleid))
                                return new ValidationFailure("roleid", "角色Id不能为空", d.roleid);
                            if (String.IsNullOrEmpty(d.entityid))
                                return new ValidationFailure("entityid", "实体Id不能为空", d.entityid);
                            break;
                        case 1:
                            if (String.IsNullOrEmpty(d.rulename))
                                return new ValidationFailure("roleid", "规则筛选名称不能为空", d.rulename);
                            if (String.IsNullOrEmpty(d.entityid))
                                return new ValidationFailure("entityid", "实体Id不能为空", d.entityid);
                            break;
                        case 2:
                            if (String.IsNullOrEmpty(d.entityid))
                                return new ValidationFailure("entityid", "实体Id不能为空", d.entityid);
                            break;
                    }
                    return null;
                });
            }
        }
    }

    public class RuleItemMapper : BaseEntity
    {
        public string itemid { get; set; }
        public int controltype { get; set; }
        public string itemname { get; set; }
        public string entityid { get; set; }
        public string fieldid { get; set; }
        public string operate { get; set; }

        public string ruledata { get; set; }
        public int ruletype { get; set; }
        public string rulesql { get; set; }
        public int usetype { get; set; }
        [JsonIgnore]
        public RuleItemRelationMapper Relation { get; set; }
        protected override IValidator GetValidator()
        {
            return new RuleItemMapperValidator();
        }
        class RuleItemMapperValidator : AbstractValidator<RuleItemMapper>
        {
            public RuleItemMapperValidator()
            {
                Custom((d, validationContext) =>
                {
                    switch (d.ruletype)
                    {
                        case 0:
                            if (String.IsNullOrEmpty(d.operate))
                                return new ValidationFailure("operate", "运算符不能为空", d.operate);
                            if (String.IsNullOrEmpty(d.fieldid))
                                return new ValidationFailure("fieldid", "字段Id不能为空", d.fieldid);
                            var data = JObject.Parse(d.ruledata);
                            string tmpStr = data["dataVal"].ToString();
                            if (String.IsNullOrEmpty(tmpStr))
                                return new ValidationFailure("ruledata", "规则值不能为空", tmpStr);
                            break;
                        case 1:
                            if (String.IsNullOrEmpty(d.operate))
                                return new ValidationFailure("operate", "运算符不能为空", d.operate);
                            if (String.IsNullOrEmpty(d.fieldid))
                                return new ValidationFailure("fieldid", "字段Id不能为空", d.fieldid);
                            var data_1 = JObject.Parse(d.ruledata);
                            object tmpStr_1 = data_1["dataVal"] ?? string.Empty;
                            if (String.IsNullOrEmpty(tmpStr_1.ToString()))
                                return new ValidationFailure("ruledata", "规则值不能为空", tmpStr_1);
                            break;
                        case 2:
                            var data_2 = JObject.Parse(d.ruledata);
                            string tmpStr_2 = data_2["dataVal"].ToString();
                            if (String.IsNullOrEmpty(tmpStr_2))
                                return new ValidationFailure("ruledata", "规则值不能为空", tmpStr_2);
                            break;
                    }
                    return null;
                });
            }
        }
    }


    public class MenuRuleMapper : BaseEntity
    {
        public string menuid { get; set; }
        public string menuname { get; set; }
        public string ruleid { get; set; }
        public string rulename { get; set; }
        public string entityid { get; set; }
        public string rulesql { get; set; }

        [JsonIgnore]
        public ICollection<RuleItemMapper> RuleItems { get; set; }
        [JsonIgnore]
        public RuleSetMapper RuleSet { get; set; }

        protected override IValidator GetValidator()
        {
            return new MenuRuleMapperValidator();
        }
        class MenuRuleMapperValidator : AbstractValidator<MenuRuleMapper>
        {
            public MenuRuleMapperValidator()
            {
                RuleFor(d => d.menuid).NotEmpty().WithMessage("菜单Id不能为空");
            }
        }
    }

    public class RoleRuleMapper : BaseEntity
    {
        public string roleid { get; set; }
        public string ruleid { get; set; }
        public string rulename { get; set; }
        public string entityid { get; set; }
        public string rulesql { get; set; }

        [JsonIgnore]
        public ICollection<RuleItemMapper> RuleItems { get; set; }
        [JsonIgnore]
        public RuleSetMapper RuleSet { get; set; }

        protected override IValidator GetValidator()
        {
            return new RoleRuleMapperValidator();
        }
        class RoleRuleMapperValidator : AbstractValidator<RoleRuleMapper>
        {
            public RoleRuleMapperValidator()
            {
                RuleFor(d => d.roleid).NotEmpty().WithMessage("角色Id不能为空");
                RuleFor(d => d.entityid).NotEmpty().WithMessage("实体Id不能为空");

            }
        }
    }
    public class DynamicRuleMapper : BaseEntity
    {
        public string ruleid { get; set; }
        public string rulename { get; set; }
        public string entityid { get; set; }
        public string rulesql { get; set; }

        [JsonIgnore]
        public ICollection<RuleItemMapper> RuleItems { get; set; }
        [JsonIgnore]
        public RuleSetMapper RuleSet { get; set; }

        protected override IValidator GetValidator()
        {
            return new DynamicRuleMapperValidator();
        }
        class DynamicRuleMapperValidator : AbstractValidator<DynamicRuleMapper>
        {
            public DynamicRuleMapperValidator()
            {
                RuleFor(d => d.entityid).NotNull().NotEmpty().WithMessage("实体Id不能为空");

            }
        }
    }

    public class FlowRuleMapper : BaseEntity
    {
        public string ruleid { get; set; }
        public string rulename { get; set; }
        public string flowid { get; set; }
        public string rulesql { get; set; }

        [JsonIgnore]
        public ICollection<RuleItemMapper> RuleItems { get; set; }
        [JsonIgnore]
        public RuleSetMapper RuleSet { get; set; }

        protected override IValidator GetValidator()
        {
            return new FlowRuleMapperValidator();
        }
        class FlowRuleMapperValidator : AbstractValidator<FlowRuleMapper>
        {
            public FlowRuleMapperValidator()
            {
                RuleFor(d => d.flowid).NotNull().NotEmpty().WithMessage("流程Id不能为空");

            }
        }
    }

    public class RuleItemRelationMapper
    {
        public string itemid { get; set; }
        public string ruleid { get; set; }
        public int userid { get; set; }
        public int rolesub { get; set; }
        public int paramindex { get; set; }
    }

    public class RuleSetMapper : BaseEntity
    {
        public string ruleid { get; set; }
        public string ruleset { get; set; }
        public int userid { get; set; }
        public string ruleformat { get; set; }

        protected override IValidator GetValidator()
        {
            return new RulseSetMapperValidator();
        }
        class RulseSetMapperValidator : AbstractValidator<RuleSetMapper>
        {
            public RulseSetMapperValidator()
            {
                RuleFor(d => d.ruleset).NotEmpty().WithMessage("规则集合不能为空");
            }
        }

    }

    public class RuleQueryMapper
    {
        public string MenuName { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public int RecStatus { get; set; }
        public string ItemId { get; set; }
        public string FieldId { get; set; }
        public string ItemName { get; set; }
        public string Operate { get; set; }
        public string RuleSql { get; set; }
        public int UseType { get; set; }
        public int RuleType { get; set; }
        public string RuleData { get; set; }
        public string RuleSet { get; set; }
        public string MenuName_Lang { get; set;}
    }
    public class RoleRuleQueryMapper
    {
        public string EntityId { set; get; }
        public string ItemIdEntityId { set; get; }
        public string Roleid { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public int RecStatus { get; set; }
        public string ItemId { get; set; }
        public string FieldId { get; set; }
        public string ItemName { get; set; }
        public string Operate { get; set; }
        public string RuleSql { get; set; }
        public int UseType { get; set; }
        public int RuleType { get; set; }
        public string RuleData { get; set; }
        public string RuleSet { get; set; }
    }

    public class DynamicRuleQueryMapper
    {
        public string EntityId { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public int RecStatus { get; set; }
        public string ItemId { get; set; }
        public string FieldId { get; set; }
        public string ItemName { get; set; }
        public string Operate { get; set; }
        public string RuleSql { get; set; }
        public int UseType { get; set; }
        public int RuleType { get; set; }
        public string RuleData { get; set; }
        public string RuleSet { get; set; }
    }

    public class WorkFlowRuleQueryMapper
    {
        public string FlowId { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public int RecStatus { get; set; }
        public string ItemId { get; set; }
        public string FieldId { get; set; }
        public string ItemName { get; set; }
        public string Operate { get; set; }
        public string RuleSql { get; set; }
        public int UseType { get; set; }
        public int RuleType { get; set; }
        public string RuleData { get; set; }
        public string RuleSet { get; set; }
    }
}
