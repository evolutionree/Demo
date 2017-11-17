using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Notify
{
    public class NotifyEntity
    {
        public int msggroupid { set; get; }
        public Guid msgdataid { set; get; }
        public Guid entityid { set; get; }
        public int msgtype { set; get; }
        public string msgtitle { set; get; }
        public string msgcontent { set; get; }
        public string msgstatus { set; get; }
        public string msgparam { set; get; }
        public DateTime sendtime { set; get; }
        public string receiver { set; get; }
        public int userno { set; get; }

        
    }

    public class NotifyEntityExt: NotifyEntity
    {
        public string username { set; get; }
        public string usericon { set; get; }
    }
}
