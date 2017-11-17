using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.MetaData;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class MetaDataServices : BasicBaseServices
    {
        private readonly IVersionRepository _repository;

        public MetaDataServices(IVersionRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 清理非登录信息的缓存数据
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> RemoveCaches(int userNumber)
        {
            RemoveUserDataCache(userNumber);
            RemoveCommonCache();
            RemoveAllUserCache();
            return new OutputResult<object>("OK");
        }

        //获取元数据的增量数据
        public OutputResult<object> GetIncrementData(List<IncrementDataModel> bodyData, int userNumber)
        {
            var result = new IncrementDataRespone();
            bool hasMoreData = false;
            foreach (var m in bodyData)
            {
                List<Dictionary<string, object>> dataList = null;
                long maxVersion = m.RecVersion;
                switch (m.VersionType)
                {
                    case DataVersionType.BasicData:
                        dataList = GetBasicData(m.VersionKey, m.RecVersion, userNumber, out maxVersion, out hasMoreData);
                        break;
                    case DataVersionType.MsgData:
                        dataList = GetMessageData(m.VersionKey, m.RecVersion, userNumber, out maxVersion, out hasMoreData);
                        break;
                    case DataVersionType.DicData:
                        dataList = GetDicData(m.VersionKey, m.RecVersion, userNumber, out maxVersion, out hasMoreData);
                        break;
                    case DataVersionType.EntityData:
                        dataList = GetEntityData(m.VersionKey, m.RecVersion, userNumber, out maxVersion, out hasMoreData);
                        break;
                    case DataVersionType.FlowData:
                        dataList = GetFlowData(m.VersionKey, m.RecVersion, userNumber, out maxVersion, out hasMoreData);
                        break;
                    case DataVersionType.PowerData:
                        dataList = GetPowerData(m.VersionKey, m.RecVersion, userNumber, out maxVersion, out hasMoreData);
                        break;
                    case DataVersionType.ProductData:
                        dataList = GetProductData(m.VersionKey, m.RecVersion, userNumber, out maxVersion, out hasMoreData);
                        break;
                }
                if (hasMoreData)
                    result.HasMoreData.Add(m.VersionKey);
                result.Datas.Add(m.VersionKey, dataList);
                result.Versions.Add(new IncrementDataModel()
                {
                    RecVersion = maxVersion,
                    VersionKey = m.VersionKey,
                    VersionType = m.VersionType,
                    VersionKeyName = GetVersionKeyName(m.VersionKey)
                });
            }

            return new OutputResult<object>(result);
        }

        private string GetVersionKeyName(string versionKey)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("regionsync", "行政区域");
            dic.Add("deptsync", "团队组织");
            dic.Add("yearweeksync", "年次周期");
            dic.Add("funcactivesync", "指标系数(CRM统计指标)");
            dic.Add("templatesync", "系统模版配置(周报日报模板)");
            dic.Add("usersync", "用户信息配置(通讯录)");
            dic.Add("msgnotifysync", "推送消息配置");
            dic.Add("msggroupsync", "消息分组配置");
            dic.Add("datadicsync", "数据字典配置");
            dic.Add("deleteentitysync", "删除实体列表");
            dic.Add("entitysync", "实体列表(实体注册表配置)");
            dic.Add("entityentrysync", "实体入口");
            dic.Add("entityfieldsync", "实体字段表配置");
            dic.Add("entitycategorysync", "实体分类表配置");
            dic.Add("entityfieldrulesync", "实体规则表配置");
            dic.Add("entityfieldrulevocationsync", "实体职能规则表配置");
            dic.Add("entitymenusync", "菜单配置");
            dic.Add("entitysearchsync", "高级搜索配置");
            dic.Add("moblistviewconfsync", "实体列表显示配置（设置手机端列表显示）");
            dic.Add("entitypageconfigsync", "实体主页显示配置");
            dic.Add("entitycompomentsync", "实体功能按钮配置(例如：转移、线索转客户等)");
            dic.Add("entitystagesettingsync", "实体销售阶段高级设置配置");
            dic.Add("entityrelatesync", "实体关系页签配置（实体关联动态实体配置）");
            dic.Add("entityconditionsync", "实体查重配置");
            dic.Add("workflowsync", "流程审批配置");
            dic.Add("vocationfunctionsync", "个人职能数据(职能功能表配置)");
            dic.Add("productsync", "产品信息");
            dic.Add("productserialsync", "产品系列");

            if (dic.ContainsKey(versionKey))
                return dic[versionKey];
            return null;

        }

        private List<Dictionary<string, object>> GetBasicData(string versionKey, long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            List<Dictionary<string, object>> resutl = null;
            maxVersion = 0;
            hasMoreData = false;
            switch (versionKey)
            {
                case "regionsync"://行政区域
                    resutl = _repository.GetRegionsByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "deptsync"://团队组织
                    resutl = _repository.GetDepartmentsByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "yearweeksync"://年次周期
                    resutl = _repository.GetWeekInfoByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "funcactivesync"://指标系数(CRM统计指标)
                    resutl = _repository.GetAnalyseFuncActiveByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "templatesync"://系统模版配置(周报日报模板)
                    resutl = _repository.GetTemplateByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "usersync"://用户信息配置(通讯录)
                    resutl = _repository.GetUserInfoByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
            }

            return resutl;
        }

        private List<Dictionary<string, object>> GetMessageData(string versionKey, long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            List<Dictionary<string, object>> resutl = null;
            maxVersion = 0;
            hasMoreData = false;
            switch (versionKey)
            {
                case "msgnotifysync"://推送消息配置
                    resutl = _repository.GetNotifyMessageByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "msggroupsync"://消息分组配置
                    resutl = _repository.GetNotifyGroupByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
            }
            return resutl;
        }

        private List<Dictionary<string, object>> GetDicData(string versionKey, long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            List<Dictionary<string, object>> resutl = null;
            maxVersion = 0;
            hasMoreData = false;
            switch (versionKey)
            {
                case "datadicsync"://数据字典配置
                    resutl = _repository.GetDictionaryByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
            }

            return resutl;
        }

        private List<Dictionary<string, object>> GetEntityData(string versionKey, long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            List<Dictionary<string, object>> resutl = null;
            maxVersion = 0;
            hasMoreData = false;
            switch (versionKey)
            {
                case "deleteentitysync"://删除实体列表
                    resutl = _repository.GetDeleteedEntityListByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entitysync"://实体列表(实体注册表配置)
                    resutl = _repository.GetEntityListByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entityentrysync"://实体入口
                    resutl = _repository.GetEntityEntranceByVersion(recVersion, userNumber, (int)DeviceClassic, out maxVersion, out hasMoreData);
                    break;
                case "entityfieldsync"://实体字段表配置
                    resutl = _repository.GetEntityFieldsByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entitycategorysync"://实体分类表配置
                    resutl = _repository.GetEntityCategoryByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entityfieldrulesync"://实体规则表配置
                    resutl = _repository.GetEntityFieldRulesByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entityfieldrulevocationsync"://实体职能规则表配置
                    resutl = _repository.GetEntityFieldVocationRulesByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entitymenusync"://菜单配置
                    resutl = _repository.GetEntityMenuByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entitysearchsync"://高级搜索配置
                    resutl = _repository.GetEntitySearchByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "moblistviewconfsync"://实体列表显示配置（设置手机端列表显示）
                    resutl = _repository.GetEntityListViewByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entitypageconfigsync"://实体主页显示配置
                    resutl = _repository.GetEntityPageByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entitycompomentsync"://实体功能按钮配置(例如：转移、线索转客户等)
                    resutl = _repository.GetEntityCompomentByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entitystagesettingsync"://实体销售阶段高级设置配置
                    resutl = _repository.GetEntitySalessTagetByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "entityrelatesync":  //实体关系页签配置（实体关联动态实体配置）
                    resutl = _repository.GetEntityRelateTabByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                //crm_sys_entity_condition
                case "entityconditionsync":  //实体查重条件
                    resutl = _repository.GetEntityConditionByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
            }

            return resutl;
        }

        private List<Dictionary<string, object>> GetFlowData(string versionKey, long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            List<Dictionary<string, object>> resutl = null;
            maxVersion = 0;
            hasMoreData = false;
            switch (versionKey)
            {
                case "workflowsync"://流程审批配置
                    resutl = _repository.GetWorkflowListByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;

            }
            return resutl;
        }

        //该接口只提供手机端服务
        private List<Dictionary<string, object>> GetPowerData(string versionKey, long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            List<Dictionary<string, object>> resutl = new List<Dictionary<string, object>>();
            maxVersion = 0;
            hasMoreData = false;
            var userData = GetUserData(userNumber);
            switch (versionKey)
            {
                case "vocationfunctionsync"://个人职能数据(职能功能表配置)

                    if (userData != null && userData.Vocations != null)
                    {
                        foreach (var m in userData.Vocations)
                        {
                            foreach (var f in m.Functions)
                            {
                                //只提供手机端服务
                                if (f.DeviceType == 1 && !resutl.Exists(a => (Guid)a["funcid"] == f.FuncId && (Guid)a["entityid"] == f.EntityId))
                                {
                                    var func = new Dictionary<string, object>();
                                    func.Add("funcid", f.FuncId);
                                    func.Add("funcname", f.FuncName);
                                    func.Add("funccode", f.Funccode);
                                    func.Add("parentid", f.ParentId);
                                    func.Add("entityid", f.EntityId);
                                    func.Add("relationvalue", f.RelationValue);
                                    func.Add("rectype", f.RecType);
                                    func.Add("rectypename", f.RecTypeName);

                                    resutl.Add(func);
                                }

                            }
                        }
                    }
                    break;
                case "vocationsync"://个人职能

                    if (userData != null && userData.Vocations != null)
                    {
                        foreach (var m in userData.Vocations)
                        {
                            var func = new Dictionary<string, object>();
                            func.Add("vocationid", m.VocationId);
                            func.Add("vocationname", m.VocationName);

                            resutl.Add(func);
                        }
                    }
                    break;
            }

            return resutl;
        }


        private List<Dictionary<string, object>> GetProductData(string versionKey, long recVersion, int userNumber, out long maxVersion, out bool hasMoreData)
        {
            List<Dictionary<string, object>> resutl = null;
            maxVersion = 0;
            hasMoreData = false;
            switch (versionKey)
            {
                case "productsync"://产品信息 
                    resutl = _repository.GetProductsByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
                case "productserialsync"://产品系列
                    resutl = _repository.GetProductSeriesByVersion(recVersion, userNumber, out maxVersion, out hasMoreData);
                    break;
            }

            return resutl;
        }



    }
}
