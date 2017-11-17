using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;

namespace UBeat.Crm.CoreApi.Services.Models.DynamicEntity
{
    public class DynamicEntityProfile : Profile
    {
        public DynamicEntityProfile()
        {
            CreateMap<DynamicEntityAddModel, DynamicEntityAddMapper>();
            CreateMap<DynamicEntityEditModel, DynamicEntityEditMapper>();
            CreateMap<DynamicEntityListModel, DynamicEntityListMapper>();
            CreateMap<DynamicEntityDetailModel, DynamicEntityDetailtMapper>();
            CreateMap<DynamicPluginVisibleModel, DynamicPluginVisibleMapper>();
            CreateMap<DynamicEntityTransferModel, DynamicEntityTransferMapper>();
            CreateMap<DataSrcDeleteRelationModel, DataSrcDeleteRelationMapper>();
             CreateMap<DynamicEntityAddConnectModel, DynamicEntityAddConnectMapper>();
            CreateMap<DynamicEntityEditConnectModel, DynamicEntityEditConnectMapper>();
            CreateMap<DynamicEntityConnectListModel, DynamicEntityConnectListMapper>();
            CreateMap<DynamicPageVisibleModel, DynamicPageVisibleMapper>();
            CreateMap<PermissionModel, PermissionMapper>();

            CreateMap<RelTabListModel, RelTabListMapper>();
            CreateMap<RelTabInfoModel, RelTabInfoMapper>();
            CreateMap<AddRelTabModel, AddRelTabMapper>();
            CreateMap<UpdateRelTabModel, UpdateRelTabMapper>();
            CreateMap<DisabledRelTabModel, DisabledRelTabMapper>();
            CreateMap<AddRelTabRelationDataSrcModel, AddRelTabRelationDataSrcMapper>();
        }
    }
}
