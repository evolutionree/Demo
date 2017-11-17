using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Message
{
    public class MsgWriteBackInfo
    {
        public MsgWriteBackInfo(Guid msgId, int msgStatus)
        {
            MsgId = msgId;
            MsgStatus = msgStatus;
        }

        /// <summary>
        /// 消息ID
        /// </summary>
        public Guid MsgId { set; get; }
        /// <summary>
        /// 消息状态，0为未读，1为已查，2为已读
        /// </summary>
        public int MsgStatus { set; get; }
    }
}
