using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.ScheduleTask;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.ScheduleTask
{

    public class ScheduleTaskRepository : RepositoryBase, IScheduleTaskRepository
    {

        public ScheduleTaskCountMapper GetScheduleTaskCount(ScheduleTaskListMapper mapper, int userId, DbTransaction trans = null)
        {
            var sql = @"select to_char(daytime::date, 'yyyy-MM-dd') as daytime,COALESCE(tmp.unfinishedschedule,0) as unfinishedschedule,COALESCE(tmp1.unfinishedtask,0) as unfinishedtask  from generate_series((@starttime::date),
            (@endtimetmp::date),'1 day'
            ) s(daytime) LEFT JOIN (select generate_series((starttime::date),
(endtime::date),'1 day'
) as datetime,count(1) as unfinishedschedule from crm_sys_schedule  where {0} and recid not in (select recid from crm_sys_schedule where endtime<@starttime OR starttime>@endtime) AND starttime is not null AND endtime is not null {1}
            GROUP BY datetime
            ) as tmp ON tmp.datetime::date=daytime
LEFT JOIN(
SELECT count(1) as unfinishedtask,to_char(endtime, 'yyyy-MM-dd')::date as datetime from crm_sys_task where {0} and endtime >=@starttime and endtime<=@endtime {1}
GROUP BY datetime
) as tmp1 ON tmp.datetime::date=daytime ";

            String scheduleCondition = String.Empty;
            String statusCondition = String.Empty;
            if (mapper.AffairStatus == 2)
            {
                statusCondition = " now() <=endtime ";
            }
            else if (mapper.AffairStatus == 1)
            {
                statusCondition = " now() >endtime ";
            }
            else
            {
                statusCondition = "1=1";
            }

            if (!string.IsNullOrEmpty(mapper.UserType) && mapper.UserType == "subordinate")
            {
                scheduleCondition = " and recmanager in (SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree((select deptid from crm_sys_account_userinfo_relate where userid=@userid), 1)) )";
            }
            else if (!string.IsNullOrEmpty(mapper.UserIds))
            {
                scheduleCondition = " and recmanager in (select regexp_split_to_table(@userids::text,',')::int4 )";
            }
            sql = string.Format(sql, statusCondition,scheduleCondition);
            var param = new DynamicParameters();
            param.Add("userids", mapper.UserIds);
            param.Add("starttime", mapper.DateFrom);
            param.Add("endtime", mapper.DateTo.Value.AddDays(1));
            param.Add("affairstatus", mapper.AffairStatus);
            param.Add("endtimetmp", mapper.DateTo);
            var unCount = DataBaseHelper.Query<Count>(sql, param);
            ScheduleTaskCountMapper result = new ScheduleTaskCountMapper();
            result.UnCount = unCount;
            return result;
        }


        public List<Dictionary<string, object>> GetUnConfirmList(UnConfirmListMapper mapper, int userId)
        {
            var sql = @"Select outersql.*,
crm_func_entity_protocol_format_userinfo_multi(outersql.participant) as participant_name,
crm_func_entity_protocol_format_userinfo_multi(outersql.notConfirmParticipant) as notConfirmParticipant_name,
crm_func_entity_protocol_format_workflow_auditstatus(outersql.recaudits) as recaudits_name,
crm_func_entity_protocol_format_userinfo_multi(outersql.refuser) as refuser_name from (
select e.*,u.usericon ,affairstatus_t.dataval as affairstatus_name, affairstatus_t.dataval_lang as affairstatus_lang,reccreator_t.username as reccreator_name,TO_CHAR(e.reccreated,'YYYY-MM-DD HH24:MI:SS') reccreated_name,recmanager_t.username as recmanager_name,recupdator_t.username as recupdator_name,TO_CHAR(e.recupdated,'YYYY-MM-DD HH24:MI:SS') recupdated_name,rectype_t.categoryname as rectype_name,TO_CHAR(e.endtime,'YYYY-MM-DD HH24:MI:SS') endtime_name,repeatEnd_t.dataval as repeatEnd_name, repeatEnd_t.dataval_lang as repeatEnd_lang,TO_CHAR(e.starttime,'YYYY-MM-DD HH24:MI:SS') starttime_name,scheduleType_t.dataval as scheduleType_name, scheduleType_t.dataval_lang as scheduleType_lang,predeptgroup_t.deptname as predeptgroup_name,affairtype_t.dataval as affairtype_name, affairtype_t.dataval_lang as affairtype_lang,deptgroup_t.deptname as deptgroup_name,remindType_t.dataval as remindType_name, remindType_t.dataval_lang as remindType_lang,e.address->>'address' as address_name,repeatType_t.dataval as repeatType_name, repeatType_t.dataval_lang as repeatType_lang from  (
						SELECT * FROM ((SELECT * FROM crm_sys_schedule AS e WHERE affairtype=@affairtype and ( @userid in ( select regexp_split_to_table(notConfirmParticipant::text,',')::int4)))) AS e
						WHERE  e.recstatus = 1
      ) as e  LEFT JOIN crm_sys_userinfo AS u ON u.userid = e.reccreator left outer join crm_sys_dictionary  as affairstatus_t on e.affairstatus = affairstatus_t.dataid and affairstatus_t.dictypeid=152  left outer join crm_sys_userinfo  as reccreator_t on e.reccreator = reccreator_t.userid  left outer join crm_sys_userinfo  as recmanager_t on e.recmanager = recmanager_t.userid  left outer join crm_sys_userinfo  as recupdator_t on e.recupdator = recupdator_t.userid  left outer join crm_sys_entity_category  as rectype_t on e.rectype = rectype_t.categoryid  left outer join crm_sys_dictionary  as repeatEnd_t on e.repeatEnd = repeatEnd_t.dataid and repeatEnd_t.dictypeid=89  left outer join crm_sys_dictionary  as scheduleType_t on e.scheduleType = scheduleType_t.dataid and scheduleType_t.dictypeid=81  left outer join (SELECT
	                                            relate.userid,
	                                            parentdept.deptname
                                            FROM
	                                            crm_sys_department tmpa
                                            INNER JOIN crm_sys_account_userinfo_relate relate ON relate.deptid = tmpa.deptid
                                            inner join crm_sys_department parentdept on parentdept.deptid = tmpa.pdeptid
                                            WHERE
	                                            relate.recstatus = 1) as predeptgroup_t on e.recmanager = predeptgroup_t.userid  left outer join crm_sys_dictionary  as affairtype_t on e.affairtype = affairtype_t.dataid and affairtype_t.dictypeid=91  left outer join (select  relate.userid,tmpa.deptname
                                                from crm_sys_department tmpa
	                                                inner join crm_sys_account_userinfo_relate relate on relate.deptid = tmpa.deptid
                                                where  relate.recstatus = 1 ) as deptgroup_t on e.recmanager = deptgroup_t.userid  left outer join crm_sys_dictionary  as remindType_t on e.remindType = remindType_t.dataid and remindType_t.dictypeid=87  left outer join crm_sys_dictionary  as repeatType_t 
on e.repeatType = repeatType_t.dataid and repeatType_t.dictypeid=84   where  1=1 order by  e.recversion desc  limit 10 offset 0
) as outersql";

            return base.ExecuteQuery(sql, new DbParameter[] { new NpgsqlParameter("userid", userId) });
        }

        public OperateResult AceptSchedule(UnConfirmScheduleStatusMapper mapper, int userId, DbTransaction trans = null)
        {
            var sqlAcept = @"update crm_sys_schedule set participant=array_to_string
(array(select regexp_split_to_table::int4 FROM regexp_split_to_table(participant,',') UNION select @userid), ',') where recid=@recid
 ";
            var sql = @"update crm_sys_schedule set notConfirmParticipant=array_to_string
(array(select regexp_split_to_table::int4 FROM regexp_split_to_table(notConfirmParticipant,',') where regexp_split_to_table::int4 not in (@userid) ), ',') where recid=@recid
 ";

            var param = new DynamicParameters();
            param.Add("recid", mapper.RecId);
            param.Add("userid", userId);
            param.Add("scheduleid", mapper.RecId);

            var result = DataBaseHelper.ExecuteNonQuery(sqlAcept, trans.Connection, trans, param);
            if (result > 0)
            {
                DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "确认成功"
                };
            }
            else
                return new OperateResult
                {
                    Msg = "确认失败"
                };

        }
        public OperateResult RejectSchedule(UnConfirmScheduleStatusMapper mapper, int userId, DbTransaction trans = null)
        {
            var sqlReject = @"update crm_sys_schedule set participant=array_to_string
(array(select regexp_split_to_table::int4 FROM regexp_split_to_table(refuser,',') UNION select @userid), ',') where recid=@recid
 ";
            var sql = @"update crm_sys_schedule set notConfirmParticipant=array_to_string
(array(select regexp_split_to_table::int4 FROM regexp_split_to_table(notConfirmParticipant,',') where regexp_split_to_table::int4 not in (@userid) ), ',') where recid=@recid
 ";
            var sqlRejectReason = @" insert into  crm_sys_schedule_rej_reason(userid,scheduleid,recjectreason) values (@userid,@scheduleid,@recjectreason);";
            var sqlDel = @"delete from crm_sys_schedule_rej_reason where userid=@userid and scheduleid=@scheduleid";

            var param = new DynamicParameters();
            param.Add("recid", mapper.RecId);
            param.Add("userid", userId);
            param.Add("scheduleid", mapper.RecId);

            var result = DataBaseHelper.ExecuteNonQuery(sqlReject, trans.Connection, trans, param);
            if (result > 0)
            {
                DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
                DataBaseHelper.ExecuteNonQuery(sqlDel, trans.Connection, trans, param);
                DataBaseHelper.ExecuteNonQuery(sqlRejectReason, trans.Connection, trans, param);
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "拒绝成功"
                };
            }
            else
                return new OperateResult
                {
                    Msg = "拒绝失败"
                };

        }

        public OperateResult DeleteOrExitSchedule(DeleteScheduleTaskMapper mapper, int userId, DbTransaction trans = null)
        {
            var sql = @"delete from crm_sys_schedule where recid=@recid";
            var sqlExit = @"update crm_sys_schedule set participant=array_to_string
(array(select regexp_split_to_table::int4 FROM regexp_split_to_table(notConfirmParticipant,',') UNION select @userid), ',') where recid=@recid";
            OperateResult result = null;
            var param = new DynamicParameters();
            param.Add("recid", mapper.RecId);
            param.Add("userid", userId);
            if (mapper.OperateType == 1)
            {
                var count = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
                if (count > 0)
                {
                    result = new OperateResult
                    {
                        Flag = 1,
                        Msg = "删除成功"
                    };
                }
                else
                {
                    result = new OperateResult
                    {
                        Msg = "删除失败"
                    };
                }
            }
            else
            {
                var count = DataBaseHelper.ExecuteNonQuery(sqlExit, trans.Connection, trans, param);
                if (count > 0)
                {
                    result = new OperateResult
                    {
                        Flag = 1,
                        Msg = "退出成功"
                    };
                }
                else
                {
                    result = new OperateResult
                    {
                        Msg = "退出失败"
                    };
                }
            }
            return result;
        }

        public OperateResult DeleteOrExitTask(DeleteScheduleTaskMapper mapper, int userId, DbTransaction trans = null)
        {
            var sql = @"delete from crm_sys_schedule where recid=@recid";
            var sqlExit = @"update crm_sys_schedule set participant=array_to_string
(array(select regexp_split_to_table::int4 FROM regexp_split_to_table(notConfirmParticipant,',') UNION select @userid), ',') where recid=@recid";
            OperateResult result = null;
            var param = new DynamicParameters();
            param.Add("recid", mapper.RecId);
            param.Add("userid", userId);
            if (mapper.OperateType == 1)
            {
                var count = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
                if (count > 0)
                {
                    result = new OperateResult
                    {
                        Flag = 1,
                        Msg = "删除成功"
                    };
                }
                else
                {
                    result = new OperateResult
                    {
                        Msg = "删除失败"
                    };
                }
            }
            else
            {
                var count = DataBaseHelper.ExecuteNonQuery(sqlExit, trans.Connection, trans, param);
                if (count > 0)
                {
                    result = new OperateResult
                    {
                        Flag = 1,
                        Msg = "退出成功"
                    };
                }
                else
                {
                    result = new OperateResult
                    {
                        Msg = "退出失败"
                    };
                }
            }
            return result;
        }

        public OperateResult DelayScheduleDay(DelayScheduleMapper mapper, int userId, DbTransaction trans = null)
        {
            var sql = @"update crm_sys_schedule set   starttime=@starttime,endtime=@endtime where recid=@recid;";
            var param = new DynamicParameters();
            param.Add("recid", mapper.RecId);
            param.Add("starttime", mapper.StartTime);
            param.Add("endtime", mapper.EndTime);

            var result = DataBaseHelper.ExecuteNonQuery(sql, trans.Connection, trans, param);
            if (result > 0)
            {
                return new OperateResult
                {
                    Flag = 1,
                    Msg = "确认成功"
                };
            }
            else
                return new OperateResult
                {
                    Msg = "确认失败"
                };

        }
    }
}
