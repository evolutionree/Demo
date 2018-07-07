using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Order;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class OrderController : BaseController
    {
        private readonly OrderServices _orderServices;
        public OrderController(OrderServices orderServices):base(orderServices)
        {
            _orderServices = orderServices;
        }

        [HttpPost]
        [Route("queryorderpayment")]
        public OutputResult<object> SalesStageQuery([FromBody]OrderPaymentListModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _orderServices.OrderPaymentQuery(entityModel, UserId);
        }

        [HttpPost]
        [Route("updateorderstatus")]
        public OutputResult<object> UpdateOrderStatus([FromBody]OrderStatusModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _orderServices.UpdateOrderStatus(entityModel, LoginUser);
        }

        [HttpPost]
        [Route("finishorder")]
        public OutputResult<object> FinishOrder([FromBody]OrderStatusModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _orderServices.UpdateOrderStatus(entityModel, LoginUser);
        }

        [HttpPost]
        [Route("cancelorder")]
        public OutputResult<object> CancelOrder([FromBody]OrderStatusModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _orderServices.UpdateOrderStatus(entityModel, LoginUser);
        }

        [HttpPost]
        [Route("invalidorder")]
        public OutputResult<object> InvalidOrder([FromBody]OrderStatusModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _orderServices.UpdateOrderStatus(entityModel, LoginUser);
        }
    }
}
