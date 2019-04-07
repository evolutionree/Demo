using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.DevAssist.Models;
using UBeat.Crm.CoreApi.DevAssist.Utils;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.DevAssist.Controllers
{
    [Route("api/[controller]")]
    public class UKWebApiController: BaseController
    {

        public UKWebApiController() {

        }
        [AllowAnonymous]
        [UKWebApiAttribute("获取接口列表",Description = "获取所有的WEB - API接口的列表的接口,以及相关的参数说明")]
        [HttpPost("listapi")]
        public OutputResult<object> ListAllApi() {
            List<UKWebApiInfo> ret = UKControllerInspector.getInstance().ListAllApi();
            return new OutputResult<object>(ret);
        }
        [AllowAnonymous]
        [UKWebApiAttribute("保存接口", Description = "保存接口的备注信息")]
        [HttpPost("saveapi")]
        public OutputResult<object> SaveApiInfo([FromBody] UKWebApiInfo paramInfo = null) {
            try
            {
                if (paramInfo == null || paramInfo.FullPath == null || paramInfo.FullPath.Length ==  0) {
                    return ResponseError<object>("参数异常");
                }
                string[] subPaths = paramInfo.FullPath.Split("/", StringSplitOptions.RemoveEmptyEntries);
                string CurrentDir = System.IO.Directory.GetCurrentDirectory();
                CurrentDir = CurrentDir + Path.DirectorySeparatorChar + "apijson";
                if (Directory.Exists(CurrentDir) == false) {
                    Directory.CreateDirectory(CurrentDir);
                }
                for (int i = 0; i < subPaths.Length -1 ; i++)
                {
                    CurrentDir = CurrentDir + Path.DirectorySeparatorChar + subPaths[i];
                    if (Directory.Exists(CurrentDir) == false)
                    {
                        Directory.CreateDirectory(CurrentDir);
                    }
                }
                CurrentDir = CurrentDir + Path.DirectorySeparatorChar + subPaths[subPaths.Length - 1] +".json";
                StreamWriter wr = new StreamWriter(CurrentDir);
                wr.Write(JsonConvert.SerializeObject(paramInfo));
                wr.Close();
                return new OutputResult<object>("完成");
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }

        }

    }
}
