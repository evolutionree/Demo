using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.ReportDefine;
using UBeat.Crm.CoreApi.DomainModel.Reports;
using UBeat.Crm.CoreApi.Services.Services;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace UBeat.Crm.CoreApi.Controllers
{

    [Route("api/[controller]")]
    public class ReportEngineController : BaseController
    {
        private readonly ReportEngineServices _reportEngineService;
        public ReportEngineController(ReportEngineServices reportEngineService) : base(reportEngineService)
        {
            this._reportEngineService = reportEngineService;
        }
        [HttpPost]
        [Route("queryReportDefine")]
        public OutputResult<object> QueryReportDefine([FromBody] ReportDefineQueryModel queryInfo)
        {
            if (queryInfo == null || queryInfo.Id == null || queryInfo.Id.Length == 0) {
                return new OutputResult<object>(null, "参数异常", 1);
            }
            Guid id = Guid.Empty;
            if (Guid.TryParse(queryInfo.Id, out id) == false) {
                return new OutputResult<object>(null, "参数异常", 1);
            }
            ReportDefineInfo report = _reportEngineService.queryReportDefineInfo(queryInfo.Id, UserId);
            if (report == null) {
                return new OutputResult<object>(null, "没有找到报表定义，或者报表定义不正确", 1);
            }
            return new OutputResult<object>(report);
        }

        [HttpPost]
        [Route("queryData")]
        public OutputResult<object> queryDataForDataSource([FromBody]DataSourceQueryDataModel queryModel) {
            if (queryModel == null || queryModel.InstId == null || queryModel.InstId.Length == 0
                || queryModel.DataSourceId == null || queryModel.DataSourceId.Length == 0)
                return new OutputResult<object>(null, "参数异常", 1);
            Guid tmp = Guid.Empty;

            if (Guid.TryParse(queryModel.DataSourceId, out tmp) == false) return new OutputResult<object>(null, "参数异常", 1);
            try
            {

                return this._reportEngineService.queryDataFromDataSource(ControllerContext.HttpContext.RequestServices, queryModel, UserId);
            }
            catch (Exception ex) {
                if (ex.InnerException != null)
                {
                    return new OutputResult<object>(null, ex.InnerException.Message, -1);

                }
                else {

                    return new OutputResult<object>(null, ex.Message, -1);
                }
            }

        }


        /// <summary>
        ///  导出表格数据(生成文件，并不下载)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("export")]
        public OutputResult<object> exportTableData([FromBody ]ReportTableQueryDataModel paramInfo) {
            OutputResult<object> dataResult = null;
            try
            {
                dataResult = this.queryDataForDataSource(paramInfo.DataQueryModel);
            }
            catch (Exception ex)
            {
            }
            if (dataResult == null || dataResult.Status != 0)
            {
                return ResponseError<object>("获取数据异常");
            }
            Dictionary<string, object> dataDict = (Dictionary<string, object>)dataResult.DataBody;
            List<Dictionary<string, object>> listData = null;
            List<TableColumnInfo> Columns = paramInfo.Columns;
            if (dataDict.ContainsKey("data") && dataDict["data"] != null)
            {

                listData = (List<Dictionary<string, object>>)dataDict["data"];
            }
            if (dataDict.ContainsKey("columns") && dataDict["columns"]!=null) {
                string tmp = Newtonsoft.Json.JsonConvert.SerializeObject(dataDict["columns"]);
                Columns = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TableColumnInfo>>(tmp);
            }
            try
            {
                string filename = this._reportEngineService.ExportTable(listData, Columns);
                return new OutputResult<object>(filename);
            }
            catch (Exception ex)
            {
                return ResponseError<object>("导出Excel异常：" + ex.Message);
            }
        }
        [HttpGet("export2file")]
        [AllowAnonymous]
        public IActionResult DownloadExportFile([FromQuery] string fileid,[FromQuery]string  fileName = null) {
            string curDir = Directory.GetCurrentDirectory();
            string tmppath = Path.Combine(curDir, "reportexports");
            string tmpFile = fileid;
            if (Directory.Exists(curDir))
            {
                Directory.CreateDirectory(tmppath);
            }
            string fileFullPath = Path.Combine(tmppath, tmpFile + ".xlsx");
            return PhysicalFile(fileFullPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName ?? string.Format("{0:yyyyMMddHHmmssffff}.xlsx", DateTime.Now));
        }
        /***
         * 提供给手机客户端获取报表列表
         * */
        [HttpPost]
        [Route("getReportListForMob")]

        public OutputResult<object> getReportListForMobile() {
            return this._reportEngineService.queryReportListForMobile(UserId);
        }
        [HttpPost]
        [Route("repairfunc")]
        public OutputResult<object> repairFunctions() {
            this._reportEngineService.repairFunc(UserId);
            return new OutputResult<object>("ok");
        }

        [HttpPost]
        [Route("mainpagereport")]
        public OutputResult<object> getMyMainPageReportInfo() {
            MainPageReportInfo report = _reportEngineService.getMyMainPageReport( UserId);
            if (report == null)
            {
                return new OutputResult<object>(null, "没有找到首页定义，请联系系统管理员", 1);
            }
            return new OutputResult<object>(report);
        }
    }   
    
}
