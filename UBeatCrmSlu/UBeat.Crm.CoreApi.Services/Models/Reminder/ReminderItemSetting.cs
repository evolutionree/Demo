using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.Services.Models.Reminder
{
    public class ReminderItemSettingEditModel
    {
        public List<ReminderItemSettingModel> ConfigList { get; set; }
        public List<int> setTypeList { get; set; }
    }
    public class ReminderItemSettingModel
    {
        public Guid id { get; set; }

        public Guid dicId { get; set; }

        public int? dicTypeId { get; set; }

    }
}
