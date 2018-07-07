
using Microsoft.AspNetCore.Builder;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using System;
using System.Net;
using System.ComponentModel.DataAnnotations;
using UBeat.Crm.CoreApi.Services.Models;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace UBeat.Crm.CoreApi.Utility
{
    public static class GlobalErrorsExtensions
    {
        private static ILogger _logger;
        public static IApplicationBuilder UseGlobalErrors(this IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            _logger = loggerFactory.CreateLogger("GlobalErrors");
            ExceptionHandlerOptions options = new ExceptionHandlerOptions();
            options.ExceptionHandler = HandleError;
            //options.ExceptionHandlingPath = new Microsoft.AspNetCore.Http.PathString(exceptionHandlingPath);
            app.UseExceptionHandler(options);
            return app;
        }

        /// <summary>
        /// 统一处理系统异常
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async static Task HandleError(HttpContext context)
        {

            var feature = context.Features.Get<IExceptionHandlerFeature>();
            var error = feature?.Error;

            _logger.Log(LogLevel.Error, 0, error.Message, error, (m, e) => e.ToString());

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response started,so ignored.");
                throw error;
            }
            try
            {
                await DisplayRuntimeException(context, error);
            }
            catch (Exception ext)
            {
                _logger.LogError(0, ext, "DisplayRuntimeException method exception occurred");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                var outputResult = new OutputResult<object>(string.Format("系统异常：{0}", (int)HttpStatusCode.InternalServerError), error.Message, 1);
                await context.Response.WriteAsync(JsonConvert.SerializeObject(outputResult));
            }

        }


        private static Task DisplayRuntimeException(HttpContext context, Exception ex)
        {
            context.Response.Clear();
            //reset the contenttype for response
            context.Response.ContentType = "application/json";

            var exceptionType = ex.GetType();
            OutputResult<object> outputResult = null;
            if (exceptionType == typeof(ValidationException))
            {
                //context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                outputResult = new OutputResult<object>(string.Format("数据参数验证失败：{0}", (int)HttpStatusCode.BadRequest), ex.Message, 1);
            }
            else if (exceptionType == typeof(UnauthorizedAccessException))
            {
                //context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                //outputResult = new OutputResult<object>(string.Format("身份验证失败：{0}", (int)HttpStatusCode.Unauthorized), ex.Message, 1);
                outputResult = new OutputResult<object>(null,"您的帐号session已过期", -25013);

               
            }
            else if(exceptionType==typeof(TimeoutException))
            {
                outputResult = new OutputResult<object>(string.Format("服务繁忙，请稍后再试", (int)HttpStatusCode.InternalServerError), ex.Message, 1);
            }
            
          
            else
            {
                //context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                outputResult = new OutputResult<object>(string.Format("系统异常：{0}", (int)HttpStatusCode.InternalServerError), ex.Message, 1);
            }
            return context.Response.WriteAsync(JsonConvert.SerializeObject(outputResult));
        }
    }
}
