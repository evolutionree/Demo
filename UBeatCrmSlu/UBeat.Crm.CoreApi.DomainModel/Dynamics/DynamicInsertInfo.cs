using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Dynamics
{
    public class DynamicInsertInfo
    {
        /// <summary>
        /// 动态类型：1=系统，2=实体
        /// </summary>
        public DynamicType DynamicType { set; get; }

        /// <summary>
        /// 对应的实体ID
        /// </summary>
        public Guid EntityId { set; get; }
        /// <summary>
        /// 对应的实体ID
        /// </summary>
        public Guid TypeId { set; get; }
        /// <summary>
        /// 实体业务数据的ID
        /// </summary>
        public Guid BusinessId { set; get; }

        /// <summary>
        /// 对应的实体的关联实体
        /// </summary>
        public Guid? RelEntityId { set; get; }
        /// <summary>
        /// 对应的实体的关联实体数据的业务ID
        /// </summary>
        public Guid RelBusinessId { set; get; }

        /// <summary>
        /// 动态模板的数据，JArray类型的字符串
        /// </summary>
        public string TemplateData { set; get; }
        /// <summary>
        /// 动态内容,动态类型为 0=默认，1=系统时 有效
        /// </summary>
        public string Content { set; get; }
    }
}
