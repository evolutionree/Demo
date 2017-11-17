using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Controllers
{
    /// <summary>
    /// 这个controller主要用于客户端直接调用存储过程的方法
    /// UK100的标准产品尽量不要调用此方法来处理业务逻辑，
    /// 建议使用编写单独的controller方法来实现逻辑。
    /// 这里的方法一般仅用于项目中，由于产品不符合客户的要求，需要扩展服务的方法。
    /// </summary>
    [Route("api/[controller]")]
    public class UKExtEngineController: BaseController
    {
        [HttpPost]
        [Route("callfunction/{functionname}")]
        public OutputResult<object> CallFunction([FromRoute] string functionname,[FromForm] Dictionary<string,object> paramInfo) {
            return null;
        }
    }
}
