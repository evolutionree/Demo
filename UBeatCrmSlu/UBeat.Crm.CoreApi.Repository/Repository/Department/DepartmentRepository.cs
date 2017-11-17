using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Department;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Linq;

namespace UBeat.Crm.CoreApi.Repository.Repository.Department
{
    public class DepartmentRepository : RepositoryBase, IDepartmentRepository
    {
        public OperateResult DeptAdd(DbTransaction tran, DepartmentAddMapper deptEntity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_department_add(@topdeptId,@deptName, @oglevel, @userNo)
            ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("topdeptId",deptEntity.PDeptId),
                new NpgsqlParameter("deptName",deptEntity.DeptName),
                new NpgsqlParameter("oglevel",deptEntity.OgLevel),
                 new NpgsqlParameter("userNo", userNumber)
            };
            var result = DBHelper.ExecuteQuery<OperateResult>(tran, sql, param);

            return result.FirstOrDefault();
        }

        public OperateResult DeptEdit(DepartmentEditMapper deptEntity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_department_edit(@deptId,@deptName, @pdeptid, @userNo)
            ";
            var param = new
            {
                DeptId = deptEntity.DeptId,
                DeptName = deptEntity.DeptName,
                PDeptId = deptEntity.PDeptId,
                UserNo = userNumber
            };
            var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
            return result;
        }



    }
}
