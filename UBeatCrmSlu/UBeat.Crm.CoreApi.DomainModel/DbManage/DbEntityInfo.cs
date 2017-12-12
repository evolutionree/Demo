using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;

namespace UBeat.Crm.CoreApi.DomainModel.DbManage
{

    /// <summary>
    /// 实体配置信息,用于实体数据导出
    /// </summary>
    public class DbEntityInfo
    {
        /// <summary>
        /// 实体id
        /// </summary>
        public Guid EntityId { get; set; }
        /// <summary>
        /// 实体名称
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// 实体对应的数据库的表名
        /// </summary>
        public string EntityTable { get; set; }
        /// <summary>
        /// 实体类型
        /// </summary>
        public DbEntityModelTypeEnum ModelType { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 风格
        /// </summary>
        public string Styles { get; set; }

        /// <summary>
        /// 图标
        /// </summary>
        public string Icons { get; set;  }

        /// <summary>
        /// 关联实体id
        /// 主要用于简单实体、动态实体、嵌套实体
        /// </summary>
        public Guid? RelEntityId { get; set; }
        /// <summary>
        /// 是否关联实体
        /// </summary>
        public int RelAudit { get; set;  }

        /// <summary>
        /// 新增时的js脚本
        /// </summary>
        public string NewLoad { get; set; }
        /// <summary>
        /// 修改时的js脚本
        /// </summary>
        public string EditLoad { get; set; }
        /// <summary>
        /// 查看时的js脚本
        /// </summary>
        public string CheckLoad { get; set; }

        /// <summary>
        /// 字段信息
        /// </summary>
        public List<DbEntityFieldInfo> Fields { get; set; }
        /// <summary>
        /// 实体分类属性
        /// </summary>
        public List<DbEntityCatelogInfo> Catelogs { get; set; }
    }
    /// <summary>
    /// 实体分类信息
    /// </summary>
    public class DbEntityCatelogInfo {
        public Guid CatelogId { get; set; }
        public string CatelogName { get; set; }
        public Guid? RelCatelogId { get; set; }
        public List<DbEntityFieldRuleInfo> FieldRules { get; set; }
    }
    public class DbEntityFieldRuleInfo {
        public Guid FieldRulesId { get; set; }
        public Guid TypeId { get; set; }
        public Guid FieldId  { get; set; }
        public int OperateType { get; set; }
        public int IsRequire { get; set; }
        public int IsVisible { get; set; }
        public int IsReadOnly { get; set; }
        public Dictionary<string, object> ViewRules { get; set; }
        public Dictionary<string, object> ValidRules { get; set; }

    }
    /// <summary>
    /// 字段信息定义表
    /// </summary>
    public class DbEntityFieldInfo {
        /// <summary>
        /// 字段id
        /// </summary>
        public Guid FieldId { get; set; }
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 字段标题
        /// </summary>
        public string FieldLabel { get; set; }
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 控件类型
        /// </summary>
        public EntityFieldControlType ControlType { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public FieldTypeEnum FieldType { get; set; }
        /// <summary>
        /// 自定扩展属性
        /// </summary>
        public Dictionary<string, object> FieldConfig { get; set; }
        /// <summary>
        /// 字段顺序
        /// </summary>
        public int RecOrder { get; set;  }

        /// <summary>
        /// 扩展JS字段
        /// </summary>
        public string Expandjs { get; set; }
        /// <summary>
        /// 过滤JS字段 
        /// </summary>
        public string Filterjs { get; set; }
        /// <summary>
        /// 字段状态
        /// </summary>
        public int RecStatus { get; set; }
    }
    public enum DbEntityModelTypeEnum {
        StandAloneEntity = 0,
        NestEntity = 1,
        SimpleEntity = 2,
        DynamicEntity = 3
    }
    public enum FieldTypeEnum {
        /// <summary>
        /// 1默认字段 
        /// </summary>
        SystemField = 1,
        /// <summary>
        /// 自定义字段
        /// </summary>
        UserDefinedField = 2,
        /// <summary>
        /// 虚拟字段
        /// </summary>
        VirtualField = 3
    }
}
