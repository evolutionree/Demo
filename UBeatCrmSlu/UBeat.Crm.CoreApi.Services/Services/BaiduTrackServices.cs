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


        public OutputResult<object> GetRecentLocationByUserIds(LocationSearchInfo searchQuery, int userNumber)
        {
            //过滤掉没有分配定位策略的人员
            var dynamicService = (TrackConfigurationServices)dynamicCreateService(typeof(TrackConfigurationServices).FullName, true);
            var hadBindUserIds = dynamicService.FilterHadBindTrackStrategyUser(searchQuery.UserIds);

            //没有绑定定位策略的人员，固定返回“未定位,未分配策略”
            List<object> locationDetailList = new List<object>() { };
            var wholeUserIdsArr = searchQuery.UserIds.Split(",");
            var hadBindUserIdsArr = hadBindUserIds.Split(",");
            if (wholeUserIdsArr.Length != hadBindUserIdsArr.Length) {
                foreach (var userid in wholeUserIdsArr) {
                    if (!hadBindUserIds.Contains(userid)) {
                        var detail = new
                        {
                            UserId = int.Parse(userid),
                            UserName = "",
                        };
                        locationDetailList.Add(detail);
                    }
                }
            }

            //不做分页，size前端传一个大数
            Dictionary<string, string> queryDic = new Dictionary<string, string>() { };
            if (!string.IsNullOrEmpty(hadBindUserIds)) {
                queryDic.Add("filter", "entity_names:" + hadBindUserIds);
                queryDic.Add("sortby", "loc_time:desc");
                queryDic.Add("coord_type_output", "bd09ll");//该字段在国外无效，国外均返回 wgs84坐标
                queryDic.Add("page_index", searchQuery.PageIndex.ToString());
                queryDic.Add("page_size", searchQuery.PageSize.ToString());
            }
            var result = BaiduTrackHelper.LocationSearch(locationSearchURL, queryDic);
            var searchResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
            //searchResult["entities"].


            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("data1", locationDetailList);
            data.Add("data2", result);
            return new OutputResult<object>(data);
        }
    }
}
