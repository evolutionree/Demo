using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Reminder
{
    public class ReminerEventInsert : BaseEntity
    {
        public string EventName { get; set; }
        public string EntityId { get; set; }
        public string Title { get; set; }
        public int CheckDay { get; set; }

        public string SendTime { get; set; }
        public int Type { get; set; }
        public string ExpandFieldId { get; set; }
        public string Params { get; set; }

        public int UserNumber { get; set; }
        public string Content { get; set; }
        public string UserColumn { get; set; }
        public int RemindType { get; set; }
        public string TimeFormat { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ReminerEventInsert>
        {
            public Validator()
            {
                RuleFor(d => d.EventName).NotNull().WithMessage("EventName不能为空");
                RuleFor(d => d.EntityId).NotNull().WithMessage("EntityId不能为空");
                RuleFor(d => d.Title).NotNull().WithMessage("Title不能为空");
                RuleFor(d => d.CheckDay).NotNull().WithMessage("CheckDay不能为空");
                RuleFor(d => d.SendTime).NotNull().WithMessage("SendTime不能为空");
                RuleFor(d => d.Type).NotNull().WithMessage("Type不能为空");
                RuleFor(d => d.ExpandFieldId).NotNull().WithMessage("ExpandFieldId不能为空");
                RuleFor(d => d.Params).NotNull().WithMessage("Params不能为空");
                RuleFor(d => d.UserNumber).NotNull().WithMessage("UserNumber不能为空");
                RuleFor(d => d.Content).NotNull().WithMessage("Content不能为空");
                RuleFor(d => d.UserColumn).NotNull().WithMessage("UserColumn不能为空");
                RuleFor(d => d.RemindType).NotNull().WithMessage("RemindType不能为空");
                RuleFor(d => d.TimeFormat).NotNull().WithMessage("TimeFormat不能为空");
            }
        }

    }

    public class ReminerEventUpdate : BaseEntity
    {
        public string EventId { get; set; }
        public string EventName { get; set; }

        public string Title { get; set; }

        public int CheckDay { get; set; }

        public string SendTime { get; set; }

        public string ExpandFieldId { get; set; }
        public string Params { get; set; }

        public int UserNumber { get; set; }

        public string Content { get; set; }
        public string TimeFormat { get; set; }


        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ReminerEventUpdate>
        {
            public Validator()
            {
                RuleFor(d => d.EventId).NotNull().WithMessage("EventId不能为空");
                RuleFor(d => d.EventName).NotNull().WithMessage("EventName不能为空");
                RuleFor(d => d.Title).NotNull().WithMessage("Title不能为空");
                RuleFor(d => d.CheckDay).NotNull().WithMessage("CheckDay不能为空");
                RuleFor(d => d.SendTime).NotNull().WithMessage("SendTime不能为空");
                RuleFor(d => d.ExpandFieldId).NotNull().WithMessage("ExpandFieldId不能为空");
                RuleFor(d => d.Params).NotNull().WithMessage("Params不能为空");
                RuleFor(d => d.UserNumber).NotNull().WithMessage("UserNumber不能为空");
                RuleFor(d => d.Content).NotNull().WithMessage("Content不能为空");
                RuleFor(d => d.TimeFormat).NotNull().WithMessage("TimeFormat不能为空");
            }
        }

    }

}
