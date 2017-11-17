using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Dynamics
{
    public class DynamicAbstractInsert: BaseEntity
    {
        public Guid TypeID { set; get; }
        public Guid EntityID { set; get; }

        public List<string> Fieldids { set; get; }

        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DynamicAbstractInsert>
        {
            public Validator()
            {
                RuleFor(d => d.TypeID).NotNull().WithMessage("TypeID不能为空");
                RuleFor(d => d.EntityID).NotNull().WithMessage("EntityId不能为空");
                RuleFor(d => d.Fieldids).NotNull().WithMessage("字段ID列表不可为空");
            }
        }
    }

    public class DynamicAbstractSelect: BaseEntity
    {
        public Guid TypeID { set; get; }

        public Guid EntityID { set; get; }
        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DynamicAbstractSelect>
        {
            public Validator()
            {
                RuleFor(d => d.EntityID).NotNull().WithMessage("EntityID不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }
}
