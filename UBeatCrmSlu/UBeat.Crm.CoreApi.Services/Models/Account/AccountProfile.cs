using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.Account;

namespace UBeat.Crm.CoreApi.Services.Models.Account
{
    public class AccountProfile : Profile
    {
        public AccountProfile()
        {
            CreateMap<AccountRegistModel, AccountUserRegistMapper>();
            CreateMap<AccountEditModel, AccountUserEditMapper>();
            CreateMap<AccountPasswordModel, AccountUserPwdMapper>();
            CreateMap<AccountModel, AccountMapper>();
            CreateMap<AccountModifyPhotoModel, AccountUserPhotoMapper>();
            CreateMap<AccountQueryModel, AccountUserQueryMapper>();
            CreateMap<AccountQueryForControlModel, AccountUserQueryForControlMapper>();
            CreateMap<AccountStatusModel, AccountStatusMapper>();
            CreateMap<AccountDepartmentModel, AccountDepartmentMapper>();
            CreateMap<DeptOrderbyModel, DeptOrderbyMapper>();
            CreateMap<DeptDisabledModel, DeptDisabledMapper>();
            CreateMap<SetLeaderModel, SetLeaderMapper>();
        }
    }

}
