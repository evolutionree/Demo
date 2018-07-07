using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UBeat.Crm.CoreApi.Services.Models;
using UBeat.Crm.CoreApi.Services.Models.Account;
using UBeat.Crm.CoreApi.Services.Services;
using UBeat.Crm.CoreApi.IRepository;

namespace UBeat.Crm.CoreApi.Controllers
{
    [Produces("application/json")]
    [Route("api/RedisTest")]
    public class RedisTestController : BaseController
    {
        private readonly ICacheRepository _cacheRepository;

        public RedisTestController(CacheServices cacheService)
        {
            _cacheRepository = cacheService.Repository;
        }


        [HttpPost("Exists")]
        public OutputResult<object> Exists([FromBody] AddModel param)
        {
            var data = _cacheRepository.Exists("RedisTest");
            return new OutputResult<object>(data);
        }
        [HttpPost("Add")]
        public OutputResult<object> Add([FromBody] AddModel param)
        {
            var data = _cacheRepository.Add("RedisTest", param);
            return new OutputResult<object>(data);
        }
        [HttpPost("Add2")]
        public OutputResult<object> Add2([FromBody] AddModel param)
        {
            var data = _cacheRepository.Add(param.Key, param, new TimeSpan(0, 0, 30), param.IsSliding);
            return new OutputResult<object>(data);
        }

        [HttpPost("Remove")]
        public OutputResult<object> Remove([FromBody] AddModel param)
        {
            var data = _cacheRepository.Remove(param.Key);
            return new OutputResult<object>(data);
        }

        [HttpPost("RemoveAll")]
        public OutputResult<object> RemoveAll([FromBody] IEnumerable<string> keys)
        {
            _cacheRepository.RemoveAll(keys);
            return new OutputResult<object>("OK");
        }
        [HttpPost("Get")]
        public OutputResult<object> Get()
        {
            return new OutputResult<object>(_cacheRepository.Get("RedisTest"));
        }

        [HttpPost("GetAll")]
        public OutputResult<object> GetAll([FromBody] IEnumerable<string> keys)
        {
            return new OutputResult<object>(_cacheRepository.GetAll(keys));
        }

        [HttpPost("Replace")]
        public OutputResult<object> Replace([FromBody]AddModel param)
        {
            return new OutputResult<object>(_cacheRepository.Replace(param.Key, param.Value));
        }

        [HttpPost("Replace2")]
        public OutputResult<object> Replace2([FromBody]AddModel param)
        {
            return new OutputResult<object>(_cacheRepository.Replace(param.Key, param.Value,new TimeSpan(0,0,30),param.IsSliding));
        }
    }


    public class AddModel
    {
        string key;
        object value;
        bool isSliding = false;

        public string Key { get => key; set => key = value; }
        public object Value { get => value; set => this.value = value; }
        public bool IsSliding { get => isSliding; set => isSliding = value; }
    }
}