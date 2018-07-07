using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.SalesLead;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class SalesLeadController : BaseController
    {
        private readonly SalesLeadServices _salesLeadServices;

        public SalesLeadController(SalesLeadServices salesLeadServices) : base(salesLeadServices)
        {
            _salesLeadServices = salesLeadServices;
        }

        [HttpPost]
        [Route("changesaleslead")]
        public OutputResult<object> DisabledEntityRule([FromBody]SalesLeadModel entityModel)
        {
            if (entityModel == null) return ResponseError<object>("参数格式错误");
            return _salesLeadServices.ChangeSalesLeadToCustomer(entityModel, UserId);
        }
    }
}
