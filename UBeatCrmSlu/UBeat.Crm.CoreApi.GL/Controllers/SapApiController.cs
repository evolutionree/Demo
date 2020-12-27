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

    [Route("gl/[controller]")]
    public class SapApiController : BaseController
    {
        private readonly BaseDataServices _baseDataServices;
        private readonly FetchCustomerServices _fetchCustomerServices;

        public SapApiController(BaseDataServices baseDataServices, FetchCustomerServices fetchCustomerServices)
        {
            _baseDataServices = baseDataServices;
            _fetchCustomerServices = fetchCustomerServices;
        }

        [Route("test")]
        [HttpPost]
        [AllowAnonymous]
        public OutputResult<object> SapTest([FromBody] SynSapModel paramInfo = null)
        {
            if (paramInfo == null|| paramInfo.type==-1)
            {
                return ResponseError<object>("参数异常");
            }
            var typeId = (BizSynEnum)Convert.ToInt32(paramInfo.type);
            switch (typeId)
            {
                case BizSynEnum.验证业务:
                        return new OutputResult<object>("测试通过:参数:" + typeId.ToString());
                    break;
                default:
                        return ResponseError<object>("没有找到对应业务类型");
                    break;
            }
        }

        [Route("saprequest")]
        [HttpPost]
        [AllowAnonymous]
        public OutputResult<object> SapRequest([FromBody] SynSapModel paramInfo = null)
        {
            if (paramInfo == null || paramInfo.type == -1)
            {
                return ResponseError<object>("参数异常");
            }
            var typeId = (BizSynEnum)Convert.ToInt32(paramInfo.type);
            switch (typeId)
            {
                case BizSynEnum.验证业务:
                    return new OutputResult<object>("测试通过:参数:" + typeId.ToString());
                    break;
                default:
                    return ResponseError<object>("没有找到对应业务类型");
                    break;
            }
        }

        [Route("initdicdata")]
        [HttpPost]
        [AllowAnonymous]
        public OutputResult<object> InitDicData([FromBody] SynSapModel model = null)
        {
            _baseDataServices.InitDicDataQrtz();
            return new OutputResult<object>(null);
        }

        [Route("fetchcustdata")]
        [HttpPost]
        [AllowAnonymous]
        public OutputResult<object> FetchCustData([FromBody] SynSapModel model = null)
        {
            if (model == null || model.RecIds.Count == 0)
                return ResponseError<object>("参数格式错误");
            WriteOperateLog("获取SAP客户数据", string.Empty);

            var sendResult = _fetchCustomerServices.FetchCustData(model,UserId,1);
            return new OutputResult<object>(sendResult);
        }

        [Route("fetchcustdatabyid")]
        [HttpPost]
        [AllowAnonymous]
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
