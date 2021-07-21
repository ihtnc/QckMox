using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using FluentAssertions;
using NSubstitute;
using Xunit;
using QckMox.Tests.Integration.TestHelper;

namespace QckMox.Tests.Integration.Configuration
{
    public class ResponseSource
    {
        [Fact]
        public async Task Should_Use_Mock_Response_At_Default_ResponseSource_When_Not_Specified()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var responsePath = $"{qcxmox.AppConfig.ResponseSource}\\{resource}\\GET.json";
            var content = $"{{'data':'{resource}'}}";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(responsePath))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(responsePath)
                .Returns(content);

            var server = await qcxmox.StartServer();

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
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
        public async Task Should_Use_Mock_Response_At_Specified_ResponseSource()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var mockedResponseSource = "Mocks";
            var responsePath = $"{mockedResponseSource}\\{resource}\\GET.json";
            var content = $"{{'data':'{resource}'}}";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(responsePath))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(responsePath)
                .Returns(content);

            var server = await qcxmox.StartServer(config =>
            {
                config.ResponseSourceConfigValue = mockedResponseSource;
            });

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, resource);
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
        public async Task Should_Use_Mock_Response_Based_On_ResponseSource_Folder_Structure(int subFolderCount)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var folderStructure = string.Empty;
            while(subFolderCount > 0)
            {
                var folder = $"{Guid.NewGuid():N}";
                folderStructure = Path.Combine(folderStructure, $"{folder}");
                subFolderCount--;
            }

            var responsePath = Path.Combine(qcxmox.AppConfig.ResponseSource, folderStructure, "GET.json");
            var content = $"{{'data':'{Guid.NewGuid():N}'}}";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(responsePath))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(responsePath)
                .Returns(content);

            var server = await qcxmox.StartServer();

            // ACT
            var requestUri = Path.Combine(qcxmox.AppConfig.EndPoint, folderStructure);
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
    }
}