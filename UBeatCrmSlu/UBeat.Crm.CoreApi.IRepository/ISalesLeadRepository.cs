using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.SalesLead;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface ISalesLeadRepository
    {
        OperateResult ChangeSalesLeadToCustomer(SalesLeadMapper entity, int userNumber);
    }
}
