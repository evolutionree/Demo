using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Role;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Role;
using System.Linq;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class RoleServices : BasicBaseServices
    {

        private readonly IRoleRepository _roleRepsitory;
        private readonly IMapper _mapper;

        public RoleServices(IMapper mapper, IRoleRepository roleRepsitory)
        {
            _roleRepsitory = roleRepsitory;
            _mapper = mapper;
        }

        public OutputResult<object> RoleGroupQuery(int userNumber)
        {
            return new OutputResult<object>(_roleRepsitory.RoleGroupQuery(userNumber));
        }
        public OutputResult<object> InsertRoleGroup(SaveRoleGroupModel model, int userNumber)
        {
            var entity = _mapper.Map<SaveRoleGroupModel, SaveRoleGroupMapper>(model);
            if (entity != null) {
                string groupname = "";
                MultiLanguageUtils.GetDefaultLanguageValue(entity.GroupName, entity.GroupName_Lang, out groupname);
                if (groupname != null)
                {
                    entity.GroupName = groupname;
                }
            }
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            
            return HandleResult(_roleRepsitory.InsertRoleGroup(entity, userNumber));
        }

        public OutputResult<object> UpdateRoleGroup(SaveRoleGroupModel model, int userNumber)
        {
            var entity = _mapper.Map<SaveRoleGroupModel, SaveRoleGroupMapper>(model);
            if (entity != null) {
                string groupname = "";
                MultiLanguageUtils.GetDefaultLanguageValue(entity.GroupName, entity.GroupName_Lang, out groupname);
                if (groupname != null)
                {
                    entity.GroupName = groupname;
                }
            }
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
           
            return HandleResult(_roleRepsitory.UpdateRoleGroup(entity, userNumber));
        }
        public OutputResult<object> DisabledRoleGroup(SaveRoleGroupModel model, int userNumber)
        {
            return HandleResult(_roleRepsitory.DisabledRoleGroup(model.RoleGroupId, userNumber));
        }

        public OutputResult<object> OrderByRoleGroup(ICollection<SaveRoleGroupModel> models, int userNumber)
        {
            var roleGroupIds = string.Join(",", models.Select(t => t.RoleGroupId));
            return HandleResult(_roleRepsitory.OrderByRoleGroup(roleGroupIds, userNumber));
        }


        public OutputResult<object> RoleQuery(RoleListModel roleList, int userNumber)
        {
            var entity = _mapper.Map<RoleListModel, RoleListMapper>(roleList);
            return new OutputResult<object>(_roleRepsitory.RoleQuery(entity, userNumber));
        }

        public OutputResult<object> InsertRole(RoleModel role, int userNumber)
        {
            var entity = _mapper.Map<RoleModel, RoleMapper>(role);
            if (entity != null) {
                string rolename = "";
                MultiLanguageUtils.GetDefaultLanguageValue(entity.RoleName, entity.RoleName_Lang, out rolename);
                if (rolename != null)
                {
                    entity.RoleName = rolename;
                }
            }
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            
            return HandleResult(_roleRepsitory.InsertRole(entity, userNumber));
        }

        public OutputResult<object> UpdateRole(RoleModel role, int userNumber)
        {
            var entity = _mapper.Map<RoleModel, RoleMapper>(role);
            if (entity != null) {
                string rolename = "";
                MultiLanguageUtils.GetDefaultLanguageValue(entity.RoleName, entity.RoleName_Lang, out rolename);
                if (rolename != null)
                {
                    entity.RoleName = rolename;
                }
            }
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            
            List<Guid> roleIds = new List<Guid>();
            role.RoleId.Split(',').ToList().ForEach(a =>
            {
                roleIds.Add(Guid.Parse(a));
            });
            var res= HandleResult(_roleRepsitory.UpdateRole(entity, userNumber));
            var roleUsers = _roleRepsitory.GetRolesUsers(roleIds);

            if (roleUsers != null && roleUsers.Count > 0)
            {

                IncreaseDataVersion(DataVersionType.PowerData, roleUsers.Select(m => m.UserId).ToList());
                RemoveUserDataCache(roleUsers.Select(m => m.UserId).ToList());
            }
            return res;
        }

        public OutputResult<object> CopyRole(RoleCopyModel role, int userNumber)
        {
            var entity = _mapper.Map<RoleCopyModel, RoleCopyMapper>(role);
            if (entity != null) {
                string rolename = "";
                MultiLanguageUtils.GetDefaultLanguageValue(entity.RoleName, entity.RoleName_Lang, out rolename);
                if (rolename != null)
                {
                    entity.RoleName = rolename;
                }
            }
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            
            List<Guid> roleIds = new List<Guid>();
            role.RoleId.Split(',').ToList().ForEach(a =>
            {
                roleIds.Add(Guid.Parse(a));
            });
            var res= HandleResult(_roleRepsitory.CopyRole(entity, userNumber));
            var roleUsers = _roleRepsitory.GetRolesUsers(roleIds);

            if (roleUsers != null && roleUsers.Count > 0)
            {
                IncreaseDataVersion(DataVersionType.PowerData, roleUsers.Select(m => m.UserId).ToList());
                RemoveUserDataCache(roleUsers.Select(m => m.UserId).ToList());
            }

            return res;
        }
        public OutputResult<object> DeleteRole(RoleDisabledModel role, int userNumber)
        {
            List<Guid> roleIds = new List<Guid>();
            role.RoleIds.Split(',').ToList().ForEach(a =>
            {
                roleIds.Add(Guid.Parse(a));
            });
            var res= HandleResult(_roleRepsitory.DeleteRole(role.RoleIds, userNumber));
            var roleUsers = _roleRepsitory.GetRolesUsers(roleIds);

            if (roleUsers != null && roleUsers.Count > 0)
            {
                IncreaseDataVersion(DataVersionType.PowerData, roleUsers.Select(m => m.UserId).ToList());
                RemoveUserDataCache(roleUsers.Select(m => m.UserId).ToList());
            }
            return res;
        }

        public OutputResult<object> RoleUserQuery(RoleUserModel roleList, int userNumber)
        {
            var entity = _mapper.Map<RoleUserModel, RoleUserMapper>(roleList);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return new OutputResult<object>(_roleRepsitory.RoleUserQuery(entity, userNumber));
        }
        public OutputResult<object> DeleteRoleUser(RoleUserRelationModel relate, int userNumber)
        {
            var userList = relate.UserIds.Split(',').ToList();
            var res= HandleResult(_roleRepsitory.DeleteRoleUser(relate.UserIds, relate.RoleId, userNumber));
            if (userList != null && userList.Count > 0)
            {
                var userListtemp = new List<int>();
                foreach (var temp in userList)
                {
                    userListtemp.Add(int.Parse(temp));
                }
                IncreaseDataVersion(DataVersionType.PowerData, userListtemp);
                RemoveUserDataCache(userList);
            }
            return res;
        }

        public OutputResult<object> AssigneRoleUser(AssigneUserToRoleModel assigne, int userNumber)
        {
            var entity = _mapper.Map<AssigneUserToRoleModel, AssigneUserToRoleMapper>(assigne);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            var res= ExcuteAction((transaction, arg, userData) =>
            {
                return HandleResult(_roleRepsitory.AssigneRoleUser(entity, userNumber));
            }, assigne, userNumber);
            var userList = assigne.UserIds.Split(',').ToList();
            RemoveUserDataCache(userList);

            if (userList != null && userList.Count > 0)
            {
                var userListtemp = new List<int>();
                foreach(var temp in userList)
                {
                    userListtemp.Add(int.Parse(temp));
                }
                IncreaseDataVersion(DataVersionType.PowerData, userListtemp);
                RemoveUserDataCache(userList);
            }

            return res;

        }
    }
}
