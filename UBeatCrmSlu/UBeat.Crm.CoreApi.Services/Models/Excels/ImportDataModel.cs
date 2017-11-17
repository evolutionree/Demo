using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Excels
{
    public class ImportDataModel
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
        /// 导入类型，实体动态导入有效，模板导入由定义的sql决定
        /// </summary>
        public ExcelOperateType OperateType { get; set; }

        /// <summary>
        /// 导入时的默认参数，这些参数不存在与Excel文档中，由调用方传入
        /// </summary>
		[JsonIgnore]
        public Dictionary<string, object> DefaultParameters
		{
			get
			{
				if (string.IsNullOrEmpty(DefaultParametersJson))
					return new Dictionary<string, object>();
				return JsonConvert.DeserializeObject<Dictionary<string,object>>(DefaultParametersJson);
			}
		}

		public string DefaultParametersJson { set; get; }

		public IFormFile Data { set; get; }
    }

    public enum ExcelOperateType
    {
        /// <summary>
        /// 新增导入(重复的放弃导入)
        /// </summary>
        ImportAdd = 4,
        /// <summary>
        /// 覆盖导入
        /// </summary>
        ImportUpdate = 5,
        /// <summary>
        /// 导出
        /// </summary>
        Export = 6

    }
    
}
