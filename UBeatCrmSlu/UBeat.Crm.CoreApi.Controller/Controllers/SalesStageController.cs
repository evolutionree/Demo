using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.SalesStage;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class SalesStageController : BaseController
    {
        private readonly SalesStageServices _salesStageServices;
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly WorkFlowServices _workFlowServices;
        public SalesStageController(SalesStageServices salesStageServices, 
            DynamicEntityServices dynamicEntityServices,
            WorkFlowServices workFlowServices) : base(dynamicEntityServices)
        {
            _salesStageServices = salesStageServices;
            _dynamicEntityServices = dynamicEntityServices;
            _workFlowServices = workFlowServices;
        }
        #region 销售阶段后台配置
        [HttpPost]
        [Route("querysalesstage")]
        public OutputResult<object> SalesStageQuery([FromBody]SalesstageTypeModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.SalesStageQuery(entityModel, UserId);
        }

        [HttpPost]
        [Route("insertsalesstage")]
        public OutputResult<object> InsertEntityPro([FromBody]SaveSalesStageModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.InsertSalesStage(entityModel, UserId);
        }

        [HttpPost]
        [Route("updatesalesstage")]
        public OutputResult<object> UpdateEntityPro([FromBody]SaveSalesStageModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.UpdateSalesStage(entityModel, UserId);
        }

        [HttpPost]
        [Route("disabledsalesstage")]
        public OutputResult<object> DisabledSalesStage([FromBody]DisabledSalesStageModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.DisabledSalesStage(entityModel, UserId);
        }


        [HttpPost]
        [Route("orderbysalesstage")]
        public OutputResult<object> OrderBySalesStage([FromBody]OrderBySalesStageModel entityModels = null)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.OrderBySalesStage(entityModels, UserId);
        }
        [HttpPost]
        [Route("openhighsetting")]
        public OutputResult<object> OpentHighSetting([FromBody]OpenHighSettingModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.OpentHighSetting(entityModel, UserId);
        }


        [HttpPost]
        [Route("querysalesstageset")]
        public OutputResult<object> SalesStageSettingQuery([FromBody]SalesStageSetLstModel entityModels = null)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.SalesStageSettingQuery(entityModels, UserId);
        }

        [HttpPost]
        [Route("querysalesstagerelentity")]
        public OutputResult<object> SalesStageRelEntityQuery()
        {
            return _salesStageServices.SalesStageRelEntityQuery(UserId);
        }

        [HttpPost]
        [Route("insertsalesstageeventset")]
        public OutputResult<object> InsertSalesStageEventSetting([FromBody]AddSalesStageEventSetModel entityModels = null)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.InsertSalesStageEventSetting(entityModels, UserId);
        }
        [HttpPost]
        [Route("updatesalesstageeventset")]
        public OutputResult<object> UpdateSalesStageEventSetting([FromBody]UpdateSalesStageEventSetModel entityModels = null)
        {
            if (entityModels == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.UpdateSalesStageEventSetting(entityModels, UserId);
        }
        [HttpPost]
        [Route("disabledsalesstageeventset")]
        public OutputResult<object> DisabledSalesStageEventSetting([FromBody]DisabledSalesStageEventSetModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.DisabledSalesStageEventSetting(entityModel, UserId);
        }

        [HttpPost]
        [Route("querysalesstageinfofields")]
        public OutputResult<object> SalesStageInfoFieldsQuery([FromBody]SalesStageOppInfoFieldsModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.SalesStageInfoFieldsQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("savesalesstageoppinfosetting")]
        public OutputResult<object> SaveSalesStageOppInfoSetting([FromBody]SaveSalesStageOppInfoSetModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.SaveSalesStageOppInfoSetting(entityModel, UserId);
        }

        [HttpPost]
        [Route("addsalesstagedyentitysetting")]
        public OutputResult<object> InsertSalesStageDynEntitySetting([FromBody]AddSalesStageDynEntitySetModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.InsertSalesStageDynEntitySetting(entityModel, UserId);
        }

        [HttpPost]
        [Route("delsalesstagedyentitysetting")]
        public OutputResult<object> DeleteSalesStageDynEntitySetting([FromBody]DelSalesStageDynEntitySetModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.DeleteSalesStageDynEntitySetting(entityModel, UserId);
        }
        #endregion






        [HttpPost]
        [Route("querysalesstagestepinfo")]
        public OutputResult<object> SalesStageStepInfoQuery([FromBody]SalesStageStepInfoModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.SalesStageStepInfoQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("updateopporstatus")]
        public OutputResult<object> UpdateOpportunityStatus([FromBody]UpdateOpportunityStatusModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.UpdateOpportunityStatus(entityModel, UserId);
        }
        [HttpPost]
        [Route("checkallowpushsalesstage")]
        public OutputResult<object> CheckAllowPushSalesStage([FromBody]SaveSalesStageStepInfoModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.CheckAllowPushSalesStage(entityModel, UserId);
        }

        [HttpPost]
        [Route("checkallowreturnsalesstage")]
        public OutputResult<object> CheckAllowReturnSalesStage([FromBody]ReturnSalesStageStepInfoModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            
            return _salesStageServices.CheckAllowReturnSalesStage(entityModel, UserId);
        }


        [HttpPost]
        [Route("savesalesstageevent")]
        public OutputResult<object> SaveSalesStageEvent([FromBody]SaveSalesStageStepInfoModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.SaveSalesStageEvent(entityModel, UserId);
        }
        /// <summary>
        /// 保存阶段信息
        /// </summary>
        /// <param name="entityModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("savesalesstageinfo")]
        public OutputResult<object> SaveSalesStageInfo([FromBody]SaveSalesStageStepInfoModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            OutputResult<object> result = new OutputResult<object>();
            var header = GetAnalyseHeader();
            //检查是否在流程内
            if (entityModel.DynEntity != null && entityModel.DynEntity.TypeId != Guid.Empty)
            {
                string dynRecId = _salesStageServices.ReturnSalesStageDynentityId(entityModel.RecId, entityModel.SalesStageId, UserId);
                if (!string.IsNullOrEmpty(dynRecId))
                {
                    List<WorkFlowCaseInfo> workflows = _workFlowServices.getWorkFlowCaseListByRecId(dynRecId, UserId, null);
                    if (workflows != null && workflows.Count > 0)
                    {
                        foreach (WorkFlowCaseInfo info in workflows)
                        {
                            if (info.AuditStatus == AuditStatusType.Approving)
                            {
                                return new OutputResult<object>("单据正在流程中，不能修改", "单据正在流程中，不能修改", -1);
                            }
                        }
                    }
                }
                    
            }
                
            if (entityModel.Event != null && entityModel.Event.Count > 0)
                result = _salesStageServices.SaveSalesStageInfo(entityModel, UserId);
            if (result.Status == 0)
            {
                if (entityModel.Info != null && entityModel.Info.TypeId != Guid.Empty)
                {
                    entityModel.Info.RecId = Guid.Parse(entityModel.RecId);
                    result = _salesStageServices.SaveSalesStageInfo(entityModel.Info, UserId);
                }
                if (entityModel.DynEntity != null && entityModel.DynEntity.TypeId != Guid.Empty)
                {
                    string dynRecId = _salesStageServices.ReturnSalesStageDynentityId(entityModel.RecId, entityModel.SalesStageId, UserId);
                    if (string.IsNullOrEmpty(dynRecId))
                    {
                        result = _dynamicEntityServices.Add(entityModel.DynEntity, header, UserId);
                        if (result.Status == 1)
                        {
                            return result;
                        }
                        SaveDynEntityModel model = new SaveDynEntityModel
                        {
                            RecId = entityModel.RecId,
                            DynRecId =result.DataBody.ToString(),
                            SalesStageId = entityModel.SalesStageId
                        };
                        result = _salesStageServices.SaveSalesStageDynEntity(model, UserId);
                    }
                    else
                    {
                        DynamicEntityEditModel dynEntity = new DynamicEntityEditModel
                        {
                            RecId = Guid.Parse(dynRecId),
                            TypeId = entityModel.DynEntity.TypeId,
                            FieldData = entityModel.DynEntity.FieldData
                        };
                        
                        result = _dynamicEntityServices.Edit(dynEntity, header, UserId);
                    }
                }
            }
            if (entityModel.SalesStageFlag == 1)
            {
                result = _salesStageServices.CheckAllowPushSalesStage(entityModel, UserId);
            }
            if (result.DataBody == null && result.Status == 0 && string.IsNullOrEmpty(result.Message))
                result = new OutputResult<object>("保存销售阶段信息成功");
            return result;
        }

        [HttpPost]
        [Route("savesalesstagedynentity")]
        public OutputResult<object> SaveSalesStageDynEntity([FromBody]SaveSalesStageStepInfoModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _salesStageServices.SaveSalesStageInfo(entityModel, UserId);
        }

        [HttpPost]
        [Route("salesstagerestart")]
        public OutputResult<object> SalesStageRestart([FromBody]SalesStageRestartModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _salesStageServices.SalesStageRestart(entityModel, UserId);
        }
    }
}
