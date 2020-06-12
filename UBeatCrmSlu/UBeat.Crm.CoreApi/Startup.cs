using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using UBeat.Crm.CoreApi.Utility;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using App.Metrics.Reporting.Interfaces;
using App.Metrics.Extensions.Reporting.InfluxDB;
using App.Metrics.Extensions.Reporting.InfluxDB.Client;
using App.Metrics;
using UBeat.Crm.CoreApi.Models;
using UBeat.Crm.CoreApi.Services.webchat;
using UBeat.Crm.CoreApi.Core;
using UBeat.Crm.CoreApi.Core.Utility;

namespace UBeat.Crm.CoreApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddJsonFile("RoutePathSetting.json", false, true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            env.ConfigureNLog("NLog.config");

            Configuration = builder.Build();
            HostingEnvironment = env;
        }

        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public IContainer Container { get; set; }



        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ConfigureMetrics(services);
            //Add Json Web Token Auth
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o => JwtAuth.GetJwtOptions(o));

            //添加本地缓存
            services.AddMemoryCache();

            // Add framework services.
            IMvcBuilder mvcBuilder = services.AddMvc();
            LoadCustomerProjectAssembly(mvcBuilder);
            //这里开始处理项目扩展
            mvcBuilder.AddControllersAsServices().AddJsonOptions(options =>
            {
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                options.SerializerSettings.ContractResolver = new LowerCasePropertyNamesContractResolver();
                options.SerializerSettings.Converters.Add(new JsonCodeConverter(typeof(string)));
            });
            services.AddSingleton(Configuration);
            services.AddSingleton(HostingEnvironment);

            //needed for NLog.Web
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // AutoFac Regist Inject
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);
            // Regists Dependency

            // CoreApiRegistEngine.RegisterServices(containerBuilder, "UBeat.Crm.CoreApi.Services", "Services");
            //  CoreApiRegistEngine.RegisterImplemented(containerBuilder, "UBeat.Crm.CoreApi.Repository", "Repository");
            #region 新的装载程序
            dynamic type = this.GetType();
            string currentDirectory = Path.GetDirectoryName(type.Assembly.Location);
            System.IO.DirectoryInfo dir = new DirectoryInfo(currentDirectory);
            FileInfo [] files = dir.GetFiles("UBeat.Crm.CoreApi.*.dll");
            foreach (FileInfo f in files) {
                string assemblename = f.Name.Substring(0, f.Name.Length - 4);
                CoreApiRegistEngine.RegisterServices(containerBuilder, assemblename, "Services");
                CoreApiRegistEngine.RegisterImplemented(containerBuilder, assemblename, "Repository");
            }
            #endregion
            LoadCustomerProjectAssemblyForService(containerBuilder);
            Container = containerBuilder.Build();

            // Service Locator
            var locatorProvider = new CoreApiServiceLocator(Container);
            ServiceLocator.SetLocatorProvider(locatorProvider);
            //启动初始化应用数据
            StartupHelper.InitApplicationData();

            return new AutofacServiceProvider(Container);
        }



        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime lifetime)
        {


            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            //add NLog to ASP.NET Core
            loggerFactory.AddNLog();
            //add NLog.Web
            app.AddNLogWeb();


            app.UseGlobalErrors(env, loggerFactory);
            var appMetrics = Configuration.GetSection("AppMetrics").Get<AppMetricsModel>();
            if (appMetrics != null && appMetrics.IsEnable == 1)
            {
                app.UseMetrics();
                app.UseMetricsReporting(lifetime);
            }
            //Add Json Web Token Auth
            //app.UseJwtBearerAuthentication(JwtAuth.GetJwtOptions());


            //Add Cross Domain
            app.UseCors(policy =>
            {
                policy.AllowAnyOrigin();
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseMiddleware<WebChatHandler>();
            app.UseMvc();
        }
        /// <summary>
        /// 配置监控程序Metric
        /// </summary>
        /// <param name="services"></param>
        private void ConfigureMetrics(IServiceCollection services)
        {
            var appMetrics = Configuration.GetSection("AppMetrics").Get<AppMetricsModel>();
            if (appMetrics == null|| appMetrics.IsEnable!=1)
                return;
            services.AddMetrics(options =>
            {
                options.GlobalTags.Add("app", appMetrics.AppName);
                options.GlobalTags.Add("env", appMetrics.EnvName);
            }).AddHealthChecks(
                factory => {
                    string phyMemoryHealthCheckName=null;
                    if(appMetrics.PhysicalMemoryHealthCheck>=1024L*1024*1024)
                    {
                        phyMemoryHealthCheckName = (double)appMetrics.PhysicalMemoryHealthCheck / (1024L * 1024 * 1024 ) +"GB";
                    }
                    else if (appMetrics.PhysicalMemoryHealthCheck >= 1024L * 1024 )
                    {
                        phyMemoryHealthCheckName = (double)appMetrics.PhysicalMemoryHealthCheck / (1024L * 1024 ) + "MB";
                    }
                    else
                    {
                        phyMemoryHealthCheckName = (double)appMetrics.PhysicalMemoryHealthCheck / (1024L * 1024) + "KB";
                    }
                    factory.RegisterProcessPhysicalMemoryHealthCheck(string.Format("占用内存是否超过阀值({0})", phyMemoryHealthCheckName), appMetrics.PhysicalMemoryHealthCheck);
                    
                }
                ).AddReporting(
                factory =>
                {
                    factory.AddInfluxDb(
                        new InfluxDBReporterSettings
                        {
                            InfluxDbSettings = new InfluxDBSettings(appMetrics.DataBase, new Uri(appMetrics.Uri))
                            {
                                 UserName= appMetrics.DbUserName,
                                 Password= appMetrics.DbPassword
                            },
                            ReportInterval = TimeSpan.FromSeconds(5)
                        });
                }).AddMetricsMiddleware(options => options.IgnoredHttpStatusCodes = new[] { 404 });

        }


        /// <summary>
        /// 装载第三方（插件）的Controller
        /// </summary>
        /// <param name="mvcBuilder"></param>
        public void LoadCustomerProjectAssembly(IMvcBuilder mvcBuilder)
        {
            var config = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("plugin.json")
              .Build();
            List<string> plugins = new List<string>();
            IEnumerator<IConfigurationSection> it = config.GetSection("PluginProjectList").GetChildren().GetEnumerator();
            while (it.MoveNext())
            {
                IConfigurationSection item = it.Current;
                if (item.Value != null && item.Value.Length != 0)
                {
                    plugins.Add(item.Value);
                }
            }
            dynamic type = this.GetType();
            string currentDirectory = Path.GetDirectoryName(type.Assembly.Location);
            DirectoryInfo d = new DirectoryInfo(currentDirectory);

            string filesplite = "\\";
            string env = config.GetSection("Environment").Value;
            if (env == null) env = "";
            if (env.ToUpper().Equals("DEBUG"))
            {
                if (!(d != null
                    && d.Parent != null
                    && d.Parent.Parent != null
                    && d.Parent.Parent.Parent != null
                    && d.Parent.Parent.Parent.Parent != null))
                {
                    return;
                }
                currentDirectory = d.Parent.Parent.Parent.Parent.FullName;
                foreach (string item in plugins)
                {
                    string filePath = "";
                    if (item.EndsWith(".dll"))
                    {
                        filePath = item;
                    }
                    else
                    {
                        filePath = string.Format("{0}{1}{2}{1}obj{1}Debug{1}netcoreapp1.1{1}{2}.dll", currentDirectory, filesplite, item);
                    }
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists == false) continue;
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(fileInfo.FullName);
                    mvcBuilder.AddApplicationPart(assembly);
                }
            }
            else
            {
                filesplite = "/";
                foreach (string item in plugins)
                {
                    string filePath = string.Format("{0}{1}{2}.dll", currentDirectory, filesplite, item);
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (fileInfo.Exists == false) continue;
                    Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(fileInfo.FullName);
                    mvcBuilder.AddApplicationPart(assembly);
                }
            }
        }
        /// <summary>
        /// 装载并初始化第三方（插件）的Services和Repository
        /// </summary>
        /// <param name="containerBuilder"></param>
        public void LoadCustomerProjectAssemblyForService(ContainerBuilder containerBuilder)
        {
            foreach (string filePath in PlugInsUtils.getInstance().PlugInFiles)
            {
                Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(filePath);
                string endWithName = "Services";
                containerBuilder.RegisterAssemblyModules(assembly);
                containerBuilder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith(endWithName)).SingleInstance().PropertiesAutowired();
                string endWithName2 = "Repository";
                containerBuilder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith(endWithName2)).AsImplementedInterfaces().SingleInstance().PropertiesAutowired();
            }
        }
    }
}
