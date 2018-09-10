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
        [Route("getdesktops")]
        public dynamic GetDesktops([FromBody]SearchDesktop model)
        {
            return _desktopServices.GetDesktops(model, UserId);
        }
        [HttpPost]
        [Route("getdesktopcoms")]
        public dynamic GetDesktopComponents([FromBody]SearchDesktopComponent model)
        {
            return _desktopServices.GetDesktopComponents(model, UserId);
        }
        [HttpPost]
        [Route("savedesktopcomponent")]
        public dynamic SaveDesktopComponent([FromBody]DesktopComponent model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.SaveDesktopComponent(model, UserId);
        }
        [HttpPost]
        [Route("saveactualdesktopcom")]
        public dynamic SaveActualDesktopComponent([FromBody]ActualDesktopRelateToCom model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.SaveActualDesktopComponent(model, UserId);
        }
        [HttpPost]
        [Route("savedesktop")]
        public dynamic SaveDesktop([FromBody]Desktop model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.SaveDesktop(model, UserId);
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
        [Route("getactualdesktopcom")]
        public dynamic GetActualDesktopCom([FromBody]DesktopRelation model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.GetActualDesktopCom(model,UserId);
        }
        [HttpPost]
        [Route("getdesktopdetail")]
        public dynamic GetDesktopDetail([FromBody]Desktop model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.GetDesktopDetail(model);
        }
        [HttpPost]
        [Route("enabledesktop")]
        public dynamic EnableDesktop([FromBody]Desktop model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.EnableDesktop(model, UserId);
        }
        [HttpPost]
        [Route("assigncomstodesktop")]
        public dynamic AssignComsToDesktop([FromBody]ComToDesktop model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _desktopServices.AssignComsToDesktop(model, UserId);
        }
        [HttpPost]
        [Route("savedesktoprolerelate")]
        public dynamic SaveDesktopRoleRelation([FromBody]IList<DesktopRoleRelation> models)
        {
            if (models == null || models.Count == 0) return ResponseError<object>("参数格式错误");
            return _desktopServices.SaveDesktopRoleRelation(models,UserId);
        }


        [HttpPost]
        [Route("getroles")]
        public dynamic GetRoles([FromBody]DesktopRoleRelation model)
        {
            return _desktopServices.GetRoles(model, UserId);
        }


        #region 动态列表

        /// <summary>
        /// 动态列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("dynamiclist")]
        public dynamic GetDynamicList([FromBody] DynamicListRequest body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            return _desktopServices.GetDynamicList(body, UserId);
        }


        /// <summary>
        /// 获取主实体列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("entitylist")]
        public dynamic GetMainEntityList()
        {
            return _desktopServices.GetMainEntityList(UserId);
        }


        /// <summary>
        /// 获取关联实体列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("relatedentitylist")]
        public dynamic GetRelatdEntityList([FromBody] RelatdEntityListRequest body)
        {
            if (body == null)
                return ResponseError<object>("参数错误");

            return _desktopServices.GetRelatedEntityList(body.EntityId, UserId);

        }


        #endregion

    }
}
