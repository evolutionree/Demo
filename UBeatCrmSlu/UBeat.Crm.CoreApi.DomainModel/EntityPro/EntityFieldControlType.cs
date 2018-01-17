using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public enum EntityFieldControlType
    {
        /// <summary>
        /// 无
        /// </summary>
        None = 0,
        /// <summary>
        /// 文本text
        /// </summary>
        Text = 1, 
        /// <summary>
        /// 提示文本 text 
        /// </summary>
        TipText = 2,
        /// <summary>
        /// 本地字典单选 int4
        /// </summary>
        SelectSingle = 3,
        /// <summary>
        /// 本地字典多选 text
        /// </summary>
        SelectMulti = 4,
        /// <summary>
        /// 大文本 text
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
        /// 日期，年月日 date
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
        /// 地址 jsonb 
        /// {"lat": "", "lon": "", "address": ""}
        /// </summary>
        Address = 13,
        /// <summary>
        /// 定位 jsonb
        /// {"lat": 0, "lon": 0, "address": ""}
        /// </summary>
        Location = 14,
        /// <summary>
        /// 头像
        /// 数据库存储为text，存储图片的uuid
        /// </summary>
        HeadPhoto = 15,
        /// <summary>
        /// 行政区域
        /// 数据库存储为int4类型，存储行政区域的id
        /// </summary>
        AreaRegion = 16,
        /// <summary>
        /// 团队组织 text(多选，单选)
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
        /// 表格控件text
        /// </summary> 
        LinkeTable = 24,
        /// <summary>
        /// 单选人 text
        /// </summary>
        PersonSelectSingle = 25,
        /// <summary>
        /// 多选人 text
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
        /// <summary>
        /// 记录ID UUID
        /// </summary>
        RecId = 1001,
        /// <summary>
        /// 创建人 int4
        /// </summary>
        RecCreator = 1002,
      
        /// <summary>
        /// 更新人 int4
        /// </summary>
        RecUpdator = 1003,
        /// <summary>
        /// 创建时间  timestamp
        /// </summary>
        RecCreated = 1004,
        /// <summary>
        /// 更新时间 timestamp
        /// </summary>
        RecUpdated = 1005,
        /// <summary>
        /// 负责人 int4
        /// </summary>
        RecManager = 1006,
        /// <summary>
        /// 审批状态 int4
        /// </summary>
        RecAudits = 1007,
        /// <summary>
        /// 记录状态  int4
        /// </summary>
        RecStatus = 1008,
        /// <summary>
        /// 记录类型 uuid
        /// </summary>
        RecType = 1009,
        /// <summary>
        /// 明细ID  
        /// </summary>
        RecItemid = 1010,
        /// <summary>
        /// 活动时间 timestamp
        /// </summary>
        RecOnlive = 1011,
        /// <summary>
        /// 记录名称
        /// 数据库存储为text
        /// </summary>
        RecName = 1012,
        /// <summary>
        /// 销售阶段 uuid
        /// </summary>
        SalesStage = 1013,
        /// <summary>
        /// 产品 text
        /// </summary>
        Product = 28,
        /// <summary>
        /// 产品系列 text
        /// </summary>
        ProductSet = 29,
    }
}
