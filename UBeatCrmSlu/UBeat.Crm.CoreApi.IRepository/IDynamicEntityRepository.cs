﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDynamicEntityRepository : IBaseRepository
    {
        RuleContentMapper SelectRule(Guid ruleId, DbTransaction tran, int userNumber);
        List<RuleItemRelationDataMapper> SelectRuleRelation(Guid ruleId, DbTransaction tran, int userNumber);
        RuleSetDataMapper SelectRuleSet(Guid ruleId, DbTransaction tran, int userNumber);
          List<RuleItemDataMapper> SelectRuleItem(Guid ruleId, DbTransaction tran, int userNumber);
        bool ExistsRule(Guid ruleId);
        List<DynamicEntityDataFieldMapper> GetTypeFields(Guid typeId, int operateType, int userNumber);
        Guid getGridTypeByMainType(Guid typeId, Guid entityId);
        RelConfigInfo GetRelConfig(Guid RelId, int userNumber);
        List<IDictionary<string, object>> GetRelConfigEntity(RelTabListMapper entity, int userNumber);
        decimal queryDataForDataSource_CalcuteType(RelConfig config, Guid parentRecId, int userNumber);
        decimal queryDataForDataSource_funcType(RelConfig config, Guid parentRecId, int userNumber);
        OperateResult SaveRelConfig(List<RelConfig> entity, Guid RelId, int userNumber);

        List<IDictionary<string, object>> GetRelConfigFields(GetEntityFieldsMapper entity, int userNumber);

        OperateResult SaveRelConfigSet(List<RelConfigSet> entity,Guid RelId, int userNumber);

        List<DynamicEntityWebFieldMapper> GetWebFields(Guid entityId, int viewType, int userNumber);

        List<DynamicEntityWebFieldMapper> GetWebDynamicListFields(Guid typeId, int operateType, int userNumber);

        List<DynamicEntityFieldSearch> GetSearchFields(Guid entityId, int userNumber);
        List<DynamicEntityFieldSearch> GetEntityFields(Guid entityId, int userNumber, IDbTransaction tran = null);
        OperateResult DynamicAdd(DbTransaction tran, Guid typeId, Dictionary<string, object> fieldData, Dictionary<string, object> extraData, int userNumber);

        void DynamicAddList(List<DynamicEntityAddListMapper> data, int userNumber);

        OperateResult DynamicAdd(DbTransaction tran, Guid typeId, Dictionary<string, object> fieldData, Guid flowId, Guid? relEntityId, Guid? relRecId, int userNumber);

        OperateResult DynamicEdit(DbTransaction tran, Guid typeId, Guid recId, Dictionary<string, object> fieldData, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> DataList(PageParam pageParam, Dictionary<string, object> extraData, DynamicEntityListMapper searchParm, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> DataListUseFunc(string funcName,PageParam pageParam, Dictionary<string, object> extraData, DynamicEntityListMapper searchParm, int userNumber);
        string CheckDataListSpecFunction(Guid entityid);
        IDictionary<string, object> Detail(DynamicEntityDetailtMapper detailMapper, int userNumber,DbTransaction transaction = null);

        List<IDictionary<string, object>> DetailList(DynamicEntityDetailtListMapper detailMapper, int userNumber, DbTransaction tran);

        Dictionary<string, List<IDictionary<string, object>>> DetailMulti(DynamicEntityDetailtMapper detailMapper, int userNumber, DbTransaction tran = null);

        List<GeneralDicItem> GetDicItemByKeys(string dicKeys);
        int inertEntityModify(DynamicEntityModifyMapper record, Guid entityId, Guid businessid, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> PluginVisible(DynamicPluginVisibleMapper visibleMapper, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> PageVisible(DynamicPageVisibleMapper visibleMapper, int userNumber);

        Guid DynamicEntityExist(Guid entityid, Dictionary<string, object> fieldData, Guid updateRecid);

        OperateResult Transfer(DynamicEntityTransferMapper transMapper, int userNumber);

        OperateResult Delete(DbTransaction trans, Guid entityId, string recIds, int pageType, string pageCode, int userNumber);

        OperateResult DeleteDataSrcRelation(DataSrcDeleteRelationMapper entityMapper, int userNumber);

        OperateResult AddConnect(DynamicEntityAddConnectMapper connectMapper, int userNumber);

        OperateResult EditConnect(DynamicEntityEditConnectMapper connectMapper, int userNumber);

        OperateResult DeleteConnect(Guid connectId, int userNumber);

        List<IDictionary<string, object>> ConnectList(Guid entityId, Guid recId, int userNumber);

        List<IDictionary<string, object>> EntitySearchList(int modelType, Dictionary<string, object> searchData, int userNumber);

        List<IDictionary<string, object>> EntitySearchRepeat(Guid entityId, string checkField, string checkName, int extra, Dictionary<string, object> searchData, int userNumber);

        string ReturnRelTabSql(Guid relId, Guid recId, int userNumber);
        RelTabSrcMapper ReturnRelTabSrcSql(Guid relId);
        List<RelTabSrcValListMapper> GetSrcSqlDataList(Guid recId, string srcSql, string searchWhere, int userNumber);

        int initDefaultTab(RelTabListMapper entity, int userNumber);

        List<IDictionary<string, object>> GetRelTabEntity(RelTabListMapper entity, int userNumber);

        List<IDictionary<string, object>> GetRelEntityFields(GetEntityFieldsMapper entity, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> RelTabListQuery_1(RelTabListMapper entity, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> RelTabInfoQuery_1(RelTabInfoMapper entity, int userNumber);
        OperateResult AddRelTab_1(AddRelTabMapper entity, int userNumber);
        OperateResult UpdateRelTab_1(UpdateRelTabMapper entity, int userNumber);
        OperateResult DisabledRelTab_1(DisabledRelTabMapper entity, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> RelTabListQuery(RelTabListMapper entity, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> RelTabInfoQuery(RelTabInfoMapper entity, int userNumber);
        OperateResult AddRelTab(AddRelTabMapper entity, int userNumber);
        OperateResult UpdateRelTab(UpdateRelTabMapper entity, int userNumber);
        int UpdateDefaultRelTab(UpdateRelTabMapper entity, int userNumber);
        OperateResult DisabledRelTab(DisabledRelTabMapper entity, int userNumber);
        OperateResult OrderbyRelTab(OrderbyRelTabMapper entity, int userNumber);
        OperateResult AddRelTabRelationDataSrc(AddRelTabRelationDataSrcMapper entity, int userNumber);
        OperateResult IsHasPerssion(PermissionMapper entity, int userNumber);

        //获取数据源控件类型的所有字段
        List<EntityFieldInfo> GetDataSourceEntityFields();
        Dictionary<string, object> getEntityBaseInfoById(Guid entityid, int userNum, DbTransaction tran = null);
        Dictionary<string, object> getEntityBaseInfoByTypeId(Guid typeid, int userNum, DbTransaction tran = null);
        Dictionary<string, object> getEntityBaseInfoByCaseId(Guid caseid, int userNum, DbTransaction tran = null);
        bool WriteBack(DbTransaction tran, List<Dictionary<string, object>> writebackrules, int userNum);


        OperateResult FollowRecord(FollowRecordMapper entity, int userNumber);
        Dictionary<string, object> GetAllDataSourceDefine(DbTransaction tran);
        List<Dictionary<string, object>> ExecuteQuery(string strSQL, DbTransaction tran);
        List<Dictionary<string, object>> ExecuteQuery(string strSQL, DbParameter[] dbParmas, DbTransaction tran);
        OperateResult MarkRecordComplete(Guid recId, int userNumber);
        EntityExtFunctionInfo getExtFunctionByFunctionName(Guid entityId, string functionname,DbTransaction tran = null);
        object  ExecuteExtFunction(EntityExtFunctionInfo funcInfo, string [] recIds, Dictionary<string, object> otherParams, int userId, DbTransaction tran = null);
        Dictionary<string, object> GetPersonalWebListColumnsSetting(Guid entityId, int userId, DbTransaction tran);
        void UpdatePersonalWebListColumnsSetting(Guid entityId,WebListPersonalViewSettingInfo viewConfig, int userId, DbTransaction tran);
        void AddPersonalWebListColumnsSetting(Guid entityId,WebListPersonalViewSettingInfo viewConfig, int userId, DbTransaction tran);

        //查重
        Dictionary<string, object> QueryEntityCondition(DynamicEntityCondition entity, DbTransaction tran);
        //修改查重
        bool UpdateEntityCondition(List<DynamicEntityCondition> entityList, int userNumber, DbTransaction tran);

        bool ExistsData(Guid CacheId, int userNumber, DbTransaction tran);
        bool AddTemporaryData(TemporarySaveMapper data, int userNumber, DbTransaction tran);
        bool UpdateTemporaryData(TemporarySaveMapper data, int userNumber, DbTransaction tran);
        void DeleteTemporary(Guid CacheId, int userNumber, DbTransaction tran);
        List<Dictionary<string,object>> SelectTemporaryDetails(Guid cacheId, int userNumber, DbTransaction tran);
        bool DeleteTemporaryList(List<Guid> cacheIds,int userNumber,DbTransaction tran);
        void AddRule(RuleContentMapper ruleData, Guid RelId, DbTransaction tran, int userNumber);
    
        void AddRuleItems(List<RuleItemDataMapper> listItem, Guid RelId, DbTransaction tran, int userNumber);
        void AddRuleSet(RuleSetDataMapper ruleSet, Guid RelId, DbTransaction tran, int userNumber);
        void AddRuleItemRelation(List<RuleItemRelationDataMapper> relaList, Guid RelId, DbTransaction tran, int userNumber);


    }
}
