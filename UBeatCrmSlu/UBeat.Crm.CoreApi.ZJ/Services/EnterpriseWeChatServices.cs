using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.ZJ.WJXModel;

namespace UBeat.Crm.CoreApi.ZJ.Services
{
    public class EnterpriseWeChatServices : BasicBaseServices
    {
        public static readonly string OAuth2 = "https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_base&state=STATE#wechat_redirect";
        public OperateResult GetSSOCode(EnterpriseWeChatModel enterpriseWeChat)
        {

        }
    }
}
