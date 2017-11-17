using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class QueryAppTagsModel:BaseRequestModel
    {
        /// <summary>
        /// 开始值
        /// </summary>
        public int start { set; get; }

        /// <summary>
        /// 限制数量
        /// </summary>
        public int limit { set; get; } = 100;

    }
}
