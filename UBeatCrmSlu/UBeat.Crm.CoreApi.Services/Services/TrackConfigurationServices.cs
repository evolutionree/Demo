using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
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
            return new OutputResult<object>(result);
        }

        public OutputResult<object> AllocationList(TrackConfigurationAllocationList trackConfigurationAllocationListQuery, int userNumber)
        {
            return new OutputResult<object>(_tackConfigurationRepository.AllocationList(trackConfigurationAllocationListQuery, userNumber));
        }

        public OutputResult<object> AddAllocation(TrackConfigurationAllocation addQuery, int userNumber)
        {
            bool result = _tackConfigurationRepository.AddAllocation(addQuery);
            return new OutputResult<object>(result);
        }

        public OutputResult<object> CancelAllocation(TrackConfigurationAllocationDel delQuery)
        {
            bool result = _tackConfigurationRepository.CancelAllocation(delQuery);
            return new OutputResult<object>(result);
        }
    }
}
