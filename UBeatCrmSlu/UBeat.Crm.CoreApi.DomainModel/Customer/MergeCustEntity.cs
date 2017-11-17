using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Customer
{
    public class MergeCustEntity
    {
        /// <summary>
        /// 客户id
        /// </summary>
        public Guid CustId { set; get; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CustName { set; get; }
        /// <summary>
        /// 负责人
        /// </summary>
        public int Manager { set; get; }
        /// <summary>
        /// 负责人名称
        /// </summary>
        public string ManagerName { set; get; }

        /// <summary>
        /// 客户状态
        /// </summary>
        public int CustStatus { set; get; }
        /// <summary>
        /// 客户状态名称
        /// </summary>
        public string CustStatus_Name { set; get; }
    }
}
