using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Dynamics
{
    public class DynamicPraiseMapper: BaseEntity
    {
        public Guid DynamicId { set; get; }

        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DynamicPraiseMapper>
        {
            public Validator()
            {
                RuleFor(d => d.DynamicId).NotNull().WithMessage("DynamicId不能为空");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
    }
}
