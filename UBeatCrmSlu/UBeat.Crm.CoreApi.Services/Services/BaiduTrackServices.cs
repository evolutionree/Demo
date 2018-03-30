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

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class BaiduTrackServices : BaseServices
    {
        static string locationSearchURL = "http://yingyan.baidu.com/api/v3/entity/search";

        private readonly IBaiduTrackRepository _repository;
        private readonly IAccountRepository _accountRepository;

        public BaiduTrackServices(IBaiduTrackRepository repository, IAccountRepository accountRepository)
        {
            _repository = repository;
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
            List<LocationDetailInfo> searchResult = BaiduTrackHelper.LocationSearch(locationSearchURL, queryDic);
            foreach(var item in searchResult)
            {
                System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
                DateTime dt = startTime.AddSeconds(item.latest_location.loc_time);
                item.latest_location.loc_time_format = dt;
                foreach (var userInfo in hadBindUserInfo) {
                    if (userInfo.UserId == item.entity_name) {
                        item.entity_desc = userInfo.UserName;
                        TimeSpan sp = DateTime.Now.Subtract(dt);
                        item.Status = sp.Minutes > userInfo.WarnningInterval ? 3 : 1;
                        item.WarnningInterVal = userInfo.WarnningInterval;
                        item.latest_location.lot_address = BaiduTrackHelper.SearchAddressByLocationPoint(item.latest_location.latitude, item.latest_location.longitude);
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
                        var detail = new LocationDetailInfo { entity_name = int.Parse(userid), entity_desc = userinfo.UserName, Status = 3, WarnningInterVal = userinfo.WarnningInterval };
                        locationDetailList.Add(detail);
                    }
                }
            }

            locationDetailList.AddRange(searchResult);
            return new OutputResult<object>(locationDetailList);
        }
    }
}
