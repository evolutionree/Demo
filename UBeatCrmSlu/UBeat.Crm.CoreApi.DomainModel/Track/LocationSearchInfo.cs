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
        public LatestLocation latest_location { get; set; }
        public int Status { get; set; }//状态：1正常定位，2未定位,未分配策略，3定位失败,超过{预警时间}未提交位置信息
        public int WarnningInterVal { get; set; }
    }

    public class LatestLocation
    {
        public Int64 loc_time { get; set; }
        public DateTime loc_time_format { get; set; }
        public double longitude { get; set; }
        public double latitude { get; set; }
        public string lot_address { get; set; }
        public int direction { get; set; }
        public int height { get; set; }
        public int radius { get; set; }
        public int speed { get; set; }
    }
}