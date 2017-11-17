﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class PushBaseModel:BaseRequestModel
    {
        /// <summary>
        /// 消息类型：1：通知 2：透传消息。iOS平台请填0
        /// </summary>
        public int message_type { set; get; }

        /// <summary>
        /// 消息内容Json数据，IOS平台使用IOSMessage对象，Android平台使用AndroidMessage（推送通知）对象和 PassthroughMessage（透传消息）对象
        /// </summary>
        public string message { set; get; }

        /// <summary>
        /// 消息离线存储时间（单位为秒），最长存储时间3天。若设置为0，则使用默认值（3天）。在超时时间内，可以发起此消息的批量推送。
        /// </summary>
        public int expire_time { set; get; }

        /// <summary>
        /// 向iOS设备推送时必填，1表示推送生产环境；2表示推送开发环境。推送Android平台不填或填0
        /// </summary>
        public int environment { set; get; }

        /// <summary>
        /// 0表示按注册时提供的包名分发消息；1表示按access id分发消息，所有以该access id成功注册推送的app均可收到消息。本字段对iOS平台无效
        /// </summary>
        public int multi_pkg { set; get; }
    }
}
