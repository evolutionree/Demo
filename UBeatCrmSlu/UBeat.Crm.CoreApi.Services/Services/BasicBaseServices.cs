using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.Vocation;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Services.Services
{
    /// <summary>
    /// 普通基础服务抽象类
    /// </summary>
    public abstract class BasicBaseServices : BaseServices
    {
        protected UserData HasFunctionAccess(int usernumber)
        {
            return HasFunctionAccess(usernumber, Guid.Empty);
        }
        protected UserData HasFunctionAccess(int usernumber, Guid entityid)
        {
            //获取公共缓存数据
            var commonData = GetCommonCacheData(usernumber);
            //获取个人用户数据
            UserData userData = GetUserData(usernumber);



            string _RoutePath = RoutePath;
            //判断该接口是否有职能控制，只控制有职能控制的接口，其他接口不处理功能权限判断
            if (commonData.TotalFunctions.Exists(a => a.EntityId == entityid && a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(_RoutePath)))
            {
                if (!userData.HasFunction(RoutePath, entityid, DeviceClassic))
                {
                    throw new Exception("对不起，您没有该功能的权限");
                }

            }
            return userData;
        }
        protected Dictionary<string, FunctionInfo> AllMyFunctionIds(int userNumber)
        {
            //获取公共缓存数据
            var commonData = GetCommonCacheData(userNumber);
            //获取个人用户数据
            UserData userData = GetUserData(userNumber);
            Dictionary<string, FunctionInfo> myFunctions = new Dictionary<string, FunctionInfo>();
            if (userData.Vocations != null)
            {
                foreach (VocationInfo vInfo in userData.Vocations)
                {
                    if (vInfo.Functions == null) continue;
                    foreach (FunctionInfo func in vInfo.Functions)
                    {
                        if (myFunctions.ContainsKey(func.FuncId.ToString()) == false)
                        {
                            myFunctions.Add(func.FuncId.ToString(), func);
                        }
                    }

                }
            }
            return myFunctions;
        }
        protected List<FunctionInfo> AllFunctions(int userNumber)
        {
            var commonData = GetCommonCacheData(userNumber);
            return commonData.TotalFunctions;
        }

        protected OutputResult<object> ExcuteAction<T>(Func<DbTransaction, T, UserData, OutputResult<object>> func, T arg, int usernumber, string connectString = null, IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted)
        {
            OutputResult<object> outResult = null;
            object preActionResult = null;
            object actionResult = null;


            //获取公共缓存数据
            var commonData = GetCommonCacheData(usernumber);
            //获取个人用户数据
            UserData userData = GetUserData(usernumber);

            string _RoutePath = RoutePath;
            //判断该接口是否有职能控制，只控制有职能控制的接口，其他接口不处理功能权限判断
            if (commonData.TotalFunctions.Exists(a => a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(_RoutePath)))
            {
                if (!userData.HasFunction(RoutePath, Guid.Empty, DeviceClassic))
                {
                    return ShowError<object>("对不起，您没有该功能的权限");
                }

            }


            using (var conn = GetDbConnect(connectString))
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {

                    if (PreActionExtModelList != null && PreActionExtModelList.Count > 0)
                    {
                        var actionExtModel = PreActionExtModelList.First();
                        //执行预处理逻辑,返回值必须与func返回值一致，参数为func参数的json对象
                        outResult = ActionExtService.ExcutePreAction(tran, arg, userData, actionExtModel);
                        preActionResult = outResult.DataBody;
                        //如果配置了立即终止，则立刻返回，不执行后面逻辑
                        if (actionExtModel.operatetype == 1)
                        {
                            tran.Commit();
                            return outResult;
                        }
                    }
                    outResult = func.Invoke(tran, arg, userData);
                    actionResult = outResult.DataBody;

                    if (FinishActionExtModelList != null && FinishActionExtModelList.Count > 0 && outResult.Status == 0)
                    {
                        //执行预处理逻辑，返回值必须与func返回值一致，参数为func参数的json对象和func执行结果数据的json
                        var tmp = ActionExtService.ExcuteFinishAction(tran, arg, preActionResult, actionResult, userData, FinishActionExtModelList.First());
                    }
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    //Logger.Error(ex, "数据库执行出错");
                    return ShowError<object>(ex.Message);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            outResult.Versions = userData.GetUserVersionData(commonData.DataVersions);
            return outResult;
        }
    }
}
