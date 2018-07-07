using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.WorkReport;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
 
    [Route("api/[controller]")]
    public class WorkReportController : BaseController
    {

        private readonly WorkReportServices _workReportService;
        private readonly DynamicEntityServices _dynamicEntityServices;

        public WorkReportController(WorkReportServices workReportService, DynamicEntityServices dynamicEntityServices) : base(workReportService)
        {
            _workReportService = workReportService;
            _dynamicEntityServices = dynamicEntityServices;
        }


        [HttpPost]
        [Route("querydaily")]
        public OutputResult<object> DailyQuery([FromBody]DailyReportLstModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _workReportService.DailyQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("querydailyinfo")]
        public OutputResult<object> DailyInfoQuery([FromBody]DailyReportLstModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _workReportService.DailyInfoQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("insertdaily")]
        public OutputResult<object> InsertDaily([FromBody]DailyReportModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _workReportService.InsertDaily(entityModel, UserId);
        }
        [HttpPost]
        [Route("updatedaily")]
        public OutputResult<object> UpdateEntityField([FromBody]DailyReportModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _workReportService.UpdateDaily(entityModel, UserId);
        }



        [HttpPost]
        [Route("queryweekly")]
        public OutputResult<object> WeeklyQuery([FromBody]WeeklyReportLstModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _workReportService.WeeklyQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("queryweeklyinfo")]
        public OutputResult<object> WeeklyInfoQuery([FromBody]WeeklyReportLstModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _workReportService.WeeklyInfoQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("insertweekly")]
        public OutputResult<object> InsertWeekly([FromBody]WeeklyReportModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _workReportService.InsertWeekly(entityModel, UserId);
        }
        [HttpPost]
        [Route("updatweekly")]
        public OutputResult<object> UpdateWeekly([FromBody]WeeklyReportModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _workReportService.UpdateWeekly(entityModel, UserId);
        }
        [HttpPost]
        [Route("list")]
        public OutputResult<object> DataList([FromBody] DynamicEntityListModel dynamicModel = null)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            DateTime dtFrom = DateTime.MinValue;
            DateTime dtTo = DateTime.MaxValue;
            string tmp = "";
            if (dynamicModel.SearchData.ContainsKey("fromdate") 
                    && dynamicModel.SearchData["fromdate"] != null 
                    && ((string)dynamicModel.SearchData["fromdate"]).Length >0 ) {
                DateTime.TryParse(dynamicModel.SearchData["fromdate"].ToString(), out dtFrom);
                if (dtFrom != DateTime.MinValue) {
                    dtFrom = dtFrom - new TimeSpan(6, 0, 0, 0);
                }
                dynamicModel.SearchData.Remove("fromdate");
            }
            if (dynamicModel.SearchData.ContainsKey("todate")
                && dynamicModel.SearchData["todate"] != null
                    && ((string)dynamicModel.SearchData["todate"]).Length > 0)
            {
                DateTime.TryParse(dynamicModel.SearchData["todate"].ToString(), out dtTo);
                dynamicModel.SearchData.Remove("todate");
            }
            tmp = dtFrom.ToString("yyyy-MM-dd") + "," + dtTo.ToString("yyyy-MM-dd");
            if (dynamicModel.SearchData.ContainsKey("dept")) {
                dynamicModel.ExtraData.Add("dept", dynamicModel.SearchData["dept"]);
                dynamicModel.SearchData.Remove("dept");
            }
            if (dynamicModel.SearchData.ContainsKey("reportdate")) {
                dynamicModel.SearchData.Remove("reportdate");
            }
            dynamicModel.SearchData.Add("reportdate", tmp);
            var isAdvance = dynamicModel.IsAdvanceQuery == 1;
            return _dynamicEntityServices.DataList(dynamicModel, isAdvance, UserId);
        }
    }
}
