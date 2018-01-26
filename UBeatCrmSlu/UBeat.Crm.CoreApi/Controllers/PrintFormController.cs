using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.PrintForm;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class PrintFormController: BaseController
    {
        private readonly PrintFormServices _printFormServices;
        public PrintFormController(PrintFormServices printFormServices) : base(printFormServices)
        {
            this._printFormServices = printFormServices;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("testdocx")]
        public OutputResult<object> testReplace() {
            _printFormServices.TetSaveDoc("仁千科技");
            return new OutputResult<object>("ok");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("getoutputdoc")]
        public IActionResult GetOutputDocument([FromForm] OutputDocumentParameter formData)
        {
            if (formData == null)
                return ResponseError("缺乏查询参数");
            string fileName;
            byte[] fileData= _printFormServices.GetOutputDocument(formData,out  fileName);
            return Ok();
            //return File(fileData, "application/octet-stream", fileName ?? string.Format("{0:yyyyMMddHHmmssffff}.pdf", DateTime.Now));
        }
    }
}
