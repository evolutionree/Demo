using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NLog;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.BasicData;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Notify;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class BasicDataController : BaseController
    {
        private static readonly Logger Logger = LogManager.GetLogger(typeof(BasicDataController).FullName);

        private readonly BasicDataServices _basicDataServices;

        public BasicDataController(BasicDataServices basicDataServices):base(basicDataServices)
        {
            _basicDataServices = basicDataServices;
        }

        [HttpPost]
        [Route("message")]
        public OutputResult<object> Message([FromBody] BasicDataMessageModel messageModel = null)
        {
            if (messageModel == null) return ResponseError<object>("参数格式错误");

            return _basicDataServices.GetMessageList(messageModel, UserId);
        }

        //[HttpPost]
        //[Route("sync")]
        //public OutputResult<object> SyncData([FromBody] BasicDataSyncModel syncModel = null)
        //{
        //    if (syncModel == null) return ResponseError<object>("参数格式错误");

        //    return _basicDataServices.SyncData(syncModel, UserId);
        //}

        [HttpPost]
        [Route("syncbasic")]
        public OutputResult<object> SyncDataBasic([FromBody] BasicDataSyncModel syncModel = null)
        {
            if (syncModel == null) return ResponseError<object>("参数格式错误");

            return _basicDataServices.SyncDataBasic(syncModel, UserId);
        }

        [HttpPost]
        [Route("syncentity")]
        public OutputResult<object> SyncDataEntity([FromBody] BasicDataSyncModel syncModel = null)
        {
            if (syncModel == null) return ResponseError<object>("参数格式错误");

            return _basicDataServices.SyncDataEntity(syncModel, UserId);
        }

        [HttpPost]
        [Route("syncdelentity")]
        public OutputResult<object> SyncDelDataEntity()
        {

            return _basicDataServices.SyncDelDataEntity(UserId);
        }

        [HttpPost]
        [Route("syncview")]
        public OutputResult<object> SyncDataView([FromBody] BasicDataSyncModel syncModel = null)
        {
            if (syncModel == null) return ResponseError<object>("参数格式错误");

            return _basicDataServices.SyncDataView(syncModel, UserId);
        }

        [HttpPost]
        [Route("synctemplate")]
        public OutputResult<object> SyncDataTemplate([FromBody] BasicDataSyncModel syncModel = null)
        {
            if (syncModel == null) return ResponseError<object>("参数格式错误");

            return _basicDataServices.SyncDataTemplate(syncModel, UserId);
        }

        [HttpPost]
        [Route("dept")]
        public OutputResult<object> DeptData([FromBody] BasicDataDeptModel deptModel = null)
        {
            if (deptModel == null) return ResponseError<object>("参数格式错误");
            return _basicDataServices.DeptData(deptModel, UserId);
        }

        [HttpPost]
        [Route("deptpower")]
        public OutputResult<object> DeptPowerData([FromBody] BasicDataDeptModel deptModel = null)
        {
            if (deptModel == null) return ResponseError<object>("参数格式错误");
            return _basicDataServices.DeptPowerData(deptModel, UserId);
        }
        [HttpPost]
        [Route("usercontact")]
        public OutputResult<object> UserContactList([FromBody] BasicDataUserContactListModel contactListModel = null)
        {
            if (contactListModel == null) return ResponseError<object>("参数格式错误");
            return _basicDataServices.UserContactList(contactListModel, UserId);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("servertime")]
        public OutputResult<object> ServerTime()
        {
            var serverTime = new { ServiceTime = DateTime.Now };
            return new OutputResult<object>(serverTime);
        }

        [HttpPost]
        [Route("funccount")]
        public OutputResult<object> FuncCount()
        {
            return _basicDataServices.FuncCount(UserId);
        }

        [HttpPost]
        [Route("funccountlist")]
        public OutputResult<object> FuncCountList([FromBody] BasicDataFuncCountListModel funcCountModel = null)
        {
            return _basicDataServices.FuncCountList(funcCountModel, UserId);
        }


        #region 统计指标

        [HttpPost]
        [Route("queryanalysefunc")]
        public OutputResult<object> AnalyseFuncQuery([FromBody] AnalyseListModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _basicDataServices.AnalyseFuncQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("addanalysefunc")]
        public OutputResult<object> InsertAnalyseFunc([FromBody] AddAnalyseModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _basicDataServices.InsertAnalyseFunc(entityModel, UserId);
        }
        [HttpPost]
        [Route("updateanalysefunc")]
        public OutputResult<object> UpdateAnalyseFunc([FromBody] EditAnalyseModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _basicDataServices.UpdateAnalyseFunc(entityModel, UserId);
        }
        [HttpPost]
        [Route("disabledanalysefunc")]
        public OutputResult<object> DisabledAnalyseFunc([FromBody] DisabledOrOderbyAnalyseModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _basicDataServices.DisabledAnalyseFunc(entityModel, UserId);
        }

        [HttpPost]
        [Route("orderbyanalysefunc")]
        public OutputResult<object> OrderByAnalyseFunc([FromBody] DisabledOrOderbyAnalyseModel entityModel = null)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _basicDataServices.OrderByAnalyseFunc(entityModel, UserId);
        }
        #endregion
    }
}
