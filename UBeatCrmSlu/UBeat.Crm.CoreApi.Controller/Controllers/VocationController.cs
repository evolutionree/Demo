using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models.Vocation;
using UBeat.Crm.CoreApi.Services.Models;

namespace UBeat.Crm.CoreApi.Controllers
{

    [Route("api/[controller]")]
    public class VocationController : BaseController
    {
    
        private readonly VocationServices _service;
        private readonly RuleTranslatorServices _ruleService;

        public VocationController(VocationServices service, RuleTranslatorServices ruleService) : base(ruleService)
        {
            _service = service;
            _ruleService = ruleService;

        }

        #region 职能

        /// <summary>
        /// 保存职能
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("savevocation")]
        public OutputResult<object> SaveVocation([FromBody] VocationSaveModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            if (body.VocationId.HasValue)
            {
                return _service.EditVocation(body, UserId);

            }
            else
            {
                return _service.AddVocation(body, UserId);
            }
        }

        /// <summary>
        /// 保存职能
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("copyvocation")]
        public OutputResult<object> CopyVocation([FromBody] CopyVocationSaveModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
                return _service.AddCopyVocation(body, UserId);
 
        }


        /// <summary>
        /// 删除职能
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("deletevocation")]
        public OutputResult<object> DeleteVocation([FromBody] VocationDeleteModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.DeleteVocation(body, UserId);

        }

        /// <summary>
        /// 获取职能列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getvocations")]
        public OutputResult<object> GetVocations([FromBody] VocationSelectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetVocations(body, UserId);

        }


        #endregion

        #region 功能


        /// <summary>
        /// 根据职能id,获取功能列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getfunctions")]
        public OutputResult<object> GetFunctionsByVocationId([FromBody] VocationFunctionSelectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetFunctionsByVocationId(body, UserId);

        }




        /// <summary>
        /// 编辑职能下的功能
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("editfunctions")]
        public OutputResult<object> EditVocationFunctions([FromBody] VocationFunctionEditModel body)
        {
           
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.EditVocationFunctions(body, UserId);

        }


        /// <summary>
        /// 保存功能下的规则
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("saverule")]
        public OutputResult<object> SaveFunctionRule([FromBody] FunctionRuleAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _ruleService.SaveRuleForVocation(body, UserId);
        }

        /// <summary>
        /// 保存功能下的规则
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("savefuncrule")]
        public OutputResult<object> SaveFuncRule([FromBody] FuncRuleAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _ruleService.SaveRuleForFunction(body, UserId);
        }


        /// <summary>
        /// 获取功能下的规则
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getrule")]
        public OutputResult<object> GetFunctionRule([FromBody] FunctionRuleSelectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetFunctionRule(body, UserId);
        }



        #endregion

        #region 用户

       

        /// <summary>
        /// 获取职能下的用户
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getuser")]
        public OutputResult<object> GetVocationUser([FromBody] VocationUserSelectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetVocationUser(body, UserId);
        }




        /// <summary>
        /// 删除职能下的用户
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("deleteuser")]
        public OutputResult<object> DeleteVocationUser([FromBody] VocationUserDeleteModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.DeleteVocationUser(body, UserId);
        }




        /// <summary>
        /// 根据用户的职能，获取某个用户可用的功能列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getuserfunction")]
        public OutputResult<object> GetUserFunctions([FromBody] UserFunctionSelectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetUserFunctions(body, UserId);
        }

        /// <summary>
        /// 获取用户的所有职能拥有的功能
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getuserfunctionlist")]
        public OutputResult<object> GetUserFunctionList([FromBody] UserFunctionModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetUserFunctions(body, UserId);
        }

        #endregion

        #region 功能树管理

        /// <summary>
        /// 保存功能
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("savefunction")]
        public OutputResult<object> SaveFunction([FromBody] FunctionAddModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.AddFunction(body, UserId);
        }




        /// <summary>
        /// 删除职能
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("deletefunction")]
        public OutputResult<object> DeleteFunction([FromBody] FunctionItemDeleteModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.DeleteFunction(body, UserId);

        }




        /// <summary>
        /// 获取职能列表
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        [HttpPost("getfunctiontree")]
        public OutputResult<object> GetFunctionTree([FromBody] FunctionTreeSelectModel body)
        {
            if (body == null) return ResponseError<object>("参数格式错误");
            return _service.GetFunctionTree(body, UserId);

        }


        #endregion




    }
}
