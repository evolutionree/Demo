using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility
{
    public class ExcelCellInfo
    {
        public object Data { set; get; }



        public string CellReference { get; set; }

        public uint? StyleIndex { get; set; }

        public uint? CellMetaIndex { get; set; }

        public uint? ValueMetaIndex { get; set; }

        public string CellFormula { get; set; }

        public object CellValue { get; set; }

        public string InlineString { get; set; }

        public string ExtensionList { get; set; }
    }
}
