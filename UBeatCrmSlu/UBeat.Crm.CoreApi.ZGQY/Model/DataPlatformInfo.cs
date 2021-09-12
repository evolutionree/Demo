using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.ZGQY.Model
{
	public class DataPlatformInfo
	{
	}

	public class MiddleServerConfig
	{
		public string DataBase { get; set; }
	}

	public class SaveProjectInfo
	{
		public SaveProjectInfo()
		{
		}
		public string contract_code { get; set; }
		public string project_id { get; set; }
		public string project_code { get; set; }
		public string project_name { get; set; }

		public string son_project_code { get; set; }
		public string son_project_name { get; set; }
		public string unit_code { get; set; }
		public string name { get; set; }
		public string status { get; set; }
		public string status_code { get; set; }
		public string status_name { get; set; }
		public string creation_date { get; set; }
		public string last_update_date { get; set; }

		public string contract_code_json { get; set; }
	}

	public class QueryModelInfo
	{
		public Guid RecId { get; set; }
		public string RecName { get; set; }
		public int Recmanager { get; set; }
	}

	public class SaveBillInfo
	{
		public SaveBillInfo()
		{
		}
		public string paf_num { get; set; }
		public string applicant { get; set; }
		public string applicant_num { get; set; }

		public string department { get; set; }
		public string invoice_date { get; set; }
		public string contract_code { get; set; }
		public string money { get; set; }
		public string paf_status { get; set; }
		public string creation_date { get; set; }
		public string paf_primary_id { get; set; }
		 
		public string contract_code_json { get; set; }
	}

	public class QueryUserInfo
	{
		public int UserId { get; set; }
		public string UserName { get; set; }
		public Guid DeptId { get; set; }
		public string DeptName { get; set; }
	}

	public class SavePayInfo
	{
		public SavePayInfo()
		{
		}
		public string bill_code { get; set; }
		public string contract_code { get; set; }
		public string receivable_date { get; set; }

		public string distribute_amount { get; set; }
		
		public string contract_code_json { get; set; }
	}

	public class SaveZdInfo
	{
		public SaveZdInfo()
		{
		}
		public string number { get; set; }
		public string applier { get; set; }
		public string applierNumber { get; set; }

		public string orgUnit { get; set; }
		public string entertain_date { get; set; }
		public string amount { get; set; }
		public string used_flag { get; set; }
	}


	public class SaveClInfo
	{
		public SaveClInfo()
		{
		}
		public string number { get; set; }
		public string applier { get; set; }
		public string applierNumber { get; set; }

		public string orgUnit { get; set; }
		public string travel_date { get; set; }
		public string amount { get; set; }
		public string used_flag { get; set; }
	}
}
