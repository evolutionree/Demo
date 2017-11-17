using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace UBeat.Crm.CoreApi.DomainModel.Notify
{
    public class NotifyFetchMessageMapper : BaseEntity
    {
        public Int64 RecVersion { get; set; }

        protected override IValidator GetValidator()
        {
            return new NotifyFetchMessageMapperValidator();
        }
    }

    public class NotifyFetchMessageMapperValidator : AbstractValidator<NotifyFetchMessageMapper>
    {
        public NotifyFetchMessageMapperValidator()
        {
            RuleFor(d => d.RecVersion).NotNull().GreaterThanOrEqualTo(0).WithMessage("版本ID不能为空");
        }
    }
}
