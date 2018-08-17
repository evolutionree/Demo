using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.ScheduleTask;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IScheduleTaskRepository
    {
        ScheduleTaskCountMapper GetScheduleTaskCount(ScheduleTaskListMapper mapper, int userId,DbTransaction trans=null);
    }
}
