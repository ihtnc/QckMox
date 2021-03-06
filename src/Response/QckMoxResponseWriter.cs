using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;

namespace QckMox.Response
{
    internal interface IQckMoxResponseWriter
    {
        Task<bool> Write(HttpContext context);
    }

    internal class QckMoxResponseWriter : IQckMoxResponseWriter
    {
        private readonly IQckMoxConfigurationProvider _config;
        private readonly IQckMoxResponseFileProvider _file;

        public QckMoxResponseWriter(IQckMoxConfigurationProvider config, IQckMoxResponseFileProvider file)
        {
            _config = config;
            _file = file;
        }

        public async Task<bool> Write(HttpContext context)
        {
            var response = await _file.GetResponse(context.Request);

            switch(response.Result)
            {
                case QckMoxResponseResult.Success:
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    break;

                case QckMoxResponseResult.NotFound:
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    break;

                case QckMoxResponseResult.Error:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    break;

                case QckMoxResponseResult.Redirect:
                    return false;

                default:
                    throw new NotImplementedException();
            }

            context.Response.ContentType = response.Data.ContentType;

            var headers = response.Data?.ResponseHeaders ?? new Dictionary<string, string>();
            foreach(var header in headers)
            {
                context.Response.Headers.Add(header.Key, header.Value);
            }

            if (response.Data?.Content is not null)
            {
                response.Data.Content.Position = 0;
                await response.Data.Content.CopyToAsync(context.Response.Body);
            }

            return true;
        }
    }
}