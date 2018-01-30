using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Contact
{
    public class ContactMapper
    {
    }

    public class LinkManMapper
    {
        public int userid { get; set; }
        public Boolean flag { get; set; } = false;
        public string username { get; set; }
        public string deptname { get; set; }
        public string userphone { get; set; }
        public string usertel { get; set; }
        public string useremail { get; set; }
        public DateTime joineddate { get; set; }
        public DateTime birthday { get; set; }
        public string remark { get; set; }
        public string SearchKey { get; set; }
        /// <summary>
        /// 要查询的页码
        /// </summary>
        public int PageIndex { get; set; } = 1;
        /// <summary>
        /// 每页返回的数量
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}
