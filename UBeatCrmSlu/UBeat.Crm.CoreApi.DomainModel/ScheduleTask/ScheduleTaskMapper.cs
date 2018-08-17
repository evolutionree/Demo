using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.ScheduleTask
{
    public class ScheduleTaskListMapper : BaseEntity
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public String UserIds { get; set; }

        public String UserType { get; set; }

        public int AffairStatus { get; set; }

        public int AffairType { get; set; }
        protected override IValidator GetValidator()
        {
            return new ScheduleTaskListMapperValidator();
        }

        class ScheduleTaskListMapperValidator : AbstractValidator<ScheduleTaskListMapper>
        {
            public ScheduleTaskListMapperValidator()
            {
                RuleFor(d => d.DateFrom).NotNull().WithMessage("开始时间不能为空");
                RuleFor(d => d.DateTo).NotNull().WithMessage("结束时间不能为空");
                Custom((d, validationContext) =>
                {
                    if (string.IsNullOrEmpty(d.UserType) && String.IsNullOrEmpty(d.UserIds))
                    {
                        return new ValidationFailure("", "下属或者用户不能同时为空");
                    }
                    return null;
                });
            }
        }
    }

    public class ScheduleTaskCountMapper
    {

        public int UnFinishedSchedule { get; set; }

        public int UnFinishedTask { get; set; }
    }
}
