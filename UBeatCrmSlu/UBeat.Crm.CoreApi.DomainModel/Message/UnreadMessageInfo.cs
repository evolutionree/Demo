using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Message
{
    public class UnreadMessageInfo
    {
        /// <summary>
        /// 消息分组ID，用于对消息分类
        /// </summary>
        public MessageGroupType MsgGroupId { set; get; }


        public long Count { set; get; }
    }
}
