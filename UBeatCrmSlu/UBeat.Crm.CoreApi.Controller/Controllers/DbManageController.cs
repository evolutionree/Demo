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
using System.Reflection;
using System.IO;
using UBeat.Crm.CoreApi.Services.Models.DbManage;
using System.Text;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class DbManageController : BaseController
    {
        private readonly DbManageServices _dbManageServices;
        private readonly DbEntityManageServices _dbEntityManageServices;
        public DbManageController(DbManageServices dbManageServices,
                    DbEntityManageServices dbEntityManageServices) : base(dbManageServices, dbEntityManageServices) {
            this._dbManageServices = dbManageServices;
            this._dbEntityManageServices =  dbEntityManageServices;
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
            return this._dbManageServices.SaveObject(paramInfo, UserId,0);
        }
        [HttpPost("saveobjectforbase")]
        [AllowAnonymous]
        public OutputResult<object> SaveObjectForBase([FromBody] SQLObjectModel paramInfo)
        {
            if (paramInfo == null)
                return new OutputResult<object>("参数异常", "参数异常", -1);
            return this._dbManageServices.SaveObject(paramInfo, UserId,1);
        }
        /// <summary>
        /// 获取脚本信息
        /// </summary>
        /// <returns></returns>

        [AllowAnonymous]
        [Route("getobjectsql")]
        public OutputResult<object> GetObjectSQL([FromBody] DbGetSQLParamInfo paramInfo) {
            if (paramInfo == null || paramInfo.ObjId == null || paramInfo.ObjId == Guid.Empty) {
                return ResponseError<object>("参数异常");
            }
            string tmp = this._dbManageServices.getObjectSQL(paramInfo, UserId);
            if (tmp == null) tmp = "";
            return new OutputResult<object>(tmp);
        }
        /// <summary>
        /// 保存脚本
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("saveobjectsql")]
        public OutputResult<object> SaveObjectSQL([FromBody] DbSaveSQLParamInfo paramInfo) {
            if (paramInfo == null || paramInfo.ObjId == null || paramInfo.ObjId == Guid.Empty) {
                return ResponseError<object>("参数异常");
            }
            if (paramInfo.SqlText == null) paramInfo.SqlText = "";
            return this._dbManageServices.SaveObjectSQL(paramInfo, UserId);

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
        [HttpPost("exportentity")]
        [AllowAnonymous]
        public OutputResult<object> ExportEntity([FromBody]EntityExportParamInfo paramInfo) {
            if (paramInfo == null || paramInfo.EntityId == null || paramInfo.EntityId == Guid.Empty)
                return new OutputResult<object>("参数异常", "参数异常", -1);
            DbEntityReflectParamInfo reflectParamInfo = new DbEntityReflectParamInfo();
            reflectParamInfo.EntityId = paramInfo.EntityId.ToString();
            DbEntityInfo info = this._dbEntityManageServices.ReflectEntity(reflectParamInfo, UserId);
            #region 保存到文件
            string tmp = System.IO.Directory.GetCurrentDirectory();
            DirectoryInfo path = new System.IO.DirectoryInfo(tmp);
            DirectoryInfo subPath = path.CreateSubdirectory("entityjson");
            string filename = subPath.FullName + Path.DirectorySeparatorChar + info.EntityId + ".json";
            System.IO.FileStream f = new FileStream(filename, FileMode.OpenOrCreate);
            string outStr = Newtonsoft.Json.JsonConvert.SerializeObject(info);
            byte[] buf = System.Text.UTF8Encoding.UTF8.GetBytes(outStr);
            f.Write(buf, 0, buf.Length);
            f.Flush(); 
            #endregion
            return new OutputResult<object>(Newtonsoft.Json.JsonConvert.SerializeObject(info));
        }
        [HttpPost("importentity")]
        [AllowAnonymous]
        public OutputResult<object> ImportEntity([FromBody] DbEntityImportParamInfo paramInfo) {
            if (paramInfo == null || paramInfo.EntityId == null || paramInfo.EntityId == Guid.Empty) return ResponseError<object>("参数异常");
            #region 检查当前目录是否有json文件
            string tmp = System.IO.Directory.GetCurrentDirectory();
            DirectoryInfo path = new System.IO.DirectoryInfo(tmp);
            DirectoryInfo subPath = path.CreateSubdirectory("entityjson");
            string filename = subPath.FullName + Path.DirectorySeparatorChar + paramInfo.EntityId.ToString() + ".json";
            if (System.IO.File.Exists(filename) == false) {
                return ResponseError<object>("文件不存在");
            }
            int buflen = 1024 * 1024 * 20;
            byte[] buf = new byte[buflen];
            FileStream fin = new FileStream(filename, FileMode.Open);
            int index = 0;
            int readLen = 0;
            while ((readLen = fin.Read(buf,index,buflen - index)) > 0){
                index += readLen;
            }
            string tmpjson = System.Text.UTF8Encoding.UTF8.GetString(buf, 0, index);
            DbEntityInfo entityInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<DbEntityInfo>(tmpjson);
            this._dbEntityManageServices.ImportEntity(entityInfo, paramInfo, UserId);
            #endregion
            return null;
        }


        [HttpPost("dbsizestat")]
        [AllowAnonymous]
        public OutputResult<object> StatDbSize() {
            DbSizeStatInfo statInfo = this._dbManageServices.StatDbSize(UserId);
            return new OutputResult<object>(statInfo);
        }


        [HttpGet("workflowconfigs")]
        [AllowAnonymous]
        public IActionResult GetWorkFlowInfoList([FromQuery]WorkFlowInfoListModel queryParam)
        {
            var jsonText = _dbManageServices.GetWorkFlowInfoList(queryParam);
            byte[] json_bytes = System.Text.Encoding.UTF8.GetBytes(jsonText);
            return File(json_bytes, "application/octet-stream; charset=utf-8", "workflowconfigs.json");

        }
        /// <summary>
        /// 保存工作流配置
        /// </summary>
        /// <param name="paramInfo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("saveworkflowconfigs")]
        public OutputResult<object> SaveWorkFlowInfoList([FromForm]SaveWorkFlowInfoModel formData)
        {
            if (formData == null)
                return ResponseError<object>("缺乏查询参数");

            var stream = formData.Data.OpenReadStream();
            var jsonText = string.Empty;
            using (StreamReader sr = new StreamReader(stream))
            {
                jsonText = sr.ReadToEnd().ToString();
            }
            return this._dbManageServices.SaveWorkFlowInfoList(jsonText, UserId);
        }
    }
    

}
