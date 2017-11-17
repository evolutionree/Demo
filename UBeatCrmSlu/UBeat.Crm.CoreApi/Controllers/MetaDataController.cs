using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.MetaData;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class MetaDataController : BaseController
    {
        MetaDataServices _service = null;

        public MetaDataController(MetaDataServices service) : base(service)
        {
            _service = service;
        }




        [HttpPost("getincrementdata")]
        public OutputResult<object> GetIncrementData([FromBody] List<IncrementDataModel> bodyData)
        {
            if (bodyData == null)
                return ResponseError<object>("参数格式错误");

            return _service.GetIncrementData(bodyData,LoginUser.UserId);
        }


        /// <summary>
        /// 清理非登录信息的缓存数据
        /// </summary>
        /// <returns></returns>
        [HttpPost("removecaches")]
        public OutputResult<object> RemoveCaches()
        {
            return _service.RemoveCaches(LoginUser.UserId);
        }


    }
}