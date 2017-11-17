using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Rule;

namespace UBeat.Crm.CoreApi.DomainModel.Role
{

    public class RoleInfo
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public Guid RoleId { set; get; }

        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName { set; get; }

        /// <summary>
        /// 角色类型 0系统角色 1自定义角色
        /// </summary>
        public int RoleType { set; get; }

        /// <summary>
        /// 角色特权 主要用于多角色时，区分等级
        /// </summary>
        public int RolePriority { set; get; }

        /// <summary>
        /// 角色备注
        /// </summary>
        public string RoleRemark { set; get; }

        /// <summary>
        /// 角色分组ID
        /// </summary>
        public Guid RoleGroupId { set; get; }

        /// <summary>
        /// 角色分组名称
        /// </summary>
        public string RoleGroupName { set; get; }

        /// <summary>
        /// 分组类型 0系统 1自定义
        /// </summary>
        public int GroupType { set; get; }

        public List<RuleInfo> Rules { set; get; } = new List<RuleInfo>();
    }

    
}
