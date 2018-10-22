using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DingTalk.Utils
{
    public class DingTalkConfig
    {
        /// <summary>
        /// 企业ID
        /// </summary>
        public string CorpId { get; set; }
        /// <summary>
        /// 企业秘钥
        /// </summary>
        public string CorpSecret { get; set; }
        public string SSOCorpSecret { get; set; }

        /// <summary>
        /// 应用id
        /// </summary>
        public long AccessId { get; set; }


        public string AppId { get; set; }

        public string AppSecret { get; set; }


        private static DingTalkConfig instance = new DingTalkConfig()
        {
            CorpId = "dingc464c1bcba24669335c2f4657eb6378f",
            CorpSecret = "rir6aMQbAGS5hObb2WPYoYfoh5mrke2L9IKbQILSplsrj5Xyjkvc0xNqp3QVa6nU",
            SSOCorpSecret= "9Shab6LrfXPP_gaPpFx_jNbqTsjj2Ii-jt1BaL4nGSUskwjWNyA7Mu90sO_n9fU_",
            AppId= "dingoakvipp0zjcxb5mm2q",
            AppSecret= "OFS2koE1MpdJdzd54kt1RRimwpnDVm7kLYv9OPU5Rz6fBQvvyQHPcNHd5CsHSOB-",
 
        };
        public static DingTalkConfig getInstance() {
            return instance;
        }
    }
}
