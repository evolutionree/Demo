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
}
