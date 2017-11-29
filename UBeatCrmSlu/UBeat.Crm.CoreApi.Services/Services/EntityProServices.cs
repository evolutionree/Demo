using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.Services.Models.EntityPro;
using System.Linq;
using Newtonsoft.Json.Linq;
using UBeat.Crm.CoreApi.Services.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using Microsoft.Extensions.Caching.Redis;
using UBeat.Crm.CoreApi.Repository.Repository.Cache;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Vocation;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class EntityProServices : BaseServices
    {
        private readonly IEntityProRepository _entityProRepository;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IMapper _mapper;
        private readonly IVocationRepository _vocationRepository;

        public EntityProServices(IMapper mapper, IEntityProRepository entityProRepository, IDynamicEntityRepository dynamicEntityRepository, CacheServices cacheService, IVocationRepository vocationRepository)
        {
            _entityProRepository = entityProRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _mapper = mapper;
            _vocationRepository = vocationRepository;
        }

        public OutputResult<object> EntityProQuery(EntityProQueryModel entityQuery, int userNumber)
        {
            var entity = _mapper.Map<EntityProQueryModel, EntityProQueryMapper>(entityQuery);
            return new OutputResult<object>(_entityProRepository.EntityProQuery(entity, userNumber));
        }
        public OutputResult<object> EntityProInfoQuery(EntityProInfoModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityProInfoModel, EntityProInfoMapper>(entityModel);
            return new OutputResult<object>(_entityProRepository.EntityProInfoQuery(entity, userNumber));
        }
        public OutputResult<object> InsertEntityPro(EntityProModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityProModel, EntityProSaveMapper>(entityModel);
            OperateResult newEntity = _entityProRepository.InsertEntityPro(entity, userNumber);
            if (newEntity.Flag == 0)
                return HandleResult(newEntity);
            var result = HandleResult(newEntity);
            if (result.Status == 0)
            {
                //RemoveUserDataCache(userNumber);
                RemoveCommonCache();
                RemoveAllUserCache();
                IncreaseDataVersion(DataVersionType.EntityData);
                IncreaseDataVersion(DataVersionType.PowerData);
            }

            RelTabListMapper initEntity = new RelTabListMapper
            {
                EntityId = new Guid(newEntity.Id)
            };
            //_dynamicEntityRepository.initDefaultTab(initEntity, userNumber);
            return result;
        }
        public OutputResult<object> SaveEntityGlobalJs(EntityGlobalJsModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityGlobalJsModel, EntityGlobalJsMapper>(entityModel);
            OperateResult newEntity = _entityProRepository.SaveEntityGlobalJs(entity, userNumber);
            if (newEntity.Flag == 0)
                return HandleResult(newEntity);
            var result = HandleResult(newEntity);
            if (result.Status == 0)
            {
                RemoveCommonCache();
                RemoveAllUserCache();
                IncreaseDataVersion(DataVersionType.EntityData);
                IncreaseDataVersion(DataVersionType.PowerData);
            }

            return result;
        }
        public OutputResult<object> UpdateEntityPro(EntityProModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityProModel, EntityProSaveMapper>(entityModel);
            var result = HandleResult(_entityProRepository.UpdateEntityPro(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            IncreaseDataVersion(DataVersionType.PowerData);
            RemoveUserDataCache(userNumber);
            RemoveCommonCache();
            return result;
        }

        public OutputResult<object> DisabledEntityPro(EntityProModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityProModel, EntityProMapper>(entityModel);
            var result = HandleResult(_entityProRepository.DisabledEntityPro(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            IncreaseDataVersion(DataVersionType.PowerData);
            RemoveUserDataCache(userNumber);
            RemoveCommonCache();
            return result;
        }
        public OutputResult<object> DeleteEntityData(DeleteEntityDataModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<DeleteEntityDataModel, DeleteEntityDataMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var result = HandleResult(_entityProRepository.DeleteEntityData(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            IncreaseDataVersion(DataVersionType.PowerData);
            RemoveUserDataCache(userNumber);
            RemoveCommonCache();
            return result;
        }
        public OutputResult<object> EntityClassQuery(EntityProModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityProModel, EntityProMapper>(entityModel);
            return new OutputResult<object>(_entityProRepository.EntityClassQuery(entity, userNumber));
        }
        public OutputResult<object> EntityOrderbyQuery(EntityOrderbyModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityOrderbyModel, EntityOrderbyMapper>(entityModel);
            return new OutputResult<object>(_entityProRepository.EntityOrderbyQuery(entity, userNumber));
        }
        public OutputResult<object> OrderByEntityPro(OrderByEntityProModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<OrderByEntityProModel, OrderByEntityProMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            return HandleResult(_entityProRepository.OrderByEntityPro(entity, userNumber));
        }


        public OutputResult<object> EntityFieldProQuery(string entityId, int userNumber)
        {
            return new OutputResult<object>(_entityProRepository.EntityFieldProQuery(entityId, userNumber));
        }

        public OutputResult<object> InsertEntityField(EntityFieldProModel entityModel, int userNumber)
        {
            if (checkIsKeyFieldName(entityModel)) {
                return new OutputResult<object>(null, "【字段表列名】不能是保留的关键字",-1);
            }
            var entity = _mapper.Map<EntityFieldProModel, EntityFieldProSaveMapper>(entityModel);
            
            var result = HandleResult(_entityProRepository.InsertEntityField(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }
        private bool checkIsKeyFieldName(EntityFieldProModel fieldInfo) {
            if (KeyFieldNameList().ContainsKey(fieldInfo.FieldName.ToLower())) {
                return true;
            }
            return false;
        }

        private Dictionary<string, string> KeyFieldNameList() {
            Dictionary<string, string> ret = new Dictionary<string, string>();
            ret.Add("rectype".ToLower(), "rectype");
            ret.Add("id".ToLower(), "id");
            ret.Add("recid".ToLower(), "recid");
            ret.Add("recname".ToLower(), "recname");
            return ret;
        }

        public OutputResult<object> UpdateEntityField(EntityFieldProModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityFieldProModel, EntityFieldProSaveMapper>(entityModel);
            var result = HandleResult(_entityProRepository.UpdateEntityField(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<Object> UpdateEntityFieldExpandJS(EntityFieldExpandJSModel entityFieldExpandJS, int userNumber)
        {
            EntityFieldExpandJSDataMapper entity = new EntityFieldExpandJSDataMapper()
            {
                ExpandJS = entityFieldExpandJS.ExpandJS,
                FieldId = entityFieldExpandJS.FieldId
            };
            var result = HandleResult(_entityProRepository.UpdateEntityFieldExpandJS(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<Object> UpdateEntityFieldFilterJS(EntityFieldFilterJSModel entityFieldFilterJS, int userNumber)
        {
            EntityFieldFilterJSDataMapper entity = new EntityFieldFilterJSDataMapper()
            {
                FilterJS = entityFieldFilterJS.FilterJS,
                FieldId = entityFieldFilterJS.FieldId
            };
            var result = HandleResult(_entityProRepository.UpdateEntityFieldFilterJS(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> DisabledEntityFieldPro(EntityFieldProModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityFieldProModel, EntityFieldProMapper>(entityModel);
            var result = HandleResult(_entityProRepository.DisabledEntityFieldPro(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }
        public OutputResult<object> DeleteEntityFieldPro(EntityFieldProModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityFieldProModel, EntityFieldProMapper>(entityModel);
            var result = HandleResult(_entityProRepository.DeleteEntityFieldPro(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> OrderByEntityFieldPro(ICollection<EntityFieldProModel> entityModels, int userNumber)
        {
            IList<EntityFieldProMapper> entities = new List<EntityFieldProMapper>();
            foreach (var entity in entityModels)
            {
                entities.Add(_mapper.Map<EntityFieldProModel, EntityFieldProMapper>(entity));
            }
            var result = HandleResult(_entityProRepository.OrderByEntityFieldPro(entities, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> EntityTypeQuery(EntityTypeQueryModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityTypeQueryModel, EntityTypeQueryMapper>(entityModel);
            bool typeChange = _entityProRepository.CheckAndNestEntityType(entityModel.EntityId, userNumber);
            if (typeChange)
            {
                IncreaseDataVersion(DataVersionType.EntityData);
            }
            return new OutputResult<object>(_entityProRepository.EntityTypeQuery(entity, userNumber));
        }

        public OutputResult<object> InsertEntityTypePro(EntityTypeModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityTypeModel, SaveEntityTypeMapper>(entityModel);
            var result = HandleResult(_entityProRepository.InsertEntityTypePro(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> UpdateEntityTypePro(EntityTypeModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityTypeModel, SaveEntityTypeMapper>(entityModel);
            var result = HandleResult(_entityProRepository.UpdateEntityTypePro(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> DisabledEntityTypePro(EntityTypeModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityTypeModel, EntityTypeMapper>(entityModel);
            var result = HandleResult(_entityProRepository.DisabledEntityTypePro(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> OrderByEntityType(ICollection<EntityTypeModel> entityModels, int userNumber)
        {
            IList<EntityTypeMapper> entities = new List<EntityTypeMapper>();
            foreach (var entity in entityModels)
            {
                entities.Add(_mapper.Map<EntityTypeModel, EntityTypeMapper>(entity));
            }
            var result = HandleResult(_entityProRepository.OrderByEntityType(entities, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> EntityFieldRulesQuery(string entityId, string typeId, int userNumber)
        {
            var result = _entityProRepository.EntityFieldRulesQuery(entityId, typeId, userNumber);
            var handlResult = result.GroupBy(t =>
                                                                        new
                                                                        {
                                                                            FieldId = t.FieldId,
                                                                            TypeId = t.TypeId,
                                                                            FieldLabel = t.FieldLabel,
                                                                            RecStatus = t.RecStatus
                                                                        }).Select(group => new EntityFieldRulesSaveMapper
                                                                        {
                                                                            FieldId = group.Key.FieldId.ToString(),
                                                                            TypeId = group.Key.TypeId.ToString(),
                                                                            FieldLabel = group.Key.FieldLabel,
                                                                            RecStatus = group.Key.RecStatus,
                                                                            Rules = group.Select(g => new FieldRulesDetailMapper
                                                                            {
                                                                                FieldRulesId = g.FieldRulesId,
                                                                                IsReadOnly = g.IsReadOnly,
                                                                                IsVisible = g.IsVisible,
                                                                                IsRequired = g.IsRequire,
                                                                                OperateType = g.OperateType,
                                                                                ValidRule = g.ValidRules == "{}" ? new JObject() : JObject.Parse(g.ValidRules),
                                                                                ViewRule = g.ViewRules == "{}" ? new JObject() : JObject.Parse(g.ViewRules),
                                                                            }).ToList()
                                                                        });
            return new OutputResult<object>(handlResult.ToList());
        }

        public OutputResult<object> SaveEntityFieldRules(ICollection<EntityFieldRulesSaveModel> rulesModels, int userNumber)
        {
            IList<EntityFieldRulesSaveMapper> rulesEntities = new List<EntityFieldRulesSaveMapper>();
            foreach (var rule in rulesModels)
            {
                rulesEntities.Add(_mapper.Map<EntityFieldRulesSaveModel, EntityFieldRulesSaveMapper>(rule));
            }
            var result = HandleResult(_entityProRepository.SaveEntityFieldRules(rulesEntities, userNumber));
            //var re= _cacheRepository.Add("EntityFieldRules", 1);
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> EntityFieldRulesVocationQuery(string entityId, string VocationId, int userNumber)
        {
            var result = _entityProRepository.EntityFieldRulesVocationQuery(entityId, VocationId, userNumber);
            var handlResult = result.GroupBy(t =>
                                                                        new
                                                                        {
                                                                            FieldLabel = t.FieldLabel,
                                                                            FieldId = t.FieldId,
                                                                            EntityId = t.EntityId,
                                                                            VocationId = t.VocationId,
                                                                            RecStatus = t.RecStatus
                                                                        }).Select(group => new EntityFieldRulesVocationListMapper
                                                                        {
                                                                            FieldId = group.Key.FieldId.ToString(),
                                                                            VocationId = group.Key.VocationId.ToString(),
                                                                            FieldLabel = group.Key.FieldLabel,
                                                                            RecStatus = group.Key.RecStatus,
                                                                            Rules = group.Select(g => new FieldRulesVocationInfoMapper
                                                                            {
                                                                                FieldRulesId = g.FieldRulesId.ToString(),
                                                                                OperateType = g.OperateType,
                                                                                IsVisible = g.IsVisible,
                                                                                IsReadOnly = g.IsReadOnly,
                                                                            }).ToList()
                                                                        });
            return new OutputResult<object>(handlResult.ToList());
        }

        public OutputResult<object> SaveEntityFieldRulesVocation(string entityid, ICollection<EntityFieldRulesVocationSaveModel> rulesModels, int userNumber)
        {
            IList<EntityFieldRulesVocationSaveMapper> rulesEntities = new List<EntityFieldRulesVocationSaveMapper>();
            foreach (var rule in rulesModels)
            {
                rulesEntities.Add(_mapper.Map<EntityFieldRulesVocationSaveModel, EntityFieldRulesVocationSaveMapper>(rule));
            }
            var result = HandleResult(_entityProRepository.SaveEntityFieldRulesVocation(entityid, rulesEntities, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }


        public OutputResult<object> EntityFieldFilterQuery(string entityid, int userNumber)
        {
            return new OutputResult<object>(_entityProRepository.EntityFieldFilterQuery(entityid, userNumber));
        }

        public OutputResult<object> UpdateEntityFieldFilter(SimpleSearchModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SimpleSearchModel, SimpleSearchMapper>(entityModel);
            string searchJson = JsonConvert.SerializeObject(entity.AdvanceSearch);
            string simpleSearchJson = JsonConvert.SerializeObject(entity);
            var result = HandleResult(_entityProRepository.UpdateEntityFieldFilter(searchJson, simpleSearchJson, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }
        public OutputResult<object> FieldWebVisibleQuery(string entityid, int userNumber)
        {
            return new OutputResult<object>(_entityProRepository.FieldWebVisibleQuery(entityid, userNumber));
        }

        public OutputResult<object> SaveWebFieldVisible(SaveListViewColumnModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SaveListViewColumnModel, SaveListViewColumnMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            return HandleResult(_entityProRepository.SaveWebFieldVisible(entity, userNumber));
        }
        public OutputResult<object> FieldMOBVisibleQuery(string entityid, int userNumber)
        {
            var res = _entityProRepository.FieldMOBVisibleQuery(entityid, userNumber);
            
            return new OutputResult<object>(res);
        }

        public OutputResult<object> InsertMOBFieldVisible(ListViewModel view, int userNumber)
        {
            var entity = _mapper.Map<ListViewModel, ListViewMapper>(view);
            var result = HandleResult(_entityProRepository.InsertMOBFieldVisible(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> UpdateMOBFieldVisible(ListViewModel view, int userNumber)
        {
            var entity = _mapper.Map<ListViewModel, ListViewMapper>(view);
            var result = HandleResult(_entityProRepository.UpdateMOBFieldVisible(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }
        public OutputResult<object> EntityPageConfigInfoQuery(EntityPageConfigModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityPageConfigModel, EntityPageConfigMapper>(entityModel);
            return new OutputResult<object>(_entityProRepository.EntityPageConfigInfoQuery(entity, userNumber));
        }
        public OutputResult<object> SaveEntityPageConfig(EntityPageConfigModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityPageConfigModel, EntityPageConfigMapper>(entityModel);
            var result = HandleResult(_entityProRepository.SaveEntityPageConfig(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }
        public OutputResult<object> SetRepeatList(SetRepeatModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SetRepeatModel, SetRepeatMapper>(entityModel);
            return new OutputResult<object>(_entityProRepository.SetRepeatList(entity.EntityId, userNumber));
        }

        public OutputResult<object> SaveSetRepeat(SetRepeatModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SetRepeatModel, SaveSetRepeatMapper>(entityModel);
            return new OutputResult<object>(_entityProRepository.SaveSetRepeat(entity.EntityId, entity.Fieldids, userNumber));
        }

        public OutputResult<object> SaveEntanceGroup(ICollection<SaveEntranceGroupModel> entityModel, int userNumber)
        {
            var entity = _mapper.Map<ICollection<SaveEntranceGroupModel>, ICollection<SaveEntranceGroupMapper>>(entityModel);
            string entranceJson = JsonConvert.SerializeObject(entity);
            var result = HandleResult(_entityProRepository.SaveEntanceGroup(entranceJson, userNumber));
            if (result.Status == 0)
            {
                //GetCommonCacheData(userNumber, true);
                RemoveCommonCache();
                RemoveAllUserCache();
                IncreaseDataVersion(DataVersionType.EntityData);
                IncreaseDataVersion(DataVersionType.PowerData);
                //RemoveUserDataCache(userNumber);

            }
            return result;
        }

        public OutputResult<object> EntranceListQuery(int userNumber)
        {
            return new OutputResult<object>(_entityProRepository.EntranceListQuery(userNumber));
        }


        public OutputResult<object> RelContorlValueQuery(RelControlValueModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<RelControlValueModel, RelControlValueMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return new OutputResult<object>(_entityProRepository.RelContorlValueQuery(entity, userNumber));
        }
        public OutputResult<object> PersonalSettingQuery(PersonalSettingModel entityModel, int userNumber)
        {
            return new OutputResult<object>(_entityProRepository.PersonalSettingQuery(entityModel.EntityId, userNumber));
        }
        public OutputResult<object> SavePersonalViewSet(List<PersonalViewSetModel> entityModels, int userId)
        {
            List<PersonalViewSetMapper> entities = new List<PersonalViewSetMapper>();
            foreach (var model in entityModels)
            {
                var entity = _mapper.Map<PersonalViewSetModel, PersonalViewSetMapper>(model);
                if (entity == null || !entity.IsValid())
                {
                    return HandleValid(entity);
                }
                entities.Add(entity);
            }
            return HandleResult(_entityProRepository.SavePersonalViewSet(entities, userId));
        }

        public OutputResult<object> SaveEntityBaseData(ICollection<EntityBaseDataModel> models, int userNumber)
        {
            List<EntityBaseDataMapper> entities = new List<EntityBaseDataMapper>();
            foreach (var model in models)
            {
                var entity = _mapper.Map<EntityBaseDataModel, EntityBaseDataMapper>(model);
                if (entity == null || !entity.IsValid())
                {
                    return HandleValid(entity);
                }
                entities.Add(entity);
            }
            return HandleResult(_entityProRepository.SaveEntityBaseData(entities, userNumber));
        }

        public OutputResult<object> EntityBaseDataFieldQuery(EntityBaseDataFieldModel model, int userNumber)
        {

            var entity = _mapper.Map<EntityBaseDataFieldModel, EntityBaseDataFieldMapper>(model);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            return new OutputResult<object>(_entityProRepository.EntityBaseDataFieldQuery(entity, userNumber));
        }


        /// <summary>
        /// 获取功能按钮列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> GetFunctionBtnList(FunctionBtnListModel dynamicModel, int userNumber)
        {
            List<FunctionBtnInfo> funcBtns = new List<FunctionBtnInfo>();
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null)
                return new OutputResult<object>(funcBtns);

            funcBtns = info.FuncBtns;
            return new OutputResult<object>(funcBtns);
        }

        #region --实体功能列表配置--
        /// <summary>
        /// 获取实体功能列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> GetFunctionList(FunctionBtnListModel dynamicModel, int userNumber)
        {
            if (dynamicModel == null)
                throw new Exception("参数不可为空");
            if (dynamicModel.EntityId == Guid.Empty)
                throw new Exception("参数EntityId不可为空");
            List<FunctionInfo> webfuncs = new List<FunctionInfo>();
            List<FunctionInfo> mobilefuncs = new List<FunctionInfo>();

            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null)
            {
                info = new FunctionJsonInfo();
            }
            var entityInfo = _entityProRepository.GetEntityInfo(dynamicModel.EntityId);

            webfuncs = info.WebFunctions;
            mobilefuncs = info.MobileFunctions;
            if (info.WebFunctions == null || info.WebFunctions.Count == 0)
                webfuncs = GetDefaultFunctions(entityInfo, 0);
            if (info.MobileFunctions == null || info.MobileFunctions.Count == 0)
                mobilefuncs = GetDefaultFunctions(entityInfo, 1);
            var result = new { Web = webfuncs, Mobile = mobilefuncs };
            return new OutputResult<object>(result);
        }

        /// <summary>
        /// 保存实体功能列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> SaveFunctionList(SaveFuncsModel dynamicModel, int userNumber)
        {
            if (dynamicModel == null)
                throw new Exception("参数不可为空");
            if (dynamicModel.EntityId == Guid.Empty)
                throw new Exception("参数EntityId不可为空");
            if (dynamicModel.MobileFuncs == null)
                throw new Exception("参数MobileFuncs不可为空");
            if (dynamicModel.WebFuncs == null )
                throw new Exception("参数WebFuncs不可为空");
           
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null)
            {
                info = new FunctionJsonInfo();
            }
            info.WebFunctions = dynamicModel.WebFuncs;
            info.MobileFunctions = dynamicModel.MobileFuncs;

            if (_entityProRepository.SaveFunctionJson(dynamicModel.EntityId, info, userNumber))
                return new OutputResult<object>("保存成功");

            return new OutputResult<object>("保存失败");
           
        }

        /// <summary>
        /// 同步实体功能列表到function表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> SyncFunctionList(SyncFuncListModel dynamicModel, int userNumber)
        {
            if (dynamicModel == null)
                throw new Exception("参数不可为空");
            if (dynamicModel.EntityId == Guid.Empty)
                throw new Exception("参数EntityId不可为空");
            List<FunctionInfo> webfuncs = new List<FunctionInfo>();
            List<FunctionInfo> mobilefuncs = new List<FunctionInfo>();

            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null)
            {
                throw new Exception("不存在已配置的数据，请先保存配置再同步");
            }
            _entityProRepository.SyncFunctionList(dynamicModel.EntityId, info.WebFunctions, info.MobileFunctions, userNumber);
            return new OutputResult<object>("OK");
        }



        #region --获取默认的实体function--
        public List<FunctionInfo> GetDefaultFunctions(SimpleEntityInfo entityInfo, int deviceType)
        {
            List<FunctionInfo> functions = new List<FunctionInfo>();
            Guid entityId = entityInfo.EntityId;
            string entityName = entityInfo.EntityName;
            EntityModelType modelType = entityInfo.ModelType;
            if (modelType == EntityModelType.Independent || modelType == EntityModelType.Simple)
            {

                //实体根节点
                var entityroot = new FunctionInfo(Guid.NewGuid(), Guid.Empty, entityName, "Entity", entityId, deviceType, FunctionType.Entity, 0, null, null);
                functions.Add(entityroot);

                //一级节点：菜单
                var menuNode = new FunctionInfo(Guid.NewGuid(), entityroot.FuncId, entityName + "菜单", "EntityMenu", entityId, deviceType, FunctionType.Menu, 0, null, null);
                functions.Add(menuNode);
                #region --二级节点 ：菜单--
                //获取 menuid
                string menuid = null;
                var menus = _entityProRepository.GetEntityMenuInfoList(entityId);
                var menuinfo = menus.Find(m => m.MenuType == MenuType.AllList);
                menuid = menuinfo == null ? null : menuinfo.MenuId.ToString();
                functions.Add(new FunctionInfo(Guid.NewGuid(), menuNode.FuncId, "全部数据", "AllEntityData", entityId, deviceType, FunctionType.Default, -1, menuid, "api/dynamicentity/list"));
                if (modelType == EntityModelType.Independent)
                {
                    menuinfo = menus.Find(m => m.MenuType == MenuType.TransferList);
                    menuid = menuinfo == null ? null : menuinfo.MenuId.ToString();
                    functions.Add(new FunctionInfo(Guid.NewGuid(), menuNode.FuncId, "待转移数据", "TransferEntityData", entityId, deviceType, FunctionType.Default, -1, menuid, "api/dynamicentity/list"));
                }
                #endregion

                //一级节点：功能
                var funcNode = new FunctionInfo(Guid.NewGuid(), entityroot.FuncId, entityName + "功能", "EntityFunc", entityId, deviceType, FunctionType.Function, 0, null, null);
                functions.Add(funcNode);
                #region --二级节点 ：功能--
                //获取 menuid

                functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "新增", "EntityDataAdd", entityId, deviceType, FunctionType.Default, -1, null, "api/dynamicentity/add"));
                functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "编辑", "EntityDataEdit", entityId, deviceType, FunctionType.Default, -1, null, "api/dynamicentity/edit"));
                functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "删除", "EntityDataDelete", entityId, deviceType, FunctionType.Default, -1, null, "api/dynamicentity/delete"));
                if (modelType == EntityModelType.Independent)
                {
                    functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "转移", "EntityDataTransfer", entityId, deviceType, FunctionType.Default, -1, null, "api/dynamicentity/transfer"));
                }
                if (deviceType == 0)//web
                {
                    functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "导出", "EntityDataExport", entityId, deviceType, FunctionType.Default, -1, null, "api/dynamicentity/exportdata"));
                    functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "导入", "EntityDataImport", entityId, deviceType, FunctionType.Default, -1, null, "api/dynamicentity/importdata"));
                }

                #endregion


                if (modelType == EntityModelType.Independent)
                {
                    //一级节点：主页Tab
                    var tabNode = new FunctionInfo(Guid.NewGuid(), entityroot.FuncId, entityName + "主页Tab", "EntityTab", entityId, deviceType, FunctionType.Tab, 0, null, null);
                    functions.Add(tabNode);
                    #region --二级节点 ：主页Tab--

                    string docs_relateValue = _entityProRepository.GetEntityRelTabId(entityId, "docs").ToString();
                    var documet_tabNode = new FunctionInfo(Guid.NewGuid(), tabNode.FuncId, "文档", "EntityDataDocment", entityId, deviceType, FunctionType.Document, 0, null, null);
                    functions.Add(documet_tabNode);
                    if (deviceType == 0)//web
                    {
                        functions.Add(new FunctionInfo(Guid.NewGuid(), documet_tabNode.FuncId, "文档列表", "EntityDataDocmentList", entityId, deviceType, FunctionType.Default, -1, docs_relateValue, "api/dynamicentity/documentlist"));
                        functions.Add(new FunctionInfo(Guid.NewGuid(), documet_tabNode.FuncId, "文档上传", "DocumentUpload", entityId, deviceType, FunctionType.Default, -1, null, "api/documents/adddocument"));
                        functions.Add(new FunctionInfo(Guid.NewGuid(), documet_tabNode.FuncId, "文档删除", "DocumentDelete", entityId, deviceType, FunctionType.Default, -1, null, "api/documents/deletedocument"));
                        string relateValue = _entityProRepository.GetEntityRelTabId(entityId, "info").ToString();
                        functions.Add(new FunctionInfo(Guid.NewGuid(), tabNode.FuncId, "基础信息", "EntityDataDetail", entityId, deviceType, FunctionType.Default, -1, relateValue, "api/dynamicentity/detial"));
                        relateValue = _entityProRepository.GetEntityRelTabId(entityId, "activities").ToString();
                        functions.Add(new FunctionInfo(Guid.NewGuid(), tabNode.FuncId, "动态", "EntityDataDynamicList", entityId, deviceType, FunctionType.Default, -1, relateValue, "api/dynamicentity/getdynamiclist"));
                    }
                    else
                    {

                        functions.Add(new FunctionInfo(Guid.NewGuid(), documet_tabNode.FuncId, "文档列表", "EntityDataDocmentList", entityId, deviceType, FunctionType.Default, -1, docs_relateValue, "api/dynamicentity/documentlist"));
                        string chat_relateValue = _entityProRepository.GetEntityRelTabId(entityId, "chat").ToString();
                        functions.Add(new FunctionInfo(Guid.NewGuid(), tabNode.FuncId, "沟通", "EntityDataChat", entityId, deviceType, FunctionType.Default, -1, chat_relateValue, "api/chat/send"));
                    }
                    #endregion
                    //一级节点：主页动态
                    var dynamicNode = new FunctionInfo(Guid.NewGuid(), entityroot.FuncId, entityName + "主页动态Tab", "EntityDynamicTab", entityId, deviceType, FunctionType.Dynamic, 0, null, null);
                    functions.Add(dynamicNode);
                }
            }
            else if (modelType == EntityModelType.Dynamic)
            {
                if(entityInfo.RelAudit==0)
                {

                }
            }

            return functions;
        }


        #endregion



        #endregion
        /// <summary>
        /// 获取页面入口信息
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> GetEntryPages(ServiceJsonModel dynamicModel, int userNumber)
        {
            EntryPageModel pageInfo = new EntryPageModel();
            var info = _entityProRepository.GetEntryPagesInfo(dynamicModel.EntityId);
            if (info == null || info.EntryPages==null)
                return new OutputResult<object>(pageInfo);

            pageInfo = info.EntryPages;
            return new OutputResult<object>(pageInfo);
        }

        public OutputResult<object> SaveEntryPagesInfo(ServiceJsonDetailModel dynamicModel, int userNumber)
        {
            var info = _entityProRepository.GetEntryPagesInfo(dynamicModel.EntityId);
            if (info == null)
                info = new ServicesJsonInfo();
            if (info.EntryPages == null)
                info.EntryPages = new EntryPageModel();

            var model = new EntryPageModel()
            {
                WebListPage = dynamicModel.WebListPage,
                WebIndexPage = dynamicModel.WebIndexPage,
                WebEditPage = dynamicModel.WebEditPage,
                WebViewPage = dynamicModel.WebViewPage,
                AndroidListPage = dynamicModel.AndroidListPage,
                AndroidIndexPage = dynamicModel.AndroidIndexPage,
                AndroidEditPage = dynamicModel.AndroidEditPage,
                AndroidViewPage = dynamicModel.AndroidViewPage,
                IOSListPage = dynamicModel.IOSListPage,
                IOSIndexpage = dynamicModel.IOSIndexpage,
                IOSEditPage = dynamicModel.IOSEditPage,
                IOSViewPage = dynamicModel.IOSViewPage
            };
            info.EntryPages = model;
            if (_entityProRepository.SaveEntryPagesInfo(dynamicModel.EntityId, info, userNumber))
                return new OutputResult<object>("保存成功");

            return new OutputResult<object>("保存失败");
        }

        #region --检查function节点是否存在--
        /// <summary>
        /// 检查function节点是否存在
        /// </summary>
        /// <param name="totalFunctions"></param>
        /// <param name="dynamicModel"></param>
        /// <param name="userNumber"></param>
        /// <param name="deviceType">设备类型,0:web,1:mobile</param>
        private void CheckFunction(List<FunctionInfo> totalFunctions, FunctionBtnDetailModel dynamicModel, int userNumber, int deviceType)
        {
            var func = totalFunctions.FirstOrDefault(m => m.RoutePath == dynamicModel.RoutePath && m.EntityId == dynamicModel.EntityId && m.DeviceType == deviceType);
            if (func == null)
            {
                var parentFunction = totalFunctions.FirstOrDefault(m => m.EntityId == dynamicModel.EntityId && m.DeviceType == deviceType && m.RecType == FunctionType.Function);
                if (parentFunction == null)
                    throw new Exception("找不到该实体的Function功能节点");
                func = new FunctionInfo()
                {
                    FuncId = Guid.NewGuid(),
                    FuncName = dynamicModel.Name,
                    Funccode = dynamicModel.ButtonCode,
                    ParentId = parentFunction.FuncId,
                    EntityId = dynamicModel.EntityId,
                    DeviceType = deviceType,
                    RecType = FunctionType.Default,
                    ChildType = 1,
                    IsLastChild = 1,
                    RoutePath = dynamicModel.RoutePath,
                    RelationValue = null
                };
                var funResult = _vocationRepository.AddFunction(func, userNumber);
                if(funResult.Flag==0)
                {
                    throw new Exception("添加Function节点失败");
                }
            }
        } 
        #endregion

        public OutputResult<object> AddFunctionBtn(FunctionBtnDetailModel dynamicModel, int userNumber)
        {
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null)
                info = new FunctionJsonInfo();
            if (info.FuncBtns == null)
                info.FuncBtns = new List<FunctionBtnInfo>();
            var totalFunctions = _vocationRepository.GetTotalFunctions();
            List<FunctionInfo> funcs = new List<FunctionInfo>();

            if (totalFunctions != null)
            {
                CheckFunction(totalFunctions, dynamicModel, userNumber, 0);
                CheckFunction(totalFunctions, dynamicModel, userNumber, 1);
            }

            var model = new FunctionBtnInfo()
            {
                Id = Guid.NewGuid(),
                Name = dynamicModel.Name,
                Title = dynamicModel.Title,
                ButtonCode = dynamicModel.ButtonCode,
                Icon = dynamicModel.Icon,
                DisplayPosition = dynamicModel.DisplayPosition,
                IsRefreshPage = dynamicModel.IsRefreshPage,
                RoutePath = dynamicModel.RoutePath,
                RecOrder = info.FuncBtns.Count,
                SelectType = dynamicModel.SelectType,
                extraData= dynamicModel.extradata
            };
            info.FuncBtns.Add(model);
            if (_entityProRepository.SaveFunctionJson(dynamicModel.EntityId, info, userNumber))
                return new OutputResult<object>("保存成功");

            return new OutputResult<object>("保存失败");
        }

        public OutputResult<object> EditFunctionBtn(FunctionBtnDetailModel dynamicModel, int userNumber)
        {
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null|| info.FuncBtns == null|| !info.FuncBtns.Exists(m=>m.Id==dynamicModel.Id))
                throw new Exception("该数据无效，不能编辑保存");

            var totalFunctions = _vocationRepository.GetTotalFunctions();
            List<FunctionInfo> funcs = new List<FunctionInfo>();

            if (totalFunctions != null)
            {
                CheckFunction(totalFunctions, dynamicModel, userNumber, 0);
                CheckFunction(totalFunctions, dynamicModel, userNumber, 1);
            }
            var model = info.FuncBtns.FirstOrDefault(m => m.Id == dynamicModel.Id);
            model.Name = dynamicModel.Name;
            model.Title = dynamicModel.Title;
            model.ButtonCode = dynamicModel.ButtonCode;
            model.Icon = dynamicModel.Icon;
            model.DisplayPosition = dynamicModel.DisplayPosition;
            model.IsRefreshPage = dynamicModel.IsRefreshPage;
            model.RoutePath = dynamicModel.RoutePath;
            model.extraData = dynamicModel.extradata;
            //model.RecOrder = dynamicModel.RecOrder;
            model.SelectType = dynamicModel.SelectType;
            if( _entityProRepository.SaveFunctionJson(dynamicModel.EntityId, info, userNumber))
                return new OutputResult<object>("保存成功");

            return new OutputResult<object>("保存失败");

        }

        public OutputResult<object> DeleteFunctionBtn( DeleteFunctionBtnModel dynamicModel, int userNumber)
        {
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null || info.FuncBtns == null || !info.FuncBtns.Exists(m => m.Id == dynamicModel.Id))
                throw new Exception("该数据不存在");
            var deleteBtn = info.FuncBtns.FirstOrDefault(m => m.Id == dynamicModel.Id);
            info.FuncBtns.Remove(deleteBtn);
            foreach(var btn in info.FuncBtns)
            {
                if (btn.RecOrder > deleteBtn.RecOrder)
                {
                    btn.RecOrder = btn.RecOrder - 1;
                }
            }
            if (_entityProRepository.SaveFunctionJson(dynamicModel.EntityId, info, userNumber))
                return new OutputResult<object>("保存成功");

            return new OutputResult<object>("保存失败");
        }


        public OutputResult<object> SortFunctionBtn(SortFunctionBtnModel dynamicModel, int userNumber)
        {
            if(dynamicModel==null|| dynamicModel.OrderMapper == null|| dynamicModel.OrderMapper.Count==0)
            {
                throw new Exception("该数据无效");
            }
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null || info.FuncBtns == null )
                throw new Exception("该实体数据功能配置无效，不能排序");

            foreach(var map in dynamicModel.OrderMapper)
            {
                var btn = info.FuncBtns.FirstOrDefault(m => m.Id == map.Key);
                if (btn != null)
                    btn.RecOrder = map.Value;
            }
            info.FuncBtns = info.FuncBtns.OrderBy(m => m.RecOrder).ToList();
            if (_entityProRepository.SaveFunctionJson(dynamicModel.EntityId, info, userNumber))
                return new OutputResult<object>("保存成功");

            return new OutputResult<object>("保存失败");
        }

    }
}
