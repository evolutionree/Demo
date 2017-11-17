using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Chat
{
    public class ChatInsert: BaseEntity
    {
        /// <summary>
        /// 0私聊  1群聊
        /// </summary>
        public int ChatType { set; get; }

        /// <summary>
        /// 群聊时群id
        /// </summary>
        public Guid GroupId { set; get; }
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
        /// 消息类型 ：1聊天消息  2创建群组  3删除群 4加入群组 5退出群组 6群主踢人 7修改群信息 8设置管理员
        /// </summary>
        public int MsgType { set; get; } = 1;

        public int UserNo { set; get; }
        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ChatInsert>
        {
            public Validator()
            {
                RuleFor(d => d.ChatContent).NotNull().NotEmpty().WithMessage("ChatContent不能为空");
                RuleFor(d => d.ContentType).InclusiveBetween(1, 5).WithMessage("ContentType必须1到5");
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }
        
    }

    public class ChatInsertResult
    {
        public Guid GroupId { set; get; }

        public Guid EntityId { set; get; }

        public Guid BusinessId { set; get; }

        public string Receivers { set; get; }

        public int MsgType { set; get; }

        public int ConType { set; get; }

        public long RecVersion { set; get; }

    }


    public class ChatSelect : BaseEntity
    {
        /// <summary>
        /// 群聊时群id
        /// </summary>
        public Guid GroupId { set; get; }
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

        public int UserNo { set; get; }
        protected override IValidator GetValidator()
        {
            return new Validator();
        }
        class Validator : AbstractValidator<ChatSelect>
        {
            public Validator()
            {
               
                RuleFor(d => d.UserNo).GreaterThan(0).WithMessage("UserNo无效，请重新登录");
            }
        }

    }




}
