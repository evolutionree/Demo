using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.SalesLead
{
    public class SalesLeadMapper : BaseEntity
    {
        public Guid TypeId { get; set; }

        public Guid Con_TypeId { get; set; }

        public Guid SalesLeadId { get; set; }

        protected override IValidator GetValidator()
        {
            return new SalesLeadMapperValidator();
        }
        class SalesLeadMapperValidator : AbstractValidator<SalesLeadMapper>
        {
            public SalesLeadMapperValidator()
            {
                RuleFor(d => d.TypeId).NotEmpty().WithMessage("客户实体Id不能为null");
                RuleFor(d => d.Con_TypeId).NotEmpty().WithMessage("联系人实体Id不能为null");
                RuleFor(d => d.SalesLeadId).NotEmpty().WithMessage("销售线索Id不能为null");
            }
        }
    }
}
