using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Chat
{

    public class AddGroupModel
    {
        public string GroupName { set; get; }
        /// <summary>
        /// 分组类型 1：讨论群  2：部门群  3：商机群
        /// </summary>
        public int GroupType { set; get; }

        public Guid? EntityId { set; get; } = Guid.Empty;

        public Guid? BusinessId { set; get; } = Guid.Empty;
        /// <summary>
        /// 群头像
        /// </summary>
        public string GroupIcon { set; get; }

        /// <summary>
        /// 成员id，如用户id
        /// </summary>
        public List<int> MemberIds { set; get; }


    }

    public class UpdateGroupModel
    {
        public Guid? GroupId { set; get; } = Guid.Empty;
        public string GroupName { set; get; }
        
        /// <summary>
        /// 群头像
        /// </summary>
        //public string GroupIcon { set; get; }
    }
    public class AddMembersModel
    {
        public Guid? GroupId { set; get; } = Guid.Empty;
        /// <summary>
        /// 成员id，如用户id
        /// </summary>
        public List<int> MemberIds { set; get; }
    }

    public class SetMembersModel
    {
        public Guid? GroupId { set; get; } = Guid.Empty;
        /// <summary>
        /// 成员id
        /// </summary>
        public int Memberid { set; get; }
        /// <summary>
        /// 操作类型：0为设置管理员，1为取消管理员，2为设置屏蔽群，3为取消屏蔽群，4为管理员踢人
        /// </summary>
        public int OperateType { set; get; }
    }

    public class GetMembersModel
    {
        public Guid? GroupId { set; get; }

        /// <summary>
        /// 版本号
        /// </summary>
        public long RecVersion { set; get; }
    }

    public class DeleteGroupModel
    {
        public Guid? GroupId { set; get; }
        /// <summary>
        /// 操作类型，0为主动退群,1为管理员解散群
        /// </summary>
        public int OperateType { set; get; }
    }
    

    public class GrouplistModel
    {
        /// <summary>
        /// 分组类型 1：讨论群  2：部门群  3：商机群
        /// </summary>
        public int GroupType { set; get; }

        public Guid? EntityId { set; get; } = Guid.Empty;

        public Guid? BusinessId { set; get; } = Guid.Empty;
        /// <summary>
        /// 版本号
        /// </summary>
        public long RecVersion { set; get; }
    }

    public class SendChatModel
    {
        /// <summary>
        /// 群聊时群id
        /// </summary>
        public Guid? GroupId { set; get; } = Guid.Empty;
        /// <summary>
        /// 单聊时，好友的id
        /// </summary>
        public int FriendId { set; get; }

        public string ChatContent { set; get; }
        /// <summary>
        /// 1文字  2图片  3录音 4位置 5文件
        /// </summary>
        public int ContentType { set; get; }

        /// <summary>
        /// 0私聊  1群聊
        /// </summary>
        public int ChatType { set; get; }

    }

    public class ChatListModel
    {
        /// <summary>
        /// 群聊时群id
        /// </summary>
        public Guid? GroupId { set; get; } = Guid.Empty;
        /// <summary>
        /// 单聊时，好友的id
        /// </summary>
        public int FriendId { set; get; }

        /// <summary>
        /// 是否历史记录，0为否，1为是
        /// </summary>
        public int IsHistory { set; get; }

        /// <summary>
        /// 版本号
        /// </summary>
        public long RecVersion { set; get; }
    }

    public class ReadedCallbackModel
    {
        public List<string> ChatMsgIds { set; get; }
    }

    public class ChatUnreadListModel
    {
        /// <summary>
        /// 版本号
        /// </summary>
        public long RecVersion { set; get; }
    }

}
