using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QckMox
{
    internal class QckMoxMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IQckMoxResponseWriter _responseWriter;
        private readonly IQckMoxConfigurationProvider _configProvider;

        public QckMoxMiddleware(RequestDelegate next, IQckMoxResponseWriter responseWriter, IQckMoxConfigurationProvider configProvider)
        {
            _next = next;
            _responseWriter = responseWriter;
            _configProvider = configProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            // check if we need to mock the response
            var config = _configProvider.GetGlobalConfig();
            var isMockRequest = context.Request.Path.Value.StartsWith(config.EndPoint, StringComparison.OrdinalIgnoreCase);

            // mock not needed so let the request pass through
            if(config.Disabled is false && isMockRequest is true)
            {
                // respond to the request with a mock value
                var responseWritten = await _responseWriter.Write(context);
                if (responseWritten is true) { return; }
            }

            await _next.Invoke(context);
        }
    }
}