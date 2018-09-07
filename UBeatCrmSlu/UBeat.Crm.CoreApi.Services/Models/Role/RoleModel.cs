using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Role
{
    public class SaveRoleGroupModel
    {
        public string RoleGroupId { get; set; }
        public string GroupName { get; set; }
        public int GroupType { get; set; }
        public Dictionary<string, string> GroupName_Lang { get; set; }
    }

    public class RoleListModel
    {
        public int RoleType { get; set; }
        public string RoleName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public string GroupId { get; set; }
        public Dictionary<string, string> RoleName_Lang { get; set; }
    }
    public class RoleModel
    {
        public string RoleGroupId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int RoleType { get; set; }
        public int RolePriority { get; set; }
        public string RoleRemark { get; set; }
        public Dictionary<string, string> RoleName_Lang { get; set; }

    }

    public class RoleCopyModel
    {
        public string RoleGroupId { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int RoleType { get; set; }
        public int RolePriority { get; set; }
        public string RoleRemark { get; set; }
        public Dictionary<string, string> RoleName_Lang { get; set; }
    }

    public class RoleUserModel
    {
        public string DeptId { get; set; }

        public string RoleId { get; set; }

        public string UserName { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }

    public class RoleUserRelationModel
    {
        public string RoleId { get; set; }

        public string UserIds { get; set; }
    }

    public class AssigneUserToRoleModel
    {
        public string RoleIds { get; set; }

        public string UserIds { get; set; }

        public string FuncIds { get; set; }
    }

    public class RoleDisabledModel
    {
        public string RoleIds { get; set; }
    }
}
