using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class TagsDeviceModel:PushBaseModel
    {
        /// <summary>
        /// tags 的 Json,如[“tag1”,”tag2”,”tag3”]
        /// </summary>
        public string tags_list { set; get; }

        /// <summary>
        /// 取值为AND或OR
        /// </summary>
        public string tags_op { set; get; }

       

        /// <summary>
        /// 指定推送时间，格式为year-mon-day hour:min:sec 若小于服务器当前时间，则会立即推送
        /// </summary>
        public string send_time { set; get; }


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
