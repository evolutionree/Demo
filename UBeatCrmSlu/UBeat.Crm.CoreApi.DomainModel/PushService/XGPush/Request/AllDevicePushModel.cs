using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class AllDevicePushModel : PushBaseModel
    {
        ///// <summary>
        ///// 消息类型：1：通知 2：透传消息。iOS平台请填0
        ///// </summary>
        //public int message_type { set; get; }

        //public string message { set; get; }
        ///// <summary>
        ///// 消息离线存储时间（单位为秒），最长存储时间3天。若设置为0，则使用默认值（3天）
        ///// </summary>
        //public int expire_time { set; get; }

        /// <summary>
        /// 指定推送时间,格式为year-mon-day hour:min:sec 若小于服务器当前时间，则会立即推送
        /// </summary>
        public string send_time { set; get; }

        ///// <summary>
        ///// 0表示按注册时提供的包名分发消息；1表示按access id分发消息，所有以该access id成功注册推送的app均可收到消息。本字段对iOS平台无效
        ///// </summary>
        //public int multi_pkg { set; get; }

        ///// <summary>
        ///// 向iOS设备推送时必填，1表示推送生产环境；2表示推送开发环境。推送Android平台不填或填0
        ///// </summary>
        //public int environment { set; get; }
        /// <summary>
        /// 循环任务执行的次数，取值[1, 15]
        /// </summary>
        public int loop_times { set; get; }
        /// <summary>
        /// 循环任务的执行间隔，以天为单位，取值[1, 14]。loop_times和loop_interval一起表示任务的生命周期，不可超过14天
        /// </summary>
        public int loop_interval { set; get; }

    }
}
