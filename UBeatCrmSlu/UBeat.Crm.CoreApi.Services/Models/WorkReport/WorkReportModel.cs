using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.WorkReport
{
    public class DailyReportLstModel
    {
        public string RecId { get; set; }
        public string RecType { get; set; }
        public int Recaudits { get; set; }
        public int RecStatus { get; set; }
        public int RecManager { get; set; }
        public string ReportDate { get; set; }
        public string ReportCon { get; set; }
        public string UdeptCode { get; set; }
        public int ReportType { get; set; }


        public ICollection<DailyReportUserRecModel> RecUsers { get; set; }

    }
    public class DailyReportModel
    {
        public string RecId { get; set; }
        public string RecType { get; set; }
        public int Recaudits { get; set; }
        public int RecStatus { get; set; }
        public int RecManager { get; set; }
        public string ReportDate { get; set; }
        public string ReportCon { get; set; }
        public string UdeptCode { get; set; }
        public string MenuId { get; set; }

        public string EntityId { get; set; }
        public int ReportType { get; set; }


        public ICollection<DailyReportUserRecModel> RecUsers { get; set; }

    }
    public class DailyReportUserRecModel
    {
        public int Optype { get; set; }
        public int UserId { get; set; }
    }


    public class WeeklyReportLstModel
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
        public ICollection<WeeklyReportUserRecModel> RecUsers { get; set; }

    }

    public class WeeklyReportModel
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
        public string EntityId { get; set; }

        public string MenuId { get; set; }
        public ICollection<WeeklyReportUserRecModel> RecUsers { get; set; }

    }
    public class WeeklyReportUserRecModel
    {
        public int Optype { get; set; }
        public int UserId { get; set; }
    }
}
