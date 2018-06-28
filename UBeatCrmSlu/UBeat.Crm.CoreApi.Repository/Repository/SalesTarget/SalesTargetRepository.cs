using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.SalesTarget;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;
using System.Data.Common;
using Npgsql;
using UBeat.Crm.CoreApi.Repository.Repository;

namespace UBeat.Crm.CoreApi.Repository.Repository.SalesTarget
{
    public class SalesTargetRepository : RepositoryBase, ISalesTargetRepository
    {
        /// <summary>
        /// 获取销售目标列表
        /// </summary>
        /// <returns></returns>
        public dynamic GetSalesTargets(PageParam page, SalesTargetSelectMapper data, int userNumber)
        {

            var executeSql = @"SELECT * FROM crm_func_sales_target_select(@year,@normtypeid,@departmentid,@searchname,@userno,@pageindex,@pagesize)";
            var args = new
            {
                Year = data.Year,
                NormTypeId = data.NormTypeId,
                DepartmentId = data.DepartmentId,
                SearchName = data.SearchName,
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
        /// 编辑销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult EditSalesTarget(SalesTargetEditMapper data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_record_update(@taregtid,@yearcount,@jancount,@febcount,@marcount,
                                                                                 @aprcount,@maycount,@juncount,@julcount,@augcount,
                                                                                 @sepcount,@octcount,@novcount,@deccount,@userno)";
            var args = new
            {
                TaregtId = data.TargetId,
                YearCount = data.YearCount,
                JanCount = data.JanCount,
                FebCount = data.FebCount,
                MarCount = data.MarCount,
                AprCount = data.AprCount,
                MayCount = data.MarCount,
                JunCount = data.JunCount,
                JulCount = data.JulCount,
                AugCount = data.AugCount,
                SepCount = data.SepCount,
                OctCount = data.OctCount,
                NovCount = data.NovCount,
                DecCount = data.DecCount,
                UserNo = userNumber
            };





            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 插入销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult InsertSalesTarget(SalesTargetInsertMapper data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_record_insert(@year,@jancount,@febcount,@marcount,
                                                                                @aprcount,@maycount,@juncount,@julcount,@augcount,
                                                                                @sepcount,@octcount,@novcount,@deccount,@userid,
                                                                               @departmentid, @isgrouptarget,@userno,@normtypeid)";
            var args = new
            {
                Year = data.Year,
                JanCount = data.JanCount,
                FebCount = data.FebCount,
                MarCount = data.MarCount,
                AprCount = data.AprCount,
                MayCount = data.MarCount,
                JunCount = data.JunCount,
                JulCount = data.JulCount,
                AugCount = data.AugCount,
                SepCount = data.SepCount,
                OctCount = data.OctCount,
                NovCount = data.NovCount,
                DecCount = data.DecCount,
                UserId = data.UserId,
                DepartmentId = data.DepartmentId,
                IsGroupTarget = data.IsGroupTarget,
                UserNo = userNumber,
                NormTypeId = data.NormTypeId,
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 获取销售目标明细
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumbe"></param>
        /// <returns></returns>
        public dynamic GetSalesTargetDetail(SalesTargetSelectDetailMapper data, int userNumbe)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_detail(@departmentid,@userid,@normtypeid,@isgrouptarget,@year,@userno)";
            var args = new
            {
                DepartmentId = data.DepartmentId,
                UserId = data.UserId,
                NormTypeId = data.NormTypeId,
                IsGroupTarget = data.IsGroupTarget,
                Year = data.Year,
                UserNo = userNumbe
            };

            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor", "pagecursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);


            var fields = dataResult["datacursor"];
            var visible = dataResult["pagecursor"];

            List<VisibleItem> values = new List<VisibleItem>();

            foreach (IDictionary<string, object> kv in visible)
            {
                string[] strArray = kv["unnest"].ToString().Split(':');
                values.Add(new VisibleItem()
                {
                    Name = strArray[0],
                    Value = strArray[1]
                });
            }

            return new
            {
                detail = fields,
                visible = values
            };
        }

        public OperateResult SetBeginMoth(SalesTargetSetBeginMothMapper data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_beginmonth_set(@beginyear,@beginmonth,@departmentid,@userid,@userno)";
            var args = new
            {
                BeginYear = data.BeginYear,
                BeginMonth = data.BeginMonth,
                DepartmentId = data.DepartmentId,
                UserId=data.UserId,
                UserNo = userNumber
            };
            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 新增销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult InsertSalesTargetNormType(SalesTargetNormTypeMapper data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_norm_type_save(@normtypeid,@normtypename,@entityid,@fieldname,@calcutetype,@userno,@reclanguage::jsonb)";

            var args = new
            {
                NormTypeId = data.Id,
                NormTypeName = data.Name,
                EntityId = data.EntityId,
                FieldName = data.FieldName,
                CalcuteType = data.CaculateType,
                UserNo = userNumber,
                reclanguage = data.RecLanguage
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 删除销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult DeleteSalesTargetNormType(SalesTargetNormTypeDeleteMapper data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_norm_type_delete(@normtypeid,@userno)";

            var args = new
            {
                NormTypeId = data.Id,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 获取销售目标列表
        /// </summary>
        /// <returns></returns>
        public dynamic GetTargetNormTypeList()
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_norm_type_select()";
            var args = new { };

            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;
        }


        /// <summary>
        /// 新增销售目标规则
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult InsertSalesTargetNormTypeRule(SalesTargetNormTypeMapper data, SalesTargetNormRuleInsertMapper normRule, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_norm_type_rule_save(@normtypeid,@normtypename,@entityid,@fieldname,@bizdatefieldname,@calcutetype,
                                                                                       @isdefaultrule,@typeid,@rule,@ruleitem,@ruleset,
                                                                                       @rulerelation,@userno)";

            var args = new
            {
                NormTypeId = data.Id,
                NormTypeName = string.Empty, //data.Name,
                EntityId = data.EntityId,
                FieldName = data.FieldName,
                BizDateFieldName = data.BizDateFieldName,
                CalcuteType = data.CaculateType,

                IsDefaultRule = 1,
                TypeId = 1,

                Rule = normRule.Rule,
                RuleItem = normRule.RuleItem,
                RuleSet = normRule.RuleSet,
                RuleRelation = normRule.RuleRelation,

                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }



        /// <summary>
        /// 编辑销售目标规则
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult EditSalesTargetNormTypeRule(SalesTargetNormTypeMapper data, SalesTargetNormRuleInsertMapper normRule, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_norm_type_rule_edit(@normtypeid,@normtypename,@entityid,@fieldname,@bizdatefieldname,@calcutetype,
                                                                                       @rule,@ruleitem,@ruleset, @rulerelation,@userno)";
            var args = new
            {
                NormTypeId = data.Id,
                NormTypeName = data.Name,
                EntityId = data.EntityId,
                FieldName = data.FieldName,
                BizDateFieldName = data.BizDateFieldName,
                CalcuteType = data.CaculateType,
                Rule = normRule.Rule,
                RuleItem = normRule.RuleItem,
                RuleSet = normRule.RuleSet,
                RuleRelation = normRule.RuleRelation,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }



        /// <summary>
        /// 获取销售指标明细
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userNumbe"></param>
        /// <returns></returns>
        public List<SalesTargetNormRuleMapper> GetSalesTargetNormDetail(SalesTargetNormTypeDetailMapper data, int userNumbe)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_norm_type_detail(@normtypeid,@userno)";
            var args = new
            {
                NormTypeId = data.Id,
                UserNo = userNumbe,
            };


            // var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            //var dataNames = new List<string> { "datacursor" };

            var dataResult = DataBaseHelper.QueryStoredProcCursor<SalesTargetNormRuleMapper>(executeSql, args, CommandType.Text);
            return dataResult;
        }






        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <returns></returns>
        public dynamic GetEntityList()
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_entity_select()";
            var args = new { };

            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;
        }

        public dynamic GetSalesTargetDept(int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_import_dept_data(@userno)";
            var args = new
            {
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;

        }

        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <returns></returns>
        public dynamic GetEntityFields(Guid id,int fieldtype)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_entity_field_select(@entityid,@fieldtype)";
            var args = new { entityid = id , fieldtype = fieldtype };

            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;
        }

        /// <summary>
        /// 获取下级团队和本团队成员
        /// </summary>
        /// <returns></returns>
        public dynamic GetSubDepartmentAndUser(Guid deptId, int userNumbe)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_get_department_and_user(@deptmentid,@userno)";
            var args = new { deptmentid = deptId, userno = userNumbe };

            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;
        }







        /// <summary>
        /// 分配年度销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult SaveYearSalesTarget(YearSalesTargetSaveMapper data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_year_target_insert(@id,@departmentid,@isgroup,@yeartarget,@year,@normtypeid,@userno)";

            var args = new
            {
                Id = data.Id,
                DepartmentId=data.DepartmentId,
                IsGroup = data.IsGroup,
                YearTarget = data.YearCount,
                Year = data.Year,
                NormTypeId = data.NormTypeId,
                UserNo = userNumber
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 获取下级团队和本团队成员的年度销售目标
        /// </summary>
        /// <returns></returns>
        public dynamic GetSubDepartmentAndUserYearSalesTarget(string id, int isGroup, int year, Guid normTypeId, int userNumbe)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_year_target_select(@id,@isgroup,@year,@normtypeid,@userno)";
            var args = new
            {
                Id = id,
                IsGroup = isGroup,
                Year = year,
                NormTypeId = normTypeId,
                UserNo = userNumbe
            };




            var result = DataBaseHelper.QueryStoredProcCursor(executeSql, args, CommandType.Text);
            var dataNames = new List<string> { "datacursor" };
            var dataResult = DataBaseHelper.QueryStoredProcCursor(executeSql, dataNames, args, CommandType.Text);
            return dataResult;
        }



        /// <summary>
        /// 保存销售目标
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public OperateResult SaveSalesTarget(SalesTargetInsertMapper data, int userNumber)
        {
            var executeSql = @"SELECT * FROM crm_func_sales_target_record_save(@year,@userid,@departmentid,@isgrouptarget,@normtypeid,
                                                                               @jancount,@febcount,@marcount,@aprcount,@maycount,
                                                                               @juncount,@julcount,@augcount,@sepcount,@octcount,
                                                                               @novcount,@deccount,@userno)";
            var args = new
            {
                Year = data.Year,
                UserId = data.UserId,
                DepartmentId = data.DepartmentId,
                IsGroupTarget = data.IsGroupTarget,
                NormTypeId = data.NormTypeId,
                JanCount = data.JanCount,
                FebCount = data.FebCount,
                MarCount = data.MarCount,
                AprCount = data.AprCount,
                MayCount = data.MayCount,
                JunCount = data.JunCount,
                JulCount = data.JulCount,
                AugCount = data.AugCount,
                SepCount = data.SepCount,
                OctCount = data.OctCount,
                NovCount = data.NovCount,
                DecCount = data.DecCount,
                UserNo = userNumber,
            };

            return DataBaseHelper.QuerySingle<OperateResult>(executeSql, args);
        }


        /// <summary>
        /// 销售目标是否已经存在
        /// </summary>
        /// <param name="id"></param>
        /// <param name="year"></param>
        /// <param name="isGroup"></param>
        /// <param name="normTypeId"></param>
        /// <param name="userNumber"></param>
        /// <returns></returns>
        public bool IsSalesTargetExists(string id, int year, bool isGroup, Guid normTypeId, int userNumber)
        {
            string strSql = string.Empty;
            if (isGroup)
            {
                strSql = @" SELECT COUNT(1) 
                               FROM crm_sys_sales_target 
                               WHERE departmentid=@id::uuid
                               AND year=@year 
                               AND normtypeid=@normtypeid 
                               AND isgrouptarget=true ";
            }
            else
            {
                strSql = @" SELECT COUNT(1) 
                               FROM crm_sys_sales_target 
                               WHERE userid=@id::integer 
                               AND year=@year 
                               AND normtypeid=@normtypeid 
                               AND isgrouptarget=false ";
            }


            var param = new
            {
                Id = id,
                Year = year,
                NormTypeId = normTypeId,
            };

            var count = DataBaseHelper.QuerySingle<int>(strSql, param, CommandType.Text);
            if (count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }


        public bool InsertSalesTarget(string id, int year, bool isGroup, Guid normTypeId, decimal yearCount, int userNumber)
        {
            string strSql = string.Empty;

            if (isGroup)
            {
                strSql = @" INSERT INTO crm_sys_sales_target (year,yearcount,departmentid,normtypeid,reccreator,recupdator,recmanager,isgrouptarget)
		                    VALUES(@year,@yearcount,@id::uuid,@normtypeid,@userno,@userno,@userno,true) ";
            }
            else
            {

                strSql = @"INSERT INTO crm_sys_sales_target (year,yearcount,userid,normtypeid,reccreator,recupdator,recmanager,isgrouptarget,departmentid)
			               VALUES(@year,@yearcount,@id::integer,@normtypeid,@userno,@userno,@userno,false,'00000000-0000-0000-0000-000000000000')";
            }

            var param = new
            {
                Id = id,
                Year = year,
                NormTypeId = normTypeId,
                YearCount=yearCount,
                UserNo=userNumber,
            };

            int result = DataBaseHelper.ExecuteNonQuery(strSql, param, CommandType.Text);
            if (result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public bool UpdateSalesTarget(string id, int year, bool isGroup, Guid normTypeId, decimal yearCount, int userNumber) {

            string strSql = string.Empty;

            if (isGroup)
            {
                strSql = @"  UPDATE crm_sys_sales_target
		                     SET yearcount=@yearcount
		                     WHERE departmentid=@id::uuid
		                     AND year=@year
		                     AND normtypeid=@normtypeid;  ";
            }
            else
            {

                strSql = @"  UPDATE crm_sys_sales_target 
		                     SET yearcount=@yearcount
		                     WHERE userid=@id::integer
		                     AND year=@year
		                     AND normtypeid=@normtypeid; ";
            }

            var param = new
            {
                Id = id,
                Year = year,
                NormTypeId = normTypeId,
                YearCount = yearCount,
                UserNo = userNumber,
            };

            int result = DataBaseHelper.ExecuteNonQuery(strSql, param, CommandType.Text);
            if (result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

		public dynamic GetCurYearALlTargetList(int year)
		{
			var executeSql = @"select t.* from crm_sys_sales_target t 
								left  join crm_sys_department d on d.deptid = t.departmentid
								where t.recstatus = 1 and isgrouptarget = true and d.recstatus = 1
								and t.year = @year
								union

								select t.* from crm_sys_sales_target t
								left join crm_sys_userinfo u on u.userid = t.userid
									  left JOIN crm_sys_account_userinfo_relate r on r.userid = t.userid
								where t.recstatus = 1 and isgrouptarget = false and u.recstatus = 1
								and r.deptid = t.departmentid
								and t.year = @year; ";
			var args = new {
				year = year
			};

			var result = DataBaseHelper.Query(executeSql, args, CommandType.Text);

			return result;
		}
	}
}
