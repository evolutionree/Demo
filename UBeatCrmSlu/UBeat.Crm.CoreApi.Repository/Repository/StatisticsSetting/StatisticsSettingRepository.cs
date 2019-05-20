using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.StatisticsSetting;
using UBeat.Crm.CoreApi.IRepository;
using System.Linq;
namespace UBeat.Crm.CoreApi.Repository.Repository.StatisticsSetting
{
    public class StatisticsSettingRepository : RepositoryBase, IStatisticsSettingRepository
    {
        public OperateResult AddStatisticsSetting(AddStatisticsSettingMapper add, DbTransaction dbTran, int userId)
        {
            var sql = "INSERT INTO \"public\".\"crm_sys_analyse_func\" ( \"anafuncname\", \"moreflag\", \"countfunc\", \"morefunc\",  \"entityid\", \"allowinto\", \"anafuncname_lang\",\"reccreator\",\"recupdator\") VALUES (@anafuncname,@moreflag,@countfunc,@morefunc,@entityid::uuid,@allowinto,@anafuncname_lang::jsonb,@userno,@userno);";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("anafuncname",add.AnaFuncName),
                new NpgsqlParameter("moreflag",add.MoreFlag),
                new NpgsqlParameter("countfunc",add.CountFunc),
                new NpgsqlParameter("morefunc",add.MoreFunc),
                new NpgsqlParameter("entityid",add.EntityId),
                new NpgsqlParameter("allowinto",add.AllowInto),
                new NpgsqlParameter("anafuncname_lang",add.AnaFuncName_Lang),
                new NpgsqlParameter("userno",userId),
            };
            this.ExecuteNonQuery(sql, param, dbTran);
            return new OperateResult
            {
                Flag = 1,
                Msg = "新增成功"
            };
        }
        public OperateResult UpdateStatisticsSetting(EditStatisticsSettingMapper edit, DbTransaction dbTran, int userId)
        {
            var sql = "update crm_sys_analyse_func set anafuncname=@anafuncname, moreflag=@moreflag, countfunc=@countfunc, morefunc=@morefunc, entityid=@entityid::uuid, allowinto=@allowinto, anafuncname_lang=@anafuncname_lang::jsonb,recupdator=@userno,recversion=recversion+1 where anafuncid=@anafuncid";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("anafuncid",edit.AnaFuncId),
                new NpgsqlParameter("anafuncname",edit.AnaFuncName),
                new NpgsqlParameter("moreflag",edit.MoreFlag),
                new NpgsqlParameter("countfunc",edit.CountFunc),
                new NpgsqlParameter("morefunc",edit.MoreFunc),
                new NpgsqlParameter("entityid",edit.EntityId),
                new NpgsqlParameter("allowinto",edit.AllowInto),
                new NpgsqlParameter("anafuncname_lang",edit.AnaFuncName_Lang),
                 new NpgsqlParameter("userno",userId),
            };
            int result;
            if (dbTran == null)
                result = DBHelper.ExecuteNonQuery("", sql, param);
            else
                result = DBHelper.ExecuteNonQuery(dbTran, sql, param);
            if (result >= 0)
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

        public OperateResult DisabledStatisticsSetting(DeleteStatisticsSettingMapper delete, DbTransaction dbTran, int userId)
        {
            var sql = "update  crm_sys_analyse_func set recstatus=@recstatus,recversion=recversion+1  where anafuncid=@anafuncid";
            var existSql = "select count(1) from crm_sys_analyse_func as f where  EXISTS(select 1 from crm_sys_analyse_func_active where recstatus=1 and anafuncid=f.anafuncid) " + "AND anafuncid = @anafuncid";


            foreach (var tmp in delete.AnaFuncIds)
            {
                var param = new DbParameter[]
                {
                    new NpgsqlParameter("anafuncid", tmp),
                    new NpgsqlParameter("recstatus",delete.RecStatus)
                };
                int result;
                if (delete.RecStatus == 0)
                {
                    var param1 = new DbParameter[]
                    {
                  new NpgsqlParameter("anafuncid", tmp)
                    };

                    if (dbTran == null)
                        result = DBHelper.ExecuteQuery<int>("", existSql, param1).FirstOrDefault();
                    else
                        result = DBHelper.ExecuteQuery<int>(dbTran, existSql, param1).FirstOrDefault();
                    if (result > 0)
                    {
                        return new OperateResult
                        {
                            Flag = 0,
                            Msg = "函数被引用"
                        };
                    }
                }
                result = 0;
                if (dbTran == null)
                    result = DBHelper.ExecuteNonQuery("", sql, param);
                else
                    result = DBHelper.ExecuteNonQuery(dbTran, sql, param);
                if (result <= 0)
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

        public OperateResult DeleteStatisticsSetting(DeleteStatisticsSettingMapper delete, DbTransaction dbTran, int userId)
        {
            var sql = "delete from  crm_sys_analyse_func where anafuncid=@anafuncid";
            var existSql = "select count(1) from crm_sys_analyse_func as f where  EXISTS(select 1 from crm_sys_analyse_func_active where recstatus=1 and anafuncid=f.anafuncid) " + "AND anafuncid = @anafuncid";

            foreach (var tmp in delete.AnaFuncIds)
            {
                var param1 = new DbParameter[]
                {
                  new NpgsqlParameter("anafuncid", tmp)
                };
                int result;
                if (dbTran == null)
                    result = DBHelper.ExecuteQuery<int>("", existSql, param1).FirstOrDefault();
                else
                    result = DBHelper.ExecuteQuery<int>(dbTran, existSql, param1).FirstOrDefault();
                if (result > 0)
                {
                    return new OperateResult
                    {
                        Flag = 0,
                        Msg = "函数被引用"
                    };
                }
                result = 0;
                if (dbTran == null)
                    result = DBHelper.ExecuteNonQuery("", sql, param1);
                else
                    result = DBHelper.ExecuteNonQuery(dbTran, sql, param1);
                if (result <= 0)
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

        public List<Dictionary<string, object>> GetStatisticsListData(QueryStatisticsSettingMapper mapper, DbTransaction dbTran, int userId)
        {
            var sql = "select \n" +
                            "anafuncid,\n" +
                            "anafuncname,\n" +
                            "anafuncname_lang,\n" +
                            "CASE WHEN moreflag=0 THEN	'不跳' ELSE '跳转' END moreflag_name,\n" +
                            "morefunc,\n" +
                            "moreflag,\n" +
                            "f.reccreated,\n" +
                            "f.recupdated,\n" +
                            "entity.entityname,\n" +
                            "entity.entityid,\n" +
                            "allowinto,f.recstatus,f.countfunc," +
                            "CASE WHEN allowinto=0 THEN	'允许进入' ELSE '不允许进入' END allowinto_name\n" +
                            " from crm_sys_analyse_func f\n" +
                            "LEFT JOIN crm_sys_entity entity on entity.entityid=f.entityid {0}";
            var param = new DbParameter[]
           {
               new NpgsqlParameter("anafuncname",mapper.AnaFuncName),
           };
            string conditionSql = String.Empty;
            if (!string.IsNullOrEmpty(mapper.AnaFuncName))
            {
                conditionSql = " where f.anafuncname ilike '%@anafuncname%' ";
            }
            sql = string.Format(sql, conditionSql);
            if (dbTran == null)
                return DBHelper.ExecuteQuery("", sql, param);

            var result = DBHelper.ExecuteQuery(dbTran, sql, param);
            return result;
        }

        public List<Dictionary<string, object>> GetStatisticsData(QueryStatisticsMapper mapper, DbTransaction dbTran, int userId)
        {
            var sql = "select groupmark,groupmark_lang from crm_sys_analyse_func_active where recstatus=1 GROUP BY groupmark,groupmark_lang ";
            var param = new DbParameter[]
           {
               new NpgsqlParameter("anafuncname",mapper.AnaFuncName),
           };
            if (dbTran == null)
                return DBHelper.ExecuteQuery("", sql, param);

            var result = DBHelper.ExecuteQuery(dbTran, sql, param);
            return result;
        }

        public List<Dictionary<string, object>> GetStatisticsDetailData(QueryStatisticsMapper mapper, DbTransaction dbTran, int userId)
        {
            var sql = " select * from crm_sys_analyse_func where anafuncid in (select anafuncid from crm_sys_analyse_func_active where recstatus = 1 and groupmark =@groupmark) ";
            var param = new DbParameter[]
           {
               new NpgsqlParameter("groupmark",mapper.GroupName),
           };
            if (dbTran == null)
                return DBHelper.ExecuteQuery("", sql, null);

            var result = DBHelper.ExecuteQuery(dbTran, sql, param);
            return result;
        }


        public OperateResult UpdateStatisticsGroupSetting(EditStatisticsGroupMapper edit, DbTransaction dbTran, int userId)
        {
            var sql = "update crm_sys_analyse_func_active set groupmark=@newgroupmark,groupmark_lang=@groupmark_lang::jsonb,recversion=recversion+1 where groupmark=@groupmark";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("groupmark",edit.GroupName),
                new NpgsqlParameter("newgroupmark",edit.NewGroupName),
                new NpgsqlParameter("groupmark_lang",edit.NewGroupName_Lang)
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
        public OperateResult SaveStatisticsGroupSumSetting(SaveStatisticsGroupMapper save, DbTransaction dbTran, int userId)
        {
            var sql = "update  crm_sys_analyse_func_active set recstatus=0  where groupmark=@groupmark";
            var newSql = "INSERT INTO \"public\".\"crm_sys_analyse_func_active\" (\"anafuncid\", \"recorder\", \"groupmark\", \"reccreator\", \"recupdator\",  \"groupmark_lang\") \n" +
 "VALUES (@anafuncid,@recorder,@groupmark,@userno,@userno,@groupmark_lang::jsonb);";
            var param = new DbParameter[]
            {
                new NpgsqlParameter("groupmark",save.Data.FirstOrDefault().GroupName),
            };
            int result;
            if (dbTran == null)
                result = DBHelper.ExecuteNonQuery("", sql, param);
            else
                result = DBHelper.ExecuteNonQuery(dbTran, sql, param);
            if (result >= 0)
            {
                if (save.IsDel == 0)
                {
                    foreach (var tmp in save.Data)
                    {
                        param = new DbParameter[]
                    {
                new NpgsqlParameter("groupmark",tmp.GroupName),
                new NpgsqlParameter("recorder",tmp.RecOrder),
                new NpgsqlParameter("anafuncid",tmp.AnafuncId==null?null:tmp.AnafuncId),
                new NpgsqlParameter("userno",userId),
                new NpgsqlParameter("groupmark_lang",tmp.GroupName_Lang)
                    };
                        if (dbTran == null)
                            result = DBHelper.ExecuteNonQuery("", newSql, param);
                        else
                            result = DBHelper.ExecuteNonQuery(dbTran, newSql, param);
                        if (result <= 0)
                        {
                            throw new Exception("保存分组异常");
                        }
                    }
                }
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

    }
}
