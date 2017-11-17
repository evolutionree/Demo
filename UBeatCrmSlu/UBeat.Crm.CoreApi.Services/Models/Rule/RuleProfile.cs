using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainMapper.Rule;

namespace UBeat.Crm.CoreApi.Services.Models.Rule
{
    public class RuleProfile : Profile
    {
        public RuleProfile()
        {
            CreateMap<RuleSetModel, RuleSetMapper>();
            CreateMap<RuleItemRelationModel, RuleItemRelationMapper>();
            CreateMap<RuleItemModel, RuleItemMapper>();
            CreateMap<RuleModel, RuleMapper>();
            CreateMap<RoleRuleModel, RoleRuleMapper>();
            CreateMap<MenuRuleModel, MenuRuleMapper>();
            CreateMap<DynamicRuleModel, DynamicRuleMapper>();
        }
    }
}
