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

        #region ---套打模板管理---
        [HttpPost("inserttemplate")]
        public OutputResult<object> InsertTemplate([FromBody] TemplateInfoModel data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _printFormServices.InsertTemplate(data, UserId);
        }
        [HttpPost("settemplatestatus")]
        public OutputResult<object> SetTemplatesStatus([FromBody] TemplatesStatusModel data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _printFormServices.SetTemplatesStatus(data, UserId);
        }
        [HttpPost("updatetemplate")]
        public OutputResult<object> UpdateTemplate([FromBody] TemplateInfoModel data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _printFormServices.UpdateTemplate(data, UserId);
        }
        [HttpPost("gettemplatelist")]
        public OutputResult<object> GetTemplateList([FromBody] TemplateListModel data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _printFormServices.GetTemplateList(data, UserId);
        }
        #endregion

        [HttpPost("printentity")]
        public OutputResult<object> PrintEntity([FromBody] PrintEntityModel data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _printFormServices.PrintEntity(data, UserId);
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
