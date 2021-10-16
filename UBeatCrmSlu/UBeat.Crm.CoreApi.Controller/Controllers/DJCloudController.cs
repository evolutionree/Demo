using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.DJCloud;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DJCloudController : BaseController
    {
        private readonly DJCloudServices _djCloudServices;

        public DJCloudController(DJCloudServices djCloudServices) : base(djCloudServices)
        {
            _djCloudServices = djCloudServices;
        }

        [HttpPost]
        [Route("call")]
        public OutputResult<object> Call([FromBody] DJCloudCallBody callBodyModel = null)
        {
            if (callBodyModel == null) return ResponseError<object>("参数格式错误");

            AnalyseHeader header = GetAnalyseHeader();
            var isMobile = IsMobile();
            return _djCloudServices.Call(callBodyModel, UserId, isMobile);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("callback")]
        public void Callback([FromBody] DJCloudCallBody callBodyModel = null)
        {
           
        }
    }
}