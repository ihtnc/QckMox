using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QckMox.Configs;

namespace QckMox
{
    public static class QckMoxStartupExtensions
    {
        public static IServiceCollection AddQckMox(this IServiceCollection services, IConfiguration config)
        {
            return services
                        .AddOptions()
                        .Configure<QckMoxAppConfig>(config.GetSection(Constants.CONFIG_KEY))
                        .AddSingleton<IFileProvider, FileProvider>()
                        .AddSingleton<IQckMoxConfigurationProvider, QckMoxConfigurationProvider>()
                        .AddSingleton<IQckMoxResponseFileProvider, QckMoxResponseFileProvider>()
                        .AddSingleton<IQckMoxResponseWriter, QckMoxResponseWriter>();
        }
        public static IApplicationBuilder UseQckMox(this IApplicationBuilder app)
        {
            return app.MapWhen(
                context => QckMoxStartupHelper.IsMockUri(context),
                builder => { builder.Use(QckMoxStartupHelper.MockMiddleware); }
            );
        }
    }
}