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
    public class TrackConfigurationController : BaseController
    {
        private readonly TrackConfigurationServices _trackConfigurationServices;

        public TrackConfigurationController(TrackConfigurationServices trackConfigurationServices) : base(trackConfigurationServices)
        {
            _trackConfigurationServices = trackConfigurationServices;
        }

        /// <summary>
        /// 定位策略列表
        /// </summary>
        /// <param name="trackConfigurationQuery"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("List")]
        public OutputResult<object> TrackConfigurationList([FromBody] TrackConfigurationInfo trackConfigurationQuery)
        {
            if (trackConfigurationQuery == null) return ResponseError<object>("参数格式错误");

            return _trackConfigurationServices.TrackConfigurationList(trackConfigurationQuery, UserId);
        }

        /// <summary>
        /// 新增定位策略
        /// </summary>
        /// <param name="trackConfigurationQuery"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Add")]
        public OutputResult<object> AddTrackConfiguration([FromBody] TrackConfigurationInfo trackConfigurationQuery)
        {
            var addConfiguration = _trackConfigurationServices.AddTrackConfiguration(trackConfigurationQuery);
            bool result = (bool)addConfiguration.DataBody;
            var response = new
            {
                value = result
            };
            return new OutputResult<object>(response);
        }

        /// <summary>
        /// 修改定位策略
        /// </summary>
        /// <param name="trackConfigurationQuery"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Update")]
        public OutputResult<object> UpdateTrackConfiguration([FromBody] TrackConfigurationInfo trackConfigurationQuery)
        {
            var addConfiguration = _trackConfigurationServices.UpdateTrackConfiguration(trackConfigurationQuery);
            bool result = (bool)addConfiguration.DataBody;
            var response = new
            {
                value = result
            };
            return new OutputResult<object>(response);
        }

        /// <summary>
        /// 人员定位策略列表
        /// </summary>
        /// <param name="trackConfigurationQuery"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AllocationList")]
        public OutputResult<object> AllocationList([FromBody] TrackConfigurationAllocationList trackConfigurationAllocationListQuery)
        {
            if (trackConfigurationAllocationListQuery == null) return ResponseError<object>("参数格式错误");

            return _trackConfigurationServices.AllocationList(trackConfigurationAllocationListQuery, UserId);
        }


        [HttpPost]
        [Route("AddAllocation")]
        public OutputResult<object> AddAllocation([FromBody] TrackConfigurationAllocation addQuery)
        {
            if (addQuery == null) return ResponseError<object>("参数格式错误");

            return _trackConfigurationServices.AddAllocation(addQuery, UserId);
        }

        /// <summary>
        /// 取消人员定位策略
        /// </summary>
        /// <param name="trackConfigurationAllocationListQuery"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CancelAllocation")]
        public OutputResult<object> CancelAllocation([FromBody] TrackConfigurationAllocationDel delQuery)
        {
            var addConfiguration = _trackConfigurationServices.CancelAllocation(delQuery);
            bool result = (bool)addConfiguration.DataBody;
            var response = new
            {
                value = result
            };
            return new OutputResult<object>(response);
        }
    }
}