using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Message
{
    public class MessageReceiverInfo
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public Guid MsgId { set; get; }

        /// <summary>
        /// 接收人ID
        /// </summary>
        public int UserId { set; get; }

        /// <summary>
        /// 是否已读，0未读 1已读
        /// </summary>
        public MessageReadStatus ReadStatus { set; get; }
    }

    public enum MessageReadStatus
    {
        /// <summary>
        /// 未读
        /// </summary>
        UnRead=0,
        /// <summary>
        /// 已查
        /// </summary>
        Getted = 1,
        /// <summary>
        /// 已读
        /// </summary>
        Readed =2,
    }
}
