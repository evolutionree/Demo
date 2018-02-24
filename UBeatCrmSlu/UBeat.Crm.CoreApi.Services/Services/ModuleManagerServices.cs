using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.DomainModel.BasicData;

namespace UBeat.Crm.CoreApi.Services.Services
{
    /// <summary>
    /// 用于管理各个模块的使用亲宽广
    /// </summary>
    public class ModuleManagerServices: EntityBaseServices
    {
        public ModulesSettingsInfo getPersonalSetting(int userid) {
            ModulesSettingsInfo modulesSettingsInfo = new ModulesSettingsInfo()
            {
                IsCardScan = 1,
                IsCloudDialing = 1,
                IsMailSys = 1
            };
            return modulesSettingsInfo;
        }
    }
}
