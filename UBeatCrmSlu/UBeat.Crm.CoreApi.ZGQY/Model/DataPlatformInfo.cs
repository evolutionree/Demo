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
		public string PROJECT_ID { get; set; }
		public string PROJECT_CODE { get; set; }
		public string PROJECT_NAME { get; set; } 
	}
}
