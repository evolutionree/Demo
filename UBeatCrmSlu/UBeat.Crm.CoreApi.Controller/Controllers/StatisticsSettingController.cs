using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.StatisticsSetting;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class StatisticsSettingController : BaseController
    {
        private readonly StatisticsSettingServices _statisticsSettingServices;

        [HttpPost]
        [Route("getstatistics")]

        public OutputResult<object> GetStatisticsListData([FromBody]QueryStatisticsSettingModel version)
        {
            var result = _statisticsSettingServices.GetStatisticsListData(version, UserId);
            return result;
        }

        public StatisticsSettingController(StatisticsSettingServices statisticsSettingServices) : base(statisticsSettingServices)
        {
            _statisticsSettingServices = statisticsSettingServices;
        }
        [HttpPost]
        [Route("addstatistics")]

        public OutputResult<object> AddScriptManager([FromBody]AddStatisticsSettingModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _statisticsSettingServices.AddStatisticsSetting(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("updatestatistics")]

        public OutputResult<object> UpdateScriptManager([FromBody]EditStatisticsSettingModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _statisticsSettingServices.UpdateStatisticsSetting(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("deletestatistics")]

        public OutputResult<object> DeleteScriptManager([FromBody]DeleteStatisticsSettingModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _statisticsSettingServices.DeleteStatisticsSetting(model, UserId);
            return result;
        }
        [HttpPost]
        [Route("disabledstatistics")]

        public OutputResult<object> DisabledStatisticsSetting([FromBody]DeleteStatisticsSettingModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _statisticsSettingServices.DisabledStatisticsSetting(model, UserId);
            return result;
        }


        [HttpPost]
        [Route("getstatisticsdata")]

        public OutputResult<object> GetStatisticsData([FromBody]QueryStatisticsModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _statisticsSettingServices.GetStatisticsData(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("getstatisticsdetaildata")]

        public OutputResult<object> GetStatisticsDetailData([FromBody]QueryStatisticsModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _statisticsSettingServices.GetStatisticsDetailData(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("updatestatisticsgroupsetting")]

        public OutputResult<object> UpdateStatisticsGroupSetting([FromBody]EditStatisticsGroupModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _statisticsSettingServices.UpdateStatisticsGroupSetting(model, UserId);
            return result;
        }

        [HttpPost]
        [Route("savestatisticsgroupsumsetting")]

        public OutputResult<object> SaveStatisticsGroupSumSetting([FromBody]SaveStatisticsGroupModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var result = _statisticsSettingServices.SaveStatisticsGroupSumSetting(model, UserId);
            return result;
        }

    }
}
