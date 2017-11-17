using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Message;

namespace UBeat.Crm.CoreApi.Services.Models.Message
{
    public class UnreadMsgParameter
    {
        public List<MessageGroupType> MsgGroupIds { set; get; }
    }
}
