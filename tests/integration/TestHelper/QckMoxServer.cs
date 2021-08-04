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
using QckMox.IO;
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

        public IIOProvider IO { get; }
        public IFileProvider FileProvider { get; }
        public QckMoxAppConfig AppConfig { get; private set;}

        public QckMoxServer()
        {
            IO = Substitute.For<IIOProvider>();
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
                                    var defaultConfig = QckMoxAppConfig.GetDefaultValues();
                                    appConfigSetter?.Invoke(defaultConfig);
                                    AppConfig = defaultConfig;

                                    return Options.Create<QckMoxAppConfig>(defaultConfig);
                                })

                                .AddSingleton<IPathWrapper, PathWrapper>()
                                .AddSingleton<IFileWrapper, FileWrapper>()
                                .AddSingleton<IDirectoryWrapper, DirectoryWrapper>()
                                .AddSingleton<IPathResolver, PathResolver>()
                                .AddSingleton<IIOProvider>(services => IO)
                                .AddSingleton<IFileProvider>(services => FileProvider)
                                .AddDependencies();
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

        public QckMoxServer UseActualPathResolver()
        {
            var resolver = new PathResolver(IO.Path, IO.File, IO.Directory);
            IO.PathResolver.Returns(resolver as IPathResolver);

            return this;
        }

        public QckMoxServer UsePathResolverThatResolvesAllPaths(IFileWrapper fileWrapper = null, IDirectoryWrapper directoryWrapper = null)
        {
            var file = fileWrapper ?? Substitute.For<IFileWrapper>();
            file.Exists(Arg.Any<string>()).Returns(true);

            var directory = directoryWrapper ?? Substitute.For<IDirectoryWrapper>();
            directory.Exists(Arg.Any<string>()).Returns(true);

            IO.PathResolver.Returns(new PathResolver(new PathWrapper(), file, directory));

            return this;
        }

        public QckMoxServer UseActualPathWrapper()
        {
            IO.Path.Returns(new PathWrapper());

            return this;
        }

        public QckMoxServer MockAllFoldersToExist()
        {
            IO.Directory
                .Exists(Arg.Any<string>())
                .Returns(true);

            return this;
        }

        public QckMoxServer MockSpecificFoldersToExist(params string[] folders)
        {
            var list = new List<string>();
            foreach(var folder in folders)
            {
                var fullPath = IO.PathResolver.ResolveFilePath(folder);
                list.Add(fullPath);

                IO.Directory
                    .Exists(fullPath)
                    .Returns(true);
            }

            var fullPaths = list.ToArray();
            IO.Directory
                .Exists(ArgAny.Except(fullPaths))
                .Returns(false);

            return this;
        }

        public QckMoxServer MockNoFilesToExist()
        {
            IO.File
                .Exists(Arg.Any<string>())
                .Returns(false);

            FileProvider
                .GetContent(Arg.Any<string>())
                .Returns(null as string);

            FileProvider
                .GetStreamContent(Arg.Any<string>())
                .Returns(null as Stream);

            return this;
        }

        public QckMoxServer MockSpecificFilesToExist(params string[] files)
        {
            var list = new List<string>();
            foreach(var file in files)
            {
                var fullPath = IO.PathResolver.ResolveFilePath(file);
                list.Add(fullPath);

                IO.File
                    .Exists(fullPath)
                    .Returns(true);
            }

            var fullPaths = list.ToArray();

            IO.File
                .Exists(ArgAny.Except(fullPaths))
                .Returns(false);

            FileProvider
                .GetContent(ArgAny.Except(fullPaths))
                .Returns(null as string);

            FileProvider
                .GetStreamContent(ArgAny.Except(fullPaths))
                .Returns(null as Stream);

            return this;
        }

        public QckMoxServer MockFileContent(string filePath, string content)
        {
            var fullPath = IO.PathResolver.ResolveFilePath(filePath);

            FileProvider
                .GetContent(fullPath)
                .Returns(content);

            FileProvider
                .GetStreamContent(fullPath)
                .ReturnsAsStream(content);

            return this;
        }
    }
}