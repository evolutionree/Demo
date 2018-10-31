using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;

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

        public string AGENTId { get; set; }
        static IConfigurationSection config;

        private static DingTalkConfig instance = null;
        public static DingTalkConfig getInstance()
        {
            if (instance != null) return instance;
            config = ServiceLocator.Current.GetInstance<IConfigurationRoot>().GetSection("DingdingConfig");
            instance = new DingTalkConfig()
            {
                AGENTId = config.GetValue<string>("AgentId"),
                CorpSecret = config.GetValue<string>("CorpSecret"),
                CorpId = config.GetValue<string>("CorpId"),
                SSOCorpSecret = "9Shab6LrfXPP_gaPpFx_jNbqTsjj2Ii-jt1BaL4nGSUskwjWNyA7Mu90sO_n9fU_",
                AppId = "dingoakvipp0zjcxb5mm2q",
                AppSecret = "OFS2koE1MpdJdzd54kt1RRimwpnDVm7kLYv9OPU5Rz6fBQvvyQHPcNHd5CsHSOB-",
            };
            return instance;
        }
    }
}
