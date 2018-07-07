using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Role;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class RoleController : BaseController
    {

        private readonly RoleServices _roleServices;

        public RoleController(RoleServices roleServices) : base(roleServices)
        {
            _roleServices = roleServices;
        }

        #region 角色分组
        [HttpPost]
        [Route("queryrolegroup")]
        public OutputResult<object> RoleGroupQuery()
        {
            return _roleServices.RoleGroupQuery(UserId);
        }

        [HttpPost]
        [Route("insertrolegroup")]
        public OutputResult<object> InsertRoleGroup([FromBody]SaveRoleGroupModel roleGrop = null)
        {
            if (roleGrop == null) return ResponseError<object>("参数格式错误");

            return _roleServices.InsertRoleGroup(roleGrop, UserId);
        }

        [HttpPost]
        [Route("updaterolegroup")]
        public OutputResult<object> UpdateRoleGroup([FromBody]SaveRoleGroupModel roleGrop = null)
        {
            if (roleGrop == null) return ResponseError<object>("参数格式错误");

            return _roleServices.UpdateRoleGroup(roleGrop, UserId);
        }

        [HttpPost]
        [Route("disabledrolegroup")]
        public OutputResult<object> DisabledRoleGroup([FromBody]SaveRoleGroupModel roleGrop = null)
        {
            if (roleGrop == null) return ResponseError<object>("参数格式错误");

            return _roleServices.DisabledRoleGroup(roleGrop, UserId);
        }

        [HttpPost]
        [Route("orderbyrolegroup")]
        public OutputResult<object> OrderByRoleGroup([FromBody]ICollection<SaveRoleGroupModel> roleGrops = null)
        {
            if (roleGrops == null || roleGrops.Count == 0) return ResponseError<object>("参数格式错误");

            return _roleServices.OrderByRoleGroup(roleGrops, UserId);
        }
        #endregion

        #region 角色
        [HttpPost]
        [Route("queryrole")]
        public OutputResult<object> RoleQuery([FromBody]RoleListModel roleList = null)
        {
            if (roleList == null) return ResponseError<object>("参数格式错误");

            return _roleServices.RoleQuery(roleList, UserId);
        }

        [HttpPost]
        [Route("insertrole")]
        public OutputResult<object> InsertRole([FromBody]RoleModel role = null)
        {
            if (role == null) return ResponseError<object>("参数格式错误");

            return _roleServices.InsertRole(role, UserId);
        }

        [HttpPost]
        [Route("updaterole")]
        public OutputResult<object> UpdateRole([FromBody]RoleModel role = null)
        {
            if (role == null) return ResponseError<object>("参数格式错误");

            return _roleServices.UpdateRole(role, UserId);
        }

        [HttpPost]
        [Route("copyrole")]
        public OutputResult<object> CopyRole([FromBody]RoleCopyModel role = null)
        {
            if (role == null) return ResponseError<object>("参数格式错误");

            return _roleServices.CopyRole(role, UserId);
        }
        [HttpPost]
        [Route("deletedrole")]
        public OutputResult<object> DeleteRole([FromBody]RoleDisabledModel role = null)
        {
            if (role == null) return ResponseError<object>("参数格式错误");

            return _roleServices.DeleteRole(role, UserId);
        }

        #endregion

        #region 角色关联用户
        [HttpPost]
        [Route("queryroleuser")]
        public OutputResult<object> RoleUserQuery([FromBody]RoleUserModel roleList = null)
        {
            if (roleList == null) return ResponseError<object>("参数格式错误");

            return _roleServices.RoleUserQuery(roleList, UserId);
        }
        [HttpPost]
        [Route("assigneroleuser")]
        public OutputResult<object> AssigneRoleUser([FromBody]AssigneUserToRoleModel assigneUser = null)
        {
            if (assigneUser == null) return ResponseError<object>("参数格式错误");

            return _roleServices.AssigneRoleUser(assigneUser, UserId);
        }
 
        [HttpPost]
        [Route("deleteroleuser")]
        public OutputResult<object> DeleteRoleUser([FromBody]RoleUserRelationModel userList = null)
        {
            if (userList == null ) return ResponseError<object>("参数格式错误");

            return _roleServices.DeleteRoleUser(userList, UserId);
        }
        #endregion
    }
}
