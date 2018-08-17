using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models.ScheduleTask;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class ScheduleTaskController:BaseController
    {

        private readonly ScheduleTaskServices _scheduleTaskServices;

        public ScheduleTaskController(ScheduleTaskServices scheduleTaskServices) : base(scheduleTaskServices)
        {
            _scheduleTaskServices = scheduleTaskServices;
        }

        [HttpPost]
        [Route("getscheduletaskcount")]
        public dynamic GetScheduleTaskCount([FromBody]ScheduleTaskListModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _scheduleTaskServices.GetScheduleTaskCount(model, UserId);
        }

    }
}
