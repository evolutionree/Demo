using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Excels
{
    public class DeleteExcelDomainModel : BaseEntity
    {
        public Guid RecId { set; get; }

       
        
        public int UserNo { set; get; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<DeleteExcelDomainModel>
        {
            public Validator()
            {
                RuleFor(d => d.RecId).NotEmpty().NotNull().WithMessage("RecId不能为空");
            }
        }
    }
}
