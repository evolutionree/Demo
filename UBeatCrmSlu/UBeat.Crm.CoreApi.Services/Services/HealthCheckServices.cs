using System;
using System.Collections.Generic;
using System.Text;

namespace UBeat.Crm.CoreApi.Services.Services
{
    public class HealthCheckServices : BaseServices
    {
        CacheServices _cacheService;
        public HealthCheckServices(CacheServices cacheService)
        {
            _cacheService = cacheService;
        }

        public int CheckPgsqlConnected()
        {
            DateTime dt1 = System.DateTime.Now;
            using (var conn = GetDbConnect())
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "数据库执行连接失败");
                    return -1;
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }

            DateTime dt2 = System.DateTime.Now;
            return (int)((dt2 - dt2).TotalMilliseconds);
        }
        public int CheckRedisConnected()
        {
            DateTime dt1 = System.DateTime.Now;
            try
            {
                if (!_cacheService.Repository.IsConnected)
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Redis连接失败");
                return -1;
            }
            

            DateTime dt2 = System.DateTime.Now;
            return (int)((dt2 - dt2).TotalMilliseconds);
        }

    }
}
