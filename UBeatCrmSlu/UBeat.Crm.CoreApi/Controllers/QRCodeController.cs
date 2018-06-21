using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class QRCodeController : BaseController
    {
        private QRCodeServices _qRCodeServices;
        public QRCodeController(QRCodeServices qRCodeServices) {
            _qRCodeServices = qRCodeServices;

        }
        [HttpPost("qrcodeaction")]
        [AllowAnonymous]
        public OutputResult<object> CheckCodeAction([FromBody]QRCodeCheckParamInfo paramInfo )
        {
            return _qRCodeServices.CheckQrCode(paramInfo.Code, paramInfo.CodeType,UserId);
        }
    }
    public class QRCodeCheckParamInfo {
        public string Code { get; set; }
        public int CodeType { get; set; }
    }
}
