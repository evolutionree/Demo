using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.GL.Model;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{

    [Route("gl/[controller]")]
    public class SapApiController : BaseController
    {
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly BaseDataServices _baseDataServices;

        public SapApiController(DynamicEntityServices dynamicEntityServices, BaseDataServices baseDataServices)
        {
            _dynamicEntityServices = dynamicEntityServices;
            _baseDataServices = baseDataServices;
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

        [Route("initdicdata")]
        [HttpPost]
        [AllowAnonymous]
        public OutputResult<object> InitDicData([FromBody] SynSapModel model = null)
        {
            _baseDataServices.InitDicDataQrtz();
            return new OutputResult<object>(null);
        }

    }
}
