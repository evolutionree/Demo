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
        

       
        #endregion



       
    }
}
