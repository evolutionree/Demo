using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Role;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IRoleRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> RoleGroupQuery(int userNumber);
        OperateResult InsertRoleGroup(SaveRoleGroupMapper entity, int userNumber);
        OperateResult UpdateRoleGroup(SaveRoleGroupMapper entity, int userNumber);
        OperateResult DisabledRoleGroup(string roleGroupId, int userNumber);
        OperateResult OrderByRoleGroup(string roleGroupIds, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> RoleQuery(RoleListMapper roleList, int userNumber);
        OperateResult InsertRole(RoleMapper role, int userNumber);
        OperateResult UpdateRole(RoleMapper role, int userNumber);
        OperateResult CopyRole(RoleCopyMapper roleCopy, int userNumber);
        OperateResult DeleteRole(string roleIds, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> RoleUserQuery(RoleUserMapper roleUser, int userNumber);
        OperateResult AssigneRoleUser(AssigneUserToRoleMapper assigne, int userNumber);
        OperateResult DeleteRoleUser(string userIds, string roleId, int userNumber);

        /// <summary>
        /// 格式化rolerule
        /// </summary>
        /// <param name="roleRule"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        string FormatRoleRule(string roleRule, int userNumber);

        /// <summary>
        /// 获取用户的角色信息
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        List<RoleInfo> GetUserRoles(int userNumber);

        List<RolesUsersInfo> GetRolesUsers(List<Guid> roleids);

        string GetRoleRuleSql(Guid entityId, int userNumber);

    }
}

