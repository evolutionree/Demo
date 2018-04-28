using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Department
{
    /// <summary>
    /// 部门及
    /// </summary>
    public class DepartmentPermissionInfo
    {
    }
    /// <summary>
    /// 部门及用户的授权方案
    /// </summary>
    public class DeptPermissionScheme {
        /// <summary>
        /// 方案id
        /// </summary>
        public Guid RecId { get; set; }
        /// <summary>
        /// 方案名称
        /// </summary>
        public string SchemeName { get; set; }
        public int RecStatus { get; set; }
        public string Remark { get; set; }
        public int IsDefault { get; set; }
    }
    /// <summary>
    /// 用于记录授权方案中的每一个授权项目的授权情况
    /// </summary>
    public class DeptPermissionSchemeEntryInfo {
        /// <summary>
        /// 关联授权方案
        /// </summary>
        [JsonProperty(PropertyName = "schemeid")]
        public Guid SchemeId { get; set; }
        [JsonProperty(PropertyName = "recid")]
        public Guid RecId { get; set; }

        [JsonProperty(PropertyName = "authorized_userid")]
        public int Authorized_UserId { get; set; }
        public string Authorized_UserName { get; set; }
        /// <summary>
        /// 如果被授权人类型为角色时，此字段记录角色的id
        /// </summary>
        [JsonProperty(PropertyName = "authorized_roleid")]
        public Guid Authorized_RoleId { get; set; }
        /// <summary>
        /// 被授权人的类型为角色时，此字段记录角色的名称，但不保存数据库，仅为了返回前端显示使用
        /// </summary>
        public string Authorized_RoleName { get; set; }
        /// <summary>
        /// 被授权人的类型，可能是角色，可能是用户,但不能是部门
        /// </summary>
        [JsonProperty(PropertyName = "authorized_type")]
        public DeptPermissionObjectTypeEnum Authorized_Type { get; set; }
        [JsonProperty(PropertyName = "pmobject_userid")]
        public int PMObject_UserId { get; set; }
        public string PMObject_UserName { get; set; }
        [JsonProperty(PropertyName = "pmobject_deptid")]
        public Guid PMObject_DeptId { get; set; }
        public string PMObject_DeptName { get; set; }
        [JsonProperty(PropertyName = "pmobject_type")]
        public DeptPermissionObjectTypeEnum PMObject_Type { get; set; }
        [JsonProperty(PropertyName = "permissiontype")]
        public DeptPermissionAuthTypeEnum PermissionType { get; set; }
        /// <summary>
        /// 默认情况下，子部门的权限情况
        /// </summary>
        [JsonProperty(PropertyName = "subdeptpermission")]
        public DeptPermissionSubPolicyEnum SubDeptPermission { get; set; }
        /// <summary>
        /// 默认情况下，本部门人员的授权情况
        /// </summary>
        [JsonProperty(PropertyName = "subuserpermission")]
        public DeptPermissionSubPolicyEnum SubUserPermission { get; set; }
        /// <summary>
        /// 用于返回处理，实际不保存
        /// </summary>
        public Guid ParentId { get; set; }
        /// <summary>
        /// 仅用于中间处理,不返回，不保存
        /// 不做索引，因为中间有地方使用到ref
        /// </summary>
        public List<DeptPermissionSchemeEntryInfo> SubDepts;
    }
    public enum DeptPermissionObjectTypeEnum
    {
        User =1,
        Department =2,
        Role=3
    }
    /// <summary>
    /// 授权情况，0=未设置，1=未获得授权，2=已获得授权，3=明确拒绝
    /// </summary>
    public enum DeptPermissionAuthTypeEnum {
        NotDefined = 0 ,
        NotAuthed = 1,
        Authed = 2,
        Reject = 3
    }
    public enum DeptPermissionSubPolicyEnum {
        Auto_NoGain=0,
        Auto_Gain = 1
    }
}
