using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.ScheduleTask;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IScheduleTaskRepository
    {
        ScheduleTaskCountMapper GetScheduleTaskCount(ScheduleTaskListMapper mapper, int userId, DbTransaction trans = null);

        List<Dictionary<string, object>> GetUnConfirmList(UnConfirmListMapper mapper, int userId);

        OperateResult RejectSchedule(UnConfirmScheduleStatusMapper mapper, int userId, DbTransaction trans = null);
        OperateResult AceptSchedule(UnConfirmScheduleStatusMapper mapper, int userId, DbTransaction trans = null);

        OperateResult DeleteOrExitSchedule(DeleteScheduleTaskMapper mapper, int userId, DbTransaction trans = null);

        OperateResult DelayScheduleDay(DelayScheduleMapper mapper, int userId, DbTransaction trans = null);

        List<dynamic> CheckAuth(IList<Guid> recIds, IList<int> checkUserIds, int userNumber);
    }
}
