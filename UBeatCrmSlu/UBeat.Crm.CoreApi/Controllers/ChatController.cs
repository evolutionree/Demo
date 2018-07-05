using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Excels;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.Chat;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    /// <summary>
    /// 聊天服务控制器
    /// </summary>
    [Route("api/[controller]")]
    public class ChatController : BaseController
    {
        private readonly ILogger<ExcelController> _logger;

        private readonly ChatServices _service;

        public ChatController(ILogger<ExcelController> logger, ChatServices service) : base(service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost("addgroup")]
        public OutputResult<object> AddGroup([FromBody] AddGroupModel bodyData)
        {
            return _service.AddGroup(bodyData, UserId);
        }

        [HttpPost("updategroup")]
        public OutputResult<object> UpdateGroup([FromBody] UpdateGroupModel bodyData)
        {
            return _service.UpdateGroup(bodyData, UserId);
        }

        [HttpPost("addmembers")]
        public OutputResult<object> AddMembers([FromBody] AddMembersModel bodyData)
        {
            return _service.AddMembers(bodyData, UserId);
        }
        [HttpPost("updatemembers")]
        public OutputResult<object> UpdateMembers([FromBody] AddMembersModel bodyData)
        {
            try
            {

                return _service.UpdateMembers(bodyData, LoginUser.UserName, UserId);
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }
        //设置成员：设置管理员，取消管理员，设置屏蔽群，取消屏蔽群
        [HttpPost("setmembers")]
        public OutputResult<object> SetMembers([FromBody] SetMembersModel bodyData)
        {
            return _service.SetMembers(bodyData, UserId);
        }

        [HttpPost("getmembers")]
        public OutputResult<object> GetMembers([FromBody] GetMembersModel bodyData)
        {
            return _service.GetMembers(bodyData, UserId);
        }

        [HttpPost("deletegroup")]
        public OutputResult<object> DeleteGroup([FromBody] DeleteGroupModel bodyData)
        {
            return _service.DeleteGroup(bodyData, UserId);
        }
       
        [HttpPost("grouplist")]
        public OutputResult<object> Grouplist([FromBody] GrouplistModel bodyData)
        {
            return _service.Grouplist(bodyData, UserId);
        } 

        [HttpPost("send")]
        public OutputResult<object> SendChat([FromBody] SendChatModel bodyData)
        {
            return _service.SendChat(bodyData, UserId);
        }
        
        [HttpPost("list")]
        public OutputResult<object> ChatList([FromBody] ChatListModel bodyData)
        {
            return _service.ChatList(bodyData, UserId);
        }

        [HttpPost("unreadlist")]
        public OutputResult<object> ChatUnreadList([FromBody] ChatUnreadListModel bodyData)
        {
            
            return _service.ChatUnreadList(UserId, bodyData==null?0: bodyData.RecVersion);
        }


        [HttpPost("readedcallback")]
        public OutputResult<object> ReadedCallback([FromBody] ReadedCallbackModel bodyData)
        {
            return _service.ReadedCallback(bodyData,UserId);
        }
        [HttpPost("recentchat")]
        public OutputResult<object> RecentChatList() {
            List<IDictionary<string, object>> ret = this._service.GetRecentChatList(UserId);
            return new OutputResult<object>(ret);
        }
        [HttpPost("userlist")]
        public OutputResult<object> UserList([FromBody]ChatUserListModel paramInfo)
        {
            string key = "";
            if (paramInfo != null) key = paramInfo.SearchKey;
            return _service.GetUserList(key, UserId);
        }
    }
}
