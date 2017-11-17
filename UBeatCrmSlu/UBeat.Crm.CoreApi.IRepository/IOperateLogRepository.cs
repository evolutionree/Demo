using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.OpreateLog;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IOperateLogRepository : IBaseRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> RecordList(PageParam pageParam, OperateLogRecordListMapper searchParam, int userNumber);
    }
}
