using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Rule;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class RuleController : BaseController
    {

        private readonly RuleTranslatorServices _ruleTranslatorServices;

        public RuleController(RuleTranslatorServices ruleTranslatorServices) : base(ruleTranslatorServices)
        {
            _ruleTranslatorServices = ruleTranslatorServices;
        }
        [HttpPost]
        [Route("queryrulemenu")]
        public OutputResult<object> EntityRuleMenuQuery([FromBody]MenuRuleModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _ruleTranslatorServices.EntityRuleMenuQuery(entityModel.EntityId, UserId);
        }


        [HttpPost]
        [Route("querymenuruleinfo")]
        public OutputResult<object> EntityRuleInfoQuery([FromBody]MenuRuleModel entityModel)
        {

            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _ruleTranslatorServices.MenuRuleInfoQuery(entityModel, UserId);
        }
        [HttpPost]
        [Route("disabledmenu")]
        public OutputResult<object> DisabledEntityRule([FromBody]MenuRuleModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _ruleTranslatorServices.DisabledEntityRule(entityModel.MenuId, UserId);
        }

        [HttpPost]
        [Route("saverule")]
        public OutputResult<object> SaveEntityRule([FromBody]RuleModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            //删除规则
            //if (entityModel.RuleItems.Count == 0) return ResponseError<object>("规则明细不能为空");
            //if (string.IsNullOrEmpty(entityModel.RuleSet.RuleSet)) return ResponseError<object>("规则集合不能为空");
            if (entityModel.MenuName_Lang == null) ResponseError<object>("请完善多语言");
            return _ruleTranslatorServices.SaveRule(entityModel, UserId);
        }

        [HttpPost]
        [Route("getrule")]
        public OutputResult<object> GetRule([FromBody]GetRuleInfoModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");

            return _ruleTranslatorServices.GetRule(entityModel, UserId);
        }
        [HttpPost("savemenuorderby")]
        public OutputResult<object> SaveMenuOrderBy([FromBody] List<EntityMenuOrderByModel> paramInfo) {
            if (paramInfo == null) {
                return ResponseError<object>("参数异常");
            }
            try
            {
                this._ruleTranslatorServices.SaveMenuOrder(paramInfo, UserId);
                return new OutputResult<object>("");
            }
            catch (Exception ex) {
                return ResponseError<object>(ex.Message);
            }
        }

        //[HttpPost]
        //[Route("saverolerule")]
        //public OutputResult<object> SaveRoleRule([FromBody]RoleRuleModel entityModel)
        //{
        //    if (entityModel == null) return ResponseError<object>("参数格式错误");
        //    return _ruleTranslatorServices.SaveRoleRule(entityModel, UserId);
        //}


        [HttpPost]
        [Route("queryroleruleinfo")]
        public OutputResult<object> RoleRuleInfoQuery([FromBody]RoleRuleModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _ruleTranslatorServices.RoleRuleInfoQuery(entityModel, UserId);
        }

        [HttpPost]
        [Route("querydynamicruleinfo")]
        public OutputResult<object> DynamicRuleInfoQuery([FromBody]DynamicRuleModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _ruleTranslatorServices.DynamicRuleInfoQuery(entityModel, UserId);
        }

        [HttpPost]
        [Route("queryflowruleinfo")]
        public OutputResult<object> WorkFlowRuleInfoQuery([FromBody]FlowRuleModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _ruleTranslatorServices.WorkFlowRuleInfoQuery(entityModel, UserId);
        }
    }
}
