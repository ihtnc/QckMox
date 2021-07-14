using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QckMox
{
    public static class QckMoxStartupHelper
    {
        public static bool IsMockUri(HttpContext context)
        {
            var configProvider = context.RequestServices.GetService(typeof(IQckMoxConfigurationProvider)) as IQckMoxConfigurationProvider;
            var config = configProvider.GetGlobalConfig();
            return config.Disabled != true && context.Request.Path.Value.StartsWith(config.EndPoint, StringComparison.OrdinalIgnoreCase);
        }

        public static async Task MockMiddleware(HttpContext context, Func<Task> next)
        {
            var writer = context.RequestServices.GetService(typeof(IQckMoxResponseWriter)) as IQckMoxResponseWriter;
            var success = await writer.Write(context);

            if(!success) { await next.Invoke(); }
        }
    }
}