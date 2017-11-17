using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.Reminder
{

    public class ReminderSettingEditModel
    {
        public List<ReminderSettingModel> ConfigList { get; set; }
    }

    public class ReminderSettingModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public int RecStatus { get; set; }

        public int CheckDay { get; set; }

        public string CronString { get; set; }

        public string ConfigVal { get; set; }
    }
}
