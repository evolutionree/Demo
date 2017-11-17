using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Vocation
{
    public class VocationUserInfo
    {
        public Guid VocationId { set; get; }
        public int UserId { set; get; }

        public string UserName { set; get; }

        public string NamePinyin { set; get; }

        public string UserIcon { set; get; }

        public int UserSex { set; get; }
    }
}
