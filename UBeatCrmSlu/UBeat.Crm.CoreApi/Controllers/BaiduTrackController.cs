using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.Track;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class BaiduTrackController : BaseController
    {
        private readonly BaiduTrackServices _baiduTrackServices;

        public BaiduTrackController(BaiduTrackServices baiduTrackServices) : base(baiduTrackServices)
        {
            _baiduTrackServices = baiduTrackServices;
        }

        /// <summary>
        /// 实时定位信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("recentlocation")]
        public OutputResult<object> RecentLocationByUserIds([FromBody] LocationSearchInfo searchQuery)
        {
            if (searchQuery == null) return ResponseError<object>("参数格式错误");

            return _baiduTrackServices.GetRecentLocationByUserIds(searchQuery, UserId);
        }

        /// <summary>
        /// 轨迹查询
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("track")]
        public OutputResult<object> GetTrack([FromBody] TrackQuery trackQuery)
        {
            if (trackQuery == null) return ResponseError<object>("参数格式错误");

            return _baiduTrackServices.GetTrack(trackQuery, UserId);
        }

        /// <summary>
        /// 附近的客户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("nearbyCust")]
        public OutputResult<object> GetNearbyCustomer([FromBody] NearbyCustomerQuery query)
        {
            if (query == null) return ResponseError<object>("参数格式错误");

            return _baiduTrackServices.GetNearbyCustomerList(query, UserId);
        }
    }
}