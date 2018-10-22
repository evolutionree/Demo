using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DingTalk.Models
{

    public class GetUserInfoResponseInfo : DingTalkResponseInfo
    {
        public string UnionId { get; set; }
        public string OpenId { get; set; }
        public string Remark { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
    }



    public class DingTalkUserInfo : DingTalkResponseInfo
    {
        public string userid { get; set; }
        public string deviceId { get; set; }
        public bool is_sys { get; set; }
        public string sys_level { get; set; }
    }



    public class DingTalkSSOUserInfo : DingTalkResponseInfo
    {


        public UserInfo user_info { get; set; }

        public CorpInfo corp_info { get; set; }

        public int errcode { get; set; }

        public string errmsg { get; set; }

        public bool is_sys { get; set; }

    }


    public class CorpInfo
    {
        public string corp_name { get; set; }

        public string corpid { get; set; }
    }


    public class UserInfo
    {

        public string email { get; set; }
        public string name { get; set; }
        public string userid { get; set; }

    }



 


 


 


}


