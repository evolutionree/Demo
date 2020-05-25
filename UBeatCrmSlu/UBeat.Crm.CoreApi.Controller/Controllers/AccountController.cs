using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Utility;
using UBeat.Crm.CoreApi.Services.Models.SalesTarget;
using UBeat.Crm.CoreApi.Models;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UBeat.Crm.LicenseCore;
using MessagePack;
using MessagePack.Resolvers;
using UBeat.Crm.CoreApi.Services.Utility;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : BaseController
    {
        private readonly AccountServices _accountServices;
        private readonly SalesTargetServices _salesTargetServices;
        private readonly SoapServices _soapServices;

        public AccountController(AccountServices accountServices, SalesTargetServices salesTargetServices, SoapServices soapServices) : base(accountServices)
        {
            _accountServices = accountServices;
            _salesTargetServices = salesTargetServices;
            _soapServices = soapServices;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("t1")]
        public OutputResult<object> AuthErp()
        {
            var result = _soapServices.AuthErp(UserId);
            return new OutputResult<object>(result);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("t2")]
        public OutputResult<object> ToErpCustomer()
        {
            var result = _soapServices.ToErpCustomer(null, "saveCustomerFromCrm", "新增客户", 1);
            return new OutputResult<object>(result);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("t3")]
        public OutputResult<object> FromErpProducts()
        {
            var result = _soapServices.FromErpProduct(null, "getSalesPartList", "同步产品", 1);
            return new OutputResult<object>(result);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("t5")]
        public OutputResult<object> a()
        {
            var result = _soapServices.FromErpPackingShip(null, "getPackingSlipList", "同步产品单", 1);
            return new OutputResult<object>(result);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("t6")]
        public OutputResult<object> b()
        {
            var result = _soapServices.FromErpOrder(null, "getContractList", "同步产品单", 1);
            return new OutputResult<object>(result);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("t7")]
        public OutputResult<object> c()
        {
            var result = _soapServices.FromErpPackingShipCost(null, "getPackingSlipCost", "同步产品单", 1);
            return new OutputResult<object>(result);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("t8")]
        public OutputResult<object> d()
        {
            var result = _soapServices.FromErpMakeCollectionsOrder(null, "getReceivableList", "同步产品单", 1);
            return new OutputResult<object>(result);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("t9")]
        public OutputResult<object> e()
        {
            Stream stream = Request.Body;
            Byte[] byteData = new Byte[stream.Length];
            stream.Read(byteData, 0, (Int32)stream.Length);
            string jsonData = Encoding.UTF8.GetString(byteData)+Request.Query["sojumpparm"];
            SoapHttpHelper.Log(new List<string> { "finallyresult" }, new List<string> { "erp产品同步到CRM成功" + jsonData }, 0, 1);
            //  var result = _soapServices.FromErpMakeCollectionsOrder(null, "getReceivableList", "同步产品单", 1);
            return null;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("redisstatus")]
        public OutputResult<object> GetRedisStatus()
        {
            string status = CacheService.RedisServerStatus();
            return new OutputResult<object>(status);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("getpublickey")]
        public OutputResult<object> GetPublicKey()
        {
            return _accountServices.GetPublicKey();
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public OutputResult<object> Login([FromBody] AccountLoginModel loginModel = null)
        {

            if (loginModel == null) return ResponseError<object>("参数格式错误");


            var header = GetAnalyseHeader();
            var isMobile = header.Device.ToLower().Contains("android") || header.Device.ToLower().Contains("ios");
            if (isMobile && header.DeviceId.Equals("UnKnown"))
            {
                throw new Exception("Headers缺少DeviceId参数");
            }
            long requestTimeStamp = 0;
            if (loginModel.EncryptType == 1) //RSA加密算法
            {
                loginModel.AccountPwd = _accountServices.DecryptAccountPwd(loginModel.AccountPwd, out requestTimeStamp, true);
            }

            var handleResult = _accountServices.Login(loginModel, header);
            if (!(handleResult.DataBody is AccountUserMapper))
            {
                return handleResult;
            }

            var userInfo = (AccountUserMapper)handleResult.DataBody;
            //登录成功才写入操作日志
            WriteOperateLog("登录系统", loginModel, userInfo.UserId);

            //设备绑定
            if (isMobile)
            {
                //var hadBinded = _accountServices.checkHadBinded(loginModel, userInfo.UserId);
                //if (!(hadBinded.DataBody is bool))
                //{
                //    return hadBinded;
                //}
                ///移动端未调通，暂时注释
            }

            //login finished
            DateTime expiration;
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("uid", userInfo.UserId.ToString()));
            claims.Add(new Claim("username", userInfo.UserName));
            var token = JwtAuth.SignToken(claims, out expiration);


            LoginUser.UserId = userInfo.UserId;
            //校验是否该请求时间戳是否合法
            if (loginModel.EncryptType == 1 && !ValidateRequestTimeStamp(requestTimeStamp))
            {
                throw new Exception("登录凭证已失效");
            }

            //update user's Token in redis 
            if (isMobile)
            {

                SetLoginSession(MobileLoginSessionKey, token, header.DeviceId, expiration - DateTime.UtcNow, requestTimeStamp, header.SysMark, header.Device, false);

                //Cache.Remove(userInfo.UserId.ToString());
                //CacheService.Repository.Add($"MOBILE_{userInfo.UserId.ToString()}", $"Bearer {token}_{header.DeviceId}", expiration - DateTime.UtcNow);
                //var userToken = Cache.Get(userInfo.UserId.ToString());
            }
            else
            {
                var deviceId = header.DeviceId;
                if (header.DeviceId.Equals("UnKnown"))
                {
                    //如果web没有传deviceid字段，则取token作为设备id
                    deviceId = token;
                }
                var Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("WebSession");
                TimeSpan webexpiration = new TimeSpan(0, 20, 0);
                if (Config != null)
                {
                    var seconds = Config.GetValue<int>("Expiration");
                    webexpiration = new TimeSpan(0, 0, seconds);
                }

                SetLoginSession(WebLoginSessionKey, token, deviceId, webexpiration, requestTimeStamp, header.SysMark, header.Device, true);
                //CacheService.Repository.Add($"WEB_{userInfo.UserId.ToString()}", $"Bearer {token}", expiration - DateTime.UtcNow);
            }
            #region 检查是否符合账号安全限制(包含两部分1、账号是否被设定了下次登陆必须修改密码，2、密码是否已经过期或者临近过期)
            int policy_reuslt = 0;
            string policy_msg = "";
            if (userInfo.NextMustChangepwd == 1)
            {
                policy_reuslt = 2;
                policy_msg = "您必须修改密码后才能正常使用系统";
            }
            else
            {
                try
                {
                    PwdPolicy policy = this._accountServices.GetPwdPolicy(userInfo.UserId);
                    if (policy != null && policy.IsUserPolicy == 1)
                    {
                        //密码策略存在且已经启用了
                        if (policy.IsPwdExpiry == 1 && userInfo.LastChangedPwdTime != null)
                        {
                            if ((System.DateTime.Now - userInfo.LastChangedPwdTime).TotalDays >= policy.PwdExpiry)
                            {
                                //已经过期
                                policy_reuslt = 2;
                                policy_msg = "您的密码已经过期， 请修改密码后再使用系统";
                            }
                            else
                            {
                                if (policy.IsCueUserDate == 1 && userInfo.LastChangedPwdTime != null)
                                {
                                    int totalDay = (int)(System.DateTime.Now - userInfo.LastChangedPwdTime).TotalDays;
                                    if (totalDay >= policy.CueUserDate)
                                    {

                                        policy_reuslt = 1;
                                        policy_msg = "您的密码即将过期， 请尽快修改个人密码";
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }

            #endregion
            //result
            var result = (handleResult.DataBody as AccountUserMapper);
            var response = new
            {
                access_token = token,
                AccessType = result.AccessType,
                usernumber = userInfo.UserId,
                servertime = DateTime.Now,
                security = new
                {
                    policy_reuslt = policy_reuslt,
                    policy_msg = policy_msg
                }
            };
            //清理旧缓存，且获取个人用户数据到缓存中
            _accountServices.GetUserData(userInfo.UserId, true);
            return new OutputResult<object>(response);
        }

        /// <summary>
        /// 解除设备绑定
        /// </summary>
        /// <param name="registModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UnBind")]
        public OutputResult<object> UnBind([FromBody] UnBindQuery unBindQuery)
        {
            var unBind = _accountServices.UnDeviceBind(unBindQuery.RecIds, UserId);
            bool result = (bool)unBind.DataBody;
            var response = new
            {
                value = result
            };
            return new OutputResult<object>(response);
        }

        /// <summary>
        /// 设备绑定信息列表
        /// </summary>
        /// <param name="bindInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeviceBindList")]
        public OutputResult<object> DeviceBindList([FromBody] DeviceBindInfo bindInfo)
        {
            if (bindInfo == null) return ResponseError<object>("参数格式错误");

            return _accountServices.DeviceBindList(bindInfo, UserId);
        }

        /// <summary>
        /// 验证请求时间戳是否合法
        /// </summary>
        /// <param name="requestTimeStamp"></param>
        /// <returns></returns>
        private bool ValidateRequestTimeStamp(long requestTimeStamp)
        {
            try
            {
                var mobileLoginSession = CacheService.Repository.Get<LoginSessionModel>(MobileLoginSessionKey);
                var webLoginSession = CacheService.Repository.Get<LoginSessionModel>(WebLoginSessionKey);
                if (mobileLoginSession != null && mobileLoginSession.Sessions != null)
                {
                    foreach (var m in mobileLoginSession.Sessions)
                    {
                        if (m.Value.RequestTimeStamp == requestTimeStamp)
                            return false;
                    }
                }
                if (webLoginSession != null && webLoginSession.Sessions != null)
                {
                    foreach (var m in webLoginSession.Sessions)
                    {
                        if (m.Value.RequestTimeStamp == requestTimeStamp)
                            return false;
                    }
                }
            }
            catch
            {
            }
            return true;
        }

        private void SetLoginSession(string sessionKey, string token, string deviceId, TimeSpan expiration, long requestTimeStamp,
                            string SysMark, string DeviceType,
                            bool isMultipleLogin = true)
        {
            LoginSessionModel loginSession = null;
            try
            {
                loginSession = CacheService.Repository.Get<LoginSessionModel>(sessionKey);
                if (loginSession != null) ClearExpiredSession(loginSession); //清除已经过期的session

            }
            catch { }
            bool isExist = loginSession == null;
            if (loginSession == null)
            {

                loginSession = new LoginSessionModel()
                {
                    IsMultipleLogin = isMultipleLogin,
                    Sessions = new Dictionary<string, TokenInfo>(),
                    Expiration = expiration

                };
            }
            loginSession.LatestSession = token;

            if (loginSession.Sessions.ContainsKey(deviceId))
            {
                loginSession.Sessions[deviceId] = new TokenInfo(token, DateTime.UtcNow + expiration, requestTimeStamp, deviceId, DeviceType, SysMark);
            }
            else loginSession.Sessions.Add(deviceId, new TokenInfo(token, DateTime.UtcNow + expiration, requestTimeStamp, deviceId, DeviceType, SysMark));

            if (isExist)
                CacheService.Repository.Replace(sessionKey, loginSession, expiration);
            else
                CacheService.Repository.Add(sessionKey, loginSession, expiration);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("loginout")]
        public OutputResult<object> LoginOut([FromBody] AccountLoginOutModel loginOutModel = null)
        {
            if (loginOutModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("登出系统", loginOutModel);

            var header = GetAnalyseHeader();
            //delete user's Token in redis 
            bool result = true;
            var isMobile = header.Device.ToLower().Contains("android") || header.Device.ToLower().Contains("ios");

            if (isMobile)
            {
                var loginSession = CacheService.Repository.Get<LoginSessionModel>(MobileLoginSessionKey);
                if (loginSession != null) ClearExpiredSession(loginSession);//清除已经过期的session
                if (loginSession != null && loginSession.Sessions.ContainsKey(loginOutModel.DeviceId))
                {
                    loginSession.Sessions.Remove(loginOutModel.DeviceId);
                    CacheService.Repository.Replace(MobileLoginSessionKey, loginSession, loginSession.Expiration);
                }
            }
            else
            {
                //string requestToken = HttpContext.Request.Headers["Authorization"];
                if (loginOutModel.Token != null)
                {
                    var requestToken = loginOutModel.Token.Trim();
                    var deviceId = header.DeviceId;
                    if (header.DeviceId.Equals("UnKnown"))
                    {
                        //如果web没有传deviceid字段，则取token作为设备id
                        deviceId = requestToken;
                    }
                    var loginSession = CacheService.Repository.Get<LoginSessionModel>(WebLoginSessionKey);
                    if (loginSession != null) ClearExpiredSession(loginSession);//清除已经过期的session
                    if (loginSession != null && loginSession.Sessions.ContainsKey(deviceId))
                    {
                        loginSession.Sessions.Remove(deviceId);
                        CacheService.Repository.Replace(WebLoginSessionKey, loginSession, loginSession.Expiration);
                    }
                }
            }
            var response = new
            {
                value = result
            };
            return new OutputResult<object>(response);
        }
        /// <summary>
        /// 清除已经过期的数据
        /// </summary>
        /// <param name="sessions"></param>
        private void ClearExpiredSession(LoginSessionModel sessions)
        {
            List<string> ExpiredSession = new List<string>();
            foreach (string key in sessions.Sessions.Keys)
            {
                if (sessions.Sessions[key].Expiration < System.DateTime.Now) ExpiredSession.Add(key);
            }
            foreach (string key in ExpiredSession)
            {
                sessions.Sessions.Remove(key);
            }
        }

        [HttpPost]
        [Route("regist")]
        public OutputResult<object> Regist([FromBody] AccountRegistModel registModel = null)
        {
            if (registModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("注册用户资料", registModel);
            return _accountServices.RegistUser(registModel, UserId);
        }

        [HttpPost]
        [Route("edit")]
        public OutputResult<object> Edit([FromBody] AccountEditModel editModel = null)
        {
            if (editModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改用户资料", editModel);
            return _accountServices.EditUser(editModel, UserId);
        }

        [HttpPost]
        [Route("pwd")]
        public OutputResult<object> Password([FromBody] AccountPasswordModel pwdModel = null)
        {
            if (pwdModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改用户密码", pwdModel);
            string requestAuthorization = HttpContext.Request.Headers["Authorization"];
            return _accountServices.PwdUser(pwdModel, requestAuthorization, UserId);
        }
        [HttpPost]
        [Route("reconvertpwd")]
        public OutputResult<object> ReConvertPwd([FromBody]AccountModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            if (string.IsNullOrEmpty(model.UserId)) return ResponseError<object>("用户Id不能为空");
            if (string.IsNullOrEmpty(model.Pwd)) return ResponseError<object>("用户重置密码不能为空");
            string requestAuthorization = HttpContext.Request.Headers["Authorization"];
            return _accountServices.ReConvertPwd(model, requestAuthorization, UserId);
        }
        [HttpPost]
        [Route("userlist")]
        public OutputResult<object> UserList([FromBody] AccountQueryModel queryModel = null)
        {
            if (queryModel == null) return ResponseError<object>("参数格式错误");
            return _accountServices.GetUserList(queryModel, UserId);
        }
        [HttpPost]
        [Route("userpowerlist")]
        public OutputResult<object> UserPowerList([FromBody] AccountQueryModel queryModel = null)
        {
            if (queryModel == null) return ResponseError<object>("参数格式错误");
            return _accountServices.GetUserPowerList(queryModel, UserId);
        }
        [HttpPost]
        [Route("userpowerlst")]
        public OutputResult<object> GetUserPowerListForControl([FromBody] AccountQueryForControlModel queryModel = null)
        {
            if (queryModel == null) return ResponseError<object>("参数格式错误");
            return _accountServices.GetUserPowerListForControl(queryModel, UserId);
        }
        [HttpPost]
        [Route("userinfo")]
        public OutputResult<object> UserInfo()
        {
            return _accountServices.GetUserInfo(UserId, UserId);
        }
        [HttpPost]
        [Route("getuserinfo")]
        public OutputResult<object> GetUserInfo([FromBody]UserInfoModel queryModel = null)
        {
            if (queryModel == null) return ResponseError<object>("参数格式错误");
            return _accountServices.GetUserInfo(queryModel.UserId, UserId);
        }

        [HttpPost]
        [Route("modifyphoto")]
        public OutputResult<object> ModifyPhoto([FromBody] AccountModifyPhotoModel photoModel = null)
        {
            if (photoModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("修改用户头像", photoModel);
            return _accountServices.ModifyPhoto(photoModel, UserId);
        }
        [HttpPost]
        [Route("updateaccstatus")]
        public OutputResult<object> UpdateAccountStatus([FromBody] AccountStatusModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _accountServices.UpdateAccountStatus(entityModel, UserId);
        }
        [HttpPost]
        [Route("updateaccdept")]
        public OutputResult<object> UpdateAccountDept([FromBody] AccountDepartmentModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            var result = _accountServices.UpdateAccountDept(entityModel, UserId);
            if (result.Status == 0)
            {
                SalesTargetSetBeginMothModel model = new SalesTargetSetBeginMothModel
                {
                    BeginDate = entityModel.EffectiveDate,
                    DepartmentId = Guid.Parse(entityModel.DeptId),
                    UserId = entityModel.UserId
                };
                result = _salesTargetServices.SetBeginMoth(model, UserId);
            }
            return result;
        }

        [HttpPost]
        [Route("orderbydept")]
        public OutputResult<object> OrderByDept([FromBody] DeptOrderbyModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _accountServices.OrderByDept(entityModel, UserId);
        }

        [HttpPost]
        [Route("disableddept")]
        public OutputResult<object> DisabledDept([FromBody] DeptDisabledModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _accountServices.DisabledDept(entityModel, UserId);
        }

        [HttpPost]
        [Route("setleader")]
        public OutputResult<object> SetLeader([FromBody] SetLeaderModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _accountServices.SetLeader(entityModel, UserId);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("downloadapp")]
        public OutputResult<object> DownloadApp()
        {
            IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            var downloadappurl = config.GetSection("DownloadAppUrl").Value;

            return new OutputResult<object>(downloadappurl);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("updatesoftware")]
        public OutputResult<object> UpdateSoftware([FromBody] UpdateSoftwareModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _accountServices.UpdateSoftware(entityModel, UserId);
        }
        /// <summary>
        /// 用于自动构建程序更新版本信息（jenkins),仅限于Android
        /// </summary>
        /// <param name="apkname"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("updateversion/android")]
        public OutputResult<object> UpdateSoftwareVersionForAndroid([FromQuery] string apkname)
        {
            if (apkname == null || apkname.Length == 0)
            {
                return ResponseError<object>("安卓包名异常");
            }
            return _accountServices.UpdateSoftwareVersionForAndorid(apkname, UserId);
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("authcompany")]
        public OutputResult<object> AuthCompany()
        {
            return _accountServices.AuthCompany();
        }

        [HttpPost]
        [Route("authlicenseinfo")]
        public OutputResult<object> AuthLicenseInfo()
        {
            return _accountServices.AuthLicenseInfo();
        }
        [HttpPost("updatelicense")]
        [UKWebApi("更新许可文件", Description = "更新许可文件的接口")]
        public OutputResult<object> UploadLicenseFile([FromBody] LinceseImportParamInfo paramInfo = null)
        {
            if (paramInfo == null) return new OutputResult<object>("参数异常");
            try
            {
                StreamReader sr = new StreamReader(paramInfo.Data.OpenReadStream());
                string encryptData = sr.ReadLine();
                sr.Close();
                string jsonLicense = RSAEncrypt.RSADecryptStr(encryptData);
                var bytes = MessagePackSerializer.FromJson(jsonLicense);
                LicenseConfig tmpConfig = MessagePackSerializer.Deserialize<LicenseConfig>(bytes, ContractlessStandardResolver.Instance);
                if (tmpConfig == null) throw (new Exception("解析文件异常"));
                if (paramInfo.IsImport == 1)
                {
                    //需要保存，则需要先把原来的备份

                    string tmpFileName = System.DateTime.Now.Ticks.ToString();
                    string path = Directory.GetCurrentDirectory();
                    if (string.IsNullOrEmpty(path))
                    {
                        throw new Exception("应用主程序目录不存在");
                    }
                    path = path + "//encryptdata.dat";
                    FileInfo fs = new FileInfo(path);
                    if (fs.Exists)
                    {
                        fs.MoveTo(path + "." + tmpFileName);
                    }
                    FileStream fout = new FileStream(path, FileMode.OpenOrCreate);
                    StreamWriter wr = new StreamWriter(fout);
                    wr.Write(encryptData);
                    wr.Close();
                    fout.Close();
                    LicenseInstance.Instance = tmpConfig;

                }
                return _accountServices.AuthLicenseInfo(tmpConfig);
            }
            catch (Exception ex)
            {
                return ResponseError<object>(ex.Message);
            }

        }
        [HttpPost("importlicense")]
        public OutputResult<object> ImportLicenseFile()
        {
            return null;
        }
        [Route("requireToken")]
        public OutputResult<object> requireAuthToken([FromBody] AuthTokenRequireModel requireModel)
        {
            if (requireModel == null
                || requireModel.AppId == null || requireModel.AppId.Length == 0
                || requireModel.SecurityCode == null || requireModel.SecurityCode.Length == 0
                || requireModel.RandomCode == null || requireModel.RandomCode.Length == 0)
            {
                return ResponseError<object>("参数异常");
            }
            if (UserId <= 0)
            {
                return ResponseError<object>("尚未登录");
            }
            string accessToken = Guid.NewGuid().ToString();
            requireModel.AccessToken = accessToken;
            requireModel.LoginUserInfo = LoginUser;
            TimeSpan expired = new TimeSpan(0, 3, 0);
            CacheService.Repository.Add("authtoken:" + accessToken, requireModel, expired);
            return new OutputResult<object>(accessToken);
        }
        [HttpPost]
        [Route("checkToken")]
        public OutputResult<object> checkToken([FromBody] CheckTokenModel checkTokenModel)
        {
            if (checkTokenModel == null
                || checkTokenModel.AccessToken == null || checkTokenModel.AccessToken.Length == 0
                || checkTokenModel.Md5 == null || checkTokenModel.Md5.Length == 0)
            {
                return ResponseError<object>("参数异常");
            }
            string key = "authtoken:" + checkTokenModel.AccessToken;
            if (CacheService.Repository.Exists(key))
            {
                AuthTokenRequireModel requestModel = CacheService.Repository.Get<AuthTokenRequireModel>(key);
                CacheService.Repository.Remove(key);
                string md5 = GetMD5HashToUpper(requestModel.RandomCode + requestModel.AccessToken);
                if (md5 != checkTokenModel.Md5.ToUpper())
                {
                    return ResponseError<object>("认证失败");
                }
                //获取用户信息
                return new OutputResult<object>(requestModel.LoginUserInfo);

            }
            return ResponseError<object>("认证失败");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("authlogin")]
        public OutputResult<object> AuthLogin([FromBody] CheckTokenModel loginModel = null)
        {
            if (loginModel == null) return ResponseError<object>("参数格式错误");


            var header = GetAnalyseHeader();
            var isMobile = header.Device.ToLower().Contains("android") || header.Device.ToLower().Contains("ios");
            if (isMobile && header.DeviceId.Equals("UnKnown"))
            {
                throw new Exception("Headers缺少DeviceId参数");
            }
            string key = "authtoken:" + loginModel.AccessToken;
            if (!CacheService.Repository.Exists(key))
            {
                return ResponseError<object>("认证失败");
            }
            AuthTokenRequireModel requestModel = CacheService.Repository.Get<AuthTokenRequireModel>(key);
            CacheService.Repository.Remove(key);
            string md5 = GetMD5HashToUpper(requestModel.RandomCode + requestModel.AccessToken);
            if (md5 != loginModel.Md5.ToUpper())
            {
                return ResponseError<object>("认证失败");
            }
            //获取用户信息
            AccountUserMapper userInfo = new AccountUserMapper();
            userInfo.UserId = requestModel.LoginUserInfo.UserId;
            userInfo.UserName = requestModel.LoginUserInfo.UserName;
            if (!(userInfo is AccountUserMapper))
            {
                return ResponseError<object>("认证失败");
            }

            //登录成功才写入操作日志
            WriteOperateLog("登录系统", loginModel, userInfo.UserId);

            //login finished
            DateTime expiration;
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim("uid", userInfo.UserId.ToString()));
            claims.Add(new Claim("username", userInfo.UserName));
            var token = JwtAuth.SignToken(claims, out expiration);


            LoginUser.UserId = userInfo.UserId;
            //update user's Token in redis 
            if (isMobile)
            {
                //这种情况是不允许mobile登录，只提供给web登录
            }
            else
            {
                var deviceId = header.DeviceId;
                if (header.DeviceId.Equals("UnKnown"))
                {
                    //如果web没有传deviceid字段，则取token作为设备id
                    deviceId = token;
                }
                var Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("WebSession");
                TimeSpan webexpiration = new TimeSpan(0, 20, 0);
                if (Config != null)
                {
                    var seconds = Config.GetValue<int>("Expiration");
                    webexpiration = new TimeSpan(0, 0, seconds);
                }

                SetLoginSession(WebLoginSessionKey, token, deviceId, webexpiration, 0, header.SysMark, header.Device, true);
                //CacheService.Repository.Add($"WEB_{userInfo.UserId.ToString()}", $"Bearer {token}", expiration - DateTime.UtcNow);
            }
            //result
            var response = new
            {
                access_token = token,
                usernumber = userInfo.UserId,
                servertime = DateTime.Now
            };
            //清理旧缓存，且获取个人用户数据到缓存中
            _accountServices.GetUserData(userInfo.UserId, true);
            return new OutputResult<object>(response);
        }

        private string GetMD5HashToUpper(String inputValue)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(inputValue));
                var strResult = BitConverter.ToString(result);
                return strResult.Replace("-", "").ToUpper();
            }
        }

        #region 安全机制
        /// <summary>
        /// 获取密码策略
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getpwdpolicy")]
        public OutputResult<PwdPolicy> GetPwdPolicy()
        {
            PwdPolicy pwdPolicy = _accountServices.GetPwdPolicy(UserId);
            return new OutputResult<PwdPolicy>(pwdPolicy);
        }

        /// <summary>
        /// 保存密码策略
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("savepwdpolicy")]
        public OutputResult<object> SavePwdPolicy([FromBody] PwdPolicy data)
        {
            if (data == null) return ResponseError<object>("参数有误");
            if (data.IsSetPwdLength > 0 && data.SetPwdLength < 3) return ResponseError<object>("密码长度不得低于3");
            if (data.IsPwdExpiry > 0 && data.CueUserDate > (data.PwdExpiry / 2)) return ResponseError<object>("提前多少个月必须小于有效期的一半");
            return _accountServices.SavePwdPolicy(data, UserId);
        }
        [HttpPost("forcelogout")]
        public OutputResult<object> ForUseLogout([FromBody] List<ForceUserLogoutParamInfo> paramList)
        {
            if (paramList == null || paramList.Count == 0)
            {
                return ResponseError<object>("参数异常或者未提供参数");
            }
            string requestAuthorization = HttpContext.Request.Headers["Authorization"];
            int totalCount = this._accountServices.ForUserLogout(paramList, requestAuthorization, UserId);
            return new OutputResult<object>("共注销" + totalCount.ToString() + "个登录");
        }
        [HttpPost("passwordvalid")]
        public OutputResult<object> PasswordValid([FromBody] List<int> UserList)
        {
            if (UserList == null || UserList.Count == 0)
            {
                return ResponseError<object>("参数异常或者未提供参数");
            }
            this._accountServices.SetPasswordInvalid(UserList, UserId);
            return new OutputResult<object>("完成");
        }
        #endregion
        #region PaaS平台权限判断
        [HttpPost("haspaaspermission")]
        public OutputResult<object> HasPaasPermission()
        {
            return new OutputResult<object>(true);
        }
        #endregion
    }

}
