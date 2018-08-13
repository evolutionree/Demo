using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.Vocation;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IVocationRepository
    {
        //=======职能

        /// <summary>
        /// 添加职能
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult AddVocation(VocationAdd data, int userNumber);

        OperateResult AddCopyVocation(CopyVocationAdd data, int userNumber);

        /// <summary>
        /// 编辑职能
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        OperateResult EditVocation(VocationEdit data, int userNumber);


        /// <summary>
        /// 删除职能
        /// </summary>
        /// <param name="eventids"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        OperateResult DeleteVocation(VocationDelete data, int userNumber);


        /// <summary>
        /// 获取职能列表
        /// </summary>
        /// <returns></returns>
        dynamic GetVocations(PageParam page, string vocationName, int userNumbe);



        //=======职能下的功能

        /// <summary>
        /// 根据职能id,获取功能列表k
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        dynamic GetFunctionsByVocationId(VocationFunctionSelect data);



        /// <summary>
        /// 编辑职能下的函数
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult EditVocationFunctions(VocationFunctionEdit data, int userNumber);



        /// <summary>
        /// 添加功能的规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult AddFunctionRule(List<FunctionRuleAdd> data, int userNumber);



        /// <summary>
        /// 修改功能的规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult EditFunctionRule(List<FunctionRuleEdit> data, int userNumber);
        /// <summary>
        /// 添加功能的规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult AddFuncRule(List<FunctionRuleAdd> data, int userNumber);



        /// <summary>
        /// 修改功能的规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult EditFuncRule(List<FunctionRuleEdit> data, int userNumber);
        /// <summary>
        /// 获取功能的规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumberk"></param>
        /// <returns></returns>
        List<FunctionRuleQueryMapper> GetFunctionRule(FunctionRuleSelect data, int userNumberk);

          dynamic  GetFunctionRule(Guid vocationId, Guid entityId, Guid funcId);
        dynamic GetDeviceTypeFunction(int deviceType, Guid entityId);
        //======职能下的用户

        /// <summary>
        /// 获取职能下的用户
        /// </summary>
        /// <param name="page"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        dynamic GetVocationUser(PageParam page, VocationUserSelect data, int userNumber);


        /// <summary>
        /// 获取职能下的用户
        /// </summary>
        /// <param name="page"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        List<VocationUserInfo> GetVocationUsers(List<Guid> vocationIds);

        /// <summary>
        /// 获取职能下的用户
        /// </summary>
        /// <param name="page"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        List<UserInfo> GetVocationUsers(Guid vocationId);



        /// <summary>
        /// 删除职能下的用户
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult DeleteVocationUser(VocationUserDelete data, int userNumber);


        /// <summary>
        /// 根据用户的职能，获取某个用户可用的功能列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>

        dynamic GetUserFunctions(UserFunctionSelect data);


        /// <summary>
        /// 添加功能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult AddFunction(FunctionAdd data, int userNumber);

        OperateResult AddFunction(FunctionInfo data, int userNumber, DbTransaction trans = null);

        bool EditFunction(FunctionInfo data, int userNumber, DbTransaction trans = null);
        bool DeleteFunction(Guid funcid, int userNumber, DbTransaction trans = null);

        /// <summary>
        /// 编辑功能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult EditFunction(FunctionItemEdit data, int userNumber);


        /// <summary>
        /// 删除功能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        OperateResult DeleteFunction(FunctionItemDelete data, int userNumber);



        /// <summary>
        /// 根据职能树
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        dynamic GetFunctionTree(FunctionTreeSelect data);


        /// <summary>
        /// 获取功能信息
        /// </summary>
        /// <param name="routePath"></param>
        /// <param name="entityid"></param>
        /// <returns></returns>
        FunctionDefineModel GetFunctionDefine(string routePath, Guid entityid);

        /// <summary>
        /// 获取用户的职能数据
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        List<VocationInfo> GetUserVocations(int userNumber);

        /// <summary>
        /// 获取所有的功能信息(不包含rule数据)
        /// </summary>
        /// <returns></returns>
        List<FunctionInfo> GetTotalFunctions();
        List<FunctionInfo> GetTotalFunctionsWithStatus0();
        bool ExplainVocationRulePower( string ruleSql, int userNo);
    }
}

