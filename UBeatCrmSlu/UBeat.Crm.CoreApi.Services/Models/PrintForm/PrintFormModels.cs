using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.PrintForm;

namespace UBeat.Crm.CoreApi.Services.Models.PrintForm
{
    public class OutputDocumentParameter
    {

        [JsonProperty("entityid")]
        public Guid? EntityId { set; get; }

        public int DocType { set; get; }

        [JsonProperty("data")]
        public IFormFile Data { set; get; }
    }
    public class PrintEntityModel
    {
        public Guid EntityId { set; get; }

        public Guid TemplateId { set; get; }

        public List<Guid> RecIds { set; get; }

    }

    public class TemplateListModel
    {
        public Guid EntityId { set; get; }
        public int RecState { set; get; }
    }

    public class TemplatesStatusModel
    {
        public List<Guid> RecIds { set; get; }
        public int RecStatus { set; get; }
    }

    

    public class TemplateInfoModel
    {
        /// <summary>
        /// update时的记录id。新增时可空
        /// </summary>
        public Guid? RecId { set; get; }

        public Guid EntityId { set; get; }
        /// <summary>
        /// 模板名称
        /// </summary>
        public string TemplateName { set; get; }
        /// <summary>
        /// 模板类型：0=Excel、1=word
        /// </summary>
        public TemplateType TemplateType { set; get; }
        /// <summary>
        /// 数据源类型：0=实体Detail接口、1=数据库函数、2=内部服务接口
        /// </summary>
        public DataSourceType DataSourceType { set; get; }
        /// <summary>
        /// 数据源处理接口:数据库函数名或者内部服务接口的命名空间
        /// </summary>
        public string DataSourceFunc { set; get; }
        /// <summary>
        /// 数据源扩展处理JS
        /// </summary>
        public string ExtJs { set; get; }
        /// <summary>
        /// 模板文件ID
        /// </summary>
        public Guid? FileId { set; get; }
        /// <summary>
        /// 适用范围RuleId
        /// </summary>
        public Guid? RuleId { set; get; }

        /// <summary>
        /// 适用范围说明
        /// </summary>
        public string RuleDesc { set; get; }
        /// <summary>
        /// 模板备注
        /// </summary>
        public string Description { set; get; }
       
    }
}
