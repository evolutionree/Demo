using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Utility;
using UBeat.Crm.CoreApi.Services.Models.SalesTarget;
using UBeat.Crm.CoreApi.Models;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using UBeat.Crm.LicenseCore;
using MessagePack;
using MessagePack.Resolvers;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DockingAPIController : BaseController
    {
        private readonly DockingAPIServices _dockingAPIServices;
        public DockingAPIController(DockingAPIServices dockingAPIServices) {
            this._dockingAPIServices = dockingAPIServices;
        }
        [HttpPost]
        [Route("getbusinesslist")]
        public OutputResult<object> GetBusinessList([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetBusinessList(api);
        }
        [HttpPost]
        [Route("getyearreport")]
        public OutputResult<object> GetYearReport([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetYearReport(api);
        }
        [HttpPost]
        [Route("getcasedetail")]
        public OutputResult<object> GetCaseDetail([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetCaseDetail(api);
        }
        [HttpPost]
        [Route("getcourtnotice")]
        public OutputResult<object> GetCourtNotice([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetCourtNotice(api);
        }
        [HttpPost]
        [Route("getbreakpromise")]
        public OutputResult<object> GetBuildBreakPromise([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetBuildBreakPromise(api);
        }
    }
}
