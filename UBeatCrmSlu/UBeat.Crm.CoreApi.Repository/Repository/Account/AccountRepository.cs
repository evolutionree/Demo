using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Repository.Utility;
using Dapper;
using System;
using UBeat.Crm.CoreApi.IRepository;
using System.Data.Common;
using Npgsql;
using System.Linq;

namespace UBeat.Crm.CoreApi.Repository.Repository.Account
{
    public class AccountRepository : RepositoryBase, IAccountRepository
    {
        private readonly string _passwordSalt;

        public AccountRepository(IConfigurationRoot config)
        {
            _passwordSalt = config.GetSection("Securitys").GetValue<string>("PwdSalt");
        }

        public AccountUserMapper GetUserInfo(string accountName, string accountPwd)
        {
            var sql = @"
                SELECT u.userid,u.username,a.accesstype,a.accountpwd,a.recstatus FROM crm_sys_account AS a
                LEFT JOIN crm_sys_account_userinfo_relate AS r ON a.accountid = r.accountid
                LEFT JOIN crm_sys_userinfo AS u ON r.userid = u.userid
                WHERE accountname = @accountName LIMIT 1
            ";

            var param = new { AccountName = accountName };
            var userInfo = DataBaseHelper.QuerySingle<AccountUserMapper>(sql, param);
            return userInfo;
        }
        /// <summary>
        /// 统计注册总人数
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="accountPwd"></param>
        /// <returns></returns>
        public int GetUserCount()
        {
            var sql = @"select count(1) nums from crm_sys_userinfo
            ";
            var count = DataBaseHelper.QuerySingle<int>(sql);
            return count;
        }

        public OperateResult RegistUser(AccountUserRegistMapper registEntity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_account_userinfo_add(@accountName, @accountPwd, @accessType, @userName, @userIcon, @userPhone, @userJob, @deptid,@namepinyin, @email,@joineddate,@birthday,@remark,@sex,@tel, @status,@workCode,@userNo)
            ";

            //pwd salt security
            var securityPwd = SecurityHash.GetPwdSecurity(registEntity.AccountPwd, _passwordSalt);
            var param = new
            {
                AccountName = registEntity.AccountName,
                AccountPwd = securityPwd,
                AccessType = registEntity.AccessType,
                UserName = registEntity.UserName,
                UserIcon = registEntity.UserIcon,
                UserPhone = registEntity.UserPhone,
                UserJob = registEntity.UserJob,
                DeptId = registEntity.DeptId,
                NamePinYin = PinYinConvert.ToChinese(registEntity.UserName, true),
                Email = registEntity.Email,
                BirthDay = registEntity.BirthDay,
                JoinedDate = registEntity.JoinedDate,
                Remark = registEntity.Remark,
                Sex = registEntity.Sex,
                Tel = registEntity.Tel,
                Status = registEntity.Status,
                WorkCode = registEntity.WorkCode,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> GetUserList(PageParam pageParam,
            AccountUserQueryMapper searchParam, int userNumber)
        {
            /*
            var procName = @"crm_func_account_userinfo_dept_list";

            var dataNames = new List<string> {"PageData", "PageCount"};

            var paramList = new DynamicParameters();
            paramList.Add("_username", searchParam.UserName);
            paramList.Add("_userphone", searchParam.UserPhone);
            paramList.Add("_deptid", searchParam.DeptId);
            paramList.Add("_status", searchParam.RecStatus);
            paramList.Add("_pageindex", pageParam.PageIndex);
            paramList.Add("_pagesize", pageParam.PageSize);
            paramList.Add("_userno", userNumber);
            */

            //这里提供两种方式
            var procName =
                "SELECT crm_func_account_userinfo_dept_list(@userName,@userPhone,@deptId,@recStatus,@pageIndex,@pageSize,@userNo)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new
            {
                UserName = searchParam.UserName,
                UserPhone = searchParam.UserPhone,
                DeptId = searchParam.DeptId,
                RecStatus = searchParam.RecStatus,
                PageIndex = pageParam.PageIndex,
                PageSize = pageParam.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> GetUserPowerList(PageParam pageParam,
          AccountUserQueryMapper searchParam, int userNumber)
        {
            /*
            var procName = @"crm_func_account_userinfo_dept_list";

            var dataNames = new List<string> {"PageData", "PageCount"};

            var paramList = new DynamicParameters();
            paramList.Add("_username", searchParam.UserName);
            paramList.Add("_userphone", searchParam.UserPhone);
            paramList.Add("_deptid", searchParam.DeptId);
            paramList.Add("_status", searchParam.RecStatus);
            paramList.Add("_pageindex", pageParam.PageIndex);
            paramList.Add("_pagesize", pageParam.PageSize);
            paramList.Add("_userno", userNumber);
            */

            //这里提供两种方式
            var procName =
                "SELECT crm_func_account_userinfo_dept_list_power(@userName,@userPhone,@deptId,@recStatus,@pageIndex,@pageSize,@userNo)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new
            {
                UserName = searchParam.UserName,
                UserPhone = searchParam.UserPhone,
                DeptId = searchParam.DeptId,
                RecStatus = searchParam.RecStatus,
                PageIndex = pageParam.PageIndex,
                PageSize = pageParam.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> GetUserPowerListForControl(PageParam pageParam,
       AccountUserQueryForControlMapper searchParam, int userNumber)
        {

            //这里提供两种方式
            var procName =
                "SELECT crm_func_account_userinfo_dept_list_power_for_control(@keyword,@pageIndex,@pageSize,@userNo)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new
            {
                KeyWord = searchParam.KeyWord,
                PageIndex = pageParam.PageIndex,
                PageSize = pageParam.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult EditUser(AccountUserEditMapper editEntity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_account_userinfo_edit(@accountId,@accountName, @accessType, @userName, @userIcon, @userPhone, @userJob, @deptid,@namepinyin,@email,@joineddate,@birthday,@remark,@sex,@tel, @status,@workCode,@userNo)
            ";

            var param = new
            {
                AccountId = editEntity.AccountId,
                AccountName = editEntity.AccountName,
                AccessType = editEntity.AccessType,
                UserName = editEntity.UserName,
                UserIcon = editEntity.UserIcon,
                UserPhone = editEntity.UserPhone,
                UserJob = editEntity.UserJob,
                DeptId = editEntity.DeptId,
                NamePinYin = PinYinConvert.ToChinese(editEntity.UserName, true),
                Email = editEntity.Email,
                BirthDay = editEntity.BirthDay,
                JoinedDate = editEntity.JoinedDate,
                Remark = editEntity.Remark,
                Sex = editEntity.Sex,
                Tel = editEntity.Tel,
                Status = editEntity.Status,
                WorkCode = editEntity.WorkCode,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult PwdUser(AccountUserPwdMapper pwdEntity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_account_userinfo_changepwd(@accountId,@userId, @accountPwd, @orginPwd, @userNo)
            ";

            //pwd salt security
            var securityPwd = SecurityHash.GetPwdSecurity(pwdEntity.AccountPwd, _passwordSalt);
            var orginSecurityPwd = SecurityHash.GetPwdSecurity(pwdEntity.OrginPwd, _passwordSalt);

            var param = new
            {
                AccountId = pwdEntity.AccountId,
                UserId = pwdEntity.UserId,
                AccountPwd = securityPwd,
                OrginPwd = orginSecurityPwd,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult ReConvertPwd(AccountMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_account_userinfo_reconvertpwd(@userid,@securitypwd,@userNo)
            ";

            //pwd salt security
            var securityPwd = SecurityHash.GetPwdSecurity(entity.Pwd, _passwordSalt);

            var param = new
            {
                UserId = entity.UserId,
                SecurityPwd = securityPwd,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> GetUserInfo(int userNumber)
        {
            var procName =
               "SELECT crm_func_account_userinfo_fetch(@userNo)";

            var dataNames = new List<string> { "User", "Role", "Vocation" };
            var param = new
            {
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult ModifyPhoto(AccountUserPhotoMapper iconEntity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_account_userinfo_modify_photo(@userIcon,@userNo)
            ";

            var param = new
            {
                UserIcon = iconEntity.UserIcon,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateAccountStatus(AccountStatusMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_user_status(@userid,@status,@userNo)
            ";

            var param = new DynamicParameters();
            param.Add("userid", entity.UserId);
            param.Add("status", entity.Status);
            param.Add("userNo", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult UpdateAccountDept(AccountDepartmentMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_dept_change(@userid,@deptid,@userNo)
            ";

            var param = new DynamicParameters();
            param.Add("userid", entity.UserId);
            param.Add("deptid", entity.DeptId);
            param.Add("userNo", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult OrderByDept(DeptOrderbyMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_department_orderby(@changedeptid,@deptid,@userNo)
            ";

            var param = new DynamicParameters();
            param.Add("changedeptid", entity.ChangeDeptId);
            param.Add("deptid", entity.DeptId);
            param.Add("userNo", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public OperateResult DisabledDept(DeptDisabledMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_department_disabled(@deptid,@status,@userNo)
            ";

            var param = new DynamicParameters();
            param.Add("deptid", entity.DeptId);
            param.Add("status", entity.RecStatus);
            param.Add("userNo", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }
        public OperateResult SetLeader(SetLeaderMapper entity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_set_leader(@userid,@isleader,@userNo)
            ";

            var param = new DynamicParameters();
            param.Add("userid", entity.UserId);
            param.Add("isleader", entity.IsLeader);
            param.Add("userNo", userNumber);
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }

        public UpdateSoftwareEntity UpdateSoftware(DbTransaction tran, int clientType, int versionNo, int buildNo, int userNumber)
        {
            var sql = @"
               select * from crm_sys_updatesoftware WHERE clienttype=@_clienttype AND (versionno>@_versionno  OR (versionno=@_versionno AND buildno>@_buildno)) ORDER BY versionno DESC,buildno DESC LIMIT 1";

            var param = new DynamicParameters();
            param.Add("_clienttype", clientType);
            param.Add("_versionno", versionNo);
            param.Add("_buildno", buildNo);
            var result = DataBaseHelper.QuerySingle<UpdateSoftwareEntity>(tran.Connection, sql, param);
            return result;
        }


        public AccountUserInfo GetAccountUserInfo(int userNumber)
        {
            var sql = @"
              SELECT ur.accountid,a.accountname,ur.userid,u.username,u.namepinyin AS UserNamePinyin,ur.deptid AS DepartmentId,d.deptcode AS DepartmentCode,d.deptname AS DepartmentName,d.pdeptid AS PDepartmentId 
                FROM crm_sys_account_userinfo_relate AS ur
                LEFT JOIN crm_sys_department AS d ON ur.deptid=d.deptid
                LEFT JOIN crm_sys_userinfo AS u ON u.userid=ur.userid
                LEFT JOIN crm_sys_account AS a ON a.accountid=ur.accountid
                WHERE ur.recstatus = 1 AND ur.userid = @userid";

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("userid", userNumber),
                    };

            var result = DBHelper.ExecuteQuery<AccountUserInfo>("", sql, param);
            return result == null ? null : result.FirstOrDefault();
        }

        public List<UserInfo> GetAllUserInfoList()
        {
            var sql = @"SELECT userid, username,namepinyin,usericon,usersex FROM crm_sys_userinfo";
          
            return ExecuteQuery<UserInfo>(sql, null);
        }
        public List<UserInfo> GetUserInfoList(List<int> userids)
        {
            var sql = @"SELECT userid, username,namepinyin,usericon,usersex FROM crm_sys_userinfo WHERE userid =ANY(@userids)";

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("userids", userids.ToArray()),
                    };
            return ExecuteQuery<UserInfo>(sql, param);

        }

        public UserInfo GetUserInfoById(int userid)
        {
            var sql = @"SELECT userid, username,namepinyin,usericon,usersex FROM crm_sys_userinfo WHERE userid =@userid";

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("userid", userid),
                    };
            return ExecuteQuery<UserInfo>(sql, param).FirstOrDefault();

        }
        /// <summary>
        /// 用于更新版本信息
        /// </summary>
        /// <param name="tran"></param>
        /// <param name="apkName"></param>
        /// <param name="iMainVersion"></param>
        /// <param name="iSubVersion"></param>
        /// <param name="iBuildCode"></param>
        /// <param name="userNum"></param>
        public void UpdateSoftwareVersion(DbTransaction tran, string apkName, int iMainVersion, int iSubVersion, int iBuildCode, string serverUrl ,int userNum)
        {
            var sql = string.Format(@"
               insert into crm_sys_updatesoftware(
                    clienttype,clienttypename,versionno,versionname,updateurl,
                    enforceupdate,buildno,versionstatus,remark) 
               values(2,'Android',{0},'V{0}.{1}.{2}','http://{4}/deploy/{3}','f',{2},1,'修复Bugs')", iMainVersion,iSubVersion,iBuildCode,apkName,serverUrl);

            ExecuteNonQuery(sql, new DbParameter[] { }, tran);
        }
    }
}