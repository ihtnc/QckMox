using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using Xunit;
using QckMox.Tests.Integration.TestHelper;

namespace QckMox.Tests.Integration.Configuration.Request
{
    public class MatchQuery
    {
        [Fact]
        public async Task Should_Ignore_Queries_When_Not_Set()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, resource, "GET.json");
            var content = $"{{'data':'{resource}'}}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(responsePath)
                .MockFileContent(responsePath, content);

            var server = await qcxmox.StartServer(config =>
            {
                config.Request.MatchQuery = new string[0];
            });

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
                c.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {"query", new StringValues("value")}
                });
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.FromObject(QckMoxServer.PassthroughResponseContent);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_Handle_Unmatched_Queries_When_Set()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, resource, "GET.json");
            var content = $"{{'data':'{resource}'}}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(responsePath)
                .MockFileContent(responsePath, content);

            var queryName = "queryName";
            var server = await qcxmox.StartServer(config =>
            {
                config.Request.MatchQuery = new [] { queryName };
            });

            // ACT
            var otherQueryName = $"otherQueryName";
            var otherQueryValue = $"{Guid.NewGuid():N}";
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
                c.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {otherQueryName, new StringValues(otherQueryValue)}
                });
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task Should_Mock_Matching_Query_Values_When_Set()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var queryName = "queryName";
            var queryValue = $"{Guid.NewGuid():N}";
            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, resource, $"GET {queryName}={queryValue}.json");
            var content = $"{{'data':'{resource}'}}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(responsePath)
                .MockFileContent(responsePath, content);

            var server = await qcxmox.StartServer(config =>
            {
                config.Request.MatchQuery = new [] { queryName };
            });

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
                c.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {queryName, new StringValues(queryValue)}
                });
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.Parse(content);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_Ignore_Additional_Queries_Not_In_The_Config()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var queryName = "queryName";
            var queryValue = $"{Guid.NewGuid():N}";
            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, resource, $"GET {queryName}={queryValue}.json");
            var content = $"{{'data':'{resource}'}}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(responsePath)
                .MockFileContent(responsePath, content);

            var server = await qcxmox.StartServer(config =>
            {
                config.Request.MatchQuery = new [] { queryName };
            });

            // ACT
            var otherQueryName = "otherQueryName";
            var otherQueryValue = $"{Guid.NewGuid():N}";
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
                c.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {queryName, new StringValues(queryValue)},
                    {otherQueryName, new StringValues(otherQueryValue)}
                });
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.Parse(content);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_Allow_Partial_Match_Of_Queries()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var queryName = "queryName";
            var queryValue = $"{Guid.NewGuid():N}";
            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, resource, $"GET {queryName}={queryValue}.json");
            var content = $"{{'data':'{resource}'}}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(responsePath)
                .MockFileContent(responsePath, content);

            var otherQueryName = "otherQueryName";
            var server = await qcxmox.StartServer(config =>
            {
                config.Request.MatchQuery = new [] { queryName, otherQueryName };
            });

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
                c.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {queryName, new StringValues(queryValue)}
                });
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.Parse(content);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_Not_Return_Matching_Mocks_Unless_Set_In_Config()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var queryName = "queryName";
            var queryValue = $"{Guid.NewGuid():N}";
            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, resource, $"GET {queryName}={queryValue}.json");
            var content = $"{{'data':'{resource}'}}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(responsePath)
                .MockFileContent(responsePath, content);

            var otherQueryName = "otherQueryName";
            var server = await qcxmox.StartServer(config =>
            {
                config.Request.MatchQuery = new [] { otherQueryName };
            });

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
                c.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {queryName, new StringValues(queryValue)}
                });
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task Should_Handle_Matching_Queries_Without_A_Corresponding_Mock()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockNoFilesToExist();

            var queryName = "queryName";
            var server = await qcxmox.StartServer(config =>
            {
                config.Request.MatchQuery = new [] { queryName };
            });

            // ACT
            var queryValue = $"{Guid.NewGuid():N}";
            var resource = $"{Guid.NewGuid():N}";
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
                c.Request.Query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    {queryName, new StringValues(queryValue)}
                });
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }






        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        public async Task Should_Use_Configuration_From_Folder(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var folderStructure = string.Empty;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                folderStructure = Path.Combine(folderStructure, folder);
                subFolderCount--;
            }

            var folderConfig = Path.Combine(qcxmox.AppConfig.ResponseSource, folderStructure, QckMoxConfig.FOLDER_CONFIG_FILE);
            var config = @"{
    'Request': {
        'UnmatchedRequest': {
            'Passthrough': true
        }
    }
}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(folderConfig)
                .MockFileContent(folderConfig, config);

            var server = await qcxmox.StartServerWithRequestHandler();

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folderStructure);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(QckMoxServer.PassthroughResponseStatusCode);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.FromObject(QckMoxServer.PassthroughResponseContent);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_Ignore_Configuration_From_Non_Parent_Folders()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var folder = $"{Guid.NewGuid():N}";
            var otherFolder = $"{Guid.NewGuid():N}";
            var otherFolderConfig = Path.Combine(qcxmox.AppConfig.ResponseSource, otherFolder, QckMoxConfig.FOLDER_CONFIG_FILE);

            var config = @"{
    'Request': {
        'UnmatchedRequest': {
            'Passthrough': true
        }
    }
}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(otherFolderConfig)
                .MockFileContent(otherFolderConfig, config);

            var server = await qcxmox.StartServerWithRequestHandler();

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folder);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        public async Task Should_Inherit_Configuration_From_Parent_Folder(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var folderStructure = string.Empty;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                folderStructure = Path.Combine(folderStructure, folder);
                subFolderCount--;
            }

            var folderConfig = Path.Combine(qcxmox.AppConfig.ResponseSource, folderStructure, QckMoxConfig.FOLDER_CONFIG_FILE);
            var config = @"{
    'Request': {
        'UnmatchedRequest': {
            'Passthrough': true
        }
    }
}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(folderConfig)
                .MockFileContent(folderConfig, config);

            var server = await qcxmox.StartServerWithRequestHandler();

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folderStructure, $"{Guid.NewGuid():N}");
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(QckMoxServer.PassthroughResponseStatusCode);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.FromObject(QckMoxServer.PassthroughResponseContent);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(99)]
        public async Task Should_Inherit_Configuration_From_Anscestor_Folder(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var anscestorFolder = $"{Guid.NewGuid():N}";
            var anscestorConfigPath = Path.Combine(qcxmox.AppConfig.ResponseSource, anscestorFolder, QckMoxConfig.FOLDER_CONFIG_FILE);
            var ancestorConfig = @"{
    'Request': {
        'UnmatchedRequest': {
            'Passthrough': true
        }
    }
}";

            var folderStructure = anscestorFolder;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                folderStructure = Path.Combine(folderStructure, folder);
                subFolderCount--;
            }

            var folderConfigPath = Path.Combine(qcxmox.AppConfig.ResponseSource, folderStructure, QckMoxConfig.FOLDER_CONFIG_FILE);
            var folderConfig = @"{
    'Request': { }
}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(folderConfigPath, anscestorConfigPath)
                .MockFileContent(folderConfigPath, folderConfig)
                .MockFileContent(anscestorConfigPath, ancestorConfig);

            var server = await qcxmox.StartServerWithRequestHandler();

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folderStructure, $"{Guid.NewGuid():N}");
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(QckMoxServer.PassthroughResponseStatusCode);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.FromObject(QckMoxServer.PassthroughResponseContent);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        public async Task Should_Inherit_Configuration_From_AppSettings(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var folderStructure = string.Empty;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                folderStructure = Path.Combine(folderStructure, folder);
                subFolderCount--;
            }

            var folderConfig = Path.Combine(qcxmox.AppConfig.ResponseSource, folderStructure, QckMoxConfig.FOLDER_CONFIG_FILE);
            var config = @"{
    'Request': { }
}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(folderConfig)
                .MockFileContent(folderConfig, config);

            var server = await qcxmox.StartServerWithRequestHandler(config =>
            {
                config.Request.UnmatchedRequest.Passthrough = true;
            });

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folderStructure, $"{Guid.NewGuid():N}");
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(QckMoxServer.PassthroughResponseStatusCode);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.FromObject(QckMoxServer.PassthroughResponseContent);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        public async Task Should_Override_Configuration_From_Parent_Folder(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var parentStructure = string.Empty;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                parentStructure = Path.Combine(parentStructure, folder);
                subFolderCount--;
            }
            var parentConfigPath = Path.Combine(qcxmox.AppConfig.ResponseSource, parentStructure, QckMoxConfig.FOLDER_CONFIG_FILE);

            var resource = $"{Guid.NewGuid():N}";
            var folderConfigPath = Path.Combine(qcxmox.AppConfig.ResponseSource, parentStructure, resource, QckMoxConfig.FOLDER_CONFIG_FILE);
            var parentConfig = @"{
    'Request': {
        'UnmatchedRequest': {
            'Passthrough': true
        }
    }
}";
            var folderConfig = @"{
    'Request': {
        'UnmatchedRequest': {
            'Passthrough': false
        }
    }
}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(parentConfigPath, folderConfigPath)
                .MockFileContent(parentConfigPath, parentConfig)
                .MockFileContent(folderConfigPath, folderConfig);

            var server = await qcxmox.StartServerWithRequestHandler();

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, parentStructure, resource);
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(99)]
        public async Task Should_Override_Configuration_From_Anscestor_Folder(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var anscestorFolder = $"{Guid.NewGuid():N}";
            var anscestorConfigPath = Path.Combine(qcxmox.AppConfig.ResponseSource, anscestorFolder, QckMoxConfig.FOLDER_CONFIG_FILE);
            var ancestorConfig = @"{
    'Request': {
        'UnmatchedRequest': {
            'Passthrough': true
        }
    }
}";

            var folderStructure = anscestorFolder;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                folderStructure = Path.Combine(folderStructure, folder);
                subFolderCount--;
            }

            var folderConfigPath = Path.Combine(qcxmox.AppConfig.ResponseSource, folderStructure, QckMoxConfig.FOLDER_CONFIG_FILE);
            var folderConfig = @"{
    'Request': {
        'UnmatchedRequest': {
            'Passthrough': false
        }
    }
}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(folderConfigPath, anscestorConfigPath)
                .MockFileContent(folderConfigPath, folderConfig)
                .MockFileContent(anscestorConfigPath, ancestorConfig);

            var server = await qcxmox.StartServerWithRequestHandler();

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folderStructure, $"{Guid.NewGuid():N}");
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        public async Task Should_Override_Configuration_From_AppSettings(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var folderStructure = string.Empty;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                folderStructure = Path.Combine(folderStructure, folder);
                subFolderCount--;
            }

            var folderConfig = Path.Combine(qcxmox.AppConfig.ResponseSource, folderStructure, QckMoxConfig.FOLDER_CONFIG_FILE);
            var config = @"{
    'Request': {
        'UnmatchedRequest': {
            'Passthrough': false
        }
     }
}";

            qcxmox
                .UseActualPathWrapper()
                .UsePathResolverThatResolvesAllPaths()
                .MockAllFoldersToExist()
                .MockSpecificFilesToExist(folderConfig)
                .MockFileContent(folderConfig, config);

            var server = await qcxmox.StartServerWithRequestHandler(config =>
            {
                config.Request.UnmatchedRequest.Passthrough = true;
            });

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folderStructure, $"{Guid.NewGuid():N}");
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}