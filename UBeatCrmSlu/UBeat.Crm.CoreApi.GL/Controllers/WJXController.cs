using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Models.WJXModel;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{

    [Route("api/zj/[controller]")]
    public class WJXController : BaseController
    {
        private readonly WJXServices _wjxServices;
        private readonly DynamicEntityServices _dynamicEntityServices;

        public WJXController(DynamicEntityServices dynamicEntityServices, WJXServices wjxServices)
        {
            _dynamicEntityServices = dynamicEntityServices;
            _wjxServices = wjxServices;
        }

        [Route("getwjxquestionlist")]
        [HttpPost]
        public OutputResult<object> GetWJXQuestionList([FromBody]  UKExtExecuteFunctionModel paramInfo)
        {
            if (paramInfo == null || paramInfo.RecIds.Length == 0)
                return ResponseError<object>("参数格式错误");
            var recId = Guid.Parse(paramInfo.RecIds[0]);
            var entityId = paramInfo.EntityId;
            return _wjxServices.GetWJXQuestionList(recId, entityId);
        }
        [Route("saveanswer")]
        [HttpPost]
        [AllowAnonymous]
        public void SaveWXJAnswer([FromBody]  Dictionary<string, object> paramInfo)
        {
            _wjxServices.SaveWXJAnswer(paramInfo, UserId);
        }
        [Route("getanswerlists")]
        [HttpPost]
        public OutputResult<object> GetWXJAnswerList([FromBody]  WJXCustParam custParam)
        {
            if (custParam == null)
                return ResponseError<object>("参数格式错误");
            return _wjxServices.GetWXJAnswerList(custParam, UserId);
        }

    }
}
