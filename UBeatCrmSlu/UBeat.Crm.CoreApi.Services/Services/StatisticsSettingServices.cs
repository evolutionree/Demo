using AutoMapper;
using Microsoft.Extensions.Configuration;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using System.Data;
using UBeat.Crm.CoreApi.DomainModel.StatisticsSetting;
using UBeat.Crm.CoreApi.Services.Models.StatisticsSetting;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class StatisticsSettingServices : BasicBaseServices
    {

        private readonly IStatisticsSettingRepository _statisticsSettingRepository;
        private readonly IMapper _mapper;


        public StatisticsSettingServices(IMapper mapper, IStatisticsSettingRepository statisticsSettingRepository, IConfigurationRoot config)
        {
            _statisticsSettingRepository = statisticsSettingRepository;
            _mapper = mapper;
        }
        public OutputResult<object> AddStatisticsSetting(AddStatisticsSettingModel add, int userId)
        {
            var mapper = _mapper.Map<AddStatisticsSettingModel, AddStatisticsSettingMapper>(add);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var data = _statisticsSettingRepository.AddStatisticsSetting(mapper, transaction, userId);
                return HandleResult(data);
            }, add, userId, isolationLevel: IsolationLevel.ReadUncommitted);
        }
        public OutputResult<object> UpdateStatisticsSetting(EditStatisticsSettingModel edit, int userId)
        {
            var mapper = _mapper.Map<EditStatisticsSettingModel, EditStatisticsSettingMapper>(edit);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _statisticsSettingRepository.UpdateStatisticsSetting(mapper, transaction, userId);
                return HandleResult(result);
            }, edit, userId, null, IsolationLevel.ReadUncommitted);
        }
        public OutputResult<object> DeleteStatisticsSetting(DeleteStatisticsSettingModel delete, int userId)
        {
            var mapper = _mapper.Map<DeleteStatisticsSettingModel, DeleteStatisticsSettingMapper>(delete);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _statisticsSettingRepository.DeleteStatisticsSetting(mapper, transaction, userId);
                return HandleResult(result);

            }, delete, userId);
        }
        public OutputResult<object> DisabledStatisticsSetting(DeleteStatisticsSettingModel delete, int userId)
        {
            var mapper = _mapper.Map<DeleteStatisticsSettingModel, DeleteStatisticsSettingMapper>(delete);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var result = _statisticsSettingRepository.DisabledStatisticsSetting(mapper, transaction, userId);
                return HandleResult(result);

            }, delete, userId);
        }
        public OutputResult<object> GetStatisticsListData(QueryStatisticsSettingModel model, int userId)
        {
            var mapper = _mapper.Map<QueryStatisticsSettingModel, QueryStatisticsSettingMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var scripts = _statisticsSettingRepository.GetStatisticsListData(new QueryStatisticsSettingMapper(), transaction, userId);
                return new OutputResult<object>(scripts);
            }, model, userId);
        }



        public OutputResult<object> GetStatisticsData(QueryStatisticsModel model, int userId)
        {
            var mapper = _mapper.Map<QueryStatisticsModel, QueryStatisticsMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var scripts = _statisticsSettingRepository.GetStatisticsData(mapper, transaction, userId);
                return new OutputResult<object>(scripts);
            }, model, userId);
        }

        public OutputResult<object> GetStatisticsDetailData(QueryStatisticsModel model, int userId)
        {
            var mapper = _mapper.Map<QueryStatisticsModel, QueryStatisticsMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var scripts = _statisticsSettingRepository.GetStatisticsDetailData(mapper, transaction, userId);
                return new OutputResult<object>(scripts);
            }, model, userId);
        }

        public OutputResult<object> SaveStatisticsGroupSumSetting(SaveStatisticsGroupModel model, int userId)
        {
            SaveStatisticsGroupMapper save = new SaveStatisticsGroupMapper();
            foreach (var tmp in model.Data)
            {
                var mapper = _mapper.Map<SaveStatisticsGroupSumModel, SaveStatisticsGroupSumMapper>(tmp);
                save.Data.Add(mapper);
            }
            save.IsDel = model.IsDel;
            return ExcuteAction((transaction, arg, userData) =>
            {
                var scripts = _statisticsSettingRepository.SaveStatisticsGroupSumSetting(save, transaction, userId);
                return new OutputResult<object>(scripts);
            }, model, userId);
        }

        public OutputResult<object> UpdateStatisticsGroupSetting(EditStatisticsGroupModel model, int userId)
        {
            var mapper = _mapper.Map<EditStatisticsGroupModel, EditStatisticsGroupMapper>(model);

            return ExcuteAction((transaction, arg, userData) =>
            {
                var scripts = _statisticsSettingRepository.UpdateStatisticsGroupSetting(mapper, transaction, userId);
                return new OutputResult<object>(scripts);
            }, model, userId);
        }
    }
}
