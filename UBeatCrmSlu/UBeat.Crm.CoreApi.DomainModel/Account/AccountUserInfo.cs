using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Account
{
    public class AccountUserInfo
    {
        public int AccountId { set; get; }

        public string AccountName { set; get; }

        public int UserId { set; get; }

        public string UserName { set; get; }

        public string UserNamePinyin { set; get; }

        public Guid DepartmentId { set; get; }

        public string DepartmentCode { set; get; }

        public string DepartmentName { set; get; }

        public Guid PDepartmentId { set; get; }

        public String DDUserId { get; set; }

        public String WCUserid { get; set; }

    }
}
