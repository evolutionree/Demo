using AutoMapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Desktop
{
    public class DesktopServices : BasicBaseServices
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IDesktopRepository _desktopRepository;
        private readonly IMapper _mapper;

        public DesktopServices(IMapper mapper, IAccountRepository accountRepository, IDesktopRepository desktopRepository, IConfigurationRoot config)
        {
            _accountRepository = accountRepository;
            _desktopRepository = desktopRepository;
            _mapper = mapper;

        }

        public OutputResult<object> GetDesktop(int userId)
        {
            return new OutputResult<object>(_desktopRepository.GetDesktop(userId));
        }


        #region config
        public OutputResult<object> SaveDesktopComponent(DesktopComponent model, int userId)
        {
            var mapper = _mapper.Map<DesktopComponent, DesktopComponentMapper>(model);
            if (mapper == null || !mapper.IsValid())
            {
                return HandleValid(mapper);
            }

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _desktopRepository.SaveDesktopComponent(mapper);
                return new OutputResult<object>(result);

            }, model, userId);
        }

        public OutputResult<object> EnableDesktopComponent(DesktopComponent model)
        {
            if (model.Status < 0)
            {
                return new OutputResult<object>(model, "状态码不能小于0", status: 1);
            }
            var mapper = _mapper.Map<DesktopComponent, DesktopComponentMapper>(model);
            return new OutputResult<object>(_desktopRepository.EnableDesktopComponent(mapper));
        }

        public OutputResult<object> GetDesktopComponentDetail(DesktopComponent model)
        {
            if (model.DsComponetId == null || model.DsComponetId == Guid.Empty)
            {
                return new OutputResult<object>(model, "工作台组件Id不能为空", status: 1);
            }
            return new OutputResult<object>(_desktopRepository.GetDesktopComponentDetail(model.DsComponetId));
        }

        public OutputResult<object> EnableDesktop(Desktop model)
        {
            if (model.Status < 0)
            {
                return new OutputResult<object>(model, "状态码不能小于0", status: 1);
            }
            var mapper = _mapper.Map<Desktop, DesktopMapper>(model);
            return new OutputResult<object>(_desktopRepository.EnableDesktop(mapper));
        }

        public OutputResult<object> SaveDesktopRoleRelation(IList<DesktopRoleRelation> models)
        {
            List<DesktopRoleRelationMapper> desktopRoleRelations = new List<DesktopRoleRelationMapper>();
            foreach (var model in models)
            {
                var mapper = _mapper.Map<DesktopRoleRelation, DesktopRoleRelationMapper>(model);
                if (mapper == null || !mapper.IsValid())
                {
                    return HandleValid(mapper);
                }
                desktopRoleRelations.Add(mapper);
            }
            return new OutputResult<object>(_desktopRepository.SaveDesktopRoleRelation(desktopRoleRelations));
        }
        #endregion
    }
}
