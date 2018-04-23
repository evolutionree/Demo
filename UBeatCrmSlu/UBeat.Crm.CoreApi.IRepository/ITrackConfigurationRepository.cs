using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Track;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface ITrackConfigurationRepository : IBaseRepository
    {
        Dictionary<string, List<IDictionary<string, object>>> TrackConfigurationList(TrackConfigurationInfo trackConfigurationQuery, int userNumber);

        bool AddTrackConfiguration(TrackConfigurationInfo trackConfigurationQuery, DbTransaction tran = null);

        bool UpdateTrackConfiguration(TrackConfigurationInfo trackConfigurationQuery, DbTransaction tran = null);

        OperateResult DeleteTrackConfiguration(TrackConfigurationDel delquery, int userNumber);

        Dictionary<string, List<IDictionary<string, object>>> AllocationList(TrackConfigurationAllocationList trackConfigurationAllocationListQuery, int userNumber);

        OperateResult AddAllocation(TrackConfigurationAllocation trackConfigurationAllocationListQuery, int userNumber);

        bool DelAllocation(TrackConfigurationAllocation delQuery, DbTransaction tran = null);

        TrackConfigurationInfo GetUserTrackStrategy(int userNumber);

        List<UserTrackStrategyInfo> FilterHadBindTrackStrategyUser(string userIds);

        string AllocatedUsers();
    }
}
