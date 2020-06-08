using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IToERPRepository
    {
        string IsExistsPackingShipOrder(string packingshipid);
        string IsExistsOrder(string orderId);
        string IsExistsMakeCollectionOrder(string makeColOrderId);

        string GetOrderLastUpdatedTime();
        string GetShippingOrderLastUpdatedTime();
    }
}
