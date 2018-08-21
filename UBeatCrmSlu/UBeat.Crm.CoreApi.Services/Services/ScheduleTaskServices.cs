using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.ScheduleTask;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.ScheduleTask;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class ScheduleTaskServices : BasicBaseServices
    {
        private readonly IScheduleTaskRepository _iScheduleTaskRepository;
        private IMapper _iMapper;
        public ScheduleTaskServices(IScheduleTaskRepository iScheduleTaskRepository, IMapper iMapper)
        {
            _iScheduleTaskRepository = iScheduleTaskRepository;
            _iMapper = iMapper;
        }

        public OutputResult<object> GetScheduleTaskCount(ScheduleTaskListModel model, int userId)
        {
            var mapper = _iMapper.Map<ScheduleTaskListModel, ScheduleTaskListMapper>(model);
            if (mapper == null || !mapper.IsValid())
            {
                return HandleValid(mapper);
            }

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _iScheduleTaskRepository.GetScheduleTaskCount(mapper, userId, transaction);
                return new OutputResult<object>(result);

            }, model, userId);
        }

        public OutputResult<object> GetUnConfirmList(UnConfirmListModel model, int userId)
        {
            var mapper = _iMapper.Map<UnConfirmListModel, UnConfirmListMapper>(model);

            var result = _iScheduleTaskRepository.GetUnConfirmList(mapper, userId);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> ScheduleStatus(UnConfirmScheduleStatusModel model, int userId)
        {
            var mapper = _iMapper.Map<UnConfirmScheduleStatusModel, UnConfirmScheduleStatusMapper>(model);
            if (mapper == null || !mapper.IsValid())
            {
                return HandleValid(mapper);
            }

            return ExcuteAction((transaction, arg, userData) =>
            {
                OperateResult result = null;
                if (mapper.AcceptStatus == 1)
                {
                    result = _iScheduleTaskRepository.AceptSchedule(mapper, userId, transaction);
                }
                else
                {
                    result = _iScheduleTaskRepository.RejectSchedule(mapper, userId, transaction);
                }
                return new OutputResult<object>(result);
            }, model, userId);
        }
    }
}
