using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.GL.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.GL.Controllers
{

    [Route("api/gl/[controller]")]
    public class SapHttpApiController : BaseController
    {
        private readonly BaseDataServices _baseDataServices;
        private readonly FetchCustomerServices _fetchCustomerServices;
        private readonly ModifyCustomerServices _modifyCustomerServices;
        private readonly Services.OrderServices _orderServices;
        private readonly DelivnoteServices _delivnoteServices;
        private readonly ProductServices _productServices;

        public SapHttpApiController(BaseDataServices baseDataServices, FetchCustomerServices fetchCustomerServices,
            ModifyCustomerServices modifyCustomerServices, Services.OrderServices orderServices, DelivnoteServices delivnoteServices,
            ProductServices productServices)
        {
            _baseDataServices = baseDataServices;
            _fetchCustomerServices = fetchCustomerServices;
            _modifyCustomerServices = modifyCustomerServices;
            _orderServices = orderServices;
            _delivnoteServices = delivnoteServices;
            _productServices = productServices;
        }

        [Route("fetchcustdatabyid")]
        [HttpPost]
        public OutputResult<object> FetchCustDataById([FromBody] SynSapModel model = null)
        {
            if (model == null || model.RecIds.Count == 0)
                return ResponseError<object>("参数格式错误");
            WriteOperateLog("获取SAP客户数据根据id", string.Empty);
            var c = _fetchCustomerServices.FetchCustData(model, UserId, 3);

            if (c.Flag == 1)
            {
                return new OutputResult<object>(c.Msg);
            }
            else
            {
                return ResponseError<object>(c.Msg);
            }
        }

        [Route("fetchorderdatabyid")]
        [HttpPost]
        public OutputResult<object> FetchOrderDataById([FromBody] SoOrderParamModel model = null)
        {
            if (model == null)
                return ResponseError<object>("参数格式错误");
            WriteOperateLog("获取SAP订单数据根据id", string.Empty);
            var c = _orderServices.getOrders(model, UserId);

            if (c.Status == 0)
            {
                return new OutputResult<object>(c.Message);
            }
            else
            {
                return ResponseError<object>(c.Message);
            }
        }

        [Route("syncusttosap")]
        [HttpPost]
        public OutputResult<object> SynCustomerToSap([FromBody] SynSapModel model = null)
        {
            if (model == null || model.RecIds.Count == 0)
                return ResponseError<object>("参数格式错误");

            WriteOperateLog("同步Sap客户", model);
            bool sendResult = false;
            var recId = model.RecIds[0];
            var entityId = model.EntityId;

            var c = _modifyCustomerServices.SynSapCustData(entityId, recId, UserId);
            if (c.Result)
            {
                return new OutputResult<object>(c.Message);
            }
            else
            {
                return ResponseError<object>(c.Message);
            }
        }

        [HttpPost("sysncbankinfo")]
        public OutputResult<object> SyncBankInfo2CRM()
        {
            var c = _modifyCustomerServices.SyncBankInfo2CRM();
            if (c.Result)
            {
                return new OutputResult<object>(c.Message);
            }
            else
            {
                return ResponseError<object>(c.Message);
            }
        }

        [HttpPost("sysncdelivnote")]
        public OutputResult<object> SyncDelivnote2CRM([FromBody] Sync2CRMInfo info)
        {
            if (info == null)
                return ResponseError<object>("参数格式有误");
            var c = _delivnoteServices.SyncDelivnote2CRM(info);
            if (c.Result)
            {
                return new OutputResult<object>(c.Message);
            }
            else
            {
                return ResponseError<object>(c.Message);
            }
        }

        [Route("synordertosap")]
        [HttpPost]
        public OutputResult<object> SynOrderToSap([FromBody] SynSapModel model = null)
        {
            if (model == null || model.RecIds.Count == 0)
                return ResponseError<object>("参数格式错误");

            WriteOperateLog("同步Sap订单", model);
            bool sendResult = false;
            var recId = model.RecIds[0];
            var entityId = model.EntityId;

            var c = _orderServices.SynSapOrderDataByHttp(entityId, recId, UserId);
            if (c.Result)
            {
                return new OutputResult<object>(c.Message);
            }
            else
            {
                return ResponseError<object>(c.Message);
            }
        }

        [Route("syndeliverytosap")]
        public OutputResult<object> SynDelivNoteToSap([FromBody] SynSapModel model = null)
        {
            if (model == null || model.RecIds.Count == 0)
                return ResponseError<object>("参数格式错误");

            WriteOperateLog("同步Sap交货单", model);
            bool sendResult = false;
            var recId = model.RecIds[0];
            var entityId = model.EntityId;

            var c = _delivnoteServices.SynSapDelivNoteData(entityId, recId, UserId);
            if (c.Result)
            {
                return new OutputResult<object>(c.Message);
            }
            else
            {
                return ResponseError<object>(c.Message);
            }
        }

        [Route("synccredittosap")]
        [HttpPost]
        [AllowAnonymous]
        public OutputResult<object> SyncSapCustCreditLimitData()
        {
            return _modifyCustomerServices.SyncSapCustCreditLimitData(Guid.Parse("a62a6832-3ef3-4338-9764-d9a27ffdb854"), Guid.Parse("095b3102-d131-4cda-b8a0-242fca4031f1"), 1);
        }

        [Route("getproductstocks")]
        [AllowAnonymous]
        [HttpPost]
        public OutputResult<object> Getproductstocks([FromBody] QueryProductStockModel model = null)
        {
            if (model == null || model.ProductIds == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("获取SAP产品库存数据", model);

            var sendResult = _productServices.GetProductStockByIds(model.ProductIds);
            return new OutputResult<object>(sendResult);
        }
    }
}
