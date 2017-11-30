using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.EntityPro
{
    public class EntityMenuInfo
    {
        /// <summary>
        /// 实体菜单ID
        /// </summary>
        public Guid MenuId { set; get; }
        /// <summary>
        /// 实体菜单名称
        /// </summary>
        public string MenuName { set; get; }
        /// <summary>
        /// 菜单类型，0显示菜单 1转移菜单
        /// </summary>
        public MenuType MenuType { set; get; }
        /// <summary>
        /// 关联的实体ID
        /// </summary>
        public Guid EntityId { set; get; }

        /// <summary>
        /// 规则ID，用于定义该菜单执行的SQL规则
        /// </summary>
        public Guid RuleId { set; get; }

        /// <summary>
        /// 需要父级权限0为不需要1为需要
        /// </summary>
        public int NeedPower { set; get; }
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

    }

    /// <summary>
    /// 菜单类型，0显示菜单 1转移菜单
    /// </summary>
    public enum MenuType
    {
        /// <summary>
        /// 0显示菜单
        /// </summary>
        AllList = 0,
        /// <summary>
        /// 1转移菜单
        /// </summary>
        TransferList = 1,
    }
}
