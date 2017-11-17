using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Notice;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class NoticeController : BaseController
    {
        private readonly NoticeServices _noticeServices;

        public NoticeController(NoticeServices noticeServices) : base(noticeServices)
        {
            _noticeServices = noticeServices;
        }

        [HttpPost]
        [Route("querynotice")]
        public OutputResult<object> NoticeQuery([FromBody]NoticeListModel notice = null)
        {
            if (notice == null) return ResponseError<object>("参数格式错误");
            //WriteOperateLog("安卓端调试", notice.KeyWord);
            return _noticeServices.NoticeQuery(notice, UserId);
        }
        [HttpPost]
        [Route("querymobnotice")]
        public OutputResult<object> NoticeMobQuery([FromBody]NoticeListModel notice = null)
        {
            if (notice == null) return ResponseError<object>("参数格式错误");

            return _noticeServices.NoticeMobQuery(notice, UserId);
        }

        [HttpPost]
        [Route("querynoticesendrecord")]
        public OutputResult<object> NoticeSendRecordQuery([FromBody]NoticeSendRecordModel notice = null)
        {
            if (notice == null) return ResponseError<object>("参数格式错误");

            return _noticeServices.NoticeSendRecordQuery(notice, UserId);
        }
        [HttpPost]
        [Route("querynoticeversion")]
        public OutputResult<object> NoticeVersionHistoryQuery([FromBody]NoticeListModel notice = null)
        {
            if (notice == null) return ResponseError<object>("参数格式错误");

            return _noticeServices.NoticeVersionHistoryQuery(notice, UserId);
        }

        [HttpPost]
        [Route("querynoticeinfo")]
        public OutputResult<object> NoticeInfoQuery([FromBody]NoticeListModel notice = null)
        {
            if (notice == null) return ResponseError<object>("参数格式错误");

            return _noticeServices.NoticeInfoQuery(notice, UserId);
        }

        [HttpPost]
        [Route("insertnotice")]
        public OutputResult<object> InsertNotice([FromBody]NoticeModel notice = null)
        {
            if (notice == null) return ResponseError<object>("参数格式错误");

            return _noticeServices.InsertNotice(notice, UserId);
        }

        [HttpPost]
        [Route("updatenotice")]
        public OutputResult<object> UpdateNotice([FromBody]NoticeModel notice = null)
        {
            if (notice == null) return ResponseError<object>("参数格式错误");

            return _noticeServices.UpdateNotice(notice, UserId);
        }

        [HttpPost]
        [Route("disablednotice")]
        public OutputResult<object> DisabledNotice([FromBody]NoticeDisabledModel notice = null)
        {
            if (notice == null) return ResponseError<object>("参数格式错误");

            return _noticeServices.DisabledNotice(notice, UserId);
        }
        [HttpPost]
        [Route("updatenoticereadflag")]
        public OutputResult<object> DisabledNotice([FromBody]NoticeReadFlagModel notice = null)
        {
            if (notice == null) return ResponseError<object>("参数格式错误");

            return _noticeServices.UpdateNoticeReadFlag(notice, UserId);
        }
        [HttpPost]
        [Route("sendnotice")]
        public OutputResult<object> SendNoticeToUser([FromBody]NoticeReceiverModel noticeReceiver = null)
        {
            if (noticeReceiver == null) return ResponseError<object>("参数格式错误");

            return _noticeServices.SendNoticeToUser(noticeReceiver, UserId);
        }

    }
}
