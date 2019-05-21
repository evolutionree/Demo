using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.ReportRelation;
using UBeat.Crm.CoreApi.IRepository;
using System.Linq;


namespace UBeat.Crm.CoreApi.Repository.Repository.ReportRelation
{
    public class ReportRelationRepository : RepositoryBase, IReportRelationRepository
    {
        public OperateResult AddReportRelation(AddReportRelationMapper add, DbTransaction dbTran, int userId)
        {
            var sql = "INSERT INTO \"public\".\"crm_sys_reportrelation\" ( \"reportrelationname\", \"reportremark\") VALUES (@reportrelationname,@reportremark);";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("reportrelationname",add.ReportRelationName),
                new NpgsqlParameter("reportremark",add.ReportreMark)
            };
            this.ExecuteNonQuery(sql, param, dbTran);
            return new OperateResult
            {
                Flag = 1,
                Msg = "新增成功"
            };
        }
        public OperateResult UpdateReportRelation(EditReportRelationMapper edit, DbTransaction dbTran, int userId)
        {
            var sql = "update crm_sys_reportrelation set reportrelationname=@reportrelationname, reportremark=@reportremark  where reportrelationid=@reportrelationid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("reportrelationid",edit.ReportRelationId),
                new NpgsqlParameter("reportrelationname",edit.ReportRelationName),
                new NpgsqlParameter("reportremark",edit.ReportreMark)
            };
            int result;
            if (dbTran == null)
                result = DBHelper.ExecuteNonQuery("", sql, param);
            else
                result = DBHelper.ExecuteNonQuery(dbTran, sql, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "编辑成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "编辑失败"
                };
            }
        }

        public OperateResult DeleteReportRelation(DeleteReportRelationMapper delete, DbTransaction dbTran, int userId)
        {
            var sql = "update  crm_sys_reportrelation set recstatus=@recstatus where reportrelationid=@reportrelationid";
            // var existSql = "select count(1) from crm_sys_analyse_func as f where  EXISTS(select 1 from crm_sys_analyse_func_active where recstatus=1 and /anafuncid=f.anafuncid) " + "AND anafuncid = @anafuncid";

            foreach (var tmp in delete.ReportRelationIds)
            {
                var param1 = new DbParameter[]
                {
                  new NpgsqlParameter("reportrelationid", tmp),
                  new NpgsqlParameter("recstatus", delete.RecStatus)
                };
                int result;
                //if (dbTran == null)
                //    result = DBHelper.ExecuteQuery<int>("", existSql, param1).FirstOrDefault();
                //else
                //    result = DBHelper.ExecuteQuery<int>(dbTran, existSql, param1).FirstOrDefault();
                //if (result > 0)
                //{
                //    return new OperateResult
                //    {
                //        Flag = 0,
                //        Msg = "函数被引用"
                //    };
                //}
                result = 0;
                if (dbTran == null)
                    result = DBHelper.ExecuteNonQuery("", sql, param1);
                else
                    result = DBHelper.ExecuteNonQuery(dbTran, sql, param1);
                if (result < 0)
                {
                    return new OperateResult
                    {
                        Flag = 0,
                        Msg = "操作失败"
                    };
                }
            }
            return new OperateResult
            {
                Flag = 1,
                Msg = "操作成功"
            };
        }

        public PageDataInfo<Dictionary<string, object>> GetReportRelationListData(QueryReportRelationMapper mapper, DbTransaction dbTran, int userId)
        {
            var sql = " select * from crm_sys_reportrelation where recstatus=1 {0} {1}";
            DbParameter[] param = new DbParameter[mapper.ColumnFilter.Count];
            string conditionSql = String.Empty;
            int index = 0;
            foreach (var tmp in mapper.ColumnFilter)
            {
                if (tmp.Value == null || string.IsNullOrEmpty(tmp.Value.ToString()))
                {
                    param[index] = new NpgsqlParameter(tmp.Key, tmp.Value);
                }
                else
                {
                    conditionSql += string.Format(" and {0}  ILIKE '%' || @{1} || '%' ESCAPE '`' ", tmp.Key, tmp.Key);
                    param[index] = new NpgsqlParameter(tmp.Key, tmp.Value);
                }
                index++;
            }
            sql = string.Format(sql, conditionSql, (!string.IsNullOrEmpty(mapper.SearchOrder) ? " order by " + mapper.SearchOrder : string.Empty));
            if (dbTran == null)
                return ExecuteQueryByPaging(sql, param, mapper.PageSize, mapper.PageIndex);
            var result = ExecuteQueryByPaging(sql, param, mapper.PageSize, mapper.PageIndex, dbTran);
            return result;
        }

        public List<EditReportRelDetailMapper> GetReportRelDetail(QueryReportRelDetailMapper mapper, DbTransaction dbTran, int userId)
        {
            var sql = " select * from crm_sys_reportreldetail where 1=1 {0} limit 1";
            var param = new DbParameter[]
           {
               new NpgsqlParameter("reportrelationname",mapper.Name),
               new NpgsqlParameter("reportrelationid",mapper.ReportRelationId.Value),
               new NpgsqlParameter("userid",mapper.UserId.Value)
           };
            string conditionSql = String.Empty;
            if (mapper.ReportRelationId.HasValue)
            {
                conditionSql = " and reportrelationid=@reportrelationid ";
            }
            if (mapper.UserId.HasValue)
            {
                conditionSql = string.IsNullOrEmpty(conditionSql) ? " and @userid::int4 in (select regexp_split_to_table(reportuser,',')::int4) " : conditionSql + "  and @userid::int4 in (select regexp_split_to_table(reportuser,',')::int4)  ";
            }
            sql = string.Format(sql, conditionSql, mapper.SearchOrder);
            if (dbTran == null)
                return DBHelper.ExecuteQuery<EditReportRelDetailMapper>("", sql, param);

            var result = DBHelper.ExecuteQuery<EditReportRelDetailMapper>(dbTran, sql, param);
            return result;
        }


        public OperateResult AddReportRelDetail(AddReportRelDetailMapper add, DbTransaction dbTran, int userId)
        {
            var sql = "INSERT INTO \"public\".\"crm_sys_reportreldetail\" ( \"reportrelationid\", \"reportuser\", \"reportleader\") VALUES (@reportrelationid,@reportuser,@reportleader);";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("reportrelationid",add.ReportRelationId),
                new NpgsqlParameter("reportuser",add.ReportUser),
                new NpgsqlParameter("reportleader",add.ReportLeader),
            };
            this.ExecuteNonQuery(sql, param, dbTran);
            return new OperateResult
            {
                Flag = 1,
                Msg = "新增成功"
            };
        }
        public OperateResult UpdateReportRelDetail(EditReportRelDetailMapper edit, DbTransaction dbTran, int userId)
        {
            var sql = "update crm_sys_reportreldetail set reportuser=@reportuser, reportleader=@reportleader  where reportreldetailid=@reportreldetailid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("reportreldetailid",edit.ReportRelDetailId),
                new NpgsqlParameter("reportuser",edit.ReportUser),
                new NpgsqlParameter("reportleader",edit.ReportLeader),
            };
            int result;
            if (dbTran == null)
                result = DBHelper.ExecuteNonQuery("", sql, param);
            else
                result = DBHelper.ExecuteNonQuery(dbTran, sql, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "编辑成功"
                };
            }
            else
            {
                return new OperateResult
                {
                    Msg = "编辑失败"
                };
            }
        }

        public OperateResult DeleteReportRelDetail(DeleteReportRelDetailMapper delete, DbTransaction dbTran, int userId)
        {
            var sql = "update  crm_sys_reportreldetail set recstatus=@recstatus where reportreldetailid=@reportreldetailid";
            // var existSql = "select count(1) from crm_sys_analyse_func as f where  EXISTS(select 1 from crm_sys_analyse_func_active where recstatus=1 and /anafuncid=f.anafuncid) " + "AND anafuncid = @anafuncid";

            foreach (var tmp in delete.ReportRelDetailIds)
            {
                var param1 = new DbParameter[]
                {
                  new NpgsqlParameter("reportreldetailid", tmp),
                  new NpgsqlParameter("recstatus", delete.RecStatus)
                };
                int result;
                //if (dbTran == null)
                //    result = DBHelper.ExecuteQuery<int>("", existSql, param1).FirstOrDefault();
                //else
                //    result = DBHelper.ExecuteQuery<int>(dbTran, existSql, param1).FirstOrDefault();
                //if (result > 0)
                //{
                //    return new OperateResult
                //    {
                //        Flag = 0,
                //        Msg = "函数被引用"
                //    };
                //}
                result = 0;
                if (dbTran == null)
                    result = DBHelper.ExecuteNonQuery("", sql, param1);
                else
                    result = DBHelper.ExecuteNonQuery(dbTran, sql, param1);
                if (result < 0)
                {
                    return new OperateResult
                    {
                        Flag = 0,
                        Msg = "操作失败"
                    };
                }
            }
            return new OperateResult
            {
                Flag = 1,
                Msg = "操作成功"
            };
        }

        public PageDataInfo<Dictionary<string, object>> GetReportRelDetailListData(QueryReportRelDetailMapper mapper, DbTransaction dbTran, int userId)
        {
            var sql = "select tmp1.*,tmp2.reportuser,tmp2.reportuser_name from (\n" +
" select * from ( select  tmp.reportreldetailid,tmp.reportrelationid,tmp.reportleader,\n" +
"array_to_string(array_agg(tmp.reportleader_name),',') as  \n" +
"reportleader_name from ( \n" +
"select t1.reportreldetailid,t1.reportrelationid,t1.reportleader, \n" +
"t1.username AS reportleader_name from ( \n" +
"select d.*,u.username from (select *,regexp_split_to_table(reportleader,',')::int4 as reportleaderid from  \n" +
"crm_sys_reportreldetail where  recstatus=1) as d LEFT JOIN crm_sys_userinfo u on u.userid=d.reportleaderid \n" +
") as t1  \n" +
" ) as tmp \n" +
"  where 1=1   \n" +
" GROUP BY tmp.reportreldetailid,tmp.reportrelationid,tmp.reportleader ) as tmp1\n" +
") as tmp1 LEFT JOIN\n" +
"(\n" +
" select * from ( select  tmp.reportreldetailid,tmp.reportrelationid,tmp.reportuser,\n" +
"array_to_string(array_agg(tmp.reportuser_name),',') as  \n" +
"reportuser_name from ( \n" +
"select t1.reportreldetailid,t1.reportrelationid,t1.reportuser, \n" +
"t1.username AS reportuser_name from ( \n" +
"select d.*,u.username from (select *,regexp_split_to_table(reportuser,',')::int4 as reportuserid from  \n" +
"crm_sys_reportreldetail where  recstatus=1 ) as d LEFT JOIN crm_sys_userinfo u on u.userid=d.reportuserid \n" +
") as t1  \n" +
" ) as tmp \n" +
"  where 1=1   \n" +
" GROUP BY tmp.reportreldetailid,tmp.reportrelationid,tmp.reportuser ) as tmp1\n" +
") tmp2  on  tmp2.reportreldetailid=tmp1.reportreldetailid where 1=1 {0} {1}";
            var param = new DbParameter[mapper.ColumnFilter.Count];
            string conditionSql = String.Empty;
            int index = 0;
            foreach (var tmp in mapper.ColumnFilter)
            {
                if (tmp.Value == null || string.IsNullOrEmpty(tmp.Value.ToString()))
                {
                    param[index] = new NpgsqlParameter(tmp.Key, tmp.Value);
                }
                else
                {
                    conditionSql += string.Format(" and {0}_name  ILIKE '%' || @{1}_name || '%' ESCAPE '`' ", tmp.Key, tmp.Key);
                    param[index] = new NpgsqlParameter(tmp.Key + "_name", tmp.Value);
                }
                index++;
            }
            sql = string.Format(sql, conditionSql, (!string.IsNullOrEmpty(mapper.SearchOrder) ? " order by " + mapper.SearchOrder : string.Empty));
            if (dbTran == null)
                return ExecuteQueryByPaging(sql, param, mapper.PageSize, mapper.PageIndex);
            var result = ExecuteQueryByPaging(sql, param, mapper.PageSize, mapper.PageIndex, dbTran);
            return result;
        }

    }
}