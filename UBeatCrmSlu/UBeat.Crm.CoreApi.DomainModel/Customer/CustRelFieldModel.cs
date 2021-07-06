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
}
