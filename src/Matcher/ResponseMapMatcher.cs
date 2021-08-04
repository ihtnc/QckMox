using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;
using QckMox.IO;
using QckMox.Request;

namespace QckMox.Matcher
{
    internal class ResponseMapMatcher : QckMoxMatcher
    {

        public ResponseMapMatcher(IQckMoxConfigurationProvider config, IIOProvider io, IFileProvider file, IQckMoxRequestConverter converter) : base(config, io, file, converter)
        { }

        public override async Task<QckMoxMatchResult> Match(HttpRequest request, QckMoxConfig requestConfig)
        {
            var config = requestConfig;

            if (config.Disabled is true || config.ResponseMap?.Any() is not true)
            {
                return NoMatchResult;
            }

            var converted = Converter.ToRequestString(request, config);
            var requestString = $"{converted}";

            var responseFile = string.Empty;
            if(config.ResponseMap.ContainsKey(requestString))
            {
                var match = config.ResponseMap[requestString].Trim();
                responseFile = IOProvider.PathResolver.ResolveFilePath(match);
            }
            else
            {
                responseFile = FindMatch(config.ResponseMap, converted, config.Request?.QueryTag, config.Request?.HeaderTag);
                if (string.IsNullOrWhiteSpace(responseFile)) { responseFile = string.Empty; }
            }

            return await GetMatchResult(responseFile, config.Response);
        }

        private string FindMatch(IReadOnlyDictionary<string, string> map, QckMoxRequestString requestString, string queryTag, string headerTag)
        {
            var path = null as string;
            var matchingkey = map.Keys.SingleOrDefault(k =>
            {
                var invalid = QckMoxRequestString.TryParse(k, out var converted, queryTag: queryTag, headerTag: headerTag) is false;
                if (invalid is true) { return false; }

                return converted.Equals(requestString);
            });

            if (string.IsNullOrWhiteSpace(matchingkey) is true) { return string.Empty; }

            var match = map[matchingkey];
            return IOProvider.PathResolver.ResolveFilePath(match);
        }
    }
}