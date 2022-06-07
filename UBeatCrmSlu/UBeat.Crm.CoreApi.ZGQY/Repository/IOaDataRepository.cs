using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Jint.Native;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.ZGQY.Model;

namespace UBeat.Crm.CoreApi.ZGQY.Repository
{
    public interface IOaDataRepository
    {
        DataSet getContractFromOa();

        DataSet getContractFromOaChange();
        int insertContract(DataRow dataRow, int userId,DbTransaction tran = null);
        
        int changeContract(DataRow dataRow, int userId,DbTransaction tran = null);
        DataSet getCustomerRiskFromOa();
        int updateCustomerRisk(DataRow list, int userId, DbTransaction tran = null);

        List<Dictionary<string, object>> GetMessageSendData(DbTransaction tran = null);

        OperateResult UpdateMessageStatus(Guid recId, int userNumber);
    }
}
