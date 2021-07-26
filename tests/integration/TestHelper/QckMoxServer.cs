using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QckMox.Configuration;
using QckMox.IO;
using QckMox.Response;
using QckMox.Matcher;
using NSubstitute;

namespace QckMox.Tests.Integration.TestHelper
{
    internal class QckMoxServer
    {
        internal static readonly int PassthroughResponseStatusCode = StatusCodes.Status418ImATeapot;
        internal static readonly Dictionary<string, string> PassthroughResponseContent = new Dictionary<string, string>
        {
            {"Message", "I'm a teapot, I can't brew coffee."}
        };

        public IFileProvider FileProvider { get; }
        public QckMoxAppConfig AppConfig { get; private set;}

        public QckMoxServer()
        {
            FileProvider = Substitute.For<IFileProvider>();
            AppConfig = QckMoxAppConfig.GetDefaultValues();
        }

        public async Task<TestServer> StartServer(Action<QckMoxAppConfig> appConfigSetter = null)
        {
            return await InternalStartServer(appConfigSetter, false);
        }

        public async Task<TestServer> StartServerWithRequestHandler(Action<QckMoxAppConfig> appConfigSetter = null)
        {
            return await InternalStartServer(appConfigSetter, true);
        }

        private async Task<TestServer> InternalStartServer(Action<QckMoxAppConfig> appConfigSetter, bool addRequestHandler)
        {
            var host = await new HostBuilder()
                .ConfigureWebHost(builder =>
                {
                    builder
                        .UseTestServer()
                        .ConfigureServices(services =>
                        {
                            services
                                .AddSingleton<IOptions<QckMoxAppConfig>>(provider =>
                                {
                                    appConfigSetter?.Invoke(AppConfig);
                                    return Options.Create<QckMoxAppConfig>(AppConfig);
                                })

                                .AddSingleton<IFileProvider>(services => FileProvider)
                                .AddSingleton<IQckMoxConfigurationProvider, QckMoxConfigurationProvider>()

                                .AddSingleton<IQckMoxMatcher, ResponseMapMatcher>()
                                .AddSingleton<IQckMoxMatcher, RequestParameterMatcher>()
                                .AddSingleton<IQckMoxMatcher, RequestMethodMatcher>()

                                .AddSingleton<IQckMoxCustomMatchResultHandler, QckMoxCustomMatchResultHandler>()
                                .AddSingleton<IQckMoxDefaultMatchResultHandler, QckMoxDefaultMatchResultHandler>()

                                .AddSingleton<IQckMoxResponseFileProvider, QckMoxResponseFileProvider>()
                                .AddSingleton<IQckMoxResponseWriter, QckMoxResponseWriter>();
                        })
                        .Configure(app =>
                        {
                            app.UseQckMox();

                            if (addRequestHandler)
                            {
                                app.Use(async (context, next) =>
                                {
                                    context.Response.StatusCode = PassthroughResponseStatusCode;
                                    await context.Response.WriteAsJsonAsync(PassthroughResponseContent);
                                });
                            }
                        });
                })
                .StartAsync();

            var server = host.GetTestServer();
            server.BaseAddress = new Uri("http://localhost/");
            return server;
        }

        public static async Task<string> GetContent(HttpResponse response)
        {
            using(var reader = new StreamReader(response.Body))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}