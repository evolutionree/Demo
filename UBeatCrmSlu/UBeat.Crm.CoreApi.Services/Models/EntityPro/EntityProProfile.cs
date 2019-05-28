using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;

namespace UBeat.Crm.CoreApi.Services.Models.EntityPro
{
    public class EntityProProfile : Profile
    {
        public EntityProProfile()
        {
            CreateMap<EntityProQueryModel, EntityProQueryMapper>();
            CreateMap<EntityProModel, EntityProMapper>();
            CreateMap<EntityFieldProModel, EntityFieldProMapper>();
            CreateMap<EntityTypeQueryModel, EntityTypeQueryMapper>();
            CreateMap<EntityTypeModel, EntityTypeMapper>();
            CreateMap<EntityTypeModel, SaveEntityTypeMapper>();
             CreateMap<FieldRulesDetailModel, FieldRulesDetailMapper>();
            CreateMap<EntityFieldRulesSaveModel, EntityFieldRulesSaveMapper>();
            CreateMap<FieldRulesVocationDetailModel, FieldRulesVocationDetailMapper>();
            CreateMap<EntityFieldRulesVocationSaveModel, EntityFieldRulesVocationSaveMapper>();
            CreateMap<SaveListViewColumnModel, SaveListViewColumnMapper>();
            CreateMap<ListViewModel, ListViewMapper>();
            CreateMap<SimpleSearchModel, SimpleSearchMapper>();
            CreateMap<AdvanceSearchModel, AdvanceSearchMapper>();
            CreateMap<EntityPageConfigModel, EntityPageConfigMapper>();
            CreateMap<EntityFieldProModel, EntityFieldProSaveMapper>();
            CreateMap<EntityProModel, EntityProSaveMapper>();
            CreateMap<DeleteEntityDataModel, DeleteEntityDataMapper>();
            CreateMap<SetRepeatModel, SetRepeatMapper>();
            CreateMap<SetRepeatModel, SaveSetRepeatMapper>();
            CreateMap<SaveEntranceGroupModel, SaveEntranceGroupMapper>();
            CreateMap<EntityProInfoModel, EntityProInfoMapper>();
            CreateMap<RelControlValueModel, RelControlValueMapper>();
            CreateMap<OrderByEntityProModel, OrderByEntityProMapper>();
            CreateMap<EntityOrderbyModel, EntityOrderbyMapper>();
            CreateMap<PersonalViewSetModel, PersonalViewSetMapper>();

            CreateMap<EntityBaseDataModel, EntityBaseDataMapper>();
            CreateMap<EntityBaseDataFieldModel, EntityBaseDataFieldMapper>();
   CreateMap<DetailModel, DetailMapper> ();
            CreateMap< EntityGlobalJsModel, EntityGlobalJsMapper > ();
        }
    }
}
