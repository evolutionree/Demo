using FluentValidation;
using System;
using System.Collections.Generic;

namespace UBeat.Crm.CoreApi.DomainModel.DynamicEntity
{
    public class DynamicEntityDataFieldMapper
    {
        public Guid FieldId { get; set; }
        public string FieldName { get; set; }
        public Guid TypeId { get; set; }
        public string FieldLabel { get; set; }
        public string DisplayName { get; set; }
        public int ControlType { get; set; }
        public int FieldType { get; set; }
        public string FieldConfig { get; set; }
        public bool IsRequire { get; set; }
        public bool IsVisible { get; set; }
        public bool IsReadOnly { get; set; }
        public string DefaultValue { get; set; }

        public string ExpandJS { get; set; }
        public string FilterJS { get; set; }
    }

    public class DynamicEntityWebFieldMapper
    {
        public Guid FieldId { get; set; }
        public string FieldName { get; set; }
        public Guid TypeId { get; set; }
        public string FieldLabel { get; set; }
        public string DisplayName { get; set; }
        public int ControlType { get; set; }
        public int FieldType { get; set; }
        public string FieldConfig { get; set; }
        public bool IsRequire { get; set; }
        public bool IsVisible { get; set; }
        public bool IsReadOnly { get; set; }
        public string DefaultValue { get; set; }
    }

    public class DynamicEntityFieldSearch
    {
        public Guid EntityId { get; set; }
        public Guid FieldId { get; set; }

        public int IsLike { get; set; }
        public string FieldName { get; set; }
        public string FieldLabel { get; set; }
        public string DisplayName { get; set; }
        public int ControlType { get; set; }
        public int NewType { get; set; }

        public string FieldConfig { set; get; }
    }

    public class GeneralDicItem
    {
        public int DicTypeId { get; set; }
        public int DataId { get; set; }
        public string DataVal { get; set; }
        public int RecOrder { get; set; }

        public int Recstatus { get; set; }
    }

    public class PermissionMapper : BaseEntity
    {
        public Guid RecId { get; set; }
        public Guid EntityId { get; set; }
        public Guid RelEntityId { get; set; }
        public Guid RelRecId { get; set; }

        protected override IValidator GetValidator()
        {
            return new PermissionMapperValidator();
        }

        class PermissionMapperValidator : AbstractValidator<PermissionMapper>
        {
            public PermissionMapperValidator()
            {
                RuleFor(d => d.RecId).NotNull().WithMessage("记录Id不能为空");
                RuleFor(d => d.EntityId).NotNull().WithMessage("实体Id不能为空");

            }
        }
    }
}
