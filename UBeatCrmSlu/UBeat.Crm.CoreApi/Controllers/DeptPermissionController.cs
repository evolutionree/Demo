using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel.Department;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    /// <summary>
    /// 用于部门及人员特殊权限的处理过程
    /// </summary>
    [Route("api/[controller]")]
    public class DeptPermissionController : BaseController
    {
        private DeptPermissionServices _deptPermissionServices;
        public DeptPermissionController(DeptPermissionServices  deptPermissionServices) : base( deptPermissionServices) {
            _deptPermissionServices = deptPermissionServices;
        }
        /// <summary>
        /// 新增方案
        /// </summary>
        /// <returns></returns>
        [HttpPost("add")]
        public OutputResult<object> AddScheme([FromBody] SchemeParamModel paramInfo) {
            throw (new NotImplementedException());
        }
        [HttpPost("update")]
        public OutputResult<object> UpdateScheme([FromBody] SchemeParamModel paramInfo) {
            throw (new NotImplementedException());
        }
        [AllowAnonymous]
        [HttpPost("list")]
        public OutputResult<object> ListScheme([FromBody] SchemeParamModel paramInfo)
        {
            return new OutputResult<object>(this._deptPermissionServices.ListPermissionDetailByRoleId(paramInfo.RecId, paramInfo.RecId, 0));

        }
        /// <summary>
        /// 查询指定的权限方案下，所有被记录的被授权对象
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost("authlist")]
        public OutputResult<object> ListAuthorizedObject([FromBody] SchemeAuthorizedListParamInfo paramInfo) {
            if (paramInfo == null || paramInfo.SchemeId == null || paramInfo.SchemeId.Equals(Guid.Empty)) {
                return ResponseError<object>("参数异常");
            }
            return new OutputResult<object>(this._deptPermissionServices.ListAuthorizedObjects(paramInfo.SchemeId, UserId));
        }
        /// <summary>
        /// 获取授权的详情信息
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost("detail")]
        public OutputResult<object> DetailScheme([FromBody] SchemeDetailFetchParamInfo paramInfo)
        {
            if (paramInfo == null || paramInfo.SchemeId == null || paramInfo.SchemeId.Equals(Guid.Empty)) {
                return ResponseError<object>("参数异常");
            }
            if (paramInfo.RoleId == null || paramInfo.RoleId.Equals(Guid.Empty))
            {
                return new OutputResult<object>(this._deptPermissionServices.ListPermissionDetailByRoleId(paramInfo.SchemeId, paramInfo.RoleId, UserId));
            }
            else {
                return new OutputResult<object>(this._deptPermissionServices.ListPermissionDetailByUserId(paramInfo.SchemeId, paramInfo.UserId, UserId));
            }
        }

        [HttpPost("savedetail")]
        public OutputResult<object> SaveSchemeDetail([FromBody]SchemeDetailSaveParamInfo paramInfo  ) {
            if (paramInfo == null || paramInfo.SchemeId == null || paramInfo.SchemeId.Equals(Guid.Empty))
                return ResponseError<object>("参数异常");
            if (paramInfo.Authorized_Type == DeptPermissionObjectTypeEnum.User)
            {
                if (paramInfo.Authorized_UserId <= 0) return ResponseError<object>("参数异常");
            }
            else if (paramInfo.Authorized_Type == DeptPermissionObjectTypeEnum.Role)
            {
                if (paramInfo.Authorized_RoleId == null || paramInfo.Authorized_RoleId.Equals(Guid.Empty)) return ResponseError<object>("参数异常");
            }
            else {
                return ResponseError<object>("参数异常");
            }
            return new OutputResult<object>(this._deptPermissionServices.SaveSchemeDetail(paramInfo.SchemeId,paramInfo.Authorized_UserId,paramInfo.Authorized_RoleId,paramInfo.Authorized_Type,paramInfo.Items, UserId));
        }
        [HttpPost("fetch")]
        public OutputResult<object> FetchOrgAndUsers() {
            throw (new NotImplementedException());
        }
    }
    /// <summary>
    /// 用于新增和修改参数使用
    /// </summary>
    public class SchemeParamModel {
        public Guid RecId { get; set; }
        public string SchemeName{get;set;}
        public string Remark { get; set; }
    }
    /// <summary>
    /// 在指定权限方案下，查询某指定的被授权对象（可能是角色，也可能是用户）的所有组织用户权限分配情况（权限项）的查询参数
    /// </summary>
    public class SchemeDetailFetchParamInfo {
        /// <summary>
        /// 方案id
        /// </summary>
        public Guid SchemeId { get; set; }
        /// <summary>
        /// 角色id，可能为空，如果为空，则userid有效，
        /// </summary>
        public Guid RoleId { get; set; }
        /// <summary>
        /// 授权的用户的id
        /// </summary>
        public int UserId { get; set; }
    }
    /// <summary>
    /// 查询某权限方案的所有被授权对象（包括用户和角色）的查询参数
    /// </summary>
    public class SchemeAuthorizedListParamInfo {
        public Guid SchemeId { get; set; }
    }
    /// <summary>
    /// 保存权限授权列表
    /// </summary>
    public class SchemeDetailSaveParamInfo {
        public Guid SchemeId { get; set; }
        public int Authorized_UserId { get; set; }
        public Guid Authorized_RoleId { get; set; }
        public DeptPermissionObjectTypeEnum Authorized_Type { get; set; }
        public List<DeptPermissionSchemeEntryInfo> Items { get; set; }
    }
}
