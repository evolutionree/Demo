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

        OperateResult DeptEdit( DepartmentEditMapper deptEntity, int userNumber);
    }
}
