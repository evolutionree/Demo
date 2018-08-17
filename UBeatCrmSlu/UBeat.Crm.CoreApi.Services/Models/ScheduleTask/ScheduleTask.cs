using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.Services.Models.ScheduleTask
{
    public class ScheduleTaskListModel
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public String UserIds { get; set; }

        public String UserType { get; set; }

        public int AffairStatus { get; set; }

        public int AffairType { get; set; }
    }
   
}
