using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class SimpleEntityInfo
    {

        public string EntityTable { get; set; }
        /// <summary>
        /// 实体类型ID
        /// </summary>
        public Guid CategoryId { set; get; }

        /// <summary>
        /// 实体类型名称
        /// </summary>
        public string CategoryName { set; get; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 实体名称
        /// </summary>
        public string EntityName { set; get; }

        /// <summary>
        /// 实体模型类型0独立实体1嵌套实体2简单(应用)实体3动态实体
        /// </summary>
        public EntityModelType ModelType { set; get; }

        /// <summary>
        /// 关联实体
        /// </summary>
        public Guid? RelEntityId { set; get; }

        public Guid? RelFieldId { get; set; }
        public string RelFieldName { get; set; }
        /// <summary>
        /// 关联实体名称
        /// </summary>
        public string RelEntityName { set; get; }

        /// <summary>
        /// 关联审批 0为不关联，1为关联
        /// </summary>
        public int RelAudit { set; get; }

        public ServicesJsonInfo Servicesjson { set; get; }
    }
}
