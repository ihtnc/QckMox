using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QckMox.Configuration;
using QckMox.IO;
using QckMox.Response;

namespace QckMox
{
    public static class QckMoxStartupExtensions
    {
        public static IServiceCollection AddQckMox(this IServiceCollection services, IConfiguration config)
        {
            return services.Configure<QckMoxAppConfig>(config.GetSection(QckMoxAppConfig.CONFIG_KEY))
                           .AddSingleton<IFileProvider, FileProvider>()
                           .AddSingleton<IQckMoxConfigurationProvider, QckMoxConfigurationProvider>()
                           .AddSingleton<IQckMoxResponseFileProvider, QckMoxResponseFileProvider>()
                           .AddSingleton<IQckMoxResponseWriter, QckMoxResponseWriter>();

        }

        public static IApplicationBuilder UseQckMox(this IApplicationBuilder app)
        {
            return app.UseMiddleware<QckMoxMiddleware>();
        }
    }
}