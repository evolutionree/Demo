using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Excels
{
   public class ExportDataDomainModel
    {
        /// <summary>
        /// 是否是游标查询
        /// </summary>
        public bool IsStoredProcCursor { set; get; }
        public string Sql { get; set; }

       
        /// <summary>
        /// 导出时的查询参数
        /// </summary>
        public Dictionary<string, object> QueryParameters { set; get; }

        public int UserNo { set; get; }

        public string RuleSql { set; get; }
    }
}
