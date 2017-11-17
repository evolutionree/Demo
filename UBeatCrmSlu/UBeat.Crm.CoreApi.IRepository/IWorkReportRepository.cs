using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.WorkReport;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IWorkReportRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> DailyQuery(DailyReportLstMapper daily, int userNumber);


        Dictionary<string, List<IDictionary<string, object>>> DailyInfoQuery(DailyReportLstMapper daily, int userNumber);


        OperateResult InsertDaily(DailyReportMapper daily, int userNumber);


        OperateResult UpdateDaily(DailyReportMapper daily, int userNumber);


        Dictionary<string, List<IDictionary<string, object>>> WeeklyQuery(WeeklyReportLstMapper daily, int userNumber);


        Dictionary<string, List<IDictionary<string, object>>> WeeklyInfoQuery(WeeklyReportLstMapper daily, int userNumber);


        OperateResult InsertWeekly(WeeklyReportMapper week, int userNumber);


        OperateResult UpdateWeekly(WeeklyReportMapper week, int userNumber);


    }
}
