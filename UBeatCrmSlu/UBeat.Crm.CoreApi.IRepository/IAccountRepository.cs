using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IAccountRepository : IBaseRepository
    {
         int GetUserCount();

        bool CheckDeviceHadBind(string uniqueId, int userNumber);

        void AddDeviceBind(string deviceModel, string osVersion, string uniqueId, int userNumber);

        bool UnDeviceBind(string recordIds, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> DeviceBindList(DeviceBindInfo deviceBindQuery, int userNumber);

        AccountUserMapper GetUserInfo(string accountName);
        AccountUserMapper GetUserInfoByLoginName(string loginName);
        OperateResult RegistUser(AccountUserRegistMapper registEntity, int userNumber);

        OperateResult EditUser(AccountUserEditMapper editEntity, int userNumber);

        OperateResult PwdUser(AccountUserPwdMapper pwdEntity, int userNumber);
        OperateResult ReConvertPwd(AccountMapper entity, int userNumber);

        OperateResult ModifyPhoto(AccountUserPhotoMapper iconEntity, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> GetUserList(PageParam pageParam, AccountUserQueryMapper searchParam, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> GetUserPowerList(PageParam pageParam, AccountUserQueryMapper searchParam, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> GetUserPowerListForControl(PageParam pageParam,
       AccountUserQueryForControlMapper searchParam, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> GetUserInfo(int userNumber,int currentUserId);

        OperateResult UpdateAccountStatus(AccountStatusMapper entity, int userNumber);

        OperateResult UpdateAccountDept(AccountDepartmentMapper entity, int userNumber);

        OperateResult OrderByDept(DeptOrderbyMapper entity, int userNumber);

        OperateResult DisabledDept(DeptDisabledMapper entity, int userNumber);

        OperateResult SetLeader(SetLeaderMapper entity, int userNumber);
        bool SetIsCrm(SetIsCrmMapper entity, int userNumber);

        UpdateSoftwareEntity UpdateSoftware(DbTransaction tran,int clientType, int versionNo, int buildNo, int userNumber);

        /// <summary>
        /// 获取用户的账号关联数据
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        AccountUserInfo GetAccountUserInfo(int userNumber);
        AccountUserInfo GetWcAccountUserInfo(string userNumber);
        List<UserInfo> GetAllUserInfoList();

        List<UserInfo> GetUserInfoList(List<int> userids);

        UserInfo GetUserInfoById(int userid);
        void UpdateSoftwareVersion(DbTransaction tran, string apkName, int iMainVersion, int iSubVersion, int iBuildCode,string serverUrl, int userNum);

        /// <summary>
        /// 获取企业信息
        /// </summary>
        /// <returns></returns>
        EnterpriseInfo GetEnterpriseInfo();
        //查询密码策略
        Dictionary<string, object> GetPwdPolicy(int userNumber,DbTransaction tran);
        //保存密码策略
        void SavePwdPolicy(PwdPolicy data, int userNumber, DbTransaction tran);
        string EncryPwd(string plaintext, int userNumber);
        //获取历史密码数据
        List<HistoryPwd> GetHistoryPwd(int count ,int userId);
        void SetPasswordInvalid(List<int> userList, int userId, DbTransaction tran);
        int GetLicenseUserCount();
    }
}
