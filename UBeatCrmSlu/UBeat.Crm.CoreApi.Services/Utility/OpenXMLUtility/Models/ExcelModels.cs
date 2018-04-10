using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using UBeat.Crm.CoreApi.Services.Models.Excels;

namespace UBeat.Crm.CoreApi.Services.Utility.OpenXMLUtility
{
    public class ExcelInfo
    {
        /// <summary>
        /// 解析Excel 的文件数据
        /// </summary>
        public byte[] ExcelFileBytes { set; get; }
        /// <summary>
        /// 每个sheet表的数据
        /// </summary>
        public List<ExcelSheetInfo> Sheets { set; get; }
    }

    public class ExcelSheetInfo
    {
        public string SheetName { set; get; }

        public List<ExcelRowInfo> Rows { set; get; }

       
    }

    public class ExcelRowInfo
    {
        public uint RowIndex { set; get; }

        public string OuterXml { set; get; }

        public List<ExcelCellInfo> Cells { set; get; }

        /// <summary>
        /// 行状态：0=未修改，1=新增,2=已编辑，-1=删除
        /// </summary>
        public RowStatus RowStatus { set; get; }

        public ExcelRowInfo Clone()
        {
            var newboj= this.MemberwiseClone() as ExcelRowInfo;
            newboj.Cells = new List<ExcelCellInfo>();
            foreach (var cells in this.Cells)
            {
                newboj.Cells.Add(cells.Clone());
            }

            return newboj;
        }
    }


    public class ExcelCellInfo
    {
        /// <summary>
        /// 列名称，如A,B,C等
        /// </summary>
        public string ColumnName { set; get; }
        /// <summary>
        /// 单元格值，如果是图片，则为byte[]
        /// </summary>
        public object CellValue { set; get; }
        /// <summary>
        /// 是否被更新
        /// </summary>
        public bool IsUpdated { set; get; }

        public ExcelCellInfo Clone()
        {
            return this.MemberwiseClone() as ExcelCellInfo;
        }
        public OXSDataItemType DataType { set; get; } = OXSDataItemType.String;
    }

    public enum RowStatus
    {
        Deleted = -1,
        Normal =0,
        Add=1,
        Edit=2
    }
}
