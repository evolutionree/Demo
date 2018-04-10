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

        public List<NearbyCustomerInfo> GetNearbyCustomerList(NearbyCustomerQuery query, int userNumber)
        {
            Dictionary<string, string> bindUserInfo = new Dictionary<string, string>();
            string centerPointStr = string.Format("{0} {1}", query.longitude, query.latitude);
            var sql = string.Format(@"select
 ST_distance(
 	ST_GeographyFromText('SRID=4326;POINT(' || (custaddr->>'lon') || ' ' || (custaddr->>'lat') || ')'), 
 	ST_GeographyFromText('SRID=4326;POINT({0})')) 
 as distance, recid as CustId, recname as CustName
from crm_sys_customer c
where ST_dwithin(ST_GeographyFromText('SRID=4326;POINT(' || (custaddr->>'lon') || ' ' || (custaddr->>'lat') || ')'), 
				 ST_GeographyFromText('SRID=4326;POINT({1})'), @radius) and recstatus = 1
order by distance asc;", centerPointStr, centerPointStr);
            
            var param = new DbParameter[]{
                        new NpgsqlParameter("radius", query.searchRadius)
            };
            return DBHelper.ExecuteQuery<NearbyCustomerInfo>("", sql, param);
        }
    }
}
