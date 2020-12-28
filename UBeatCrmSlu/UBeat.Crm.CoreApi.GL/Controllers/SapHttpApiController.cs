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

        public SapHttpApiController(BaseDataServices baseDataServices, FetchCustomerServices fetchCustomerServices)
        {
            _baseDataServices = baseDataServices;
            _fetchCustomerServices = fetchCustomerServices;
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

    }
}
