using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Customer
{
    public class CustMergeModel
    {
        public Guid MainCustId { set; get; }
        public List<Guid> CustIds { set; get; }
    }


    public class MergeCustListModel
    {
       public string MenuId { set; get; }

        public string SearchKey { set; get; }
        /// <summary>
        /// 表示要返回的页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 表示每页的大小
        /// </summary>
        public int PageSize { get; set; }

    }

    public class DistributionCustomerParam
    {
        public int UserId { get; set; }
        public List<string> Recids { get; set; }
    }
    public class SyncErpCusomter
    {
        public Guid EntityId { get; set; }
        public string[] RecIds { get; set; }
    }

	public class CustContactTreeModel
	{
		public Guid CustId { set; get; }
	}
}
