using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Reminder
{
    public class ReminderItemInsert: BaseEntity
    {
        public Guid id { get; set; }

        public Guid dicId { get; set; }

        public int dicTypeId { get; set; }

        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ReminderItemInsert>
        {
            public Validator()
            {
                RuleFor(d => d.id).NotNull().WithMessage("ID不能为空");
                RuleFor(d => d.dicId).NotNull().WithMessage("字典字段id不能为空");
                RuleFor(d => d.dicTypeId).NotNull().WithMessage("字典类型id不可为空");

            }
        }
    }
}
