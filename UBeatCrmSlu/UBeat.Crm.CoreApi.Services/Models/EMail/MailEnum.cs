using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.EMail
{
    //1=等待，2=内部审查中，3=内部审查完毕，4=等待发送，5=发送中，6=发送成功，7=发送失败

    public enum MailStatus
    {
        Waiting = 1,
        InnerCheck = 2,
        InnerCheckFinish = 3,
        WaitingSend = 4,
        Sending = 5,
        SendSuccess = 6,
        SendFail = 7
    }

    public enum MailActionType
    {
        InnerCheck = 1,
        ExternalSend = 2
    }

    public enum RelatedMySelf
    {
        RelatedCurrent = 0,
        RelatedMySelf = 1,
        RelatedAllUser = 2
    }
    public enum RelatedSendOrReceive
    {
        AllSendOrReceive = 0,
        AllReceive = 1,
        AllSend = 2
    }
}
