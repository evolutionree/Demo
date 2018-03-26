using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Track;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface ITrackConfigurationRepository : IBaseRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> TrackConfigurationList(TrackConfigurationInfo trackConfigurationQuery, int userNumber);

        bool AddTrackConfiguration(TrackConfigurationInfo trackConfigurationQuery, DbTransaction tran = null);

        bool UpdateTrackConfiguration(TrackConfigurationInfo trackConfigurationQuery, DbTransaction tran = null);

        Dictionary<string, List<IDictionary<string, object>>> AllocationList(TrackConfigurationAllocationList trackConfigurationAllocationListQuery, int userNumber);

        bool CancelAllocation(TrackConfigurationAllocationDel delQuery, DbTransaction tran = null);

        bool AddAllocation(TrackConfigurationAllocation trackConfigurationAllocationListQuery, DbTransaction tran = null);
    }
}
