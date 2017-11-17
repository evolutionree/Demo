using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.WorkReport;

namespace UBeat.Crm.CoreApi.Services.Models.WorkReport
{
   public class WorkReportProfile:Profile
    {
        public WorkReportProfile()
        {
            CreateMap<DailyReportUserRecModel, DailyReportUserRecMapper>();
            CreateMap<WeeklyReportUserRecModel, WeeklyReportUserRecMapper>();

            CreateMap<DailyReportModel, DailyReportMapper>();
            CreateMap<WeeklyReportModel, WeeklyReportMapper>();
            CreateMap<DailyReportLstModel, DailyReportLstMapper>();
        }
    }
}
