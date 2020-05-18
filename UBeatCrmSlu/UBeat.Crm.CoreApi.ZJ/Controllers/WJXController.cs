﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Controllers;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.ZJ.Controllers
{

    [Route("api/zj/[controller]")]
    public class WJXController: BaseController
    {
        private readonly WJXServices _wjxServices;
        private readonly DynamicEntityServices _dynamicEntityServices;

        public WJXController(DynamicEntityServices dynamicEntityServices, WJXServices wjxServices){
            _dynamicEntityServices = dynamicEntityServices;
            _wjxServices = wjxServices;
        }

        [Route("getwjxquestionlist")]
        [HttpPost]
        public OutputResult<object> GetWJXQuestionList([FromBody]  DynamicEntityListModel dynamicModel = null) {
            return _wjxServices.GetWJXQuestionList();
        }
    }
}
