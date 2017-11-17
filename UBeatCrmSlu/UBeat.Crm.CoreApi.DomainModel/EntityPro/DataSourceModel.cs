using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class DataSourceEntityModel
    {
        /// <summary>
        /// 数据源ID
        /// </summary>
        public Guid DataSrcId { set; get; }

        /// <summary>
        /// 数据源名称
        /// </summary>
        public string DataSrcName { set; get; }
        /// <summary>
        /// 数据源类型 0关联实体的数据源 1自定义数据源
        /// </summary>
        public int SrcType { set; get; }
        /// <summary>
        /// 实体ID
        /// </summary>
        public Guid EntityId { set; get; }
        /// <summary>
        /// 数据源描述
        /// </summary>
        public string SrcMark { set; get; }
        /// <summary>
        /// 规则SQL
        /// </summary>
        public string RuleSql { set; get; }
        /// <summary>
        /// 排序
        /// </summary>
        public int RecOrder { set; get; }
        /// <summary>
        /// 状态 1启用 0停用
        /// </summary>
        public int RecStatus { set; get; }
        /// <summary>
        /// 创建人
        /// </summary>
        public int RecCreator { set; get; }
        /// <summary>
        /// 更新人
        /// </summary>
        public int RecUpdator { set; get; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime RecCreated { set; get; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime RecUpdated { set; get; }
        /// <summary>
        /// 版本，系统自动生成
        /// </summary>
        public long RecVersion { set; get; }
        /// <summary>
        /// 0不关联1关联
        /// </summary>
        public int IsRelatePower { set; get; }



    }
}
