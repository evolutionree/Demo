using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.SalesStage;

namespace UBeat.Crm.CoreApi.Services.Models.SalesStage
{
    public class SalesStageProfile : Profile
    {
        public SalesStageProfile()
        {
            CreateMap<SalesstageTypeModel, SalesstageTypeMapper>();
            CreateMap<SaveSalesStageModel, SaveSalesStageMapper>();
            CreateMap<DisabledSalesStageModel, DisabledSalesStageMapper>();
            CreateMap<OrderBySalesStageModel, OrderBySalesStageMapper>();



            CreateMap<SalesStageSetLstModel, SalesStageSetLstMapper>();
            CreateMap<AddSalesStageEventSetModel, AddSalesStageEventSetMapper>();
            CreateMap<UpdateSalesStageEventSetModel, EditSalesStageEventSetMapper>();
            CreateMap<DisabledSalesStageEventSetModel, DisabledSalesStageEventSetMapper>();


            //CreateMap<SalesStageOppInfoSetLstModel, SalesStageOppInfoSetLstMapper>();
            CreateMap<SalesStageOppInfoFieldsModel, SalesStageOppInfoFieldsMapper>();
            CreateMap<SaveSalesStageOppInfoSetModel, SaveSalesStageOppInfoSetMapper>();

            CreateMap<SalesStageStepInfoModel, SalesStageStepInfoMapper>();
            CreateMap<EventSetModel, EventSetMapper>();
            CreateMap<SaveSalesStageStepInfoModel, SaveSalesStageStepInfoMapper>();
             CreateMap<UpdateOpportunityStatusModel, UpdateOpportunityStatusMapper>();
            CreateMap<OppInfoSetModel, OppInfoSetMapper>();
            CreateMap<SaveDynEntityModel, SaveDynEntityMapper>();
            CreateMap<PushSalesStageStepInfoModel, PushSalesStageStepInfoMapper>();
            CreateMap<ReturnSalesStageStepInfoModel, ReturnSalesStageStepInfoMapper>();
            CreateMap<SalesStageRestartModel, SalesStageRestartMapper>();
            CreateMap<WinOrderModel, WinOrderMapper>();
            CreateMap<LoseOrderModel, LoseOrderMapper>();
            CreateMap<OrderInfoModel, OrderInfoMapper>();


            CreateMap<AddSalesStageDynEntitySetModel, AddSalesStageDynEntitySetMapper>();

            CreateMap<DelSalesStageDynEntitySetModel, DelSalesStageDynEntitySetMapper>();
            CreateMap<OpenHighSettingModel, OpenHighSettingMapper>();
            CreateMap<OpenHighSettingMapper, OpenHighSettingModel>();
        }
    }
}
