using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using Npgsql;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Track;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Utility;

namespace UBeat.Crm.CoreApi.Repository.Repository.Track
{
    public class BaiduTrackRepository : RepositoryBase, IBaiduTrackRepository
    {
        public List<CustVisitLocation> GetVisitCustDataList(DateTime visitDate, int visitUser, int userNumber)
        {
            Dictionary<string, string> bindUserInfo = new Dictionary<string, string>();
            var sql = @"SELECT recid, reccreated as visitTime, (location->>'lon')::real as longitude, (location->>'lat')::real as latitude, location->>'address' as address,
                        relatedcustomer->> 'id' as custId,relatedcustomer->> 'name' as custName
                        FROM crm_sys_customervisit where reccreated::date = @visitDate::date and reccreator = @visitUser; ";
            var param = new DbParameter[]{
                        new NpgsqlParameter("visitDate", visitDate),
                        new NpgsqlParameter("visitUser", visitUser)
            };
            return DBHelper.ExecuteQuery<CustVisitLocation>("", sql, param);
        }
    }
}
