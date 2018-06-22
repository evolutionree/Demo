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

    
        public OperateResult EditDepartment(DepartmentEditMapper deptEntity, int userNumber)
        {
            OperateResult res = new OperateResult();
            if (string.IsNullOrEmpty(deptEntity.DeptName))
            {
                throw new Exception("部门名称不能为空");
            }
            using (DbConnection conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();

                try
                {
                    var param = new DbParameter[]
                    {
                        new NpgsqlParameter("deptid", deptEntity.DeptId),
                        new NpgsqlParameter("pdeptid", deptEntity.PDeptId),
                        new NpgsqlParameter("deptname", deptEntity.DeptName),
                        new NpgsqlParameter("recupdated", DateTime.Now),
                        new NpgsqlParameter("recupdator", userNumber),
                    };

                    var check_sql = @"SELECT 1 FROM crm_sys_department where deptid<>@deptid AND pdeptid=@pdeptid AND deptname=@deptname LIMIT 1";
                    var isExist = ExecuteScalar(check_sql, param, tran);
                    if (isExist != null && isExist.ToString() == "1")
                    {
                        throw new Exception("同一级的部门名称不能重复");
                    }
                    var old_pdeptid_sql = @"SELECT pdeptid FROM crm_sys_department where deptid = @deptid;";
                    var old_pdeptid_res = ExecuteScalar(old_pdeptid_sql, param, tran);
                    Guid old_pdeptid;
                    if (old_pdeptid_res != null && Guid.TryParse(old_pdeptid_res.ToString(), out old_pdeptid))
                    {
                        if (old_pdeptid != deptEntity.PDeptId)
                        {
                            var repair_sql = @" DELETE FROM crm_sys_department_treepaths WHERE descendant=@deptid;
                                                INSERT INTO crm_sys_department_treepaths(ancestor,descendant,nodepath)
					                                        SELECT t.ancestor,@deptid,nodepath+1
					                                        FROM crm_sys_department_treepaths AS t
					                                        WHERE t.descendant = @pdeptid
					                                        UNION ALL
					                                        SELECT @deptid,@deptid,0;";
                            var repairParam = new DbParameter[]
                            {
                                new NpgsqlParameter("deptid", deptEntity.DeptId),
                                new NpgsqlParameter("pdeptid", deptEntity.PDeptId)
                            };
                           ExecuteNonQuery(repair_sql, repairParam, tran);
                        }
                    }

                    var sql = @" UPDATE crm_sys_department SET deptname = @deptname,pdeptid = @pdeptid,recupdated=@recupdated, recupdator=@recupdator WHERE deptid = @deptid;";

                    var result = ExecuteNonQuery(sql, param, tran);
                    if (result > 0)
                    {
                        res.Id = deptEntity.DeptId.ToString();
                        res.Flag = 1;
                    }
                    else throw new Exception("修改保存失败！");
                    tran.Commit();
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw ex;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
                return res;
            }
        }

        public List<Dictionary<string, object>> ListSubDepts(DbTransaction tran, Guid deptId, int userId)
        {
            try
            {
                string strSQL = "select *  from crm_sys_department  where pdeptid =@pdeptid and recstatus = 1 ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@pdeptid",deptId)
                };
                return ExecuteQuery(strSQL, p, tran);
            }
            catch (Exception ex) {
                return new List<Dictionary<string, object>>();
            }
        }

        public List<Dictionary<string, object>> ListSubUsers(DbTransaction tran, Guid deptId, int userId)
        {
            try
            {
                string strSQL = @"select a.* 
                            from crm_sys_userinfo a
                             inner join (
		                            select *
		                            from crm_sys_account_userinfo_relate 
		                            where recstatus = 1
                            ) b on a.userid = b.userid 
                            where b.deptid = @pdeptid
                            and a.recstatus  =1  ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@pdeptid",deptId)
                };
                return ExecuteQuery(strSQL, p, tran);
            }
            catch (Exception ex)
            {
                return new List<Dictionary<string, object>>();
            }
        }
    }
}
