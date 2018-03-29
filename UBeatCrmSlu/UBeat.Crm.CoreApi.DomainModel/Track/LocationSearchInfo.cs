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
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string LocationTimeStr { get; set; }
        public Dictionary<string, object> LatestLocation { get; set; }
        public int Status { get; set; }//状态：1正常定位，2未定位,未分配策略，3定位失败,超过{预警时间}未提交位置信息
    }
}