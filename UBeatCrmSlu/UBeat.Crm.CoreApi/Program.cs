using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog;
using Quartz;
using Quartz.Impl;
using UBeat.Crm.CoreApi.Services.Utility;
using UBeat.Crm.CoreApi.Utility;

namespace UBeat.Crm.CoreApi
{
    public class Program
    {
        private static IScheduler _scheduler; 
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Custom Host Config
            var config = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("hosting.json")
              .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseConfiguration(config) 
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            ServerFingerPrintUtils.getInstance().checkAndAddFingerPrint();//检查服务器指纹
            StartScheduler();//启动主Scheler

            host.Run();
        }

        private static void StartScheduler()
        {
            var config = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json")
              .Build();

            int isStart = 0;
            IEnumerator<IConfigurationSection> it = config.GetSection("ScheduleSetting").GetChildren().GetEnumerator();
            while (it.MoveNext())
            {
                IConfigurationSection item = it.Current;
                if (item.Key.Equals("IsStart") && item.Value != null)
                {
                    int tmp = 0;
                    if (Int32.TryParse(item.Value, out tmp))
                    {
                        isStart = tmp;
                    }
                }
            }
            var properties = new NameValueCollection
            {
            };

            var schedulerFactory = new StdSchedulerFactory(properties);
            _scheduler = schedulerFactory.GetScheduler().Result;
            _scheduler.Start().Wait();
            var heartBeatJob = JobBuilder.Create<HeartBeatWithRedisJob>()
                .WithIdentity("heartBeatJob")
                .Build();
            var heartBeatCron = TriggerBuilder.Create()
                .WithIdentity("heartBeatCron")
                .StartNow()
                .WithCronSchedule("0/30 * * * * ?")
                .Build();
            _scheduler.ScheduleJob(heartBeatJob, heartBeatCron);
            if (isStart != 1)
            {
                Logger logger = LogManager.GetLogger("UBeat.Qrtz");
                logger.Warn("!!!!!未配置后台事务启动，后台事务将不在本服务器内运行!!!!!!");
                return;
            }
            
            var mainScheduleJob = JobBuilder.Create<MainSchedulerJobImp>()
                .WithIdentity("MainScheduleJob")
                .Build();
            var mainScheduleTrigger = TriggerBuilder.Create()
                .WithIdentity("MainScheduleCron")
                .StartNow()
                .WithCronSchedule("* * * * * ?")
                .Build();
            _scheduler.ScheduleJob(mainScheduleJob, mainScheduleTrigger);
            //每秒调用一次


        }
    }
}
