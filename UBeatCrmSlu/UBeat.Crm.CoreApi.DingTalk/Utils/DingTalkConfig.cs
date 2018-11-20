using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using System.Linq;
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
        public string RootDeptId { get; set; }
        static IConfigurationSection config;
        public List<MapperDeptId> mapperDeptIds { get; set; }
        private static DingTalkConfig instance = null;
        public static DingTalkConfig getInstance()
        {
            if (instance != null) return instance;
            IConfigurationRoot configRoot = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            var deptIds = configRoot.GetSection("DeptIdMapper").Get<List<MapperDeptId>>();
            config = configRoot.GetSection("DingdingConfig");
            instance = new DingTalkConfig()
            {
                AGENTId = config.GetValue<string>("AgentId"),
                CorpSecret = config.GetValue<string>("CorpSecret"),
                CorpId = config.GetValue<string>("CorpId"),
                mapperDeptIds = deptIds,
                // RootDeptId = config.GetValue<string>("RootDeptId"),
                //SSOCorpSecret = "9Shab6LrfXPP_gaPpFx_jNbqTsjj2Ii-jt1BaL4nGSUskwjWNyA7Mu90sO_n9fU_",
                //AppId = "dingoakvipp0zjcxb5mm2q",
                //AppSecret = "OFS2koE1MpdJdzd54kt1RRimwpnDVm7kLYv9OPU5Rz6fBQvvyQHPcNHd5CsHSOB-",
            };

            return instance;
        }


    }

    public class MapperDeptId
    {
        public String DingDingDeptId { get; set; }
        public String CrmDeptId { get; set; }
    }
}
