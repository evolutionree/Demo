﻿using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using NLog.Web;
using UBeat.Crm.CoreApi.Core;
using UBeat.Crm.CoreApi.Core.Utility;
using UBeat.Crm.CoreApi.Utility;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Diagnostics;
using UBeat.Crm.CoreApi.Services.Services;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Reflection;

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

            CoreApiRegistEngine.RegisterServices(containerBuilder, "UBeat.Crm.CoreApi.Services", "Services");
            CoreApiRegistEngine.RegisterImplemented(containerBuilder, "UBeat.Crm.CoreApi.Repository", "Repository");

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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {


            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            //add NLog to ASP.NET Core
            loggerFactory.AddNLog();
            //add NLog.Web
            app.AddNLogWeb();


            app.UseGlobalErrors(env, loggerFactory);


            //Add Json Web Token Auth
            app.UseJwtBearerAuthentication(JwtAuth.GetJwtOptions());


            //Add Cross Domain
            app.UseCors(policy =>
            {
                policy.AllowAnyOrigin();
            });
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }
        /// <summary>
        /// 装载第三方（插件）的Controller
        /// </summary>
        /// <param name="mvcBuilder"></param>
        public void LoadCustomerProjectAssembly(IMvcBuilder mvcBuilder) {
            var config = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("plugin.json")
              .Build();
            List<string> plugins = new List<string>();
            IEnumerator<IConfigurationSection> it = config.GetSection("PluginProjectList").GetChildren().GetEnumerator();
            while (it.MoveNext()) {
                IConfigurationSection item = it.Current;
                if (item.Value != null && item.Value.Length!= 0) {
                    plugins.Add(item.Value);
                }
            }
            dynamic type = this.GetType();
            string currentDirectory =Path.GetDirectoryName(type.Assembly.Location);
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
                    && d.Parent.Parent.Parent.Parent != null) ){
                    return;
                }
                currentDirectory = d.Parent.Parent.Parent.Parent.FullName;
                foreach (string item in plugins) {
                    string filePath = "";
                    if (item.EndsWith(".dll")) {
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
                    string endWithName = "Services";
                    containerBuilder.RegisterAssemblyModules(assembly);
                    containerBuilder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith(endWithName)).SingleInstance().PropertiesAutowired();
                    string endWithName2 = "Repository";
                    containerBuilder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith(endWithName2)).AsImplementedInterfaces().SingleInstance().PropertiesAutowired();

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
                    string endWithName = "Services";
                    containerBuilder.RegisterAssemblyModules(assembly);
                    containerBuilder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith(endWithName)).SingleInstance().PropertiesAutowired();
                    string endWithName2 = "Repository";
                    //containerBuilder.RegisterAssemblyModules(assembly)
                    containerBuilder.RegisterAssemblyTypes(assembly)
                    .Where(t => t.Name.EndsWith(endWithName2)).AsImplementedInterfaces().SingleInstance().PropertiesAutowired();

                }
            }
            //var assembly = Assembly.Load(new AssemblyName(fileInfo.FullName));
            //CoreApiRegistEngine.RegisterServices(containerBuilder, "UBeat.Crm.CoreApi.Services", "Services");
            
        }
    }
}
