using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using Microsoft.Extensions.Configuration;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public class MsgForPug_inHelper
    {
        public static bool SendMessage(MSGServiceType serviceType, MSGType msgType, Pug_inMsg msg)
        {
            string classNameFullPath = "";
            string serviceTypeName = Enum.GetName(typeof(MSGServiceType), serviceType);
            classNameFullPath = string.Format("UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility.{0}MsgServiceFactory", serviceTypeName);
            
            IMSGServiceFactory obj = (IMSGServiceFactory)Activator.CreateInstance(Type.GetType(classNameFullPath));
            IMSGService service = obj.createMsgService();
            string token = service.updateToken();
            //todo 缓存token
            if (string.IsNullOrEmpty(token))
                return false;

            switch (msgType)
            {
                case MSGType.Text:
                    {
                        service.sendTextMessage(msg);
                        break;
                    }
                case MSGType.TextCard:
                    {
                        service.sendTextCardMessage(msg);
                        break;
                    }
                case MSGType.Picture:
                    {
                        service.sendPictureMessage(msg);
                        break;
                    }
                case MSGType.PicText:
                    {
                        service.sendPicTextMessage(msg);
                        break;
                    }
            }
            return true;
        }
    }
}
