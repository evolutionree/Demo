using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.ScheduleTask;
using UBeat.Crm.CoreApi.Services.Models.ScheduleTask;

namespace UBeat.Crm.CoreApi.Services.Models.SalesStage
{
    public class ScheduleTaskProfile : Profile
    {
        public ScheduleTaskProfile()
        {
            CreateMap<ScheduleTaskListModel, ScheduleTaskListMapper>();
            CreateMap<UnConfirmListModel, UnConfirmListMapper>();
        }
    }
}
