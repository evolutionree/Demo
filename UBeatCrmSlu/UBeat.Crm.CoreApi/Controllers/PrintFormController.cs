using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
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

        [HttpPost("deletetemplate")]
        public OutputResult<object> DeleteTemplates([FromBody] DeleteTemplatesModel data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _printFormServices.DeleteTemplates(data, UserId);
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

        #region ---获取某条数据拥有的模板列表---
        [HttpPost("getrectemplatelist")]
        public OutputResult<object> GetRecDataTemplateList([FromBody] EntityRecTempModel data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _printFormServices.GetRecDataTemplateList(data, UserId);
        } 
        #endregion



        [HttpPost("testformula")]
        [AllowAnonymous]
        public OutputResult<object> PrintEntity1([FromBody] PrintEntity1 data = null)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            var really_data = dt.Compute(data.Formula, null).ToString() ;
            bool re = false;
            bool.TryParse(really_data, out re);
            //string Key_FieldPath = @"【#\s*\S+\s*#】";
            //var really_data = System.Text.RegularExpressions.Regex.Matches(data.Formula, Key_FieldPath);

            //var really_datass = System.Text.RegularExpressions.Regex.Split(data.Formula, Key_FieldPath, System.Text.RegularExpressions.RegexOptions.Multiline);
            return new OutputResult<object>(really_data);
        }
       

        [HttpPost("printentity")]
        public OutputResult<object> PrintEntity([FromBody] PrintEntityModel data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _printFormServices.PrintEntity(data, UserId);

        }
        [HttpGet("exportfile")]
        [AllowAnonymous]
        public IActionResult DownloadExportFile([FromQuery] string fileid, [FromQuery]string fileName = null)
        {
            string curDir = Directory.GetCurrentDirectory();
            string tmppath = Path.Combine(curDir, "reportexports");
            string tmpFile = fileid;
            if (Directory.Exists(curDir))
            {
                Directory.CreateDirectory(tmppath);
            }
            string fileFullPath = Path.Combine(tmppath, tmpFile + ".pdf");
            if (System.IO.File.Exists(fileFullPath)) {
                return PhysicalFile(fileFullPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName ?? string.Format("{0:yyyyMMddHHmmssffff}.pdf", DateTime.Now));
            }
            fileFullPath = Path.Combine(tmppath, tmpFile + ".xlsx");
            return PhysicalFile(fileFullPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName ?? string.Format("{0:yyyyMMddHHmmssffff}.xlsx", DateTime.Now));
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
