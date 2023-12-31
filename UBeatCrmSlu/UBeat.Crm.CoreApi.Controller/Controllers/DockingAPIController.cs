﻿using System;
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
        public DockingAPIController(DockingAPIServices dockingAPIServices)
        {
            this._dockingAPIServices = dockingAPIServices;
        }
        [HttpPost]
        [Route("getbusinesslist")]
        public OutputResult<object> GetBusinessList([FromBody]CompanyModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetBusinessList(api, UserId);
        }
        [HttpPost]
        [Route("updatebusiinfo")]
        public OutputResult<object> UpdateBusinessInfomation([FromBody]CompanyModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.UpdateBusinessInfomation(api, UserId);
        }
        [HttpPost]
        [Route("updateforebusiinfo")]
        public OutputResult<object> UpdateForeignBusinessInfomation([FromBody]CompanyModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.UpdateForeignBusinessInfomation(api, UserId);
        }
        [HttpPost]
        [Route("getforebusinessdetail")]
        public OutputResult<object> GetForeignBusinessDetail([FromBody]CompanyModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetForeignBusinessDetail(api, 0, UserId);
        }
        [HttpPost]
        [Route("saveforebusidetail")]
        public OutputResult<object> SaveForeignBusinessDetail([FromBody]CompanyModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.SaveForeignBusinessDetail(api, 1, UserId);
        }
        [HttpPost]
        [Route("getbusinessdetail")]
        public OutputResult<object> GetBusinessDetail([FromBody]CompanyModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetBusinessDetail(api, 0, UserId);
        }
        
        [HttpGet("checkCompanyInfo")]
        public OutputResult<object> checkQccCustomer([FromBody]CompanyModel api)
        {
            //CompanyName 传统一社会信用编码
            var result = _dockingAPIServices.BuildCompanyRunningInfo(new DockingAPIModel
                {CompanyName = api.CompanyName, AppKey = api.AppKey, Secret = api.Secret});
            return new OutputResult<object>(result);
        }
        [HttpPost]
        [Route("getyearreport")]
        public OutputResult<object> GetYearReport([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetYearReport(api, UserId);
        }
        [HttpPost]
        [Route("getcasedetail")]
        public OutputResult<object> GetCaseDetail([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetCaseDetail(api, UserId);
        }
        [HttpPost]
        [Route("getlawsuit")]
        public OutputResult<object> GetLawSuit([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetLawSuit(api, UserId);
        }
        [HttpPost]
        [Route("getcourtnotice")]
        public OutputResult<object> GetCourtNotice([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetCourtNotice(api, UserId);
        }
        [HttpPost]
        [Route("getbreakpromise")]
        public OutputResult<object> GetBuildBreakPromise([FromBody]DockingAPIModel api)
        {
            if (api == null) return new OutputResult<object>("参数异常");
            return _dockingAPIServices.GetBuildBreakPromise(api, UserId);
        }
    }
}
