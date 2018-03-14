using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.IRepository;
using UBeat.Crm.CoreApi.Repository.Repository.Cache;
using UBeat.Crm.CoreApi.Repository.Utility.Cache;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class CacheServices
    {
        //private readonly ICacheRepository _cacheRepository;
        RedisCacheOptions _option;

        public CacheServices()
        {
            IConfigurationRoot config = ServiceLocator.Current.GetInstance<IConfigurationRoot>();
            var redis = config.GetSection("Redis");
            _option = new RedisCacheOptions();
            _option.Configuration = redis.GetValue<string>("configuration");
            _option.InstanceName = redis.GetValue<string>("instance");

        }

        public ICacheRepository Repository
        {
            get {
                return CacheRepository.GetInstance(_option);
            }
            
        }
        /// <summary>
        /// 获取Redis服务状态
        /// </summary>
        /// <returns></returns>
        public string RedisServerStatus() {
            return ((CacheRepository)Repository).getRedisStatus();
        }
        
        public IMemoryCache MemoryCache { get; } = new MemoryCache(new MemoryCacheOptions());
    }
}
