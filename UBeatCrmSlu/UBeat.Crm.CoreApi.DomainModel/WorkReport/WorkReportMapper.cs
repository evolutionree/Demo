using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.WorkReport
{
    public class DailyReportLstMapper
    {
        public string RecId { get; set; }
        public string RecType { get; set; }
        public int Recaudits { get; set; }
        public int RecStatus { get; set; }
        public int RecManager { get; set; }
        public string ReportDate { get; set; }
        public string ReportCon { get; set; }
        public string UdeptCode { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string EntityId { get; set; }

        public string MenuId { get; set; }
        public int ReportType { get; set; }

        public ICollection<DailyReportUserRecMapper> RecUsers { get; set; }

    }
    public class DailyReportMapper : BaseEntity
    {
        public string RecId { get; set; }
        public string RecType { get; set; }
        public int Recaudits { get; set; }
        public int RecStatus { get; set; }
        public int RecManager { get; set; }
        public string ReportDate { get; set; }
        public string ReportCon { get; set; }
        public string UdeptCode { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string EntityId { get; set; }

        public string MenuId { get; set; }
        public int ReportType { get; set; }

        public ICollection<DailyReportUserRecMapper> RecUsers { get; set; }
        protected override IValidator GetValidator()
        {
            return new DailyReportMapperValidator();
        }

        class DailyReportMapperValidator : AbstractValidator<DailyReportMapper>
        {
            public DailyReportMapperValidator()
            {
                RuleFor(d => d.ReportDate).NotEmpty().WithMessage("日报日期不能为空");
                RuleFor(d => d.ReportCon).NotEmpty().WithMessage("日报内容不能为空");
            }
        }

    }

    public class DailyReportUserRecMapper
    {
        public int Optype { get; set; }
        public string UserIds { get; set; }
    }

    public class WeeklyReportLstMapper 
    {
        public string RecId { get; set; }
        public string RecType { get; set; }
        public int Recaudits { get; set; }
        public int RecStatus { get; set; }
        public int RecManager { get; set; }
        public string ReportDate { get; set; }
        public string ReportCon { get; set; }
        public string UdeptCode { get; set; }
        public int Weeks { get; set; }
        public int WeekType { get; set; }
        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public string EntityId { get; set; }
        public string MenuId { get; set; }
        public int ReportType { get; set; }

        public ICollection<WeeklyReportUserRecMapper> RecUsers { get; set; }
    }

    public class WeeklyReportMapper : BaseEntity
    {
        public string RecId { get; set; }
        public string RecType { get; set; }
        public int Recaudits { get; set; }
        public int RecStatus { get; set; }
        public int RecManager { get; set; }
        public string ReportDate { get; set; }
        public string ReportCon { get; set; }
        public string UdeptCode { get; set; }
        public int Weeks { get; set; }
        public int WeekType { get; set; }
        public int ReportType { get; set; }

        public ICollection<WeeklyReportUserRecMapper> RecUsers { get; set; }

        protected override IValidator GetValidator()
        {
            return new WeeklyReportMapperValidator();
        }

        class WeeklyReportMapperValidator : AbstractValidator<WeeklyReportMapper>
        {
            public WeeklyReportMapperValidator()
            {
                RuleFor(d => d.ReportDate).NotEmpty().WithMessage("周报日期不能为空");
                RuleFor(d => d.ReportCon).NotEmpty().WithMessage("周报内容不能为空");
            }
        }
    }

    public class WeeklyReportUserRecMapper
    {
        public int Optype { get; set; }
        public string UserIds { get; set; }
    }
}
