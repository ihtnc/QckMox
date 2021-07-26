using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using QckMox.Configuration;
using FluentAssertions;
using NSubstitute;
using Xunit;
using QckMox.Tests.Integration.TestHelper;

namespace QckMox.Tests.Integration.Configuration
{
    public class ResponseMap
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(99)]
        public async Task Should_Mock_Request_Matching_A_ResponseMap_Key(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var resourcePath = "response.json";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, resourcePath);
            var content = $"{{'data':'{resource}'}}";

            qcxmox.FileProvider
                .GetContent(Arg.Any<string>())
                .Returns(null as string);

            qcxmox.FileProvider
                .GetStreamContent(responsePath)
                .ReturnsAsStream(content);

            var folderStructure = string.Empty;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                folderStructure = Path.Combine(folderStructure, folder);
                subFolderCount--;
            }

            var resourceUri = Path.Combine(folderStructure, resource);
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resourceUri);
            var server = await qcxmox.StartServer(config =>
            {
                config.ResponseMapConfigValue.Add($"GET {resourceUri}", resourcePath);
                config.ResponseMapConfigValue.Add($"GET {resource}", "notexisting.json");
            });

            // ACT
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.Parse(content);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(99)]
        public async Task Should_Use_Mock_Response_At_Matching_ResponseMap_Value(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";

            var folderStructure = string.Empty;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                folderStructure = Path.Combine(folderStructure, folder);
                subFolderCount--;
            }

            var resourcePath = Path.Combine(folderStructure, resource, "response.json");
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, resourcePath);
            var content = $"{{'data':'{resource}'}}";

            var otherResponsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, "response.json");
            var otherContent = $"{{'data':'otherContent'}}";

            qcxmox.FileProvider
                .GetContent(Arg.Any<string>())
                .Returns(null as string);

            qcxmox.FileProvider
                .GetStreamContent(responsePath)
                .ReturnsAsStream(content);
            qcxmox.FileProvider
                .GetStreamContent(otherResponsePath)
                .ReturnsAsStream(otherContent);

            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
            var server = await qcxmox.StartServer(config =>
            {
                config.ResponseMapConfigValue.Add($"GET {resource}", resourcePath);
            });

            // ACT
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.Parse(content);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_Ignore_Request_Not_Matching_A_ResponseMap_Key()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var otherResource = $"{Guid.NewGuid():N}";
            var folder = $"{Guid.NewGuid():N}";

            var folderConfig = Path.Combine(qcxmox.AppConfig.ResponseSource, folder, QckMoxConfig.FOLDER_CONFIG_FILE);
            var config = $@"{{
    'ResponseMap': {{
        'GET {otherResource}': 'response.json'
    }}
}}";

            var otherResponsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, folder, "response.json");
            var otherContent = $"{{'data':'{otherResource}'}}";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(folderConfig))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(folderConfig)
                .Returns(config);

            qcxmox.FileProvider
                .GetStreamContent(otherResponsePath)
                .ReturnsAsStream(otherContent);

            var server = await qcxmox.StartServer();

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folder, resource);
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
        public async Task Should_Resolve_ResponseMap_Items_Relative_To_Configuration_Location(int subFolderCount)
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

            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, folderStructure, "response.json");
            var content = $"{{'data':'{resource}'}}";

            var folderConfigPath = Path.Combine(folderStructure, QckMoxConfig.FOLDER_CONFIG_FILE);
            var configPath = Path.Combine(qcxmox.AppConfig.ResponseSource, folderConfigPath);
            var config = $@"{{
    'ResponseMap': {{
        'GET {resource}': 'response.json'
    }}
}}
";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(configPath))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(configPath)
                .Returns(config);

            qcxmox.FileProvider
                .GetStreamContent(responsePath)
                .ReturnsAsStream(content);

            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folderStructure, resource);
            var server = await qcxmox.StartServer();

            // ACT
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.Parse(content);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("", "folder\\subfolder\\")]
        [InlineData("folder", "subfolder\\")]
        [InlineData("folder\\subfolder", "")]
        public async Task Should_Resolve_ResponseMap_Keys_Relative_To_Configuration_Location(string uri, string expectedResponsePath)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, uri, "response.json");
            var content = $"{{'data':'{resource}'}}";

            var folderConfigPath = Path.Combine(uri, QckMoxConfig.FOLDER_CONFIG_FILE);
            var configPath = Path.Combine(qcxmox.AppConfig.ResponseSource, folderConfigPath);
            var config = $@"{{
    'ResponseMap': {{
        'GET {expectedResponsePath.Replace('\\', '/')}{resource}': 'response.json'
    }}
}}
";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(configPath))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(configPath)
                .Returns(config);

            qcxmox.FileProvider
                .GetStreamContent(responsePath)
                .ReturnsAsStream(content);

            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, "folder\\subfolder", resource);
            var server = await qcxmox.StartServer();

            // ACT
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.Parse(content);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory]
        [InlineData("", "folder\\subfolder\\")]
        [InlineData("folder", "subfolder\\")]
        [InlineData("folder\\subfolder", "")]
        public async Task Should_Resolve_ResponseMap_Values_Relative_To_Configuration_Location(string uri, string expectedConfigPath)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, "folder\\subfolder\\response.json");
            var content = $"{{'data':'{resource}'}}";

            var configPath = Path.Combine(qcxmox.AppConfig.ResponseSource, uri, QckMoxConfig.FOLDER_CONFIG_FILE);
            var config = $@"{{
    'ResponseMap': {{
        'GET {resource}': '{expectedConfigPath.Replace("\\", "\\\\")}response.json'
    }}
}}
";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(configPath))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(configPath)
                .Returns(config);

            qcxmox.FileProvider
                .GetStreamContent(responsePath)
                .ReturnsAsStream(content);

            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, uri, resource);
            var server = await qcxmox.StartServer();

            // ACT
            var context = await server.SendAsync(c =>
            {
                c.Request.Method = HttpMethods.Get;
                c.Request.Path = requestUri;
            });

            // ASSERT
            context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            var actualContent = await QckMoxServer.GetContent(context.Response);
            var expected = JToken.Parse(content);
            var actual = JToken.Parse(actualContent);
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task Should_Ignore_Configuration_From_Non_Parent_Folders()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var folder = $"{Guid.NewGuid():N}";
            var otherFolder = $"{Guid.NewGuid():N}";

            var otherFolderConfig = Path.Combine(qcxmox.AppConfig.ResponseSource, otherFolder, QckMoxConfig.FOLDER_CONFIG_FILE);
            var otherConfig = $@"{{
    'ResponseMap': {{
        'GET {folder}/{resource}': 'response.json'
    }}
}}";

            var otherFolderResponsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, otherFolder, QckMoxConfig.FOLDER_CONFIG_FILE);
            var otherContent = $"{{'data':'{resource}'}}";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(otherFolderConfig))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(otherFolderConfig)
                .Returns(otherConfig);

            qcxmox.FileProvider
                .GetStreamContent(otherFolderResponsePath)
                .ReturnsAsStream(otherContent);

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
    }
}