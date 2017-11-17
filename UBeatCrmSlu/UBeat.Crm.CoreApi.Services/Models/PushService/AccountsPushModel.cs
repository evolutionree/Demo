using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Models.PushService
{
    public class AccountsPushModel
    {
        /// <summary>
        /// 账号推送,多个时使用','分割
        /// </summary>
        public string Accounts { set; get; }
        /// <summary>
        /// 安卓平台的消息title
        /// </summary>
        public string Title { set; get; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { set; get; }

        /// <summary>
        /// 指定推送时间，格式为year-mon-day hour:min:sec 若小于服务器当前时间，则会立即推送
        /// </summary>
        public string SendTime { set; get; }

        /// <summary>
        /// 用户自定义的key-value，选填
        /// </summary>
        public Dictionary<string, object> CustomContent { set; get; }
    }

    public class AccountsPushExtModel
    {
        /// <summary>
        /// 账号推送
        /// </summary>
        public List<string> Accounts { set; get; }
        /// <summary>
        /// 安卓平台的消息title
        /// </summary>
        public string Title { set; get; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { set; get; }

        /// <summary>
        /// 指定推送时间，格式为year-mon-day hour:min:sec 若小于服务器当前时间，则会立即推送
        /// </summary>
        public string SendTime { set; get; }

        /// <summary>
        /// 用户自定义的key-value，选填
        /// </summary>
        public Dictionary<string, object> CustomContent { set; get; }
    }
}
