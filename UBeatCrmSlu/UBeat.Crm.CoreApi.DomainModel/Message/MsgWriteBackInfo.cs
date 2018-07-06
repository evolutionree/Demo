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
    public class MsgWriteBackBizStatusInfo {
        public Guid MsgId { set; get; }
        public int ReceiverId { get; set; }
        /// <summary>
        /// 消息状态，0为未读，1为已查，2为已读
        /// </summary>
        public int BizStatus { set; get; }
        public MsgWriteBackBizStatusInfo() { }
        public MsgWriteBackBizStatusInfo(Guid msgId,int userid, int bizStatus) {
            MsgId = msgId;
            BizStatus = bizStatus;
            ReceiverId = userid;
        }
    }
}
