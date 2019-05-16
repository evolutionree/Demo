using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.ReportRelation
{

    public class AddReportRelationModel
    {
        // public Guid AnaFuncId { get; set; }
        public String ReportRelationName { get; set; }
        public String ReportreMark { get; set; }

    }

    public class EditReportRelationModel
    {
        public Guid ReportRelationId { get; set; }
        public String ReportRelationName { get; set; }
        public String ReportreMark { get; set; }

    }


    public class DeleteReportRelationModel
    {
        public List<Guid> ReportRelationIds { get; set; }
        public int RecStatus { get; set; }

    }

    public class AddReportRelDetailModel
    {
        // public Guid AnaFuncId { get; set; }

        public String ReportrelationId { get; set; }
        public int ReportUser { get; set; }
        public String ReportLeader { get; set; }
    }

    public class EditReportRelDetailModel
    {
        public Guid ReportRelDetailId { get; set; }
        public Guid ReportRelationId { get; set; }
        public int ReportUser { get; set; }
        public String ReportLeader { get; set; }

    }


    public class DeleteReportRelDetailModel
    {
        public List<Guid> ReportRelDetailIds { get; set; }
        public int RecStatus { get; set; }

    }

    public class QueryReportRelationModel
    {
        public String Name { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public string SearchOrder { get; set; }
        public Dictionary<string, object> ColumnFilter { get; set; }
    }

    public class QueryReportRelDetailModel
    {
        public String Name { get; set; }
        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public string SearchOrder { get; set; }
        public Dictionary<string, object> ColumnFilter { get; set; }
    }

}
