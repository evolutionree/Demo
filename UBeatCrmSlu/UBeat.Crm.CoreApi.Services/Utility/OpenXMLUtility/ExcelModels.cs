using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility
{
    public class ExcelInfo
    {
        /// <summary>
        /// Excel样式定义
        /// </summary>
        public string StyleSheetXml { set; get; }
        /// <summary>
        /// 共享内容数据定义
        /// </summary>
        public string SharedStringsXml { set; get; }



        List<ExcelSheetInfo> Sheets { set; get; }
    }

    public class ExcelSheetInfo
    {
        public uint SheetId { set; get; }

        public string SheetName { set; get; }

        /// <summary>
        /// sheet表中每列格式定义，如宽度
        /// </summary>
        public string ColumnsOuterXml { get; set; }

        public List<ExcelRowInfo> Rows { set; get; }
    }

    public class ExcelRowInfo
    {
        public Row Row { set; get; }
        public List<ExcelCellInfo> Cells { set; get; }
    }


    public class ExcelCellInfo
    {
        public object Data { set; get; }

        public bool IsOverWrighted { set; get; }
        
    }
}
