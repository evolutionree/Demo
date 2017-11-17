using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Attendance;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IAttendanceRepository : IBaseRepository
    {
        OperateResult Sign(AttendanceSignMapper signEntity, int userNumber);

        Dictionary<string, List<Dictionary<string, object>>> SignList(PageParam pageParam, AttendanceSignListMapper searchParm, int userNumber);
    }
}
