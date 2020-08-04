using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Contact;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/zj/[controller]")]
    public class BusinessCardController : BaseController
    {
        private readonly ZjBusinessCardServices _businessCardServices;

        public BusinessCardController(ZjBusinessCardServices businessCardServices) : base(businessCardServices)
        {
            _businessCardServices = businessCardServices;
        }

        [HttpPost]
        [Route("getcardinfo")]
        [AllowAnonymous]
        public OutputResult<object> GetCardInfo([FromBody] ContactVCardModel vcardModel = null)
        {
           if (vcardModel == null) return ResponseError<object>("参数格式错误");
            return _businessCardServices.GetBusinessCardInfo(vcardModel, UserId);
        }
        [HttpPost]
        [Route("getcardinfocontact")]
        [AllowAnonymous]
        public OutputResult<object> GetCardInfoContact([FromBody] ContactVCardModel vcardModel = null)
        {
            if (vcardModel == null) return ResponseError<object>("参数格式错误");
            return _businessCardServices.GetBusinessCardInfoContact(vcardModel, UserId);
        }
    }
}
