using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel.DbManage;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DbManageController : BaseController
    {
        private readonly DbManageServices _dbManageServices;
        public DbManageController(DbManageServices dbManageServices) : base(dbManageServices) {
            this._dbManageServices = dbManageServices;
        }
        [HttpPost("test")]
        [AllowAnonymous]
        public OutputResult<object> test() {
            return null;
            // return this._dbManageServices.GenerateTableCreateSQL("crm_sys_entity", UserId);
            //return this._dbManageServices.GenerateProcSQL("crm_func_daily_add", UserId);
        }

        [HttpGet("export")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportSQL([FromBody] SQLExportParamInfo paramInfo) {
            if (paramInfo == null)
                return ResponseError("缺乏查询参数");
            return await Task.Run<IActionResult>(() =>
            {
                IActionResult result = NotFound();
               // this._dbManageServices.ExportSQL(paramInfo, UserId);
                return result;
            });
        }
        [HttpPost("reflect")]
        [AllowAnonymous]
        public OutputResult<object> reflect([FromBody] SQLReflectQueryModel paramInfo) {
            if (paramInfo == null || paramInfo.RecIds == null || paramInfo.RecIds.Length == 0) {
                return new OutputResult<object>("参数异常", "参数异常", -1);
            }
            return this._dbManageServices.ReflectInitStructSQL(paramInfo.RecIds, 0);
        }

    }
}
