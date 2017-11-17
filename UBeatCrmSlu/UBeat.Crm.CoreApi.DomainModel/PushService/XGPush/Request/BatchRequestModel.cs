using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.PushService.XGPush
{
    public class BatchRequestModel:BaseRequestModel
    {
        /// <summary>
        /// json字符串，包含若干标签-token对，后台将把每一对里面的token打上对应的标签。
        /// 每次调用最多允许设置20对，每个对里面标签在前，token在后。
        /// 注意标签最长50字节，不可包含空格；真实token长度至少40字节。
        /// 示例（其中token值仅为示意）： [[”tag1”,”token1”],[”tag2”,”token2”]]
        /// </summary>
        public string tag_token_list { set; get; }

    }
}
