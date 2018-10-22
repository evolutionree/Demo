using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DingTalk.Models
{
    public class AccessTokenInfo
    {
        public  string Access_Token { get; set; }
    }

    public class AccessTokenResponseInfo {
        public int ErrorCode { get; set; }
        public string ErrorMsg { get; set; }
        public string Access_Token { get; set; }
    }
}
