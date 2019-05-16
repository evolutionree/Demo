using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.ReportRelation;
using UBeat.Crm.CoreApi.DomainModel.StatisticsSetting;

namespace UBeat.Crm.CoreApi.Services.Models.ReportRelation
{
    public class ReportRelationProfile : Profile
    {
        public ReportRelationProfile()
        {

            CreateMap<AddReportRelationModel, AddReportRelationMapper>();
            CreateMap<EditReportRelationModel, EditReportRelationMapper>();
            CreateMap<DeleteReportRelationModel, DeleteReportRelationMapper>();
            CreateMap<QueryReportRelationModel, QueryReportRelationMapper>();
            CreateMap<QueryReportRelDetailModel, QueryReportRelDetailMapper>();
            CreateMap<AddReportRelDetailModel, AddReportRelDetailMapper>();
            CreateMap<EditReportRelDetailModel, EditReportRelDetailMapper>();
            CreateMap<DeleteReportRelDetailModel, DeleteReportRelDetailMapper>();

        }

    }
}
