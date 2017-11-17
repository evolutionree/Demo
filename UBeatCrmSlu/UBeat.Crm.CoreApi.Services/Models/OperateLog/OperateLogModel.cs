using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.OperateLog
{
    public class OperateLogRecordListModel
    {
        public Guid DeptId { get; set; }
        public string UserName { get; set; }
        public string SearchBegin { get; set; }
        public string SearchEnd { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
