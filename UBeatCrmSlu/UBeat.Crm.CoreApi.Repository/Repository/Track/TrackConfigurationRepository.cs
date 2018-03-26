using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Npgsql;
using System.Linq;
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
            var sql = @"
                         update public.crm_sys_track_strategy_configuare set recname = @recname, locationinterval = @locationinterval, locationtimerange = @locationtimerange, locationcycle = @locationcycle, warninginterval = @warninginterval, remark = @remark, recstatus = @recstatus, updatetime = now() where recid = @recid;";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("recname", trackConfigurationInfo.RecName),
                new NpgsqlParameter("locationinterval", trackConfigurationInfo.LocationInterval),
                new NpgsqlParameter("locationtimerange", trackConfigurationInfo.LocationTimeRange),
                new NpgsqlParameter("locationcycle", trackConfigurationInfo.LocationCycle),
                new NpgsqlParameter("warninginterval", trackConfigurationInfo.WarningInterval),
                new NpgsqlParameter("remark", trackConfigurationInfo.Remark),
                new NpgsqlParameter("recid", trackConfigurationInfo.RecId),
                new NpgsqlParameter("recstatus", trackConfigurationInfo.RecStatus),
            };
            var rowcount = ExecuteNonQuery(sql, param, tran);
            if (rowcount > 0)
                return true;
            return false;
        }

        public Dictionary<string, List<IDictionary<string, object>>> AllocationList(TrackConfigurationAllocationList trackConfigurationAllocationListQuery, int userNumber)
        {
            var procName = "SELECT crm_func_strategyallocation_list(@recname, @pageindex, @pagesize, @userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new
            {
                recname = trackConfigurationAllocationListQuery.StrategyName,
                PageIndex = trackConfigurationAllocationListQuery.PageIndex,
                PageSize = trackConfigurationAllocationListQuery.PageSize,
                UserNo = userNumber
            };

            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public bool AddAllocation(TrackConfigurationAllocation trackConfigurationAllocationListQuery, DbTransaction tran = null)
        {

            return false;
        }

        public bool CancelAllocation(TrackConfigurationAllocationDel delQuery, DbTransaction tran = null)
        {
            var sql = @"delete from crm_sys_track_strategy_allocation where position(recid in @recids) > 0";

            var param = new DbParameter[]
            {
                new NpgsqlParameter("recids", delQuery.StrategyIds),
            };
            var rowcount = ExecuteNonQuery(sql, param, tran);
            if (rowcount > 0)
                return true;
            return false;
        }
    }
}
