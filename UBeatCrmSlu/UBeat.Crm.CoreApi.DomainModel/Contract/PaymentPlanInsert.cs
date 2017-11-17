using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Contract
{
    public class PaymentPlanAdd : BaseEntity
    {
        public DateTime PlanTime { get; set; }
        public decimal PlanMoney { get; set; }
        public Guid ParentId { get; set; }
        public string Remark { get; set; }
        public int RefundType { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<PaymentPlanAdd>
        {
            public Validator()
            {
                RuleFor(d => d.PlanTime).NotNull().WithMessage("PlanTime不能为空");
                RuleFor(d => d.PlanMoney).NotNull().WithMessage("PlanMoney不能为空");
                RuleFor(d => d.ParentId).NotNull().WithMessage("ParentId不能为空");
                RuleFor(d => d.RefundType).NotNull().WithMessage("RefundType不能为空");
            }
        }

    }


    public class PaymentPlanEdit : BaseEntity
    {
        public Guid PlanId { get; set; }
        public DateTime PlanTime { get; set; }
        public decimal PlanMoney { get; set; }
        public string Remark { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<PaymentPlanEdit>
        {
            public Validator()
            {
                RuleFor(d => d.PlanId).NotNull().WithMessage("PlanId不能为空");
                RuleFor(d => d.PlanTime).NotNull().WithMessage("PlanTime不能为空");
                RuleFor(d => d.PlanMoney).NotNull().WithMessage("PlanMoney不能为空");
            }
        }
    }


    public class PaymentPlanDelete : BaseEntity
    {
        public DateTime PlanTime { get; set; }
        public decimal PlanMoney { get; set; }
        public Guid ContractId { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<PaymentPlanDelete>
        {
            public Validator()
            {
                //RuleFor(d => d.PlanTime).NotNull().WithMessage("PlanTime不能为空");
                //RuleFor(d => d.PlanMoney).NotNull().WithMessage("PlanMoney不能为空");
                //RuleFor(d => d.ContractId).NotNull().WithMessage("ContractId不能为空");
            }
        }

    }


    public class PaymentPlanList : BaseEntity
    {
        public DateTime PlanTime { get; set; }
        public decimal PlanMoney { get; set; }
        public Guid ContractId { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<PaymentPlanList>
        {
            public Validator()
            {
                //RuleFor(d => d.PlanTime).NotNull().WithMessage("PlanTime不能为空");
                //RuleFor(d => d.PlanMoney).NotNull().WithMessage("PlanMoney不能为空");
                //RuleFor(d => d.ContractId).NotNull().WithMessage("ContractId不能为空");
            }
        }
    }



    public class PaymentAdd : BaseEntity
    {
        public DateTime PayTime { get; set; }
        public decimal PayMoney { get; set; }
        public string PayName { get; set; }
        public string Remark { get; set; }
        public int BizType { get; set; }
        public Guid ParentId { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<PaymentAdd>
        {
            public Validator()
            {
                //RuleFor(d => d.PlanTime).NotNull().WithMessage("PlanTime不能为空");
                //RuleFor(d => d.PlanMoney).NotNull().WithMessage("PlanMoney不能为空");
                //RuleFor(d => d.ContractId).NotNull().WithMessage("ContractId不能为空");
            }
        }
    }


    public class PaymentEdit : BaseEntity
    {

        public Guid PaymentId { get; set; }
        public DateTime PayTime { get; set; }
        public decimal PayMoney { get; set; }
        public string PayName { get; set; }
        public string Remark { get; set; }
        public Guid ParentId { get; set; }
        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<PaymentEdit>
        {
            public Validator()
            {
                //RuleFor(d => d.PlanTime).NotNull().WithMessage("PlanTime不能为空");
                //RuleFor(d => d.PlanMoney).NotNull().WithMessage("PlanMoney不能为空");
                //RuleFor(d => d.ContractId).NotNull().WithMessage("ContractId不能为空");
            }
        }

    }


    public class PaymentDelete : BaseEntity
    {

        public DateTime PayTime { get; set; }
        public decimal PayMoney { get; set; }
        public string PayName { get; set; }
        public string Remark { get; set; }
        public Guid ContractId { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<PaymentDelete>
        {
            public Validator()
            {
                //RuleFor(d => d.PlanTime).NotNull().WithMessage("PlanTime不能为空");
                //RuleFor(d => d.PlanMoney).NotNull().WithMessage("PlanMoney不能为空");
                //RuleFor(d => d.ContractId).NotNull().WithMessage("ContractId不能为空");
            }
        }
    }


    public class PaymentList : BaseEntity
    {

        public DateTime PayTime { get; set; }
        public decimal PayMoney { get; set; }
        public string PayName { get; set; }
        public string Remark { get; set; }
        public Guid ContractId { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<PaymentList>
        {
            public Validator()
            {
                //RuleFor(d => d.PlanTime).NotNull().WithMessage("PlanTime不能为空");
                //RuleFor(d => d.PlanMoney).NotNull().WithMessage("PlanMoney不能为空");
                //RuleFor(d => d.ContractId).NotNull().WithMessage("ContractId不能为空");
            }
        }
    }

    public class UpdateContractStatusMapper : BaseEntity
    {
        public string RecIds { get; set; }
        protected override IValidator GetValidator()
        {
            return new UpdateContractStatusMapperValidator();
        }
        class UpdateContractStatusMapperValidator : AbstractValidator<UpdateContractStatusMapper>
        {
            public UpdateContractStatusMapperValidator()
            {
                RuleFor(d => d.RecIds).NotNull().WithMessage("合同Id不能为空");
                //RuleFor(d => d.PlanMoney).NotNull().WithMessage("PlanMoney不能为空");
                //RuleFor(d => d.ContractId).NotNull().WithMessage("ContractId不能为空");
            }
        }
    }
}
