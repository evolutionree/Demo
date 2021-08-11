using AutoMapper;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.IO;
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
using System.Linq;
using Microsoft.AspNetCore.Http;
using static MessagePack.MessagePackSerializer;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class AccountServices : BasicBaseServices
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly string _passwordSalt;
        private readonly SecuritysModel _securitysModel;
        private readonly AdAuthConfigModel _adAuthConfigModel;
        private readonly bool _isOpenAdAuth;
        private readonly AdAuthServices _adAuthServices;

        public AccountServices(IMapper mapper, IAccountRepository accountRepository, IConfigurationRoot config, 
            AdAuthServices adAuthServices)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
            _securitysModel = config.GetSection("Securitys").Get<SecuritysModel>();
            _passwordSalt = _securitysModel.PwdSalt;

            _adAuthServices = adAuthServices;
            _adAuthConfigModel = config.GetSection("ADConfig").Get<AdAuthConfigModel>();
            if (_adAuthConfigModel != null)
                _isOpenAdAuth = _adAuthConfigModel.IsOpen == 1 ? true : false;
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
                new
                {
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
            else
            {
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
            AccountUserMapper userInfo = null;
            if (_isOpenAdAuth == false)
                userInfo = _accountRepository.GetUserInfo(loginModel.AccountName);
            else
                userInfo = _accountRepository.GetUserInfoByLoginName(loginModel.AccountName);
            if (userInfo == null)
            {
                return ShowError<object>("请输入正确的帐号");
            }
            if (userInfo.IsCrmUser == 2)
            {
                return ShowError<object>("非CRM用户，不能登录");
            }

            if (userInfo.RecStatus == 0)
            {
                return ShowError<object>("该账户已停用");
            }

            var needCheckAdAuth = true;
            if (_isOpenAdAuth)
            {
                var specialAccount = _adAuthConfigModel.SpecialAccountId;
                if (!string.IsNullOrEmpty(specialAccount))
                {
                    foreach (var item in specialAccount.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (!string.IsNullOrEmpty(item) && loginModel.AccountName == item)
                        {
                            needCheckAdAuth = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                needCheckAdAuth = false;
            }

            if (needCheckAdAuth == true)
            {
                var adModel = new AdAuthModelInfo();
                adModel.ServerIp = _adAuthConfigModel.ServerIp;
                adModel.ServerPort = _adAuthConfigModel.ServerPort;
                adModel.AdminAccount = _adAuthConfigModel.AdminAccount;
                adModel.AdminPwd = _adAuthConfigModel.AdminPwd;
                adModel.BinDN = _adAuthConfigModel.BinDN;
                adModel.BaseDN = _adAuthConfigModel.BaseDN;
                adModel.Account = userInfo.AccountName;
                adModel.Pwd = loginModel.AccountPwd;

                if (_adAuthServices == null) return ShowError<object>("_adAuthServices");
                var result = _adAuthServices.CheckAdAuthByAccount(adModel);
                if (!string.IsNullOrEmpty(result) && result != "AD验证成功")
                {
                    return ShowError<object>(result);
                }
            }
            else
            {
                //pwd salt security
                var securityPwd = SecurityHash.GetPwdSecurity(loginModel.AccountPwd, _passwordSalt);
                if (!securityPwd.Equals(userInfo.AccountPwd))
                {
                    return ShowError<object>("密码输入错误");
                }
            }

            //判断登录授权
            var isMobile = header.Device.ToLower().Contains("android")
                         || header.Device.ToLower().Contains("ios");
            var isWeb = header.Device.ToLower().Contains("web");

            var isAdmin = userInfo.AccessType == "10" ? true : false;

            var accessOk = true;
            if (!String.IsNullOrEmpty(loginModel.UniqueId))
            {
				if (CacheService.Repository.Get(loginModel.UniqueId) == null)
				 return ShowError<object>("验证参数错误，请稍后再试！");

				string uniqueId = CacheService.Repository.Get(loginModel.UniqueId).ToString();
				uniqueId = uniqueId.ToLower();
				string sendcode = loginModel.sendcode.ToLower();
				if (isWeb && uniqueId != sendcode)
				{
					return ShowError<object>("请输入正确的验证码！");
				}
			}
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
            int count = _accountRepository.GetLicenseUserCount();
            OperateResult result = new OperateResult();
            if (registModel.AccessType != "99")//如果是99，表示限制登录，不会考虑许可问题 
            {
                if (count >= Convert.ToInt32(LicenseInstance.Instance.LimitPersonNum))
                {
                    result.Msg = "注册用户人数已经超过项目许可注册人数,请联系生产商";
                    return HandleResult(result);
                }
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
            #region 检查密码策略，是否符合密码策略，检查是否需要下次首次登陆是否需要修改密码
            registEntity.NextMustChangePwd = 0;
            PwdPolicy pwdPolicy = GetPwdPolicy(userNumber);
            if (pwdPolicy != null && pwdPolicy.IsUserPolicy == 1)
            {
                //启用密码策略
                if (pwdPolicy.IsFirstUpdatePwd == 1)
                {
                    registEntity.NextMustChangePwd = 1;
                }
            }


            #endregion
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

                 if (editEntity.AccessType != "99")//如果是变成不可登录，则不检查
                 {
                     int count = _accountRepository.GetLicenseUserCount();//更新完后检查
                     if (count > Convert.ToInt32(LicenseInstance.Instance.LimitPersonNum))
                     {
                         throw (new Exception("注册用户人数已经超过项目许可注册人数,请联系生产商"));
                     }
                 }

                 return HandleResult(result);
             }, editModel, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
            return resulttemp;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="pwdModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> PwdUser(AccountPasswordModel pwdModel, string authorizedCode, int userNumber)
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
            string checkValid = this.CheckPasswordValidForPolicy(userNumber, pwdEntity.AccountPwd, _accountRepository.EncryPwd(pwdEntity.AccountPwd, userNumber), userNumber);
            if (checkValid != null && checkValid.Length > 0)
            {
                throw (new Exception(checkValid));//不满足密码策略
            }
            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _accountRepository.PwdUser(pwdEntity, userNumber);
                if (result != null && result.Flag == 1)
                {//只有成功才注销设备
                    this.ForceLogoutButThis(userNumber, authorizedCode, null, ForceUserLogoutTypeEnum.All, userNumber);//注销本人但不是本次登陆的
                }
                return HandleResult(result);
            }, pwdModel, userNumber);

        }
        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> ReConvertPwd(AccountModel model, string authorizedCode, int userNumber)
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
            foreach (var tmp in entity.UserId.Split(","))
            {
                string CheckPassResult = CheckPasswordValidForPolicy(int.Parse(tmp), entity.Pwd, _accountRepository.EncryPwd(entity.Pwd, userNumber), userNumber);
                if (CheckPassResult != null && CheckPassResult.Length > 0)
                {
                    throw (new Exception(CheckPassResult));//有异常就抛出
                }
            }
            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _accountRepository.ReConvertPwd(arg, userNumber);
                if (result != null && result.Flag == 1)
                {
                    foreach (var tmp in entity.UserId.Split(","))
                    {
                        this.ForceLogoutButThis(int.Parse(tmp), authorizedCode, null, ForceUserLogoutTypeEnum.All, userNumber);//重置密码，注销所有设备，但不包括本身
                    }
                }
                return HandleResult(result);
            }, entity, userNumber);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="NextUpdatePwdUserIds"></param>
        /// <param name="IsForceLogout"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> SetToNextUpdatePwd(List<int> NextUpdatePwdUserIds, bool IsForceLogout, int userNumber)
        {
            return null;
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
        public OutputResult<object> GetUserInfo(int userNumber, int CurrentUserId)
        {
            var result = _accountRepository.GetUserInfo(userNumber, CurrentUserId);
            var users = result["User"];
            if (users != null)
            {
                foreach (var t in users)
                {
                    if (t["frpwd"] == null) continue;
                    t["frpwd"] = RSAEncrypt.RSADecryptStr(t["frpwd"].ToString());
                }
            }
            return new OutputResult<object>(result);
        }

        public OutputResult<object> UpdateAccountStatus(AccountStatusModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AccountStatusModel, AccountStatusMapper>(entityModel);

            var res = ExcuteAction((transaction, arg, userData) =>
             {
                 var result = _accountRepository.UpdateAccountStatus(entity, userNumber);
                 if (entity.Status == 1)
                 {//启用状态就检查，如果是停用，可以不检查
                     int count = _accountRepository.GetLicenseUserCount();//更新完后检查
                     if (count >= Convert.ToInt32(LicenseInstance.Instance.LimitPersonNum))
                     {
                         throw (new Exception("注册用户人数已经超过项目许可注册人数,请联系生产商"));
                     }
                 }

                 return HandleResult(result);
             }, entity, userNumber);
            IncreaseDataVersion(DataVersionType.BasicData, null);
			RemoveUserLoginCache(entityModel.UserId);
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

        public OutputResult<object> SetIsCrm(SetIsCrmModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SetIsCrmModel, SetIsCrmMapper>(entityModel);
            var userInfo = _accountRepository.GetAccountUserInfo(entityModel.UserId);
            int count = _accountRepository.GetLicenseUserCount();
            OperateResult result = new OperateResult();
            if (userInfo.AccessType != "99")//如果是99，表示限制登录，不会考虑许可问题 
            {
                if (count >= Convert.ToInt32(LicenseInstance.Instance.LimitPersonNum))
                {
                    result.Msg = "注册用户人数已经超过项目许可注册人数,不能将Hr用户转为CRM用户";
                    return HandleResult(result);
                }
            }
            var res = ExcuteAction((transaction, arg, userData) =>
            {
                //验证通过后，插入数据
                var iscrm = _accountRepository.SetIsCrm(entity, userNumber);
                var op = new OperateResult
                {
                    Flag = iscrm ? 1 : 0,
                    Msg = iscrm ? "设置成功" : "设置失败"
                };
                return HandleResult(op);
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
        public OutputResult<object> AuthLicenseInfo(LicenseConfig config = null)
        {
            //添加license
            if (config == null) config = LicenseInstance.Instance;
            IDictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("company", config.Company);
            dic.Add("endtime", config.EndTime);
            dic.Add("totaluser", config.LimitPersonNum);
            var userCount = _accountRepository.GetLicenseUserCount();
            dic.Add("usercount", userCount);
            dic.Add("allowregcount", config.LimitPersonNum - userCount);
            return new OutputResult<object>(dic);
        }

        #region 安全机制
        /// <summary>
        /// 获取密码策略
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public PwdPolicy GetPwdPolicy(int userNumber)
        {
            DbTransaction tran = null;
            var policy = string.Empty;
            var data = _accountRepository.GetPwdPolicy(userNumber, tran);
            if (data == null) return new PwdPolicy();
            if (data.ContainsKey("policy"))
            {
                policy = data["policy"].ToString();
            }
            return Newtonsoft.Json.JsonConvert.DeserializeObject<PwdPolicy>(policy);
        }
        /// <summary>
        /// 保存密码策略
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> SavePwdPolicy(PwdPolicy data, int userNumber)
        {
            _accountRepository.SavePwdPolicy(data, userNumber, null);
            return new OutputResult<object>("保存成功");
        }
        /// <summary>
        /// 判断密码是否符合密码规则
        /// </summary>
        /// <param name="userid">被修改密码的用户id</param>
        /// <param name="plantextpassword">加密前的密码</param>
        /// <param name="encryptpwd">加密后的密码，主要用于判断密码的历史版本信息</param>
        /// <param name="operatorUserId">操作的用户</param>
        /// <returns>如果返回是null或者空字符串，表示验证成功，否则返回验证错误信息，如：密码长度不足等</returns>
        public string CheckPasswordValidForPolicy(int userid, string plantextpassword, string encryptpwd, int operatorUserId)
        {
            PwdPolicy pwdPolicy = GetPwdPolicy(userid);
            if (pwdPolicy.IsUserPolicy > 0)
            {
                AccountUserInfo accountInfo = this._accountRepository.GetAccountUserInfo(userid);
                string userName = accountInfo.AccountName;
                List<HistoryPwd> historyPwd = _accountRepository.GetHistoryPwd(pwdPolicy.HistoryPwdCount, userid);
                if (pwdPolicy.HistoryPwdCount <= 0) pwdPolicy.HistoryPwdCount = 2;
                var pwds = historyPwd.Select(r => r.NewPwd).ToList();
                if (pwdPolicy.IsSetPwdLength > 0)
                {
                    if (pwdPolicy.SetPwdLength > 0 && plantextpassword.Length < pwdPolicy.SetPwdLength)
                        return "密码长度不符，必须包含" + pwdPolicy.SetPwdLength + "个字符";
                    if (pwdPolicy.IsNumber > 0 && !Regex.IsMatch(plantextpassword, "[0-9]"))
                        return "密码必须包含数字";
                    if (pwdPolicy.IsUpper > 0 && !Regex.IsMatch(plantextpassword, "^(?:(?=.*[A-Z])(?=.*[a-z])).*$"))
                        return "密码必须包含大小写";
                    if (pwdPolicy.IsSpecialStr > 0 && Regex.IsMatch(plantextpassword, "^[a-zA-Z0-9]*$"))
                        return "密码必须包含特殊字符";
                }
                if (pwdPolicy.IsLikeLetter > 0)
                {
                    if (pwdPolicy.LikeLetter > 0 && Regex.IsMatch(plantextpassword, @"(.)\1{" + (pwdPolicy.LikeLetter) + "}"))
                        return "密码不得连续多于" + pwdPolicy.LikeLetter + "位相同的字母";
                }
                if (pwdPolicy.IsContainAccount > 0 && plantextpassword.Contains(userName))
                    return "密码不得包含用户名";
                if (pwds.Contains(encryptpwd))
                    return "不能用近" + pwdPolicy.HistoryPwdCount + "次使用过的密码";
            }
            return "";
        }
        /// <summary>
        /// 注销除指定的Session以外的登陆信息，主要用于修改密码后强制大部分设备退出重新登陆
        /// </summary>
        /// <param name="userId">被操作的用户id</param>
        /// <param name="thisSession">需要排除的authorizedcode</param>
        /// <param name="operatorUserId">操作者id</param>
        public int ForceLogoutButThis(int userId, string thisSession, string deviceid, ForceUserLogoutTypeEnum forcetype, int operatorUserId)
        {
            int logoutSessionCount = 0;
            if (thisSession != null && thisSession.StartsWith("Bearer "))
            {
                thisSession = thisSession.Substring("Bearer ".Length);
            }
            LoginSessionModel_ForInner loginSession = CacheService.Repository.Get<LoginSessionModel_ForInner>(MobileLoginSessionKey(userId));
            if (loginSession != null && (forcetype == ForceUserLogoutTypeEnum.All || forcetype == ForceUserLogoutTypeEnum.AllMobile || forcetype == ForceUserLogoutTypeEnum.SpecialDevice))
            {
                List<string> removeKeys = new List<string>();
                foreach (string key in loginSession.Sessions.Keys)
                {
                    TokenInfo_ForInner tokenInfo = loginSession.Sessions[key];
                    if (tokenInfo.Token.Equals(thisSession)) continue;
                    if (forcetype == ForceUserLogoutTypeEnum.All
                        || forcetype == ForceUserLogoutTypeEnum.AllMobile)
                    {
                        removeKeys.Add(key);
                        logoutSessionCount++;
                    }
                    else if (forcetype == ForceUserLogoutTypeEnum.SpecialDevice && (deviceid == null || (deviceid != null && deviceid.Equals(tokenInfo.DeviceId))))
                    {
                        removeKeys.Add(key);
                        logoutSessionCount++;
                    }
                }
                foreach (string key in removeKeys)
                {
                    loginSession.Sessions.Remove(key);
                }
                if (!(loginSession.LatestSession != null && loginSession.LatestSession.Equals(thisSession)))
                {
                    if (loginSession.Sessions.Count > 0)
                    {
                        loginSession.LatestSession = loginSession.Sessions.Last().Value.Token;
                    }
                    else
                    {
                        loginSession.LatestSession = "";
                    }
                }
                else
                {
                    loginSession.LatestSession = "";
                }
                if (loginSession.Sessions.Count == 0)
                {
                    CacheService.Repository.Remove(MobileLoginSessionKey(userId));
                }
                else
                {

                    CacheService.Repository.Replace(MobileLoginSessionKey(userId), loginSession);
                }
            }
            loginSession = CacheService.Repository.Get<LoginSessionModel_ForInner>(WebLoginSessionKey(userId));
            if (loginSession != null && (forcetype == ForceUserLogoutTypeEnum.All || forcetype == ForceUserLogoutTypeEnum.AllWeb || forcetype == ForceUserLogoutTypeEnum.SpecialDevice))
            {
                List<string> removeKeys = new List<string>();
                foreach (string key in loginSession.Sessions.Keys)
                {
                    TokenInfo_ForInner tokenInfo = loginSession.Sessions[key];
                    if (tokenInfo.Token.Equals(thisSession)) continue;
                    if (forcetype == ForceUserLogoutTypeEnum.All
                        || forcetype == ForceUserLogoutTypeEnum.AllWeb)
                    {
                        removeKeys.Add(key);
                        logoutSessionCount++;
                    }
                    else if (forcetype == ForceUserLogoutTypeEnum.SpecialDevice && (deviceid == null || (deviceid != null && deviceid.Equals(tokenInfo.DeviceId))))
                    {
                        removeKeys.Add(key);
                        logoutSessionCount++;
                    }
                }
                foreach (string key in removeKeys)
                {
                    loginSession.Sessions.Remove(key);
                }
                if (!(loginSession.LatestSession != null && loginSession.LatestSession.Equals(thisSession)))
                {
                    if (loginSession.Sessions.Count > 0)
                    {
                        loginSession.LatestSession = loginSession.Sessions.Last().Value.Token;
                    }
                    else
                    {
                        loginSession.LatestSession = "";
                    }
                }
                else
                {
                    loginSession.LatestSession = "";
                }
                if (loginSession.Sessions.Count == 0)
                {
                    CacheService.Repository.Remove(WebLoginSessionKey(userId));
                }
                else
                {

                    CacheService.Repository.Replace(WebLoginSessionKey(userId), loginSession);
                }
            }
            return logoutSessionCount;
        }

        public void SetPasswordInvalid(List<int> userList, int userId)
        {
            DbTransaction tran = null;
            this._accountRepository.SetPasswordInvalid(userList, userId, tran);

        }

        public int ForUserLogout(List<ForceUserLogoutParamInfo> paramList, string thisSession, int userId)
        {
            int totalCount = 0;
            foreach (ForceUserLogoutParamInfo item in paramList)
            {
                switch (item.ForceType)
                {
                    case ForceUserLogoutTypeEnum.All:
                    case ForceUserLogoutTypeEnum.AllMobile:
                    case ForceUserLogoutTypeEnum.AllWeb:
                        totalCount = totalCount + ForceLogoutButThis(item.UserId, thisSession, null, item.ForceType, userId);
                        break;
                    case ForceUserLogoutTypeEnum.SpecialDevice:
                        totalCount = totalCount + ForceLogoutButThis(item.UserId, thisSession, item.DeviceId, item.ForceType, userId);
                        break;
                }
            }
            return totalCount;
        }
        public bool CheckAuthorizedCodeValid(int UserId, string authorizedCode)
        {
            if (authorizedCode == null) authorizedCode = "";
            if (authorizedCode.StartsWith("Bearer "))
                authorizedCode = authorizedCode.Substring("Bearer ".Length);
            LoginSessionModel_ForInner loginSession = CacheService.Repository.Get<LoginSessionModel_ForInner>(WebLoginSessionKey(UserId));
            if (loginSession != null && loginSession.Sessions.Count > 0)
            {
                if (loginSession.Sessions.ContainsKey(authorizedCode))
                {
                    TokenInfo_ForInner sessionInfo = loginSession.Sessions[authorizedCode];
                    if (sessionInfo.Expiration > System.DateTime.Now)
                        return true;
                }
            }
            return false;
        }
		
        #endregion
        
        public string MakeCode(int codeLen)
        {
            if (codeLen < 1)
            {
                return string.Empty;
            }
            int number;
            StringBuilder sbCheckCode = new StringBuilder();
            Random random = new Random();

            for (int index = 0; index < codeLen; index++)
            {
                number = random.Next();

                if (number % 2 == 0)
                {
                    sbCheckCode.Append((char)('0' + (char)(number % 10))); //生成数字
                }
                else
                {
                    sbCheckCode.Append((char)('A' + (char)(number % 26))); //生成字母
                }
            }
            return sbCheckCode.ToString();
        }
        
          ///<summary>
         /// 获取验证码图片流
         /// </summary>
        /// <param name="checkCode">验证码字符串</param>
        /// <returns>返回验证码图片流</returns>
        public MemoryStream CreateCodeImg(string checkCode)
        {
            if (string.IsNullOrEmpty(checkCode))
            {
                return null;
            }
            Bitmap image = new Bitmap((int)Math.Ceiling((checkCode.Length * 27.0)), 46);
            Graphics graphic = Graphics.FromImage(image);
            try
            {
                Random random = new Random();
                graphic.Clear(Color.White);
                int x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                for (int index = 0; index < 25; index++)
                {
                    x1 = random.Next(image.Width);
                    x2 = random.Next(image.Width);
                    y1 = random.Next(image.Height);
                    y2 = random.Next(image.Height);

                    graphic.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 27, (FontStyle.Bold | FontStyle.Italic));
                System.Drawing.Drawing2D.LinearGradientBrush brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height), Color.Red, Color.DarkRed, 1.2f, true);
                graphic.DrawString(checkCode, font, brush, 2, 2);

                int x = 0;
                int y = 0;

                //画图片的前景噪音点
                for (int i = 0; i < 100; i++)
                {
                    x = random.Next(image.Width);
                    y = random.Next(image.Height);

                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //画图片的边框线
                graphic.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                //将图片验证码保存为流Stream返回
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms;
            }
            finally
            {
                graphic.Dispose();
                image.Dispose();
            }
        }
    }
    public class LoginSessionModel_ForInner
    {
        public Dictionary<string, TokenInfo_ForInner> Sessions { set; get; } = new Dictionary<string, TokenInfo_ForInner>();

        public TimeSpan Expiration { set; get; }

        /// <summary>
        /// 是否多设备同时登陆
        /// </summary>
        public bool IsMultipleLogin { set; get; }


        /// <summary>
        /// 最新登陆session
        /// </summary>
        public string LatestSession { set; get; }




    }
    public class TokenInfo_ForInner
    {
        public string Token { set; get; }
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string SysMark { get; set; }

        public DateTime Expiration { set; get; }
        public DateTime LastRequestTime { get; set; }
        /// <summary>
        /// 记录本次登录的时间戳
        /// </summary>
        public long RequestTimeStamp { set; get; }

        public TokenInfo_ForInner(string token, DateTime expiration, long requestTimeStamp)
        {
            Token = token;
            Expiration = expiration;
            RequestTimeStamp = requestTimeStamp;
        }
    }
    public class ForceUserLogoutParamInfo
    {
        public int UserId { get; set; }
        public ForceUserLogoutTypeEnum ForceType { get; set; }
        public string DeviceId { get; set; }
    }
    public enum ForceUserLogoutTypeEnum
    {
        All = 0,
        AllWeb = 1,
        AllMobile = 2,
        SpecialDevice = 3
    }

}