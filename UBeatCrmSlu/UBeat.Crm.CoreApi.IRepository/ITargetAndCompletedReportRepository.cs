using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface ITargetAndCompletedReportRepository
    {
        Dictionary<string, List<Dictionary<string, object>>> DataList(string entityid, string searchsql, string orderby, int pageIndex, int pageSize, int userNumber);
    }
}
