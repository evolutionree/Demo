using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Desktop
{
    public class DesktopProfile : Profile
    {

        public DesktopProfile()
        {
            CreateMap<Desktop, DesktopMapper>();
            CreateMap<DesktopComponent, DesktopComponentMapper>();
            CreateMap<DesktopRelation, DesktopRelationMapper>();
            CreateMap<DesktopRunTime, DesktopRunTimeMapper>();
            CreateMap<DesktopRoleRelation, DesktopRoleRelationMapper>();
        }
    }
}
