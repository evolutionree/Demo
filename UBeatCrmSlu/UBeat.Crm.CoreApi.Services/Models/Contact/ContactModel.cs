using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Contact
{
    public class ContactVCardModel
    {
        public string CollectionName { get; set; }
        public string FileId { get; set; }
    }

    public class LinkManModel
    {
        public int userid { get; set; }
        public Boolean flag { get; set; } = false;
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
