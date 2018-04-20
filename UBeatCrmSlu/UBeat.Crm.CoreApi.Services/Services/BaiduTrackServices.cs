using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using System.Linq;
using UBeat.Crm.CoreApi.DomainModel.EntityPro;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.DomainModel.Track;
using System.IO;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class BaiduTrackServices : BaseServices
    {
        static string locationSearchURL = "http://yingyan.baidu.com/api/v3/entity/search";
        static string getTrackDataURL = "http://yingyan.baidu.com/api/v3/track/gettrack";
        static string stayPointURL = "http://yingyan.baidu.com/api/v3/analysis/staypoint";

        private readonly IBaiduTrackRepository _baiduTrackRepository;
        private readonly IAccountRepository _accountRepository;

        public BaiduTrackServices(IBaiduTrackRepository repository, IAccountRepository accountRepository)
        {
            _baiduTrackRepository = repository;
            _accountRepository = accountRepository;
        }

        public OutputResult<object> GetRecentLocationByUserIds(LocationSearchInfo searchQuery, int userNumber)
        {
            //过滤掉没有分配定位策略的人员
            var dynamicTrackService = (TrackConfigurationServices)dynamicCreateService(typeof(TrackConfigurationServices).FullName, true);
            var hadBindUserInfo = dynamicTrackService.FilterHadBindTrackStrategyUser(searchQuery.UserIds);
            var hadBindUserIdsArr = hadBindUserInfo.Select(x => x.UserId.ToString()).ToArray();
            string hadBindUserIdsStr = string.Join(',', hadBindUserIdsArr);

            //没有绑定定位策略的人员，固定返回“未定位,未分配策略”
            List<LocationDetailInfo> locationDetailList = new List<LocationDetailInfo>() { };
            var wholeUserIdsArr = searchQuery.UserIds.Split(",");
            if (wholeUserIdsArr.Length != hadBindUserIdsArr.Length) {
                foreach (var userid in wholeUserIdsArr) {
                    if (!hadBindUserIdsArr.Contains(userid)) {
                        var userinfo = _accountRepository.GetAccountUserInfo(int.Parse(userid));
                        var detail = new LocationDetailInfo { entity_name = int.Parse(userid), entity_desc = userinfo.UserName, Status = 2};
                        locationDetailList.Add(detail);
                    }
                }
            }
            //查询百度鹰眼接口
            Dictionary<string, string> queryDic = new Dictionary<string, string>() { };
            if (!string.IsNullOrEmpty(hadBindUserIdsStr)) {
                queryDic.Add("filter", "entity_names:" + hadBindUserIdsStr);
            }
            List<LocationDetailInfo> searchResult =  BaiduTrackHelper.LocationSearch(locationSearchURL, queryDic, searchQuery);
            foreach(var item in searchResult)
            {
                System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
                DateTime dt = startTime.AddSeconds(item.latest_location.loc_time);
                item.latest_location.loc_time_format = dt;
                foreach (var userInfo in hadBindUserInfo) {
                    if (userInfo.UserId == item.entity_name) {
                        item.entity_desc = userInfo.UserName;
                        TimeSpan sp = DateTime.Now.Subtract(dt);
                        item.Status = sp.Minutes > userInfo.WarningInterval ? 3 : 1;
                        item.WarningInterval = userInfo.WarningInterval;
                        item.latest_location.lot_address =  BaiduTrackHelper.SearchAddressByLocationPoint(item.latest_location.latitude, item.latest_location.longitude);
                    }
                }
            }

            //绑定定位策略但未有任何定位信息
            if (searchResult.Count < hadBindUserIdsArr.Length)
            {
                var hadLocationUserIds = searchResult.Select(x => x.entity_name.ToString()).ToArray();
                foreach (var userid in hadBindUserIdsArr)
                {
                    if (!hadLocationUserIds.Contains(userid))
                    {
                        UserTrackStrategyInfo userinfo = hadBindUserInfo.Where(x => x.UserId.ToString() == userid).FirstOrDefault();
                        var detail = new LocationDetailInfo { entity_name = int.Parse(userid), entity_desc = userinfo.UserName, Status = 3, WarningInterval = userinfo.WarningInterval };
                        locationDetailList.Add(detail);
                    }
                }
            }

            locationDetailList.AddRange(searchResult);
            return new OutputResult<object>(locationDetailList);
        }

        public OutputResult<object> GetTrack(TrackQuery trackQuery, int userNumber)
        {
            //1、鹰眼定位数据
            Dictionary<string, string> queryDic = new Dictionary<string, string>() { };
            queryDic.Add("entity_name", trackQuery.UserId.ToString());
            queryDic.Add("is_processed", trackQuery.IsProcessed.ToString());
            queryDic.Add("page_index", trackQuery.PageIndex.ToString());
            queryDic.Add("page_size", trackQuery.PageSize.ToString());
            if(trackQuery.IsProcessed == 1 && !string.IsNullOrEmpty(trackQuery.ProcessOption)) {
                queryDic.Add("process_option", trackQuery.ProcessOption);
            }
            if (trackQuery.searchDate != null) {
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
                long startTimeStamp = (long)(trackQuery.searchDate - startTime).TotalSeconds; // 相差秒数
                long endTimeStamp = (long)(trackQuery.searchDate.AddHours(24).AddSeconds(-1) - startTime).TotalSeconds; // 相差秒数
                queryDic.Add("start_time", startTimeStamp.ToString());
                queryDic.Add("end_time", endTimeStamp.ToString());
            }
            var trackData = BaiduTrackHelper.GetTrackData(getTrackDataURL, queryDic);

            //2、签到数据
            if (trackQuery.PageIndex == 1)
            {
                List<CustVisitLocation> custVisitLocation = _baiduTrackRepository.GetVisitCustDataList(trackQuery.searchDate, trackQuery.UserId, userNumber);
                trackData.custVisitLocation = custVisitLocation;
            }

            return new OutputResult<object>(trackData);
        }

        public OutputResult<object> StayPoint(StayPointQuery stayPointQuery, int userNumber)
        {
            //1、鹰眼定位数据
            Dictionary<string, string> queryDic = new Dictionary<string, string>() { };
            queryDic.Add("entity_name", stayPointQuery.UserId.ToString());
            queryDic.Add("stay_time", stayPointQuery.StayTime == 0 ? "600" :  stayPointQuery.StayTime.ToString());
            queryDic.Add("stay_radius", stayPointQuery.StayRadius == 0 ? "20" : stayPointQuery.StayRadius.ToString());
            if (stayPointQuery.searchDate != null)
            {
                DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
                long startTimeStamp = (long)(stayPointQuery.searchDate - startTime).TotalSeconds; // 相差秒数
                long endTimeStamp = (long)(stayPointQuery.searchDate.AddHours(24).AddSeconds(-1) - startTime).TotalSeconds; // 相差秒数
                queryDic.Add("start_time", startTimeStamp.ToString());
                queryDic.Add("end_time", endTimeStamp.ToString());
            }
            var stayPointData = BaiduTrackHelper.StayPoint(stayPointURL, queryDic);
            return new OutputResult<object>(stayPointData);
        }

        public OutputResult<object> DownLoadTrackDataZip()
        {
            //todo 待完善
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "bak", "157572_103626.zip");
            BaiduTrackHelper.TraceFileDownSave(@"http://gz.bcebos.com/v1/mapopen-yingyan-export/track/157572_103626.zip?authorization=bce-auth-v1/add02280138247ffafecb6baf2bbfa98/2018-04-03T00:41:04Z/172800/host/e9366d7b8dd97e657a40e08b81385692dfb289c9c1411352d9cb38010bdd3099", filePath);
            return null;
        }

        public OutputResult<object> GetNearbyCustomerList(NearbyCustomerQuery query, int userNumber)
        {
            List<NearbyCustomerInfo> custList = _baiduTrackRepository.GetNearbyCustomerList(query, userNumber);
            return new OutputResult<object>(custList);
        }
    }
}
