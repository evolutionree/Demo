using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class QueryAppTagsResponseData
    {
        /// <summary>
        /// 应用的tag总数，注意不是本次查询返回的tag数
        /// </summary>
        [JsonProperty("total")]
        public int Total { set; get; }

        [JsonProperty("tags")]
        public List<string> Tags { set; get; }
    }
}
