using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Department;

namespace UBeat.Crm.CoreApi.IRepository
{
    /// <summary>
    /// 与部门人员特殊授权相关的数据库接口
    /// </summary>
    public interface IDeptPermissionRepository
    {
        /// <summary>
        /// 根据授权用户，获取当前方案下的所有权限信息
        /// </summary>
        /// <param name="SchemeId"></param>
        /// <param name="authed_userid"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        List<DeptPermissionSchemeEntryInfo> ListPermissionDetailByUserId(Guid SchemeId, int authed_userid, int userNum, DbTransaction dbTransaction);
        /// <summary>
        /// 根据角色，获取某方案下的权限信息
        /// </summary>
        /// <param name="SchemeId"></param>
        /// <param name="RoleId"></param>
        /// <param name="userNum"></param>
        /// <returns></returns>
        List<DeptPermissionSchemeEntryInfo> ListPermissionDetailByRoleId(Guid SchemeId, Guid RoleId, int userNum, DbTransaction dbTransaction);
        /// <summary>
        /// 根据权限方案，获取该权限方案下的所有处理过的被授权对象
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="schemeId"></param>
        /// <param name="userId"></param>
        /// <returns>包括：被授权对象类型（角色or 用户），用户id，用户名称，角色id，角色名称</returns>
        List<Dictionary<string, object>> ListAuthorizedObjects(DbTransaction tran, Guid schemeId, int userId);
        void DeletePermissionItemByUser(DbTransaction tran, Guid schemeId, int authorized_userid, int userId);
        void DeletePermissionItemByRole(DbTransaction tran, Guid schemeId, Guid authorized_roleid, int userId);
        void SavePermissionItem(DbTransaction tran, List<DeptPermissionSchemeEntryInfo> items, int userId);
        /// <summary>
        /// 获取某用户的角色列表
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="searchResultUserId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<Guid> GetRolesByUser(DbTransaction tran , int searchResultUserId, int userId);
    }
}
