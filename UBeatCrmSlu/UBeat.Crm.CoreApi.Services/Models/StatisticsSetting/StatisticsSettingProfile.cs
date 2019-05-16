using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.StatisticsSetting;

namespace UBeat.Crm.CoreApi.Services.Models.StatisticsSetting
{
    public class StatisticsSettingProfile : Profile
    {
        public StatisticsSettingProfile()
        {
            CreateMap<AddStatisticsSettingModel, AddStatisticsSettingMapper>();
            CreateMap<EditStatisticsSettingModel, EditStatisticsSettingMapper>();
            CreateMap<DeleteStatisticsSettingModel, DeleteStatisticsSettingMapper>();
            CreateMap<QueryStatisticsSettingModel, QueryStatisticsSettingMapper>();
            CreateMap<QueryStatisticsModel, QueryStatisticsMapper>();
            CreateMap<EditStatisticsGroupModel, EditStatisticsGroupMapper>();
            CreateMap<SaveStatisticsGroupModel, SaveStatisticsGroupMapper>();
            CreateMap<SaveStatisticsGroupSumModel, SaveStatisticsGroupSumMapper>();
        }
    }

}
