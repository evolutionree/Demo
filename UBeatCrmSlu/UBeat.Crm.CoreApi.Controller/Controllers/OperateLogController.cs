using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.OperateLog;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class OperateLogController:BaseController
    {
        private readonly OperateLogServices _operateLogServices;

        public OperateLogController(OperateLogServices operateLogServices) : base(operateLogServices)
        {
            _operateLogServices = operateLogServices;
        }

        [HttpPost]
        [Route("recordlist")]
        public OutputResult<object> RecordList([FromBody] OperateLogRecordListModel recordModel = null)
        {
            if (recordModel == null) return ResponseError<object>("参数格式错误");
            return _operateLogServices.RecordList(recordModel, UserId);
        }
    }   
}
