using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.Department;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DepartmentController:BaseController
    {
        private readonly DepartmentServices _departmentServices;

        public DepartmentController(DepartmentServices deptServices) : base(deptServices)
        {
            _departmentServices = deptServices;
        }

        [HttpPost]
        [Route("add")]
        public OutputResult<object> DeptAdd([FromBody] DepartmentAddModel deptModel = null)
        {
            if (deptModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("新增团队部门", deptModel);
            return _departmentServices.DeptAdd(deptModel, UserId);
        }

        [HttpPost]
        [Route("edit")]
        public OutputResult<object> DeptEdit([FromBody] DepartmentEditModel deptModel = null)
        {
            if (deptModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("编辑团队部门", deptModel);
            return _departmentServices.DeptEdit(deptModel, UserId);
        }
        [HttpPost("listsub")]
        public OutputResult<object> ListSubDeptsAndUsers([FromBody]DepartmentListSubDeptParamInfo paramInfo  ) {
            if (paramInfo == null || paramInfo.DeptId == null || paramInfo.DeptId == Guid.Empty) {
                return ResponseError<object>("参数异常");
            }
            return _departmentServices.ListSubDeptsAndUsers(paramInfo.DeptId, UserId);

        }
    }
}
