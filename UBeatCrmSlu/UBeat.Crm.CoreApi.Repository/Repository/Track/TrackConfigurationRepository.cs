using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Npgsql;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Track;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Track
{
    public class TrackConfigurationRepository : RepositoryBase, ITrackConfigurationRepository
    {
        public Dictionary<string, List<IDictionary<string, object>>> TrackConfigurationList(TrackConfigurationInfo trackConfigurationQuery, int userNumber)
        {
            var procName = "SELECT crm_func_trackconfiguration_list(@recname, @status, @pageindex, @pagesize, @userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new
            {
                recname = trackConfigurationQuery.RecName,
                Status = trackConfigurationQuery.RecStatus,
                PageIndex = trackConfigurationQuery.PageIndex,
                PageSize = trackConfigurationQuery.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public bool AddTrackConfiguration(TrackConfigurationInfo trackConfigurationQuery, DbTransaction tran = null)
        {
            var sql = @"
                         INSERT INTO public.crm_sys_track_strategy_configuare(recname, locationinterval, locationtimerange, locationcycle, warninginterval, remark)
                            VALUES (@recname, @locationinterval, @locationtimerange, @locationcycle, @warninginterval, @remark);";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("recname", trackConfigurationQuery.RecName),
                new NpgsqlParameter("locationinterval", trackConfigurationQuery.LocationInterval),
                new NpgsqlParameter("locationtimerange", trackConfigurationQuery.LocationTimeRange),
                new NpgsqlParameter("locationcycle", trackConfigurationQuery.LocationCycle),
                new NpgsqlParameter("warninginterval", trackConfigurationQuery.WarningInterval),
                new NpgsqlParameter("remark", trackConfigurationQuery.Remark),
            };
            var rowcount = ExecuteNonQuery(sql, param, tran);
            //var rowcount = DBHelper.ExecuteNonQuery(tran, sql, param);
            if (rowcount > 0)
                return true;
            return false;
        }

        public bool UpdateTrackConfiguration(TrackConfigurationInfo trackConfigurationInfo, DbTransaction tran = null)
        {
            var sql = @" update public.crm_sys_track_strategy_configuare set recname = @recname, locationinterval = @locationinterval, locationtimerange = @locationtimerange, locationcycle = @locationcycle, warninginterval = @warninginterval, remark = @remark, updatetime = now() where recid = @recid;";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("recname", trackConfigurationInfo.RecName),
                new NpgsqlParameter("locationinterval", trackConfigurationInfo.LocationInterval),
                new NpgsqlParameter("locationtimerange", trackConfigurationInfo.LocationTimeRange),
                new NpgsqlParameter("locationcycle", trackConfigurationInfo.LocationCycle),
                new NpgsqlParameter("warninginterval", trackConfigurationInfo.WarningInterval),
                new NpgsqlParameter("remark", trackConfigurationInfo.Remark),
                new NpgsqlParameter("recid", trackConfigurationInfo.RecId),
                //new NpgsqlParameter("recstatus", trackConfigurationInfo.RecStatus),
            };
            var rowcount = ExecuteNonQuery(sql, param, tran);
            if (rowcount > 0)
                return true;
            return false;
        }

        public OperateResult DeleteTrackConfiguration(TrackConfigurationDel delquery, int userNumber)
        {
            var sql = @"SELECT * FROM crm_sys_track_strategy_configuare_del(@strategyids, @status, @userno)";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("strategyids",delquery.StrategyIds),
                new NpgsqlParameter("status",delquery.Status),
                new NpgsqlParameter("userno", userNumber)
            };
            var result = DBHelper.ExecuteQuery<OperateResult>("", sql, param);

            return result.FirstOrDefault();
        }

        public Dictionary<string, List<IDictionary<string, object>>> AllocationList(TrackConfigurationAllocationList trackConfigurationAllocationListQuery, int userNumber)
        {
            var procName = "SELECT crm_func_strategyallocation_list(@recname, @username, @pageindex, @pagesize, @userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new
            {
                recname = trackConfigurationAllocationListQuery.StrategyName,
                username = trackConfigurationAllocationListQuery.UserName,
                PageIndex = trackConfigurationAllocationListQuery.PageIndex,
                PageSize = trackConfigurationAllocationListQuery.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult AddAllocation(TrackConfigurationAllocation addQuery, int userNo)
        {
             var sql = @"
                SELECT * FROM crm_sys_track_strategy_allocation_add(@strategyid, @userids, @userno)
            ";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("strategyid",addQuery.StrategyId),
                new NpgsqlParameter("userids",addQuery.UserIds),
                 new NpgsqlParameter("userno", userNo)
            };
            var result = DBHelper.ExecuteQuery<OperateResult>("", sql, param);

            return result.FirstOrDefault();
        }

        public bool DelAllocation(TrackConfigurationAllocation delQuery, DbTransaction tran = null)
        {
            var sql = @"delete from crm_sys_track_strategy_allocation where position(userid::text in @userids) > 0";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("userids", delQuery.UserIds),
            };
            var rowcount = ExecuteNonQuery(sql, param, tran);
            if (rowcount > 0)
                return true;
            return false;
        }

        public TrackConfigurationInfo GetUserTrackStrategy(int userNumber)
        {
            var sql = @"select sc.recid, sc.recname, sc.locationinterval, sc.locationtimerange, sc.locationcycle, 
                                                sc.remark, sc.recstatus, sc.updatetime, sc.warninginterval 
                        from crm_sys_track_strategy_configuare sc
                        left join crm_sys_track_strategy_allocation sa on sc.recid = sa.strategyid
                        where sa.userid = @userid and sa.recstatus = 1 and sc.recstatus = 1";

            var param = new DbParameter[]
                    {
                        new NpgsqlParameter("userid", userNumber),
                    };

            var result = DBHelper.ExecuteQuery<TrackConfigurationInfo>("", sql, param);
            return result == null ? null : result.FirstOrDefault();
        }

        public List<UserTrackStrategyInfo> FilterHadBindTrackStrategyUser(string userIds)
        {
            Dictionary<string, string> bindUserInfo = new Dictionary<string, string>();
            var sql = @"select sa.userid,  u.username, sc.warninginterval
from crm_sys_track_strategy_allocation sa
left join crm_sys_track_strategy_configuare sc on sa.strategyid = sc.recid
left join crm_sys_userinfo u on sa.userid = u.userid
where sa.recstatus = 1 and position(sa.userid::text in @userids) > 0
and sc.recstatus = 1";
            var param = new DbParameter[]{
                        new NpgsqlParameter("userids", userIds),
            };
            return DBHelper.ExecuteQuery<UserTrackStrategyInfo>("", sql, param);
        }

        public string AllocatedUsers()
        {
            string userIds = string.Empty;
            var sql = @"select string_agg(sa.userid::text, ',') as userids
from crm_sys_track_strategy_allocation sa left
join crm_sys_track_strategy_configuare sc on sa.strategyid = sc.recid
where sc.recstatus = 1 and sa.recstatus = 1";
            object obj = ExecuteScalar(sql, null);
            if (obj != null)
            {
                userIds = obj.ToString();
            }
            return userIds;
        }
    }
}
