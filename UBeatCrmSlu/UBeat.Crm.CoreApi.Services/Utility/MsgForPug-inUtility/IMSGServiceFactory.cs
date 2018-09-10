using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public interface IMSGServiceFactory
    {
        IMSGService createMsgService();
    }
}
