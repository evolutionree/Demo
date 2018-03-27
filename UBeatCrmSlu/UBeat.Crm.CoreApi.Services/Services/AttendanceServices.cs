using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Account;
using UBeat.Crm.CoreApi.DomainModel.Attendance;
using UBeat.Crm.CoreApi.DomainModel.BasicData;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.Attendance;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class AttendanceServices:EntityBaseServices
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper _mapper;

        public AttendanceServices(IMapper mapper, IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
            _mapper = mapper;
        }

        public OutputResult<object> Sign(AttendanceSignModel signModel, int userNumber)
        {
            var signEntity = _mapper.Map<AttendanceSignModel, AttendanceSignMapper>(signModel);
            if (signEntity == null || !signEntity.IsValid())
            {
                return HandleValid(signEntity);
            }

            var result = _attendanceRepository.Sign(signEntity, userNumber);
            return HandleResult(result);
        }

        public OutputResult<object> GroupUserQuery(GroupUserModel settingList, int userNumber)
        {
            var entity = new GroupUserMapper
            {
                DeptId= settingList.DeptId,
                UserName=settingList.UserName,
                PageIndex=settingList.PageIndex,
                PageSize=settingList.PageSize
            };
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }
            return new OutputResult<object>(_attendanceRepository.GroupUserQuery(entity, userNumber)) ;
        }

        public OutputResult<object> SignList(AttendanceSignListModel listModel, int userNumber)
        {
            var listEntity = _mapper.Map<AttendanceSignListModel, AttendanceSignListMapper>(listModel);
            if (listEntity == null || !listEntity.IsValid())
            {
                return HandleValid(listEntity);
            }

            var pageParam = new PageParam { PageIndex = listModel.PageIndex, PageSize = listModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }
            return ExcuteSelectAction((transaction, arg, userData) =>
            {                    //验证通过后，插入数据
                var   result = _attendanceRepository.SignList(pageParam, listEntity, userNumber);
                return new OutputResult<object>(result);
            }, listModel,Guid.Parse("00000000-0000-0000-0000-000000000004"), userNumber);
        }
    }
}
