using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public class TaskDataModel
    {
        public string TaskName { set; get; }
        

        public List<SheetDefine> SheetDefines { set; get; }
        public List<ImportSheetData> Datas { set; get; }

        /// <summary>
        /// 导出模板的Key，固定模板时为FuncName，动态模板时为entityid
        /// </summary>
        public string FormDataKey { set; get; }


        /// <summary>
        /// 导入类型，实体动态导入有效，模板导入由定义的sql决定
        /// </summary>
        public ExcelOperateType OperateType { get; set; }

        /// <summary>
        /// 导入时的默认参数，这些参数不存在与Excel文档中，由调用方传入
        /// </summary>
        public Dictionary<string, object> DefaultParameters { set; get; }

        public int UserNo { set; get; }
    }
}
