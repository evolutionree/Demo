using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.BasicData;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.BasicData;
using UBeat.Crm.CoreApi.Services.Models.Notify;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class BasicDataServices : BaseServices
    {
        private readonly IBasicDataRepository _basicDataRepository;
        private readonly IMapper _mapper;

        public BasicDataServices(IMapper mapper, IBasicDataRepository basicDataRepository)
        {
            _basicDataRepository = basicDataRepository;
            _mapper = mapper;
        }

        public OutputResult<object> GetMessageList(BasicDataMessageModel messageModel, int userNumber)
        {
            var notifyEntity = _mapper.Map<BasicDataMessageModel, NotifyMessageMapper>(messageModel);
            if (notifyEntity == null || !notifyEntity.IsValid())
            {
                return HandleValid(notifyEntity);
            }

            var result = _basicDataRepository.GetMessageList(null, notifyEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> SyncData(BasicDataSyncModel syncModel, int userNumber)
        {
            var syncEntity = _mapper.Map<BasicDataSyncModel, SyncDataMapper>(syncModel);
            if (syncEntity == null || !syncEntity.IsValid())
            {
                return HandleValid(syncEntity);
            }

            var result = _basicDataRepository.SyncData(syncEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> SyncDataBasic(BasicDataSyncModel syncModel, int userNumber)
        {
            var syncEntity = _mapper.Map<BasicDataSyncModel, SyncDataMapper>(syncModel);
            if (syncEntity == null || !syncEntity.IsValid())
            {
                return HandleValid(syncEntity);
            }

            var result = _basicDataRepository.SyncDataBasic(syncEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> SyncDataEntity(BasicDataSyncModel syncModel, int userNumber)
        {
            var syncEntity = _mapper.Map<BasicDataSyncModel, SyncDataMapper>(syncModel);
            if (syncEntity == null || !syncEntity.IsValid())
            {
                return HandleValid(syncEntity);
            }

            var result = _basicDataRepository.SyncDataEntity(syncEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> SyncDelDataEntity(int userNumber)
        {
            var result = _basicDataRepository.SyncDelDataEntity(userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> SyncDataView(BasicDataSyncModel syncModel, int userNumber)
        {
            var syncEntity = _mapper.Map<BasicDataSyncModel, SyncDataMapper>(syncModel);
            if (syncEntity == null || !syncEntity.IsValid())
            {
                return HandleValid(syncEntity);
            }

            var result = _basicDataRepository.SyncDataView(syncEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> SyncDataTemplate(BasicDataSyncModel syncModel, int userNumber)
        {
            var syncEntity = _mapper.Map<BasicDataSyncModel, SyncDataMapper>(syncModel);
            if (syncEntity == null || !syncEntity.IsValid())
            {
                return HandleValid(syncEntity);
            }

            var result = _basicDataRepository.SyncDataTemplate(syncEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> DeptData(BasicDataDeptModel deptModel, int userNumber)
        {
            var deptEntity = _mapper.Map<BasicDataDeptModel, DeptDataMapper>(deptModel);
            if (deptEntity == null || !deptEntity.IsValid())
            {
                return HandleValid(deptEntity);
            }


            var result = _basicDataRepository.DeptData(deptEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> DeptPowerData(BasicDataDeptModel deptModel, int userNumber)
        {
            var deptEntity = _mapper.Map<BasicDataDeptModel, DeptDataMapper>(deptModel);
            if (deptEntity == null || !deptEntity.IsValid())
            {
                return HandleValid(deptEntity);
            }


            var result = _basicDataRepository.DeptPowerData(deptEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> UserContactList(BasicDataUserContactListModel listModel, int userNumber)
        {
            var listEntity = _mapper.Map<BasicDataUserContactListModel, BasicDataUserContactListMapper>(listModel);
            if (listEntity == null || !listEntity.IsValid())
            {
                return HandleValid(listEntity);
            }

            var pageParam = new PageParam { PageIndex = listModel.PageIndex, PageSize = listModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }

            var result = _basicDataRepository.UserContactList(pageParam, listEntity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> FuncCount(int userNumber)
        {
            var result = _basicDataRepository.FuncCount(userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> FuncCountList(BasicDataFuncCountListModel funcCountModel, int userNumber)
        {
            if (funcCountModel?.AnaFuncId == null)
            {
                return ShowError<object>("指标详情ID不能为空");
            }

            var pageParam = new PageParam { PageIndex = funcCountModel.PageIndex, PageSize = funcCountModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }

            var result = _basicDataRepository.FuncCountList(pageParam, funcCountModel.AnaFuncId, userNumber);
            return new OutputResult<object>(result);
        }


        public OutputResult<object> AnalyseFuncQuery(AnalyseListModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AnalyseListModel, AnalyseListMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _basicDataRepository.AnalyseFuncQuery(entity, userNumber);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> InsertAnalyseFunc(AddAnalyseModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<AddAnalyseModel, AddAnalyseMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _basicDataRepository.InsertAnalyseFunc(entity, userNumber);
            return HandleResult(result);
        }

        public OutputResult<object> UpdateAnalyseFunc(EditAnalyseModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<EditAnalyseModel, EditAnalyseMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _basicDataRepository.UpdateAnalyseFunc(entity, userNumber);
            return HandleResult(result);
        }

        public OutputResult<object> DisabledAnalyseFunc(DisabledOrOderbyAnalyseModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<DisabledOrOderbyAnalyseModel, DisabledOrOderbyAnalyseMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _basicDataRepository.DisabledAnalyseFunc(entity, userNumber);
            return HandleResult(result);
        }

        public OutputResult<object> OrderByAnalyseFunc(DisabledOrOderbyAnalyseModel entityModel, int userNumber)
        {
            var entity = _mapper.Map<DisabledOrOderbyAnalyseModel, DisabledOrOderbyAnalyseMapper>(entityModel);
            if (entity == null || !entity.IsValid())
            {
                return HandleValid(entity);
            }

            var result = _basicDataRepository.DisabledAnalyseFunc(entity, userNumber);
            return HandleResult(result);
        }
    }
}
