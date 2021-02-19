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

        public SapHttpApiController(BaseDataServices baseDataServices, FetchCustomerServices fetchCustomerServices,
            ModifyCustomerServices modifyCustomerServices, Services.OrderServices orderServices)
        {
            _baseDataServices = baseDataServices;
            _fetchCustomerServices = fetchCustomerServices;
            _modifyCustomerServices = modifyCustomerServices;
            _orderServices = orderServices;
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
            var c = _modifyCustomerServices.SyncDelivnote2CRM(info);
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
            return _modifyCustomerServices.SyncSapCustCreditLimitData(Guid.Parse("67121d89-cc88-43cb-a459-f86370774259"), Guid.Parse("23f8d4ab-7b7e-491b-a5f2-eae02ad8b12b"), 1);
        }
    }
}
