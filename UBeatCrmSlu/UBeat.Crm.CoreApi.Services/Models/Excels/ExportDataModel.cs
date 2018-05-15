using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    
    public class ExportDataModel
    {
        /// <summary>
        /// 模板类型
        /// </summary>
        public TemplateType TemplateType { set; get; }

        #region --固定模板导出--
        /// <summary>
        /// 导出模板的Key，固定模板时为FuncName，动态模板时不需传
        /// </summary>
        public string FuncName { set; get; }

        /// <summary>
        /// 导出时的查询参数
        /// </summary>
        public string QueryParameters { set; get; } 

        /// <summary>
        /// 导出时的查询参数
        /// </summary>
        public Dictionary<string, object> QueryParametersDic { set; get; } = new Dictionary<string, object>();
        #endregion

        #region --动态模板导出--

        /// <summary>
        /// 操作的用户id
        /// </summary>
        public int UserId { set; get; }

        /// <summary>
        /// 实体类型的导出的查询参数json 字符串，最后解析为 DynamicModel
        /// </summary>
        public string DynamicQuery { set; get; }

        /// <summary>
        /// 实体类型的导出的查询参数对象，由DynamicQuery生成，不需传该字段
        /// </summary>
        public DynamicEntityListModel DynamicModel { set; get; }
        
        /// <summary>
        /// 标注哪些嵌套表格字段要导出/导出
        /// </summary>
        public List<string> NestTableList { get; set; }

        /// <summary>
        /// Excel导出是，如果涉及嵌套实体时，主表记录的表达方式
        /// fullfill表示每行重复
        /// KeepEmpty表示只是在第一行显示
        /// Merge表示合并主表的数据
        /// </summary>
        public ExportDataRowModeEnum RowMode { get; set; }
        /// <summary>
        /// 导出列的来源
        /// </summary>
        public ExportDataColumnSourceEnum ColumnSource { get; set; }

        public Guid EntityId { get; set; }

        #endregion
    }
    /// <summary>
    /// 实体导出Excel时，标记主表行的显示模式
    /// </summary>
    public enum ExportDataRowModeEnum {
        FullFill = 0,//每行填充
        KeepEmpty = 1,//留空白
        MergeRow = 2//合并
    }
    public enum ExportDataColumnSourceEnum {
        WEB_Standard = 0,
        WEB_Personal = 1,
        All_Columns = 2
    }
}
