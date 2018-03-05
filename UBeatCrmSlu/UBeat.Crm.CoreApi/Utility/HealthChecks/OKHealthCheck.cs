using App.Metrics.Health;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UBeat.Crm.CoreApi.Services.Services;

namespace UBeat.Crm.CoreApi.Utility.HealthChecks
{
    public class PostgresHealthCheck : HealthCheck
    {
        HealthCheckServices _services;
        public PostgresHealthCheck(HealthCheckServices services) : base("数据库Postgres健康检查")
        {
            _services = services;
        }

        protected override Task<HealthCheckResult> CheckAsync(CancellationToken token = default(CancellationToken))
        {
            Task<HealthCheckResult> task = new Task<HealthCheckResult>(() =>
            {
                HealthCheckResult result = HealthCheckResult.Healthy();
                var time = _services.CheckPgsqlConnected();
                if (time == -1)
                {
                    result = HealthCheckResult.Unhealthy("数据库连接失败");
                }
                else if (time > 3000)
                {
                    result = HealthCheckResult.Degraded("数据库连接耗时较长");
                }
                return result;
            });
            task.Start();
            //返回正常的信息
            return task;
        }
    }


    public class RedisHealthCheck : HealthCheck
    {
        HealthCheckServices _services;
        public RedisHealthCheck(HealthCheckServices services) : base("Redis健康检查")
        {
            _services = services;
        }

        protected override Task<HealthCheckResult> CheckAsync(CancellationToken token = default(CancellationToken))
        {
            Task<HealthCheckResult> task = new Task<HealthCheckResult>(() =>
            {
                HealthCheckResult result = HealthCheckResult.Healthy();
                var time = _services.CheckRedisConnected();
                if (time == -1)
                {
                    result = HealthCheckResult.Unhealthy("数据库连接失败");
                }
                else if (time > 3000)
                {
                    result = HealthCheckResult.Degraded("数据库连接耗时较长");
                }
                return result;
            });
            task.Start();
            //返回正常的信息
            return task;
        }
    }
    /// <summary>
    /// 检查Redis队列情况
    /// </summary>
    public class RedisQueueHealthCheck : HealthCheck
    {
        private static int ErrorQU = 10;
        private static int WarnQU = 5;
        private CacheServices _cacheServices;

        public RedisQueueHealthCheck(CacheServices cacheServices) :base("Redis队列检查"){
            this._cacheServices = cacheServices;
        }
    
        protected override Task<HealthCheckResult> CheckAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            Task<HealthCheckResult> task = new Task<HealthCheckResult>(() =>
            {
                string statusString = _cacheServices.RedisServerStatus();
                string tmpStatus = "";
                if (statusString == null || statusString.Length == 0) {
                    return HealthCheckResult.Unhealthy("Redis队列检查异常");
                }
                tmpStatus = statusString;
                int thisMaxQu = 0;
                for (int i = 0; i < 10; i++) {//最多检查十个队列
                    int index = tmpStatus.IndexOf(", qu=");
                    if (index < 0) break;
                    tmpStatus = tmpStatus.Substring(index + ", qu=".Length);
                    int index2 = tmpStatus.IndexOf(",");
                    if (index2 < 0) break;
                    string tmp = tmpStatus.Substring(0, index2);
                    tmpStatus = tmpStatus.Substring(index2);
                    int tmpqu = 0;
                    if (Int32.TryParse(tmp, out tmpqu)) {
                        if (thisMaxQu < tmpqu) thisMaxQu = tmpqu;
                    }
                }
                if (thisMaxQu >= ErrorQU) return HealthCheckResult.Unhealthy(statusString);
                else if (thisMaxQu >= WarnQU) return HealthCheckResult.Degraded(statusString);
                else return HealthCheckResult.Healthy();
                
            });
            task.Start();
            //返回正常的信息
            return task;
        }
    }
}
