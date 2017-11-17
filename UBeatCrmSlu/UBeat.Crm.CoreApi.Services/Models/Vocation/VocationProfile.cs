using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Vocation;

namespace UBeat.Crm.CoreApi.Services.Models.Vocation
{
   public class VocationProfile: Profile
    {
        public VocationProfile()
        {
            CreateMap<CopyVocationSaveModel, CopyVocationAdd>();
        }
    }
}
