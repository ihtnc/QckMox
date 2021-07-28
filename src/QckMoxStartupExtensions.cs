using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using QckMox.Configuration;
using QckMox.IO;
using QckMox.Response;
using QckMox.Matcher;

namespace QckMox
{
    public static class QckMoxStartupExtensions
    {
        public static IServiceCollection AddQckMox(this IServiceCollection services, IConfiguration config)
        {
            return services.Configure<QckMoxAppConfig>(config.GetSection(QckMoxAppConfig.CONFIG_KEY))
                           .AddDependencies();
        }

        public static IServiceCollection AddQckMox(this IServiceCollection services, Func<QckMoxAppConfig, QckMoxAppConfig> configurator)
        {
            return services.AddSingleton<IOptions<QckMoxAppConfig>>(provider =>
                            {
                                var defaultConfig = QckMoxAppConfig.GetDefaultValues();
                                var config = defaultConfig;
                                config = configurator?.Invoke(config);
                                config = config ?? defaultConfig;
                                return Options.Create<QckMoxAppConfig>(config);
                            })
                           .AddDependencies();
        }

        public static IServiceCollection AddQckMoxCustomMatcher<T>(this IServiceCollection services) where T : IQckMoxCustomMatcher, new()
        {
            return services.AddSingleton<IQckMoxCustomMatcher>(provider => new T());
        }

        internal static IServiceCollection AddDependencies(this IServiceCollection services)
        {
            return services.AddSingleton<IFileProvider, JsonFileProvider>()
                           .AddSingleton<IQckMoxConfigurationProvider, QckMoxConfigurationProvider>()

                           .AddSingleton<IQckMoxMatcher, ResponseMapMatcher>()
                           .AddSingleton<IQckMoxMatcher, RequestParameterMatcher>()
                           .AddSingleton<IQckMoxMatcher, RequestMethodMatcher>()

                           .AddSingleton<IQckMoxCustomMatchResultHandler, QckMoxCustomMatchResultHandler>()
                           .AddSingleton<IQckMoxDefaultMatchResultHandler, QckMoxDefaultMatchResultHandler>()

                           .AddSingleton<IQckMoxResponseFileProvider, QckMoxResponseFileProvider>()
                           .AddSingleton<IQckMoxResponseWriter, QckMoxResponseWriter>();
        }

        public static IApplicationBuilder UseQckMox(this IApplicationBuilder app)
        {
            return app.UseMiddleware<QckMoxMiddleware>();
        }
    }
}