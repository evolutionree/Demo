
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.ActionExt;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.ActionExt;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ActionExtServices 
    {
        IActionExtRepository _repository;


        CacheServices _cacheService;
        //string redisKey = "f4d81fcc-fb37-4967-abd9-6c763250d3b0";
        static bool isReadExtDataToCache = false;

        public ActionExtServices(CacheServices cacheService)
        {
            _cacheService = cacheService;
            _repository = new ActionExtRepository();
            if (isReadExtDataToCache == false)
            {
                GetActionExtData();
                isReadExtDataToCache = true;
            }
        }

        public List<ActionExtModel> GetActionExtData()
        {
            var extData = _repository.GetActionExtData();
            _cacheService.Repository.Add(CacheKeyManager.ActionExtDataKey, extData, CacheKeyManager.ActionExtDataExpires);
            return extData;
        }

        /// <summary>
        /// 检查扩展数据
        /// </summary>
        /// <param name="routePath">路由path</param>
        /// <param name="operatetype">操作类型:  0=预处理(action执行前)，1=action完成后执行</param>
        /// <returns></returns>
        public List<ActionExtModel> CheckActionExt(string routePath, int operatetype)
        {
            var resutl = new List<ActionExtModel>();
             var extData = _cacheService.Repository.Get(CacheKeyManager.ActionExtDataKey) as List<ActionExtModel>;
            if (extData == null)
            {
                extData = GetActionExtData();
            }

            if (extData != null)
            {
                resutl = extData.Where(m => routePath == m.routepath && m.operatetype == operatetype).ToList();
            }
            return resutl;
        }

        public OutputResult<object> ExcutePreAction( DbTransaction transaction, object basicParamData, UserData userData, ActionExtModel actionExtModel)
        {

            //数据库函数方式实现
            if (actionExtModel.implementtype == 0)
            {
                var result = _repository.ExcuteActionExt(transaction, actionExtModel.funcname, basicParamData,null,null, userData.UserId);
                return new OutputResult<object>(result);
            }
            else
            {
                var assemblyName = actionExtModel.assemblyname;
                var classTypeName = actionExtModel.classtypename;
                var mehtodName = actionExtModel.funcname;
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                Type type = assembly.GetType(classTypeName);//用类型的命名空间和名称获得类型
                Object obj = Activator.CreateInstance(type);//利用无参数实例初始化类型
                MethodInfo mi = type.GetMethod(mehtodName);//通过方法名称获得方法
                var result = mi.Invoke(obj, new object[] {  transaction,basicParamData, userData.UserId });//根据参数直线方法,返回值就是原方法的返回值
                return new OutputResult<object>(result);
            }


        }

        public OutputResult<object> ExcuteFinishAction( DbTransaction transaction, object basicParamData, object preActionResult, object actionResult, UserData userData, ActionExtModel actionExtModel)
        {
            //数据库函数方式实现
            if (actionExtModel.implementtype == 0)
            {
                var result = _repository.ExcuteActionExt(transaction, actionExtModel.funcname, basicParamData,preActionResult,actionResult, userData.UserId);
                return new OutputResult<object>(result);
            }
            else
            {
                var assemblyName = actionExtModel.assemblyname;
                var classTypeName = actionExtModel.classtypename;
                var mehtodName = actionExtModel.funcname;
                var assembly = Assembly.Load(new AssemblyName(assemblyName));
                Type type = assembly.GetType(classTypeName);//用类型的命名空间和名称获得类型
                Object obj = Activator.CreateInstance(type);//利用无参数实例初始化类型
                MethodInfo mi = type.GetMethod(mehtodName);//通过方法名称获得方法
                var result = mi.Invoke(obj, new object[] {  transaction, basicParamData, preActionResult, actionResult, userData.UserId });//根据参数直线方法,返回值就是原方法的返回值
                return new OutputResult<object>(result);
            }
        }


    }
}
