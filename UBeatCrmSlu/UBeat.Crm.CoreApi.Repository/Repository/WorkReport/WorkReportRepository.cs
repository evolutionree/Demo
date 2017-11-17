using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.WorkReport;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.WorkReport
{
    public class WorkReportRepository : IWorkReportRepository
    {
        #region 日报
        public Dictionary<string, List<IDictionary<string, object>>> DailyQuery(DailyReportLstMapper daily, int userNumber)
        {
            var procName =
                "SELECT crm_func_daily_list(@entityid,@menuid,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new DynamicParameters();
            param.Add("entityid", daily.EntityId);
            param.Add("menuid", daily.MenuId);
            param.Add("pageindex", daily.PageIndex);
            param.Add("pagesize", daily.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> DailyInfoQuery(DailyReportLstMapper daily, int userNumber)
        {
            var procName =
                "SELECT crm_func_daily_info(@recid,@userno)";

            var dataNames = new List<string> { "DailyInfo" };
            var param = new DynamicParameters();
            param.Add("recid", daily.RecId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult InsertDaily(DailyReportMapper daily, int userNumber)
        {
            var sql = @"
                     SELECT * FROM crm_func_daily_add(@count,@reportdate,@reportcon,@reporttype,@recid,@optype,@userids, @userno)
            ";
            OperateResult result = new OperateResult();
            int i = 0;
            foreach (var tmp in daily.RecUsers)
            {
                i = i + 1;
                var param = new DynamicParameters();
                param.Add("count", i);
                param.Add("reporttype", daily.ReportType = 0);
                param.Add("recid", result.Id == null ? String.Empty : result.Id);
                param.Add("reportdate", daily.ReportDate);
                param.Add("reportcon", daily.ReportCon);
                param.Add("optype", tmp.Optype);
                param.Add("userids", tmp.UserIds);
                param.Add("userno", userNumber);
                result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                if (result.Flag == 0) return result;
            }
            return result;
        }

        public OperateResult UpdateDaily(DailyReportMapper daily, int userNumber)
        {
            var sql = @"
                     SELECT * FROM crm_func_daily_edit(@count,@reportdate,@reportcon,@reporttype,@recid,@optype,@userids, @userno)
            ";
            OperateResult result = new OperateResult();
            int i = 0;
            foreach (var tmp in daily.RecUsers)
            {
                i = i + 1;
                var param = new DynamicParameters();
                param.Add("count", i);
                param.Add("reporttype", daily.ReportType = 0);
                param.Add("recid", daily.RecId);
                param.Add("reportdate", daily.ReportDate);
                param.Add("reportcon", daily.ReportCon);
                param.Add("optype", tmp.Optype);
                param.Add("userids", tmp.UserIds);
                param.Add("userno", userNumber);
                result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                if (result.Flag == 0) return result;
            }
            return result;
        }

        #endregion

        #region 周报
        public Dictionary<string, List<IDictionary<string, object>>> WeeklyQuery(WeeklyReportLstMapper daily, int userNumber)
        {
            var procName =
                "SELECT crm_func_weekly_list(@entityid,@menuid,@pageindex,@pagesize,@userno)";

            var dataNames = new List<string> { "PageData", "PageCount" };
            var param = new DynamicParameters();
            param.Add("entityid", daily.EntityId);
            param.Add("menuid", daily.MenuId);
            param.Add("pageindex", daily.PageIndex);
            param.Add("pagesize", daily.PageSize);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public Dictionary<string, List<IDictionary<string, object>>> WeeklyInfoQuery(WeeklyReportLstMapper daily, int userNumber)
        {
            var procName =
                "SELECT crm_func_week_info(@recid,@userno)";

            var dataNames = new List<string> { "WeeklyInfo" };
            var param = new DynamicParameters();
            param.Add("recid", daily.RecId);
            param.Add("userno", userNumber);
            var result = DataBaseHelper.QueryStoredProcCursor(procName, dataNames, param, CommandType.Text);
            return result;
        }

        public OperateResult InsertWeekly(WeeklyReportMapper week, int userNumber)
        {
            var sql = @"
                     SELECT * FROM crm_func_weekly_add(@count,@reportdate,@reportcon,@weeks,@weektype,@reporttype,@recid,@optype,@userids, @userno)
            ";
            OperateResult result = new OperateResult();
            int i = 0;
            foreach (var tmp in week.RecUsers)
            {
                i = i + 1;
                var param = new DynamicParameters();
                param.Add("count", i);
                param.Add("reporttype", week.ReportType = 1);
                param.Add("recid", result.Id == null ? String.Empty : result.Id);
                param.Add("reportdate", week.ReportDate);
                param.Add("reportcon", week.ReportCon);
                param.Add("weeks", week.Weeks);
                param.Add("weektype", week.WeekType);
                param.Add("optype", tmp.Optype);
                param.Add("userids", tmp.UserIds);
                param.Add("userno", userNumber);
                result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                if (result.Flag == 0) return result;
            }
            return result;
        }

        public OperateResult UpdateWeekly(WeeklyReportMapper week, int userNumber)
        {
            var sql = @"
                     SELECT * FROM crm_func_weekly_edit(@count,@reportdate,@reportcon,@weeks,@weektype,@reporttype,@recid,@optype,@userids, @userno)
            ";
            OperateResult result = new OperateResult();
            int i = 0;
            foreach (var tmp in week.RecUsers)
            {
                i = i + 1;
                var param = new DynamicParameters();
                param.Add("count", i);
                param.Add("reporttype", week.ReportType = 1);
                param.Add("recid", week.RecId);
                param.Add("reportdate", week.ReportDate);
                param.Add("reportcon", week.ReportCon);
                param.Add("weeks", week.Weeks);
                param.Add("weektype", week.WeekType);
                param.Add("optype", tmp.Optype);
                param.Add("userids", tmp.UserIds);
                param.Add("userno", userNumber);
                result = DataBaseHelper.QuerySingle<OperateResult>(sql, param);
                if (result.Flag == 0) return result;
            }
            return result;
        }

        #endregion
    }
}
