using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.SalesTarget;
using Microsoft.AspNetCore.Authorization;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using System;
using UBeat.Crm.CoreApi.Services.Models.Account;
using System.Collections.Generic;
using UBeat.Crm.CoreApi.Services.Utility.ExcelUtility; 

namespace UBeat.Crm.CoreApi.Controllers
{

    [Route("api/[controller]")]
    public class SalesTargetController : BaseController
    {

        private readonly SalesTargetServices _service;
        private readonly RuleTranslatorServices _ruleService;
		private readonly AccountServices _accountService;

        public SalesTargetController(SalesTargetServices service, RuleTranslatorServices ruleService,AccountServices accountService) : base(service)
        {
            _service = service;
            _ruleService = ruleService;
			_accountService = accountService; 
		}

        #region 销售指标


        /// <summary>
        /// 保存销售指标类型
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("savenormtype")]
        public OutputResult<object> SaveNormType([FromBody] SaleTargetNormTypeSaveModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.AddSalesTargetNormType(body, UserId);

        }


        /// <summary>
        /// 删除销售指标
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("deletenormtype")]
        public OutputResult<object> DeleteNormType([FromBody] SaleTargetNormTypeDeleteModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.DeleteSalesTargetNormType(body, UserId);

        }


        /// <summary>
        /// 获取销售指标列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getnormtypelist")]
        public OutputResult<object> GeteNormTypeList()
        {
            return _service.GetNormTypeList();
        }


        /// <summary>
        /// 保存销售指标
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("savenormtyperule")]
        public OutputResult<object> SaveNormTypeRule([FromBody] SaleTargetNormTypeRuleSaveModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _ruleService.SaveRuleForSalesTargetNorm(body, UserId);
        }



        /// <summary>
        /// 获取销售指标
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getnormtyperule")]
        public OutputResult<object> GetNormTypeRule([FromBody] SaleTargetNormRuleDetailModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetSalesTargetNormRule(body, UserId);

        }



        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getentitylist")]
        public OutputResult<object> GeteEntityList()
        {
            return _service.GetEntityList();
        }



        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getentityfield")]
        public OutputResult<object> GeteEntityField([FromBody] SaleTargetEntityFieldSelect body)
        {
            return _service.GetEntityFields(body);
        }







        #endregion

        #region 销售目标


        /// <summary>
        /// 获取销售目标分页列表数据
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("gettargets")]
        public OutputResult<object> GetSalesTargets([FromBody] SalesTargetSelectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetSalesTargets(body, UserId);

        }




        /// <summary>
        /// 保存销售目标
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("savetarget")]
        public OutputResult<object> SaveSalesTarget([FromBody] SalesTargetSaveModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.SaveSalesTarget(body, UserId);
        }


        /// <summary>
        /// 获取销售目标明细数据
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("gettargetdetail")]
        public OutputResult<object> GetSalesTargetDetail([FromBody] SalesTargetSelectDetailModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");

            var rel = _service.GetSalesTargetDetail(body, UserId);
            return rel;

        }
		   
        /// <summary>
        /// 分配年度销售目标
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("saveyeartarget")]
        public OutputResult<object> SaveYearSalesTarget([FromBody] List<YearSalesTargetSaveModel> body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            var rel = _service.SaveYearSalesTarget(body, UserId);
            return rel;

        }

        /// <summary>
        /// 获取年度销售目标
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getyeartarget")]
        public OutputResult<object> GetYearSalesTart([FromBody] YearSaleTargetSetlectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");

            var rel = _service.GetYearSalesTarget(body, UserId);
            return rel;

        }

        /// <summary>
        /// 获取子团队和团队下的人
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getdepartment")]
        public OutputResult<object> GetDepartmentAndUser([FromBody] SaleTargetDepartmentSelect body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");

            var rel = _service.GetEntityFields(body.DepartmentId, UserId);
            return rel;

        }


        #endregion 

		#region 导入销售目标
		/// <summary>
		/// 下载导入模版
		/// </summary>
		/// <param name="queryParam"></param>
		/// <returns></returns>
		[HttpGet("exporttemplate")]
		[AllowAnonymous]
		public IActionResult ExportSalesTargetTemplate([FromQuery]ExportTemplateModel queryParam)
		{
			if (queryParam.UserId <= 0)
				return ResponseError("用户Id不能为空");
			  
			var deptList = _service.GetSalesTargetDept(queryParam.UserId);

			var queryModel = new AccountQueryModel();
			queryModel.UserName = "";
			queryModel.UserPhone = "";
			queryModel.PageIndex = 1;
			queryModel.PageSize = int.MaxValue;
			queryModel.DeptId = new Guid("7f74192d-b937-403f-ac2a-8be34714278b");
			queryModel.RecStatus = 1;
			var userList = _accountService.GetUserPowerList(queryModel, queryParam.UserId); 
			var targetTypeList = _service.GetNormTypeList();
			var targetDic = _service.GetTargetDic();

			var defines = _service.GeneralDynamicTemplate_ImportData(targetDic, deptList, userList, targetTypeList); 
			var sheetsdata = new List<ExportSheetData>();
			
			foreach (var m in defines)
			{  
				var simpleTemp = m as SimpleSheetTemplate; 

				var sheetDataTemp = new ExportSheetData() { SheetDefines = m };
				sheetDataTemp.DataRows.AddRange(simpleTemp.DataObject as List<Dictionary<string,object>>);
				 
				sheetsdata.Add(sheetDataTemp);
			}

			ExportModel model = null;
			model = new ExportModel()
			{
				FileName = "销售目标导入.xlsx",
				ExcelFile = OXSExcelWriter.GenerateExcel(sheetsdata)
			};

			return File(model.ExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", model.FileName ?? string.Format("{0:yyyyMMddHHmmssffff}.xlsx", DateTime.Now));
		}

		[HttpPost("importdata")]
		public OutputResult<object> ImportData([FromForm] ImportDataModel formData)
		{
			formData.Key = "crm_func_sales_target_import";
			var deptList = _service.GetSalesTargetDept(UserId);

			var queryModel = new AccountQueryModel();
			queryModel.UserName = "";
			queryModel.UserPhone = "";
			queryModel.PageIndex = 1;
			queryModel.PageSize = int.MaxValue;
			queryModel.DeptId = new Guid("7f74192d-b937-403f-ac2a-8be34714278b");
			queryModel.RecStatus = 1;
			var userList = _accountService.GetUserPowerList(queryModel, UserId);
			var targetTypeList = _service.GetNormTypeList();

			return _service.ImportData(formData, UserId, deptList, userList, targetTypeList);
		}
		#endregion

		#region 导出销售目标
		[HttpGet("exportdata")]
		[AllowAnonymous]
		public IActionResult ExportData([FromQuery]YearSaleTargetSetlectModel queryParam)
		{ 
			ExportModel model = _service.ExportData(queryParam);
			return File(model.ExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", model.FileName ?? string.Format("{0:yyyyMMddHHmmssffff}.xlsx", DateTime.Now));
		}
		#endregion
	}
}