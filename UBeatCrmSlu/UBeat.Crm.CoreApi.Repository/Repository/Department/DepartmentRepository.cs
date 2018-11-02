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
using Dapper;

namespace UBeat.Crm.CoreApi.Repository.Repository.Department
{
    public class DepartmentRepository : RepositoryBase, IDepartmentRepository
    {
        public OperateResult DeptAdd(DbTransaction tran, DepartmentAddMapper deptEntity, int userNumber)
        {
            var sql = @"
                SELECT * FROM crm_func_department_add(@topdeptId,@deptName, @oglevel, @userNo,@deptlanguage)
            ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("topdeptId",deptEntity.PDeptId),
                new NpgsqlParameter("deptName",deptEntity.DeptName),
                new NpgsqlParameter("oglevel",deptEntity.OgLevel),
                new NpgsqlParameter("userNo", userNumber),
                new NpgsqlParameter("deptlanguage",deptEntity.DeptLanguage)
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
                        new NpgsqlParameter("deptlanguage",deptEntity.DeptLanguage)
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

                    var sql = @" UPDATE crm_sys_department SET deptname = @deptname,pdeptid = @pdeptid,recupdated=@recupdated, recupdator=@recupdator ,deptlanguage=@deptlanguage::jsonb WHERE deptid = @deptid;";

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
            catch (Exception ex)
            {
                return new List<Dictionary<string, object>>();
            }
        }

        public List<Dictionary<string, object>> ListSubUsers(DbTransaction tran, Guid deptId, int userId)
        {
            try
            {
                string strSQL = @"SELECT
	                                A .*,c.deptid,c.deptname,case when d.userid is null then false else true end ""flag""
                                FROM
                                    crm_sys_userinfo A
                                INNER JOIN(
                                    SELECT
                                        *
                                    FROM
                                        crm_sys_account_userinfo_relate
                                    WHERE
                                        recstatus = 1
                                ) b ON A.userid = b.userid
                                inner join(
                                select* from crm_sys_department
                                )c on c.deptid = b.deptid
                                left outer join(
                                    select distinct userid
                                        from crm_sys_flaglinkman
                                        where recmanager = @recmanager
                                ) d on d.userid = a.userid
                                WHERE
                                    b.deptid = @pdeptid
                                AND A .recstatus = 1  ";
                DbParameter[] p = new DbParameter[] {
                    new Npgsql.NpgsqlParameter("@pdeptid",deptId),
                    new Npgsql.NpgsqlParameter("@recmanager",userId)
                };
                return ExecuteQuery(strSQL, p, tran);
            }
            catch (Exception ex)
            {
                return new List<Dictionary<string, object>>();
            }
        }


        public OperateResult SaveUpdateDepartmentPosition(DbTransaction tran, DepartmentPosition position, int userId)
        {
            try
            {
                var sql = @"
                SELECT * FROM crm_func_dept_change(@userid,@deptid,@userNo)
            ";
                var param = new DynamicParameters();
                param.Add("userid", position.UserId);
                param.Add("deptid", position.Departs.FirstOrDefault(t => t.IsMaster == 1).DepartId.ToString());
                param.Add("userNo", userId);
                var result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                if (result.Flag == 1)
                {
                    foreach (var tmp in position.Departs.Where(t => t.IsMaster == 0))
                    {
                        var delSql = @"delete from crm_sys_parttime where userid=@userid and departid=@predepartid;";
                        sql = @"insert into crm_sys_parttime(userid,departid) values(@userid,@departid)";
                        var positionRecord = @"insert into crm_sys_user_position_record(userid,predepartid,curdepartid) values (@userid,@predepartid,@departid)";
                        param = new DynamicParameters();
                        param.Add("userid", userId);
                        param.Add("departid", tmp.DepartId);
                        param.Add("predepartid", tmp.PreDepartId);
                        // param.Add("type", tmp.Type);
                        DataBaseHelper.ExecuteNonQuery(delSql, tran.Connection, tran, param);
                        DataBaseHelper.ExecuteNonQuery(sql, tran.Connection, tran, param);
                        if (tmp.DepartId == tmp.PreDepartId) continue;
                        DataBaseHelper.ExecuteNonQuery(positionRecord, tran.Connection, tran, param);
                    }
                }
            }
            catch (Exception ex)
            {
                return new OperateResult()
                {
                    Msg = "分配职位失败"
                };
            }
            return new OperateResult()
            {
                Flag = 1,
                Msg = "分配职位成功"
            };

        }
    }
}
