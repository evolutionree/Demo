using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public class MsgForPug_inHelper
    {
        private static CacheServices _cacheService;

        static MsgForPug_inHelper()
        {
            _cacheService = new CacheServices();
        }

        public static bool SendMessage(MSGServiceType serviceType, MSGType msgType, Pug_inMsg msg)
        {
            string classNameFullPath = "";
            string serviceTypeName = Enum.GetName(typeof(MSGServiceType), serviceType);
            classNameFullPath = string.Format("UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility.{0}MsgServiceFactory", serviceTypeName);

            IMSGServiceFactory obj = (IMSGServiceFactory)Activator.CreateInstance(Type.GetType(classNameFullPath));
            IMSGService service = obj.createMsgService();

            var token = _cacheService.Repository.Get(serviceTypeName);
            if (token == null)
            {
                token = service.getToken();
                _cacheService.Repository.Add(serviceTypeName, token);//, new TimeSpan(3600));
                service.updateToken(token.ToString());
            }
            if (string.IsNullOrEmpty(token.ToString()))
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

        public static bool SendMessageForDingDing(MSGServiceType serviceType, MSGType msgType, Pug_inMsg msg)
        {
            string classNameFullPath = "";
            string serviceTypeName = Enum.GetName(typeof(MSGServiceType), serviceType);
            classNameFullPath = string.Format("UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility.{0}MsgServiceFactory", serviceTypeName);

            IMSGServiceFactory obj = (IMSGServiceFactory)Activator.CreateInstance(Type.GetType(classNameFullPath));
            IMSGService service = obj.createMsgService();

            var token = _cacheService.Repository.Get(serviceTypeName);
            if (token == null)
            {
                token = service.getToken();
                _cacheService.Repository.Add(serviceTypeName, token);//, new TimeSpan(3600));
            }
            service.updateToken(token.ToString());
            if (string.IsNullOrEmpty(token.ToString()))
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
