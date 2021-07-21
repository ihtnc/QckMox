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
    public class EndPoint
    {
        [Fact]
        public async Task Should_Mock_Request_At_Default_EndPoint_When_Not_Specified()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(QckMoxAppConfig.Default.ResponseSource, resource, "GET.json");
            var content = $"{{'data':'{resource}'}}";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(responsePath))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(responsePath)
                .Returns(content);

            var server = await qcxmox.StartServer();

            // ACT
            var requestUri = Path.Combine(QckMoxAppConfig.Default.EndPoint, resource);
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
        public async Task Should_Mock_Request_At_Specified_EndPoint()
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var responsePath = Path.Combine(QckMoxAppConfig.Default.ResponseSource, resource, "GET.json");
            var content = $"{{'data':'{resource}'}}";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(responsePath))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(responsePath)
                .Returns(content);

            var mockedEndPoint = "/api/mocks/";
            var server = await qcxmox.StartServer(config =>
            {
                config.EndPointConfigValue = mockedEndPoint;
            });

            // ACT
            var requestUri = Path.Combine(mockedEndPoint, resource);
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
        [InlineData(true)]
        [InlineData(false)]
        public async Task Should_Handle_Unmocked_Request(bool useDefaultEndPoint)
        {
            // ARRANGE
            var qcxmox = new QckMoxServer();

            var resource = $"{Guid.NewGuid():N}";
            var requestUri = Path.Combine("/api/active-endpoint/", resource);
            var responsePath = Path.Combine(QckMoxAppConfig.Default.ResponseSource, resource, "GET.json");
            var content = $"{{'data':'{resource}'}}";

            qcxmox.FileProvider
                .GetContent(ArgAny.Except(responsePath))
                .Returns(null as string);
            qcxmox.FileProvider
                .GetContent(responsePath)
                .Returns(content);

            var server = await qcxmox.StartServer(config =>
            {
                if (useDefaultEndPoint is false)
                {
                    config.EndPointConfigValue = "/api/mocks/";
                }
            });

            // ACT
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