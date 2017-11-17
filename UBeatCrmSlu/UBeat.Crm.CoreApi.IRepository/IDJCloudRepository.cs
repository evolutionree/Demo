using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.DJCloud;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDJCloudRepository : IBaseRepository
    {
        OperateResult AddDJCloudCallLog(DJCloudCallMapper data);
        string getCurrentLoginMobileNO(int userNumber);
    }
}
