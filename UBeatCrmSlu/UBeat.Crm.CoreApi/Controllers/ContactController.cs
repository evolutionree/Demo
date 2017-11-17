using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Contact;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Route("api/[controller]")]
    public class ContactController : BaseController
    {
        private readonly ContactServices _contactServices;

        public ContactController(ContactServices contactServices) : base(contactServices)
        {
            _contactServices = contactServices;
        }

        [HttpPost]
        [Route("vcard")]
        public OutputResult<object> VCardInfo([FromBody] ContactVCardModel vcardModel = null)
        {
            if (vcardModel == null) return ResponseError<object>("参数格式错误");
            return _contactServices.VCardInfo(vcardModel, UserId);
        }
    }
}
