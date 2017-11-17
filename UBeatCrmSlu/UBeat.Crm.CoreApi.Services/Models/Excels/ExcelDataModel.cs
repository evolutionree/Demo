using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public abstract class SheetDefine
    {
        public string SheetName { get; set; }

        /// <summary>
        /// 执行的sql语句，sql中的参数名必须为@FieldName
        /// </summary>
        public string ExecuteSQL { set; get; }

		public string DefaultDataSql { set; get; }

		/// <summary>
		/// 是否游标返回,是=1，否=0
		/// </summary>
		public int IsStoredProcCursor { set; get; }

        public SheetDefine Clone()
        {
            return this.MemberwiseClone() as SheetDefine;
        }
    }

    /// <summary>
    /// 简单的模板定义，仅仅只支持单行单列的表头设计
    /// </summary>
    public class SimpleSheetTemplate : SheetDefine
    {
        /// <summary>
        /// 表头的文本列表，按顺序排列
        /// </summary>
        public List<SimpleHeader> Headers { set; get; }

        /// <summary>
        /// 预留字段，根据需要使用
        /// </summary>
        public object DataObject { set; get; }
    }
    public class SimpleHeader
    {
        public string HeaderText { set; get; }

        /// <summary>
        /// 列对应字段
        /// </summary>
        public string FieldName { set; get; }


        public int Width { set; get; }


        /// <summary>
        /// 是否可空，用于导入做判断条件
        /// </summary>
        public bool IsNotEmpty { set; get; }

        /// <summary>
        /// 列对应的数据类型，用于导入做判断条件,目前用于处理时间类型的字段
        /// </summary>
        public FieldType FieldType { set; get; }

    }

    

    public class ExcelTemplate
    {
        public List<SheetTemplate> DataSheets { set; get; } = new List<SheetTemplate>();

        /// <summary>
        /// 注意事项的标签页内容，生成导入模板时，该内容会存在于模板中
        /// </summary>
        public string AttentionOuterXml { set; get; }

        /// <summary>
        /// 模板定义说明标签页内容
        /// </summary>
        public string TemplateDefineOuterXml { set; get; }

        public string SQLDefineOuterXml { set; get; }
        /// <summary>
        /// 共享数据的表数据
        /// </summary>
        public string SharedStringTableOuterXml { set; get; } 
	}
    


    /// <summary>
    /// 通过Excel的模板生成Excel
    /// </summary>
    public class SheetTemplate : SheetDefine
    {

        public string StylesheetXml { set; get; }
        public string ColumnsOuterXml { get; set; }
        
        public HeadersTemplate HeadersTemplate { get; set; }
        /// <summary>
        /// 列的映射对象集合,与模板对应的
        /// </summary>
        public List<ColumnMapModel> ColumnMap { set; get; } = new List<ColumnMapModel>();

		public object DataObject { set; get; }

	}
    public class ColumnMapModel
    {
        /// <summary>
        /// 列的顺序
        /// </summary>
        public int Index { set; get; }
        /// <summary>
        /// 列对应字段
        /// </summary>
        public string FieldName { set; get; }
        /// <summary>
        /// 是否可空，用于导入做判断条件
        /// </summary>
        public bool IsNotEmpty { set; get; }

        /// <summary>
        /// 列对应的数据类型，用于导入做判断条件
        /// </summary>
        public FieldType FieldType { set; get; }
        /// <summary>
        /// 样式的id
        /// </summary>
        public uint StyleIndex { set; get; }
    }

    public enum FieldType
    {
        /// <summary>
        /// 文本
        /// </summary>
        Text = 1,
        /// <summary>
        /// 附加拼音字段,某个字段定为该类型时，系统会自动生成该字段的拼音字段
        /// 如：在Excel模板中username字段定义为该类型，则系统会自动生成username_pinyin字段，值为username的拼音首字母字符串
        /// </summary>
        TextPinyinShort = 2,
        /// <summary>
        /// 附加拼音字段,某个字段定为该类型时，系统会自动生成该字段的拼音字段，
        /// 如：在Excel模板中username字段定义为该类型，则系统会自动生成username_pinyin字段，值为username的拼音字符串
        /// </summary>
        TextPinyin = 3,
        /// <summary>
        /// 整数
        /// </summary>
        NumberInt = 6,
        /// <summary>
        /// 小数
        /// </summary>
        NumberDecimal = 7,
        /// <summary>
        /// 日期，年月日
        /// </summary>
        TimeDate = 8,
        /// <summary>
        /// 日期时间
        /// </summary>
        TimeStamp = 9,
        /// <summary>
        /// 手机号
        /// </summary>
        PhoneNum = 10,
        /// <summary>
        /// 邮箱地址
        /// </summary>
        EmailAddr = 11,
        /// <summary>
        /// 电话
        /// </summary>
        Telephone = 12,
        /// <summary>
        /// 地址
        /// </summary>
        Address = 13,

        /// <summary>
        /// 文件类型
        /// </summary>
        File = 14,
        /// <summary>
        /// 图片类型
        /// </summary>
        Image = 15,
        /// <summary>
        /// 引用类型，如用户id，数据源等类型，取值时通过_name获取
        /// </summary>
        reference = 16,
    }

    public class ImportSheetData
    {
        public List<Dictionary<string, object>> DataRows { get; set; }
        public string SheetName { get; set; }
        /// <summary>
        /// 执行的sql语句，sql中的参数名必须为@FieldName
        /// </summary>
        public string ExecuteSQL { set; get; }
        public ImportSheetData()
        {
            DataRows = new List<Dictionary<string, object>>();
        }
    }


    public class HeadersTemplate
    {
        public List<string> MergeCellsOuterXmls { set; get; }

        public List<CellModel> RowCellsTemplate { set; get; }

        public int RowsCount { set; get; }
    }
    public class ExportSheetData
    {
        public SheetDefine SheetDefines { get; set; }
        public List<Dictionary<string, object>> DataRows { get; set; }
        /// <summary>
        /// 数据行的错误信息提示，如导入失败的数据行的提示内容，按照下标和DataRows的数据行一一对应
        /// </summary>
        public List<string> RowErrors { set; get; }

        public ExportSheetData()
        {
            DataRows = new List<Dictionary<string, object>>();
            RowErrors = new List<string>();
        }
    }
    public class CellModel
    {

        public string CellReference { get; set; }

        public uint? StyleIndex { get; set; }

        public uint? CellMetaIndex { get; set; }

        public uint? ValueMetaIndex { get; set; }

        public string CellFormula { get; set; }

        public object CellValue { get; set; }

        public string InlineString { get; set; }

        public string ExtensionList { get; set; }
    }
    public class HyperlinkData
    {
        public string Text { set; get; }

        public string Hyperlink { set; get; }
    }

    public class OXSExcelDataCell
    {
        public OXSDataItemType DataType { set; get; } = OXSDataItemType.String;

        public object Data { set; get; }

        public int CellWidth { set; get; }

        public int CellHeight { set; get; }


        /// <summary>
        /// 创建单元格为图片的数据对象
        /// </summary>
        /// <param name="data">数据图片的byte[]</param>
        /// <param name="dataType"></param>
        public OXSExcelDataCell(byte[] data, OXSDataItemType dataType, int cellWidth = 0, int cellHeight = 0)
        {
            DataType = dataType;
            Data = data;
            CellWidth = cellWidth;
            CellHeight = cellHeight;
        }
        public OXSExcelDataCell(List<byte[]> datalist, OXSDataItemType dataType, int cellHeight = 0)
        {
            DataType = dataType;
            Data = datalist;
            CellWidth = 0;
            CellHeight = cellHeight;
        }

        public OXSExcelDataCell(string data, int cellHeight = 0)
        {
            DataType = OXSDataItemType.String;
            Data = data;
            CellWidth = 0;
            CellHeight = cellHeight;
        }

        public OXSExcelDataCell(HyperlinkData data, int cellHeight = 0)
        {
            DataType = OXSDataItemType.Hyperlink;
            Data = data;
            CellWidth = 0;
            CellHeight = cellHeight;
        }
    }
    public enum OXSDataItemType
    {
        //
        // 摘要:
        //     String type.
        String = -1,
        //
        // 摘要:
        //     Windows Bitmap Graphics (.bmp).
        Bmp = 0,
        //
        // 摘要:
        //     Graphic Interchange Format (.gif).
        Gif = 1,
        //
        // 摘要:
        //     Portable (Public) Network Graphic (.png).
        Png = 2,
        //
        // 摘要:
        //     Tagged Image Format File (.tiff).
        Tiff = 3,
        //
        // 摘要:
        //     Windows Icon (.ico).
        Icon = 4,
        //
        // 摘要:
        //     PC Paintbrush Bitmap Graphic (.pcx).
        Pcx = 5,
        //
        // 摘要:
        //     JPEG/JIFF Image (.jpeg).
        Jpeg = 6,
        //
        // 摘要:
        //     Extended (Enhanced) Windows Metafile Format (.emf).
        Emf = 7,
        //
        // 摘要:
        //     Windows Metafile (.wmf).
        Wmf = 8,
        /// <summary>
        /// 超链接
        /// </summary>
        Hyperlink = 9,


    }



}