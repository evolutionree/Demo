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

        public SapApiController(DynamicEntityServices dynamicEntityServices)
        {
            _dynamicEntityServices = dynamicEntityServices;
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

    }
}
