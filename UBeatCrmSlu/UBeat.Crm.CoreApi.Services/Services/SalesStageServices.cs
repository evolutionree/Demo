using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.SalesStage;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.DomainModel.WorkFlow;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.SalesStage;
using UBeat.Crm.CoreApi.Services.Utility;
using System.Linq;
namespace UBeat.Crm.CoreApi.Services.Services
{
    public class SalesStageServices : BasicBaseServices
    {
        private readonly ISalesStageRepository _salesStageRepository;
        private readonly IMapper _mapper;
        private IDynamicEntityRepository _dynamicEntityRepository;

        private IWorkFlowRepository _workFlowRepository;
        private Guid oppEntityId = Guid.Parse("2c63b681-1de9-41b7-9f98-4cf26fd37ef1");
        public SalesStageServices(IMapper mapper, ISalesStageRepository salesStageRepository,
            IDynamicEntityRepository dynamicEntityRepository,
            IWorkFlowRepository workFlowRepository)
        {
            _salesStageRepository = salesStageRepository;
            _mapper = mapper;
            _dynamicEntityRepository = dynamicEntityRepository;
            _workFlowRepository = workFlowRepository;
        }

        public OutputResult<object> SalesStageQuery(SalesstageTypeModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SalesstageTypeModel, SalesstageTypeMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            Dictionary<string, List<IDictionary<string, object>>> salestages = _salesStageRepository.SalesStageQuery(entity, userNumber);
            int highSetting = _salesStageRepository.GetHighSetting(entity.SalesstageTypeId, userNumber);
            Dictionary<string, object> retData = new Dictionary<string, object>();
            foreach (string key in salestages.Keys)
            {
                retData.Add(key, salestages[key]);
            }
            retData.Add("highSetting", highSetting);

            return new OutputResult<object>(retData);
        }
        public OutputResult<object> InsertSalesStage(SaveSalesStageModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SaveSalesStageModel, SaveSalesStageMapper>(entityModel);
            string StageName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(entity.StageName, entity.StageName_Lang, out StageName);
            if (StageName != null) entity.StageName = StageName;
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = HandleResult(_salesStageRepository.InsertSalesStage(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }
        public OutputResult<object> UpdateSalesStage(SaveSalesStageModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SaveSalesStageModel, SaveSalesStageMapper>(entityModel);
            string StageName = "";
            MultiLanguageUtils.GetDefaultLanguageValue(entity.StageName, entity.StageName_Lang, out StageName);
            if (StageName != null) entity.StageName = StageName;
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = HandleResult(_salesStageRepository.UpdateSalesStage(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }
        public OutputResult<object> DisabledSalesStage(DisabledSalesStageModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<DisabledSalesStageModel, DisabledSalesStageMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            //检查是否可以被删除
            //检查有无商机处于要被删除的阶段
            string operateName = "";
            if (entity.RecStatus == 1)
            {

            }
            else if (entity.RecStatus == 0)
            {
                operateName = "禁用";
            }
            else
            {
                operateName = "删除";
            }
            if (entity.RecStatus == 0 || entity.RecStatus == 2)
            {
                int totalCount = this._salesStageRepository.checkHasOppInStageID(entity.SalesStageId, userNumber, null);
                if (totalCount > 0)
                {
                    OperateResult r = new OperateResult();
                    r.Msg = string.Format("有{0}个商机处于当前阶段，无法{1}当前销售阶段", totalCount, operateName);
                    r.Codes = "-1";
                    return HandleResult(r);
                }
                //检查工作流
                totalCount = this._workFlowRepository.getWorkFlowCountByStageId(null, entity.SalesStageId, userNumber);
                if (totalCount > 0)
                {
                    OperateResult r = new OperateResult();
                    r.Msg = string.Format("有{0}个推进流程处于当前阶段，无法{1}当前销售阶段", totalCount, operateName);
                    r.Codes = "-1";
                    return HandleResult(r);
                }
                else if (totalCount < 0)
                {
                    OperateResult r = new OperateResult();
                    r.Msg = string.Format("有推进流程处于当前阶段，无法{0}当前销售阶段", operateName);
                    r.Codes = "-1";
                    return HandleResult(r);
                }
            }

            var res = HandleResult(_salesStageRepository.DisabledSalesStage(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }
        public OutputResult<object> OrderBySalesStage(OrderBySalesStageModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<OrderBySalesStageModel, OrderBySalesStageMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = HandleResult(_salesStageRepository.OrderBySalesStage(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }
        public OutputResult<object> OpentHighSetting(OpenHighSettingModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<OpenHighSettingModel, OpenHighSettingMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = HandleResult(_salesStageRepository.OpentHighSetting(entity, userNumber));
            //校验赢单和输单的的表单配置的合理性
            _salesStageRepository.CheckSaleStageDynamicFormSetting(entity.TypeId, userNumber, null);
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }
        public OutputResult<object> SalesStageSettingQuery(SalesStageSetLstModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SalesStageSetLstModel, SalesStageSetLstMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            return new OutputResult<object>(_salesStageRepository.SalesStageSettingQuery(entity, userNumber));
        }
        public OutputResult<object> SalesStageRelEntityQuery(int userNumber)
        {
            return new OutputResult<object>(_salesStageRepository.SalesStageRelEntityQuery(userNumber));
        }
        public OutputResult<object> InsertSalesStageEventSetting(AddSalesStageEventSetModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AddSalesStageEventSetModel, AddSalesStageEventSetMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = HandleResult(_salesStageRepository.InsertSalesStageEventSetting(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }

        public OutputResult<object> UpdateSalesStageEventSetting(UpdateSalesStageEventSetModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<UpdateSalesStageEventSetModel, EditSalesStageEventSetMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = HandleResult(_salesStageRepository.UpdateSalesStageEventSetting(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }

        public OutputResult<object> DisabledSalesStageEventSetting(DisabledSalesStageEventSetModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<DisabledSalesStageEventSetModel, DisabledSalesStageEventSetMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = HandleResult(_salesStageRepository.DisabledSalesStageEventSetting(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }

        public OutputResult<object> InsertSalesStageDynEntitySetting(AddSalesStageDynEntitySetModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AddSalesStageDynEntitySetModel, AddSalesStageDynEntitySetMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = HandleResult(_salesStageRepository.InsertSalesStageDynEntitySetting(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }
        public OutputResult<object> DeleteSalesStageDynEntitySetting(DelSalesStageDynEntitySetModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<DelSalesStageDynEntitySetModel, DelSalesStageDynEntitySetMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res = HandleResult(_salesStageRepository.DeleteSalesStageDynEntitySetting(entity, userNumber));
            IncreaseDataVersion(DataVersionType.EntityData);
            return res;
        }

        //public OutputResult<object> SalesStageOppInfoSettingQuery(SalesStageOppInfoSetLstModel entityModel, int userNumber)
        //{
        //    var entity = _mapper.Map<SalesStageOppInfoSetLstModel, SalesStageOppInfoSetLstMapper>(entityModel);
        //    if (entity == null || !entity.IsValid())
        //    {
        //        return HandleValid(entity);
        //    }
        //    var result = _salesStageRepository.SalesStageOppInfoSettingQuery(entity, userNumber);
        //    return new OutputResult<object>(result);
        //}
        public OutputResult<object> SalesStageInfoFieldsQuery(SalesStageOppInfoFieldsModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SalesStageOppInfoFieldsModel, SalesStageOppInfoFieldsMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var result = _salesStageRepository.SalesStageInfoFieldsQuery(entity, userNumber);
            return new OutputResult<object>(result);
        }
        public OutputResult<object> SaveSalesStageOppInfoSetting(SaveSalesStageOppInfoSetModel entityModel, int userNumber)
        {

            var entity = _mapper.Map<SaveSalesStageOppInfoSetModel, SaveSalesStageOppInfoSetMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _salesStageRepository.SaveSalesStageOppInfoSetting(entity, userNumber);
            IncreaseDataVersion(DataVersionType.EntityData);
            return new OutputResult<object>(result);
        }




        public OutputResult<object> SalesStageStepInfoQuery(SalesStageStepInfoModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SalesStageStepInfoModel, SalesStageStepInfoMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            Dictionary<string, List<IDictionary<string, object>>> result = _salesStageRepository.SalesStageStepInfoQuery(entity, userNumber);
            if (result != null && result.ContainsKey("EventSet") && result["EventSet"] != null)
            {
                List<IDictionary<string, object>> list = result["EventSet"];
                foreach (IDictionary<string, object> item in list)
                {
                    if (item.ContainsKey("fileid") && item["fileid"] != null)
                    {
                        string tmp = item["fileid"].ToString();
                        try
                        {
                            List<Dictionary<string, object>> rr = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(tmp);
                            if (rr != null)
                            {
                                item.Add("files", rr);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
            return new OutputResult<object>(result);
        }

        public OutputResult<object> UpdateOpportunityStatus(UpdateOpportunityStatusModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<UpdateOpportunityStatusModel, UpdateOpportunityStatusMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var result = _salesStageRepository.UpdateOpportunityStatus(entity, userNumber);
            return HandleResult(result);
        }
        public OutputResult<object> CheckAllowPushSalesStage(SaveSalesStageStepInfoModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SaveSalesStageStepInfoModel, SaveSalesStageStepInfoMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            if (entity.SalesStageFlag == 0)
            {
                var splits = entity.SalesStageIds.Split(",");
                String id = String.Empty;
                if (splits.Length == 1)
                    id = splits[splits.Length - 1];
                else
                    id = splits[splits.Length - 1 - 1];
                var salesStageFields = _salesStageRepository.SalesStageSettingQuery(new SalesStageSetLstMapper
                {
                    SalesStageId = id
                }, userNumber);

                if (salesStageFields != null && salesStageFields["SalesStageOppInfo"] != null)
                {
                    var infoFields = salesStageFields["SalesStageOppInfo"];
                    //获取该实体分类的字段
                    List<DynamicEntityDataFieldMapper> dynamicEntityDataFieldMappers = new List<DynamicEntityDataFieldMapper>();
                    var fields = _dynamicEntityRepository.GetTypeFields(Guid.Parse(entity.TypeId), (int)DynamicProtocolOperateType.Edit, userNumber);
                    var detail = _dynamicEntityRepository.Detail(new DynamicEntityDetailtMapper
                    {
                        EntityId = Guid.Parse(entityModel.TypeId),
                        RecId = Guid.Parse(entityModel.RecId),
                        NeedPower = 0
                    }, userNumber);
                    var dic = new Dictionary<String, Object>();
                    foreach (var tmp in fields)
                    {
                        var tmp1 = infoFields.FirstOrDefault(t => t["fieldid"].ToString() == tmp.FieldId.ToString());
                        if (tmp1 != null)
                        {
                            dynamicEntityDataFieldMappers.Add(tmp);
                            if (detail.Keys.Contains(tmp.FieldName))
                            {
                                dic.Add(tmp.FieldName, detail[tmp.FieldName]);
                            }
                        }
                    }

                    if (fields.Count == 0)
                    {
                        return ShowError<object>("该实体分类没有配置相应字段");
                    }

                    var isMobile = header.Device.ToLower().Contains("android")
                          || header.Device.ToLower().Contains("ios");
                    foreach (var tmp in dynamicEntityDataFieldMappers)
                    {
                        tmp.IsRequire = true;
                    }
                    //验证字段
                    var validResults = DynamicProtocolHelper.ValidData(dynamicEntityDataFieldMappers, dic, DynamicProtocolOperateType.Edit, isMobile);
                    var validTips = new List<string>();
                    foreach (DynamicProtocolValidResult validResult in validResults.Values)
                    {
                        if (!validResult.IsValid)
                        {
                            validTips.Add(validResult.Tips);
                        }
                    }

                    if (validTips.Count > 0)
                    {
                        return ShowError<object>("阶段推进失败，请完善阶段信息，具体异常:【" + string.Join(";", validTips) + "】");
                    }
                }
            }
            //检查权限
            List<Guid> ids1 = new List<Guid>();

            ids1.Add(Guid.Parse(entity.RecId));
            if (this.GetUserData(userNumber, false).HasFunction("api/dynamicentity/salestagepush_notimple", oppEntityId, this.DeviceClassic) == false)
            {
                OperateResult r = new OperateResult();
                r.Msg = "您没权限推进该商机";
                r.Codes = "-1";
                return HandleResult(r);
            }
            if (this.GetUserData(userNumber, false).HasDataAccess(null, "api/dynamicentity/salestagepush_notimple", oppEntityId, this.DeviceClassic, ids1) == false)
            {
                OperateResult r = new OperateResult();
                r.Msg = "您没权限推进该商机";
                r.Codes = "-1";
                return HandleResult(r);
            }
            //这里要检查是否有工作流
            List<string> ids = _salesStageRepository.queryDynamicRecIdsFromOppId(entity.RecId, userNumber, null);
            if (ids != null && ids.Count > 0)
            {
                List<WorkFlowCaseInfo> workflows = _workFlowRepository.getWorkFlowCaseListByRecIds(null, ids, userNumber);
                foreach (WorkFlowCaseInfo item in workflows)
                {
                    if (item.AuditStatus == AuditStatusType.Approving)
                    {
                        OperateResult r = new OperateResult();
                        r.Codes = "-1";
                        r.Msg = "阶段推进流程审批中,不能推进";
                        return HandleResult(r);

                    }
                }
            }
            var result = _salesStageRepository.CheckAllowPushSalesStage(entity, userNumber);
            return HandleResult(result);
        }
        public OutputResult<object> CheckAllowReturnSalesStage(ReturnSalesStageStepInfoModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<ReturnSalesStageStepInfoModel, ReturnSalesStageStepInfoMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            //检查权限
            List<Guid> ids1 = new List<Guid>();

            ids1.Add(Guid.Parse(entity.RecId));
            if (this.GetUserData(userNumber, false).HasFunction("api/dynamicentity/salestagepush_notimple", oppEntityId, this.DeviceClassic) == false)
            {
                OperateResult r = new OperateResult();
                r.Msg = "您没权限回退该商机";
                r.Codes = "-1";
                return HandleResult(r);
            }
            if (this.GetUserData(userNumber, false).HasDataAccess(null, "api/dynamicentity/salestagepush_notimple", oppEntityId, this.DeviceClassic, ids1) == false)
            {
                OperateResult r = new OperateResult();
                r.Msg = "您没权限回退该商机";
                r.Codes = "-1";
                return HandleResult(r);
            }
            //这里要检查是否有工作流
            List<string> ids = _salesStageRepository.queryDynamicRecIdsFromOppId(entity.RecId, userNumber, null);
            if (ids != null && ids.Count > 0)
            {
                List<WorkFlowCaseInfo> workflows = _workFlowRepository.getWorkFlowCaseListByRecIds(null, ids, userNumber);
                foreach (WorkFlowCaseInfo item in workflows)
                {
                    if (item.AuditStatus == AuditStatusType.Approving)
                    {
                        OperateResult r = new OperateResult();
                        r.Codes = "-1";
                        r.Msg = "阶段推进流程审批中,不能退回";
                        return HandleResult(r);

                    }
                }
            }
            var result = _salesStageRepository.CheckAllowReturnSalesStage(entity, userNumber);
            return HandleResult(result);
        }
        public OutputResult<object> SaveSalesStageEvent(SaveSalesStageStepInfoModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SaveSalesStageStepInfoModel, SaveSalesStageStepInfoMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var result = _salesStageRepository.SaveSalesStageEvent(entity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> SaveSalesStageInfo(DynamicEntityEditModel dynamicModel, int userNumber)
        {
            var dynamicEntity = _mapper.Map<DynamicEntityEditModel, DynamicEntityEditMapper>(dynamicModel);
            if (dynamicEntity == null || !dynamicEntity.IsValid())
            {
                return HandleValid(dynamicEntity);
            }

            //验证通过后，插入数据
            var result = _dynamicEntityRepository.DynamicEdit(null, dynamicEntity.TypeId, dynamicEntity.RecId, dynamicEntity.FieldData, userNumber);

            return HandleResult(result);
        }
        public OutputResult<object> SaveSalesStageInfo(SaveSalesStageStepInfoModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SaveSalesStageStepInfoModel, SaveSalesStageStepInfoMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var result = _salesStageRepository.SaveSalesStageInfo(entity, userNumber);

            return HandleResult(result);
        }

        public OutputResult<object> SaveSalesStageDynEntity(SaveDynEntityModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SaveDynEntityModel, SaveDynEntityMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var result = _salesStageRepository.SaveSalesStageDynEntity(entity, userNumber);
            return HandleResult(result);
        }
        public string ReturnSalesStageDynentityId(string recId, string salesStageId, int userNumber)
        {

            var result = _salesStageRepository.ReturnSalesStageDynentityId(recId, salesStageId, userNumber);
            return result;
        }

        public OutputResult<object> SalesStageRestart(SalesStageRestartModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<SalesStageRestartModel, SalesStageRestartMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            //先检查权限
            List<Guid> ids = new List<Guid>();

            ids.Add(Guid.Parse(entity.RecId));
            if (this.GetUserData(userNumber, false).HasFunction("api/dynamicentity/salestagepush_notimple", oppEntityId, this.DeviceClassic) == false)
            {
                OperateResult r = new OperateResult();
                r.Msg = "您没权限重启该商机";
                r.Codes = "-1";
                return HandleResult(r);
            }
            if (this.GetUserData(userNumber, false).HasDataAccess(null, "api/dynamicentity/salestagepush_notimple", oppEntityId, this.DeviceClassic, ids) == false)
            {
                OperateResult r = new OperateResult();
                r.Msg = "您没权限重启该商机";
                r.Codes = "-1";
                return HandleResult(r);
            }
            var result = _salesStageRepository.SalesStageRestart(entity, userNumber);
            return HandleResult(result);
        }
        public string ReturnEntityId(string typeId, int userNumber)
        {
            var result = _salesStageRepository.ReturnEntityId(typeId, userNumber);
            return result;
        }

    }
}
