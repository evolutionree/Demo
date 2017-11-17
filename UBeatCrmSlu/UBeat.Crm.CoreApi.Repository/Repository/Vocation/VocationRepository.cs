using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.Rule;
using UBeat.Crm.CoreApi.DomainModel.Vocation;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Vocation
{
    public class VocationRepository : RepositoryBase, IVocationRepository
    {

        /// <summary>
        /// 添加职能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult AddVocation(VocationAdd data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_vocation_insert(@vocationname,@description,@userno)";
            var args = new
            {
                VocationName = data.VocationName,
                Description = data.Description,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }
        /// <summary>
        /// 添加职能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult AddCopyVocation(CopyVocationAdd data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_vocation_copy(@vocationid,@vocationname,@description,@userno)";
            var args = new
            {
                VocationId = data.VocationId,
                VocationName = data.VocationName,
                Description = data.Description,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }
        /// <summary>
        /// 编辑职能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>

        public OperateResult EditVocation(VocationEdit data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_vocation_update(@vocationid,@vocationname,@description,@userno)";
            var args = new
            {
                VocationId = data.VocationId,
                VocationName = data.VocationName,
                Description = data.Description,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 删除职能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult DeleteVocation(VocationDelete data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_vocation_delete(@vocationid,@userno)";
            var args = new
            {
                VocationId = string.Join(",", data.VocationIds),
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 获取职能列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="vocationName"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetVocations(PageParam page, string vocationName, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_vocation_select(@vocationname,@userno,@pageindex,@pagesize)";
            var args = new
            {
                VocationName = vocationName,
                UserNo = userNumber,
                PageIndex = page.PageIndex,
                PageSize = page.PageSize
            };



            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor", "pagecursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;
        }


        /// <summary>
        /// 根据职能id,获取功能列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public dynamic GetFunctionsByVocationId(VocationFunctionSelect data)
        {
            var executeSql = @"SELECT * FROM crm_func_vocation_function_list(@funcid,@vocationid,@direction)";
            var args = new
            {
                FuncId = data.FuncId,
                VocationId = data.VocationId,
                Direction = data.Direction
            };

            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;
        }



        /// <summary>
        /// 编辑职能下的功能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult EditVocationFunctions(VocationFunctionEdit data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_vocation_function_relation_update(@vocationid,@functions,@userno)";
            var args = new
            {
                VocationId = data.VocationId,
                Functions = data.FunctionJson,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }



        /// <summary>
        /// 添加功能下的规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult AddFunctionRule(List<FunctionRuleAdd> data, int userNumber)
        {
            var conn = DataBaseHelper.GetDbConnect();
            conn.Open();
            var trans = conn.BeginTransaction();
            OperateResult result = new OperateResult();
            try
            {
                foreach (var tmp in data)
                {
                    string executeSql = string.Empty;
                    object args = null;
                    if (tmp.IsAdd)
                    {
                        executeSql = @"SELECT * FROM crm_func_vocation_function_rule_insert(@vocationid,@functionid,@rule,@ruleitem,@ruleset,@rulerelation,@userno)";
                        args = new
                        {
                            VocationId = tmp.VocationId,
                            FunctionId = tmp.FunctionId,
                            Rule = tmp.Rule,
                            Ruleitem = tmp.RuleItem,
                            Ruleset = tmp.RuleSet,
                            RuleRelation = tmp.RuleRelation,
                            UserNo = userNumber
                        };
                    }
                    else
                    {
                        executeSql = @"SELECT * FROM crm_func_vocation_function_rule_update(@vocationid,@functionid,@rule,@ruleitem,@ruleset,@rulerelation,@userno)";
                        args = new
                        {
                            VocationId = tmp.VocationId,
                            FunctionId = tmp.FunctionId,
                            Rule = tmp.Rule,
                            RuleItem = tmp.RuleItem,
                            RuleSet = tmp.RuleSet,
                            RuleRelation = tmp.RuleRelation,
                            UserNo = userNumber
                        };
                    }

                    result = DataBaseHelper.QuerySingle<OperateResult>(conn, executeSql, args);
                    if (result.Flag == 0) throw new Exception("编辑职能规则异常");
                }
                trans.Commit();
                return result;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                result.Msg = ex.Message;
                return result;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }


        /// <summary>
        /// 编辑功能下的规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult EditFunctionRule(List<FunctionRuleEdit> data, int userNumber)
        {
            var conn = DataBaseHelper.GetDbConnect();
            conn.Open();
            var trans = conn.BeginTransaction();
            OperateResult result = new OperateResult();
            try
            {
                foreach (var tmp in data)
                {
                    string executeSql = string.Empty;
                    object args = null;
                    if (tmp.IsAdd)
                    {
                        executeSql = @"SELECT * FROM crm_func_vocation_function_rule_insert(@vocationid,@functionid,@rule,@ruleitem,@ruleset,@rulerelation,@userno)";
                        args = new
                        {
                            VocationId = tmp.VocationId,
                            FunctionId = tmp.FunctionId,
                            Rule = tmp.Rule,
                            Ruleitem = tmp.RuleItem,
                            Ruleset = tmp.RuleSet,
                            RuleRelation = tmp.RuleRelation,
                            UserNo = userNumber
                        };
                    }
                    else
                    {
                        executeSql = @"SELECT * FROM crm_func_vocation_function_rule_update(@vocationid,@functionid,@rule,@ruleitem,@ruleset,@rulerelation,@userno)";
                        args = new
                        {
                            VocationId = tmp.VocationId,
                            FunctionId = tmp.FunctionId,
                            Rule = tmp.Rule,
                            RuleItem = tmp.RuleItem,
                            RuleSet = tmp.RuleSet,
                            RuleRelation = tmp.RuleRelation,
                            UserNo = userNumber
                        };
                    }


                    result = DataBaseHelper.QuerySingle<OperateResult>(conn, executeSql, args);
                    if (result.Flag == 0) throw new Exception("编辑职能规则异常");
                }
                trans.Commit();
                return result;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                result.Msg = ex.Message;
                return result;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
        public dynamic GetFunctionRule(Guid vocationId, Guid entityId, Guid funcId)
        {
            var sql = @"
SELECT* from crm_sys_function f LEFT JOIN crm_sys_vocation_function_rule_relation r
on f.funcid = r.functionid AND r.vocationid =@vocationid WHERE entityid = @entityid AND funcid<>@funcid
 AND funccode =(SELECT funccode FROM crm_sys_function WHERE funcid=@funcid)";
            var args = new
            {
                VocationId = vocationId,
                EntityId = entityId,
                FuncId = funcId
            };
            return DataBaseHelper.QuerySingle<dynamic>(sql, args);
        }

        public dynamic GetDeviceTypeFunction(int deviceType, Guid entityId)
        {
            var sql = "select * from crm_sys_function  where rectype=1 and devicetype=@devicetype and entityid=@entityid;";
            var args = new
            {
                Devicetype = deviceType,
                Entityid = entityId
            };
            return DataBaseHelper.QuerySingle<dynamic>(sql, args);
        }


        /// <summary>
        /// 获取功能下的规则
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public List<FunctionRuleQueryMapper> GetFunctionRule(FunctionRuleSelect data, int userNumber)
        {

            var executeSql = @"SELECT * FROM crm_func_vocation_function_rule_select(@vocationid,@functionid,@entityid,@userno)";
            var args = new
            {
                Vocationid = data.VocationId,
                Functionid = data.FunctionId,
                Entityid = data.EntityId,
                UserNo = userNumber
            };

            //var result = DataBaseHelper.Query(executeSql, args);

            var result = DataBaseHelper.QueryStoredProcCursor<FunctionRuleQueryMapper>(executeSql, args, CommandType.Text);
            return result;

        }




        /// <summary>
        /// 获取职能下的用户
        /// </summary>
        /// <param name="page"></param>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public dynamic GetVocationUser(PageParam page, VocationUserSelect data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_vocation_user_select(@deptid,@vocationid,@username,@userno,@pageindex,@pagesize)";
            var args = new
            {
                Vocationid = data.VocationId,
                DeptId = data.DeptId,
                UserName = data.UserName,
                UserNo = userNumber,
                PageIndex = page.PageIndex,
                PageSize = page.PageSize
            };


        


            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor", "pagecursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;



            // var result = DataBaseHelper.Query(executeSql, args);

            //var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            //return result;

        }
        public List<VocationUserInfo> GetVocationUsers(List<Guid> vocationId)
        {
            var sql = string.Format(@"SELECT v.vocationid, u.userid,u.username,u.usericon,u.namepinyin,u.usersex FROM crm_sys_vocation AS v
                                    INNER JOIN crm_sys_userinfo_vocation_relate AS uv ON uv.vocationid=v.vocationid
                                    INNER JOIN crm_sys_userinfo As u ON u.userid=uv.userid
                                    WHERE v.vocationid=ANY (@vocationid);");

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("vocationid", vocationId.ToArray()),


                    };
            return DBHelper.ExecuteQuery<VocationUserInfo>("", sql, param);
        }

        /// <summary>
        /// 获取职能下的用户
        /// </summary>
        /// <param name="page"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<UserInfo> GetVocationUsers(Guid vocationId)
        {
            var sql = string.Format(@"SELECT  u.userid,u.username,u.usericon,u.namepinyin,u.usersex FROM crm_sys_vocation AS v
                                    INNER JOIN crm_sys_userinfo_vocation_relate AS uv ON uv.vocationid=v.vocationid
                                    INNER JOIN crm_sys_userinfo As u ON u.userid=uv.userid
                                    WHERE v.vocationid=@vocationid;");

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("vocationid", vocationId),


                    };
            return DBHelper.ExecuteQuery<UserInfo>("", sql, param);
        }

        /// <summary>
        /// 删除职能下的用户
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult DeleteVocationUser(VocationUserDelete data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_vocation_user_delete(@vocationid,@userid,@userno)";
            var args = new
            {
                Vocationid = data.VocationId,
                Userid = string.Join(",", data.UserIds),
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 根据用户的职能，获取某个用户可用的功能列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public dynamic GetUserFunctions(UserFunctionSelect data)
        {
            var executeSql = @"SELECT * FROM crm_func_functions_of_user(@usernumber,@devicetype,@version)";
            var args = new
            {
                UserNumber = data.UserNumber,
                DeviceType = data.DeviceType,
                Version = data.Version,

            };


            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor", "pagecursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            //return dataResult;


            var items = dataResult["datacursor"];
            var version = dataResult["pagecursor"];

            return new
            {
                items = items,
                version = version
            };

        }


        /// <summary>
        /// 添加功能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult AddFunction(FunctionAdd data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_function_insert(@topfuncid,@funcname,@funccode,@entityid,@devicetype,@userno)";
            var args = new
            {
                TopFuncId = data.TopFuncId,
                FuncName = data.FuncName,
                FuncCode = data.FuncCode,
                EntityId = data.EntityId,
                DeviceType = data.DeviceType,
                UserNo = userNumber
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }
        public OperateResult AddFunction(FunctionInfo data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_function_insert(@islastchild,@routepath,@topfuncid,@funcname,@funccode,@entityid,@devicetype,@rectype,@relationvalue,@userno)";
            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("islastchild", data.IsLastChild));
            sqlParameters.Add(new NpgsqlParameter("routepath", data.RoutePath ?? ""));
            sqlParameters.Add(new NpgsqlParameter("topfuncid", data.ParentId));
            sqlParameters.Add(new NpgsqlParameter("funcname", data.FuncName ?? ""));
            sqlParameters.Add(new NpgsqlParameter("funccode", data.Funccode ?? ""));
            sqlParameters.Add(new NpgsqlParameter("entityid", data.EntityId));
            sqlParameters.Add(new NpgsqlParameter("devicetype", data.DeviceType));
            sqlParameters.Add(new NpgsqlParameter("rectype", (int)data.RecType));
            sqlParameters.Add(new NpgsqlParameter("relationvalue", data.RelationValue??""));
            sqlParameters.Add(new NpgsqlParameter("userno", userNumber));
            return ExecuteQuery<OperateResult>(executeSql, sqlParameters.ToArray()).FirstOrDefault();
        }



        /// <summary>
        /// 编辑功能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult EditFunction(FunctionItemEdit data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_function_update(@funcid,@funcname,@funccode,@userno)";
            var args = new
            {
                FuncId = data.FuncId,
                FuncName = data.FuncName,
                FuncCode = data.FuncCode,
                UserNo = userNumber
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }




        /// <summary>
        /// 删除功能
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public OperateResult DeleteFunction(FunctionItemDelete data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_function_delete(@funcid,@userno)";
            var args = new
            {
                FuncId = data.FuncId,
                UserNo = userNumber
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }




        /// <summary>
        /// 根据职能树
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public dynamic GetFunctionTree(FunctionTreeSelect data)
        {
            var executeSql = @"SELECT * FROM crm_func_function_tree(@topfuncid,@direction)";
            var args = new
            {
                Topfuncid = data.TopFuncId,
                Direction = data.Direction
            };

            var result = DataBaseHelper.Query(executeSql, args);
            return result;
        }

        /// <summary>
        /// 获取功能信息
        /// </summary>
        /// <param name="routePath"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        public FunctionDefineModel GetFunctionDefine(string routePath, Guid entityid)
        {
            var sql = string.Format(@"
                        SELECT f.funcid,f.funcname,f.devicetype,f.rectype,vf.vocationid,vfr.ruleid FROM crm_sys_function AS f
                        LEFT JOIN crm_sys_vocation_function_relation  AS vf ON vf.functionid=f.funcid
                        LEFT JOIN crm_sys_vocation_function_rule_relation vfr ON vfr.functionid=f.funcid AND vfr.vocationid=vf.vocationid
                        WHERE f.routepath=@routepath {0} AND f.recstatus=1
                        ", entityid != Guid.Empty ? "AND f.entityid=@entityid" : "");
            var sqlParameters = new List<DbParameter>();
            sqlParameters.Add(new NpgsqlParameter("routePath", routePath));
            sqlParameters.Add(new NpgsqlParameter("entityid", entityid));
            FunctionDefineModel result = new FunctionDefineModel();
            var temp = DBHelper.ExecuteQuery("", sql, sqlParameters.ToArray());
            if (temp.Count > 0)
            {
                result.FuncId = new Guid(temp.FirstOrDefault()["funcid"].ToString());
                result.FuncName = temp.FirstOrDefault()["funcname"].ToString();
                result.Devicetype = int.Parse(temp.FirstOrDefault()["devicetype"].ToString());
                result.RecType = int.Parse(temp.FirstOrDefault()["rectype"].ToString());
            }

            foreach (var item in temp)
            {
                VocationRuleModel tempData = new VocationRuleModel();
                tempData.VocationId = new Guid(item["vocationid"].ToString());
                tempData.RuleId = item["ruleid"] == null ? (Guid?)null : new Guid(item["ruleid"].ToString());
                result.VocationRules.Add(tempData);
            }


            return result;
        }


        /// <summary>
        /// 获取用户的职能数据
        /// </summary>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public List<VocationInfo> GetUserVocations(int userNumber)
        {


            using (var conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    var vocation_sql = string.Format(@"
                                    SELECT v.vocationid ,v.vocationname,v.description 
                                    FROM crm_sys_vocation AS v
                                    INNER JOIN crm_sys_userinfo_vocation_relate AS u ON u.vocationid=v.vocationid
                                    WHERE v.recstatus=1 AND u.userid=@userNumber;");
                    var vocation_sqlParameters = new List<DbParameter>();
                    vocation_sqlParameters.Add(new NpgsqlParameter("userNumber", userNumber));
                    var vocations = DBHelper.ExecuteQuery<VocationInfo>(tran, vocation_sql, vocation_sqlParameters.ToArray());
                    foreach (var voc in vocations)
                    {
                        // Functions
                        var functions_sql = string.Format(@"
                                SELECT f.funcid,f.funcname,f.funccode,f.parentid,f.entityid,f.devicetype,f.rectype,f.relationvalue,f.routepath 
                                FROM crm_sys_function AS f 
                                WHERE funcid NOT IN (  SELECT functionid FROM crm_sys_vocation_function_relation 
                                WHERE vocationid in (@vocationid) ) and f.recstatus=1 ;");
                        var functions_sqlParameters = new List<DbParameter>();
                        functions_sqlParameters.Add(new NpgsqlParameter("vocationid", voc.VocationId));
                        var fuctions = DBHelper.ExecuteQuery<FunctionInfo>(tran, functions_sql, functions_sqlParameters.ToArray());

                        foreach (var fun in fuctions)
                        {
                            // RuleInfo
                            var rule_sql = string.Format(@"
                                SELECT r.ruleid,r.rulename,r.entityid,r.rulesql 
                                FROM crm_sys_rule AS r
                                INNER JOIN crm_sys_vocation_function_rule_relation AS vr ON vr.ruleid=r.ruleid
                                WHERE r.recstatus=1 AND vr.functionid=@functionid AND vr.vocationid=@vocationid");
                            var rule_sqlParameters = new List<DbParameter>();
                            rule_sqlParameters.Add(new NpgsqlParameter("functionid", fun.FuncId));
                            rule_sqlParameters.Add(new NpgsqlParameter("vocationid", voc.VocationId));
                            var rules = DBHelper.ExecuteQuery<RuleInfo>(tran, rule_sql, rule_sqlParameters.ToArray());
                            fun.Rule = rules.FirstOrDefault();
                        }

                        voc.Functions = fuctions;

                    }

                    return vocations;
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
            }
        }

        /// <summary>
        /// 获取所有的功能信息(不包含rule数据)
        /// </summary>
        /// <returns></returns>
        public List<FunctionInfo> GetTotalFunctions()
        {
            using (var conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {


                    // Functions
                    var functions_sql = string.Format(@"
                                SELECT f.funcid,f.funcname,f.parentid,f.entityid,f.devicetype,f.rectype,f.relationvalue,f.routepath ,f.childtype
                                FROM crm_sys_function AS f
                                WHERE f.recstatus=1 ");
                    var functions_sqlParameters = new List<DbParameter>();

                    var fuctions = DBHelper.ExecuteQuery<FunctionInfo>(tran, functions_sql, functions_sqlParameters.ToArray());

                    return fuctions;



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
            }
        }
        public List<FunctionInfo> GetTotalFunctionsWithStatus0()
        {
            using (var conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {


                    // Functions
                    var functions_sql = string.Format(@"
                                SELECT f.funcid,f.funcname,f.parentid,f.entityid,f.devicetype,f.rectype,f.relationvalue,f.routepath ,f.childtype
                                FROM crm_sys_function AS f
                                 ");
                    var functions_sqlParameters = new List<DbParameter>();

                    var fuctions = DBHelper.ExecuteQuery<FunctionInfo>(tran, functions_sql, functions_sqlParameters.ToArray());

                    return fuctions;



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
            }
        }

        public bool ExplainVocationRulePower(string ruleSql, int userNo)
        {

            using (var conn = DBHelper.GetDbConnect())
            {
                conn.Open();
                var tran = conn.BeginTransaction();
                try
                {
                    var rule_sqlParameters = new List<DbParameter>();
                    var result = DBHelper.ExecuteQuery<dynamic>(tran, ruleSql, rule_sqlParameters.ToArray());
                    return result.Count > 0;
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
            }
        }


        //public bool RepairVocationTreeFoEntity(Guid entityId, int userNo)
        //{
        //    //菜单
        //    string menuSql = @"SELECT funcid,devicetype FROM crm_sys_function WHERE entityid=@entityid AND rectype=2 AND recstatus=1 AND routepath IS NULL";

        //    var result = DataBaseHelper.Query<dynamic>(menuSql);

        //    string entityMenuSql = @"SELECT  menuid::text,menuname from crm_sys_entity_menu WHERE entityid=@entityid and recstatus=1 ;";
        //    var args = new DynamicParameters();
        //    args.Add("entityid", entityId);
        //    var menuList = DataBaseHelper.Query<dynamic>(entityMenuSql, args);

        //    string funcMenuSql = "SELECT relationvalue FROM crm_sys_function where parentid=@parentid and entityid=@entityid;";


        //    menuSql = @" SELECT * FROM crm_func_function_insert(-1,'api/dynamicentity/list', @parentid::uuid,@menuname, 'Menu', @entityid::uuid,@devicetype,0, @menuid::text, @userno);";
        //    DbConnection dbConn = DBHelper.GetDbConnect();
        //    DbTransaction trans = dbConn.BeginTransaction();
        //    foreach (var tmp in result)
        //    {
        //        args = new DynamicParameters();
        //        args.Add("entityid", entityId);
        //        args.Add("entity")
        //        var funcMenuList = DataBaseHelper.Query<dynamic>(funcMenuSql);
        //        foreach (var tmp1 in menuList)
        //        {
        //            var param = new DbParameter[]
        //            {
        //            new NpgsqlParameter("parentid",tmp.funcid),
        //            new NpgsqlParameter("entityid", entityId.ToString()),
        //            new NpgsqlParameter("devicetype", tmp.funcid),
        //            new NpgsqlParameter("menuid",tmp1.menuid),
        //            new NpgsqlParameter("userno", userNo)
        //            };
        //            DBHelper.ExecuteQuery(trans, menuSql, param);
        //        }

        //    }

        //}
    }
}
