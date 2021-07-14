using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QckMox
{
    public interface IQckMoxResponseWriter
    {
        Task<bool> Write(HttpContext context);
    }

    public class QckMoxResponseWriter : IQckMoxResponseWriter
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
            var response = _file.GetResponse(context.Request);
            if(response == null) { return false; }

            context.Response.ContentType = response.ContentType;

            foreach(var header in response.Headers)
            {
                context.Response.Headers.Add(header.Key, header.Value);
            }

            await response.Content.CopyToAsync(context.Response.Body);
            return true;
        }
    }
}