using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using QckMox.Request;
using FluentAssertions;
using Xunit;

namespace QckMox.Tests.Unit.Request
{
    public class QckMoxRequestStringTests
    {
        [Theory]
        [InlineData("GET", "GET")]
        [InlineData("  posT", "posT")]
        [InlineData("other   ", "other")]
        [InlineData("   ", "")]
        [InlineData("", "")]
        [InlineData(null, "")]
        public void ToString_Should_Include_Method_Correctly(string method, string expected)
        {
            // ARRANGE
            var value = new QckMoxRequestString
            {
                Method = method
            };

            // ACT
            var actual = value.ToString();

            // ASSERT
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData("resource", " resource")]
        [InlineData("   resourcE/1", " resourcE/1")]
        [InlineData("\\RESOURCE\\1   ", " \\RESOURCE\\1")]
        [InlineData("   ", null)]
        [InlineData("", null)]
        [InlineData(null, null)]
        public void ToString_Should_Include_Resource_Correctly(string resource, string expected)
        {
            // ARRANGE
            var method = "GET";
            var value = new QckMoxRequestString
            {
                Method = method,
                Resource = resource
            };

            // ACT
            var actual = value.ToString();

            // ASSERT
            actual.Should().Be($"{method}{expected}");
        }

        [Theory]
        [InlineData("key", "value", " key=value")]
        [InlineData("   key", "value   ", " key=value")]
        [InlineData("key   ", "   value",  " key=value")]
        [InlineData("   ", "value",  null)]
        [InlineData("", "value",  null)]
        [InlineData("key", "   ",  " key")]
        [InlineData("key", "",  " key")]
        [InlineData("key", null,  " key")]
        public void ToString_Should_Include_Queries_Correctly(string queryKey, string queryValue, string expected)
        {
            // ARRANGE
            var method = "GET";
            var resource = "resource";
            var value = new QckMoxRequestString
            {
                Method = method,
                Resource = resource
            };

            value.Queries.Add(queryKey, queryValue);

            // ACT
            var actual = value.ToString();

            // ASSERT
            actual.Should().Be($"{method} {resource}{expected}");
        }

        [Theory]
        [InlineData("key", "value", " key=value")]
        [InlineData("   key", "value   ", " key=value")]
        [InlineData("key   ", "   value",  " key=value")]
        [InlineData("   ", "value",  null)]
        [InlineData("", "value",  null)]
        [InlineData("key", "   ",  " key")]
        [InlineData("key", "",  " key")]
        [InlineData("key", null,  " key")]
        public void ToString_Should_Include_Headers_Correctly(string headerKey, string headerValue, string expected)
        {
            // ARRANGE
            var method = "GET";
            var resource = "resource";
            var value = new QckMoxRequestString
            {
                Method = method,
                Resource = resource
            };

            value.Headers.Add(headerKey, headerValue);

            // ACT
            var actual = value.ToString();

            // ASSERT
            actual.Should().Be($"{method} {resource}{expected}");
        }


        [Theory]
        [InlineData("key1", "value1", "key2", "value2", "key1=value1&key2=value2")]
        [InlineData("key1", "value2", "key2", "value1", "key1=value2&key2=value1")]
        [InlineData("key2", "value2", "key1", "value1", "key1=value1&key2=value2")]
        [InlineData("key1", "value1", "key2", null, "key1=value1&key2")]
        [InlineData("key1", null, "key2", "value2", "key1&key2=value2")]
        public void ToString_Should_Append_And_Sort_Queries_By_Key(string queryKey1, string queryValue1, string queryKey2, string queryValue2, string expected)
        {
            // ARRANGE
            var method = "GET";
            var resource = "resource";
            var value = new QckMoxRequestString
            {
                Method = method,
                Resource = resource
            };

            value.Queries.Add(queryKey1, queryValue1);
            value.Queries.Add(queryKey2, queryValue2);

            // ACT
            var actual = value.ToString();

            // ASSERT
            actual.Should().Be($"{method} {resource} {expected}");
        }

        [Theory]
        [InlineData("key1", "value1", "key2", "value2", "key1=value1&key2=value2")]
        [InlineData("key2", "value2", "key1", "value1", "key1=value1&key2=value2")]
        [InlineData("key1", "value2", "key2", "value1", "key1=value2&key2=value1")]
        [InlineData("key2", "value1", "key1", "value2", "key1=value2&key2=value1")]
        [InlineData("key1", "value1", "key2", null, "key1=value1&key2")]
        [InlineData("key1", null, "key2", "value2", "key1&key2=value2")]
        [InlineData("key2", null, "key1", "value1", "key1=value1&key2")]
        [InlineData("key2", "value2", "key1", null, "key1&key2=value2")]
        public void ToString_Should_Append_And_Sort_Parameters_By_Key(string queryKey, string queryValue, string headerKey, string headerValue, string expected)
        {
            // ARRANGE
            var method = "GET";
            var resource = "resource";
            var value = new QckMoxRequestString
            {
                Method = method,
                Resource = resource
            };

            value.Queries.Add(queryKey, queryValue);
            value.Headers.Add(headerKey, headerValue);

            // ACT
            var actual = value.ToString();

            // ASSERT
            actual.Should().Be($"{method} {resource} {expected}");
        }

        [Theory]
        [InlineData(true, "GET", "resource", "key", "GET key")]
        [InlineData(true, "GET", null, "key", "GET key")]
        [InlineData(true, "GET", null, "", "GET")]
        [InlineData(false, "GET", "resource", "key", "GET resource key")]
        [InlineData(false, "GET", null, "key", "GET key")]
        [InlineData(false, "GET", null, "", "GET")]
        public void ToString_Should_Exclude_Resource_When_Required(bool excludeResource, string method, string resource, string queryKey, string expected)
        {
            // ARRANGE
            var value = new QckMoxRequestString
            {
                Method = method,
                Resource = resource
            };

            value.Queries.Add(queryKey, string.Empty);

            // ACT
            var actual = value.ToString(excludeResource: excludeResource);

            // ASSERT
            actual.Should().Be(expected);
        }

        [Theory]
        [InlineData(true, "key", "", "")]
        [InlineData(true, "", "key", "")]
        [InlineData(true, "key1", "key2", "")]
        [InlineData(false, "key", "", " key")]
        [InlineData(false, "", "key", " key")]
        [InlineData(false, "key1", "key2", " key1&key2")]
        public void ToString_Should_Exclude_Parameters_When_Required(bool excludeParameters, string queryKey, string headerKey, string expected)
        {
            // ARRANGE
            var method = "GET";
            var resource = "resource";
            var value = new QckMoxRequestString
            {
                Method = method,
                Resource = resource
            };

            value.Queries.Add(queryKey, string.Empty);
            value.Headers.Add(headerKey, string.Empty);

            // ACT
            var actual = value.ToString(excludeParameters: excludeParameters);

            // ASSERT
            actual.Should().Be($"{method} {resource}{expected}");
        }

        [Theory]
        [InlineData("GET", "GET")]
        [InlineData("posT resource", "posT")]
        [InlineData("other query=value", "other")]
        [InlineData("GET resource query=value", "GET")]
        public void TryParse_Should_Parse_Method_Correctly(string value, string expected)
        {
            // ACT
            var result = QckMoxRequestString.TryParse(value, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Method.Should().Be(expected);
        }

        [Theory]
        [InlineData("GET", null)]
        [InlineData("GET resource", "resource")]
        [InlineData("GET query=value", null)]
        [InlineData("GET resource query=value", "resource")]
        [InlineData("GET resourcE/1", "resourcE/1")]
        [InlineData("GET RESOURCE\\1", "RESOURCE\\1")]
        [InlineData("GET re-sou.rce_123%20", "re-sou.rce_123%20")]
        public void TryParse_Should_Parse_Resource_Correctly(string value, string expected)
        {
            // ACT
            var result = QckMoxRequestString.TryParse(value, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Resource.Should().Be(expected);
        }

        [Theory]
        [InlineData("GET", 0)]
        [InlineData("GET resource", 0)]
        [InlineData("GET query=value", 1)]
        [InlineData("GET query1=value1&query2=value2", 2)]
        [InlineData("GET resource query=value", 1)]
        [InlineData("GET resource query1=value1&query2=", 2)]
        [InlineData("GET resource query", 1)]
        [InlineData("GET resource query1&query2=value2", 2)]
        [InlineData("GET resource query=", 1)]
        [InlineData("GET resource query1=&query2=", 2)]
        public void TryParse_Should_Parse_Queries_Correctly_Using_Default_Tag(string value, int expectedCount)
        {
            // ACT
            var result = QckMoxRequestString.TryParse(value, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Queries.Count.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData("GET", "qwe", 0)]
        [InlineData("GET resource", "", 0)]
        [InlineData("GET resource", "   ", 0)]
        [InlineData("GET resource", null, 0)]
        [InlineData("GET resource", "asd", 0)]
        [InlineData("GET query=value", "", 1)]
        [InlineData("GET query=value", "  ", 1)]
        [InlineData("GET query=value", null, 1)]
        [InlineData("GET query=value", "zxc-", 0)]
        [InlineData("GET resource query=value", "   ", 1)]
        [InlineData("GET resource query", "", 1)]
        [InlineData("GET resource query=", null, 1)]
        [InlineData("GET resource query=value", "rty", 0)]
        [InlineData("GET resource query", "fgh", 0)]
        [InlineData("GET resource query=", "vbn", 0)]
        [InlineData("GET qqq-query=value", "qqq-", 1)]
        [InlineData("GET resource aaaquery=value", "aaa", 1)]
        [InlineData("GET resource zzz_query", "zzz_", 1)]
        [InlineData("GET resource www-query=", "www-", 1)]
        [InlineData("GET resource", "res", 1)]
        [InlineData("GET resource sss-query1=1&sss-query2=2", "sss-", 2)]
        [InlineData("GET resource xxx-query1=1&query2=2", "xxx-", 1)]
        public void TryParse_Should_Parse_Queries_Correctly_Using_Tag(string value, string queryTag, int expectedCount)
        {
            // ACT
            var result = QckMoxRequestString.TryParse(value, out var actual, queryTag: queryTag);

            // ASSERT
            result.Should().BeTrue();
            actual.Queries.Count.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData("GET", 0)]
        [InlineData("GET resource", 0)]
        [InlineData("GET header=value", 0)]
        [InlineData("GET qmx-header=value", 1)]
        [InlineData("GET qmx-header1=value1&qmx-header2=value2", 2)]
        [InlineData("GET resource header=value", 0)]
        [InlineData("GET resource qmx-header=value", 1)]
        [InlineData("GET resource qmx-header1=value1&qmx-header2=", 2)]
        [InlineData("GET resource qmx-header", 1)]
        [InlineData("GET resource qmx-header1&qmx-header2=value2", 2)]
        [InlineData("GET resource qmx-header=", 1)]
        [InlineData("GET resource qmx-header1=&qmx-header2=", 2)]
        public void TryParse_Should_Parse_Headers_Correctly_Using_Default_Tag(string value, int expectedCount)
        {
            // ACT
            var result = QckMoxRequestString.TryParse(value, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Headers.Count.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData("GET", "qwe", 0)]
        [InlineData("GET resource", "", 0)]
        [InlineData("GET resource", "   ", 0)]
        [InlineData("GET resource", null, 0)]
        [InlineData("GET resource", "asd", 0)]
        [InlineData("GET header=value", "", 1)]
        [InlineData("GET header=value", "  ", 1)]
        [InlineData("GET header=value", null, 1)]
        [InlineData("GET header=value", "zxc-", 0)]
        [InlineData("GET resource header=value", "   ", 1)]
        [InlineData("GET resource header", "", 1)]
        [InlineData("GET resource header=", null, 1)]
        [InlineData("GET resource header=value", "rty", 0)]
        [InlineData("GET resource header", "fgh", 0)]
        [InlineData("GET resource header=", "vbn", 0)]
        [InlineData("GET qqq-header=value", "qqq-", 1)]
        [InlineData("GET resource aaaheader=value", "aaa", 1)]
        [InlineData("GET resource zzz_header", "zzz_", 1)]
        [InlineData("GET resource www-header=", "www-", 1)]
        [InlineData("GET resource", "res", 1)]
        [InlineData("GET resource sss-header1=1&sss-header2=2", "sss-", 2)]
        [InlineData("GET resource xxx-header1=1&header2=2", "xxx-", 1)]
        public void TryParse_Should_Parse_Headers_Correctly_Using_Tag(string value, string headerTag, int expectedCount)
        {
            // ACT
            var result = QckMoxRequestString.TryParse(value, out var actual, headerTag: headerTag);

            // ASSERT
            result.Should().BeTrue();
            actual.Headers.Count.Should().Be(expectedCount);
        }

        [Theory]
        [InlineData("query", "=value", "value")]
        [InlineData("query", "=", "")]
        [InlineData("query", null, "")]
        public void TryParse_Should_Parse_Query_Values_Correctly(string query, string queryValue, string expectedValue)
        {
            // ARRANGE
            var requestString = $"GET resource {query}{queryValue}";

            // ACT
            var result = QckMoxRequestString.TryParse(requestString, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Queries.Count.Should().Be(1);
            actual.Queries.ContainsKey(query).Should().BeTrue();
            actual.Queries[query].ToString().Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("query=1&query=2", "query", "1,2")]
        [InlineData("query=1,2", "query", "1,2")]
        [InlineData("query=1,2&query=3", "query", "1,2,3")]
        [InlineData("query=1&query=2&query=", "query", "1,2")]
        [InlineData("query=1&query", "query", "1")]
        public void TryParse_Should_Parse_Duplicate_Query_Values_Correctly(string query, string expectedQuery, string expectedValue)
        {
            // ARRANGE
            var requestString = $"GET resource {query}";

            // ACT
            var result = QckMoxRequestString.TryParse(requestString, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Queries.Count.Should().Be(1);
            actual.Queries.ContainsKey(expectedQuery).Should().BeTrue();
            actual.Queries[expectedQuery].ToString().Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("query=1&other=2&query=3", "query", "1,3")]
        [InlineData("other=1&query=2,3", "query", "2,3")]
        [InlineData("query=2&query=1&other=3", "query", "1,2")]
        public void TryParse_Should_Combine_And_Sort_Duplicate_Query_Values(string query, string expectedQuery, string expectedValue)
        {
            // ARRANGE
            var requestString = $"GET resource {query}";

            // ACT
            var result = QckMoxRequestString.TryParse(requestString, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Queries.Count.Should().Be(2);
            actual.Queries.ContainsKey(expectedQuery).Should().BeTrue();
            actual.Queries[expectedQuery].ToString().Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("qmx-header", "=value", "value")]
        [InlineData("qmx-header", "=", "")]
        [InlineData("qmx-header", null, "")]
        public void TryParse_Should_Parse_Header_Values_Correctly(string query, string queryValue, string expectedValue)
        {
            // ARRANGE
            var requestString = $"GET resource {query}{queryValue}";

            // ACT
            var result = QckMoxRequestString.TryParse(requestString, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Headers.Count.Should().Be(1);
            actual.Headers.ContainsKey(query).Should().BeTrue();
            actual.Headers[query].ToString().Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("qmx-header=1&qmx-header=2", "qmx-header", "1,2")]
        [InlineData("qmx-header=1,2", "qmx-header", "1,2")]
        [InlineData("qmx-header=1,2&qmx-header=3", "qmx-header", "1,2,3")]
        [InlineData("qmx-header=1&qmx-header=2&qmx-header=", "qmx-header", "1,2")]
        [InlineData("qmx-header=1&qmx-header", "qmx-header", "1")]
        public void TryParse_Should_Parse_Duplicate_Header_Values_Correctly(string header, string expectedHeader, string expectedValue)
        {
            // ARRANGE
            var requestString = $"GET resource {header}";

            // ACT
            var result = QckMoxRequestString.TryParse(requestString, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Headers.Count.Should().Be(1);
            actual.Headers.ContainsKey(expectedHeader).Should().BeTrue();
            actual.Headers[expectedHeader].ToString().Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("qmx-header=1&qmx-other=2&qmx-header=3", "qmx-header", "1,3")]
        [InlineData("qmx-other=1&qmx-header=2,3", "qmx-header", "2,3")]
        [InlineData("qmx-header=2&qmx-header=1&qmx-other=3", "qmx-header", "1,2")]
        public void TryParse_Should_Combine_And_Sort_Duplicate_Header_Values(string header, string expectedHeader, string expectedValue)
        {
            // ARRANGE
            var requestString = $"GET resource {header}";

            // ACT
            var result = QckMoxRequestString.TryParse(requestString, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Headers.Count.Should().Be(2);
            actual.Headers.ContainsKey(expectedHeader).Should().BeTrue();
            actual.Headers[expectedHeader].ToString().Should().Be(expectedValue);
        }

        [Theory]
        [InlineData("query=value&qmx-header=value", 1, 1)]
        [InlineData("qmx-header=value&query=value", 1, 1)]
        [InlineData("query1=value1&qmx-header=value&query2=value2", 2, 1)]
        [InlineData("qmx-header1=value1&query=value&qmx-header2=value2", 1, 2)]
        [InlineData("query1=value1&query2=value2", 2, 0)]
        [InlineData("qmx-header1=value1&qmx-header2=value2", 0, 2)]
        public void TryParse_Should_Parse_Parameters_Accordingly_Using_Default_Tags(string value, int expectedQueryCount, int expectedHeaderCount)
        {
            // ARRANGE
            var requestString = $"GET resource {value}";

            // ACT
            var result = QckMoxRequestString.TryParse(requestString, out var actual);

            // ASSERT
            result.Should().BeTrue();
            actual.Queries.Count.Should().Be(expectedQueryCount);
            actual.Headers.Count.Should().Be(expectedHeaderCount);
        }

        [Theory]
        [InlineData("qqq-query=value&qmx-header=value", "qqq-", 1, 1)]
        [InlineData("qmx-header=value&qqq-query=value", "qqq-", 1, 1)]
        [InlineData("query1=value1&qmx-header=value&qqq-query2=value2", "qqq-", 1, 1)]
        [InlineData("qmx-header1=value1&query=value&qmx-header2=value2", "qqq-", 0, 2)]
        [InlineData("qqq-query1=value1&qqq-query2=value2", "qqq-", 2, 0)]
        [InlineData("qmx-header1=value1&qmx-header2=value2", "qqq-", 0, 2)]
        [InlineData("qmx-query=value&qmx-header=value", "qmx-", 2, 2)]
        public void TryParse_Should_Parse_Parameters_Accordingly_Using_Query_Tag(string value, string queryTag, int expectedQueryCount, int expectedHeaderCount)
        {
            // ARRANGE
            var requestString = $"GET resource {value}";

            // ACT
            var result = QckMoxRequestString.TryParse(requestString, out var actual, queryTag:queryTag);

            // ASSERT
            result.Should().BeTrue();
            actual.Queries.Count.Should().Be(expectedQueryCount);
            actual.Headers.Count.Should().Be(expectedHeaderCount);
        }

        [Theory]
        [InlineData("query=value&qqq-header=value", "qqq-", 1, 1)]
        [InlineData("qqq-header=value&query=value", "qqq-", 1, 1)]
        [InlineData("query1=value1&qqq-header=value&query2=value2", "qqq-", 2, 1)]
        [InlineData("qqq-header1=value1&query=value&qmx-header2=value2", "qqq-", 2, 1)]
        [InlineData("query1=value1&query2=value2", "qqq-", 2, 0)]
        [InlineData("qqq-header1=value1&qqq-header2=value2", "qqq-", 0, 2)]
        [InlineData("query=value&header=value", "", 2, 2)]
        [InlineData("query=value&header=value", "   ", 2, 2)]
        [InlineData("query=value&header=value", null, 2, 2)]
        public void TryParse_Should_Parse_Parameters_Accordingly_Using_Header_Tag(string value, string headerTag, int expectedQueryCount, int expectedHeaderCount)
        {
            // ARRANGE
            var requestString = $"GET resource {value}";

            // ACT
            var result = QckMoxRequestString.TryParse(requestString, out var actual, headerTag:headerTag);

            // ASSERT
            result.Should().BeTrue();
            actual.Queries.Count.Should().Be(expectedQueryCount);
            actual.Headers.Count.Should().Be(expectedHeaderCount);
        }

        [Theory]
        [InlineData("http://localhost:8080/api/resource?query=value&qmx-header=value")]
        [InlineData("GET http://localhost:8080/api/resource?query=value&qmx-header=value")]
        [InlineData("GET http://localhost:8080/api/resource ?query=value&qmx-header=value")]
        [InlineData("GET http://localhost:8080/api/resource query=value&qmx-header=value")]
        [InlineData("GET resource?query=value&qmx-header=value")]
        [InlineData("GET resource ?query=value&qmx-header=value")]
        [InlineData("GET ?query=value&qmx-header=value")]
        [InlineData("query=value&qmx-header=value")]
        [InlineData("?query=value&qmx-header=value")]
        [InlineData("GET123 resource query=value&qmx-header=value")]
        [InlineData("GET resource query=value qmx-header=value")]
        [InlineData("GET resource query=parent\\value&qmx-header=value")]
        [InlineData("GET resource query=parent/value&qmx-header=value")]
        public void TryParse_Should_Handle_Invalid_Request_Strings(string value)
        {
            // ACT
            var result = QckMoxRequestString.TryParse(value, out var actual);

            // ASSERT
            result.Should().BeFalse();
            actual.Should().BeNull();
        }

        [Theory]
        [InlineData("GET", "GET", true)]
        [InlineData("POST", "post", true)]
        [InlineData("GET", "NOTGET", false)]
        [InlineData("", "", true)]
        [InlineData("   ", "   ", true)]
        [InlineData("", "   ", false)]
        public void Overloaded_Equals_Should_Check_Method(string method1, string method2, bool expected)
        {
            // ARRANGE
            var string1 = new QckMoxRequestString { Method = method1 };
            var string2 = new QckMoxRequestString { Method = method2 };

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }

        [Theory]
        [InlineData("resource", "resource", true)]
        [InlineData("path/resource", "PATH/resource", true)]
        [InlineData("resource", "other/resource", false)]
        [InlineData("", "", true)]
        [InlineData("   ", "   ", true)]
        [InlineData("", "   ", false)]
        public void Overloaded_Equals_Should_Check_Resource(string resource1, string resource2, bool expected)
        {
            // ARRANGE
            var string1 = new QckMoxRequestString { Resource = resource1 };
            var string2 = new QckMoxRequestString { Resource = resource2 };

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(0, 0, true)]
        [InlineData(99, 99, true)]
        [InlineData(50, 51, false)]
        public void Overloaded_Equals_Should_Check_Query(int queryCount1, int queryCount2, bool expected)
        {
            // ARRANGE
            var maxCount = System.Math.Max(queryCount1, queryCount2);
            var values = new Dictionary<int, StringValues>();
            for(var i = 0; i < maxCount; i++)
            {
                values.Add(i, new StringValues($"{Guid.NewGuid():N}"));
            }

            var string1 = new QckMoxRequestString();
            for(var i = 0; i < queryCount1; i++)
            {
                string1.Queries.Add($"{i}", values[i]);
            }

            var string2 = new QckMoxRequestString();
            for(var i = 0; i < queryCount2; i++)
            {
                string2.Queries.Add($"{i}", values[i]);
            }

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }

        [Theory]
        [InlineData("key", "key", true)]
        [InlineData("KEY", "kEy", true)]
        [InlineData("key", "other", false)]
        public void Overloaded_Equals_Should_Check_Query_Key(string key1, string key2, bool expected)
        {
            // ARRANGE
            var value = $"{Guid.NewGuid():N}";

            var string1 = new QckMoxRequestString();
            string1.Queries.Add(key1, new StringValues(value));

            var string2 = new QckMoxRequestString();
            string2.Queries.Add(key2, new StringValues(value));

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }

        [Theory]
        [InlineData("value", "value", true)]
        [InlineData("value", "VALUE", false)]
        [InlineData("value", "other", false)]
        public void Overloaded_Equals_Should_Check_Query_Value(string value1, string value2, bool expected)
        {
            // ARRANGE
            var key = $"{Guid.NewGuid():N}";

            var string1 = new QckMoxRequestString();
            string1.Queries.Add(key, new StringValues(value1));

            var string2 = new QckMoxRequestString();
            string2.Queries.Add(key, new StringValues(value2));

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }

        [Theory]
        [InlineData(new [] {"value1", "value2"}, new [] {"value2", "value1"}, true)]
        [InlineData(new [] {"value1", "value2"}, new [] {"value1", "value2"}, true)]
        [InlineData(new [] {"value1", "value2"}, new [] {"value1", "value3"}, false)]
        public void Overloaded_Equals_Should_Check_Query_Values(string[] values1, string[] values2, bool expected)
        {
            // ARRANGE
            var key = $"{Guid.NewGuid():N}";

            var string1 = new QckMoxRequestString();
            string1.Queries.Add(key, new StringValues(values1));

            var string2 = new QckMoxRequestString();
            string2.Queries.Add(key, new StringValues(values2));

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(0, 0, true)]
        [InlineData(99, 99, true)]
        [InlineData(50, 51, false)]
        public void Overloaded_Equals_Should_Check_Header(int headerCount1, int headerCount2, bool expected)
        {
            // ARRANGE
            var maxCount = System.Math.Max(headerCount1, headerCount2);
            var values = new Dictionary<int, StringValues>();
            for(var i = 0; i < maxCount; i++)
            {
                values.Add(i, new StringValues($"{Guid.NewGuid():N}"));
            }

            var string1 = new QckMoxRequestString();
            for(var i = 0; i < headerCount1; i++)
            {
                string1.Headers.Add($"{i}", values[i]);
            }

            var string2 = new QckMoxRequestString();
            for(var i = 0; i < headerCount2; i++)
            {
                string2.Headers.Add($"{i}", values[i]);
            }

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }

        [Theory]
        [InlineData("key", "key", true)]
        [InlineData("KEY", "kEy", true)]
        [InlineData("key", "other", false)]
        public void Overloaded_Equals_Should_Check_Header_Key(string key1, string key2, bool expected)
        {
            // ARRANGE
            var value = $"{Guid.NewGuid():N}";

            var string1 = new QckMoxRequestString();
            string1.Headers.Add(key1, new StringValues(value));

            var string2 = new QckMoxRequestString();
            string2.Headers.Add(key2, new StringValues(value));

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }

        [Theory]
        [InlineData("value", "value", true)]
        [InlineData("value", "VALUE", false)]
        [InlineData("value", "other", false)]
        public void Overloaded_Equals_Should_Check_Header_Value(string value1, string value2, bool expected)
        {
            // ARRANGE
            var key = $"{Guid.NewGuid():N}";

            var string1 = new QckMoxRequestString();
            string1.Headers.Add(key, new StringValues(value1));

            var string2 = new QckMoxRequestString();
            string2.Headers.Add(key, new StringValues(value2));

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }

        [Theory]
        [InlineData(new [] {"value1", "value2"}, new [] {"value2", "value1"}, true)]
        [InlineData(new [] {"value1", "value2"}, new [] {"value1", "value2"}, true)]
        [InlineData(new [] {"value1", "value2"}, new [] {"value1", "value3"}, false)]
        public void Overloaded_Equals_Should_Check_Header_Values(string[] values1, string[] values2, bool expected)
        {
            // ARRANGE
            var key = $"{Guid.NewGuid():N}";

            var string1 = new QckMoxRequestString();
            string1.Headers.Add(key, new StringValues(values1));

            var string2 = new QckMoxRequestString();
            string2.Headers.Add(key, new StringValues(values2));

            // ACT
            var actualEquality = string1 == string2;
            var actualInequality = string1 != string2;

            // ASSERT
            actualEquality.Should().Be(expected);
            actualInequality.Should().Be(!expected);
        }
    }
}