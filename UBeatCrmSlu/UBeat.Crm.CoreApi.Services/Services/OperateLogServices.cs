using AutoMapper;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.OpreateLog;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.OperateLog;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class OperateLogServices:BaseServices
    {
        private readonly IOperateLogRepository _operateLogRepository;
        private readonly IMapper _mapper;

        public OperateLogServices(IMapper mapper, IOperateLogRepository operateLogRepository)
        {
            _operateLogRepository = operateLogRepository;
            _mapper = mapper;
        }

        public OutputResult<object> RecordList(OperateLogRecordListModel searchModel, int userNumber)
        {
            var searchEntity = _mapper.Map<OperateLogRecordListModel, OperateLogRecordListMapper>(searchModel);
            if (searchEntity == null || !searchEntity.IsValid())
            {
                return HandleValid(searchEntity);
            }
            var pageParam = new PageParam { PageIndex = searchModel.PageIndex, PageSize = searchModel.PageSize };
            if (!pageParam.IsValid())
            {
                return HandleValid(pageParam);
            }
            var result = _operateLogRepository.RecordList(pageParam, searchEntity, userNumber);
            return new OutputResult<object>(result);
        }
    }
}
