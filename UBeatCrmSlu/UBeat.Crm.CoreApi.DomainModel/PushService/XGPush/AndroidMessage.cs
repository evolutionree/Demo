using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    /// <summary>
    /// 推送通知实体定义
    /// </summary>
    public class AndroidMessage: XGMessage
    {
        /// <summary>
        /// 内容，必填
        /// </summary>
        [JsonProperty("content")]
        public string Content { set; get; }
        /// <summary>
        /// 标题，必填
        /// </summary>
        [JsonProperty("title")]
        public string Title { set; get; }
        /// <summary>
        /// 本地通知样式，必填
        /// </summary>
        [JsonProperty("builder_id")]
        public int Builder_id { set; get; }


        /// <summary>
        /// 表示消息将在哪些时间段允许推送给用户，选填
        /// </summary>
        [JsonProperty("accept_time")]
        public List<AcceptTime> AcceptTimes { set; get; } = new List<AcceptTime>();
        

        /// <summary>
        /// 通知id，选填。若大于0，则会覆盖先前弹出的相同id通知；若为0，展示本条通知且不影响其他通知；若为-1，将清除先前弹出的所有通知
        /// </summary>
        [JsonProperty("n_id")]
        public int NId { set; get; }

        /// <summary>
        /// 是否响铃，0否，1是，下同。选填，默认1
        /// </summary>
        [JsonProperty("ring")]
        public int Ring { set; get; } = 1;

        /// <summary>
        /// 指定应用内的声音（ring.mp3），选填
        /// </summary>
        [JsonProperty("ring_raw")]
        public string RingRaw { set; get; }

        /// <summary>
        /// 是否振动，选填，默认1
        /// </summary>
        [JsonProperty("vibrate")]
        public int Vibrate { set; get; } = 1;

        /// <summary>
        /// 是否呼吸灯，0否，1是，选填，默认1
        /// </summary>
        [JsonProperty("lights")]
        public int Lights { set; get; } = 1;

        /// <summary>
        /// 通知栏是否可清除，选填，默认1
        /// </summary>
        [JsonProperty("clearable")]
        public int Clearable { set; get; } = 1;

        /// <summary>
        /// 默认0，通知栏图标是应用内图标还是上传图标,0是应用内图标，1是上传图标,选填
        /// </summary>
        [JsonProperty("icon_type")]
        public int IconType { set; get; } = 0;

        /// <summary>
        /// 应用内图标文件名（xg.png）或者下载图标的url地址，选填
        /// </summary>
        [JsonProperty("icon_res")]
        public string IconRes { set; get; }

        /// <summary>
        /// Web端设置是否覆盖编号的通知样式，默认1，0否，1是,选填
        /// </summary>
        [JsonProperty("style_id")]
        public int StyleId { set; get; } = 1;

        /// <summary>
        /// 指定状态栏的小图片(xg.png),选填
        /// </summary>
        [JsonProperty("small_icon")]
        public string SmallIcon { set; get; }

        /// <summary>
        /// 动作，选填。默认为打开app
        /// </summary>
        [JsonProperty("action")]
        public ActionInfo Action { set; get; }

    }

    /// <summary>
    /// 透传消息实体定义
    /// </summary>
    public class PassthroughMessage : XGMessage
    {
        /// <summary>
        /// 内容，选填
        /// </summary>
        [JsonProperty("content")]
        public string Content { set; get; }

        /// <summary>
        /// 标题，选填
        /// </summary>
        [JsonProperty("title")]
        public string Title { set; get; }

        /// <summary>
        /// 表示消息将在哪些时间段允许推送给用户，选填
        /// </summary>
        [JsonProperty("accept_time")]
        public List<AcceptTime> AcceptTimes { set; get; } = new List<AcceptTime>();

       
    }


    public class ActionInfo
    {
        /// <summary>
        /// 动作类型，1打开activity或app本身，2打开浏览器，3打开Intent
        /// </summary>
        [JsonProperty("action_type")]
        public int ActionType { set; get; } = 1;


        [JsonProperty("activity")]
        public string Activity { set; get; }

        /// <summary>
        /// activity属性，只针对action_type=1的情况
        /// </summary>
        [JsonProperty("aty_attr")]
        public ActivityAttrInfo ActivityAttr { set; get; }

        /// <summary>
        /// url：打开的url，confirm是否需要用户确认     
        /// </summary>
        [JsonProperty("browser")]
        public BrowserInfo Browser { set; get; }

        [JsonProperty("intent")]
        public string Intent { set; get; }

    }

    public class ActivityAttrInfo
    {
        /// <summary>
        /// 创建通知时，intent的属性，如：intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_
        /// </summary>
        [JsonProperty("if")]
        public int intentFlag { set; get; }

        /// <summary>
        /// PendingIntent的属性，如：PendingIntent.FLAG_UPDATE_CURRENT
        /// </summary>
        [JsonProperty("pf")]
        public int PendingIntentFlag { set; get; }
    }
    public class BrowserInfo
    {
        /// <summary>
        /// 打开的url
        /// </summary>
        [JsonProperty("url")]
        public string Url { set; get; }

        /// <summary>
        /// 是否需要用户确认     
        /// </summary>
        [JsonProperty("confirm")]
        public int Confirm { set; get; } = 1;
    }

    /// <summary>
    /// 消息推送时间。json格式：
    /// {
    ///   “start”:{“hour”:”00”,”min”:”00”},
    ///    ”end”: {“hour”:”09”,“min”:”00”} 
    /// }
    /// </summary>
    public class AcceptTime
    {
        /// <summary>
        /// 开始时间,json格式“start”:{“hour”:”00”,”min”:”00”}
        /// </summary>
        [JsonProperty("start")]
        public TimeSet Start { set; get; } = new TimeSet() { hour = "00", min = "00" };

        /// <summary>
        /// 结束时间,json格式 ”end”: {“hour”:”09”,“min”:”00”} 
        /// </summary>
        [JsonProperty("end")]
        public TimeSet End { set; get; } = new TimeSet() {hour="23",min="59" };
    }


    /// <summary>
    /// 时间范围设置，json格式：{“hour”:”09”,“min”:”00”} 
    /// </summary>
    public class TimeSet
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        [JsonProperty("hour")]
        public string hour { set; get; } 

        /// <summary>
        /// 开始时间
        /// </summary>
        [JsonProperty("min")]
        public string min { set; get; }
    }
}
