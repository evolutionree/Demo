using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Reports;

namespace UBeat.Crm.CoreApi.Services.Models.ReportDefine
{
    public class ReportDefineModel
    {
    }
    public class ReportDefineQueryModel {
        public string Id { get; set; }
    }
    public class DataSourceQueryDataModel {
        public string DataSourceId { get; set; }
        public string InstId { get; set; }
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public Dictionary<string, string> SortBys { get; set; }

    }
    /// <summary>
    /// 报表表格导出参数
    /// </summary>
    public class ReportTableQueryDataModel {
        public DataSourceQueryDataModel  DataQueryModel { get; set; }
        public List<TableColumnInfo> Columns { get; set; }
    }
}
