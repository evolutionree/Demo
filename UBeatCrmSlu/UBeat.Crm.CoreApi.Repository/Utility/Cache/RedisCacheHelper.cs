
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBeat.Crm.CoreApi.Repository.Utility.Cache
{
    public class RedisCacheHelper : ICacheHelper
    {
        protected IDatabase _cache;

        //private ConnectionMultiplexer _connection;

        public static int POOL_SIZE = 100;
        private static readonly Object lockPookRoundRobin = new Object();
        private static Lazy<ConnectionMultiplexer>[] lazyConnection = null;

        private readonly string _instance;

        public RedisCacheHelper(RedisCacheOptions options, int database = 0)
        {
            InitConnectionPool(options);
            //实例ID.redis.rds.aliyuncs.com:6379,password=实例ID:密码
            //_connection = ConnectionMultiplexer.Connect(options.Configuration);
            //_connection.PreserveAsyncOrder = false;
            _cache = Connection.GetDatabase(database);
            _instance = options.InstanceName;
        }

        private static void InitConnectionPool(RedisCacheOptions options)
        {
            lock (lockPookRoundRobin)
            {
                if (lazyConnection == null)
                {
                    lazyConnection = new Lazy<ConnectionMultiplexer>[POOL_SIZE];
                    for (int i = 0; i < POOL_SIZE; i++)
                    {
                        if (lazyConnection[i] == null)
                            lazyConnection[i] = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options.Configuration));
                    }
                }
                
            }
        }
        private static ConnectionMultiplexer GetLeastLoadedConnection()
        {
            //choose the least loaded connection from the pool
            var minValue = lazyConnection.Min((lazyCtx) => lazyCtx.Value.GetCounters().TotalOutstanding);
            var lazyContext = lazyConnection.Where((lazyCtx) => lazyCtx.Value.GetCounters().TotalOutstanding == minValue).First();
            return lazyContext.Value;
        }

        public ConnectionMultiplexer Connection
        {
            get
            {
                lock (lockPookRoundRobin)
                {
                    return GetLeastLoadedConnection();
                }
            }
        }

        public string getRedisStatus()
        {
            if (Connection == null)
            {
                return "未初始化";
            }
            else if (Connection.IsConnected == false)
            {
                return "未连接";
            }
            else
            {
                return Connection.GetStatus();
            }
        }

        public string GetKeyForRedis(string key)
        {
            return _instance + key;
        }

        #region ---验证缓存项是否存在---

        /// <summary>
        /// 验证缓存项是否存在
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _cache.KeyExists(GetKeyForRedis(key));
        }

        /// <summary>
        /// 验证缓存项是否存在（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public Task<bool> ExistsAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _cache.KeyExistsAsync(GetKeyForRedis(key));
        }
        #endregion

        #region ---添加缓存---
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <returns></returns>
        public bool Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _cache.StringSet(GetKeyForRedis(key), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
        }
        /// <summary>
        /// 添加缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <returns></returns>
        public Task<bool> AddAsync(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _cache.StringSetAsync(GetKeyForRedis(key), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <param name="expiresSliding">滑动过期时长（如果在过期时间内有操作，则以当前时间点延长过期时间,Redis中无效）</param>
        /// <param name="expiressAbsoulte">绝对过期时长</param>
        /// <returns></returns>
        public bool Add(string key, object value, TimeSpan expiresSliding, TimeSpan expiressAbsoulte)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _cache.StringSet(GetKeyForRedis(key), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)), expiressAbsoulte);
        }
        /// <summary>
        /// 添加缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <param name="expiresSliding">滑动过期时长（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="expiressAbsoulte">绝对过期时长</param>
        /// <returns></returns>
        public Task<bool> AddAsync(string key, object value, TimeSpan expiresSliding, TimeSpan expiressAbsoulte)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _cache.StringSetAsync(GetKeyForRedis(key), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)), expiressAbsoulte);
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <param name="expiresIn">缓存时长</param>
        /// <param name="isSliding">是否滑动过期（如果在过期时间内有操作，则以当前时间点延长过期时间,Redis中无效）</param>
        /// <returns></returns>
        public bool Add(string key, object value, TimeSpan expiresIn, bool isSliding = false)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }


            return _cache.StringSet(GetKeyForRedis(key), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)), expiresIn);
        }

        /// <summary>
        /// 添加缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <param name="expiresIn">缓存时长</param>
        /// <param name="isSliding">是否滑动过期（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <returns></returns>
        public Task<bool> AddAsync(string key, object value, TimeSpan expiresIn, bool isSliding = false)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }


            return _cache.StringSetAsync(GetKeyForRedis(key), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)), expiresIn);
        }
        #endregion

        #region ---删除缓存---
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _cache.KeyDelete(GetKeyForRedis(key));
        }
        /// <summary>
        /// 删除缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public Task<bool> RemoveAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _cache.KeyDeleteAsync(GetKeyForRedis(key));
        }

        /// <summary>
        /// 批量删除缓存
        /// </summary>
        /// <param name="key">缓存Key集合</param>
        /// <returns></returns>
        public void RemoveAll(IEnumerable<string> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }
            List<RedisKey> keylist = new List<RedisKey>();
            foreach (var k in keys)
            {
                keylist.Add(GetKeyForRedis(k));
            }
            _cache.KeyDelete(keylist.ToArray());
        }

        /// <summary>
        /// 批量删除缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key集合</param>
        /// <returns></returns>
        public Task RemoveAllAsync(IEnumerable<string> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }
            List<RedisKey> keylist = new List<RedisKey>();
            foreach (var k in keys)
            {
                keylist.Add(GetKeyForRedis(k));
            }
            return _cache.KeyDeleteAsync(keylist.ToArray());
        }
        #endregion

        #region ---获取缓存---
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public T Get<T>(string key) where T : class
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var value = _cache.StringGet(GetKeyForRedis(key));

            if (!value.HasValue)
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(value);
        }
        /// <summary>
        /// 获取缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public Task<T> GetAsync<T>(string key) where T : class
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var valuetask = _cache.StringGetAsync(GetKeyForRedis(key));
            valuetask.Wait();
            return Task.Run<T>(() =>
            {
                if (!valuetask.Result.HasValue)
                {
                    return default(T);
                }
                return JsonConvert.DeserializeObject<T>(valuetask.Result);
            });

        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public object Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var value = _cache.StringGet(GetKeyForRedis(key));

            if (!value.HasValue)
            {
                return null;
            }
            return JsonConvert.DeserializeObject(value);

        }
        /// <summary>
        /// 获取缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public Task<object> GetAsync(string key)
        {
            return GetAsync<object>(key);
        }


        /// <summary>
        /// 获取缓存集合
        /// </summary>
        /// <param name="keys">缓存Key集合</param>
        /// <returns></returns>
        public IDictionary<string, object> GetAll(IEnumerable<string> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }
            var dict = new Dictionary<string, object>();

            keys.ToList().ForEach(item => dict.Add(item, Get(item)));

            return dict;
        }
        /// <summary>
        /// 获取缓存集合（异步方式）
        /// </summary>
        /// <param name="keys">缓存Key集合</param>
        /// <returns></returns>
        public Task<IDictionary<string, object>> GetAllAsync(IEnumerable<string> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }
            return Task.Run<IDictionary<string, object>>(() =>
             {
                 var dict = new Dictionary<string, object>();
                 keys.ToList().ForEach(item => dict.Add(item, Get(item)));
                 return dict;
             });
        }


        #endregion

        #region ---修改缓存---
        /// <summary>
        /// 修改缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">新的缓存Value</param>
        /// <returns></returns>
        public bool Replace(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //if (Exists(key))
            //    if (!Remove(key))
            //        return false;

            return Add(key, value);

        }
        /// <summary>
        /// 修改缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">新的缓存Value</param>
        /// <returns></returns>
        public Task<bool> ReplaceAsync(string key, object value)
        {
            return Task.Run<bool>(() =>
            {
                return Replace(key, value);
            });
        }

        /// <summary>
        /// 修改缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">新的缓存Value</param>
        /// <param name="expiresSliding">滑动过期时长（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="expiressAbsoulte">绝对过期时长</param>
        /// <returns></returns>
        public bool Replace(string key, object value, TimeSpan expiresSliding, TimeSpan expiressAbsoulte)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //if (Exists(key))
            //    if (!Remove(key))
            //        return false;

            return Add(key, value, expiresSliding, expiressAbsoulte);
        }
        /// <summary>
        /// 修改缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">新的缓存Value</param>
        /// <param name="expiresSliding">滑动过期时长（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="expiressAbsoulte">绝对过期时长</param>
        /// <returns></returns>
        public Task<bool> ReplaceAsync(string key, object value, TimeSpan expiresSliding, TimeSpan expiressAbsoulte)
        {
            return Task.Run(() => { return Replace(key, value, expiresSliding, expiressAbsoulte); });
        }

        /// <summary>
        /// 修改缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">新的缓存Value</param>
        /// <param name="expiresIn">缓存时长</param>
        /// <param name="isSliding">是否滑动过期（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <returns></returns>
        public bool Replace(string key, object value, TimeSpan expiresIn, bool isSliding = false)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            //if (Exists(key))
            //    if (!Remove(key)) return false;

            return Add(key, value, expiresIn, isSliding);
        }
        /// <summary>
        /// 修改缓存（异步方式）
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">新的缓存Value</param>
        /// <param name="expiresIn">缓存时长</param>
        /// <param name="isSliding">是否滑动过期（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <returns></returns>
        public Task<bool> ReplaceAsync(string key, object value, TimeSpan expiresIn, bool isSliding = false)
        {
            return Task.Run(() => { return Replace(key, value, expiresIn, isSliding); });
        }

        #endregion


        public void Dispose()
        {
            for (int i = 0; i < POOL_SIZE; i++)
            {

                if (lazyConnection[i] != null && lazyConnection[i].IsValueCreated)
                    lazyConnection[i].Value.Dispose();
            }


            GC.SuppressFinalize(this);
        }
    }
}
