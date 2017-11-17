using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.Notice;
using UBeat.Crm.CoreApi.DomainModel.Notify;
using UBeat.Crm.CoreApi.Services.Models.Notice;

namespace UBeat.Crm.CoreApi.Services.Models.Notify
{
    public class NotifyProfile : Profile
    {
        public NotifyProfile()
        {
            CreateMap<NotifyFetchModel, NotifyFetchMessageMapper>();
        }
    }
}
