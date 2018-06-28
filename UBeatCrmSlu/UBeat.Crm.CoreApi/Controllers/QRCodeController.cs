using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class QRCodeController : BaseController
    {
        private QRCodeServices _qRCodeServices;
        public QRCodeController(QRCodeServices qRCodeServices) {
            _qRCodeServices = qRCodeServices;

        }
        [HttpPost("qrcodeaction")]
        [AllowAnonymous]
        public OutputResult<object> CheckCodeAction([FromBody]QRCodeCheckParamInfo paramInfo)
        {
            return _qRCodeServices.CheckQrCode(paramInfo.Code, paramInfo.CodeType, UserId);
        }
        [HttpPost("add")]
        public OutputResult<object> Add([FromBody]QRCodeAddNewModel paramInfo) {
            if (paramInfo == null || paramInfo.RecName == null || paramInfo.RecName.Length == 0) {
                return ResponseError<object>("参数异常");
            }
            try
            {
                Guid recid = this._qRCodeServices.AddNew(paramInfo, UserId);
                return new OutputResult<object>(recid);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        [HttpPost("edit")]
        public OutputResult<object> Edit([FromBody] QRCodeEditModel paramInfo) {
            if (paramInfo == null || paramInfo.RecName == null || paramInfo.RecName.Length == 0
                || paramInfo.RecId == null || paramInfo.RecId == Guid.Empty)
            {
                return ResponseError<object>("参数异常");
            }
            try
            {
                bool isOK = this._qRCodeServices.Edit(paramInfo.RecId, paramInfo.RecName, paramInfo.Remark, UserId);
                return new OutputResult<object>(isOK);
            }
            catch (Exception ex)
            {
                return ResponseError<object>(ex.Message);
            }
        }
        [HttpPost("delete")]
        public OutputResult<object> Delete([FromBody] QRCodeDeleteModel paramInfo)  {
            if (paramInfo == null || paramInfo.RecIds == null || paramInfo.RecIds.Length == 0) {
                return ResponseError<object>("参数异常");
            }
            try
            {
                bool IsSuccess = this._qRCodeServices.Delete(paramInfo.RecIds, UserId);
                return new OutputResult<object> (IsSuccess);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        [HttpPost("list")]
        public OutputResult<object> List([FromBody] QRCodeListModel paramInfo) {
            bool isShowDisabled = false;
            if (paramInfo != null) isShowDisabled = paramInfo.IsShowDisabled;
            try
            {

                return new OutputResult<object>(this._qRCodeServices.List(isShowDisabled, UserId));
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        [HttpPost("setstatus")]
        public OutputResult<object> DisableOrEnable([FromBody] QRCodeStatusModel paramInfo )
        {
            if (paramInfo == null || paramInfo.RecIds == null || paramInfo.RecIds.Length == 0
                || paramInfo.RecStatus < 0 || paramInfo.RecStatus > 1) {
                return ResponseError<object>("参数异常");
            }
            try
            {
                return new OutputResult<object>(this._qRCodeServices.SetStatus(paramInfo.RecIds, paramInfo.RecStatus, UserId));
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        [HttpPost("orderrule")]
        public OutputResult<object> OrderRule([FromBody] QRCodeOrderModel paramInfo)
        {
            if (paramInfo == null || paramInfo.RecIds == null || paramInfo.RecIds.Length == 0) {
                return ResponseError<object>("参数异常");
            }
            try
            {
                this._qRCodeServices.OrderRule(paramInfo.RecIds, UserId);
                return new OutputResult<object>(true);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        [HttpPost("getmatchparam")]
        public OutputResult<object> GetCheckMatchParam([FromBody] QRCodeDetailModel paramInfo  ) {
            if (paramInfo == null || paramInfo.RecId == null || paramInfo.RecId == Guid.Empty) return ResponseError<object>("参数异常");
            return new OutputResult<object>(this._qRCodeServices.GetCheckMatchParam(paramInfo.RecId, UserId));
        }
        [HttpPost("updatematchparam")]
        public OutputResult<object> UpdateCheckMethod([FromBody ] QRCodeUpdateMatchRuleModel paramInfo) {
            if (paramInfo == null || paramInfo.RecId == null || paramInfo.RecId == Guid.Empty
                || paramInfo.CheckParam == null
                 ) {
                return ResponseError<object>("参数异常");
            }
            try
            {
                bool isSuccess = this._qRCodeServices.UpdateCheckParam(paramInfo.RecId, paramInfo.CheckType, paramInfo.CheckParam, UserId);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
            return null;
        }
        [HttpPost("getdealparam")]
        public OutputResult<object> GetDealParam([FromBody] QRCodeDetailModel paramInfo)
        {
            if (paramInfo == null || paramInfo.RecId == null || paramInfo.RecId == Guid.Empty) return ResponseError<object>("参数异常");
            return new OutputResult<object>(this._qRCodeServices.getDealParam(paramInfo.RecId, UserId));
        }

        [HttpPost("updatedealparam")]
        public OutputResult<object> UpdateResultMethod([FromBody] QRCodeUpdateDealParamModel paramInfo )
        {
            if (paramInfo == null || paramInfo.RecId == null || paramInfo.RecId == Guid.Empty
                || paramInfo.DealParam == null) {
                return ResponseError<object>("参数异常");
            }
            try
            {
                bool IsSuccess = this._qRCodeServices.UpdateDealParam( paramInfo.RecId, paramInfo.DealType, paramInfo.DealParam, UserId);
                return new OutputResult<object>(IsSuccess);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        [HttpPost("saverule")]
        public OutputResult<object> SaveRule() {
            return null;
        }
        [HttpPost("getrule")]
        public OutputResult<object> GetRule() {
            return null;
        }

        [HttpPost("fulltest")]
        public OutputResult<object> FullTestRules() {
            return null;
        }
        [HttpPost("testmatch")]
        public OutputResult<object> TestMatchRule([FromBody] QRCodeUpdateMatchRuleModel paramInfo) {
            return null;
        }
        [HttpPost("testdeal")]
        public OutputResult<object> TestDeail() {
            return null;
        }

    }

    public class QRCodeCheckParamInfo {
        public string Code { get; set; }
        public int CodeType { get; set; }
    }
}
