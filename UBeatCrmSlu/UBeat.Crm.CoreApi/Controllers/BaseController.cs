using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.Services.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using UBeat.Crm.CoreApi.Services.Services;
using System.Text;
using System.IO;
using NLog;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.LicenseCore;
using MessagePack;
using MessagePack.Resolvers;
using System.Net;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Models;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Authorize(ActiveAuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class BaseController : Controller
    {
        private static readonly Logger Logger = LogManager.GetLogger("SysOperateLog");
        private Logger _logger;
        private Guid _traceId;
        private RoutePathSettingModel routePathSetting;


        private int _userId;
        private UserInfo loginUser;
        //action 扩展服务类
        ActionExtServices _actionExtServices;
        private BaseServices[] _baseServices;


        protected string WebLoginSessionKey
        {
            get
            {
                return string.Format("WebLoginSession_{0}", LoginUser.UserId.ToString());
            }
        }

        protected string MobileLoginSessionKey
        {
            get
            {
                return string.Format("MobileLoginSession_{0}", LoginUser.UserId.ToString());
            }
        }


        #region ---property---
        /// <summary>
        /// 缓存服务对象
        /// </summary>
        public CacheServices CacheService { get; private set; }



        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        protected int UserId
        {
            get
            {
                if (_userId == 0)
                {
                    int userNum;
                    string userInfo = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
                    int.TryParse(userInfo, out userNum);
                    _userId = userNum;
                }
                return _userId;
            }

        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        protected UserInfo LoginUser
        {
            get
            {
                if (loginUser == null)
                {
                    loginUser = new UserInfo();
                    int userNum;
                    string userInfo = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
                    int.TryParse(userInfo, out userNum);
                    loginUser.UserId = userNum;
                    loginUser.UserName = User.Claims.FirstOrDefault(c => c.Type == "username")?.Value;
                }
                return loginUser;
            }
        }

        #endregion

        public BaseController(params BaseServices[]  baseServices)
        {
            _baseServices = baseServices;
            CacheService = new CacheServices();
            _actionExtServices = new ActionExtServices(CacheService);
            foreach(var service in _baseServices)
            {
                service.CacheService = CacheService;
                if (ControllerContext.HttpContext != null)
                {
                    ((BaseServices)service).ServiceProvider = ControllerContext.HttpContext.RequestServices;
                }
                //service.header = this.GetAnalyseHeader();
            }
            
            var Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            routePathSetting = new RoutePathSettingModel();
            routePathSetting.NotNeedVersionDataPath = Config.GetSection("NotNeedVersionDataPath").Get<List<string>>();
            routePathSetting.UnCheckAuthorization = Config.GetSection("UnCheckAuthorization").Get<List<string>>();
        }

        //public BaseController(BaseServices baseServices)
        //{
        //    _baseServices = baseServices;
        //    CacheService = new CacheServices();
        //    _actionExtServices = new ActionExtServices(CacheService);
        //    _baseServices.CacheService = CacheService;
        //    var Config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
        //    routePathSetting = new RoutePathSettingModel();
        //    routePathSetting.NotNeedVersionDataPath = Config.GetSection("NotNeedVersionDataPath").Get<List<string>>();
        //    routePathSetting.UnCheckAuthorization = Config.GetSection("UnCheckAuthorization").Get<List<string>>();
        //}


        #region ---获取请求头信息对象 +GetAnalyseHeader()---
        //需要获取请求头信息
        protected AnalyseHeader GetAnalyseHeader()
        {
            string device = Request.Headers["Device"];
            if (string.IsNullOrWhiteSpace(device))
            {
                device = "UnKnown";
                //throw new Exception("Headers缺少Device参数");
            }

            string deviceId = Request.Headers["DeviceId"];
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = "UnKnown";
            }

            string vernum = Request.Headers["Vernum"];
            if (string.IsNullOrWhiteSpace(vernum))
            {
                vernum = "UnKnown";
            }

            string sysmark = Request.Headers["Sysmark"];
            if (string.IsNullOrWhiteSpace(vernum))
            {
                sysmark = "UnKnown";
            }

            var analyseHeader = new AnalyseHeader
            {
                Device = device,
                DeviceId = deviceId,
                VerNum = vernum,
                SysMark = sysmark
            };


            return analyseHeader;
        }
        #endregion

        /// <summary>
        /// 记录操作日志
        /// </summary>
        /// <param name="operateMark">操作描述</param>
        /// <param name="paramData"></param>
        protected void WriteOperateLog(string operateMark, object paramData, int userid = -1)
        {
            //获取请求头
            var header = GetAnalyseHeader();
            //生成描述日志(操作人)
            LogEventInfo theEvent = new LogEventInfo(LogLevel.Info, "SysOperateLog", operateMark);
            theEvent.Properties["device"] = header.Device;
            theEvent.Properties["sysmark"] = header.SysMark;
            theEvent.Properties["vernum"] = header.VerNum;
            theEvent.Properties["logdata"] = JsonConvert.SerializeObject(paramData);
            theEvent.Properties["reccreator"] = userid == -1 ? UserId.ToString() : userid.ToString();
            theEvent.Properties["reccreated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            theEvent.Properties["url"] = Request.Path.ToString();
            Logger.Log(theEvent);
        }

        protected OutputResult<TDataBody> ResponseError<TDataBody>(string errorMsg)
        {
            return new OutputResult<TDataBody>(default(TDataBody), errorMsg, 1);
        }

        protected IActionResult ResponseError(string errorMsg)
        {
            return Json(new OutputResult<string>(null, errorMsg, 1));
        }




        public override void OnActionExecuting(ActionExecutingContext context)
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start(); // 开始监视代码


            //打印日志
            WriteRequestLog(context);


            stopwatch.Stop(); // 停止监视
            double seconds = stopwatch.Elapsed.TotalSeconds; // 秒数
            System.Console.WriteLine("WriteRequestLog:" + seconds);
            stopwatch.Reset();
            stopwatch.Restart();



            //检查系统是否超出许可日期
            bool isPass = ValidLimitTime(context);



            stopwatch.Stop(); // 停止监视
            seconds = stopwatch.Elapsed.TotalSeconds; // 秒数
            System.Console.WriteLine("ValidLimitTime:" + seconds);
            stopwatch.Reset();
            stopwatch.Restart();


            if (isPass)
            {
                //验证授权
                CheckAuthorization(context);
            }


            stopwatch.Stop(); // 停止监视
            seconds = stopwatch.Elapsed.TotalSeconds; // 秒数
            System.Console.WriteLine("CheckAuthorization:" + seconds);
            stopwatch.Reset();
            stopwatch.Restart();



            if (_baseServices != null)
            {
                foreach (var service in _baseServices)
                {
                    // Action执行完成前，执行特殊业务逻辑
                    string routePath = context.HttpContext.Request.Path.ToString().Trim().Trim('/');
                    service.PreActionExtModelList = _actionExtServices.CheckActionExt(routePath, 0);
                    service.FinishActionExtModelList = _actionExtServices.CheckActionExt(routePath, 1);
                    service.ActionExtService = _actionExtServices;
                    service.RoutePath = routePath;
                    var header = GetAnalyseHeader();
                    service.header = header;
                    if (service.ServiceProvider == null) {
                        service.ServiceProvider = context.HttpContext.RequestServices;
                    }
                    var device = header.Device.ToUpper();
                    switch (device)
                    {
                        case "WEB":
                            service.DeviceType = DeviceType.WEB;
                            break;
                        case "IOS":
                            service.DeviceType = DeviceType.IOS;
                            break;
                        case "ANDROID":
                            service.DeviceType = DeviceType.Android;
                            break;
                    }
                }

            }
            

            stopwatch.Stop(); // 停止监视
            seconds = stopwatch.Elapsed.TotalSeconds; // 秒数
            System.Console.WriteLine("ActionExtService:" + seconds);
            stopwatch.Reset();
            stopwatch.Restart();

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {

            var header = GetAnalyseHeader();
            var device = header.Device.ToUpper();
            if (device != "WEB")
            {
                //只有手机端需要返回version数据
                if (context.Result is ObjectResult && _baseServices != null && _baseServices.Length > 0 && LoginUser != null && LoginUser.UserId > 0)
                {
                    var outputResult = (context.Result as ObjectResult).Value as OutputResult<object>;
                    if (outputResult != null && outputResult.Versions == null)
                    {
                        string apiUrl = string.Format("/{0}/", context.HttpContext.Request.Path.Value.ToLower().Trim().Trim('/'));

                        if (!(routePathSetting != null && routePathSetting.NotNeedVersionDataPath.Exists(m => apiUrl.Contains(string.Format("/{0}/", m.Trim().Trim('/').ToLower())))))
                        {
                            outputResult.Versions = _baseServices.FirstOrDefault().GetVersionData(LoginUser.UserId);
                        }
                    }
                }
            }
            //打印响应数据
            //WriteResponseLog(context);

            base.OnActionExecuted(context);
        }

        protected override void Dispose(bool disposing)
        {
            if (CacheService != null && CacheService.Repository != null)
            {
                CacheService.Repository.Dispose();
            }
            base.Dispose(disposing);
        }





        #region ---检查请求授权认证 -void CheckAuthorization(ActionExecutingContext context)---
        /// <summary>
        /// 检查请求授权认证
        /// </summary>
        /// <param name="context"></param>
        private void CheckAuthorization(ActionExecutingContext context)
        {
            string requestAuthorization = context.HttpContext.Request.Headers["Authorization"];
            string apiUrl = context.HttpContext.Request.Path.Value.ToLower().Trim().Trim('/');
            if (context.HttpContext.Request.Method == "POST"
                && !string.IsNullOrEmpty(requestAuthorization)
                && !(routePathSetting != null && routePathSetting.UnCheckAuthorization.Exists(m => apiUrl.Contains(m.Trim().Trim('/').ToLower()))))
            {
                if (CacheService == null)
                    CacheService = new CacheServices();
                var header = GetAnalyseHeader();

                if (header.Device == null)
                {
                    throw new Exception("Headers缺少Device参数");
                }
                var requestToken = requestAuthorization.Replace("Bearer", "").Trim();
                var isMobile = header.Device.ToLower().Contains("android") || header.Device.ToLower().Contains("ios");
                var deviceId = header.DeviceId;
                LoginSessionModel loginSession = null;
                string sessionKey = string.Empty;
                if (isMobile)
                {
                    sessionKey = MobileLoginSessionKey;
                    loginSession = CacheService.Repository.Get<LoginSessionModel>(MobileLoginSessionKey);
                    //手机端必须传DeviceId
                    if (header.DeviceId.Equals("UnKnown"))
                    {
                        throw new Exception("Headers缺少DeviceId参数");
                    }
                }
                else
                {
                    sessionKey = WebLoginSessionKey;
                    loginSession = CacheService.Repository.Get<LoginSessionModel>(WebLoginSessionKey);
                    if (header.DeviceId.Equals("UnKnown"))
                    {
                        //如果web没有传deviceid字段，则取token作为设备id
                        deviceId = requestToken;
                    }
                }

                if (loginSession == null)
                {
                    throw new UnauthorizedAccessException("session已过期，请重新登录");
                }

                //如果是多设备同时登录，则判断是否该设备以及登录过了
                if (loginSession.IsMultipleLogin && loginSession.Sessions.ContainsKey(deviceId) && loginSession.Sessions[deviceId].Expiration > DateTime.UtcNow)
                {
                    if (DateTime.UtcNow + loginSession.Expiration - loginSession.Sessions[deviceId].Expiration > new TimeSpan(0, 5, 0))
                    {
                        //时间超过一分钟才更新缓存，否则不更新缓存
                        loginSession.Sessions[deviceId].Expiration = DateTime.UtcNow + loginSession.Expiration;
                        CacheService.Repository.Replace(sessionKey, loginSession, loginSession.Expiration);
                    }

                    return;
                }
                else if (loginSession.LatestSession.Equals(requestToken) && loginSession.Sessions.ContainsKey(deviceId) && loginSession.Sessions[deviceId].Expiration > DateTime.UtcNow)
                {
                    if (DateTime.UtcNow + loginSession.Expiration - loginSession.Sessions[deviceId].Expiration > new TimeSpan(0, 5, 0))
                    {
                        //时间超过一分钟才更新缓存，否则不更新缓存
                        loginSession.Sessions[deviceId].Expiration = DateTime.UtcNow + loginSession.Expiration;
                        CacheService.Repository.Replace(sessionKey, loginSession, loginSession.Expiration);
                    }
                    return;
                }
                else
                {
                    //您的帐号已经在其他设备登录或session已过期
                    context.Result = Json(new OutputResult<string>(null, "您的帐号已经在其他设备登录或session已过期", -25013));
                }

            }
        }
        #endregion

        #region 检查系统是否超出许可日期
        private bool ValidLimitTime(ActionExecutingContext context)
        {
            string apiUrl = context.HttpContext.Request.Path.Value;
            if (apiUrl != "/api/account/login" && apiUrl != "/api/account/loginout"
               && apiUrl != "/api/basicdata/syncview" && apiUrl != "/api/basicdata/syncbasic"
               && apiUrl != "/api/basicdata/syncentity" && apiUrl != "/api/basicdata/synctemplate"
               && apiUrl != "/api/account/userinfo" && apiUrl != "/api/basicdata/usercontact"
               && apiUrl != "/api/chat/grouplist" && apiUrl != "/api/vocation/getuserfunction"
               && apiUrl != "/api/fileservice"
               && context.HttpContext.Request.Method == "POST")
            {
                InitLicenseConfig();
                if (LicenseInstance.Instance == null)
                    throw new Exception("初始化许可信息失败");
                var dateTime = LicenseValidHelper.OnLineTime();
                if (Convert.ToDateTime(LicenseInstance.Instance.EndTime) < dateTime || Convert.ToDateTime(LicenseInstance.Instance.BeginTime) > dateTime)
                {
                    var header = GetAnalyseHeader();

                    if (header.Device == null)
                    {
                        throw new Exception("Headers缺少Device参数");
                    }

                    var isMobile = header.Device.ToLower().Contains("android") || header.Device.ToLower().Contains("ios");
                    if (isMobile)
                    {
                        //您的帐号已经在其他设备登录或session已过期
                        context.Result = Json(new OutputResult<string>(null, "系统不在使用许可日期内,请联系生产商", -25012));
                        return false;
                    }
                    else
                    {
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        context.Result = Json(new OutputResult<string>(null, "系统不在使用许可日期内,请联系生产商", -25012));
                        return false;
                    }
                }
                return true;
            }
            return true;
        }

        private static void InitLicenseConfig()
        {
            if (LicenseInstance.Instance != null) return;
            var encryptData = CommonHelper.GetEncryptDataFileContent();
            string jsonLicense = RSAEncrypt.RSADecryptStr(encryptData);
            var bytes = MessagePackSerializer.FromJson(jsonLicense);
            LicenseInstance.Instance = MessagePackSerializer.Deserialize<LicenseConfig>(bytes, ContractlessStandardResolver.Instance);
        }
        #endregion

        #region ---打印请求内容的日志 -void WriteRequestLog(ActionExecutingContext context)---
        /// <summary>
        /// 打印请求内容的日志
        /// </summary>
        /// <param name="context"></param>
        private void WriteRequestLog(ActionExecutingContext context)
        {
            if (_logger == null)
            {
                _logger = LogManager.GetLogger(this.GetType().FullName);
            }
            if (_logger.IsTraceEnabled == false)
                return;
            _traceId = Guid.NewGuid();

            StringBuilder logText = new StringBuilder();
            logText.Append(string.Format("Request Headers:  \n"));
            foreach (var h in Request.Headers)
            {
                logText.Append(string.Format("  {0}:{1}\n", h.Key, h.Value));
            }

            //logText.Append(string.Format("Request ContentType:  {0}\n请求参数：\n", Request.ContentType));

            logText.Append(string.Format("请求参数：\n", Request.ContentType));
            //如果Action的参数已经接收了body数据，或者form数据,则直接从Action的参数中获取请求数据
            if (context.ActionArguments.Count > 0)
            {
                foreach (var m in context.ActionArguments)
                {
                    logText.Append(string.Format("  {0}:{1}\n", m.Key, JsonConvert.SerializeObject(m.Value)));
                }
            }
            //如果请求数据没有在Action的参数接收，若是form表单数据,直接从Form取数据，不打印body中文件流的数据
            else if (Request.HasFormContentType)
            {
                Request.ReadFormAsync().Wait();
                foreach (var m in Request.Form)
                {
                    logText.Append(string.Format("  {0}:{1}\n", m.Key, m.Value));
                }
            }
            //如果请求数据没有在Action的参数接收，且body的数据没有被读取，则从body读取数据
            else if (Request.Body.CanRead)
            {
                using (var bodyReader = new StreamReader(Request.Body))
                {
                    string body = bodyReader.ReadToEndAsync().Result;
                    Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
                    logText.Append(string.Format("  body:{0}", body));
                }
            }

            _logger.Trace(string.Format("Request TraceId[{0}] \n{1}", _traceId, logText.ToString()));
        }

        private string GetRequestDataJson(ActionExecutingContext context)
        {
            StringBuilder dataText = new StringBuilder();
            //如果Action的参数已经接收了body数据，或者form数据,则直接从Action的参数中获取请求数据
            if (context.ActionArguments.Count > 0)
            {
                return JsonConvert.SerializeObject(context.ActionArguments);
            }
            //如果请求数据没有在Action的参数接收，若是form表单数据,直接从Form取数据，不打印body中文件流的数据
            else if (Request.HasFormContentType)
            {
                Request.ReadFormAsync().Wait();
                return JsonConvert.SerializeObject(Request.Form);
            }
            //如果请求数据没有在Action的参数接收，且body的数据没有被读取，则从body读取数据
            else if (Request.Body.CanRead)
            {
                using (var bodyReader = new StreamReader(Request.Body))
                {
                    string body = bodyReader.ReadToEndAsync().Result;
                    Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
                    return body;
                }
            }
            return null;
        }

        #endregion

        #region ---打印响应数据的日志 -void WriteResponseLog(ActionExecutedContext context)---
        /// <summary>
        /// 打印响应数据的日志
        /// </summary>
        /// <param name="context"></param>
        private void WriteResponseLog(ActionExecutedContext context)
        {
            if (_logger != null && _logger.IsTraceEnabled)
            {
                string jsonResult = null;
                if (context.Result is ObjectResult)
                {
                    jsonResult = JsonConvert.SerializeObject((context.Result as ObjectResult).Value);
                }
                else if (context.Result is JsonResult)
                {
                    jsonResult = JsonConvert.SerializeObject((context.Result as JsonResult).Value);
                }
                if (jsonResult != null)
                {
                    _logger.Trace(string.Format("Response TraceId[{0}] \n响应数据：\n{1}", _traceId, jsonResult));
                }
            }
        }
        #endregion

        #region 未处理的方法的返回值
        protected OutputResult<object> unimplementMethod()
        {
            return new OutputResult<object>(null, "功能尚未实现", -1);
        } 
        #endregion
    }
}