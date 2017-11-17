using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class MultiAccountModel:BaseRequestModel 
    {
        /// <summary>
        /// Json数组格式，每个元素是一个account，string类型，单次发送account不超过1000个。例：[“account1”,”account2”,”account3”]
        /// </summary>
        public string account_list { set; get; }

        /// <summary>
        /// 创建批量推送消息 接口的返回值中的 push_id
        /// </summary>
        public long push_id { set; get; }
    }
}
