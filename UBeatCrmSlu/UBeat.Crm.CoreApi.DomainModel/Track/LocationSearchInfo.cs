using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.Track
{
    public class LocationSearchInfo
    {
        public string UserIds { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class LocationDetailInfo
    {
        public int entity_name { get; set; }
        public string entity_desc { get; set; }
        public string create_time { get; set; }
        public string modify_time { get; set; }
        public LocationPoint latest_location { get; set; }
        public int Status { get; set; }//状态：1正常定位，2未定位,未分配策略，3定位失败,超过{预警时间}未提交位置信息 4分配策略但没有任何定位信息
        public int WarningInterval { get; set; }
    }

    public class LocationPoint
    {
        public Int64 loc_time { get; set; }
        public DateTime loc_time_format { get; set; }//该时间为用户上传的时间
        public string create_time { get; set; }//该时间为服务端时间
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string lot_address { get; set; }
        public int direction { get; set; }
        public double height { get; set; }
        public double radius { get; set; }
        public double speed { get; set; }
        public string locate_mode { get; set; }//仅当纠偏时返回。可能的返回值：未知；GPS/北斗定位；网络定位；基站定位
    }


    public class TrackQuery
    {
        public int UserId { get; set; }
        public DateTime searchDate { get; set; }
        public int IsProcessed { get; set; }
        public string ProcessOption { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    public class StayPointQuery
    {

        public int UserId { get; set; }
        public DateTime searchDate { get; set; }
        public int StayRadius { get; set; }
        public int StayTime { get; set; }
    }

    public class TrackData
    {
        public int status { get; set; }
        public string message { get; set; }
        public int total { get; set; }
        public double distance { get; set; }
        public LocationPoint start_point { get; set; }
        public LocationPoint end_point { get; set; }
        public List<LocationPoint> points { get; set; }
        public List<CustVisitLocation> custVisitLocation { get; set; }
    }

    public class StayPointData
    {
        public int status { get; set; }
        public string message { get; set; }
        public int staypoint_num { get; set; }
        public List<StayPointInfo> stay_points { get; set; }
    }

    public class StayPointInfo
    {
        public UInt64 start_time { get; set; }
        public UInt64 end_time { get; set; }
        public int duration { get; set; }
        public Dictionary<string, object> stay_point { get; set; }

    }

    public class CustVisitLocation
    {
        public Guid recId { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string address { get; set; }
        public DateTime visitTime { get; set; }
        public string custId { get; set; }
        public string custName { get; set; }
    }

    public class NearbyCustomerQuery
    {
        public double longitude { get; set; }
        public double latitude { get; set; }
        public double searchRadius { get; set; }
    }

    public class NearbyCustomerInfo
    {
        public Guid CustId { set; get; }
        public string CustName { set; get; }
        public double Distance { set; get; }
        public Dictionary<string, object> CustAddress { set; get; }
    }
}