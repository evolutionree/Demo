using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.DomainModel.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Services;
namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class EntityTransferController : BaseController
    {
        private EntityTransferServices _entityTransferServices;
        public EntityTransferController(EntityTransferServices entityTransferServices) :base(new BaseServices[] { entityTransferServices }){
            _entityTransferServices = entityTransferServices;
        }
        #region 获取转换规则列表 ,如果SrcEntityID，SrcCategoryID,DstEntityId,DstCategoryId,SrcID(暂不支持)
        [HttpPost]
        [Route("queryrules")]
        public OutputResult<object> QueryRules([FromBody]EntityTransferRuleQueryModel ruleQuery= null)
        {
            if (ruleQuery == null) {
                return ResponseError<object>("参数异常");
            }
            if (ruleQuery.SrcEntityId == null || ruleQuery.SrcEntityId.Length == 0 || ruleQuery.SrcEntityId == Guid.Empty.ToString()) {
                return ResponseError<object>("参数异常：SrcEntityId");
            }
            List<EntityTransferRuleInfo> rules = _entityTransferServices.queryRules(ruleQuery, UserId);
            foreach (EntityTransferRuleInfo info in rules) {
                info.MapperSetting = null;
                info.TransferJson = "";
            }
            return new OutputResult<object>(rules);
        }
        #endregion
        #region 转换单据
        [HttpPost]
        [Route("trnasfer")]
        public OutputResult<object> TransferEntityBill([FromBody]EntityTransferActionModel ruleQuery = null) {

            AnalyseHeader header = GetAnalyseHeader();
            try
            {

                object obj = _entityTransferServices.TransferBill( header, ruleQuery, UserId, LoginUser.UserName);
                return new OutputResult<object>(obj); ;
            }
            catch (Exception ex) {
                if (ex.InnerException != null)
                {
                    return new OutputResult<object>(null, ex.InnerException.Message, -1);
                }
                else {
                    return new OutputResult<object>(null, ex.Message, -1);
                }
            }
        }
        #endregion

    }
}
