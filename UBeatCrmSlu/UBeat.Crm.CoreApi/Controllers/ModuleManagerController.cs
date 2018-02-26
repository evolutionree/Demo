using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel.BasicData;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class ModuleManagerController: BaseController
    {
        private readonly ModuleManagerServices _moduleManagerServices;
        public ModuleManagerController(ModuleManagerServices moduleManagerServices) : base(moduleManagerServices) {
            this._moduleManagerServices = moduleManagerServices;
        }
        [HttpPost]
        [Route("personalmodules")]
        public OutputResult<object> GetPersonalModules() {
            ModulesSettingsInfo info = this._moduleManagerServices.getPersonalSetting(UserId);
            return new OutputResult<object>(info);
        }
    }
}
