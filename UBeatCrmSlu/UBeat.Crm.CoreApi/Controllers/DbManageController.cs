using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.DomainModel.DbManage;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Services;
using System.Text.Encodings.Web;

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

        /// <summary>
        /// 导出初始化脚本
        /// </summary>
        /// <param name="belongto"></param>
        /// <param name="isstruct"></param>
        /// <returns></returns>
        [HttpGet("export")]
        [AllowAnonymous]
        public async Task<IActionResult> ExportSQL([FromQuery]  int belongto, [FromQuery] int isstruct) {
            SQLExportParamInfo paramInfo = new SQLExportParamInfo();
            paramInfo.ExportSys = (SQLObjectBelongSysEnum)belongto;
            paramInfo.IsStruct = (StructOrData)isstruct;
            return await Task.Run<IActionResult>(() =>
            {
                IActionResult result = NotFound();
                string sql = this._dbManageServices.ExportInitSQL(paramInfo, UserId);
                byte[] buf = System.Text.Encoding.UTF8.GetBytes(sql);
                RequestHeaders requestHeaders = new RequestHeaders(Request.Headers);
                Response.ContentType = "application/octet-stream";
                Response.Headers.Add("Content-Disposition", "attachment; filename=db_init.sql");
                Response.Headers.Add("Accept-Ranges", "bytes");//告诉客户端接受资源为字节
                Response.Headers.Add("filename", "db_init.sql");
                Response.Headers.Add("filelength", buf.Length.ToString());
                Response.Headers.Add("Content-Length", buf.Length.ToString());//添加头文件，指定文件的大小，让浏览器显示文件下载的速度
                Response.Body.Write(buf, 0, buf.Length);
                result = File(Response.Body, "application/octet-stream");
                return result;
            });
        }

        /// <summary>
        /// 反向生成脚本，支持表格和函数
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost("reflect")]
        [AllowAnonymous]
        public OutputResult<object> reflect([FromBody] SQLReflectQueryModel paramInfo) {
            if (paramInfo == null) {
                return new OutputResult<object>("参数异常", "参数异常", -1);
            }
            return this._dbManageServices.ReflectInitStructSQL(paramInfo.RecIds, 0);
        }

        [HttpPost("saveobject")]
        [AllowAnonymous]
        public OutputResult<object> SaveObject([FromBody] SQLObjectModel paramInfo) {
            if (paramInfo == null)
                return new OutputResult<object>("参数异常", "参数异常", -1);
            return this._dbManageServices.SaveObject(paramInfo, UserId);
        }
        [HttpPost("saveupgradesql")]
        [AllowAnonymous]
        public OutputResult<object> saveUpgrade([FromBody] SQLTextModel paramInfo) {
            if (paramInfo == null)
                return new OutputResult<object>("参数异常", "参数异常", -1);
            return this._dbManageServices.SaveUpgradeSQL(paramInfo, UserId);
        }
        [HttpPost("listdir")]
        [AllowAnonymous]
        public OutputResult<object> ListDirs()
        {
            return this._dbManageServices.ListDir(UserId);
        }
        [HttpPost("listobjects")]
        [AllowAnonymous]
        public OutputResult<object> ListObjects([FromBody] DbListObjectsParamInfo paramInfo) {
            if (paramInfo == null)
                return new OutputResult<object>("参数异常", "参数异常", -1);
            return this._dbManageServices.ListObjects(paramInfo, UserId);
        }
    }
}
