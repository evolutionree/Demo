using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class CreateMultiPushRequest : PushBaseModel
    {
        ///// <summary>
        ///// 消息类型：1：通知 2：透传消息
        ///// </summary>
        //public int message_type { set; get; }

        //public string message { set; get; }

        ///// <summary>
        ///// 消息离线存储时间（单位为秒），最长存储时间3天。若设置为0，则使用默认值（3天）。在超时时间内，可以发起此消息的批量推送。
        ///// </summary>
        //public int expire_time { set; get; }

        ///// <summary>
        ///// 0表示按注册时提供的包名分发消息；1表示按access id分发消息，所有以该access id成功注册推送的app均可收到消息
        ///// </summary>
        //public int multi_pkg { set; get; }

        ///// <summary>
        ///// 向iOS设备推送时必填，1表示推送生产环境；2表示推送开发环境。推送Android平台不填或填0
        ///// </summary>
        //public int environment { set; get; }

    }

    public class MultiDevicesModel : BaseRequestModel
    {
        /// <summary>
        /// Json数组格式，每个元素是一个token，string类型，单次发送token不超过1000个。 例：[“token1”,”token2”,”token3”]
        /// </summary>
        public string device_list { set; get; }

        /// <summary>
        /// 创建批量推送消息 接口的返回值中的 push_id
        /// </summary>
        public long push_id { set; get; }
    }
}
