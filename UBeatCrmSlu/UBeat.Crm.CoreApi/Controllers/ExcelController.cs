using System;
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
			return _service.TaskStart(body.TaskId);
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
			ExportModel model = _service.ExportData(queryParam);
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
