using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Role;

namespace UBeat.Crm.CoreApi.Services.Models.Role
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<SaveRoleGroupModel, SaveRoleGroupMapper>();

            CreateMap<RoleListModel, RoleListMapper>();

            CreateMap<RoleModel, RoleMapper>();

            CreateMap<RoleCopyModel, RoleCopyMapper>();

            CreateMap<RoleUserModel, RoleUserMapper>();

            CreateMap<AssigneUserToRoleModel, AssigneUserToRoleMapper>();
        }

    }
}
