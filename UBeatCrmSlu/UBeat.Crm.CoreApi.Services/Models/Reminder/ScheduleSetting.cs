using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Reminder
{
    public class ScheduleSetting
    { 
        public string TaskJobServer { get; set; }

        public string EnterpriseNo { get; set; }

        public string CommonJobCronString { get; set; }

		public ScheduleTypeEnum IsNeedSchedule { get; set; } 
	}

	public enum ScheduleTypeEnum
	{
		ScheduleTypeNotNeed = 0,
		ScheduleTypeNeed = 1
	}
}
