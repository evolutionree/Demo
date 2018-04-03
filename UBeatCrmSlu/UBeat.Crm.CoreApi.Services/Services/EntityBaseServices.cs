using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.Vocation;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Services.Services
{
    /// <summary>
    /// 实体模块的基础服务抽象类
    /// </summary>
    public abstract class EntityBaseServices : BaseServices
    {
        /// <summary>
        /// 权限功能校验通用查询接口
        /// </summary>
        /// <typeparam name="T">参数数据类型</typeparam>
        /// <param name="func">回调具体业务的函数</param>
        /// <param name="arg">参数数据</param>
        /// <param name="entityId">实体id</param>
        /// <param name="usernumber">用户id</param>
        /// <param name="connectString"></param>
        /// <returns></returns>
        protected OutputResult<object> ExcuteSelectAction<T>(Func<DbTransaction, T, UserData, OutputResult<object>> func, T arg, Guid entityId, int usernumber, string connectString = null)
        {
            return ExcuteAction(0, func, arg, entityId, usernumber, null, connectString);
        }
        /// <summary>
        /// 权限功能校验通用新增接口
        /// </summary>
        /// <typeparam name="T">参数数据类型</typeparam>
        /// <param name="func">回调具体业务的函数</param>
        /// <param name="arg">参数数据</param>
        /// <param name="entityId">实体id</param>
        /// <param name="usernumber">用户id</param>
        /// <param name="connectString"></param>
        protected OutputResult<object> ExcuteInsertAction<T>(Func<DbTransaction, T, UserData, OutputResult<object>> func, T arg, Guid entityId, int usernumber, string connectString = null)
        {
            return ExcuteAction(1, func, arg, entityId, usernumber, null, connectString);
        }

        /// <summary>
        /// 权限功能校验通用更新接口
        /// </summary>
        /// <typeparam name="T">参数数据类型</typeparam>
        /// <param name="func">回调具体业务的函数</param>
        /// <param name="arg">参数数据</param>
        /// <param name="entityId">实体id</param>
        /// <param name="usernumber">用户id</param>
        /// <param name="recIds">需要检查权限的记录id</param>
        /// <param name="connectString"></param>
        protected OutputResult<object> ExcuteUpdateAction<T>(Func<DbTransaction, T, UserData, OutputResult<object>> func, T arg, Guid entityId, int usernumber, List<Guid> recIds, string recidFieldName = "recid", string connectString = null)
        {
            return ExcuteAction(2, func, arg, entityId, usernumber, recIds, recidFieldName, connectString);
        }
        /// <summary>
        /// 权限功能校验通用删除接口
        /// </summary>
        /// <typeparam name="T">参数数据类型</typeparam>
        /// <param name="func">回调具体业务的函数</param>
        /// <param name="arg">参数数据</param>
        /// <param name="entityId">实体id</param>
        /// <param name="usernumber">用户id</param>
        /// <param name="recIds">需要检查权限的记录id</param>
        /// <param name="connectString"></param>
        protected OutputResult<object> ExcuteDeleteAction<T>(Func<DbTransaction, T, UserData, OutputResult<object>> func, T arg, Guid entityId, int usernumber, List<Guid> recIds, string recidFieldName = "recid", string connectString = null)
        {
            return ExcuteAction(3, func, arg, entityId, usernumber, recIds, recidFieldName, connectString);
        }


        /// <summary>
        /// 权限功能校验通用接口
        /// </summary>
        /// <typeparam name="T">参数数据类型</typeparam>
        /// <param name="excuteType">执行方式，0=查询，1=新增，2=update,3=delete</param>
        /// <param name="func">回调具体业务的函数</param>
        /// <param name="arg">参数数据</param>
        /// <param name="entityId">实体id</param>
        /// <param name="usernumber">用户id</param>
        /// <param name="recIds">需要检查权限的记录id，查询接口可空</param>
        /// <param name="connectString"></param>
        /// <returns></returns>
        private OutputResult<object> ExcuteAction<T>(int excuteType, Func<DbTransaction, T, UserData, OutputResult<object>> func, T arg, Guid entityId, int usernumber, List<Guid> recIds = null, string recidFieldName = "recid", string connectString = null)
        {
            OutputResult<object> outResult = null;
            object preActionResult = null;
            object actionResult = null;

            //获取公共缓存数据
            var commonData = GetCommonCacheData(usernumber);
            //获取个人用户数据
            UserData userData = GetUserData(usernumber);

            //判断该接口是否有职能控制，只控制有职能控制的接口，其他接口不处理功能权限判断
            if (commonData.TotalFunctions.Exists(a => a.EntityId == entityId && a.RoutePath != null && a.RoutePath.Trim().Trim('/').Equals(RoutePath) && a.DeviceType == (int)DeviceClassic))
            {
                if (!userData.HasFunction(RoutePath, entityId, DeviceClassic))
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
                    bool checkAccess = false;
                    //执行方式，0=查询，1=新增，2=update,3=delete
                    switch (excuteType)
                    {
                        case 2:
                        case 3:
                             checkAccess = userData.HasDataAccess(tran, RoutePath, entityId, DeviceClassic, recIds, recidFieldName);
                            if(checkAccess==false)
                            {
                                throw new Exception("对不起，您没有操作该数据的权限");
                            }
                            break;
                        default: break;
                    }
                    

                    if (PreActionExtModelList != null && PreActionExtModelList.Count > 0)
                    {
                        var actionExtModel = PreActionExtModelList.Where(m => m.entityid == entityId).FirstOrDefault();
                        if (actionExtModel != null)
                        {
                            //执行预处理逻辑,返回值必须与func返回值一致，参数为func参数的json对象
                            outResult = ActionExtService.ExcutePreAction(tran, arg, userData, actionExtModel);
                            preActionResult = outResult.DataBody;
                            //如果配置了立即终止，则立刻返回，不执行后面逻辑
                            if ( actionExtModel.resulttype == 1)
                            {
                                tran.Commit();
                                return outResult;
                            }
                        }
                    }
                    outResult = func.Invoke(tran, arg, userData);
                    actionResult = outResult.DataBody;
                    switch (excuteType)
                    {
                        case 2:
                            checkAccess = userData.HasDataAccess(tran, RoutePath, entityId, DeviceClassic, recIds, recidFieldName);
                            if (!checkAccess)
                            {
                                throw new Exception("对不起，您没有操作该数据的权限");
                            }
                            break;
                        default: break;
                    }

                    if (FinishActionExtModelList != null && FinishActionExtModelList.Count > 0)
                    Logger.Error("FinishActionExtModelList:" + Newtonsoft.Json.JsonConvert.SerializeObject(FinishActionExtModelList));
                    Logger.Error("entityid=" + entityId);
                    if (FinishActionExtModelList != null && FinishActionExtModelList.Count > 0&& outResult.Status==0)
                    {
                        var actionExtModel = FinishActionExtModelList.Where(m => m.entityid == entityId).FirstOrDefault();
                        if (actionExtModel != null)
                        {
                            //执行预处理逻辑，返回值必须与func返回值一致，参数为func参数的json对象和func执行结果数据的json
                            var tmp = ActionExtService.ExcuteFinishAction(tran, arg, preActionResult, actionResult, userData, actionExtModel);
                            //if (tmp != null&& tmp.DataBody != null ) {
                            //    outResult = tmp;
                            //}
                        }
                    }
                    
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    
                    tran.Rollback();
                    Logger.Error(ex, "数据库执行出错");
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
