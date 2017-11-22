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

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : BaseController
    {
        private readonly AccountServices _accountServices;
        private readonly SalesTargetServices _salesTargetServices;


        public AccountController(AccountServices accountServices, SalesTargetServices salesTargetServices) : base(accountServices)
        {
            _accountServices = accountServices;
            _salesTargetServices = salesTargetServices;
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
            var handleResult = _accountServices.Login(loginModel, header);
            if (!(handleResult.DataBody is AccountUserMapper))
            {
                return handleResult;
            }

            var userInfo = (AccountUserMapper)handleResult.DataBody;
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

                SetLoginSession(MobileLoginSessionKey, token, header.DeviceId, expiration - DateTime.UtcNow, false);


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

                SetLoginSession(WebLoginSessionKey, token, deviceId, webexpiration, true);
                //CacheService.Repository.Add($"WEB_{userInfo.UserId.ToString()}", $"Bearer {token}", expiration - DateTime.UtcNow);
            }
            //result
            var result = (handleResult.DataBody as AccountUserMapper);
            var response = new
            {
                access_token = token,
                AccessType = result.AccessType,
                usernumber = userInfo.UserId,
                servertime = DateTime.Now
            };
            //清理旧缓存，且获取个人用户数据到缓存中
            _accountServices.GetUserData(userInfo.UserId, true);
            return new OutputResult<object>(response);
        }

        private void SetLoginSession(string sessionKey, string token, string deviceId, TimeSpan expiration, bool isMultipleLogin = true)
        {
            LoginSessionModel loginSession = null;
            try
            {
                loginSession = CacheService.Repository.Get<LoginSessionModel>(sessionKey);
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
                loginSession.Sessions[deviceId] = new TokenInfo(token, DateTime.UtcNow + expiration);
            }
            else loginSession.Sessions.Add(deviceId, new TokenInfo(token, DateTime.UtcNow + expiration));

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

                if (loginSession != null && loginSession.Sessions.ContainsKey(loginOutModel.DeviceId))
                {
                    loginSession.Sessions.Remove(loginOutModel.DeviceId);
                    CacheService.Repository.Replace(MobileLoginSessionKey, loginSession, loginSession.Expiration);
                }

                //$"{requestToken}_{header.DeviceId}"
                //if (CacheService.Repository.Get($"MOBILE_{UserId.ToString()}").Equals($"{requestToken}_{header.DeviceId}"))
                //{
                //    CacheService.Repository.Remove($"MOBILE_{UserId.ToString()}");
                //}
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

                    if (loginSession != null && loginSession.Sessions.ContainsKey(deviceId))
                    {
                        loginSession.Sessions.Remove(deviceId);
                        CacheService.Repository.Replace(WebLoginSessionKey, loginSession, loginSession.Expiration);
                    }

                    //if (CacheService.Repository.Get($"MOBILE_{UserId.ToString()}").Equals($"{requestToken}_{header.DeviceId}"))
                    //{
                    //    CacheService.Repository.Remove($"WEB_{UserId.ToString()}");
                    //}
                }
            }
            var response = new
            {
                value = result
            };
            return new OutputResult<object>(response);
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
            return _accountServices.PwdUser(pwdModel, UserId);
        }
        [HttpPost]
        [Route("reconvertpwd")]
        public OutputResult<object> ReConvertPwd([FromBody]AccountModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            if (string.IsNullOrEmpty(model.UserId)) return ResponseError<object>("用户Id不能为空");
            if (string.IsNullOrEmpty(model.Pwd)) return ResponseError<object>("用户重置密码不能为空");
            return _accountServices.ReConvertPwd(model, UserId);
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
            return _accountServices.GetUserInfo(UserId);
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
        public OutputResult<object> UpdateSoftwareVersionForAndroid([FromQuery] string apkname) {
            if (apkname == null || apkname.Length == 0) {
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
        [HttpPost]
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

                SetLoginSession(WebLoginSessionKey, token, deviceId, webexpiration, true);
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
    }
}
