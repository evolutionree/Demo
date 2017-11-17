using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Order
{
    public class OrderPaymentListModel
    {
        public Guid RecId { get; set; }

        public Guid EntityId { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }

    public class OrderStatusModel
    {
        public Guid RecId { get; set; }

        public int Status { get; set; }
    }
}
