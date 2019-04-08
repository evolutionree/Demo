using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Vocation
{
    public class VocationAdd : BaseEntity
    {
        public string VocationName { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> VocationName_Lang { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<VocationAdd>
        {
            public Validator()
            {
                RuleFor(d => d.VocationName).NotNull().WithMessage("职能名称不能为空");
            }
        }
    }
    public class CopyVocationAdd : BaseEntity
    {

        public Guid VocationId { get; set; }

        public string VocationName { get; set; }

        public string Description { get; set; }

        public string VocationLanguage { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<CopyVocationAdd>
        {
            public Validator()
            {
                RuleFor(d => d.VocationId).NotNull().NotEmpty().WithMessage("职能Id不能为空");
            }
        }
    }


    public class VocationEdit : BaseEntity
    {
        public Guid VocationId { get; set; }

        public string VocationName { get; set; }
        public string Description { get; set; }

        public bool IsCopy { get; set; }
        public Dictionary<string,string> VocationName_Lang { get; set; }



        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<VocationEdit>
        {
            public Validator()
            {
                RuleFor(d => d.VocationName).NotNull().WithMessage("职能名称不能为空");
            }
        }
    }

    public class VocationDelete : BaseEntity
    {
        public List<Guid> VocationIds { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<VocationDelete>
        {
            public Validator()
            {
                RuleFor(d => d.VocationIds).NotNull().WithMessage("职能Id不能为空");
            }
        }
    }




    public class VocationFunctionSelect : BaseEntity
    {
        public Guid FuncId { get; set; }

        public Guid VocationId { get; set; }

        public int Direction { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<VocationFunctionSelect>
        {
            public Validator()
            {
                RuleFor(d => d.FuncId).NotNull().WithMessage("功能Id不能为空");
                RuleFor(d => d.VocationId).NotNull().WithMessage("职能Id不能为空");
                RuleFor(d => d.Direction).NotNull().WithMessage("Direction不能为空");

            }
        }

    }


    public class VocationFunctionEdit : BaseEntity
    {
        public Guid VocationId { get; set; }
        public string FunctionJson { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<VocationFunctionEdit>
        {
            public Validator()
            {
                RuleFor(d => d.VocationId).NotNull().WithMessage("职能Id不能为空");
                RuleFor(d => d.FunctionJson).NotNull().WithMessage("功能不能为空");

            }
        }
    }


    public class FunctionEdit
    {

    }


    public class FunctionList
    {

    }



    public class FunctionRuleAdd : BaseEntity
    {
        public Guid VocationId { get; set; }
        public Guid FunctionId { get; set; }

        public bool IsAdd { get; set; }

        public string Rule { get; set; }
        public string RuleItem { get; set; }
        public string RuleSet { get; set; }

        public string RuleRelation { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<FunctionRuleAdd>
        {
            public Validator()
            {
                RuleFor(d => d.VocationId).NotNull().WithMessage("职能Id不能为空");
                RuleFor(d => d.FunctionId).NotNull().WithMessage("功能不能为空");
                RuleFor(d => d.Rule).NotNull().WithMessage("规则不能为空");
                RuleFor(d => d.RuleItem).NotNull().WithMessage("规则明细不能为空");
                RuleFor(d => d.RuleSet).NotNull().WithMessage("规则设置不能为空");


            }
        }
    }

    public class FunctionRuleEdit : BaseEntity
    {
        public Guid VocationId { get; set; }
        public Guid FunctionId { get; set; }
        public string Rule { get; set; }
        public string RuleItem { get; set; }
        public string RuleSet { get; set; }

        public bool IsAdd { get; set; }
        public string RuleRelation { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<FunctionRuleEdit>
        {
            public Validator()
            {
                RuleFor(d => d.Rule).NotNull().WithMessage("规则不能为空");
                RuleFor(d => d.RuleItem).NotNull().WithMessage("规则明细不能为空");
                RuleFor(d => d.RuleSet).NotNull().WithMessage("规则设置不能为空");


            }
        }
    }


    public class FunctionRuleSelect : BaseEntity
    {
        public Guid VocationId { get; set; }
        public Guid FunctionId { get; set; }
        public Guid EntityId { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<FunctionRuleSelect>
        {
            public Validator()
            {
                RuleFor(d => d.VocationId).NotNull().WithMessage("职能id不能为空");
                RuleFor(d => d.FunctionId).NotNull().WithMessage("功能id不能为空");
                RuleFor(d => d.EntityId).NotNull().WithMessage("实体id不能为空");
            }
        }

    }



    public class VocationUserSelect : BaseEntity
    {
        public Guid VocationId { get; set; }

        public Guid DeptId { get; set; }
        public string UserName { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<VocationUserSelect>
        {
            public Validator()
            {
                RuleFor(d => d.VocationId).NotNull().WithMessage("职能id不能为空");
            }
        }

    }


    public class VocationUserDelete : BaseEntity
    {

        public Guid VocationId { get; set; }

        public List<int> UserIds { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<VocationUserDelete>
        {
            public Validator()
            {
                RuleFor(d => d.VocationId).NotNull().WithMessage("职能id不能为空");
                RuleFor(d => d.UserIds).NotNull().WithMessage("用户id不能为空");
            }
        }

    }

    public class UserFunctionSelect : BaseEntity
    {

        public int UserNumber { get; set; }
        public int DeviceType { get; set; }

        public Int64 Version { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<UserFunctionSelect>
        {
            public Validator()
            {
                RuleFor(d => d.UserNumber).NotNull().WithMessage("用户id不能为空");
                RuleFor(d => d.DeviceType).NotNull().WithMessage("设备类型不能为空");
                RuleFor(d => d.Version).NotNull().WithMessage("版本号不能为空");
            }
        }


    }

    public class FunctionAdd : BaseEntity
    {
        public Guid TopFuncId { get; set; }
        public string FuncName { get; set; }
        public string FuncCode { get; set; }
        public Guid? EntityId { get; set; }
        public int DeviceType { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<FunctionAdd>
        {
            public Validator()
            {
                RuleFor(d => d.TopFuncId).NotNull().WithMessage("上级功能不能为空");
                RuleFor(d => d.FuncName).NotNull().WithMessage("功能名称不能为空");
                RuleFor(d => d.FuncCode).NotNull().WithMessage("功能编码不能为空");
                RuleFor(d => d.DeviceType).NotNull().WithMessage("设备类型不能为空");
            }
        }
    }


    public class FunctionItemEdit : BaseEntity
    {
        public Guid FuncId { get; set; }
        public string FuncName { get; set; }
        public string FuncCode { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<FunctionItemEdit>
        {
            public Validator()
            {
                RuleFor(d => d.FuncId).NotNull().WithMessage("功能Id不能为空");
                RuleFor(d => d.FuncName).NotNull().WithMessage("功能名称不能为空");
                RuleFor(d => d.FuncCode).NotNull().WithMessage("功能编码不能为空");
            }
        }
    }

    public class FunctionItemDelete : BaseEntity
    {
        public Guid FuncId { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<FunctionItemDelete>
        {
            public Validator()
            {
                RuleFor(d => d.FuncId).NotNull().WithMessage("功能Id不能为空");

            }
        }
    }


    public class FunctionTreeSelect : BaseEntity
    {
        public Guid TopFuncId { get; set; }

        public int Direction { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<FunctionTreeSelect>
        {
            public Validator()
            {
                RuleFor(d => d.TopFuncId).NotNull().WithMessage("功能Id不能为空");
                RuleFor(d => d.Direction).NotNull().WithMessage("方向不能为空");


            }
        }
    }

    public class FunctionRuleQueryMapper
    {
        public string VocationId { get; set; }
        public string FunctionId { get; set; }
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public int RecStatus { get; set; }
        public string ItemId { get; set; }

        public string FieldId { get; set; }
        public string ItemName { get; set; }
        public string Operate { get; set; }
        public int UseType { get; set; }
        public int RuleType { get; set; }
        public string RuleData { get; set; }
        public string RuleSet { get; set; }
    }

	public class RelTabRuleSelect : BaseEntity
	{
		public Guid RelTabId { get; set; }
		public Guid EntityId { get; set; }

		protected override IValidator GetValidator()
		{
			return new Validator();
		}
		class Validator : AbstractValidator<RelTabRuleSelect>
		{
			public Validator()
			{
				RuleFor(d => d.RelTabId).NotNull().WithMessage("页签id不能为空");
				RuleFor(d => d.EntityId).NotNull().WithMessage("实体id不能为空");
			}
		}
	}

	public class RelTabRuleQueryMapper
	{ 
		public Guid RelTabId { get; set; }
		public Guid RuleId { get; set; }
		public string RuleName { get; set; }
		public int RecStatus { get; set; }
		public Guid ItemId { get; set; }

		public Guid FieldId { get; set; }
		public string ItemName { get; set; }
		public string Operate { get; set; }
		public int UseType { get; set; }
		public int RuleType { get; set; }
		public string RuleData { get; set; }
		public string RuleSet { get; set; }
	}
}
