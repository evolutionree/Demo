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
}
