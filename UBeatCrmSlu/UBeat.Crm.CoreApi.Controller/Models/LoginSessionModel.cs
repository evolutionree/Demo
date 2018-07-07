using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UBeat.Crm.CoreApi.Models
{
    public class LoginSessionModel
    {

        public Dictionary<string, TokenInfo> Sessions { set; get; } = new Dictionary<string, TokenInfo>();

        public TimeSpan Expiration { set; get; }

        /// <summary>
        /// 是否多设备同时登陆
        /// </summary>
        public bool IsMultipleLogin { set; get; }


        /// <summary>
        /// 最新登陆session
        /// </summary>
        public string LatestSession { set; get; }




    }

    public class TokenInfo
    {
        public string Token { set; get; }

        public DateTime Expiration { set; get; }
        /// <summary>
        /// 记录本次登录的时间戳
        /// </summary>
        public long RequestTimeStamp { set; get; }

        public TokenInfo(string token,DateTime expiration, long requestTimeStamp)
        {
            Token = token;
            Expiration = expiration;
            RequestTimeStamp = requestTimeStamp;
        }
    }
}
