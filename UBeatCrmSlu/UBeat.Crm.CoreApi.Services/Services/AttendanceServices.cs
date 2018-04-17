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
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.DomainModel.Utility;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class AttendanceServices:EntityBaseServices
    {
        private readonly IDynamicEntityRepository _dynamicEntityRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly DynamicEntityServices _dynamicEntityServices;
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;

        public AttendanceServices(IMapper mapper, IAttendanceRepository attendanceRepository, IDynamicEntityRepository dynamicEntityRepository, IAccountRepository accountRepository, DynamicEntityServices dynamicEntityServices)
        {
            _dynamicEntityRepository = dynamicEntityRepository;
            _dynamicEntityServices = dynamicEntityServices;
            _attendanceRepository = attendanceRepository;
            _accountRepository = accountRepository;
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

        public OutputResult<object> Add(AttendanceSignModel signModel, int userNumber)
        {
            var signEntity = new AttendanceAddMapper()
            {
                SignType=signModel.SignType,
                SignMark=signModel.SignMark,
                SignTime= signModel.SignTime,
                CardType=signModel.CardType,
                RecordSource=signModel.RecordSource,
                SelectUser =signModel.SelectUser
            };
            if (signEntity == null || !signEntity.IsValid())
            {
                return HandleValid(signEntity);
            }

            var result = _attendanceRepository.Add(signEntity, userNumber);
            return HandleResult(result);
        }

        public OutputResult<object> AddGroupUser(AddGroupUserModel settingList, AnalyseHeader header, int userNumber)
        {
            List<string> errorList = new List<string>();
            //新增部门
            foreach (var dept in settingList.DeptSelect) {
                var pageParam = new PageParam { PageIndex = 1, PageSize = 99999 };
                var searchEntity = new AccountUserQueryMapper()
                {
                    RecStatus = 1,
                    DeptId = new Guid(dept["id"].ToString())
                };
                var result = _accountRepository.GetUserList(pageParam, searchEntity, userNumber);
                foreach (var user in result["PageData"]) {
                    Dictionary<string, object> fieldData = new Dictionary<string, object>();
                    fieldData["person"] = user["userid"];
                    fieldData["attengroup"] = JsonHelper.ToJson(new
                    {
                        id = settingList.ScheduleGroup["id"],
                        name = settingList.ScheduleGroup["name"]
                    });
                    DynamicEntityAddModel dynamicModel = new DynamicEntityAddModel()
                    {
                        TypeId = new Guid("ba77747f-a6dd-495d-a62b-027a7a6c404d"),
                        FieldData = fieldData
                    };

                    var existCount = _attendanceRepository.ExistGroupUser(user["userid"].ToString());
                    if (existCount == 0)
                    {
                        var entityResult = _dynamicEntityRepository.DynamicAdd(null, dynamicModel.TypeId, dynamicModel.FieldData, dynamicModel.ExtraData, userNumber);
                        if (entityResult.Flag == 0)
                            errorList.Add(user["username"].ToString());
                    }
                }
            };
            //对选择人新增
            foreach (var user in settingList.UserSelect)
            {
                Dictionary<string, object> fieldData = new Dictionary<string, object>();
                fieldData["person"] = user["id"];
                fieldData["attengroup"] = JsonHelper.ToJson(new
                {
                    id = settingList.ScheduleGroup["id"],
                    name = settingList.ScheduleGroup["name"]
                });
                DynamicEntityAddModel dynamicModel = new DynamicEntityAddModel()
                {
                    TypeId = new Guid("ba77747f-a6dd-495d-a62b-027a7a6c404d"),
                    FieldData = fieldData
                };
                var existCount = _attendanceRepository.ExistGroupUser(user["id"].ToString());
                if (existCount == 0) {
                    var entityResult = _dynamicEntityRepository.DynamicAdd(null, dynamicModel.TypeId, dynamicModel.FieldData, dynamicModel.ExtraData, userNumber);
                    if (entityResult.Flag == 0)
                        errorList.Add(user["name"].ToString());
                }
            }
            Dictionary<string, List<string>> errMap = new Dictionary<string, List<string>>();
            errMap["erruser"] = errorList;
            return new OutputResult<object>(errMap);
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
