﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PrintForm
{
    public class CrmSysEntityPrintTemplate
    {
        public Guid RecId { set; get; }
        /// <summary>
        /// 关联的实体id
        /// </summary>
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
        /// 数据源处理接口:数据库函数名
        /// </summary>
        public string DataSourceFunc { set; get; }

        /// <summary>
        /// 如果使用内部服务接口时，需要指定使用的程序集dll名称，如XXXX，目前版本不要包含dll后缀，其他方式再根据实际情况改加载dll的逻辑
        /// </summary>
        public string AssemblyName { set; get; }
        /// <summary>
        /// 如果使用内部服务接口时，需要指定具体的类名称，填写完整的命名空间和类型名称
        /// </summary>
        public string ClassTypeName { set; get; }


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
        /// <summary>
        /// 状态 0删除 1正常
        /// </summary>
        public int RecStatus { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecCreated { set; get; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime RecUpdated { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int RecCreator { set; get; }
        /// <summary>
        /// 修改人
        /// </summary>
        public int RecUpdator { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string RecCreator_Name { set; get; }
        /// <summary>
        /// 修改人
        /// </summary>
        public string RecUpdator_Name { set; get; }
        /// <summary>
        /// 记录版本,系统自动生成
        /// </summary>
        public long RecVersion { set; get; }
        /// <summary>
        /// 输出格式配置
        /// </summary>
        public string ExportConfig { get; set; }
    }

    /// <summary>
    /// 模板类型：0=Excel、1=word
    /// </summary>
    public enum TemplateType
    {
        Excel=0,
        Word=1
    }
    /// <summary>
    /// 数据源类型：0=实体Detail接口、1=数据库函数、2=内部服务接口
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// 实体Detail接口
        /// </summary>
        EntityDetail = 0,
        /// <summary>
        /// 数据库函数
        /// </summary>
        DbFunction = 1,
        /// <summary>
        /// 内部服务接口
        /// </summary>
        InternalMethor = 2
    }
}
