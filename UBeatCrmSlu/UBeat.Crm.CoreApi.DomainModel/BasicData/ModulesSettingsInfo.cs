using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.DomainModel.BasicData
{
    /// <summary>
    /// 第三方服务配置信息
    /// </summary>
    public class ModulesSettingsInfo
    {
        public int IsMailSys { get; set; }
        public int IsCardScan { get; set; }
        public int IsCloudDialing { get; set; }
    }
}
