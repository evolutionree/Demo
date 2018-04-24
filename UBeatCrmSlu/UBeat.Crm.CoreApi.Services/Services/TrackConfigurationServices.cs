using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using UBeat.Crm.CoreApi.DomainModel.Version;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.Track;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class TrackConfigurationServices : BaseServices
    {
        private readonly ITrackConfigurationRepository _tackConfigurationRepository;

        public TrackConfigurationServices(ITrackConfigurationRepository tackConfigurationRepository)
        {
            _tackConfigurationRepository = tackConfigurationRepository;
        }

        public OutputResult<object> TrackConfigurationList(TrackConfigurationInfo trackConfigurationQuery, int userNumber)
        {
            return new OutputResult<object>(_tackConfigurationRepository.TrackConfigurationList(trackConfigurationQuery, userNumber));
        }

        public OutputResult<object> AddTrackConfiguration(TrackConfigurationInfo trackConfigurationQuery)
        {
            bool result = _tackConfigurationRepository.AddTrackConfiguration(trackConfigurationQuery);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> UpdateTrackConfiguration(TrackConfigurationInfo trackConfigurationQuery)
        {
            bool result = _tackConfigurationRepository.UpdateTrackConfiguration(trackConfigurationQuery);
            IncreaseDataVersion(DataVersionType.TrackSettingData, null);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> DeleteTrackConfiguration(TrackConfigurationDel delQuery, int userNumber)
        {
            var result = _tackConfigurationRepository.DeleteTrackConfiguration(delQuery, userNumber);
            IncreaseDataVersion(DataVersionType.TrackSettingData, null);
            return HandleResult(result);
        }

        public OutputResult<object> AllocationList(TrackConfigurationAllocationList trackConfigurationAllocationListQuery, int userNumber)
        {
            return new OutputResult<object>(_tackConfigurationRepository.AllocationList(trackConfigurationAllocationListQuery, userNumber));
        }

        public OutputResult<object> AddAllocation(TrackConfigurationAllocation addQuery, int userNumber)
        {
            var result = _tackConfigurationRepository.AddAllocation(addQuery, userNumber);
            IncreaseDataVersion(DataVersionType.TrackSettingData, null);
            return HandleResult(result);
        }

        public OutputResult<object> DelAllocation(TrackConfigurationAllocation delQuery)
        {
            bool result = _tackConfigurationRepository.DelAllocation(delQuery);
            IncreaseDataVersion(DataVersionType.TrackSettingData, null);
            return new OutputResult<object>(result);
        }

        public TrackConfigurationInfo GetUserTrackStrategy(int userNumber)
        {
            return _tackConfigurationRepository.GetUserTrackStrategy(userNumber);
        }

        public List<UserTrackStrategyInfo> FilterHadBindTrackStrategyUser(string userIds)
        {
            return _tackConfigurationRepository.FilterHadBindTrackStrategyUser(userIds);
        }

        public string AllocatedUsers()
        {
            return _tackConfigurationRepository.AllocatedUsers();
        }
    }
}
