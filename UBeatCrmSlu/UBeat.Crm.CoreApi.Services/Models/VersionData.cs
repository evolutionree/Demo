using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models
{
    /// <summary>
    /// 版本号控制对象
    /// </summary>
    public class VersionData
    {
        /// <summary>
        /// 基础数据
        /// </summary>
        [JsonProperty("basicdata")]
        public long BasicData { set; get; } = 1;
        /// <summary>
        /// 字典数据
        /// </summary>
        [JsonProperty("dicdata")]
        public long DicData { set; get; } = 1;
        /// <summary>
        /// 产品数据
        /// </summary>
        [JsonProperty("productdata")]
        public long ProductData { set; get; } = 1;
        /// <summary>
        /// 实体数据
        /// </summary>
        [JsonProperty("entitydata")]
        public long EntityData { set; get; } = 1;
        /// <summary>
        /// 审批流程
        /// </summary>
        [JsonProperty("flowdata")]
        public long FlowData { set; get; } = 1;
        /// <summary>
        /// 权限配置
        /// </summary>
        [JsonProperty("powerdata")]
        public long PowerData { set; get; } = 1;

        /// <summary>
        /// 离线消息
        /// </summary>
        [JsonProperty("msgdata")]
        public long MsgData { set; get; } = 1;

        /// <summary>
        /// 定位策略配置数据
        /// </summary>
        [JsonProperty("tracksettingdata")]
        public long TrackSettingData { set; get; } = 1;
    }
}
