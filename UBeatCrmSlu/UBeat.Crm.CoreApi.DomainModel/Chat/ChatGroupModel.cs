using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Chat
{
    public class ChatGroupModel
    {
        
        public Guid chatgroupid { set; get; }
        public string chatgroupname { set; get; }

        public string pinyinname { set; get; }

        public int grouptype { set; get; }

        public Guid entityid { set; get; }

        public Guid businessid { set; get; }

        public int recstatus { set; get; }

        public DateTime reccreated { set; get; }

        public DateTime recupdated { set; get; }

        public int reccreator { set; get; }

        public int recupdator { set; get; }

        public long recversion { set; get; }
    }
}
