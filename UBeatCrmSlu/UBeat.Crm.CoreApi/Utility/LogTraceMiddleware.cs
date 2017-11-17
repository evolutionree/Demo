using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace UBeat.Crm.CoreApi.Utility
{
    public class LogTraceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public LogTraceMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<LogTraceMiddleware>();
        }

        public Task Invoke(HttpContext context)
        {
            if (string.IsNullOrWhiteSpace(context.Request.ContentType) ||
                !context.Request.ContentType.ToLower().Contains("json"))
            {
                return _next(context);
            }
            LogRequest(context.Request);
            return _next(context);
        }

        private void LogRequest(HttpRequest request)
        {
            using (var bodyReader = new StreamReader(request.Body))
            {
                string body = bodyReader.ReadToEnd();

                request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
                _logger.LogInformation($"Request: {body}");
            }
        }
    }
}
