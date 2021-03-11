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
using UBeat.Crm.CoreApi.Repository.Repository.Cache;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class EntityProServices : BasicBaseServices
    {
        private readonly IEntityProRepository _entityProRepository;
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IMapper _mapper;
        private readonly IVocationRepository _vocationRepository;
        private readonly IDataSourceRepository _dataSourceRepository;

        public EntityProServices(IMapper mapper, IEntityProRepository entityProRepository,
            IDynamicEntityRepository dynamicEntityRepository, CacheServices cacheService,
            IVocationRepository vocationRepository, IDataSourceRepository dataSourceRepository)
        {
            _entityProRepository = entityProRepository;
            _dynamicEntityRepository = dynamicEntityRepository;
            _mapper = mapper;
            _vocationRepository = vocationRepository;
            _dataSourceRepository = dataSourceRepository;
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
            string EntityName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(entity.EntityName, entity.EntityName_Lang, out EntityName);
            if (EntityName != null) entity.EntityName = EntityName;
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
        public OutputResult<object> SaveNestedTablesEntity(NestedTablesModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<NestedTablesModel, NestedTablesMapper>(entityModel);
            OperateResult newEntity = _entityProRepository.SaveNestedTablesEntity(entity, userNumber);
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
            string EntityName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(entity.EntityName, entity.EntityName_Lang, out EntityName);
            if (EntityName != null) entity.EntityName = EntityName;
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

			//删除时验证
			if(entity.RecStatus == 2)
			{
				var msgSB = new StringBuilder();
				var msgItems = _entityProRepository.CheckDeleteEntityPro(entity, userNumber);
				foreach (var item in msgItems)
				{
					var dataType = int.Parse(item["datatype"].ToString());
					var msg = string.Concat(item["msg"]);
					if (!string.IsNullOrEmpty(msg))
					{
						switch (dataType)
						{
							case 0:
								msgSB.AppendFormat(@"存在关联的数据源：{0}；", msg);
								break;
							case 1:
								msgSB.AppendFormat(@"存在关联的实体：{0}；", msg);
								break;
							case 2:
								msgSB.AppendFormat(@"存在关联的流程：{0}；", msg);
								break;
							default:
								break;
						}
					} 
				}

				if (!string.IsNullOrEmpty(msgSB.ToString()))
					return new OutputResult<object>(null, msgSB.ToString(), 1);
			}

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
            if (checkIsKeyFieldName(entityModel))
            {
                return new OutputResult<object>(null, "【字段表列名】不能是保留的关键字", -1);
            }
            var entity = _mapper.Map<EntityFieldProModel, EntityFieldProSaveMapper>(entityModel); 
            string DisplayName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(entity.DisplayName, entity.DisplayName_Lang, out DisplayName);
            if (DisplayName != null) entity.DisplayName = DisplayName;
			//FieldLabel 同 DisplayName
			entity.FieldLabel_Lang = entity.DisplayName_Lang;
			entity.FieldLabel = DisplayName;

			var result = HandleResult(_entityProRepository.InsertEntityField(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public void UpdateFieldName(Guid fieldId, Dictionary<string, string> displayName_Lang, int userId)
        {
            DbTransaction tran = null;
            this._entityProRepository.UpdateEntityFieldName(tran, fieldId, displayName_Lang, userId);
        }

        private bool checkIsKeyFieldName(EntityFieldProModel fieldInfo)
        {
            if (KeyFieldNameList().ContainsKey(fieldInfo.FieldName.ToLower()))
            {
                return true;
            }
            return false;
        }

        private Dictionary<string, string> KeyFieldNameList()
        {
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
            string DisplayName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(entity.DisplayName, entity.DisplayName_Lang, out DisplayName);
            if (DisplayName != null) entity.DisplayName = DisplayName;
			//FieldLabel 同 DisplayName
			entity.FieldLabel_Lang = entity.DisplayName_Lang;
			entity.FieldLabel = DisplayName;

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
            string CategoryName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(entity.CategoryName, entity.CategoryName_Lang, out CategoryName);
            if (CategoryName != null) entity.CategoryName = CategoryName;
            var result = HandleResult(_entityProRepository.InsertEntityTypePro(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return result;
        }

        public OutputResult<object> UpdateEntityTypePro(EntityTypeModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EntityTypeModel, SaveEntityTypeMapper>(entityModel);
            string CategoryName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(entity.CategoryName, entity.CategoryName_Lang, out CategoryName);
            if (CategoryName != null) entity.CategoryName = CategoryName;
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

        public List<Dictionary<string, object>> QueryEntityWithDataSource(int userId)
        {
            DbTransaction tran = null;
            List<Dictionary<string, object>> entitys = this._entityProRepository.QueryEntityWithDataSource(tran, userId);
            DataSourceListMapper listMapper = new DataSourceListMapper() {
                PageIndex = 1,
                PageSize = 10000,
                DatasourceName = null,
                RecStatus = 1
            };
            Dictionary<string, List<IDictionary<string, object>>> datasourcesmap = this._dataSourceRepository.SelectDataSource(listMapper, userId);
            List<IDictionary<string, object>> datasources = datasourcesmap["PageData"];
            foreach (Dictionary<string, object> entity in entitys) {
                string entityid = "";
                if (entity.ContainsKey("entityid") == false || entity["entityid"] == null) continue;
                entityid = entity["entityid"].ToString();
                List<IDictionary<string, object>> subSource = new List<IDictionary<string, object>>();
                foreach (IDictionary<string, object> item in datasources) {
                    if (item != null && item.ContainsKey("entityid") && item["entityid"] != null
                        && item["entityid"].ToString().Equals(entityid)) {
                        subSource.Add(item);
                    }
                }

                entity.Add("datasources", subSource);
            }
            return entitys;
        }

        public Dictionary<string, object> getRefFieldsByFieldId(string fieldId, int userId)
        {
            IDictionary<string, object> fieldInfo = this._entityProRepository.GetFieldInfo(Guid.Parse(fieldId), userId);
            if (fieldInfo == null) {
                throw (new Exception("无法获取字段信息"));
            }
            Dictionary<string, object> ds = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(fieldInfo["fieldconfig"].ToString());
            Dictionary<string, object> dstmp = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(Newtonsoft.Json.JsonConvert.SerializeObject(ds["dataSource"]));
            string datasourceid = dstmp["sourceId"].ToString();
            IDictionary<string, object> datasourceinfo = this._dataSourceRepository.GetDataSourceInfo(Guid.Parse(datasourceid), userId);
            string entityid = datasourceinfo["entityid"].ToString();
            IDictionary<string, object> entityInfo = this._entityProRepository.GetEntityInfo(Guid.Parse(entityid), userId);
            List<EntityFieldProMapper> fields = this._entityProRepository.FieldQuery(entityid, userId);
            Dictionary<string, object> retdict = new Dictionary<string, object>();
            retdict.Add("entity", entityInfo);
            retdict.Add("fields", fields);
            return retdict;
        }

        /// <summary>
        /// 获取实体输入方式详情,如果没有就默认初始化为有
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<object> GetEntityInputMethod(EntityProInfoModel paramInfo, int userId)
        {
            OutputResult<object> tmp = this.EntityProInfoQuery(paramInfo, userId);
            if (tmp.Status != 0 || tmp.DataBody == null)
            {
                throw (new Exception(tmp.Message));
            }
            Dictionary<string, List<IDictionary<string, object>>> tmpDict = (Dictionary<string, List<IDictionary<string, object>>>)tmp.DataBody;
            if (tmpDict == null || tmpDict.ContainsKey("EntityProInfo") == false || tmpDict["EntityProInfo"] == null || tmpDict["EntityProInfo"].Count == 0)
            {
                throw (new Exception("实体配置异常"));
            }
            IDictionary<string, object> entityDict = tmpDict["EntityProInfo"].First();
            if (entityDict.ContainsKey("InputMethod") && entityDict["InputMethod"] != null)
            {
                List<EntityInputModeInfo> ret = null;
                if (entityDict["InputMethod"] is string)
                {
                    ret = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EntityInputModeInfo>>((string)entityDict["InputMethod"]);
                }
                else {

                    ret = Newtonsoft.Json.JsonConvert.DeserializeObject<List<EntityInputModeInfo>>(JsonConvert.SerializeObject(entityDict["InputMethod"]));
                }
                bool hasCommon = false;
                foreach (EntityInputModeInfo item in ret) {
                    if (item.InputMethod == EntityInputMethod.CommonInput) {
                        hasCommon = true; break;
                    }
                }
                if (hasCommon == false) {
                    EntityInputModeInfo item = new EntityInputModeInfo()
                    {
                        InputMethod = EntityInputMethod.CommonInput,
                        Title = "新增"
                    };
                    ret.Add(item);
                }
                return new OutputResult<object>(ret);
            }
            else {
                List<EntityInputModeInfo> ret = new List<EntityInputModeInfo>();
                EntityInputModeInfo item = new EntityInputModeInfo()
                {
                    InputMethod = EntityInputMethod.CommonInput,
                    Title = "新增"
                };
                ret.Add(item);
                return new OutputResult<object>(ret);
            }
        }

        public OutputResult<object> SaveEntityInputMethod(Guid EntityId, List<EntityInputModeInfo> inputs, int userId) {
            DbTransaction tran = null;
            try
            {

                this._entityProRepository.SaveEntityInputMethod(tran, EntityId, inputs, userId);
                return new OutputResult<object>("ok");
            }
            catch (Exception ex) {
                return new OutputResult<object>(null, ex.Message, -1);
            }
           
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
            List<FunctionModel> webfuncs = new List<FunctionModel>();
            List<FunctionModel> mobilefuncs = new List<FunctionModel>();

            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null)
            {
                info = new FunctionJsonInfo();
            }
            var entityInfo = _entityProRepository.GetEntityInfo(dynamicModel.EntityId);
            if (entityInfo == null)
                throw new Exception("实体数据不存在");

            if (info.WebFunctions == null || info.WebFunctions.Count == 0)
                info.WebFunctions = GetDefaultFunctions(entityInfo, 0);
            if (info.MobileFunctions == null || info.MobileFunctions.Count == 0)
                info.MobileFunctions = GetDefaultFunctions(entityInfo, 1);

            webfuncs = FunctionModelMap(info.WebFunctions);
            mobilefuncs = FunctionModelMap(info.MobileFunctions);
            var result = new { Web = webfuncs, Mobile = mobilefuncs };
            return new OutputResult<object>(result);
        }

        private List<FunctionModel> FunctionModelMap(List<FunctionInfo> infos)
        {
            List<FunctionModel> funcs = new List<FunctionModel>();
            foreach (var m in infos)
            {
                funcs.Add(new FunctionModel(m.FuncId, m.ParentId, m.FuncName, m.Funccode, m.EntityId, m.IsLastChild, m.RelationValue, m.RoutePath));
            }
            return funcs;
        }
        private List<FunctionInfo> FunctionInfoMap(List<FunctionModel> infos, int devicetype)
        {
            List<FunctionInfo> funcs = new List<FunctionInfo>();
            foreach (var m in infos)
            {
                if (m != null)
                {
                    FunctionType rectype = GetFunctionType(m.Funccode, devicetype);
                    funcs.Add(new FunctionInfo(m.FuncId, m.ParentId, m.FuncName, m.Funccode, m.EntityId, devicetype, rectype, m.IsLastChild, m.RelationValue, m.RoutePath));
                }

            }
            return funcs;
        }

        private FunctionType GetFunctionType(string funccode, int devicetype)
        {
            FunctionType rectype = FunctionType.Function;
            if (string.IsNullOrEmpty(funccode))
            {
                throw new Exception("Funccode不可为空");
            }
            switch (funccode)
            {
                case "Entity":
                    rectype = FunctionType.Entity; break;
                case "EntityMenu":
                    rectype = FunctionType.EntityMenu; break;
                case "EntityDataList":
                    rectype = FunctionType.Function; break;
                case "EntityFunc":
                    rectype = FunctionType.EntityFunc; break;
                case "EntityDataAdd":
                    rectype = FunctionType.Function; break;
                case "EntityDataEdit":
                    rectype = FunctionType.Function; break;
                case "EntityDataDelete":
                    rectype = FunctionType.Function; break;
                case "EntityDataTransfer":
                    rectype = FunctionType.Function; break;
                case "EntityDataExport":
                    rectype = FunctionType.Function; break;
                case "EntityDataImport":
                    rectype = FunctionType.Function; break;
                case "EntityTab":
                    rectype = FunctionType.EntityTab; break;
                case "EntityDataDocment":
                    rectype = FunctionType.DocumentList; break;
                case "EntityDataDocmentList":
                    rectype = FunctionType.Function; break;
                case "DocumentUpload":
                    rectype = FunctionType.Function; break;
                case "DocumentDelete":
                    rectype = FunctionType.Function; break;
                case "EntityDataDetail":
                    rectype = FunctionType.Function; break;
                case "EntityDataDynamicList":
                    rectype = FunctionType.Function; break;
                case "EntityDynamicTab":
                    rectype = FunctionType.EntityDynamicTab; break;
                case "EntityDynamic":
                    rectype = FunctionType.Function; break;

                case "EntityDataChat":
                    rectype = FunctionType.Function;
                    break;
                default:
                    throw new Exception("未约定的funccode");
            }



            return rectype;
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
            if (dynamicModel.MobileFuncs == null)//不可为null，如果不配置节点，则传空list，否则影响其他地方的业务逻辑
                throw new Exception("参数MobileFuncs不可为NULL");
            if (dynamicModel.WebFuncs == null)//不可为null，如果不配置节点，则传空list，否则影响其他地方的业务逻辑
                throw new Exception("参数WebFuncs不可为NULL");

            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null)
            {
                info = new FunctionJsonInfo();
            }
            var entityInfo = _entityProRepository.GetEntityInfo(dynamicModel.EntityId);
            if (entityInfo == null)
                throw new Exception("实体数据不存在");
            if (entityInfo.ModelType != EntityModelType.Independent && entityInfo.ModelType != EntityModelType.Simple)
                throw new Exception("只有独立实体和简单实体可以配置function");

            info.WebFunctions = FunctionInfoMap(dynamicModel.WebFuncs, 0);
            info.MobileFunctions = FunctionInfoMap(dynamicModel.MobileFuncs, 1);

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

        public void SaveFunctionNode(Guid entityid, FunctionType nodeType, string funcname, string relateValue, int userNumber, string funccode = null, string routePath = null)
        {
            if (entityid == Guid.Empty)
                throw new Exception("参数entityid不可为空");
            if (string.IsNullOrEmpty(funcname))
                throw new Exception("参数funcname不可为空");

            var info = _entityProRepository.GetFunctionJsonInfo(entityid);
            if (info == null)
            {
                info = new FunctionJsonInfo();
            }
            var entityInfo = _entityProRepository.GetEntityInfo(entityid);


            if (info.WebFunctions == null)//如果为null ，则说明该节点还未生成，需要初始化数据
                info.WebFunctions = GetDefaultFunctions(entityInfo, 0);
            if (info.MobileFunctions == null)//如果为null ，则说明该节点还未生成，需要初始化数据
                info.MobileFunctions = GetDefaultFunctions(entityInfo, 1);
            switch (nodeType)
            {
                case FunctionType.EntityMenu: //relateValue为menuid
                    if (string.IsNullOrEmpty(relateValue))
                        throw new Exception("relateValue必须为有效的menuid");
                    UpdateMenuFunctions(entityid, funcname, relateValue, info.WebFunctions, entityInfo);
                    UpdateMenuFunctions(entityid, funcname, relateValue, info.MobileFunctions, entityInfo);
                    break;
                case FunctionType.EntityFunc:
                    if (string.IsNullOrEmpty(funccode))
                        throw new Exception("funccode不可为空");
                    UpdateEntityFuncs(entityid, funcname, funccode, relateValue, info.WebFunctions, entityInfo, routePath);
                    UpdateEntityFuncs(entityid, funcname, funccode, relateValue, info.MobileFunctions, entityInfo, routePath);
                    break;
                case FunctionType.EntityTab:
                    break;
                case FunctionType.EntityDynamicTab:
                    break;
            }

            if (!_entityProRepository.SaveFunctionJson(entityid, info, userNumber))
                throw new Exception("生成function节点失败");
        }


        #region --更新菜单列表的function数据--
        private void UpdateMenuFunctions(Guid entityid, string funcname, string relateValue, List<FunctionInfo> functions, SimpleEntityInfo entityInfo)
        {
            var rootNode = functions.Find(m => m.RecType == FunctionType.EntityMenu);
            if (rootNode == null)//如果此时为null，说明该节点已被手动设置为空
            {
                return;
            }
            var menuNode = functions.Find(m => m.RecType == FunctionType.Function && m.RelationValue == relateValue);
            if (menuNode != null)//如果已经存在，则update
            {
                menuNode.RelationValue = relateValue;
                menuNode.FuncName = funcname;
            }
            else
            {
                menuNode = new FunctionInfo(Guid.NewGuid(), rootNode.FuncId, funcname, "EntityDataList", entityInfo.EntityId, rootNode.DeviceType, FunctionType.Function, -1, relateValue, "api/dynamicentity/list");
                functions.Add(menuNode);
            }
        }
        #endregion

        #region --更新功能列表的function数据--
        private void UpdateEntityFuncs(Guid entityid, string funcname, string funccode, string relateValue, List<FunctionInfo> functions, SimpleEntityInfo entityInfo, string routePath)
        {
            var rootNode = functions.Find(m => m.RecType == FunctionType.EntityFunc);
            if (rootNode == null)//如果此时为null，说明该节点已被手动设置为空
            {
                return;
            }
            var node = functions.Find(m => m.RecType == FunctionType.Function && m.Funccode == funccode);
            if (node != null)//如果已经存在，则update
            {
                node.FuncName = funcname;
                node.Funccode = funccode;
                node.RoutePath = routePath;
                node.RelationValue = relateValue;
            }
            else
            {
                node = new FunctionInfo(Guid.NewGuid(), rootNode.FuncId, funcname, funccode, entityInfo.EntityId, rootNode.DeviceType, FunctionType.Function, -1, relateValue, routePath);
                functions.Add(node);
            }
        }
        #endregion

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
                var menuNode = new FunctionInfo(Guid.NewGuid(), entityroot.FuncId, entityName + "菜单", "EntityMenu", entityId, deviceType, FunctionType.EntityMenu, 0, null, null);
                functions.Add(menuNode);
                #region --二级节点 ：菜单--
                var menus = _entityProRepository.GetEntityMenuInfoList(entityId);
                foreach (var menuinfo in menus)
                {
                    var menuid = menuinfo == null ? null : menuinfo.MenuId.ToString();
                    functions.Add(new FunctionInfo(Guid.NewGuid(), menuNode.FuncId, menuinfo.MenuName, "EntityDataList", entityId, deviceType, FunctionType.Function, -1, menuid, "api/dynamicentity/list"));
                }

                #endregion

                //一级节点：功能
                var funcNode = new FunctionInfo(Guid.NewGuid(), entityroot.FuncId, entityName + "功能", "EntityFunc", entityId, deviceType, FunctionType.EntityFunc, 0, null, null);
                functions.Add(funcNode);
                #region --二级节点 ：功能--
                //获取 menuid

                functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "新增", "EntityDataAdd", entityId, deviceType, FunctionType.Function, -1, null, "api/dynamicentity/add"));
                functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "编辑", "EntityDataEdit", entityId, deviceType, FunctionType.Function, -1, null, "api/dynamicentity/edit"));
                functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "删除", "EntityDataDelete", entityId, deviceType, FunctionType.Function, -1, null, "api/dynamicentity/delete"));
                if (modelType == EntityModelType.Independent)
                {
                    functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "转移", "EntityDataTransfer", entityId, deviceType, FunctionType.Function, -1, null, "api/dynamicentity/transfer"));
                }
                if (deviceType == 0)//web
                {
                    functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "导出", "EntityDataExport", entityId, deviceType, FunctionType.Function, -1, null, "api/dynamicentity/exportdata"));
                    functions.Add(new FunctionInfo(Guid.NewGuid(), funcNode.FuncId, "导入", "EntityDataImport", entityId, deviceType, FunctionType.Function, -1, null, "api/dynamicentity/importdata"));
                }

                #endregion


                if (modelType == EntityModelType.Independent)
                {
                    //一级节点：主页Tab
                    var tabNode = new FunctionInfo(Guid.NewGuid(), entityroot.FuncId, entityName + "主页Tab", "EntityTab", entityId, deviceType, FunctionType.EntityTab, 0, null, null);
                    functions.Add(tabNode);
                    #region --二级节点 ：主页Tab--

                    string docs_relateValue = _entityProRepository.GetEntityRelTabId(entityId, "docs").ToString();
                    var documet_tabNode = new FunctionInfo(Guid.NewGuid(), tabNode.FuncId, "文档", "EntityDataDocment", entityId, deviceType, FunctionType.DocumentList, 0, null, null);
                    functions.Add(documet_tabNode);
                    if (deviceType == 0)//web
                    {
                        functions.Add(new FunctionInfo(Guid.NewGuid(), documet_tabNode.FuncId, "文档列表", "EntityDataDocmentList", entityId, deviceType, FunctionType.Function, -1, docs_relateValue, "api/dynamicentity/documentlist"));
                        functions.Add(new FunctionInfo(Guid.NewGuid(), documet_tabNode.FuncId, "文档上传", "DocumentUpload", entityId, deviceType, FunctionType.Function, -1, null, "api/documents/adddocument"));
                        functions.Add(new FunctionInfo(Guid.NewGuid(), documet_tabNode.FuncId, "文档删除", "DocumentDelete", entityId, deviceType, FunctionType.Function, -1, null, "api/documents/deletedocument"));
                        string relateValue = _entityProRepository.GetEntityRelTabId(entityId, "info").ToString();
                        functions.Add(new FunctionInfo(Guid.NewGuid(), tabNode.FuncId, "基础信息", "EntityDataDetail", entityId, deviceType, FunctionType.Function, -1, relateValue, "api/dynamicentity/detial"));
                        relateValue = _entityProRepository.GetEntityRelTabId(entityId, "activities").ToString();
                        functions.Add(new FunctionInfo(Guid.NewGuid(), tabNode.FuncId, "动态", "EntityDataDynamicList", entityId, deviceType, FunctionType.Function, -1, relateValue, "api/dynamicentity/getdynamiclist"));
                    }
                    else
                    {

                        functions.Add(new FunctionInfo(Guid.NewGuid(), documet_tabNode.FuncId, "文档列表", "EntityDataDocmentList", entityId, deviceType, FunctionType.Function, -1, docs_relateValue, "api/dynamicentity/documentlist"));
                        string chat_relateValue = _entityProRepository.GetEntityRelTabId(entityId, "chat").ToString();
                        functions.Add(new FunctionInfo(Guid.NewGuid(), tabNode.FuncId, "沟通", "EntityDataChat", entityId, deviceType, FunctionType.Function, -1, chat_relateValue, "api/chat/send"));
                    }
                    #endregion
                    //一级节点：主页动态
                    var dynamicNode = new FunctionInfo(Guid.NewGuid(), entityroot.FuncId, entityName + "主页动态Tab", "EntityDynamicTab", entityId, deviceType, FunctionType.EntityDynamicTab, 0, null, null);
                    functions.Add(dynamicNode);
                }
            }
            else if (modelType == EntityModelType.Dynamic)
            {
                if (entityInfo.RelAudit == 0)
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
            if (info == null || info.EntryPages == null)
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
        /// <param name="otype">操作类型，0=新增，1=编辑，2=删除</param>
        /// <param name="totalFunctions"></param>
        /// <param name="dynamicModel"></param>
        /// <param name="userNumber"></param>
        /// <param name="deviceType">设备类型,0:web,1:mobile</param>
        private Guid CheckFunction(int otype, List<FunctionInfo> totalFunctions, FunctionBtnDetailModel dynamicModel, int userNumber, int deviceType, Guid funcid, DbTransaction trans = null)
        {
            Guid newfuncid = Guid.Empty;
            FunctionInfo func = null;
            if (otype != 0 && funcid == Guid.Empty)
            {
                func = totalFunctions.FirstOrDefault(m => m.Funccode == dynamicModel.ButtonCode && m.EntityId == dynamicModel.EntityId && m.DeviceType == deviceType);
            }
            else func = totalFunctions.FirstOrDefault(m => m.FuncId == funcid);

            if (func == null)
            {
                if (otype == 2)
                {
                    return newfuncid;
                }
                var parentFunction = totalFunctions.FirstOrDefault(m => m.EntityId == dynamicModel.EntityId && m.DeviceType == deviceType && m.RecType == FunctionType.EntityFunc);
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
                    RecType = FunctionType.Function,
                    ChildType = 1,
                    IsLastChild = -1,
                    RoutePath = dynamicModel.RoutePath,
                    RelationValue = null
                };
                var funResult = _vocationRepository.AddFunction(func, userNumber, trans);

                if (funResult.Flag == 0)
                {
                    throw new Exception("添加Function节点失败");
                }
                newfuncid = Guid.Parse(funResult.Id);
            }
            else
            {
                bool succ = false;
                if (otype == 2)
                {
                    succ = _vocationRepository.DeleteFunction(func.FuncId, userNumber, trans);
                }
                else
                {
                    newfuncid = func.FuncId;
                    func.Funccode = dynamicModel.ButtonCode;
                    func.FuncName = dynamicModel.Name;
                    func.RoutePath = dynamicModel.RoutePath;
                    func.IsLastChild = -1;
                    succ = _vocationRepository.EditFunction(func, userNumber, trans);
                }

                if (!succ)
                {
                    throw new Exception("处理Function节点失败");
                }
            }
            return newfuncid;
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

            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    List<FunctionInfo> funcs = new List<FunctionInfo>();
                    Guid webFuncid = Guid.Empty;
                    Guid mobileFuncid = Guid.Empty;
                    if (totalFunctions != null)
                    {
                        webFuncid = CheckFunction(0, totalFunctions, dynamicModel, userNumber, 0, Guid.Empty, tran);
                        mobileFuncid = CheckFunction(0, totalFunctions, dynamicModel, userNumber, 1, Guid.Empty, tran);
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
                        extraData = dynamicModel.extradata,
                        WebFuncId = webFuncid,
                        MobileFuncId = mobileFuncid,
                        FuncBtnLanguage = dynamicModel.FuncBtnLanguage,
                        Title_Lang = dynamicModel.Title_Lang

                    };
                    string Title = "";
                    MultiLanguageUtils.GetDefaultLanguageValue(model.Title, model.Title_Lang, out Title);
                    if (Title != null) model.Title = Title;
                    info.FuncBtns.Add(model);
                    if (_entityProRepository.SaveFunctionJson(dynamicModel.EntityId, info, userNumber, tran))
                    {
                        tran.Commit();
                        return new OutputResult<object>("保存成功");

                    }

                    return new OutputResult<object>("保存失败");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public OutputResult<object> EditFunctionBtn(FunctionBtnDetailModel dynamicModel, int userNumber)
        {
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null || info.FuncBtns == null || !info.FuncBtns.Exists(m => m.Id == dynamicModel.Id))
                throw new Exception("该数据无效，不能编辑保存");
            var model = info.FuncBtns.FirstOrDefault(m => m.Id == dynamicModel.Id);
            var totalFunctions = _vocationRepository.GetTotalFunctions();
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    List<FunctionInfo> funcs = new List<FunctionInfo>();
                    Guid webFuncid = Guid.Empty;
                    Guid mobileFuncid = Guid.Empty;
                    if (totalFunctions != null)
                    {
                        webFuncid = CheckFunction(1, totalFunctions, dynamicModel, userNumber, 0, model.WebFuncId, tran);
                        mobileFuncid = CheckFunction(1, totalFunctions, dynamicModel, userNumber, 1, model.MobileFuncId, tran);
                    }

                    model.Name = dynamicModel.Name;
                    model.ButtonCode = dynamicModel.ButtonCode;
                    model.Icon = dynamicModel.Icon;
                    model.DisplayPosition = dynamicModel.DisplayPosition;
                    model.IsRefreshPage = dynamicModel.IsRefreshPage;
                    model.RoutePath = dynamicModel.RoutePath;
                    model.extraData = dynamicModel.extradata;
                    //model.RecOrder = dynamicModel.RecOrder;
                    model.SelectType = dynamicModel.SelectType;
                    model.WebFuncId = webFuncid;
                    model.MobileFuncId = mobileFuncid;
                    model.FuncBtnLanguage = dynamicModel.FuncBtnLanguage;
                    model.Title_Lang = dynamicModel.Title_Lang;
                    model.Title = dynamicModel.Title;
                    string Title = "";
                    MultiLanguageUtils.GetDefaultLanguageValue(model.Title, model.Title_Lang, out Title);
                    if (Title != null) model.Title = Title;
                    if (_entityProRepository.SaveFunctionJson(dynamicModel.EntityId, info, userNumber, tran))
                    {
                        tran.Commit();
                        return new OutputResult<object>("保存成功");

                    }

                    return new OutputResult<object>("保存失败");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

        }

        public OutputResult<object> DeleteFunctionBtn(DeleteFunctionBtnModel dynamicModel, int userNumber)
        {
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null || info.FuncBtns == null || !info.FuncBtns.Exists(m => m.Id == dynamicModel.Id))
                throw new Exception("该数据不存在");
            var deleteBtn = info.FuncBtns.FirstOrDefault(m => m.Id == dynamicModel.Id);
            info.FuncBtns.Remove(deleteBtn);
            foreach (var btn in info.FuncBtns)
            {
                if (btn.RecOrder > deleteBtn.RecOrder)
                {
                    btn.RecOrder = btn.RecOrder - 1;
                }
            }
            var totalFunctions = _vocationRepository.GetTotalFunctions();
            using (var conn = GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    Guid webFuncid = Guid.Empty;
                    Guid mobileFuncid = Guid.Empty;
                    if (totalFunctions != null)
                    {
                        FunctionBtnDetailModel model = new FunctionBtnDetailModel()
                        {
                            RoutePath = deleteBtn.RoutePath,
                            EntityId = dynamicModel.EntityId,
                        };
                        webFuncid = CheckFunction(2, totalFunctions, model, userNumber, 0, deleteBtn.WebFuncId, tran);
                        mobileFuncid = CheckFunction(2, totalFunctions, model, userNumber, 1, deleteBtn.MobileFuncId, tran);
                    }
                    if (_entityProRepository.SaveFunctionJson(dynamicModel.EntityId, info, userNumber, tran))
                    {
                        tran.Commit();
                        return new OutputResult<object>("保存成功");

                    }

                    return new OutputResult<object>("保存失败");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

        }


        public OutputResult<object> SortFunctionBtn(SortFunctionBtnModel dynamicModel, int userNumber)
        {
            if (dynamicModel == null || dynamicModel.OrderMapper == null || dynamicModel.OrderMapper.Count == 0)
            {
                throw new Exception("该数据无效");
            }
            var info = _entityProRepository.GetFunctionJsonInfo(dynamicModel.EntityId);
            if (info == null || info.FuncBtns == null)
                throw new Exception("该实体数据功能配置无效，不能排序");

            foreach (var map in dynamicModel.OrderMapper)
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


        /// <summary>
        /// 获取扩展配置信息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OutputResult<object> GetFunctionConfig(FuncConfig data, int userNumber)
        {
            DbTransaction tran = null;
            var funcEventData = new Dictionary<string, List<FuncEvent>>();
            var funcEvent = _entityProRepository.GetFuncEvent(tran, data.EntityId, userNumber);
            var distEvent = funcEvent.Select(r => r.TypeId).Distinct();
            foreach (var item in distEvent)
            {
                funcEventData.Add(item.ToString(), funcEvent.Where(r => r.TypeId == item).ToList());
            }
            
            var acConfig = _entityProRepository.GetActionExtConfig(tran, data.EntityId, userNumber);
            var extFunction = _entityProRepository.GetExtFunction(tran, data.EntityId, userNumber);
            var funcConfigList = new Dictionary<string, object>();
            funcConfigList.Add("funcEvent", funcEventData);
            funcConfigList.Add("acConfig", acConfig);
            funcConfigList.Add("extFunction", extFunction);
            return new OutputResult<object>(funcConfigList);
        }

        public OutputResult<object> UpdateFuncConfig(FuncConfigData data,int userNumber)
        {
            using (var conn= GetDbConnect())
            {
                conn.Open();
                DbTransaction tran = conn.BeginTransaction();
                List<FuncEvent> funcEvents = new List<FuncEvent>();
                List<ActionExtConfig> acConfigs = new List<ActionExtConfig>();
                try
                {
                    foreach (var item in data.funcEvent.Keys)
                    {
                        funcEvents.AddRange(data.funcEvent[item]);
                    }
                    funcEvents = funcEvents.Where(r => !string.IsNullOrEmpty(r.FuncName)).ToList();
                    acConfigs = data.acConfig.Where(r => r.ImplementType != -1).ToList();
                    _entityProRepository.UpdateFuncEvent(tran, data.entityId, funcEvents, userNumber);
                    _entityProRepository.UpdateActionExt(tran, data.entityId, acConfigs, userNumber);
                    _entityProRepository.UpdateExtFunction(tran, data.entityId, data.extFunction, userNumber);
                    tran.Commit();
                    return new OutputResult<object>("修改成功");
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return new OutputResult<object>(null, "修改失败！", 1);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }


        public OutputResult<object> GetUCodeList(UCodeModel model, int userId)
        {
            var mapper = _mapper.Map<UCodeModel, UCodeMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _entityProRepository.GetUCodeList(mapper, transaction, userId);
                return new OutputResult<object>(data);
            }, model, userId);
        }

        public OutputResult<object> UpdateGlobalJsHistoryRemark(UCodeModel model, int userId)
        {
            var mapper = _mapper.Map<UCodeModel, UCodeMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _entityProRepository.UpdateGlobalJsHistoryRemark(mapper, transaction, userId);
                return new OutputResult<object>(data);
            }, model, userId);
        }
        public OutputResult<object> UpdatePgHistoryLogRemark(PgCodeModel model, int userId)
        {
            var mapper = _mapper.Map<PgCodeModel, PgCodeMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _entityProRepository.UpdatePgHistoryLogRemark(mapper, transaction, userId);
                return new OutputResult<object>(data);
            }, model, userId);
        }
        public OutputResult<object> GetUCodeDetail(UCodeModel model, int userId)
        {
            var mapper = _mapper.Map<UCodeModel, UCodeMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _entityProRepository.GetUCodeDetail(mapper, transaction, userId);
                return new OutputResult<object>(data);
            }, model, userId);
        }

        public OutputResult<object> GetPgLogDetail(PgCodeModel model, int userId)
        {
            var mapper = _mapper.Map<PgCodeModel, PgCodeMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _entityProRepository.GetPgLogDetail(mapper, transaction, userId);
                return new OutputResult<object>(data);
            }, model, userId);
        }

        public OutputResult<object> GetPgLogList(PgCodeModel model, int userId)
        {
            var mapper = _mapper.Map<PgCodeModel, PgCodeMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _entityProRepository.GetPgLogList(mapper, transaction, userId);
                return new OutputResult<object>(data);
            }, model, userId);
        }
    }
}
