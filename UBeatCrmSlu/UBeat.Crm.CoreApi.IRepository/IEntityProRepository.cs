using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.DomainModel.Vocation;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IEntityProRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> EntityProQuery(EntityProQueryMapper crmEntityQuery, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> EntityProInfoQuery(EntityProInfoMapper entity, int userNumber);
        OperateResult InsertEntityPro(EntityProSaveMapper entity, int userNumber);

        OperateResult SaveEntityGlobalJs(EntityGlobalJsMapper entity, int userNumber);
        OperateResult UpdateEntityPro(EntityProSaveMapper entity, int userNumber);

		List<IDictionary<string, object>> CheckDeleteEntityPro(EntityProMapper entity, int userNumber);

		OperateResult DisabledEntityPro(EntityProMapper entity, int userNumber);
        OperateResult DeleteEntityData(DeleteEntityDataMapper entity, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> EntityOrderbyQuery(EntityOrderbyMapper entity, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> EntityClassQuery(EntityProMapper entity, int userNumber);

        OperateResult OrderByEntityPro(OrderByEntityProMapper entity, int userNumber);

        List<EntityFieldProMapper> FieldQuery(string entityId, int userNumber);

        //     List<EntityFieldProMapper> FieldQuery(string relentityId, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> EntityFieldProQuery(string entityId, int userNumber);

        OperateResult InsertEntityField(EntityFieldProSaveMapper entity, int userNumber);

        OperateResult UpdateEntityField(EntityFieldProSaveMapper entity, int userNumber);

        OperateResult UpdateEntityFieldExpandJS(EntityFieldExpandJSDataMapper entity, int userNumber);

        OperateResult UpdateEntityFieldFilterJS(EntityFieldFilterJSDataMapper entity, int userNumber);

        OperateResult DisabledEntityFieldPro(EntityFieldProMapper entity, int userNumber);

        OperateResult DeleteEntityFieldPro(EntityFieldProMapper entity, int userNumber);

        OperateResult OrderByEntityFieldPro(ICollection<EntityFieldProMapper> entities, int userNumber);


        Dictionary<string, List<IDictionary<string, object>>> EntityTypeQuery(EntityTypeQueryMapper entityType, int userNumber);
        bool CheckAndNestEntityType(string entityid,int userNum);

        OperateResult InsertEntityTypePro(SaveEntityTypeMapper entityType, int userNumber);

        OperateResult UpdateEntityTypePro(SaveEntityTypeMapper entityType, int userNumber);

        OperateResult DisabledEntityTypePro(EntityTypeMapper entityType, int userNumber);

        OperateResult OrderByEntityType(ICollection<EntityTypeMapper> entities, int userNumber);

        List<EntityFieldRulesMapper> EntityFieldRulesQuery(string entityId, string typeId, int userNumber);

        OperateResult SaveEntityFieldRules(ICollection<EntityFieldRulesSaveMapper> rules, int userNumber);
        List<EntityFieldRulesVocationMapper> EntityFieldRulesVocationQuery(string entityId, string vocationId, int userNumber);

        OperateResult SaveEntityFieldRulesVocation(string entityid, ICollection<EntityFieldRulesVocationSaveMapper> rules, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> EntityFieldFilterQuery(string entityid, int userNumber);

        OperateResult UpdateEntityFieldFilter(string searchJson, string simpleSearchJson, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> FieldWebVisibleQuery(string entityid, int userNumber);

        OperateResult SaveWebFieldVisible(SaveListViewColumnMapper listView, int userNumber);
        Dictionary<string, List<IDictionary<string, object>>> FieldMOBVisibleQuery(string entityid, int userNumber);

        OperateResult InsertMOBFieldVisible(ListViewMapper view, int userNumber);

        OperateResult UpdateMOBFieldVisible(ListViewMapper view, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> EntityPageConfigInfoQuery(EntityPageConfigMapper pageConfig, int userNumber);

        OperateResult SaveEntityPageConfig(EntityPageConfigMapper pageConfig, int userNumber);
        OperateResult CheckFieldRepeat(string entityId, string fieldId, string dataId, string dataValue, int userId);
        List<EntityFieldProMapper> NeedCheckFieldRepeat(string entityId, int userId);
        dynamic GetEntityInfo(Guid typeId, int userNumber);
        void UpdateEntityFieldConfig(Guid fieldId, Dictionary<string, object> fieldConfig, int v);

        /// <summary>
        /// 获取实体部分信息
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        SimpleEntityInfo GetEntityInfo(Guid typeId);

        dynamic GetFieldInfo(Guid fieldId, int userNumber);

        /// <summary>
        /// 获取实体页面入口信息
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        ServicesJsonInfo GetEntryPagesInfo(Guid entityId);

        bool SaveEntryPagesInfo(Guid entityId, ServicesJsonInfo info, int userNumber);

        /// <summary>
        /// 获取实体功能配置数据
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        FunctionJsonInfo GetFunctionJsonInfo(Guid entityId);
        /// <summary>
        /// 保存功能按钮json配置
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        bool SaveFunctionJson(Guid entityId, FunctionJsonInfo info,int userNumber, DbTransaction trans = null);


        Dictionary<string, List<IDictionary<string, object>>> SetRepeatList(string entityId, int userId);
        OperateResult SaveSetRepeat(string entityId, string fieldIds, int userId);

        OperateResult SaveEntanceGroup(string entranceJson, int userId);

        Dictionary<string, List<IDictionary<string, object>>> EntranceListQuery(int userNumber);


        Dictionary<string, List<IDictionary<string, object>>> RelContorlValueQuery(RelControlValueMapper entity, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> PersonalSettingQuery(Guid entityId, int userNumber);
        OperateResult SavePersonalViewSet(List<PersonalViewSetMapper> entities, int userId);

         OperateResult SaveEntityBaseData(List<EntityBaseDataMapper> entity, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> EntityBaseDataFieldQuery(EntityBaseDataFieldMapper entity, int userNumber);

        /// <summary>
        /// 获取关联实体列表
        /// </summary>
        /// <param name="entityid"></param>
        /// <param name="usernumber"></param>
        /// <returns></returns>
        List<RelateEntity> GetRelateEntityList( Guid entityid, int usernumber);
        /// <summary>
        /// 获取实体的菜单列表
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        List<EntityMenuInfo> GetEntityMenuInfoList(Guid entityId);


        /// <summary>
        /// 获取实体的tabid
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        Guid GetEntityRelTabId(Guid entityId,string entitytaburl);

        /// <summary>
        /// 同步实体功能列表到function表
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        void SyncFunctionList(Guid entityId, List<FunctionInfo> webfuncs, List<FunctionInfo> mobilefuncs, int usernumber);

        List<EntityProMapper> GetDynamicEntityList(int modelType = 3);

        #region 获取扩展配置信息
        //获取事件表信息
        List<FuncEvent> GetFuncEvent(DbTransaction tran, Guid entityId, int userNumber);
        //获取方法配置表信息
        List<ActionExtConfig> GetActionExtConfig(DbTransaction tran, Guid entityId, int userNumber);
        //获取方法信息
        List<ExtFunction> GetExtFunction(DbTransaction tran, Guid entityId, int userNumber);

        void UpdateFuncEvent(DbTransaction tran, Guid entityId, List<FuncEvent> data, int userNumber);

        void UpdateActionExt(DbTransaction tran, Guid entityId, List<ActionExtConfig> data, int userNumber);

        void UpdateExtFunction(DbTransaction tran, Guid entityId, List<ExtFunction> data, int userNumber);
        List<Dictionary<string, object>> QueryEntityWithDataSource(DbTransaction tran, int userId);
        #endregion

        #region 与实体定义的导入有关的操作
        Dictionary<string, object> GetEntityInfoByEntityName(DbTransaction tran, string entityName, int userId);
        Dictionary<string, object> GetEntityInfoByTableName(DbTransaction tran, string tablename, int userId);
        
        Dictionary<string, object> GetFieldInfoByFieldName(DbTransaction tran, string fieldName, Guid entityId,int userId);
        Dictionary<string, object> GetFieldInfoByDisplayName(DbTransaction tran, string displayName, Guid entityId, int userId);
        void DeleteEntityFieldRules(Guid catelogid, int userId);
        void MapEntityType(Guid subTypeId, Guid mainTypeId);
        void UpdateEntityFieldName(DbTransaction tran,Guid fieldId, string fieldName, int userId);
        void UpdateEntityFieldName(DbTransaction tran, Guid fieldId, Dictionary<string, string> displayName_Lang, int userId);
        void SaveEntityInputMethod(DbTransaction tran, Guid entityId, List<EntityInputModeInfo> inputs, int userId);
        #endregion
    }
}