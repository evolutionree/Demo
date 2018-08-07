using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.SalesTarget;

using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Core.Utility;
using System.Security.Cryptography;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;

namespace UBeat.Crm.CoreApi.Desktop
{
    [Route("api/[controller]")]
    public class DesktopController : BaseController
    {

        private readonly DesktopServices _desktopServices;

        public DesktopController(DesktopServices desktopServices) : base(desktopServices)
        {
            _desktopServices = desktopServices;
        }

        [HttpPost]
        [Route("getdesktop")]
        public dynamic GetDesktop()
        {
            return _desktopServices.GetDesktop(UserId);
        }

        [HttpPost]
        [Route("savedesktopcomponent")]
        public dynamic SaveDesktopComponent([FromBody]DesktopComponent model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.SaveDesktopComponent(model, UserId);
        }

        [HttpPost]
        [Route("enabledesktopcomponent")]
        public dynamic EnableDesktopComponent([FromBody]DesktopComponent model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.EnableDesktopComponent(model, UserId);
        }
        [HttpPost]
        [Route("getdesktopcomdetail")]
        public dynamic GetDesktopComponentDetail([FromBody]DesktopComponent model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.GetDesktopComponentDetail(model);
        }
        [HttpPost]
        [Route("enabledesktop")]
        public dynamic EnableDesktop([FromBody]Desktop model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.EnableDesktop(model, UserId);
        }
        [HttpPost]
        [Route("savedesktoprolerelat")]
        public dynamic SaveDesktopRoleRelation([FromBody]IList<DesktopRoleRelation> models)
        {
            if (models == null || models.Count == 0) return ResponseError<object>("参数格式错误");
            return _desktopServices.SaveDesktopRoleRelation(models);
        }
        [HttpPost]
        [Route("getroles")]
        public dynamic GetRoles()
        {
            return _desktopServices.GetRoles(UserId);
        }
    }
}
