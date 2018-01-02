using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.UkQrtz;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class QrtzController: BaseController
    {
        private readonly QrtzServices _qrtzServices;
        public QrtzController(QrtzServices qrtzServices) {
            this._qrtzServices = qrtzServices;
        }
        /// <summary>
        /// 根据查询条件返回后台事务
        /// </summary>
        /// <returns></returns>
        [HttpPost("listtrigger")]
        [AllowAnonymous]
        public OutputResult<object> ListTriggers([FromBody] ListTriggerParamInfo paramInfo ) {
            if (paramInfo == null) return ResponseError<object>("参数异常");
            try
            {
                if (paramInfo.SearchKey == null) paramInfo.SearchKey = "";
                if (paramInfo.PageIndex <1) paramInfo.PageIndex = 1;
                if (paramInfo.PageSize <= 0) paramInfo.PageSize = 10; 
                PageDataInfo<TriggerDefineInfo> list = this._qrtzServices.ListTriggers(paramInfo.SearchKey, paramInfo.SearchDeletedStatus == 1,
                            paramInfo.SearchNormalStatus == 1, paramInfo.SearchStopStatus == 1,
                            paramInfo.PageIndex, paramInfo.PageSize, UserId);
                return new OutputResult<object>(list);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
            return null;
        }
        [HttpPost("add")]
        [AllowAnonymous]
        public OutputResult<object> AddTrigger([FromBody] TriggerDefineInfo triggerInfo) {
            if (triggerInfo == null) return ResponseError<object>("参数异常");
            if (triggerInfo.RecId != null && triggerInfo.RecId.Equals(Guid.Empty) == false) {
                return ResponseError<object>("参数异常(RecId)");
            }
            try
            {
                TriggerDefineInfo newTriggerInfo = this._qrtzServices.AddTriggerDefineInfo(triggerInfo, UserId);
                return new OutputResult<object>(newTriggerInfo);
            }
            catch (Exception ex) {
                return new OutputResult<object>(null, ex.Message, -1);
            }
        }
        [HttpPost("update")]
        [AllowAnonymous]
        public OutputResult<object> UpdateTrigger([FromBody] TriggerDefineInfo triggerInfo) {
            if (triggerInfo == null )return ResponseError<object>("参数异常");
            if (triggerInfo.RecId == null || triggerInfo.RecId.Equals(Guid.Empty)) {
                return ResponseError<object>("参数异常(recid)");
            }
            try
            {
                TriggerDefineInfo newTrigger = this._qrtzServices.UpdateTriggerBaseInfo(triggerInfo, UserId);
                return new OutputResult<object>(newTrigger);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        [HttpPost("status")]
        [AllowAnonymous]
        public OutputResult<object> UpdateTriggerStatus([FromBody] UpdateTriggerStatuParamInfo paramInfo) {
            if (paramInfo == null) {
                return ResponseError<object>("参数异常");
            }
            if (paramInfo.RecId == null || paramInfo.RecId.Equals(Guid.Empty)) {
                return ResponseError<object>("参数异常(RecId)");
            }
            if (paramInfo.Status != 0 && paramInfo.Status != 1 && paramInfo.Status != 2) {
                return ResponseError<object>("参数异常(status)");
            }
            try
            {
                TriggerDefineInfo triggerInfo = this._qrtzServices.ForbitTrigger(paramInfo.RecId, paramInfo.Status, UserId);
                return new OutputResult<object>(triggerInfo);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        [HttpPost("instances")]
        [AllowAnonymous]
        public OutputResult<object> ListInstances([FromBody] ListInstanceParamInfo paramInfo) {
            if (paramInfo == null)
            {
                return ResponseError<object>("参数异常");
            }
            if (paramInfo.TriggerId == null || paramInfo.TriggerId.Equals(Guid.Empty))
            {
                return ResponseError<object>("参数异常(TriggerId)");
            }
            try
            {
                if (paramInfo.PageIndex < 1) paramInfo.PageIndex = 1;
                if (paramInfo.PageSize <= 0) paramInfo.PageSize = 10;
                PageDataInfo<TriggerInstanceInfo> list = this._qrtzServices.ListInstances(
                    paramInfo.TriggerId, paramInfo.SearchFrom, paramInfo.SearchTo,paramInfo.IsLoadArchived == 1,
                    paramInfo.PageIndex, paramInfo.PageSize, UserId
                    );
                return new OutputResult<object>(list);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
    }
    public class UpdateTriggerStatuParamInfo {
        public Guid RecId { get; set; }
        public int Status { get; set; }
    }
    public class ListTriggerParamInfo {
        public string SearchKey { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int SearchDeletedStatus { get; set; }
        public int SearchNormalStatus { get; set; }
        public int SearchStopStatus { get; set; }
    }
    public class ListInstanceParamInfo {
        public Guid TriggerId { get; set; }
        public DateTime SearchFrom { get; set; }
        public DateTime SearchTo { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int IsLoadArchived { get; set; }
    }
}
