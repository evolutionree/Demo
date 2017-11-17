using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Order;

namespace UBeat.Crm.CoreApi.Services.Models.Order
{
    public class OrderProfile:Profile
    {
        public OrderProfile()
        {
            CreateMap<OrderPaymentListModel, OrderPaymentListMapper>();
        }
    }
}
