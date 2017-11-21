using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.EMail;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.EMail;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.MailService.Mail.Enum;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class MailController : BaseController
    {

        private readonly EMailServices _emailServices;

        public MailController(EMailServices emailServices) : base(emailServices)
        {
            _emailServices = emailServices;
        }

        /// <summary>
        /// 发邮件
        /// </summary>
        /// <param name="emailServices"></param>
        [HttpPost]
        [Route("sendemail")]
        public OutputResult<object> SendEMailAsync([FromBody]SendEMailModel model = null)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            var header = GetAnalyseHeader();
            return _emailServices.SendEMailAsync(model, header, UserId);
        }

        /// <summary>
        /// 收邮件
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("receiveemail")]
        public OutputResult<object> ReceiveEMailAsync([FromBody] ReceiveEMailModel model = null)
        {
            if (model == null)
            {
                model = new ReceiveEMailModel();
                model.Conditon = SearchQueryEnum.None;
                model.ConditionVal = string.Empty;
                model.IsFirstInit = false;
            }
            return _emailServices.ReceiveEMailAsync(model, UserId);
        }

        #region 邮件CRUD
        [HttpPost]
        [Route("listmail")]
        public OutputResult<object> ListMail([FromBody] MailListActionParamInfo paramInfo)
        {
            PageDataInfo<MailBodyMapper> retList = _emailServices.ListMail(paramInfo, UserId);
            return new OutputResult<object>(retList);
        }
        [HttpPost]
        [Route("maildetail")]
        public OutputResult<object> MailDetail([FromBody] MailDetailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _emailServices.MailDetail(model, UserId);

        }

        /// <summary>
        /// 标记或者取消标记邮件
        /// 这里主要，如果一封邮件被一个用户放到两个文件夹了，两个文件夹的tag都要打上。
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("tagmail")]
        public OutputResult<object> tagMail([FromBody] TagMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.TagMails(model, UserId);
        }

        /// <summary>
        /// 删除邮件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("deletemail")]
        public OutputResult<object> DeleteMails([FromBody] DeleteMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.DeleteMails(model, UserId);
        }
        /// <summary>
        /// 恢复已经删除的邮件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("reconvermail")]
        public OutputResult<object> RecConverMails([FromBody] ReConverMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.ReConverMails(model, UserId);
        }
        /// <summary>
        /// 设置未已读邮件或者设置为未读邮件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("readmail")]
        public OutputResult<object> ReadMail([FromBody] ReadOrUnReadMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.ReadMail(model, UserId);
        }

        /// <summary>
        /// 获取邮件详情，只支持单个邮件获取
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [Route("gettoandfromail")]
        public OutputResult<object> GetInnerToAndFroMail([FromBody] ToAndFroModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.GetInnerToAndFroMail(model, UserId);
        }

        [HttpPost]
        [Route("gettoandfroatt")]
        public OutputResult<object> GetInnerToAndFroAttachment([FromBody] ToAndFroModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.GetInnerToAndFroAttachment(model, UserId);
        }
        [HttpPost]
        [Route("innertransfermail")]
        public OutputResult<object> InnerTransferMail([FromBody] TransferMailDataModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.InnerTransferMail(model, UserId);
        }

        /// <summary>
        /// 移动邮件，支持批量移动
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("movemail")]
        public OutputResult<object> MoveMail([FromBody] MoveMailModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");

            return _emailServices.MoveMail(model, UserId);
        }

        [HttpPost]
        [Route("getfiles")]
        public OutputResult<object> GetLocalFileFromCrm([FromBody] AttachmentListModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetLocalFileFromCrm(model, UserId);

        }

        /// <summary>
        /// 分发邮件记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("transferrecrod")]
        public OutputResult<object> distribMail([FromBody] TransferRecordParamModel model)
        {
            if (model == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetInnerTransferRecord(model, UserId);
        }


        #endregion

        #region 通讯录
        /// <summary>
        /// 模糊查询我的通讯人员
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getcontactbykeyword")]
        public OutputResult<object> GetContactByKeyword([FromBody] ContactSearchInfo paramInfo)
        {
            if (paramInfo == null) return ResponseError<object>("参数格式错误");

            return _emailServices.GetContactByKeyword(paramInfo, UserId);
        }
        /// <summary>
        /// 获取企业内部通讯录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getinnercontact")]
        public OutputResult<object> GetInnerContact([FromBody] OrgAndStaffTreeModel dynamicModel, int userId)
        {
            return _emailServices.GetInnerContact(dynamicModel, UserId);
        }
        /// <summary>
        /// 获取客户通讯录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getcustomercontact")]
        public OutputResult<object> GetCustomerContact([FromBody] MailListActionParamInfo dynamicModel, int userId)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetCustomerContact(dynamicModel, UserId);
        }

        /// <summary>
        /// 最近联系人
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getrecentcontact")]
        public OutputResult<object> GetRecentContact([FromBody] MailListActionParamInfo dynamicModel, int userId)
        {
            if (dynamicModel == null) return ResponseError<object>("参数格式错误");
            return _emailServices.GetRecentContact(dynamicModel, UserId);
        }
        #endregion
    }
}
