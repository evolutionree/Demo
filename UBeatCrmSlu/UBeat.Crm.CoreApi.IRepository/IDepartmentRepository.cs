using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Department;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDepartmentRepository : IBaseRepository
    {
        OperateResult DeptAdd(DbTransaction tran, DepartmentAddMapper deptEntity, int userNumber);

        OperateResult EditDepartment( DepartmentEditMapper deptEntity, int userNumber);
        List<Dictionary<string, object>> ListSubDepts(DbTransaction tran, Guid deptId, int userId);
        List<Dictionary<string, object>> ListSubUsers(DbTransaction tran, Guid deptId, int userId);
    }
}
