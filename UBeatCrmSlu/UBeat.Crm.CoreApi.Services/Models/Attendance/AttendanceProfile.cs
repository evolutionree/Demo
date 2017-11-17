using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel.Attendance;

namespace UBeat.Crm.CoreApi.Services.Models.Attendance
{
    public class AttendanceProfile:Profile
    {
        public AttendanceProfile()
        {
            CreateMap<AttendanceSignModel, AttendanceSignMapper>();
            CreateMap<AttendanceSignListModel, AttendanceSignListMapper>();
        }
    }
}
