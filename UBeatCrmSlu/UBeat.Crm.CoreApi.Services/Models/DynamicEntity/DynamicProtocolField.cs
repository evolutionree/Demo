using System;

namespace UBeat.Crm.CoreApi.Services.Models.DynamicEntity
{
    public enum DynamicProtocolFieldType
    {
        /// <summary>
        /// 默认字段
        /// </summary>
        Default = 1,
        /// <summary>
        /// 自定义字段
        /// </summary>
        Custom = 2,
        /// <summary>
        /// 虚拟字段
        /// </summary>
        Virtual = 3
    }

    public enum DynamicProtocolControlType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 文本
        /// </summary>
        Text = 1,
        /// <summary>
        /// 提示文本
        /// </summary>
        TipText = 2,
        /// <summary>
        /// 本地字典单选
        /// </summary>
        SelectSingle = 3,
        /// <summary>
        /// 本地字典多选
        /// </summary>
        SelectMulti = 4,
        /// <summary>
        /// 大文本
        /// </summary>
        TextArea = 5,
        /// <summary>
        /// 整数
        /// </summary>
        NumberInt = 6,
        /// <summary>
        /// 小数
        /// </summary>
        NumberDecimal = 7,
        /// <summary>
        /// 日期，年月日
        /// </summary>
        TimeDate = 8,
        /// <summary>
        /// 日期时间
        /// </summary>
        TimeStamp = 9,
        /// <summary>
        /// 手机号
        /// </summary>
        PhoneNum = 10,
        /// <summary>
        /// 邮箱地址
        /// </summary>
        EmailAddr = 11,
        /// <summary>
        /// 电话
        /// </summary>
        Telephone = 12,
        /// <summary>
        /// 地址
        /// </summary>
        Address = 13,
        /// <summary>
        /// 定位
        /// </summary>
        Location = 14,
        /// <summary>
        /// 头像
        /// </summary>
        HeadPhoto = 15,
        /// <summary>
        /// 行政区域
        /// </summary>
        AreaRegion = 16,
        /// <summary>
        /// 团队组织
        /// </summary>
        Department = 17,
        /// <summary>
        /// 数据源单选
        /// </summary>
        DataSourceSingle = 18,
        /// <summary>
        /// 数据源多选
        /// </summary>
        DataSourceMulti = 19,
        /// <summary>
        /// 分组
        /// </summary>
        AreaGroup = 20,
        /// <summary>
        /// 树形
        /// </summary>
        TreeSingle = 21,
        /// <summary>
        /// 拍照
        /// </summary>
        TakePhoto = 22,
        /// <summary>
        /// 附件
        /// </summary>
        FileAttach = 23,
        /// <summary>
        /// 表格控件
        /// </summary>
        LinkeTable = 24,
        /// <summary>
        /// 单选人
        /// </summary>
        PersonSelectSingle = 25,
        /// <summary>
        /// 多选人
        /// </summary>
        PersonSelectMulti = 26,
        /// <summary>
        /// 树形多选
        /// </summary>
        TreeMulti = 27,
        /// <summary>
        /// 关联对象
        /// </summary>
        RelateControl = 30,
        /// <summary>
        /// 引用控件
        /// </summary>
        QuoteControl = 31,
        //记录ID
        RecId = 1001,
        //创建人
        RecCreator = 1002,
        //更新人
        RecUpdator = 1003,
        //创建时间
        RecCreated = 1004,
        //更新时间
        RecUpdated = 1005,
        //负责人
        RecManager = 1006,
        //审批状态
        RecAudits = 1007,
        //记录状态
        RecStatus = 1008,
        //记录类型
        RecType = 1009,
        /// <summary>
        /// 明细ID
        /// </summary>
        RecItemid = 1010,
        /// <summary>
        /// 活动时间
        /// </summary>
        RecOnlive = 1011,
        /// <summary>
        /// 记录名称
        /// </summary>
        RecName = 1012,
        /// <summary>
        /// 销售阶段
        /// </summary>
        SalesStage=1013,
        /// <summary>
        /// 产品
        /// </summary>
        Product = 28,
        /// <summary>
        /// 产品系列
        /// </summary>
        ProductSet = 29,
    }

    public enum DynamicProtocolOperateType
    {
        /// <summary>
        /// 新增
        /// </summary>
        Add = 0,
        /// <summary>
        /// 编辑
        /// </summary>
        Edit = 1,
        /// <summary>
        /// 详情
        /// </summary>
        Detail = 2,
        /// <summary>
        /// 列表(+高级搜索)
        /// </summary>
        List = 3,
        /// <summary>
        /// 导入新增
        /// </summary>
        ImportAdd = 4,
        /// <summary>
        /// 导入覆盖
        /// </summary>
        ImportUpdate = 5,
        /// <summary>
        /// 导出
        /// </summary>
        Export = 6
    }

    public enum DynamicProtocolViewType
    {
        /// <summary>
        /// WEB
        /// </summary>
        Web = 0,
        /// <summary>
        /// MOB
        /// </summary>
        Mob = 1
    }

    public class DynamicProtocolField
    {
        /// <summary>
        /// 字段ID
        /// </summary>
        public Guid FieldId { get; set; }
        /// <summary>
        /// 数据库字段名
        /// </summary>
        public string FieldName { get; set; }
        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { get; set; }
        /// <summary>
        /// 字段中文名
        /// </summary>
        public string FieldLabel { get; set; }
        /// <summary>
        /// 字段最终显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 控件类型
        /// </summary>
        public DynamicProtocolControlType ControlType { get; set; }
        /// <summary>
        /// 字段类型  1默认字段 2自定义字段 3虚拟字段
        /// </summary>
        public DynamicProtocolFieldType FieldType { get; set; }
        /// <summary>
        /// 字段配置
        /// </summary>
        public DynamicProtocolFieldConfig FieldConfig { get; set; }
    }

    public enum DynamicProtocolDataSourceType
    {
        /// <summary>
        /// 字典表数据源
        /// </summary>
        LocalSource = 0,
        /// <summary>
        /// 在线数据源
        /// </summary>
        NetWorkSource = 1
    }

    public class DynamicProtocolDataSource
    {
        /// <summary>
        /// 数据源类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 参数KEY,本地数据源是字典表的类型ID,在线是数据源的UUID
        /// </summary>
        public string SourceId { get; set; }

        public string SourceKey { get; set; }
        /// <summary>
        /// 增加对数据源整理后的entityid处理，只是在回传前端使用，不记录到数据库中 
        /// </summary>
        public Guid EntityId { get; set; }

        ///// <summary>
        ///// 数据源类型
        ///// </summary>
        //public DynamicProtocolDataSourceType DataType { get; set; }
        ///// <summary>
        ///// 参数KEY,本地数据源是字典表的类型ID,在线是数据源的UUID
        ///// </summary>
        //public string TypeKey { get; set; }
    }

    public class DynamicProtocolFieldConfig
    {
        /// <summary>
        /// 数据源
        /// </summary>
        public DynamicProtocolDataSource DataSource { get; set; }
        /// <summary>
        /// 数据最短长度
        /// </summary>
        public int? MinLength { get; set; }
        /// <summary>
        /// 数据最长长度
        /// </summary>
        public int? MaxLength { get; set; }
        /// <summary>
        /// 正则验证
        /// </summary>
        public string ValidRegex { get; set; }
        /// <summary>
        /// 是否只读
        /// </summary>
        public bool IsReadOnly { get; set; }
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; }
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible { get; set; }
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 嵌套表格字段时记录嵌套实体的id
        /// </summary>
        public Guid EntityId { get; set; }

        /// <summary>
        /// 是否多选，只有数据源有效
        /// </summary>
        public int Multiple { get; set; }
    }

    public class DynamicProtocolValidResult
    {
        /// <summary>
        /// 数据库字段名
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldDisplay { get; set; }
        /// <summary>
        /// 错误提示
        /// </summary>
        public string Tips { get; set; }
        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValid { get; set; }

        public bool IsRequired { get; set; }
        /// <summary>
        /// 字段值
        /// </summary>
        public object FieldData { get; set; }
        /// <summary>
        /// 字段类型
        /// </summary>
        public int ControlType { get; set; }

        public string FieldConfig { get; set; }
    }
}
