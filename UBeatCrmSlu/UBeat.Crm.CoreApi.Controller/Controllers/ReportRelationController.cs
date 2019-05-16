using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.ReportRelation;
using UBeat.Crm.CoreApi.Services.Models.StatisticsSetting;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class ReportRelationController : BaseController
    {
        private readonly ReportRelationServices _reportRelationServices;
        public ReportRelationController(ReportRelationServices reportRelationServices) : base(reportRelationServices)
        {
            _reportRelationServices = reportRelationServices;
        }

        [HttpPost]
        [Route("getreportrelation")]
        [AllowAnonymous]
        public OutputResult<object> GetReportRelationListData([FromBody]QueryReportRelationModel model)
        {
            var result = _reportRelationServices.GetReportRelationListData(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("addreportrelation")]
        [AllowAnonymous]
        public OutputResult<object> AddReportRelation([FromBody]AddReportRelationModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _reportRelationServices.AddReportRelation(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("updatereportrelation")]
        [AllowAnonymous]
        public OutputResult<object> UpdateReportRelation([FromBody]EditReportRelationModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _reportRelationServices.UpdateReportRelation(model, UserId);
            return result;
        }


        [HttpPost]
        [Route("addreportreldetail")]
        [AllowAnonymous]
        public OutputResult<object> AddReportRelDetail([FromBody]AddReportRelDetailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _reportRelationServices.AddReportRelDetail(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("updatereportreldetail")]
        [AllowAnonymous]
        public OutputResult<object> UpdateReportRelDetail([FromBody]EditReportRelDetailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _reportRelationServices.UpdateReportRelDetail(model, UserId);
            return result;
        }


        [HttpPost]
        [Route("getreportreldetail")]
        [AllowAnonymous]
        public OutputResult<object> GetReportRelDetailListData([FromBody]QueryReportRelDetailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _reportRelationServices.GetReportRelDetailListData(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("deletereportreldetail")]
        [AllowAnonymous]
        public OutputResult<object> DeleteReportRelDetail([FromBody]DeleteReportRelDetailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _reportRelationServices.DeleteReportRelDetail(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("deletereportrelation")]
        [AllowAnonymous]
        public OutputResult<object> DeleteReportRelation([FromBody]DeleteReportRelationModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _reportRelationServices.DeleteReportRelation(model, UserId);
            return result;
        }
    }
}
