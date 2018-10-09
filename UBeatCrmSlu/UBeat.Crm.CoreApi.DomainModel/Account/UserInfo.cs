using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Account
{
   public  class UserInfo
    {

        public int UserId { set; get; }

        public string UserName { set; get; }

        public string NamePinyin { set; get; }

        public string UserIcon { set; get; }

        public int UserSex { set; get; }
        public string NamePinyin_FistChar { get; set; }
        public String DDUserId { get; set; }
    }
}
