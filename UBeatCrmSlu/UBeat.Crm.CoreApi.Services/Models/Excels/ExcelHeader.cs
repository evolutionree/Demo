using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public class ExcelHeader
    {
        /// <summary>
        /// 表头的文本
        /// </summary>
        public string Text { set; get; }
        /// <summary>
        /// 表头对应的映射字段
        /// </summary>
        public string FieldName { set; get; }
        /// <summary>
        /// 导出时列宽度，单位像素
        /// </summary>
        public int Width { set; get; }
        /// <summary>
        /// 导入时列是否必填
        /// </summary>
        public bool IsNotEmpty { set; get; }
    }
}
