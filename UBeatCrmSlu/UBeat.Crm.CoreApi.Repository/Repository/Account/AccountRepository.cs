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

        public AccountUserMapper GetUserInfo(string accountName)
        {
            var sql = @"
                SELECT u.userid,u.username,a.accesstype,a.accountpwd,a.recstatus,a.nextmustchangepwd ,a.lastchangedpwdtime FROM crm_sys_account AS a
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
            var sql = @"select count(1) nums from crm_sys_userinfo";
            var count = DataBaseHelper.QuerySingle<int>(sql);
            return count;
        }

        public bool CheckDeviceHadBind(string uniqueId, int userNumber)
        {
            var sql = @"select exists(select recid from crm_sys_device_bind where userid = @userid and uniqueid != @uniqueid and recstatus = 1)";
            var param = new
            {
                userid = userNumber,
                uniqueid = uniqueId,
            };
            var isBinded = DataBaseHelper.QuerySingle<bool>(sql, param);
            return isBinded;
        }

        public void AddDeviceBind(string deviceModel,string osVersion, string uniqueId, int userNumber)
        {
            var sql = @"select * from crm_sys_device_bind(@devicemodel, @osversion, @uniqueid, @userid)";

            var param = new
            {
                devicemodel = deviceModel,
                osversion = osVersion,
                uniqueid = uniqueId,
                userid = userNumber
            };
             DataBaseHelper.QuerySingle<OperateResult>(sql, param);
        }

        public bool UnDeviceBind(string recordIds, int userNumber)
        {
            var sql = @"update crm_sys_device_bind set recstatus = 0 where position(recid::text in @recordIds)> 0 and recstatus = 1";
            var param = new
            {
                recordIds = recordIds,
                //userid = userNumber,
            };
            DataBaseHelper.QuerySingle<int>(sql, param);
            return true;
        }

        public Dictionary<string, List<IDictionary<string, object>>> DeviceBindList(DeviceBindInfo deviceBindQuery, int userNumber)
        {
            var procName ="SELECT crm_func_devicebind_list(@devicemodel, @osversion, @uniqueid, @username, @status, @pageindex, @pagesize, @userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new
            {
                DeviceModel = deviceBindQuery.DeviceModel,
                OsVersion = deviceBindQuery.OsVersion,
                UniqueId = deviceBindQuery.UniqueId,
                UserName = deviceBindQuery.UserName,
                Status = deviceBindQuery.RecStatus,
                PageIndex = deviceBindQuery.PageIndex,
                PageSize = deviceBindQuery.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult RegistUser(AccountUserRegistMapper registEntity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_account_userinfo_add(@accountName, @accountPwd, @accessType, @userName, @userIcon, @userPhone, @userJob, @deptid,@namepinyin, @email,@joineddate,@birthday,@remark,@sex,@tel, @status,@workCode,@NextMustChangePwd,@userNo)
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
                NextMustChangePwd  = registEntity.NextMustChangePwd,
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

        public Dictionary<string, List<IDictionary<string, object>>> GetUserInfo(int userNumber,int currentUserId)
        {
            var procName =
               "SELECT crm_func_account_userinfo_fetch(@userNo,@CurrentUserId)";

            var dataNames = new List<string> { "User", "Role", "Vocation" };
            var param = new
            {
                UserNo = userNumber,
                CurrentUserId = currentUserId
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
              SELECT ur.accountid,a.accountname,ur.userid,u.username,u.namepinyin AS UserNamePinyin,ur.deptid AS DepartmentId,d.deptcode AS DepartmentCode,d.deptname AS DepartmentName,d.pdeptid AS PDepartmentId,u.dduserid
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
            var sql = @"SELECT userid, username,namepinyin,usericon,usersex,dduserid FROM crm_sys_userinfo WHERE userid =ANY(@userids)";

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
               values(2,'Android',{0},'V{0}.{1}.{2}','http://{4}/deploy/{3}','f',{2},1,'[""修复Bugs""]')", iMainVersion,iSubVersion,iBuildCode,apkName,serverUrl);

            ExecuteNonQuery(sql, new DbParameter[] { }, tran);
        }


        /// <summary>
        /// 获取企业信息
        /// </summary>
        /// <returns></returns>
        public EnterpriseInfo GetEnterpriseInfo()
        {
            var sql = @"SELECT * FROM crm_sys_enterprise LIMIT 1";

           
            return ExecuteQuery<EnterpriseInfo>(sql, null).FirstOrDefault();
        }

        #region 安全机制
        /// <summary>
        /// 查询密码策略
        /// </summary>
        /// <param name="userNumber"></param>
        /// <param name="tran"></param>
        /// <returns></returns>
        public Dictionary<string,object> GetPwdPolicy(int userNumber, DbTransaction tran)
        {
            string sql = @"select * from crm_sys_security_pwdpolicy";
            return ExecuteQuery(sql, new DbParameter[] { }, tran).FirstOrDefault();
        }

        /// <summary>
        /// 保存密码策略
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <param name="tran"></param>
        public void SavePwdPolicy(PwdPolicy data, int userNumber, DbTransaction tran)
        {
            #region 删除
            string delSql = @"delete from crm_sys_security_pwdpolicy";
            ExecuteNonQuery(delSql, new DbParameter[] { }, tran);
            #endregion
            #region 添加
            string insertSql = @"insert into crm_sys_security_pwdpolicy(recid,policy,recupdated,recupdator) values (@RecId,@Policy,@RecUpdated,@RecUpdator)";
            var param = new DbParameter[] {
                new NpgsqlParameter("RecId",Guid.NewGuid()),
                new NpgsqlParameter ("Policy",Newtonsoft.Json.JsonConvert.SerializeObject(data)),
                new NpgsqlParameter("RecUpdated",DateTime.Now),
                new NpgsqlParameter("RecUpdator",userNumber)
            };
            ExecuteNonQuery(insertSql, param, tran);
            #endregion
        }

        public string EncryPwd(string plaintext, int userNumber)
        {
            return SecurityHash.GetPwdSecurity(plaintext, _passwordSalt);
        }
        /// <summary>
        /// 获取历史密码
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public List<HistoryPwd> GetHistoryPwd(int count,int userId)
        {
            if (count <= 0) count =3; 
            string sql = @"select * from crm_sys_security_historypwd where userid = @userid order by reccreated desc limit "+ count;
            var param = new DbParameter[]
            {
                new NpgsqlParameter("userid",userId)
            };
            return ExecuteQuery<HistoryPwd>(sql, param, null);
        }

        public void SetPasswordInvalid(List<int> userList, int userId, DbTransaction tran)
        {
            try
            {
                string ids = string.Join(',', userList);
                string strSQL = @"update crm_sys_account a  set nextmustchangepwd =1 where a.accountid  in (
                                    select b.accountid from crm_sys_account_userinfo_relate b
                                    where b.recstatus = 1 and b.userid in ("+ ids+ @")
                                    ) ";
                DbParameter[] ps = new DbParameter[] {
                    
                };
                ExecuteNonQuery(strSQL, ps, tran);
            }
            catch (Exception ex) {
                
            }
        }

        public AccountUserInfo GetAllAccountUserInfo(int userNumber)
        {
            var sql = @"
              SELECT a.accesstype,ur.accountid,a.accountname,ur.userid,u.username,u.namepinyin AS UserNamePinyin,ur.deptid AS DepartmentId,d.deptcode AS DepartmentCode,d.deptname AS DepartmentName,d.pdeptid AS PDepartmentId 
                FROM crm_sys_account_userinfo_relate AS ur
                LEFT JOIN crm_sys_department AS d ON ur.deptid=d.deptid
                LEFT JOIN crm_sys_userinfo AS u ON u.userid=ur.userid
                LEFT JOIN crm_sys_account AS a ON a.accountid=ur.accountid
                WHERE   ur.userid = @userid";

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("userid", userNumber),
                    };

            var result = DBHelper.ExecuteQuery<AccountUserInfo>("", sql, param);
            return result == null ? null : result.FirstOrDefault();
        }
        public AccountUserInfo GetAllAccountUserInfoByAccountId(int accountId)
        {
            var sql = @"
              SELECT a.accesstype,ur.accountid,a.accountname,ur.userid,u.username,u.namepinyin AS UserNamePinyin,ur.deptid AS DepartmentId,d.deptcode AS DepartmentCode,d.deptname AS DepartmentName,d.pdeptid AS PDepartmentId 
                FROM crm_sys_account_userinfo_relate AS ur
                LEFT JOIN crm_sys_department AS d ON ur.deptid=d.deptid and ur.recstatus=1
                LEFT JOIN crm_sys_userinfo AS u ON u.userid=ur.userid
                LEFT JOIN crm_sys_account AS a ON a.accountid=ur.accountid
                WHERE   a.accountid = @accountid";

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("accountid", accountId),
                    };

            var result = DBHelper.ExecuteQuery<AccountUserInfo>("", sql, param);
            return result == null ? null : result.FirstOrDefault();
        }

        public int GetLicenseUserCount()
        {
            var sql = @"select count(1) nums from crm_sys_account  where recstatus = 1 and accesstype <>'99' ";
            var count = DataBaseHelper.QuerySingle<int>(sql);
            return count;
        }
        #endregion
    }
}