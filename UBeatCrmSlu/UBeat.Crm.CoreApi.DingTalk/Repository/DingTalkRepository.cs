using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.Repository.Repository;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Services.Models.Department;
using Dapper;
using UBeat.Crm.CoreApi.Repository.Utility;
using Newtonsoft.Json;

namespace UBeat.Crm.CoreApi.DingTalk.Repository
{
    public class DingTalkRepository : RepositoryBase, IDingTalkRepository
    {
        public AccountUserMapper GetUserInfoforDingding(string dduserid)
        {
            string sql = "select * from crm_sys_userinfo where dduserid = @dduserid";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("dduserid",dduserid)
            };
            return ExecuteQuery<AccountUserMapper>(sql, p).FirstOrDefault();

        }

        public List<Dictionary<string, object>> GetEntranceList()
        {
            string sql = @"select a.*,b.icons
                            from crm_sys_entrance a inner
                            join crm_sys_entity b on a.entityid = b.entityid
                            where b.modeltype = 0
                            order by a.entrytype ,a.recorder ";
            return ExecuteQuery(sql, new DbParameter[] { });
        }

        public AccountUserMapper GetUserInfoforDingdingByNick(string nickName)
        {
            string sql = "select * from crm_sys_userinfo where nickname = @nickname";
            var p = new DbParameter[]
            {
                new NpgsqlParameter("nickname",nickName)
            };
            return ExecuteQuery<AccountUserMapper>(sql, p).FirstOrDefault();

        }



        /// <summary>
        /// 添加部门
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="departmentName"></param>
        /// <param name="ogLevel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public int DepartmentAdd(Guid departmentId, string departmentName, int ogLevel, int userNumber, long dingtalkId, long dingtalkParentId)
        {
            var strSql = @" SELECT * FROM crm_func_department_add_dingtalk(@topdeptId,@deptName, @oglevel, @userNo,@deptlanguage::jsonb,@dingtalk_id,@dingtalk_parentid) ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("topdeptId",departmentId),
                new NpgsqlParameter("deptName",departmentName),
                new NpgsqlParameter("oglevel",ogLevel),
                new NpgsqlParameter("userNo", userNumber),
                new NpgsqlParameter("deptlanguage","{}"),
                new NpgsqlParameter("dingtalk_id",dingtalkId),
                new NpgsqlParameter("dingtalk_parentid",dingtalkParentId)
            };

            var result = ExecuteNonQuery(strSql, param);
            return result;
        }




        public int UserAdd(AccountUserRegistMapper registEntity, string dingTalkUserID, string dingTalkNick, int userNumber)
        {
            var strSql = @" SELECT * FROM crm_func_account_userinfo_add_dingtalk(@accountName, @accountPwd, @accessType, @userName,  @userIcon, 
                                                                     @userPhone, @userJob, @deptid,@namepinyin, @email,
                                                                     @joineddate,@birthday,@remark,@sex,@tel, 
                                                                     @status,@workCode,@NextMustChangePwd,@dingTalkUserID,@dingTalkNick,@userNo) ";
            //pwd salt security
            var securityPwd = "eddb1beeab2bcb5bdafacc05536eca920858a6c8adfb254ca3f0e99600a55757";
            var param = new DbParameter[]
            {
                   new NpgsqlParameter("AccountName",registEntity.AccountName),
                   new NpgsqlParameter("AccountPwd",securityPwd),
                   new NpgsqlParameter("AccessType",registEntity.AccessType),
                   new NpgsqlParameter("UserName",registEntity.UserName),
                   new NpgsqlParameter("UserIcon",registEntity.UserIcon),
                   new NpgsqlParameter("UserPhone",registEntity.UserPhone),
                   new NpgsqlParameter("UserJob",registEntity.UserJob),
                   new NpgsqlParameter("DeptId",registEntity.DeptId),
                   new NpgsqlParameter("NamePinYin",PinYinConvert.ToChinese(registEntity.UserName, true)),
                   new NpgsqlParameter("Email",registEntity.Email),
                   new NpgsqlParameter("BirthDay",registEntity.BirthDay),
                   new NpgsqlParameter("JoinedDate",registEntity.JoinedDate),
                   new NpgsqlParameter("Remark",registEntity.Remark),
                   new NpgsqlParameter("Sex",registEntity.Sex),
                   new NpgsqlParameter("Tel",registEntity.Tel),
                   new NpgsqlParameter("Status",registEntity.Status),
                   new NpgsqlParameter("WorkCode",registEntity.WorkCode),
                   new NpgsqlParameter("NextMustChangePwd",registEntity.NextMustChangePwd),
                   new NpgsqlParameter("dingTalkUserID",dingTalkUserID),
                   new NpgsqlParameter("dingTalkNick",dingTalkNick),
                   new NpgsqlParameter("UserNo",userNumber)
            };
            var result = ExecuteQuery(strSql, param);
            Console.Out.WriteLine(JsonConvert.SerializeObject(result));
            return 1;
        }


        public bool IsDepartmentExist(string departmentName, long dtDepartmetnId)
        {
            string strSql = @"  select count(1) 
                                from crm_sys_department 
                                where deptname =@deptname
                                and dingtalk_id =@dingtalk_id ";

            var param = new DbParameter[]
              {
                   new NpgsqlParameter("@deptname",departmentName),
                   new NpgsqlParameter("@dingtalk_id",dtDepartmetnId),
              };

            var result = DBHelper.ExecuteScalar("", strSql, param, System.Data.CommandType.Text);
            var count = long.Parse(result.ToString());
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public bool IsUserExist(string userMobile)
        {

            string strSql = @"  select count(1) 
                                from crm_sys_userinfo 
                                where userphone=@userphone ";

            var param = new DbParameter[]
             {
                   new NpgsqlParameter("@userphone",userMobile)
              };

            var result = DBHelper.ExecuteScalar("", strSql, param, System.Data.CommandType.Text);
            var count = long.Parse(result.ToString());
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public Guid GetDepartmentId(string departmentName, long dingTalkId)
        {
            string strSql = @"  select deptid
                                from crm_sys_department 
                                where deptname =@deptname
                                and dingtalk_id =@dingtalk_id ";

            var param = new DbParameter[]
              {
                   new NpgsqlParameter("@deptname",departmentName),
                   new NpgsqlParameter("@dingtalk_id",dingTalkId),
              };



            var result = DBHelper.ExecuteQuery<DepartmentEditModel>("", strSql, param, System.Data.CommandType.Text).FirstOrDefault();
            if (result != null && result.DeptId != null)
            {
                return result.DeptId;

            }
            else
            {
                return Guid.Empty;
            }

        }
        /// <summary>
        /// 添加部门
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="departmentName"></param>
        /// <param name="ogLevel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public int DepartmentAdd(Guid departmentId, string departmentName, int ogLevel, int userNumber)
        {
            var strSql = @" SELECT * FROM crm_func_department_add(@topdeptId,@deptName, @oglevel, @userNo,@deptlanguage) ";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("topdeptId",departmentId),
                new NpgsqlParameter("deptName",departmentName),
                new NpgsqlParameter("oglevel",ogLevel),
                new NpgsqlParameter("userNo", userNumber),
                new NpgsqlParameter("deptlanguage",string.Empty)
            };

            var result = ExecuteNonQuery(strSql, param, null, System.Data.CommandType.StoredProcedure);
            return result;
        }



        public int UserAdd(AccountUserRegistMapper registEntity, int userNumber)
        {
            var strSql = @" SELECT * FROM crm_func_account_userinfo_add(@accountName, @accountPwd, @accessType, @userName,  @userIcon, 
                                                                     @userPhone, @userJob, @deptid,@namepinyin, @email,
                                                                     @joineddate,@birthday,@remark,@sex,@tel, 
                                                                     @status,@workCode,@NextMustChangePwd,@userNo) ";
            //pwd salt security
            var securityPwd = string.Empty;
            var param = new DbParameter[]
            {
                    new NpgsqlParameter("AccountName",registEntity.AccountName),
                   new NpgsqlParameter("AccountPwd",securityPwd),
                   new NpgsqlParameter("AccessType",registEntity.AccessType),
                   new NpgsqlParameter("UserName",registEntity.UserName),
                   new NpgsqlParameter("UserIcon",registEntity.UserIcon),
                   new NpgsqlParameter("UserPhone",registEntity.UserPhone),
                   new NpgsqlParameter("UserJob",registEntity.UserJob),
                   new NpgsqlParameter("DeptId",registEntity.DeptId),
                   new NpgsqlParameter("NamePinYin",PinYinConvert.ToChinese(registEntity.UserName, true)),
                   new NpgsqlParameter("Email",registEntity.Email),
                   new NpgsqlParameter("BirthDay",registEntity.BirthDay),
                   new NpgsqlParameter("JoinedDate",registEntity.JoinedDate),
                   new NpgsqlParameter("Remark",registEntity.Remark),
                   new NpgsqlParameter("Sex",registEntity.Sex),
                   new NpgsqlParameter("Tel",registEntity.Tel),
                   new NpgsqlParameter("Status",registEntity.Status),
                   new NpgsqlParameter("WorkCode",registEntity.WorkCode),
                   new NpgsqlParameter("NextMustChangePwd",registEntity.NextMustChangePwd),
                   new NpgsqlParameter("UserNo",userNumber)
            };
            var result = ExecuteNonQuery(strSql, param, null, System.Data.CommandType.StoredProcedure);
            return result;
        }


        public bool IsDepartmentExist(string departmentName)
        {

            return true;


        }


        public bool IsUserExist(int userMobile)
        {

            return true;
        }


        public Guid AddGroup(String groupName, String dingDingGroupId, int userId)
        {

            var sql = "INSERT INTO  \"crm_sys_role_group\" (\"rolegroupname\", \"grouptype\", \"recorder\", \"reccreator\", \"recupdator\",dingdinggroupid) VALUES (@groupname, '1',(select ( max(recorder)+1) from crm_sys_role_group),  @userid, @userid,@dingdinggroupid) returning rolegroupid;";//, \"rolegroupname_lang\" '{\"cn\": \"@\", \"en\": \"fsdfdsf54\", \"tw\": \"繁体\"}'
            var delSql = "delete from crm_sys_role_group where dingdinggroupid=@dingdinggroupid";
            var param = new DynamicParameters();
            param.Add("userid", userId);
            param.Add("dingdinggroupid", dingDingGroupId);
            param.Add("groupname", groupName);
            DataBaseHelper.ExecuteNonQuery(delSql, param);
            Guid id = DataBaseHelper.ExecuteScalar<Guid>(sql, param);
            return id;
        }

        public Guid AddRole(String roleName, String dingDingRoleId, int userId)
        {

            var sql = "INSERT INTO  \"crm_sys_role\" (rolename,roletype,rolepriority,roleremark,recorder,reccreator,recupdator,dingdingroleid) VALUES (@rolename, 1,0,'',(select ( max(recorder)+1) from crm_sys_role ),@userid, @userid,@dingdingroleid) returning roleid;";//, \"rolegroupname_lang\" '{\"cn\": \"@\", \"en\": \"fsdfdsf54\", \"tw\": \"繁体\"}'
            var delSql = "delete from crm_sys_role where dingdingroleid=@dingdingroleid";
            var param = new DynamicParameters();
            param.Add("userid", userId);
            param.Add("dingdingroleid", dingDingRoleId);
            param.Add("rolename", roleName);
            DataBaseHelper.ExecuteNonQuery(delSql, param);
            Guid id = DataBaseHelper.ExecuteScalar<Guid>(sql, param);
            return id;
        }

        public bool AddRoleGroup(Guid groupId, Guid roleId, String dingDingGroupId, String dingDingRoleId, int userId)
        {
            var sql = "INSERT INTO  \"crm_sys_role_group_relate\" (roleid,rolegroupid,dingdingroleid,dingdinggroupid) VALUES (@roleid,@rolegroupid,@dingdingroleid,@dingdinggroupid);";//, \"rolegroupname_lang\" '{\"cn\": \"@\", \"en\": \"fsdfdsf54\", \"tw\": \"繁体\"}'
            var delSql = "delete from crm_sys_role_group_relate where dingdinggroupid=@dingdinggroupid and dingdingroleid=@dingdingroleid";
            var param = new DynamicParameters();
            param.Add("roleid", roleId);
            param.Add("rolegroupid", groupId);
            param.Add("dingdinggroupid", dingDingGroupId);
            param.Add("dingdingroleid", dingDingRoleId);
            DataBaseHelper.ExecuteNonQuery(delSql, param);
            int count = DataBaseHelper.ExecuteNonQuery(sql, param);
            if (count > 0)
                return true;
            else
                return false;
        }

    }
}
