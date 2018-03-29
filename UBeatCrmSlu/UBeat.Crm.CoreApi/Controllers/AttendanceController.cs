using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.Attendance;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class AttendanceController:BaseController
    { 
        private static readonly Logger Logger = LogManager.GetLogger(typeof(AttendanceController).FullName);

        private readonly AttendanceServices _attendanceServices;

        public AttendanceController(AttendanceServices attendanceServices):base(attendanceServices)
        {
            _attendanceServices = attendanceServices;
        }

        [HttpPost]
        [Route("sign")]
        public OutputResult<object> Sign([FromBody] AttendanceSignModel signModel = null)
        {
            if (signModel == null) return ResponseError<object>("参数格式错误");
            WriteOperateLog("提交考勤信息", signModel);
            return _attendanceServices.Sign(signModel, UserId);
        }

        [HttpPost]
        [Route("addgroupuser")]
        public OutputResult<object> AddGroupUser([FromBody] AddGroupUserModel settingList = null)
        {
            if (settingList == null) return ResponseError<object>("参数格式错误");
            var header = GetAnalyseHeader();
            return _attendanceServices.AddGroupUser(settingList, header,UserId);
        }

        [HttpPost]
        [Route("querygroupuser")]
        public OutputResult<object> GroupUserQuery([FromBody]GroupUserModel settingList = null)
        {
            if (settingList == null) return ResponseError<object>("参数格式错误");
            var header = GetAnalyseHeader();
            return _attendanceServices.GroupUserQuery(settingList, UserId);
        }

        [HttpPost]
        [Route("signlist")]
        public OutputResult<object> SignList([FromBody] AttendanceSignListModel listModel = null)
        {
            if (listModel == null) return ResponseError<object>("参数格式错误");
            return _attendanceServices.SignList(listModel, UserId);
        }
    }
}
