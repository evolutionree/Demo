using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.DbManage.WorkFlow;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDbWorkFlowRepository
    {
        List<DbWorkFlowInfo> GetWorkFlowInfoList(DbTransaction trans);
    }
}
