using Dapper;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Role;
using UBeat.Crm.CoreApi.DomainModel.Rule;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Role
{
    public class RoleRepository : RepositoryBase, IRoleRepository
    {

        #region 角色分类 
        public Dictionary<string, List<IDictionary<string, object>>> RoleGroupQuery(int userNumber)
        {
            var procName =
                "SELECT crm_func_role_group_list(@userno)";

            var dataNames = new List<string> { "RoleGroupList" };
            var param = new DynamicParameters();
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult InsertRoleGroup(SaveRoleGroupMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_role_group_add(@groupname, @grouptype, @userno,@rolegroupname_lang::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("groupname", entity.GroupName);
            param.Add("grouptype", entity.GroupType);
            param.Add("userno", userNumber);
            param.Add("rolegroupname_lang", JsonConvert.SerializeObject(entity.GroupName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }


        public OperateResult UpdateRoleGroup(SaveRoleGroupMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_role_group_edit(@rolegroupid, @groupname, @userno,@groupname_lang::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("rolegroupid", entity.RoleGroupId);
            param.Add("groupname", entity.GroupName);
            param.Add("userno", userNumber);
            param.Add("groupname_lang", JsonConvert.SerializeObject( entity.GroupName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DisabledRoleGroup(string roleGroupId, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_role_group_disabled(@rolegroupid, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("rolegroupid", roleGroupId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult OrderByRoleGroup(string roleGroupIds, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_role_group_orderby(@rolegroupids, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("rolegroupids", roleGroupIds);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion


        #region 角色
        public Dictionary<string, List<IDictionary<string, object>>> RoleQuery(RoleListMapper roleList, int userNumber)
        {
            var procName =
                "SELECT crm_func_role_list(@groupid,@rolename,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "Page", "PageCount" };
            var param = new DynamicParameters();
            param.Add("groupid", roleList.GroupId);
            param.Add("rolename", roleList.RoleName);
            param.Add("pageindex", roleList.PageIndex);
            param.Add("pagesize", roleList.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult InsertRole(RoleMapper role, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_role_add(@rolegroupid,@rolename, @roletype,@rolepriority,@roleremark, @userno,@rolename_lang::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("rolegroupid", role.RoleGroupId);
            param.Add("rolename", role.RoleName);
            param.Add("roletype", role.RoleType);
            param.Add("rolepriority", role.RolePriority);
            param.Add("roleremark", role.RoleRemark);
            param.Add("userno", userNumber);
            param.Add("rolename_lang",JsonConvert.SerializeObject( role.RoleName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult UpdateRole(RoleMapper role, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_role_edit(@rolegroupid,@roleid,@rolename, @roletype,@rolepriority,@roleremark, @userno,@rolename_lang::jsonb)
            ";
            var param = new DynamicParameters();
            param.Add("rolegroupid", role.RoleGroupId);
            param.Add("roleid", role.RoleId);
            param.Add("rolename", role.RoleName);
            param.Add("roletype", role.RoleType);
            param.Add("rolepriority", role.RolePriority);
            param.Add("roleremark", role.RoleRemark);
            param.Add("userno", userNumber);
            param.Add("rolename_lang", JsonConvert.SerializeObject(role.RoleName_Lang));
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult CopyRole(RoleCopyMapper roleCopy, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_role_copy(@roleid,@rolegroupid,@rolename,@rolename_lang,@rolepriority, @roletype,@roleremark, @userno)
            ";
            var param = new DynamicParameters();
            param.Add("roleid", roleCopy.RoleId);
            param.Add("rolegroupid", roleCopy.RoleGroupId);
            param.Add("rolename", roleCopy.RoleName);
            param.Add("rolename_lang", JsonConvert.SerializeObject(roleCopy.RoleName_Lang));
            param.Add("roletype", roleCopy.RoleType);
            param.Add("rolepriority", roleCopy.RolePriority);
            param.Add("roleremark", roleCopy.RoleRemark);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult DeleteRole(string roleIds, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_role_del(@roleIds,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("roleIds", roleIds);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion


        #region  角色关联人员
        public Dictionary<string, List<IDictionary<string, object>>> RoleUserQuery(RoleUserMapper roleUser, int userNumber)
        {
            var procName =
                "SELECT crm_func_role_user_list(@deptid,@roleid,@username,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "Page", "PageCount" };
            var param = new DynamicParameters();
            param.Add("deptid", roleUser.DeptId);
            param.Add("roleid", roleUser.RoleId);
            param.Add("username", roleUser.UserName);
            param.Add("pageindex", roleUser.PageIndex);
            param.Add("pagesize", roleUser.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }
        public OperateResult AssigneRoleUser(AssigneUserToRoleMapper assigne, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_user_role_assigne(@roleids,@userids,@funcids,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("userids", assigne.UserIds);
            param.Add("roleids", assigne.RoleIds);
            param.Add("funcids", assigne.FuncIds);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }


        public OperateResult DeleteRoleUser(string userIds, string roleId, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_user_role_delete(@roleid,@userids,@userno)
            ";
            var param = new DynamicParameters();
            param.Add("userids", userIds);
            param.Add("roleid", roleId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        #endregion




        /// <summary>
        /// 格式化rolerule
        /// </summary>
        /// <param name="roleRule"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public string FormatRoleRule(string roleRule, int userNumber)
        {
            var procName =
               "SELECT crm_func_role_rule_param_format(@paramsql,@userNo)";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("paramsql", roleRule));
            sqlParameters.Add(new NpgsqlParameter("userNo", userNumber));

            var result = DBHelper.ExecuteScalar("", procName, sqlParameters.ToArray());

            return result == null ? null : result.ToString();
        }

        /// <summary>
        /// 格式化rolerule
        /// </summary>
        /// <param name="roleRule"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public string GetRoleRuleSql(Guid entityId, int userNumber)
        {
            var sql =
               "SELECT * FROM  crm_func_role_rule_fetch_sql(@entityid,@userNo)";

            var param = new DynamicParameters();
            param.Add("entityid", entityId);
            param.Add("userno", userNumber);

            var result = DataBaseHelper.QuerySingle<dynamic>( sql, param);

            return result == null ? null : result.crm_func_role_rule_fetch_sql;
        }
        /// <summary>
        /// 获取用户的角色信息
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public List<RoleInfo> GetUserRoles(int userNumber)
        {
            using (var conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    var role_sql = string.Format(@"
                                    SELECT r.roleid,r.rolename,r.rolename_lang,r.roletype,r.rolepriority,r.roleremark,g.rolegroupid,g.rolegroupname,g.grouptype FROM 
                                    crm_sys_role AS r
                                    INNER JOIN crm_sys_userinfo_role_relate AS u ON r.roleid=u.roleid
                                    LEFT JOIN crm_sys_role_group_relate AS gr ON gr.roleid=r.roleid
                                    LEFT JOIN crm_sys_role_group AS g ON g.rolegroupid=gr.rolegroupid
                                    WHERE r.recstatus=1 AND u.userid=@userNumber;");
                    var role_sqlParameters = new List<DbParameter>();
                    role_sqlParameters.Add(new NpgsqlParameter("userNumber", userNumber));
                    var roles = DBHelper.ExecuteQuery<RoleInfo>(tran, role_sql, role_sqlParameters.ToArray());
                    foreach (var role in roles)
                    {

                        // RuleInfo
                        var rule_sql = string.Format(@"
                                SELECT r.ruleid,r.rulename,r.entityid,r.rulesql 
                                FROM crm_sys_rule AS r
                                INNER JOIN crm_sys_role_rule_relation AS rr ON rr.ruleid=r.ruleid
                                WHERE r.recstatus=1 AND rr.roleid=@roleid ;");
                        var rule_sqlParameters = new List<DbParameter>();
                        rule_sqlParameters.Add(new NpgsqlParameter("roleid",role.RoleId));
                        role.Rules = DBHelper.ExecuteQuery<RuleInfo>(tran, rule_sql, rule_sqlParameters.ToArray());
                    }

                    return roles;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public List<RolesUsersInfo> GetRolesUsers(List<Guid> roleids)
        {
            var sql = @"SELECT r.roleid,u.userid,u.username FROM crm_sys_role AS r
                            INNER JOIN crm_sys_userinfo_role_relate AS ur ON ur.roleid = r.roleid
                            INNER JOIN crm_sys_userinfo AS u ON u.userid = ur.userid
                            WHERE r.roleid = ANY(@roleid)";

            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("roleid", roleids.ToArray()));
            
            var result = DBHelper.ExecuteQuery<RolesUsersInfo>("", sql, sqlParameters.ToArray());

            return result;
        }


    }
}
