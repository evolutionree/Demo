using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Track
{
    public class TrackConfigurationInfo
    {
        public Guid RecId { get; set; }

        public string RecName { get; set; }

        public int LocationInterval { get; set; }

        public string LocationTimeRange { get; set; }

        public string LocationCycle { get; set; }

        public int WarningInterval { get; set; }

        public string Remark { get; set; }

        public int RecStatus { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }

    public class TrackConfigurationDel
    {
        public string StrategyIds { get; set; }
        public int Status { get; set; }
    }


    public class TrackConfigurationAllocation
    {
        public string StrategyId { get; set; }

        public string UserIds { get; set; }
    }

    public class TrackConfigurationAllocationList
    {
        public Guid RecId { get; set; }

        public Guid StrategyId { get; set; }

        public string StrategyName { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string Remark { get; set; }

        public int RecStatus { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }
    }

    public class UserTrackStrategyInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int WarningInterval { get; set; }         
    }

}