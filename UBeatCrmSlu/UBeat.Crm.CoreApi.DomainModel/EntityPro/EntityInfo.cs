using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class EntityInfo
    {
        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 实体名称
        /// </summary>
        public string EntityName { set; get; }

        /// <summary>
        /// 实体表名
        /// </summary>
        public string EntityTable { set; get; }

        /// <summary>
        /// 实体模型类型0独立实体1嵌套实体2简单(应用)实体3动态实体
        /// </summary>
        public int ModelType { set; get; }

        /// <summary>
        /// 实体备注
        /// </summary>
        public string Remark { set; get; }

        /// <summary>
        /// 实体风格 用于给手机端区分采用哪种样式
        /// </summary>
        public string Styles { set; get; }

        /// <summary>
        /// 实体图标
        /// </summary>
        public string Icons { set; get; }

        /// <summary>
        /// 关联实体
        /// </summary>
        public Guid RelEntityId { set; get; }

        /// <summary>
        /// 关联审批 0为不关联，1为关联
        /// </summary>
        public int RelAudit { set; get; }

        public int PublishStatus { set; get; }

        /// <summary>
        /// 录入方式,目前支持普通录入和Ocr录入
        /// </summary>
        public EntityInputModeInfo InputMethod { get; set; }

        /// <summary>
        /// 字段数据
        /// </summary>
        public List<EntityFieldInfo> Fields { set; get; } = new List<EntityFieldInfo>();
        /// <summary>
        /// 分类数据
        /// </summary>
        public List<EntityCategoryInfo> Categories { set; get; } = new List<EntityCategoryInfo>();

        /// <summary>
        /// 获取实体分类的字段
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public List<EntityFieldInfo> GetCategorieFields(Guid categoryId)
        {
            var fields = new List<EntityFieldInfo>();
            var categoryInfo = Categories.Find(m => m.CategoryId == categoryId);
            if (categoryInfo != null && Fields != null)
            {
                fields = Fields.Where(m => categoryInfo.FieldIds.Contains(m.FieldId)).ToList();
            }
            return fields;
        }


    }

    public class EntityFieldInfo
    {
        public Guid FieldId { set; get; }

        public string FieldName { set; get; }

        public string FieldLabel { set; get; }

        public string DisplayName { set; get; }

        public int ControlType { set; get; }

        public int FieldType { set; get; }

        public object FieldConfig { set; get; }

        public int Order { set; get; }

        public string ExpandJs { set; get; }

        public string FilterJs { set; get; }

    }

    public class EntityCategoryInfo
    {
        public Guid CategoryId { set; get; }

        public string CategoryName { set; get; }

        public List<Guid> FieldIds { set; get; }

    }


    public class UCodeMapper
    {
        public Guid? RecId { get; set; }

        public Guid? Id { get; set; }
        public List<Guid> Ids { get; set; }
        public string RecCode { get; set; }
        public string CommitDate { get; set; }
        public string UserName { get; set; }
        public string CommitUserName { get; set; }
        public int CommitUserId { get; set; }
        public string CommitRemark { get; set; }
        public string CommitRemarkDate { get; set; }
        public int PageIndex { get; set; }

        public int PageSize { get; set; }
        public string SearchOrder { get; set; }
        public Dictionary<string, object> ColumnFilter { get; set; }

    }


    public class PgCodeMapper
    {
        public List<Guid> Ids { get; set; }

        public Guid? Id { get; set; }
        public int PageIndex { get; set; }
        public string Remark { get; set; }
        public int PageSize { get; set; }
        public string SearchOrder { get; set; }
        public Dictionary<string, object> ColumnFilter { get; set; }

    }

}
