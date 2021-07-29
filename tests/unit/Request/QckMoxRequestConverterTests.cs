using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using QckMox.Configuration;
using QckMox.Request;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace QckMox.Tests.Unit.Request
{
    public class QckMoxRequestConverterTests
    {
        private readonly IQckMoxConfigurationProvider _provider;
        private readonly QckMoxAppConfig _config;

        private readonly IQckMoxRequestConverter _converter;

        public QckMoxRequestConverterTests()
        {
            _config = QckMoxAppConfig.GetDefaultValues();
            _provider = Substitute.For<IQckMoxConfigurationProvider>();
            _provider.GetGlobalConfig().Returns(_config);

            _converter = new QckMoxRequestConverter(_provider);
        }

        [Fact]
        public void GetMethodString_Should_Return_Correctly()
        {
            // ARRANGE
            var expected = $"{Guid.NewGuid():N}".ToUpper();
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Method = expected;

            // ACT
            var actual = _converter.GetMethodString(request);

            // ASSERT
            actual.Should().Be(expected);
        }

        [Fact]
        public void GetResourceString_Should_Return_Correctly()
        {
            // ARRANGE
            var endPoint = $"/api/{Guid.NewGuid():N}/";
            var expected = $"{Guid.NewGuid():N}";
            var path = $"{endPoint}{expected}";

            _config.EndPoint = endPoint;
            var context = new DefaultHttpContext();
            var request = context.Request;
            request.Path = path;

            // ACT
            var actual = _converter.GetResourceString(request);

            // ASSERT
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("query1=value1", "query1")]
        [InlineData("query1=value1&query3=value3", "query1", "query3")]
        [InlineData("", "query")]
        [InlineData("")]
        public void GetParameterString_Should_Include_Matching_Queries_Correctly(string expected, params string[] matchQueries)
        {
            // ARRANGE
            var context = new DefaultHttpContext();
            var request = context.Request;

            var queries = new Dictionary<string, StringValues>
            {
                {"query1", new StringValues("value1")},
                {"query2", new StringValues("value2")},
                {"query3", new StringValues("value3")}
            };
            request.Query = new QueryCollection(queries);

            _config.Request.MatchQuery = matchQueries;

            // ACT
            var actual = _converter.GetParameterString(request, _config);

            // ASSERT
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("query1=value1", "", "query1")]
        [InlineData("qqq-query1=value1&qqq-query3=value3", "qqq-", "query1", "query3")]
        [InlineData("", "qqq-", "query")]
        [InlineData("", "qqq-")]
        public void GetParameterString_Should_Add_Tag_To_Matching_Queries_Appropriately(string expected, string queryTag, params string[] matchQueries)
        {
            // ARRANGE
            var context = new DefaultHttpContext();
            var request = context.Request;

            var queries = new Dictionary<string, StringValues>
            {
                {"query1", new StringValues("value1")},
                {"query2", new StringValues("value2")},
                {"query3", new StringValues("value3")}
            };
            request.Query = new QueryCollection(queries);

            _config.Request.QueryTag = queryTag;
            _config.Request.MatchQuery = matchQueries;

            // ACT
            var actual = _converter.GetParameterString(request, _config);

            // ASSERT
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("qmx-header1=value1", "header1")]
        [InlineData("qmx-header1=value1&qmx-header3=value3", "header1", "header3")]
        [InlineData("", "header")]
        [InlineData("")]
        public void GetParameterString_Should_Tag_Matching_Headers_By_Default(string expected, params string[] matchHeaders)
        {
            // ARRANGE
            var context = new DefaultHttpContext();
            var request = context.Request;

            var headers = new Dictionary<string, StringValues>
            {
                {"header1", new StringValues("value1")},
                {"header2", new StringValues("value2")},
                {"header3", new StringValues("value3")}
            };
            request.Headers.Add("header1", new StringValues("value1"));
            request.Headers.Add("header2", new StringValues("value2"));
            request.Headers.Add("header3", new StringValues("value3"));

            _config.Request.MatchHeader = matchHeaders;

            // ACT
            var actual = _converter.GetParameterString(request, _config);

            // ASSERT
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("header1=value1", "", "header1")]
        [InlineData("qqq-header1=value1&qqq-header3=value3", "qqq-", "header1", "header3")]
        [InlineData("", "qqq-", "header")]
        [InlineData("", "qqq-")]
        public void GetParameterString_Should_Add_Tag_To_Matching_Headers_Appropriately(string expected, string headerTag, params string[] matchHeaders)
        {
            // ARRANGE
            var context = new DefaultHttpContext();
            var request = context.Request;

            var headers = new Dictionary<string, StringValues>
            {
                {"header1", new StringValues("value1")},
                {"header2", new StringValues("value2")},
                {"header3", new StringValues("value3")}
            };
            request.Headers.Add("header1", new StringValues("value1"));
            request.Headers.Add("header2", new StringValues("value2"));
            request.Headers.Add("header3", new StringValues("value3"));

            _config.Request.HeaderTag = headerTag;
            _config.Request.MatchHeader = matchHeaders;

            // ACT
            var actual = _converter.GetParameterString(request, _config);

            // ASSERT
            actual.Should().Be(expected);
        }


        [Theory]
        [InlineData("query2", "header3", "query2=value2&qmx-header3=value3")]
        [InlineData("query1", "query1", "query1=value1")]
        [InlineData("header2", "header2", "qmx-header2=value2")]
        public void GetParameterString_Should_Include_Matching_Parameters_Correctly(string matchQuery, string matchHeader, string expected)
        {
            // ARRANGE
            var context = new DefaultHttpContext();
            var request = context.Request;

            var queries = new Dictionary<string, StringValues>
            {
                {"query1", new StringValues("value1")},
                {"query2", new StringValues("value2")},
                {"query3", new StringValues("value3")}
            };
            request.Query = new QueryCollection(queries);

            var headers = new Dictionary<string, StringValues>
            {
                {"header1", new StringValues("value1")},
                {"header2", new StringValues("value2")},
                {"header3", new StringValues("value3")}
            };
            request.Headers.Add("header1", new StringValues("value1"));
            request.Headers.Add("header2", new StringValues("value2"));
            request.Headers.Add("header3", new StringValues("value3"));

            _config.Request.MatchQuery = new[] { matchQuery };
            _config.Request.MatchHeader = new[] { matchHeader };

            // ACT
            var actual = _converter.GetParameterString(request, _config);

            // ASSERT
            actual.Should().Be(expected);
        }
    }
}