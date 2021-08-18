using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Customer
{
    public class CustRelFieldModel
    {
        public Guid RecId { set; get; }

        public string Data { set; get; }

    }

	public class CustContactTreeItemInfo
	{
		[JsonProperty("id")]
		public Guid RecId { set; get; }
		[JsonProperty("parent_id")]
		public Guid ParantRecId { set; get; }

		/// <summary>
		/// 姓名
		/// </summary>
		[JsonProperty("name")]
		public string RecName { set; get; }

		/// <summary>
		/// 类型
		/// </summary>
		public int ContactType { set; get; }

		/// <summary>
		/// 类型名称
		/// </summary>
		public string ContactType_Name { set; get; }

		/// <summary>
		/// 职务
		/// </summary>
		public string Position { set; get; }

		/// <summary>
		/// 联系人头像
		/// </summary>
		public string HeadIcon { set; get; }

		/// <summary>
		/// 所属客户
		/// </summary>
		public DataSourceModel Customer { set; get; }

		/// <summary>
		/// 上级联系人
		/// </summary>
		public DataSourceModel SupContact { set; get; }

		[JsonIgnore]
		public bool IsSearchChildren { get; set; }

		public string CustDept { get; set; }

	}

	public class DataSourceModel
	{
		public Guid? Id { set; get; }

		public string Name { set; get; }
	}

    public class CustFrameProtocolModel
    {
        public string customername { set; get; }
        public string signdept { set; get; }
        public string validity { set; get; }
        public string recmanager { set; get; }
    }
    public class CustomerTemp
    {
	    public  Guid? recid { set; get; }
	    public string recname { set; get; }
	    public string businesscenter { set; get; }
	    
	    public string beforename { set; get; }
	    public string ucode { set; get; }
	    public string businesscode { set; get; }
	    public string organizationcode { set; get; }
	    public string qccenterprisenature { set; get; }
	    public string qccenterprisetype { set; get; }
	    public string enterprisestatus { set; get; }
	    public string registeredcapital { set; get; }
	    public string paidcapital { set; get; }
	    public string registrationauthority { set; get; }
	    public string corporatename { set; get; }
	    public string qcclocation { set; get; }
	    public string isiop { set; get; }
	    public string customercompanyaddress { set; get; }
	    public string deptgroup { set; get; }
	    public string predeptgroup { set; get; }
	    public string precustomer { set; get; }
	    public string region { set; get; }
	    public string customerstatus { set; get; }
	    public string basicinfo { set; get; }
	    public string customername { set; get; }
	    public string industry { set; get; }
	    public string contactnumber { set; get; }
	    public string contactinfo { set; get; }
	    public string viewusers { set; get; }
	    public string custlevel { set; get; }
	    public string custfax { set; get; }
	    public string custwebsite { set; get; }
	    public string coldstorsize { set; get; }
	    public string continent { set; get; }
	    public string platform { set; get; }
	    public string flowstatus { set; get; }
	    public string isnewcustomer { set; get; }
	    public string creditrating { set; get; }
	    public string enterprisenature { set; get; }
	    public string enterprisetype { set; get; }
	    public string email { set; get; }
	    public string workgroup { set; get; }
	    public string director { set; get; }
	    public string customertype { set; get; }
	    public string recmanager { set; get; }
	    
    }

}
