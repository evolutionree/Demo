using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Controllers
{
    public class QRCodeController : BaseController
    {
        public QRCodeController() {

        }
        [HttpPost("qrcodeaction")]
        public OutputResult<object> CheckCodeAction()
        {
            return null;
        }
    }
}
