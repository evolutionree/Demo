using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.Version;

namespace UBeat.Crm.CoreApi.Services.Models.MetaData
{
    public class IncrementDataRespone
    {
        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty("data")]
        public Dictionary<string, object> Datas { set; get; } = new Dictionary<string, object>();
        [JsonProperty("version")]
        public List<IncrementDataModel> Versions { set; get; } = new List<IncrementDataModel>();

        /// <summary>
        /// 需要继续拿数据的key
        /// </summary>
        [JsonProperty("hasmoredata")]
        public List<string> HasMoreData { set; get; } = new List<string>();

    }
}
