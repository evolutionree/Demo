using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace UBeat.Crm.CoreApi.GL.Repository
{
    public interface IDelivnoteRepository
    {
        string GetOrderNoByRecId(string recid);
        int UpdateDeliverySapCode(Guid recId, string sapCode, DbTransaction tran = null);
        Guid GetRecIdByCaseId(Guid caseId, DbTransaction tran);
        Dictionary<string, object> GetOrderInfo(string orderCode);
        string GetCrmProduct(string productCode);
        Guid IsExistsDelivnote(string code);
    }
}
