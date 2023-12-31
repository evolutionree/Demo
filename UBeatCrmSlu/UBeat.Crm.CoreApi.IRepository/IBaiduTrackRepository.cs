﻿using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Track;
using System.Data.Common;

namespace UBeat.Crm.CoreApi.IRepository
{
    public interface IBaiduTrackRepository : IBaseRepository
    {
        List<CustVisitLocation> GetVisitCustDataList(DateTime visitDate, int visitUser, int userNumber);

        List<NearbyCustomerInfo> GetNearbyCustomerList(NearbyCustomerQuery query, int userNumber);
    }
}
