using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Utility.MsgForPug_inUtility
{
    public class DingdingMsgService : IMSGService
    {
        public DingdingMsgService()
        {

        }

        public string getToken()
        {
            return "";
        }

        public void updateToken(string token)
        {
            
        }

        public bool sendTextMessage(Pug_inMsg msg)
        {
            return true;
        }

        public bool sendTextCardMessage(Pug_inMsg msg)
        {
            return true;
        }

        public bool sendPictureMessage(Pug_inMsg msg)
        {
            return true;
        }

        public bool sendPicTextMessage(Pug_inMsg msg)
        {
            return true;
        }
    }
}
