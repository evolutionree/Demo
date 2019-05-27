using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using UBeat.Crm.CoreApi.DomainModel.Utility;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class EntityProQueryMapper
    {
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int Status { get; set; }

        public string EntityName { get; set; }

        public int TypeId { get; set; }
        /// <summary>
        /// 实体类型ids，typeid主要兼容原接口，建议传入-1
        /// </summary>
        public string TypeIds { get; set; }

    }

    public class EntityProInfoMapper : BaseEntity
    {
        public string EntityId { get; set; }
        protected override IValidator GetValidator()
        {
            return new EntityProInfoMapperValidator();
        }

        class EntityProInfoMapperValidator : AbstractValidator<EntityProInfoMapper>
        {
            public EntityProInfoMapperValidator()
            {
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("实体Id不能为空");
            }
        }
    }
    public class EntityProSaveMapper : BaseEntity
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string EntityTable { get; set; }
        public int TypeId { get; set; }
        public string Remark { get; set; }
        public string Styles { get; set; }
        public string Icons { get; set; }

        public string RelEntityId { get; set; }
        public int Relaudit { get; set; }
        public int RecStatus { get; set; }

        public int RecOrder { get; set; }

        public Guid RelFieldId { get; set; }

        public Dictionary<string,string> EntityName_Lang { get; set; }
        public Dictionary<string, string> ServicesJson { get; set; }


        protected override IValidator GetValidator()
        {
            return new EntityProSaveMapperValidator();
        }

        class EntityProSaveMapperValidator : AbstractValidator<EntityProSaveMapper>
        {
            public EntityProSaveMapperValidator()
            {
                RuleFor(d => d.EntityName).NotEmpty().WithMessage("实体名称不能为空");
                RuleFor(d => d.EntityTable).NotEmpty().WithMessage("实体表名不能为空");
                RuleFor(d => d.RelEntityId).NotEmpty().WithMessage("关联实体不能为空");
                RuleFor(d => d).Must(Valid).WithMessage("关联实体字段不能为空");

            }

            bool Valid(EntityProSaveMapper entity)
            {
                if (!string.IsNullOrEmpty(entity.RelEntityId) && entity.TypeId == 3)
                {
                    if (entity.RelFieldId != null || entity.RelFieldId != Guid.Empty)
                        return true;
                    return false;
                }
                return true;
            }
        }
    }

    public class OrderByEntityProMapper : BaseEntity
    {
        public string EntityIds { get; set; }
        protected override IValidator GetValidator()
        {
            return new OrderByEntityProMapperValidator();
        }

        class OrderByEntityProMapperValidator : AbstractValidator<OrderByEntityProMapper>
        {
            public OrderByEntityProMapperValidator()
            {
                RuleFor(d => d.EntityIds).NotEmpty().WithMessage("实体Id不能为空");
            }
        }
    }
    public class EntityProMapper
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string EntityTable { get; set; }
        public int TypeId { get; set; }
        public string Remark { get; set; }
        public string Styles { get; set; }
        public string Icons { get; set; }

        public string RelEntityId { get; set; }
        public int Relaudit { get; set; }
        public int RecStatus { get; set; }

        public int RecOrder { get; set; }
    }


    public class EntityOrderbyMapper
    {
        public int ModelType { get; set; }

        public string RelEntityId { get; set; }
    }

    public class EntityFieldProSaveMapper : BaseEntity
    {
        public string FieldId { get; set; }
        public string FieldName { get; set; }
        public string EntityId { get; set; }
        public string FieldLabel { get; set; }
        public string DisplayName { get; set; }
        public int ControlType { get; set; }
        public int FieldType { get; set; }
        public string DataSource { get; set; }

        public int RecStatus { get; set; }

        public int RecOrder { get; set; }

        public Dictionary<string, string> DisplayName_Lang { get; set; }
        public Dictionary<string, string> FieldLabel_Lang { get; set; }


        [JsonIgnore]
        public string FieldConfig { get; set; }

        protected override IValidator GetValidator()
        {
            return new EntityFieldProSaveMapperValidator();
        }
        class EntityFieldProSaveMapperValidator : AbstractValidator<EntityFieldProSaveMapper>
        {
            public EntityFieldProSaveMapperValidator()
            {
                RuleFor(d => d.DisplayName).NotEmpty().WithMessage("显示名称不能为空");
                RuleFor(d => d.FieldName).NotEmpty().WithMessage("字段列名不能为空");
            }
        }
    }
    public class EntityFieldLanguage
    {
        public Dictionary<string,string> DisplayName_Lang { get; set; }
        public Dictionary<string, string> FieldLabel_Lang { get; set; }
    }

    public class DeleteEntityDataMapper : BaseEntity
    {
        public string EntityId { get; set; }
        [JsonIgnore]
        public string FieldConfig { get; set; }

        protected override IValidator GetValidator()
        {
            return new DeleteEntityDataMapperValidator();
        }
        class DeleteEntityDataMapperValidator : AbstractValidator<DeleteEntityDataMapper>
        {
            public DeleteEntityDataMapperValidator()
            {
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("实体Id不能为空");
            }
        }
    }

    public class EntityFieldExpandJSDataMapper : BaseEntity
    {
        public string FieldId { get; set; }
        public string ExpandJS { get; set; }

        protected override IValidator GetValidator()
        {
            return new EntityFieldExpandJSDataMapperValidator();
        }
        class EntityFieldExpandJSDataMapperValidator : AbstractValidator<EntityFieldExpandJSDataMapper>
        {
            public EntityFieldExpandJSDataMapperValidator()
            {
                RuleFor(d => d.FieldId).NotEmpty().WithMessage("字段Id不能为空");
            }
        }
    }

    public class EntityFieldFilterJSDataMapper : BaseEntity
    {
        public string FieldId { get; set; }
        public string FilterJS { get; set; }

        protected override IValidator GetValidator()
        {
            return new EntityFieldFilterJSDataMapperValidator();
        }
        class EntityFieldFilterJSDataMapperValidator : AbstractValidator<EntityFieldFilterJSDataMapper>
        {
            public EntityFieldFilterJSDataMapperValidator()
            {
                RuleFor(d => d.FieldId).NotEmpty().WithMessage("字段Id不能为空");
            }
        }
    }

    public class EntityFieldProMapper
    {
        public string FieldId { get; set; }
        public string FieldName { get; set; }
        public string EntityId { get; set; }
        public string FieldLabel { get; set; }
        public string DisplayName { get; set; }
        public int ControlType { get; set; }
        public int FieldType { get; set; }
        public string DataSource { get; set; }

        public int RecStatus { get; set; }

        public int RecOrder { get; set; }

        
        public string FieldConfig { get; set; }


    }

    public class SimpleSearchMapper
    {
        public int viewtype { get; set; }
        public string entityid { get; set; }
        public string searchfield { get; set; }
        public int iscomquery { get; set; }

        [JsonIgnore]
        public ICollection<AdvanceSearchMapper> AdvanceSearch { get; set; }

    }
    public class AdvanceSearchMapper
    {
        public string entityid { get; set; }

        public int islike { get; set; }
        public string fieldid { get; set; }

        public int controltype { get; set; }
        public int recorder { get; set; }
    }
    public class EntityTypeQueryMapper
    {
        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string EntityId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }
    public class SaveEntityTypeMapper : BaseEntity
    {
        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string EntityId { get; set; }

        public int RecOrder { get; set; }

        public int RecStatus { get; set; }

        public Dictionary<string,string> CategoryName_Lang { get; set; }
        protected override IValidator GetValidator()
        {
            return new SaveEntityTypeMapperValidator();
        }

        class SaveEntityTypeMapperValidator : AbstractValidator<SaveEntityTypeMapper>
        {
            public SaveEntityTypeMapperValidator()
            {
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("实体Id不能为空");
                RuleFor(d => d.CategoryName).NotEmpty().WithMessage("实体类型不能为空");
            }
        }

    }
    public class EntityTypeMapper
    {
        public string CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string EntityId { get; set; }

        public int RecOrder { get; set; }

        public int RecStatus { get; set; }

    }

    public class EntityFieldRulesMapper
    {
        public int RowNum { get; set; }
        public string FieldLabel { get; set; }
        public string FieldId { get; set; }
        public string TypeId { get; set; }
        public string FieldRulesId { get; set; }
        public string ViewRules { get; set; }
        public string ValidRules { get; set; }
        public int OperateType { get; set; }
        public int RecStatus { get; set; }

        public int IsVisible { get; set; }

        public int IsRequire { get; set; }

        public int IsReadOnly { get; set; }
    }
    public class EntityFieldRulesVocationMapper
    {
        public string FieldLabel { get; set; }
        public string FieldId { get; set; }
        public string EntityId { get; set; }
        public string VocationId { get; set; }
        public string FieldRulesId { get; set; }
        public int IsVisible { get; set; }
        public int IsReadOnly { get; set; }
        public int OperateType { get; set; }
        public int RecStatus { get; set; }
    }
    public class EntityFieldRulesSaveMapper
    {
        public EntityFieldRulesSaveMapper()
        {
            Rules = new List<FieldRulesDetailMapper>();
        }
        public string FieldLabel { get; set; }
        public string FieldId { get; set; }

        public string TypeId { get; set; }
        public ICollection<FieldRulesDetailMapper> Rules { get; set; }
        public int RecStatus { get; set; }
    }

    public class FieldRulesDetailMapper
    {
        public string FieldRulesId { get; set; }

        public int OperateType { get; set; }

        public int IsVisible { get; set; }

        public int IsRequired { get; set; }

        public int IsReadOnly { get; set; }

        public JObject ViewRule { get; set; }
        public JObject ValidRule { get; set; }

        [JsonIgnore]
        public string ViewRuleStr { get; set; }
        [JsonIgnore]
        public string ValidRuleStr { get; set; }

        public int UserId { get; set; }

    }

    public class EntityFieldRulesVocationSaveMapper
    {
        public EntityFieldRulesVocationSaveMapper()
        {
            Rules = new List<FieldRulesVocationDetailMapper>();
        }
        public string VocationId { set; get; }
        public string FieldId { get; set; }
        public string FieldLabel { get; set; }

        public ICollection<FieldRulesVocationDetailMapper> Rules { get; set; }
        public int RecStatus { get; set; }
    }

    public class FieldRulesVocationDetailMapper
    {
        [JsonIgnore]
        public string FieldRulesId { get; set; }
        [JsonIgnore]
        public int OperateType { get; set; }
        [JsonProperty("isVisible")]
        public int IsVisible { get; set; }
        [JsonProperty("isReadOnly")]
        public int IsReadOnly { get; set; }

        public string ViewRules() { return JsonHelper.ToJson(this); }

    }

    public class EntityFieldRulesVocationListMapper
    {
        public EntityFieldRulesVocationListMapper()
        {
            Rules = new List<FieldRulesVocationInfoMapper>();
        }
        public string VocationId { set; get; }
        public string FieldId { get; set; }
        public string FieldLabel { get; set; }

        public ICollection<FieldRulesVocationInfoMapper> Rules { get; set; }
        public int RecStatus { get; set; }
    }
    public class FieldRulesVocationInfoMapper
    {
        public string FieldRulesId { get; set; }
        public int OperateType { get; set; }
        public int IsVisible { get; set; }
        public int IsReadOnly { get; set; }

        public string ViewRules() { return JsonHelper.ToJson(this); }

    }

    public class ListViewMapper
    {
        public string ViewConfId { get; set; }
        public int ViewStyleId { get; set; }
        public string EntityId { get; set; }
        public string FieldKeys { get; set; }
        public string Fonts { get; set; }
        public string Colors { get; set; }
        public int ViewType { get; set; }

        public string FieldIds { get; set; }
    }

    public class SaveListViewColumnMapper : BaseEntity
    {
        public string EntityId { get; set; }
        public string FieldIds { get; set; }
        public int ViewType { get; set; }
        protected override IValidator GetValidator()
        {
            return new SaveListViewColumnMapperValidator();
        }

        class SaveListViewColumnMapperValidator : AbstractValidator<SaveListViewColumnMapper>
        {
            public SaveListViewColumnMapperValidator()
            {
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("实体Id不能为null");
            }
        }
    }

    public class EntityPageConfigMapper
    {
        public string EntityId { get; set; }
        public string TitlefieldId { get; set; }
        public string TitlefieldName { get; set; }
        public string SubfieldIds { get; set; }
        public string SubfieldNames { get; set; }
        public string Modules { get; set; }
        public string RelentityId { get; set; }
        public string RelfieldId { get; set; }
        public string RelfieldName { get; set; }


    }

    public class SetRepeatMapper
    {
        public string EntityId { get; set; }
    }
    public class SaveSetRepeatMapper
    {
        public string EntityId { get; set; }

        public string Fieldids { get; set; }
    }

    public class SaveEntranceGroupMapper
    {
        public string entranceid { get; set; }
        public string entryname { get; set; }
        public int entrytype { get; set; }
        public string entityid { get; set; }
        public int isgroup { get; set; }
        public int recorder { get; set; }
        public int IsDefaultGroup { get; set; }

    }

    public class RelControlValueMapper : BaseEntity
    {
        public string RecId { get; set; }
        public string EntityId { get; set; }

        public string FieldId { get; set; }
        protected override IValidator GetValidator()
        {
            return new RelControlValueMapperValidator();
        }

        class RelControlValueMapperValidator : AbstractValidator<RelControlValueMapper>
        {
            public RelControlValueMapperValidator()
            {
                RuleFor(d => d.RecId).NotEmpty().WithMessage("实体主键Id值不能为null");
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("实体Id不能为null");
                RuleFor(d => d.FieldId).NotEmpty().WithMessage("字段Id不能为null");
            }
        }
    }

    /// <summary>
    /// 个人列表字段设置
    /// </summary>

    public class PersonalViewSetMapper : BaseEntity
    {
        /// <summary>
        /// userid
        /// </summary>

        public int UserId { get; set; }
        /// <summary>
        /// 字段所属实体Id
        /// </summary>

        public Guid EntityId { get; set; }
        /// <summary>
        /// 字段Id
        /// </summary>

        public Guid FieldId { get; set; }
        /// <summary>
        /// 排序号
        /// </summary>
        public int RecOrder { get; set; }

        protected override IValidator GetValidator()
        {
            return new PersonalViewSetModelValidator();
        }

        class PersonalViewSetModelValidator : AbstractValidator<PersonalViewSetMapper>
        {
            public PersonalViewSetModelValidator()
            {
                RuleFor(d => d.EntityId).NotEmpty().WithMessage("实体Id不能为null");
                RuleFor(d => d.FieldId).NotEmpty().WithMessage("字段Id不能为null");
            }
        }
    }


    public class EntityBaseDataMapper : BaseEntity
    {
        public Guid EntityId { get; set; }

        public Guid RelEntityId { get; set; }

        public Guid FieldId { get; set; }

        public string FieldName { get; set; }

        protected override IValidator GetValidator()
        {
            return new EntityBaseDataMapperValidator();
        }
        class EntityBaseDataMapperValidator : AbstractValidator<EntityBaseDataMapper>
        {
            public EntityBaseDataMapperValidator()
            {
                RuleFor(d => d.EntityId).NotNull().WithMessage("实体Id不能为空");
                RuleFor(d => d.FieldId).NotNull().WithMessage("字段Id不能为空");
            }
        }
    }

    public class EntityBaseDataFieldMapper : BaseEntity
    {
        public Guid EntityId { get; set; }

        public Guid CommEntityId { get; set; }
        protected override IValidator GetValidator()
        {
            return new EntityBaseDataFieldMapperValidator();
        }
        class EntityBaseDataFieldMapperValidator : AbstractValidator<EntityBaseDataFieldMapper>
        {
            public EntityBaseDataFieldMapperValidator()
            {
                RuleFor(d => d.EntityId).NotNull().WithMessage("实体Id不能为空");
                RuleFor(d => d.CommEntityId).NotNull().WithMessage("基础资料Id不能为空");
            }
        }
    }

    public class EntityGlobalJsMapper : BaseEntity
    {
        public Guid EntityId { get; set; }
        public string NewLoad { get; set; }
        public string EditLoad { get; set; }
        public string CheckLoad { get; set; }
		public string CopyLoad { get; set; }
		protected override IValidator GetValidator()
        {
            return new EntityGlobalJsMapperValidator();
        }
        class EntityGlobalJsMapperValidator : AbstractValidator<EntityGlobalJsMapper>
        {
            public EntityGlobalJsMapperValidator()
            {
                RuleFor(d => d.EntityId).NotNull().WithMessage("实体Id不能为空");
            }
        }

    }
    
}
