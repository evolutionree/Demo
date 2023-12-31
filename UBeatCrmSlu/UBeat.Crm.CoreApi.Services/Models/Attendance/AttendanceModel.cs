﻿using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;

namespace UBeat.Crm.CoreApi.Services.Models.Attendance
{
    public class AttendanceSignModel
    {
        public string SignImg { get; set; }
        public AddressType Locations { get; set; }
        public int SignType { get; set; }
        public int CardType { get; set; }
        public string SignMark { get; set; }
        public string SignTime { get; set; }
        public int SelectUser { get; set; }
        public int RecordSource { get; set; }
    }

    public class GroupUserModel
    {
        public string DeptId { get; set; }

        public string UserName { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }

    public class AddGroupUserModel
    {
        public List<Dictionary<string, object>> DeptSelect { get; set; }

        public List<Dictionary<string, object>> UserSelect { get; set; }

        public Dictionary<string, string> ScheduleGroup { get; set; }
    }

    public class AttendanceSignListModel
    {
        public int MonthType { get; set; }
        public int ListType { get; set; }

        public Guid DeptId { get; set; }
        public string SearchName { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public int Type { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
