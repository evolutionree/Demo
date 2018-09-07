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
        private readonly IRoleRepository _roleRepsitory;
        public DesktopServices(IMapper mapper, IAccountRepository accountRepository, IDesktopRepository desktopRepository, IRoleRepository roleRepsitory, IConfigurationRoot config)
        {
            _accountRepository = accountRepository;
            _desktopRepository = desktopRepository;
            _mapper = mapper;
            _roleRepsitory = roleRepsitory;
        }

        public OutputResult<object> GetDesktop(int userId)
        {
            return new OutputResult<object>(_desktopRepository.GetDesktop(userId));
        }

        public OutputResult<object> GetDesktops(SearchDesktop model, int userId)
        {
            var mapper = _mapper.Map<SearchDesktop, SearchDesktopMapper>(model);
            if (mapper == null || !mapper.IsValid())
            {
                return HandleValid(mapper);
            }
            return new OutputResult<object>(_desktopRepository.GetDesktops(mapper, userId));
        }

        public OutputResult<object> GetDesktopComponents(SearchDesktopComponent model, int userId)
        {
            var mapper = _mapper.Map<SearchDesktopComponent, SearchDesktopComponentMapper>(model);
            return new OutputResult<object>(_desktopRepository.GetDesktopComponents(mapper, userId));
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
                var result = _desktopRepository.SaveDesktopComponent(mapper, transaction);
                return new OutputResult<object>(result);

            }, model, userId);
        }

        public OutputResult<object> SaveDesktop(Desktop model, int userId)
        {
            var mapper = _mapper.Map<Desktop, DesktopMapper>(model);
            if (mapper == null || !mapper.IsValid())
            {
                return HandleValid(mapper);
            }

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _desktopRepository.SaveDesktop(mapper, transaction);
                return new OutputResult<object>(result);

            }, model, userId);
        }


        public OutputResult<object> EnableDesktopComponent(DesktopComponent model, int userId)
        {
            if (model.Status < 0)
            {
                return new OutputResult<object>(model, "状态码不能小于0", status: 1);
            }
            var mapper = _mapper.Map<DesktopComponent, DesktopComponentMapper>(model);
            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _desktopRepository.EnableDesktopComponent(mapper, transaction);
                return new OutputResult<object>(result);

            }, model, userId);
        }

        public OutputResult<object> GetDesktopComponentDetail(DesktopComponent model)
        {
            if (model.DsComponetId == null || model.DsComponetId == Guid.Empty)
            {
                return new OutputResult<object>(model, "工作台组件Id不能为空", status: 1);
            }

            return new OutputResult<object>(_desktopRepository.GetDesktopComponentDetail(model.DsComponetId));
        }
        public OutputResult<object> GetDesktopDetail(Desktop model)
        {
            if (model.DesktopId == null || model.DesktopId == Guid.Empty)
            {
                return new OutputResult<object>(model, "工作台Id不能为空", status: 1);
            }
            return new OutputResult<object>(_desktopRepository.GetDesktopDetail(model.DesktopId));
        }
        public OutputResult<object> EnableDesktop(Desktop model, int userId)
        {
            if (model.Status < 0)
            {
                return new OutputResult<object>(model, "状态码不能小于0", status: 1);
            }
            var mapper = _mapper.Map<Desktop, DesktopMapper>(model);
            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _desktopRepository.EnableDesktop(mapper, transaction);
                return new OutputResult<object>(result);

            }, model, userId);
        }
        public OutputResult<object> AssignComsToDesktop(ComToDesktop model, int userId)
        {
            var mapper = _mapper.Map<ComToDesktop, ComToDesktopMapper>(model);

            var result = _desktopRepository.AssignComsToDesktop(mapper, userId);
            return new OutputResult<object>(result);
        }
        public OutputResult<object> SaveDesktopRoleRelation(IList<DesktopRoleRelation> models, int userId)
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
            return ExcuteAction((transaction, arg, userData) =>
            {
                return new OutputResult<object>(_desktopRepository.SaveDesktopRoleRelation(desktopRoleRelations, transaction));
            }, models, userId);
        }
        public OutputResult<Object> GetRoles(DesktopRoleRelation model, int userId)
        {
            var result = _desktopRepository.GetRoles(model.DesktopId, userId);
            return new OutputResult<object>(result);
        }
        #endregion


        #region 动态列表

        public OutputResult<Object> GetDynamicList(DynamicListRequest requestModel, int userId)
        {
            var mapper = _mapper.Map<DynamicListRequest, DynamicListRequestMapper>(requestModel);
            if (mapper == null || !mapper.IsValid())
            {
                return HandleValid(mapper);
            }

            var result = _desktopRepository.GetDynamicList(mapper, userId);
            return new OutputResult<object>(result);
        }


        /// <summary>
        /// 获取主实体列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<Object> GetMainEntityList(int userId)
        {
            var result = _desktopRepository.GetMainEntityList(userId);
            return new OutputResult<object>(result);

        }


        /// <summary>
        /// 获取关联实体列表
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public OutputResult<Object> GetRelatedEntityList(Guid entityId, int userId)
        {
            var result = _desktopRepository.GetRelatedEntityList(entityId, userId);
            return new OutputResult<object>(result);
        }



        #endregion
    }
}
