using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.SalesLead;

namespace UBeat.Crm.CoreApi.Services.Models.SalesLead
{
    public class SalesLeadProfile : Profile
    {
        public SalesLeadProfile()
        {
            CreateMap<SalesLeadModel, SalesLeadMapper>();
        }
    }
}
