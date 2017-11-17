using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public class ExportTemplateModel
    {
        /// <summary>
        /// 导出模板的Key，固定模板时为FuncName，动态模板时为entityid
        /// </summary>
        public string Key { set; get; }
        /// <summary>
        /// 模板类型
        /// </summary>
        public TemplateType TemplateType { set; get; }
        /// <summary>
        /// 导出的类型
        /// </summary>
        public ExportType ExportType { set; get; }
		/// <summary>
		/// 用户Id
		/// </summary>
		public int UserId { set; get; }
	}

    public enum TemplateType
    {
        /// <summary>
        /// 固定模板
        /// </summary>
        FixedTemplate = 0,
        /// <summary>
        /// 动态模板
        /// </summary>
        DynamicTemplate = 1, 
	}

    public enum ExportType
    {
        /// <summary>
        /// Excel模板文件
        /// </summary>
        ExcelTemplate = 0,
        /// <summary>
        /// Excel的数据导入模板
        /// </summary>
        ImportDataTemplate = 1,
    }
}
