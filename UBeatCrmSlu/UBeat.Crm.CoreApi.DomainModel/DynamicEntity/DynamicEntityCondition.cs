using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.DynamicEntity
{
    public class DynamicEntityCondition
    {
        #region 实体
        /// <summary>
        /// 实体ID
        /// </summary>
        [JsonProperty("entityid")]
        public Guid EntityId { get; set; }
        
        /// <summary>
        /// 字段ID
        /// </summary>
        [JsonProperty("fieldid")]
        public Guid Fieldid { get; set; }

        /// <summary>
        /// 条件类型 0是查重
        /// </summary>
        [JsonProperty("functype")]
        public int Functype { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Recorder { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecCreated { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime RecUpdated { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public int RecCreator { get; set; }

        /// <summary>
        /// 修改人
        /// </summary>
        public int RecUpdator { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public int RecVersion { get; set; }

        #endregion

        #region 扩展属性
        /// <summary>
        /// 前端显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 字段列名
        /// </summary>
        public string FieldName { get; set; }
        #endregion
    }
}
