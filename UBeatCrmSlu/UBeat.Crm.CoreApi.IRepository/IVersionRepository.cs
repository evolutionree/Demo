using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Version;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IVersionRepository
    {
        /// <summary>
        /// 获取数据版本的信息
        /// </summary>
        /// <returns></returns>
        List<DataVersionInfo> GetDataVersions();
        /// <summary>
        /// 递增数据大版本号
        /// </summary>
        /// <param name="versionType"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        bool IncreaseDataVersion(DataVersionType versionType, List<int> usernumbers);

        /// <summary>
        /// 通过版本号获取行政区域
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetRegionsByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取团队组织
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetDepartmentsByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取年次周期
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetWeekInfoByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取指标系数(CRM统计指标)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetAnalyseFuncActiveByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取系统模版配置(周报日报模板)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetTemplateByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取用户信息配置(通讯录)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetUserInfoByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取推送消息配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetNotifyMessageByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取消息分组配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetNotifyGroupByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取数据字典配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetDictionaryByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取已删除实体列表(实体注册表配置)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetDeleteedEntityListByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取实体列表(实体注册表配置)
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityListByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取实体入口
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityEntranceByVersion(long recVersion, int userNumber, int deviceType, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取实体字段表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityFieldsByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取实体分类表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityCategoryByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取实体规则表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityFieldRulesByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取实体职能规则表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityFieldVocationRulesByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取菜单配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityMenuByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取高级搜索配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntitySearchByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取实体列表显示配置（设置手机端列表显示）
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityListViewByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取实体主页显示配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityPageByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取实体功能按钮配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityCompomentByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取实体销售阶段高级设置配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntitySalessTagetByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取实体关系页签配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetEntityRelateTabByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        List<Dictionary<string, object>> GetEntityConditionByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取流程审批配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetWorkflowListByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取职能功能表配置
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetVocationFunctionByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


        /// <summary>
        /// 通过版本号获取产品信息
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetProductsByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);

        /// <summary>
        /// 通过版本号获取产品系列
        /// </summary>
        /// <param name="recVersion"></param>
        /// <param name="userNumber"></param>
        /// <param name="maxVersion"></param>
        /// <returns></returns>
        List<Dictionary<string, object>> GetProductSeriesByVersion(long recVersion, int userNumber, out long maxVersion, out bool hasMoreData);


    }
}
