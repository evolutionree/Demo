﻿using Microsoft.AspNetCore.Http;
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
        public String ReportUser { get; set; }
        public String ReportLeader { get; set; }
    }

    public class EditReportRelDetailModel
    {
        public Guid ReportRelDetailId { get; set; }
        public Guid ReportRelationId { get; set; }
        public String ReportUser { get; set; }
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

    public class ImportReportRelationModel
    {

        /// <summary>
        /// true是删除全部再导入，false是增量导入，增量导入包括遇到同一个汇报人的话 会覆盖原本有的
        /// </summary>
        public bool IsConvertImport { get; set; }
        public IFormFile Data { set; get; }
    }

}