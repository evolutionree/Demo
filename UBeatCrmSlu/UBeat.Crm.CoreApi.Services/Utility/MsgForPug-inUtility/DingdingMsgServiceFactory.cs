using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public class DingdingMsgServiceFactory : IMSGServiceFactory
    {
        public IMSGService createMsgService()
        {
		    return new DingdingMsgService();
        }
    }
}
