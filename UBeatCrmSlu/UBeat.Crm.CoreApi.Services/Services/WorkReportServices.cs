using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.WorkReport;
using UBeat.Crm.CoreApi.Services.Models.WorkReport;
using System.Linq;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class WorkReportServices : BaseServices
    {
        private readonly IWorkReportRepository _workReportRepository;
        private readonly IMapper _mapper;

        public WorkReportServices(IMapper mapper, IWorkReportRepository workReportRepository)
        {
            _workReportRepository = workReportRepository;
            _mapper = mapper;
        }


        #region 日报
        public OutputResult<object> DailyQuery(DailyReportLstModel daily, int userNumber)
        {
            var entity = _mapper.Map<DailyReportLstModel, DailyReportLstMapper>(daily);
            return new OutputResult<object>(_workReportRepository.DailyQuery(entity, userNumber));
        }

        public OutputResult<object> DailyInfoQuery(DailyReportLstModel daily, int userNumber)
        {
            var entity = _mapper.Map<DailyReportLstModel, DailyReportLstMapper>(daily);
            return new OutputResult<object>(_workReportRepository.DailyInfoQuery(entity, userNumber));
        }

        public OutputResult<object> InsertDaily(DailyReportModel daily, int userNumber)
        {
            DailyReportMapper entity = new DailyReportMapper
            {
                ReportCon = daily.ReportCon,
                ReportDate = daily.ReportDate,
            };
            IList<DailyReportUserRecMapper> recUsers = new List<DailyReportUserRecMapper>();
            DailyReportUserRecMapper recUser;
            foreach (var tmp in daily.RecUsers.GroupBy(t => t.Optype))
            {
                recUser = new DailyReportUserRecMapper
                {
                    Optype = tmp.Key,
                    UserIds = string.Join(",", tmp.Select(t => t.UserId).ToArray())
                };
                recUsers.Add(recUser);
            }
            entity.RecUsers = recUsers;
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_workReportRepository.InsertDaily(entity, userNumber));
        }

        public OutputResult<object> UpdateDaily(DailyReportModel daily, int userNumber)
        {
            DailyReportMapper entity = new DailyReportMapper
            {
                RecId = daily.RecId,
                ReportCon = daily.ReportCon,
                ReportDate = daily.ReportDate,
            };
            IList<DailyReportUserRecMapper> recUsers = new List<DailyReportUserRecMapper>();
            DailyReportUserRecMapper recUser;
            foreach (var tmp in daily.RecUsers.GroupBy(t => t.Optype))
            {
                recUser = new DailyReportUserRecMapper
                {
                    Optype = tmp.Key,
                    UserIds = string.Join(",", tmp.Select(t => t.UserId).ToArray())
                };
                recUsers.Add(recUser);
            }
            entity.RecUsers = recUsers;
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_workReportRepository.UpdateDaily(entity, userNumber));
        }

        #endregion

        #region 周报
        public OutputResult<object> WeeklyQuery(WeeklyReportLstModel weekly, int userNumber)
        {
            var entity = _mapper.Map<WeeklyReportLstModel, WeeklyReportLstMapper>(weekly);
            return new OutputResult<object>(_workReportRepository.WeeklyQuery(entity, userNumber));
        }

        public OutputResult<object> WeeklyInfoQuery(WeeklyReportLstModel weekly, int userNumber)
        {
            var entity = _mapper.Map<WeeklyReportLstModel, WeeklyReportLstMapper>(weekly);
            return new OutputResult<object>(_workReportRepository.WeeklyInfoQuery(entity, userNumber));
        }

        public OutputResult<object> InsertWeekly(WeeklyReportModel weekly, int userNumber)
        {
            WeeklyReportMapper entity = new WeeklyReportMapper
            {
                ReportCon = weekly.ReportCon,
                ReportDate = weekly.ReportDate,
                Weeks = weekly.Weeks,
                WeekType = weekly.WeekType
            };
            IList<WeeklyReportUserRecMapper> recUsers = new List<WeeklyReportUserRecMapper>();
            WeeklyReportUserRecMapper recUser;
            foreach (var tmp in weekly.RecUsers.GroupBy(t => t.Optype))
            {
                recUser = new WeeklyReportUserRecMapper
                {
                    Optype = tmp.Key,
                    UserIds = string.Join(",", tmp.Select(t => t.UserId).ToArray())
                };
                recUsers.Add(recUser);
            }
            entity.RecUsers = recUsers;
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_workReportRepository.InsertWeekly(entity, userNumber));
        }

        public OutputResult<object> UpdateWeekly(WeeklyReportModel weekly, int userNumber)
        {

            WeeklyReportMapper entity = new WeeklyReportMapper
            {
                RecId = weekly.RecId,
                ReportCon = weekly.ReportCon,
                ReportDate = weekly.ReportDate,
            };
            IList<WeeklyReportUserRecMapper> recUsers = new List<WeeklyReportUserRecMapper>();
            WeeklyReportUserRecMapper recUser;
            foreach (var tmp in weekly.RecUsers.GroupBy(t => t.Optype))
            {
                recUser = new WeeklyReportUserRecMapper
                {
                    Optype = tmp.Key,
                    UserIds = string.Join(",", tmp.Select(t => t.UserId).ToArray())
                };
                recUsers.Add(recUser);
            }
            entity.RecUsers = recUsers;
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return HandleResult(_workReportRepository.UpdateWeekly(entity, userNumber));
        }

        #endregion
    }
}
