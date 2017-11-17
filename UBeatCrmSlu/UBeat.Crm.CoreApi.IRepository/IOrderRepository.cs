using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Order;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IOrderRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> OrderPaymentQuery(OrderPaymentListMapper order, int userNumber);

        int UpdateOrderStatus(OrderStatusMapper order, int userNumber);
    }

}
