using AutoMapper;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Text.RegularExpressions;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Core.Utility.Encrypt;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.FileService;
using UBeat.Crm.LicenseCore;
using static MessagePack.MessagePackSerializer;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class AccountServices : BasicBaseServices
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly string _passwordSalt;
        private readonly SecuritysModel _securitysModel;


        public AccountServices(IMapper mapper, IAccountRepository accountRepository, IConfigurationRoot config)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
            _securitysModel = config.GetSection("Securitys").Get<SecuritysModel>();
            _passwordSalt = _securitysModel.PwdSalt;

        }

        public int GetUserCount()
        {
            int count = _accountRepository.GetUserCount();
            return count;
        }

        public OutputResult<object> GetPublicKey()
        {
            if (_securitysModel == null || _securitysModel.RSAKeys == null)
            {
                return ShowError<object>("RSA密码对配置错误");
            }
            return new OutputResult<object>(
                new {
                    RSAPublicKey = _securitysModel.RSAKeys.PublicKey,
                    TimeStamp = DateTimeUtility.ConvertToTimeStamp(DateTime.Now)
                });
        }

        public OutputResult<object> checkHadBinded(AccountLoginModel loginModel, int userNumber)
        {
            if (loginModel.DeviceModel == null || loginModel.UniqueId == null || loginModel.OsVersion == null)
            {
                return ShowError<object>("获取设备信息失败");
            }

            bool hadBinded = _accountRepository.CheckDeviceHadBind(loginModel.UniqueId, userNumber);
            if (hadBinded)
            {
                return ShowError<object>("请使用绑定的设备进行登录");
            }
            else {
                //绑定设备
                _accountRepository.AddDeviceBind(loginModel.DeviceModel, loginModel.OsVersion, loginModel.UniqueId, userNumber);
                return new OutputResult<object>(!hadBinded);
            }
        }

        public OutputResult<object> UnDeviceBind(string recordIds, int userNumber)
        {
            bool unBind = _accountRepository.UnDeviceBind(recordIds, userNumber);
            return new OutputResult<object>(unBind);
        }

        public OutputResult<object> DeviceBindList(DeviceBindInfo deviceBindQuery, int userNumber)
        {
            return new OutputResult<object>(_accountRepository.DeviceBindList(deviceBindQuery, userNumber));
        }

        public OutputResult<object> Login(AccountLoginModel loginModel, AnalyseHeader header)
        {

            var userInfo = _accountRepository.GetUserInfo(loginModel.AccountName);

            if (userInfo == null)
            {
                return ShowError<object>("请输入正确的帐号");
            }

            if (userInfo.RecStatus == 0)
            {
                return ShowError<object>("该账户已停用");
            }


            //pwd salt security
            var securityPwd = SecurityHash.GetPwdSecurity(loginModel.AccountPwd, _passwordSalt);
            if (!securityPwd.Equals(userInfo.AccountPwd))
            {
                return ShowError<object>("密码输入错误");
            }

            //判断登录授权
            var isMobile = header.Device.ToLower().Contains("android")
                         || header.Device.ToLower().Contains("ios");
            var isWeb = header.Device.ToLower().Contains("web");

            var isAdmin = userInfo.AccessType == "10" ? true : false;

            var accessOk = true;
            //登录限制 00无限制 01WEB 02MOB 10ADMIN 11WEB + ADMIN 12MOB + ADMIN
            switch (userInfo.AccessType)
            {
                case "01":
                    {
                        if (isMobile || isAdmin || !isWeb)
                        {
                            accessOk = false;
                        }
                        break;
                    }
                case "02":
                    {
                        if (!isMobile || isAdmin || isWeb)
                        {
                            accessOk = false;
                        }
                        break;
                    }
                case "10":
                    {
                        if (isAdmin && isMobile)
                        {
                            accessOk = false;
                        }
                        break;
                    }
                case "11":
                    {
                        if (isMobile)
                        {
                            accessOk = false;
                        }
                        break;
                    }
                case "12":
                    {
                        if (isWeb && isAdmin)
                        {
                            accessOk = false;
                        }
                        break;
                    }
                case "13":
                    {
                        if (isAdmin)
                        {
                            accessOk = false;
                        }
                        break;
                    }
            }

            if (!accessOk)
            {
                return ShowError<object>("权限不够,不允许登录当前端");
            }

            return new OutputResult<object>(userInfo);
        }
        /// <summary>
        /// 解密用户登录密码
        /// </summary>
        /// <param name="accountPwd"></param>
        /// <returns></returns>
        public string DecryptAccountPwd(string accountPwd, out long timeStamp, bool isValidTimeStamp = false)
        {
            timeStamp = 0;
            if (_securitysModel == null || _securitysModel.RSAKeys == null)
            {
                throw new Exception("RSA加密密码对配置错误");
            }
            var plainText = RSAHelper.Decrypt(accountPwd, _securitysModel.RSAKeys.PrivateKey, Encoding.UTF8);
            var timeStampText = plainText.Substring(0, plainText.IndexOf('_'));

            if (!long.TryParse(timeStampText, out timeStamp))
            {
                throw new Exception("时间戳格式错误");
            }
            var loginTime = DateTimeUtility.ConvertToDateTime(timeStamp);
            if (isValidTimeStamp)
            {
                if (DateTime.Now - loginTime > new TimeSpan(0, 1, 0))
                {
                    throw new Exception("登录凭证已失效");
                }
            }
            var pwd = plainText.Substring(plainText.IndexOf('_') + 1);
            return pwd;
        }
        

        public OutputResult<object> RegistUser(AccountRegistModel registModel, int userNumber)
        {
            int count = _accountRepository.GetUserCount();
            OperateResult result = new OperateResult();
            if (count >= Convert.ToInt32(LicenseInstance.Instance.LimitPersonNum))
            {
                result.Msg = "注册用户人数已经超过项目许可注册人数,请联系生产商";
                return HandleResult(result);
            }

            var registEntity = _mapper.Map<AccountRegistModel, AccountUserRegistMapper>(registModel);
            if (registEntity == null || !registEntity.IsValid())
            {
                return HandleValid(registEntity);
            }
            if (registEntity.EncryptType == 1)//RSA加密方式
            {
                long timeStamp = 0;
                registEntity.AccountPwd = DecryptAccountPwd(registEntity.AccountPwd, out timeStamp);
            }
            var resulttemp = ExcuteAction((transaction, arg, userData) =>
             {
                 result = _accountRepository.RegistUser(registEntity, userNumber);
                 return HandleResult(result);
             }, registModel, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return resulttemp;
        }

        public OutputResult<object> EditUser(AccountEditModel editModel, int userNumber)
        {
            var editEntity = _mapper.Map<AccountEditModel, AccountUserEditMapper>(editModel);
            if (editEntity == null || !editEntity.IsValid())
            {
                return HandleValid(editEntity);
            }

            var resulttemp = ExcuteAction((transaction, arg, userData) =>
             {
                 var result = _accountRepository.EditUser(editEntity, userNumber);
                 return HandleResult(result);
             }, editModel, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return resulttemp;
        }

        public OutputResult<object> PwdUser(AccountPasswordModel pwdModel, int userNumber)
        {
            var pwdEntity = _mapper.Map<AccountPasswordModel, AccountUserPwdMapper>(pwdModel);
            if (pwdEntity == null || !pwdEntity.IsValid())
            {
                return HandleValid(pwdEntity);
            }
            if (pwdModel.EncryptType == 1)//RSA加密方式
            {
                long timeStamp = 0;
                pwdEntity.AccountPwd = DecryptAccountPwd(pwdModel.AccountPwd, out timeStamp);
                pwdEntity.OrginPwd = DecryptAccountPwd(pwdModel.OrginPwd, out timeStamp);
            }

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _accountRepository.PwdUser(pwdEntity, userNumber);
                return HandleResult(result);
            }, pwdModel, userNumber);

        }
        public OutputResult<object> ReConvertPwd(AccountModel model, int userNumber)
        {
            var entity = _mapper.Map<AccountModel, AccountMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            if (entity.EncryptType == 1)//RSA加密方式
            {
                long timeStamp = 0;
                entity.Pwd = DecryptAccountPwd(entity.Pwd, out timeStamp);
            }

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _accountRepository.ReConvertPwd(arg, userNumber);
                return HandleResult(result);
            }, entity, userNumber);

        }
        public OutputResult<object> ModifyPhoto(AccountModifyPhotoModel photoModel, int userNumber)
        {
            var iconEntity = _mapper.Map<AccountModifyPhotoModel, AccountUserPhotoMapper>(photoModel);
            if (iconEntity == null || !iconEntity.IsValid())
            {
                return HandleValid(iconEntity);
            }

            var result = _accountRepository.ModifyPhoto(iconEntity, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return HandleResult(result);
        }

        public OutputResult<object> GetUserList(AccountQueryModel searchModel, int userNumber)
        {
            var searchEntity = _mapper.Map<AccountQueryModel, AccountUserQueryMapper>(searchModel);
            if (searchEntity == null || !searchEntity.IsValid())
            {
                return HandleValid(searchEntity);
            }
            var pageParam = new PageParam { PageIndex = searchModel.PageIndex, PageSize = searchModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }
            var result = _accountRepository.GetUserList(pageParam, searchEntity, userNumber);
            return new OutputResult<object>(result);
        }
        public OutputResult<object> GetUserPowerList(AccountQueryModel searchModel, int userNumber)
        {
            var searchEntity = _mapper.Map<AccountQueryModel, AccountUserQueryMapper>(searchModel);
            if (searchEntity == null || !searchEntity.IsValid())
            {
                return HandleValid(searchEntity);
            }
            var pageParam = new PageParam { PageIndex = searchModel.PageIndex, PageSize = searchModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }
            var result = _accountRepository.GetUserPowerList(pageParam, searchEntity, userNumber);

            return new OutputResult<object>(result);
        }
        public OutputResult<object> GetUserPowerListForControl(AccountQueryForControlModel searchModel, int userNumber)
        {
            var searchEntity = _mapper.Map<AccountQueryForControlModel, AccountUserQueryForControlMapper>(searchModel);

            var pageParam = new PageParam { PageIndex = searchModel.PageIndex, PageSize = searchModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }
            var result = _accountRepository.GetUserPowerListForControl(pageParam, searchEntity, userNumber);
            return new OutputResult<object>(result);
        }
        public OutputResult<object> GetUserInfo(int userNumber)
        {
            var result = _accountRepository.GetUserInfo(userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> UpdateAccountStatus(AccountStatusModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AccountStatusModel, AccountStatusMapper>(entityModel);

            var res = ExcuteAction((transaction, arg, userData) =>
             {
                 var result = _accountRepository.UpdateAccountStatus(entity, userNumber);
                 return HandleResult(result);
             }, entity, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return res;
        }

        public OutputResult<object> UpdateAccountDept(AccountDepartmentModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AccountDepartmentModel, AccountDepartmentMapper>(entityModel);

            var res = ExcuteAction((transaction, arg, userData) =>
             {                    //验证通过后，插入数据
                 var result = _accountRepository.UpdateAccountDept(entity, userNumber);
                 return HandleResult(result);
             }, entityModel, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return res;
        }

        public OutputResult<object> OrderByDept(DeptOrderbyModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<DeptOrderbyModel, DeptOrderbyMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _accountRepository.OrderByDept(entity, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return HandleResult(result);
        }

        public OutputResult<object> DisabledDept(DeptDisabledModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<DeptDisabledModel, DeptDisabledMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var res = ExcuteAction((transaction, arg, userData) =>
             {                    //验证通过后，插入数据
                 var result = _accountRepository.DisabledDept(entity, userNumber);
                 return HandleResult(result);
             }, entityModel, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return res;
        }

        public OutputResult<object> SetLeader(SetLeaderModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SetLeaderModel, SetLeaderMapper>(entityModel);

            var res = ExcuteAction((transaction, arg, userData) =>
             {
                 //验证通过后，插入数据
                 var result = _accountRepository.SetLeader(entity, userNumber);
                 return HandleResult(result);
             }, entityModel, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return res;
        }


        public OutputResult<object> UpdateSoftware(UpdateSoftwareModel entityModel, int userNumber)
        {
            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _accountRepository.UpdateSoftware(transaction, entityModel.ClientType, entityModel.VersionNo, entityModel.BuildNo, userNumber);
                return new OutputResult<object>(result);
            }, entityModel, userNumber);

        }
        public OutputResult<object> UpdateSoftwareVersionForAndorid(string apkName, int userNum)
        {
            string MainVersion = "";
            string subVersion = "";
            string buildCode = "";
            string rexstr = @"[\w]+(\d+).(\d+).(\d+).apk";
            int iMainVersion = 0;
            int iSubVersion = 0;
            int iBuildCode = 0;
            Match m = Regex.Match(apkName, rexstr);
            DbTransaction tran = null;
            if (m != null && m.Groups.Count == 4)
            {
                MainVersion = m.Groups[1].Value;
                subVersion = m.Groups[2].Value;
                buildCode = m.Groups[3].Value;
                int.TryParse(MainVersion, out iMainVersion);
                int.TryParse(subVersion, out iSubVersion);
                int.TryParse(buildCode, out iBuildCode);
                IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
                FileServiceConfig UrlConfig = config.GetSection("FileServiceSetting").Get<FileServiceConfig>();
                int index = UrlConfig.ReadUrl.IndexOf("/");
                string serverUrl = UrlConfig.ReadUrl.Substring(0, index);
                _accountRepository.UpdateSoftwareVersion(tran, apkName, iMainVersion, iSubVersion, iBuildCode, serverUrl, userNum);
                return new OutputResult<object>("ok");
            }
            else
            {
                return new OutputResult<object>("文件名异常");
            }
        }

        public OutputResult<object> AuthCompany()
        {
            return new OutputResult<object>(LicenseInstance.Instance.Company);
        }
        public OutputResult<object> AuthLicenseInfo()
        {
            //添加license
            IDictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("company", LicenseInstance.Instance.Company);
            dic.Add("endtime", LicenseInstance.Instance.EndTime);
            dic.Add("totaluser", LicenseInstance.Instance.LimitPersonNum);
            var userCount = _accountRepository.GetUserCount();
            dic.Add("usercount", userCount);
            dic.Add("allowregcount", LicenseInstance.Instance.LimitPersonNum - userCount);
            return new OutputResult<object>(dic);
        }
    }
}