using System.Linq;
using NLog;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Models;
using System.Collections.Generic;
using System;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.Repository.Repository.EntityPro;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.ActionExt;
using System.Data;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using Npgsql;
using System.Data.Common;
using UBeat.Crm.CoreApi.Repository.Repository.Vocation;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.Repository.Repository.Role;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.Repository.Repository.Version;
using UBeat.Crm.CoreApi.Repository.Repository.Account;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.Role;
using UBeat.Crm.CoreApi.Repository.Repository.Dynamics;
using UBeat.Crm.CoreApi.Repository.Repository.Notify;
using UBeat.Crm.CoreApi.DomainModel.Message;
using System.Collections;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public abstract class BaseServices
    {
        protected static readonly Logger Logger = LogManager.GetLogger(typeof(BaseServices).FullName);

        /// <summary>
        /// 缓存服务对象
        /// </summary>
        public CacheServices CacheService { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public UKServiceContext UKServiceContext { get; set; }
        public AnalyseHeader header { get; set; }

        private Hashtable _PreActionExtModelList = new Hashtable();
        public List<ActionExtModel> PreActionExtModelList {
            get {
                int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                if (_PreActionExtModelList.ContainsKey(threadid)) return (List<ActionExtModel>)_PreActionExtModelList[threadid];
                throw (new Exception("网络异常Code=200001"));
            }
            set
            {
                int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                _PreActionExtModelList[threadid] = value;
            }
        }
        private Hashtable _FinishActionExtModelList = new Hashtable();
        public List<ActionExtModel> FinishActionExtModelList {
            get
            {
                int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                if (_FinishActionExtModelList.ContainsKey(threadid)) return (List<ActionExtModel>)_FinishActionExtModelList[threadid];
                throw (new Exception("网络异常Code=200001"));
            }
            set
            {
                int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                _FinishActionExtModelList[threadid] = value;
            }
        }

        private Hashtable _RoutePath = new Hashtable();
        /// <summary>
        /// 请求api的路由路径,前后去掉'/'的字符串，如：api/customer/add
        /// </summary>
        public string RoutePath {
            get
            {
                int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                if (_RoutePath.ContainsKey(threadid)) return (string)_RoutePath[threadid];
                throw (new Exception("网络异常Code=200001"));
            }
            set
            {
                int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                _RoutePath[threadid] = value;
            }
        }
        public void ClearThreadParam()
        {
            int threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            if (_RoutePath != null && _RoutePath.ContainsKey(threadid)) _RoutePath.Remove(threadid);
            if (_FinishActionExtModelList != null && _FinishActionExtModelList.ContainsKey(threadid)) _FinishActionExtModelList.Remove(threadid);
            if (_PreActionExtModelList != null && _PreActionExtModelList.ContainsKey(threadid)) _PreActionExtModelList.Remove(threadid);
        }

        /// <summary>
        /// 设备类型：0=WEB，1=IOS，2=Android
        /// </summary>
        public DeviceType DeviceType
        {
            get; set;
        }

        public DeviceClassic DeviceClassic
        {
            get
            {
                return DeviceType == DeviceType.WEB ? DeviceClassic.WEB : DeviceClassic.Phone;
            }
        }

        public ActionExtServices ActionExtService { set; get; }

        /// <summary>
        /// 消息服务
        /// </summary>
        public MessageServices MessageService
        {
            get
            {
                if (_messageService == null)
                    _messageService = new MessageServices(CacheService);
                return _messageService;
            }
        }

        private MessageServices _messageService;
        private readonly IVocationRepository _vocationRepository = new VocationRepository();
        private readonly IRoleRepository _roleRepository = new RoleRepository();
        private readonly IVersionRepository _versionRepository = new VersionRepository();
        private readonly IAccountRepository _accountRepository = new AccountRepository(ServiceLocator.Current.GetInstance<IConfigurationRoot>());



        private static string _connectString;
        public static DbConnection GetDbConnect(string connectStr = null)
        {
            if (string.IsNullOrEmpty(connectStr))
            {
                if (_connectString == null)
                {
                    IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
                    _connectString = config.GetConnectionString("DefaultDB");
                }

                connectStr = _connectString;
            }

            return new NpgsqlConnection(connectStr);
        }


        protected OutputResult<TDataBody> ShowError<TDataBody>(string errorMsg)
        {
            return new OutputResult<TDataBody>(default(TDataBody), errorMsg, 1);
        }

        protected OutputResult<object> HandleResult(OperateResult result)
        {
            if (result.Flag == 0)
            {
                Logger.Error("数据库执行出错:{0},数据库代码:{1}", result.Stacks, result.Codes);
                return new OutputResult<object>(null, result.Msg, 1);
            }

            return new OutputResult<object>(result.Id, result.Msg);
        }


        protected OutputResult<object> HandleValid(BaseEntity baseEntity)
        {
            if (baseEntity == null) return ShowError<object>("参数转换错误");
            var errorTips = baseEntity.ValidationState.Errors.First();
            return ShowError<object>(errorTips);
        }


        /// <summary>
        /// 查重
        /// </summary>
        /// <param name="_dicFields">提交表单的数据键值表</param>
        /// <param name="typeId"></param>
        /// <param name="entityId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OperateResult CheckRepeat(Dictionary<string, object> _dicFields, int userId, string typeId = null, string entityId = null)
        {
            IEntityProRepository entityProRepositor = new EntityProRepository();
            if (string.IsNullOrEmpty(typeId) && string.IsNullOrEmpty(entityId))
                throw new Exception("Id不能为空");
            if (!string.IsNullOrEmpty(typeId) && string.IsNullOrEmpty(entityId))
                entityId = entityProRepositor.GetEntityInfo(Guid.Parse(typeId), userId).entityid;

            var fields = entityProRepositor.NeedCheckFieldRepeat(entityId, userId);
            var needCheckRepeatField = _dicFields.Select(t => t.Key).Intersect(fields.Select(t => t.FieldName));
            OperateResult result = new OperateResult
            {
                Flag = 1,
            };
            string dataId = string.Empty;
            string dataValue = string.Empty;
            foreach (var field in needCheckRepeatField)
            {
                var entity = fields.SingleOrDefault(t => t.FieldName == field);
                dataId = _dicFields["recid"] == null ? string.Empty : _dicFields["recid"].ToString();
                dataValue = _dicFields[field].ToString();
                result = entityProRepositor.CheckFieldRepeat(entityId, entity.FieldId, dataId, dataValue, userId);
                if (result.Flag == 0) return result;
            }
            return result;
        }

        /// <summary>
        /// 获取当前用户的数据版本信息
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public VersionData GetVersionData(int userNumber)
        {
            //获取公共缓存数据
            var commonData = GetCommonCacheData(userNumber);
            //获取个人用户数据
            UserData userData = GetUserData(userNumber);
            return userData.GetUserVersionData(commonData.DataVersions);
        }

        /// <summary>
        /// 递增数据大版本号
        /// </summary>
        /// <param name="versionType"></param>
        /// <param name="usernumbers">null时为全部人</param>
        /// <returns></returns>
        public bool IncreaseDataVersion(DataVersionType versionType, List<int> usernumbers = null)
        {
            string cacheKey = CacheKeyManager.DataVersionKey;
            if (CacheService != null)
            {
                CacheService.Repository.Remove(cacheKey);
            }

            return _versionRepository.IncreaseDataVersion(versionType, usernumbers);
        }


        #region --获取公共数据--
        public CommonCacheData GetCommonCacheData(int usernumber, bool isInitFunction = false)
        {
            CommonCacheData data = new CommonCacheData();
            var totalFunctionsTask = GetTotalFunctions(isInitFunction);
            totalFunctionsTask.Start();


            if (DeviceType != DeviceType.WEB)
            {
                var dataVersionsTask = GetDataVersions();
                dataVersionsTask.Start();
                //dataVersionsTask.Wait();
                data.DataVersions = dataVersionsTask.Result;
            }
            data.TotalFunctions = totalFunctionsTask.Result;

            return data;
        }

        #region --获取所有功能信息-- -List<FunctionInfo> GetTotalFunctions()
        /// <summary>
        /// 获取所有功能信息
        /// </summary>
        /// <returns></returns>
        private Task<List<FunctionInfo>> GetTotalFunctions(bool isInitFunction = false)
        {
            return new Task<List<FunctionInfo>>(() =>
            {
                List<FunctionInfo> data = null;
                string cacheKey = CacheKeyManager.TotalFunctionsDataKey;
                if (isInitFunction)//职能配置保存 将覆盖缓存的全部职能
                {
                    //如果不使用缓存管理数据，直接从数据库获取
                    data = _vocationRepository.GetTotalFunctions();
                    CacheService.Repository.Add(cacheKey, data, CacheKeyManager.TotalFunctionsDataExpires);
                    return data;
                }
                if (CacheService != null)
                {
                    //如果缓存不存在，则从数据库获取数据，并保存到缓存中
                    if (!CacheService.Repository.Exists(cacheKey))
                    {
                        data = _vocationRepository.GetTotalFunctions();
                        CacheService.Repository.Add(cacheKey, data, CacheKeyManager.TotalFunctionsDataExpires);
                    }
                    else data = CacheService.Repository.Get<List<FunctionInfo>>(cacheKey);
                }
                if (data == null)
                {
                    //如果不使用缓存管理数据，直接从数据库获取
                    data = _vocationRepository.GetTotalFunctions();
                    CacheService.Repository.Add(cacheKey, data, CacheKeyManager.TotalFunctionsDataExpires);
                }

                return data;
            });
        }

 
        #endregion

        #region --获取数据版本--
        /// <summary>
        /// 获取数据版本
        /// </summary>
        /// <returns></returns>
        private Task<List<DataVersionInfo>> GetDataVersions()
        {
            return new Task<List<DataVersionInfo>>(() =>
            {
                List<DataVersionInfo> data = null;
                string cacheKey = CacheKeyManager.DataVersionKey;

                if (CacheService != null)
                {
                    //如果缓存不存在，则从数据库获取数据，并保存到缓存中
                    if (!CacheService.Repository.Exists(cacheKey))
                    {
                        data = _versionRepository.GetDataVersions();
                        CacheService.Repository.Add(cacheKey, data, CacheKeyManager.DataVersionExpires);
                    }
                    else data = CacheService.Repository.Get<List<DataVersionInfo>>(cacheKey);
                }
                if (data == null)
                {
                    //如果不使用缓存管理数据，直接从数据库获取
                    data = _versionRepository.GetDataVersions();
                    CacheService.Repository.Add(cacheKey, data, CacheKeyManager.DataVersionExpires);
                }
                return data;
            });
        }

        #endregion


        #endregion

        #region --清除公共数据缓存--
        /// <summary>
        /// 清除公共数据缓存
        /// </summary>
        public void RemoveCommonCache()
        {
            string dataVersionKey = CacheKeyManager.DataVersionKey;
            string totalFunctionsDataKey = CacheKeyManager.TotalFunctionsDataKey;
            string actionExtDataKey = CacheKeyManager.ActionExtDataKey;
            string messageConfigKey = CacheKeyManager.MessageConfigKey;
            if (CacheService != null)
            {
                CacheService.Repository.RemoveAll(new List<string> { dataVersionKey, totalFunctionsDataKey, actionExtDataKey, messageConfigKey });
            }
        }
        public void RemoveAllUserCache()
        {
            var users = _accountRepository.GetAllUserInfoList();
            var keys = new List<string>();
            foreach (var u in users)
            {
                string cacheKey = string.Format("{0}{1}", CacheKeyManager.UserData_Profix, u.UserId);
                keys.Add(cacheKey);
            }

            if (CacheService != null)
            {
                CacheService.Repository.RemoveAll(keys);
            }
        }
        #endregion



        #region --获取用户数据--
        /// <summary>
        /// 获取用户数据，先从redis缓存获取，若为空，则读取数据库
        /// </summary>
        /// <param name="entityid"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        public UserData GetUserData(int usernumber, bool isLogin = false)
        {

            UserData userData = null;

            string cacheKey = string.Format("{0}{1}", CacheKeyManager.UserData_Profix, usernumber);

            if (CacheService != null)
            {

                //如果缓存不存在，则从数据库获取数据，并保存到缓存中
                if (!CacheService.Repository.Exists(cacheKey) || isLogin)
                {
                    userData = GetUserDataFromDb(usernumber);

                    CacheService.Repository.Add(cacheKey, userData, CacheKeyManager.UserDataExpires);
                }
                else userData = CacheService.Repository.Get<UserData>(cacheKey);
            }
            if (userData == null)
            {
                //如果不使用缓存管理数据，直接从数据库获取
                userData = GetUserDataFromDb(usernumber);
            }

            return userData;
        }

        /// <summary>
        /// 从数据库获取个人用户数据
        /// </summary>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        private UserData GetUserDataFromDb(int usernumber)
        {
            UserData userData = new UserData();
            userData.UserId = usernumber;
            //多线程异步获取各个字段的数据，提高查询效率
            var accountUserInfoTask = new Task<AccountUserInfo>(() => _accountRepository.GetAccountUserInfo(usernumber));
            accountUserInfoTask.Start();

            var vocationsTask = new Task<List<VocationInfo>>(() => _vocationRepository.GetUserVocations(usernumber));
            vocationsTask.Start();

            var rolesTask = new Task<List<RoleInfo>>(() => _roleRepository.GetUserRoles(usernumber));
            rolesTask.Start();

            //获取个人信息数据
            userData.AccountUserInfo = accountUserInfoTask.Result;

            //获取用户个人职能数据
            userData.Vocations = vocationsTask.Result;

            //获取用户个人角色数据
            userData.Roles = rolesTask.Result;

            return userData;
        }

        #endregion

        #region --清除用户数据缓存--
        /// <summary>
        /// 清除用户数据缓存
        /// </summary>
        /// <param name="usernumber"></param>
        public void RemoveUserDataCache(int usernumber)
        {
            string cacheKey = string.Format("{0}{1}", CacheKeyManager.UserData_Profix, usernumber);
            if (CacheService != null)
            {
                CacheService.Repository.Remove(cacheKey);
            }
        }
        /// <summary>
        /// 清除用户数据缓存
        /// </summary>
        /// <param name="usernumber"></param>
        public void RemoveUserDataCache(List<int> usernumbers)
        {
            List<string> keys = new List<string>();
            foreach (var user in usernumbers)
            {
                string cacheKey = string.Format("{0}{1}", CacheKeyManager.UserData_Profix, user);
                keys.Add(cacheKey);
            }

            if (CacheService != null && keys.Count > 0)
            {
                CacheService.Repository.RemoveAll(keys);
            }
        }
        /// <summary>
        /// 清除用户数据缓存
        /// </summary>
        /// <param name="usernumber"></param>
        public void RemoveUserDataCache(List<string> usernumbers)
        {
            List<string> keys = new List<string>();
            foreach (var user in usernumbers)
            {
                string cacheKey = string.Format("{0}{1}", CacheKeyManager.UserData_Profix, user);
                keys.Add(cacheKey);
            }

            if (CacheService != null && keys.Count > 0)
            {
                CacheService.Repository.RemoveAll(keys);
            }
        }

		public void RemoveUserLoginCache(int usernumber)
		{
			string cacheKeyWeb = WebLoginSessionKey(usernumber);
			string cacheKeyMobile = WebLoginSessionKey(usernumber);
			if (CacheService != null)
			{
				CacheService.Repository.Remove(cacheKeyWeb);
				CacheService.Repository.Remove(cacheKeyMobile);
			}
		}

		protected string WebLoginSessionKey(int userid)
		{
			return string.Format("WebLoginSession_{0}", userid.ToString());
		}

		protected string MobileLoginSessionKey(int userid)
		{
			return string.Format("MobileLoginSession_{0}", userid.ToString());
		}
		#endregion

		#region 在Service动态创建其他Service
		protected object dynamicCreateService(string serviceName, bool isInit)
        {
            if (ServiceProvider == null) return null;
            try
            {
                BaseServices service = (BaseServices)ServiceProvider.GetService(Type.GetType(serviceName));
                service.ServiceProvider = this.ServiceProvider;
                service.header = this.header;
                service.CacheService = this.CacheService;
                service.UKServiceContext = this.UKServiceContext;
                return service;
            }
            catch (Exception ex)
            {
            }
            return null;
        }
        #endregion

        public bool IsMobile(AnalyseHeader header)
        {
            var isMobile = header.Device.ToLower().Contains("android")
                           || header.Device.ToLower().Contains("ios")
                           || header.Device.ToLower().Contains("h5");
            return isMobile;
        }
    }
}
