using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Notice;
using UBeat.Crm.CoreApi.Services.Models.Notify;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Message;
using UBeat.Crm.CoreApi.DomainModel.Message;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class NotifyController:BaseController
    {
        private readonly NotifyServices _notifyServices;

        public NotifyController(NotifyServices notifyServices) : base(notifyServices)
        {
            _notifyServices = notifyServices;
        }

        [HttpPost]
        [Route("list")]
        public OutputResult<object> FetchMessage([FromBody]NotifyFetchModel versionModel = null)
        {
            if (versionModel == null) return ResponseError<object>("参数格式错误");

            return _notifyServices.FetchMessage(versionModel, UserId);
        }

        [HttpPost]
        [Route("msglist")]
        public OutputResult<object> GetMessageList([FromBody]PageParamModel data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _notifyServices.GetMessageList(data, UserId);
        }


        [HttpPost]
        [Route("writeread")]
        public OutputResult<object> WriteReadStatus([FromBody]NotifyReadModel readModel = null)
        {
            if (readModel == null) return ResponseError<object>("参数格式错误");
            return _notifyServices.WriteReadStatus(readModel, UserId);
        }



        [HttpPost]
        [Route("pagelist")]
        public OutputResult<object> GetPageMessageList([FromBody]PageMsgsParameter data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");

            return _notifyServices.GetPageMessageList(data, UserId);
        }

        [HttpPost]
        [Route("vertionmsglist")]
        public OutputResult<object> GetIncrementMessageList([FromBody]IncrementMsgsParameter data = null)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _notifyServices.GetIncrementMessageList(data, UserId);
        }
        //消息未读统计
        [HttpPost]
        [Route("unreadcount")]
        public OutputResult<object> StatisticUnreadMessage([FromBody]UnreadMsgParameter msgGroupIds)
        {
            if (msgGroupIds == null) return ResponseError<object>("参数格式错误");
            return _notifyServices.StatisticUnreadMessage(msgGroupIds.MsgGroupIds, UserId);
        }

        //消息回写
        [HttpPost]
        [Route("writeback")]
        public OutputResult<object> MessageWriteBack([FromBody]WriteBackParameter messageids)
        {
            if (messageids == null) return ResponseError<object>("参数格式错误");
            return _notifyServices.MessageWriteBack(messageids.MsgIds, UserId);
        }
        
        //写消息
        [HttpPost]
        [Route("writemessageasyn")]
        [AllowAnonymous]
        public OutputResult<object> WriteMessageAsyn([FromBody]MessageParameter data)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _notifyServices.WriteMessageAsyn(data, UserId);
        }

        [HttpPost]
        [Route("messageread")]
        public OutputResult<object> MessageRead([FromBody] MsgStuausParameter data)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            return _notifyServices.MessageRead(data, UserId);
        }

        [HttpPost]
        [Route("deletemessage")]
        public OutputResult<object> DeleteMessage([FromBody] MsgStuausParameter data)
        {
            if (data == null) return ResponseError<object>("参数格式错误");
            if (!string.IsNullOrEmpty(data.MessageIds))
            {
                var ids = data.MessageIds.Split(',');
                var newGuid = Guid.Empty;
                for (int i = 0; i < ids.Count(); i++)
                {
                    if (!Guid.TryParse(ids[i], out newGuid))
                        return ResponseError<object>("<MsgId>格式（guid）错误]");
                }
            }
            return _notifyServices.DeleteMessage(data, UserId);
        }
    }
}
