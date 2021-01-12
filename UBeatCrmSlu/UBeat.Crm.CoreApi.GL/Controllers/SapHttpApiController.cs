﻿using Microsoft.AspNetCore.Authorization;
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

        public SapHttpApiController(BaseDataServices baseDataServices, FetchCustomerServices fetchCustomerServices, 
            ModifyCustomerServices modifyCustomerServices)
        {
            _baseDataServices = baseDataServices;
            _fetchCustomerServices = fetchCustomerServices;
            _modifyCustomerServices = modifyCustomerServices;
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

    }
}