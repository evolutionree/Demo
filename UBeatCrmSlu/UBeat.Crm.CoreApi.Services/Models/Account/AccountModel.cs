using Microsoft.AspNetCore.Http;
using System;

namespace UBeat.Crm.CoreApi.Services.Models.Account
{
    public class AccountLoginModel
    {
        public string AccountName { get; set; }
        /// <summary>
        /// 密码文本，若不加密则为明文，若加密，则使用“时间戳_密码”的格式
        /// </summary>
        public string AccountPwd { get; set; }

        /// <summary>
        /// 加密方式，0=不加密，1=RSA方式加密
        /// </summary>
        public int EncryptType { set; get; }


        /// <summary>
        /// 设备型号：iPhone 6s
        /// </summary>
        public string DeviceModel { get; set; }
        /// <summary>
        /// 设备系统版本
        /// </summary>
        public string OsVersion { get; set; }
        /// <summary>
        /// 设备型唯一标识符
        /// </summary>
        public string UniqueId { get; set; }
        
        public string sendcode { get; set; }
    }

    public class AccountLoginOutModel
    {
        public string DeviceId { get; set; }
        public string Token { get; set; }
    }

    public class AccountRegistModel
    {
        public string AccountName { get; set; }
        /// <summary>
        /// 密码文本，若不加密则为明文，若加密，则使用“时间戳_密码”的格式
        /// </summary>
        public string AccountPwd { get; set; }

        /// <summary>
        /// 加密方式，0=不加密，1=RSA方式加密
        /// </summary>
        public int EncryptType { set; get; }

        public string UserName { get; set; }
        public string AccessType { get; set; }
        public string UserIcon { get; set; }
        public string UserPhone { get; set; }
        public string UserJob { get; set; }

        public DateTime? JoinedDate { get; set; }
        public Guid DeptId { get; set; }
        public string WorkCode { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Email { get; set; }
        public string Remark { get; set; }
        public int Sex { get; set; }
        public string Tel { get; set; }
        public int Status { get; set; }
        public int NextMustChangePwd { get; set; }
    }

    public class AccountEditModel
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public DateTime? BirthDay { get; set; }
        public DateTime? JoinedDate { get; set; }
        public string WorkCode { get; set; }
        public string Email { get; set; }
        public string Remark { get; set; }
        public int Sex { get; set; }
        public string Tel { get; set; }
        public string UserIcon { get; set; }
        public string UserJob { get; set; }
        public string UserPhone { get; set; }
        public string UserName { get; set; }
        public string AccessType { get; set; }
        public Guid DeptId { get; set; }
        public int Status { get; set; }
    }

    public class AccountPasswordModel
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        /// <summary>
        /// 密码文本，若不加密则为明文，若加密，则使用“时间戳_密码”的格式
        /// </summary>
        public string AccountPwd { get; set; }
        /// <summary>
        /// 密码文本，若不加密则为明文，若加密，则使用“时间戳_密码”的格式
        /// </summary>
        public string OrginPwd { get; set; }
        
        /// <summary>
        /// 加密方式，0=不加密，1=RSA方式加密
        /// </summary>
        public int EncryptType { set; get; }
    }
    public class AccountModel
    {
        public string UserId { get; set; }


        /// <summary>
        /// 密码文本，若不加密则为明文，若加密，则使用“时间戳_密码”的格式
        /// </summary>
        public string Pwd { get; set; }

        /// <summary>
        /// 加密方式，0=不加密，1=RSA方式加密
        /// </summary>
        public int EncryptType { set; get; }
    }
    public class AccountModifyPhotoModel
    {
        public string UserIcon { get; set; }
    }

    public class AccountQueryModel
    {
        public int IsControl { get; set; }
        public string UserName { get; set; }
        public string UserPhone { get; set; }
        public int RecStatus { get; set; }
        public Guid DeptId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int IsCrmUser { get; set; }
    }
    public class AccountQueryForControlModel
    {
        public string KeyWord { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
    public class AccountStatusModel
    {
        public int UserId { get; set; }

        public int Status { get; set; }
    }

    public class AccountDepartmentModel
    {
        public DateTime EffectiveDate { get; set; }
        public int UserId { get; set; }
        public string DeptId { get; set; }
    }

    public class DeptOrderbyModel
    {
        public string ChangeDeptId { get; set; }
        public string DeptId { get; set; }

    }


    public class DeptDisabledModel
    {
        public int RecStatus { get; set; }
        public string DeptId { get; set; }

    }

    public class SetLeaderModel
    {
        // 0 否 1 是
        public int UserId { get; set; }
        public int IsLeader { get; set; }
    }

    public class SetIsCrmModel
    {
        // 0 否 1 是
        public int UserId { get; set; }
        public int IsCrmUser { get; set; }
    }

    public class UpdateSoftwareModel
    {
        public int ClientType { set; get; }

        public int VersionNo { set; get; }

        public int BuildNo { set; get; }
    }
    public class AuthTokenRequireModel
    {
        public string AppId { get; set; }
        public string SecurityCode { get; set; }
        public string RandomCode { get; set; }
        public string AccessToken { get; set; }
        public UserInfo LoginUserInfo { get; set; }
        public AuthTokenRequireModel()
        {
            LoginUserInfo = new UserInfo();
        }
    }
    public class CheckTokenModel
    {
        public string AccessToken { get; set; }
        public string Md5 { get; set; }
    }
    public class LinceseImportParamInfo
    {
        public IFormFile Data { set; get; }
        public int IsImport { get; set; }
    }
    
}
