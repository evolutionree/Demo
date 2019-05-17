using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using System.Linq;
namespace UBeat.Crm.CoreApi.DomainModel.Department
{
    public class DepartmentAddMapper : BaseEntity
    {
        public Guid PDeptId { get; set; }
        public string DeptName { get; set; }
        public int OgLevel { get; set; }
        public string DeptLanguage { get; set; } 
		public string DeptCode { get; set; }
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
        public string DeptLanguage { get; set; }
		public string DeptCode { get; set; }
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
				RuleFor(d => d.DeptCode).NotEmpty().WithMessage("部门编码不能为空");
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
			RuleFor(d => d.DeptCode).NotEmpty().WithMessage("部门编码不能为空");
		}
    }
    public class DepartMasterSlave : BaseEntity
    {
        public Guid DepartId { get; set; }

        public Guid PreDepartId { get; set; }

        public int Type { get; set; }
        public int IsMaster { get; set; }
        protected override IValidator GetValidator()
        {
            return new DepartMasterSlaveValidator();
        }
        class DepartMasterSlaveValidator : AbstractValidator<DepartMasterSlave>
        {
            public DepartMasterSlaveValidator()
            {
                RuleFor(d => d).Must(d => ValidDepart(d)).WithMessage("部门Id不能为空");
                RuleFor(d => d.Type).GreaterThanOrEqualTo(0).WithMessage("职位类型标识不能小于0");
            }

            bool ValidDepart(DepartMasterSlave depart)
            {

                if (!(depart.DepartId != null && depart.DepartId != null && depart.IsMaster >= 0))
                {
                    return false;
                }

                return true;
            }
        }
    }
    public class DepartmentPosition : BaseEntity
    {
        public int UserId { get; set; }

        public List<DepartMasterSlave> Departs { get; set; }


        protected override IValidator GetValidator()
        {
            return new DepartmentPositionValidator();
        }
        class DepartmentPositionValidator : AbstractValidator<DepartmentPosition>
        {
            public DepartmentPositionValidator()
            {
                RuleFor(d => d).Must(t => (ValidDepartments(t.Departs))).WithMessage("任职的部门不能重复,且其中只有一个是主部门");
            }
            bool ValidDepartments(List<DepartMasterSlave> departs)
            {
                if (departs.Select(t => t.DepartId).GroupBy(t => t).ToList().Count() == departs.Select(t => t.DepartId).Count())//不能同时有安排两个一样的部门 即主副部门是同一个
                {
                    if (departs.Select(t => t.IsMaster == 1).GroupBy(t => t).ToList().Count() > 1)//不能同时有两个主部门
                    {
                        return true;
                    }
                    return true;
                }
                return false;
            }
        }
    }

    public class DepartPosition : BaseEntity
    {
        public int UserId { get; set; }

        public Guid DepartId { get; set; }

        public int Type { get; set; }

        protected override IValidator GetValidator()
        {
            return new DepartPositionValidator();
        }
        class DepartPositionValidator : AbstractValidator<DepartPosition>
        {
            public DepartPositionValidator()
            {
                RuleFor(d => d.DepartId).NotNull().WithMessage("部门不能为空");
            }
        }
    }

    public class DepartListMapper
    {
        public Guid DepartId { get; set; }
        public int IsMaster { get; set; }
    }
}
