using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public class WeChatMsgServiceFactory : IMSGServiceFactory
    {
        public IMSGService createMsgService()
        {
            return new WeChatMsgService();
        }
    }
}
