using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.EntityPro;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using Microsoft.AspNetCore.Authorization;
using UBeat.Crm.CoreApi.Services.Models.Excels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class EntityProController : BaseController
    {

        private readonly EntityProServices _entityProService;
        private readonly WebMenuServices _webMenuService;
        private readonly EntityExcelImportServices _entityExcelImportServices;


        public EntityProController(EntityProServices entityProService, WebMenuServices webMenuService, EntityExcelImportServices entityExcelImportServices) : base(entityProService)
        {
            _entityProService = entityProService;
            _webMenuService = webMenuService;
            _entityExcelImportServices = entityExcelImportServices;
        }

        #region 协议
        [HttpPost]
        [Route("queryentitypro")]
        public OutputResult<object> EntityProQuery([FromBody]EntityProQueryModel entityQuery = null)
        {
            if (entityQuery == null) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityProQuery(entityQuery, UserId);
        }
        [HttpPost]
        [Route("queryentityproinfo")]
        public OutputResult<object> EntityProInfoQuery([FromBody]EntityProInfoModel entityQuery = null)
        {
            if (entityQuery == null) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityProInfoQuery(entityQuery, UserId);
        }
        [HttpPost]
        [Route("insertentitypro")]
        public OutputResult<object> InsertEntityPro([FromBody]EntityProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            foreach (var item in entityModel.EntityName_Lang.Values)
            {
                if (string.IsNullOrEmpty(item))
                    return ResponseError<object>("请完善多语言信息");
            }
            return _entityProService.InsertEntityPro(entityModel, UserId);
        }

        [HttpPost]
        [Route("updateentitypro")]
        public OutputResult<object> UpdateEntityPro([FromBody]EntityProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            foreach (var item in entityModel.EntityName_Lang.Values)
            {
                if (string.IsNullOrEmpty(item))
                    return ResponseError<object>("请完善多语言信息");
            }
            return _entityProService.UpdateEntityPro(entityModel, UserId);
        }

        [HttpPost]
        [Route("disabledentitypro")]
        public OutputResult<object> DisabledEntityPro([FromBody]EntityProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.DisabledEntityPro(entityModel, UserId);
        }
        [HttpPost]
        [Route("deleteentitydata")]
        public OutputResult<object> DeleteEntityData([FromBody]DeleteEntityDataModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.DeleteEntityData(entityModel, UserId);
        }

        [HttpPost]
        [Route("saveentityglobaljs")]
        public OutputResult<object> SaveEntityGlobalJs([FromBody]EntityGlobalJsModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.SaveEntityGlobalJs(entityModel, UserId);
        }
        [HttpPost]
        [Route("savenestedtablesEntity")]
        public OutputResult<object> SaveNestedTablesEntity([FromBody] NestedTablesModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.SaveNestedTablesEntity(entityModel, UserId);
        }
        [HttpPost]
        [Route("entityclassquery")]
        public OutputResult<object> EntityClassQuery([FromBody]EntityProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityClassQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("entityorderbyquery")]
        public OutputResult<object> EntityOrderbyQuery([FromBody]EntityOrderbyModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityOrderbyQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("orderbyentitypro")]
        public OutputResult<object> OrderByEntityPro([FromBody]OrderByEntityProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.OrderByEntityPro(entityModel, UserId);
        }

        #endregion

        #region 字段
        [HttpPost]
        [Route("queryentityfield")]
        public OutputResult<object> EntityFieldProQuery([FromBody]EntityFieldProModel entityModel)
        {
            if (string.IsNullOrEmpty(entityModel.EntityId)) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityFieldProQuery(entityModel.EntityId, UserId);
        }
        [HttpPost]
        [Route("insertentityfield")]
        public OutputResult<object> InsertEntityField([FromBody]EntityFieldProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            if (entityModel.DisplayName_Lang == null || entityModel.FieldLabel_Lang == null) ResponseError<object>("请完善多语言");
            return _entityProService.InsertEntityField(entityModel, UserId);
        }
        [HttpPost]
        [Route("updateentityfield")]
        public OutputResult<object> UpdateEntityField([FromBody]EntityFieldProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            if (entityModel.DisplayName_Lang == null || entityModel.FieldLabel_Lang == null) ResponseError<object>("请完善多语言");
            return _entityProService.UpdateEntityField(entityModel, UserId);
        }
        [HttpPost]
        [Route("updateentityfieldexpandjs")]
        public OutputResult<object> UpdateEntityFieldExpandJS([FromBody]EntityFieldExpandJSModel fieldExpandJS = null)
        {
            if (fieldExpandJS == null) return ResponseError<object>("参数格式错误");

            return _entityProService.UpdateEntityFieldExpandJS(fieldExpandJS, UserId);
        }

        [HttpPost]
        [Route("updateentityfieldfilterjs")]
        public OutputResult<object> UpdateEntityFieldFilterJS([FromBody]EntityFieldFilterJSModel fieldfilterJS = null)
        {
            if (fieldfilterJS == null) return ResponseError<object>("参数格式错误");

            return _entityProService.UpdateEntityFieldFilterJS(fieldfilterJS, UserId);
        }
        [HttpPost]
        [Route("updateentityfieldname")]
        public OutputResult<object> UpdateFieldDisplayName([FromBody]EntityFieldUpdateDisplayNameParamInfo paramInfo = null)
        {
            if (paramInfo == null || paramInfo.FieldId == Guid.Empty)
            {
                return ResponseError<object>("参数异常");
            }
            else if (paramInfo.DisplayName_Lang == null || paramInfo.DisplayName_Lang.ContainsKey("cn") == false
               || paramInfo.DisplayName_Lang["cn"] == null
               || paramInfo.DisplayName_Lang["cn"].Length == 0)
            {
                return ResponseError<object>("字段显示名称不能为空");
            }
            try
            {
                this._entityProService.UpdateFieldName(paramInfo.FieldId, paramInfo.DisplayName_Lang, UserId);
                return new OutputResult<object>("成功");
            }
            catch (Exception ex)
            {
                return ResponseError<object>("更新异常:" + ex.Message);
            }
        }

        [HttpPost]
        [Route("disabledentityfield")]
        public OutputResult<object> DisabledEntityFieldPro([FromBody]EntityFieldProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.DisabledEntityFieldPro(entityModel, UserId);
        }
        [HttpPost]
        [Route("deleteentityfield")]
        public OutputResult<object> DeleteEntityFieldPro([FromBody]EntityFieldProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.DeleteEntityFieldPro(entityModel, UserId);
        }
        [HttpPost]
        [Route("orderbyentityfield")]
        public OutputResult<object> OrderByEntityFieldPro([FromBody]ICollection<EntityFieldProModel> entityModels = null)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");

            return _entityProService.OrderByEntityFieldPro(entityModels, UserId);
        }
        #endregion


        #region 类型
        [HttpPost]
        [Route("queryentitytype")]
        public OutputResult<object> EntityTypeQuery([FromBody]EntityTypeQueryModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityTypeQuery(entityModel, UserId);
        }

        [HttpPost]
        [Route("insertentitytype")]
        public OutputResult<object> InsertEntityTypePro([FromBody]EntityTypeModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.InsertEntityTypePro(entityModel, UserId);
        }

        [HttpPost]
        [Route("updateentitytype")]
        public OutputResult<object> UpdateEntityTypePro([FromBody]EntityTypeModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.UpdateEntityTypePro(entityModel, UserId);
        }

        [HttpPost]
        [Route("disabledentitytypepro")]
        public OutputResult<object> DisabledEntityTypePro([FromBody]EntityTypeModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.DisabledEntityTypePro(entityModel, UserId);
        }



        [HttpPost]
        [Route("orderbyentitytype")]
        public OutputResult<object> OrderByEntityType([FromBody]ICollection<EntityTypeModel> entityModels = null)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");

            return _entityProService.OrderByEntityType(entityModels, UserId);
        }
        #endregion


        #region 规则
        [HttpPost]
        [Route("queryentityfieldrules")]
        public OutputResult<object> EntityFieldRulesQuery([FromBody]FileldRulesQueryModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityFieldRulesQuery(entityModel.EntityId, entityModel.TypeId, UserId);
        }
        [HttpPost]
        [Route("saveentityfieldrules")]
        public OutputResult<object> SaveEntityFieldRules([FromBody]ICollection<EntityFieldRulesSaveModel> entityModels = null)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");

            return _entityProService.SaveEntityFieldRules(entityModels, UserId);
        }

        [HttpPost]
        [Route("queryentityfieldrulesvo")]
        public OutputResult<object> EntityFieldRulesVocationQuery([FromBody]FileldRulesVocationQueryModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityFieldRulesVocationQuery(entityModel.EntityId, entityModel.VocationId, UserId);
        }
        [HttpPost]
        [Route("saveentityfieldrulesvo")]
        public OutputResult<object> SaveEntityFieldRulesVocation([FromBody]EntityFieldRulesVocationSaveModelList entityModels)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");

            return _entityProService.SaveEntityFieldRulesVocation(entityModels.EntityId, entityModels.FieldRules, UserId);
        }
        #endregion


        #region  搜索

        [HttpPost]
        [Route("fieldsfilterlist")]
        public OutputResult<object> EntityFieldFilterQuery([FromBody]EntityProModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityFieldFilterQuery(entityModel.EntityId, UserId);
        }

        [HttpPost]
        [Route("updatefieldsfilter")]
        public OutputResult<object> UpdateEntityFieldFilter([FromBody]SimpleSearchModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.UpdateEntityFieldFilter(entityModel, UserId);
        }
        #endregion


        #region 设置手机 web字段是否可见
        [HttpPost]
        [Route("querywebfieldvisible")]
        public OutputResult<object> FieldWebVisibleQuery([FromBody]ListViewColumnModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.FieldWebVisibleQuery(entityModel.EntityId, UserId);
        }


        [HttpPost]
        [Route("savewebfieldvisble")]
        public OutputResult<object> SaveWebFieldVisible([FromBody]SaveListViewColumnModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.SaveWebFieldVisible(entityModel, UserId);
        }

        [HttpPost]
        [Route("querymobfieldvisible")]
        public OutputResult<object> FieldMOBVisibleQuery([FromBody]ListViewColumnModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.FieldMOBVisibleQuery(entityModel.EntityId, UserId);
        }

        [HttpPost]
        [Route("insertmobfieldvisble")]
        public OutputResult<object> InsertMOBFieldVisible([FromBody]ListViewModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.InsertMOBFieldVisible(entityModel, UserId);
        }

        [HttpPost]
        [Route("updatemobfieldvisble")]
        public OutputResult<object> UpdateMOBFieldVisible([FromBody]ListViewModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.UpdateMOBFieldVisible(entityModel, UserId);
        }
        #endregion

        #region 设置顶部显示字段

        [HttpPost]
        [Route("querypageconfiginfo")]
        public OutputResult<object> EntityPageConfigInfoQuery([FromBody]EntityPageConfigModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.EntityPageConfigInfoQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("savepageconfig")]
        public OutputResult<object> SaveEntityPageConfig([FromBody]EntityPageConfigModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _entityProService.SaveEntityPageConfig(entityModel, UserId);
        }

        #endregion

        #region 设置查重
        [HttpPost]
        [Route("queryrepeatlist")]
        public OutputResult<object> SetRepeatList([FromBody]SetRepeatModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.SetRepeatList(entityModel, UserId);
        }
        [HttpPost]
        [Route("savesetrepeat")]
        public OutputResult<object> SaveSetRepeat([FromBody]SetRepeatModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.SaveSetRepeat(entityModel, UserId);
        }
        #endregion

        #region  入口分组     
        [HttpPost]
        [Route("queryentrancegroup")]
        public OutputResult<object> EntranceListQuery()
        {

            return _entityProService.EntranceListQuery(UserId);
        }
        [HttpPost]
        [Route("saveentrancegroup")]
        public OutputResult<object> SaveEntanceGroup([FromBody]ICollection<SaveEntranceGroupModel> entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            OutputResult<object> ret = _entityProService.SaveEntanceGroup(entityModel, UserId);
            _webMenuService.synchCRMAndOfficeMenus();
            return ret;
        }
        #endregion

        #region 查询关联控件值
        [HttpPost]
        [Route("queryrelcontrolval")]
        public OutputResult<object> RelContorlValueQuery([FromBody]RelControlValueModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.RelContorlValueQuery(entityModel, UserId);
        }
        #endregion

        #region 保存个人设置
        [HttpPost]
        [Route("savepersonal")]
        public OutputResult<object> SavePersonalViewSet([FromBody]ICollection<PersonalViewSetModel> entityModels = null)
        {
            if (entityModels == null || entityModels.Count == 0) return ResponseError<object>("参数格式错误");
            return _entityProService.SavePersonalViewSet(entityModels.ToList(), UserId);
        }
        [HttpPost]
        [Route("querypersonal")]
        public OutputResult<object> PersonalSettingQuery([FromBody]PersonalSettingModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            if (Guid.Empty == entityModel.EntityId) return ResponseError<object>("实体Id不能为空");
            return _entityProService.PersonalSettingQuery(entityModel, UserId);
        }
        #endregion

        #region 保存基础资料

        [HttpPost]
        [Route("saveentitybasedata")]
        public OutputResult<object> EntityBaseDataSave([FromBody]ICollection<EntityBaseDataModel> entityModels = null)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");
            if (entityModels.Count == 0) return ResponseError<object>("参数集合不能为空");
            return _entityProService.SaveEntityBaseData(entityModels, UserId);
        }

        [HttpPost]
        [Route("querybasefield")]
        public OutputResult<object> QueryEntityBaseDataField([FromBody]EntityBaseDataFieldModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.EntityBaseDataFieldQuery(entityModel, UserId);
        }
        #endregion


        #region --功能按钮--
        /// <summary>
        /// 获取功能按钮列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("functionbtnlist")]
        public OutputResult<object> GetFunctionBtnList([FromBody] FunctionBtnListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.GetFunctionBtnList(dynamicModel, UserId);
        }

        /// <summary>
        /// 新增功能按钮列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("addfunctionbtn")]
        public OutputResult<object> AddFunctionBtn([FromBody] FunctionBtnDetailModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            if (dynamicModel.Title_Lang == null) return ResponseError<object>("请完善多语言");
            return _entityProService.AddFunctionBtn(dynamicModel, UserId);
        }

        /// <summary>
        /// 编辑功能按钮列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("editfunctionbtn")]
        public OutputResult<object> EditFunctionBtn([FromBody] FunctionBtnDetailModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            if (dynamicModel.Title_Lang == null) return ResponseError<object>("请完善多语言");
            return _entityProService.EditFunctionBtn(dynamicModel, UserId);
        }

        /// <summary>
        /// 删除功能按钮列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("deletefunctionbtn")]
        public OutputResult<object> DeleteFunctionBtn([FromBody] DeleteFunctionBtnModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.DeleteFunctionBtn(dynamicModel, UserId);
        }

        /// <summary>
        /// 排序功能按钮列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sortfunctionbtn")]
        public OutputResult<object> SortFunctionBtn([FromBody] SortFunctionBtnModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.SortFunctionBtn(dynamicModel, UserId);
        }

        #endregion

        #region --function 列表--
        /// <summary>
        /// 获取功能列表
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("functionlist")]
        public OutputResult<object> GetFunctionList([FromBody] FunctionBtnListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.GetFunctionList(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("savefunctions")]
        public OutputResult<object> SaveFunctionList([FromBody] SaveFuncsModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.SaveFunctionList(dynamicModel, UserId);
        }
        //同步function到function表
        [HttpPost]
        [Route("syncfunctions")]
        public OutputResult<object> SyncFunctionList([FromBody] SyncFuncListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.SyncFunctionList(dynamicModel, UserId);
        }

        #endregion

        #region --特别页面--
        /// <summary>
        /// 获取页面入口信息
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getentrypages")]
        public OutputResult<object> GetEntryPages([FromBody] ServiceJsonModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.GetEntryPages(dynamicModel, UserId);
        }
        /// <summary>
        /// 保存页面入口信息
        /// </summary>
        /// <param name="dynamicModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("saveentrypages")]
        public OutputResult<object> SaveEntryPages([FromBody] ServiceJsonDetailModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.SaveEntryPagesInfo(dynamicModel, UserId);
        }

        #endregion


        #region 扩展配置
        /// <summary>
        /// 获取扩展配置信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("getfunctionconfig")]
        public OutputResult<object> GetFunctionConfig([FromBody] FuncConfig data)
        {
            if (data == null || data.EntityId == Guid.Empty)
                return ResponseError<object>("参数格式错误");
            return _entityProService.GetFunctionConfig(data, UserId);
        }

        /// <summary>
        /// 保存扩展配置
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("updatefuncconfig")]
        public OutputResult<object> UpdateFuncConfig([FromBody]FuncConfigData data)
        {
            if (data == null || data.entityId == Guid.Empty) return ResponseError<object>("参数格式错误");
            if (data.acConfig.Where(r => r.RoutePath == null).Count() > 0)
                return ResponseError<object>("RoutePath不能为空");
            return _entityProService.UpdateFuncConfig(data, UserId);
        }
        /// <summary>
        /// 获取已经配置数据源的实体
        /// </summary>
        /// <returns></returns>
        [HttpPost("querywithdatasource")]
        public OutputResult<object> QueryAllEntityWithDataSource()
        {
            List<Dictionary<string, object>> datas = this._entityProService.QueryEntityWithDataSource(UserId);
            return new OutputResult<object>(datas);
        }

        [HttpPost("getreffieldsbyfield")]
        [AllowAnonymous]
        public OutputResult<object> QueryAllEntityWithDataSource([FromBody] EntityFieldProModel paramInfo)
        {
            Dictionary<string, object> datas = this._entityProService.getRefFieldsByFieldId(paramInfo.FieldId, UserId);
            return new OutputResult<object>(datas);
        }

        [HttpPost("importentity")]
        public OutputResult<object> ImportEntityDefine([FromForm] ImportTemplateModel formData)
        {
            try
            {

                Dictionary<string, object> listEntity = _entityExcelImportServices.ImportEntityFromExcel(formData.Data.OpenReadStream());
                listEntity.Add("allmessage", this._entityExcelImportServices.GenerateTotalMessage(listEntity));
                return new OutputResult<object>(listEntity);
            }
            catch (Exception ex)
            {
                return ResponseError<object>(ex.Message);
            }
        }
        #endregion

        #region 实体输入方式
        [HttpPost("entityinputmethodquery")]
        public OutputResult<object> GetEntityInputMethod([FromBody] EntityProInfoModel paramInfo)
        {
            if (paramInfo == null || paramInfo.EntityId == null || paramInfo.EntityId == Guid.Empty.ToString())
            {
                return ResponseError<object>("参数异常");
            }
            try
            {
                return this._entityProService.GetEntityInputMethod(paramInfo, UserId);
            }
            catch (Exception ex)
            {
                return ResponseError<object>(ex.Message);
            }

        }
        [HttpPost("entityinputmethodsave")]
        public OutputResult<object> SaveEntityInputMethod([FromBody] EntityInputMethodParamInfo paramInfo)
        {
            if (paramInfo == null || paramInfo.EntityId == null || paramInfo.EntityId == Guid.Empty)
            {
                return ResponseError<object>("参数异常");
            }
            if (paramInfo.InputMethods == null) paramInfo.InputMethods = new List<DomainModel.EntityPro.EntityInputModeInfo>();
            return this._entityProService.SaveEntityInputMethod(paramInfo.EntityId, paramInfo.InputMethods, UserId);
        }
        #endregion


        [HttpPost]
        [Route("getucodelist")]
        public OutputResult<object> GetUCodeList([FromBody] UCodeModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.GetUCodeList(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("updateglobaljshistoryremark")]
        public OutputResult<object> UpdateGlobalJsHistoryRemark([FromBody] UCodeModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.UpdateGlobalJsHistoryRemark(dynamicModel, UserId);
        }
        [HttpPost]
        [Route("updategghistorylogremark")]
        public OutputResult<object> UpdatePgHistoryLogRemark([FromBody] PgCodeModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.UpdatePgHistoryLogRemark(dynamicModel, UserId);
        }
        [HttpPost]
        [Route("getucodedetail")]
        public OutputResult<object> GetUCodeDetail([FromBody] UCodeModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.GetUCodeDetail(dynamicModel, UserId);
        }

        [HttpPost]
        [Route("getpgcodedetail")]
        public OutputResult<object> GetPgLogDetail([FromBody] PgCodeModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.GetPgLogDetail(dynamicModel, UserId);
        }


        [HttpPost]
        [Route("getpgcodelist")]
        public OutputResult<object> GetPgLogList([FromBody] PgCodeModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _entityProService.GetPgLogList(dynamicModel, UserId);
        }
    }
}
