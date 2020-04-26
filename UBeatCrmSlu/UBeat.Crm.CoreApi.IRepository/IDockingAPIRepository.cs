using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IDockingAPIRepository
    {
        OperateResult InsertBussinessInfomation(BussinessInformation data, int userNumber);
        List<BussinessInformation> GetBussinessInfomation(string selectField, int isLike, string companyName, int userNumber);
        void UpdateBussinessInfomation(BussinessInformation data, int userNumber);
    }
}
