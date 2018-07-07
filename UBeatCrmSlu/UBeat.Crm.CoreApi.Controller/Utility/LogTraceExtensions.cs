using System;
using Microsoft.AspNetCore.Builder;

namespace UBeat.Crm.CoreApi.Utility
{
    public static class LogTraceExtensions
    {
        public static IApplicationBuilder UseIoTrace(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LogTraceMiddleware>(Array.Empty<object>());
        }
    }
}
