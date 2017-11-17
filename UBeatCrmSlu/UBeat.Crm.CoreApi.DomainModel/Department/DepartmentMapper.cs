using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Department
{
    public class DepartmentAddMapper:BaseEntity
    {
        public Guid PDeptId { get; set; }
        public string DeptName { get; set; }
        public int OgLevel { get; set; }
        protected override IValidator GetValidator()
        {
            return new DepartmentAddMapperValidator();
        }
    }

    public class DepartmentAddMapperValidator : AbstractValidator<DepartmentAddMapper>
    {
        public DepartmentAddMapperValidator()
        {
            RuleFor(d => d.PDeptId).NotNull().WithMessage("父级部门不能为空");
            RuleFor(d => d.DeptName).NotEmpty().WithMessage("部门名称不能为空");
            RuleFor(d => d.OgLevel).NotNull().WithMessage("部门类型不能为空");
        }
    }

    public class DepartmentEditMapper : BaseEntity
    {
        public Guid DeptId { get; set; }
        public string DeptName { get; set; }
        public Guid PDeptId { get; set; }
        public int OgLevel { get; set; }
        protected override IValidator GetValidator()
        {
            return new DepartmentEditMapperValidator();
        }
          class DepartmentEditMapperValidator : AbstractValidator<DepartmentEditMapper>
        {
            public DepartmentEditMapperValidator()
            {
                RuleFor(d => d.DeptId).NotNull().WithMessage("部门Id不能为空");
                RuleFor(d => d.DeptName).NotEmpty().WithMessage("部门名称不能为空");
                RuleFor(d => d.PDeptId).NotNull().WithMessage("父级部门不能为空");
            }
        }
    }

    public class DepartmentEditMapperValidator : AbstractValidator<DepartmentEditMapper>
    {
        public DepartmentEditMapperValidator()
        {
            RuleFor(d => d.DeptId).NotNull().WithMessage("部门ID不能为空");
            RuleFor(d => d.DeptName).NotEmpty().WithMessage("部门名称不能为空");
            RuleFor(d => d.OgLevel).NotNull().WithMessage("部门类型不能为空");
        }
    }
}
