using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Models.DynamicEntity;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Controllers;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class RedisTest1Controller : DynamicEntityController
    {
        private readonly ICacheRepository _cacheRepository;
        private readonly DynamicEntityServices _dynamicEntityServices;

        public RedisTest1Controller(CacheServices cacheService, DynamicEntityServices dynamicEntityServices) :base(dynamicEntityServices)
        {
            _cacheRepository = cacheService.Repository;
            _dynamicEntityServices = dynamicEntityServices;
        }


        [HttpPost]
        [Route("add")]
        public OutputResult<object> Add([FromBody] DynamicEntityAddModel dynamicModel = null)
        {

            //do pre action
            OutputResult<object> result = base.Add(dynamicModel);
            //do post action
            return result;
        }


        [HttpPost]
        [Route("add2")]
        public OutputResult<object> Add1()
        {
            return null;
        }

    }
    
}