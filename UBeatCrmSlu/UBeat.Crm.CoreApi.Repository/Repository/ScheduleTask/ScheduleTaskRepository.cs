using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.ScheduleTask;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.ScheduleTask
{

    public class ScheduleTaskRepository : IScheduleTaskRepository
    {

        public ScheduleTaskCountMapper GetScheduleTaskCount(ScheduleTaskListMapper mapper, int userId, DbTransaction trans = null)
        {
            var sqlSchedule = @"select count(1) from crm_sys_schedule where  affaristatus=@affaristatus and starttime>=@starttime and endtime <=@endtime {0}";
            var sqlTask = @"select count(1) from crm_sys_task where recid in (select (relatedentity->>'id')::uuid from crm_sys_schedule where    affaristatus=@affaristatus and starttime>=@starttime and endtime <=@endtime  {0})";
            String scheduleCondition = String.Empty;
            if (!string.IsNullOrEmpty(mapper.UserType) && mapper.UserType == "subordinate")
            {
                scheduleCondition = " and recmanager in (SELECT userid FROM crm_sys_account_userinfo_relate WHERE recstatus = 1 AND deptid IN (SELECT deptid FROM crm_func_department_tree((select deptid from crm_sys_account_userinfo_relate where userid=@userid), 1)) )";
            }
            else if (!string.IsNullOrEmpty(mapper.UserIds))
            {
                scheduleCondition = " and recmanager in (select regexp_split_to_table('@userids',',')::int4 )";
            }
            var param = new DynamicParameters();
            param.Add("userids", mapper.UserIds);
            param.Add("starttime", mapper.DateFrom);
            param.Add("endtime", mapper.DateTo);
            var unFinishedSchedule = DataBaseHelper.ExecuteScalar<int>(sqlSchedule, param);
            var unFinishedTask = DataBaseHelper.ExecuteScalar<int>(sqlTask, param);
            ScheduleTaskCountMapper result = new ScheduleTaskCountMapper();
            result.UnFinishedSchedule = unFinishedSchedule;
            result.UnFinishedTask = unFinishedTask;
            return result;
        }
    }
}
