﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using System.IO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class ExcelController : BaseController
    {
        private readonly ILogger<ExcelController> _logger;

        private readonly ExcelServices _service;


        public ExcelController(ILogger<ExcelController> logger, ExcelServices service) : base(service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost("add")]
        public OutputResult<object> AddExcel([FromForm] AddExcelModel formData)
        {
            return _service.AddExcel(formData, UserId);
        }
        [HttpPost("delete")]
        public OutputResult<object> DeleteExcel([FromBody] DeleteExcelModel body)
        {
            return _service.DeleteExcel(body, UserId);
        }
        [HttpPost("list")]
        public OutputResult<object> SelectExcels([FromBody] ExcelSelectModel body)
        {
            return _service.SelectExcels(body, UserId);
        }

        [HttpPost("tasklist")]
        public OutputResult<object> GetTaskList([FromBody] TaskListRequestModel body)
        {
            return _service.GetTaskList(UserId, body.TaskIds);
        }
        [HttpPost("taskstart")]
        public OutputResult<object> TaskStart([FromBody] TaskRequestModel body)
        {
            return _service.TaskStart(body.TaskId, ControllerContext.HttpContext.RequestServices);
        }

        [HttpPost("importtemplate")]
        public OutputResult<object> ImportTemplate([FromForm] ImportTemplateModel formData)
        {
            return _service.ImportTemplate(formData, UserId);
        }

        [HttpGet("exporttemplate")]
        [AllowAnonymous]
        public IActionResult ExportTemplate([FromQuery]ExportTemplateModel queryParam)
        {
            ExportModel model = null;
            if (queryParam.TemplateType == TemplateType.FixedTemplate)
            {
                if (queryParam.ExportType == ExportType.ExcelTemplate)
                    model = _service.ExportTemplate(queryParam.Key, queryParam.UserId);
                else
                {
                    model = _service.GenerateImportTemplate(queryParam.Key, queryParam.UserId);
                }
            }
            else
            {
                Guid entityid = Guid.Empty;
                if (!Guid.TryParse(queryParam.Key, out entityid))
                    return ResponseError("实体id不可为空");
                model = _service.GenerateImportTemplate(entityid, queryParam.UserId);
            }

            return File(model.ExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", model.FileName ?? string.Format("{0:yyyyMMddHHmmssffff}.xlsx", DateTime.Now));
        }

        [HttpPost("importdata")]
        public OutputResult<object> ImportData([FromForm] ImportDataModel formData)
        {
            return _service.ImportData(formData, UserId);
        }

        [HttpGet("exportdata")]
        [AllowAnonymous]
        public IActionResult ExportData([FromQuery]ExportDataModel queryParam)
        {
            if (!string.IsNullOrEmpty(queryParam.DynamicQuery))
                queryParam.DynamicModel = JsonConvert.DeserializeObject<DynamicEntityListModel>(queryParam.DynamicQuery);
            queryParam.CanChange2Asynch = true;
            ExportModel model = _service.ExportData(queryParam, queryParam.UserId);
            return File(model.ExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", model.FileName ?? string.Format("{0:yyyyMMddHHmmssffff}.xlsx", DateTime.Now));
        }

        /// <summary>
        /// 以Post方式导出数据，返回文件ID
        /// </summary>
        /// <param name="queryParam"></param>
        /// <returns></returns>
        [HttpPost("exportdata")]
        [AllowAnonymous]
        public OutputResult<object> ExportData_ForPost([FromBody]ExportDataModel queryParam)
        {
            if (!string.IsNullOrEmpty(queryParam.DynamicQuery))
                queryParam.DynamicModel = JsonConvert.DeserializeObject<DynamicEntityListModel>(queryParam.DynamicQuery);
            ExportModel model = _service.ExportData(queryParam, UserId);
            if (model.IsAysnc == false)
            {
                try
                {
                    string curDir = Directory.GetCurrentDirectory();
                    string tmppath = Path.Combine(curDir, "reportexports");
                    string tmpFile = Guid.NewGuid().ToString();
                    if (Directory.Exists(curDir))
                    {
                        Directory.CreateDirectory(tmppath);
                    }
                    string fileFullPath = Path.Combine(tmppath, tmpFile + ".xlsx");
                    FileStream fs = new FileStream(fileFullPath, FileMode.Create);
                    fs.Write(model.ExcelFile, 0, model.ExcelFile.Length);
                    fs.Dispose();
                    object ret = new
                    {
                        RetType = 1,
                        FileId = tmpFile
                    };
                    return new OutputResult<object>(ret);
                }
                catch (Exception ex)
                {
                    return ResponseError<object>(ex.Message);
                }
            }
            else
            {
                object ret = new
                {
                    RetType = 0,
                    Message = model.Message
                };
                return new OutputResult<object>(ret);
            }

        }
        /// <summary>
        /// 嵌套表格导入数据
        /// 仅仅是解析数据，然后返回数据
        /// </summary>
        /// <param name="ParamInfo"></param>
        /// <returns></returns>
        [HttpPost("detailimport")]
        public OutputResult<object> DetailImport([FromForm] DetailImportParamInfo ParamInfo)
        {
            if (ParamInfo == null) return ResponseError<object>("参数异常");
            OutputResult<object> ret = ParamInfo.ValidateData();
            if (ret != null) return ret;
            return new OutputResult<object>(this._service.DetailImport(ParamInfo, UserId));
        }
        /// <summary>
        /// 嵌套表格导入模板生成
        /// 返回fileid
        /// </summary>
        /// <param name="ParamInfo"></param>
        /// <returns></returns>
        [HttpGet("detailexporttemplate")]
        [AllowAnonymous]
        public IActionResult DetailExportDetail([FromQuery] DetailImportTemplateParamInfo ParamInfo)
        {
            ExportModel model = _service.GenerateDetailImportTemplate(ParamInfo.MainEntityId, ParamInfo.MainTypeId, ParamInfo.DetailEntityId);
            return File(model.ExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", model.FileName ?? string.Format("{0:yyyyMMddHHmmssffff}.xlsx", DateTime.Now));
        }
        #region --test--
        [HttpPost("importdata2")]
        public OutputResult<object> ImportData2([FromForm] ImportDataModel formData)
        {
            List<ExcelHeader> headers = new List<ExcelHeader>();
            headers.Add(new ExcelHeader() { Text = "产品名称", FieldName = "productname", IsNotEmpty = true });
            headers.Add(new ExcelHeader() { Text = "产品描述", FieldName = "productfeatures", IsNotEmpty = false });
            headers.Add(new ExcelHeader() { Text = "产品系列名称", FieldName = "pproductsetname", IsNotEmpty = true });
            headers.Add(new ExcelHeader() { Text = "产品名称1", FieldName = "productname1", IsNotEmpty = false });

            return new OutputResult<object>(_service.ImportData(formData.Data.OpenReadStream(), headers));
        }

        [HttpGet("exportdata2")]
        [AllowAnonymous]
        public IActionResult ExportData2([FromQuery]ExportDataModel queryParam)
        {
            List<ExcelHeader> headers = new List<ExcelHeader>();
            headers.Add(new ExcelHeader() { FieldName = "test", Text = "test01" });
            headers.Add(new ExcelHeader() { FieldName = "test1", Text = "test02" });
            headers.Add(new ExcelHeader() { FieldName = "test2", Text = "test03" });
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            var dic = new Dictionary<string, object>();
            dic.Add("test", "asdfa");
            dic.Add("test1", "asdfa1");
            dic.Add("test2", "asdfa2");
            //dic.Add("test3", "asdfa3");
            rows.Add(dic);
            ExportModel model = _service.ExportData("sheetName", headers, rows);
            return File(model.ExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", model.FileName ?? string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now));


        }
        #endregion
    }
}
