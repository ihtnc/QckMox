using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;
using QckMox.IO;
using QckMox.Request;

namespace QckMox.Matcher
{
    internal class RequestMethodMatcher : QckMoxMatcher
    {
        public RequestMethodMatcher(IQckMoxConfigurationProvider config, IIOProvider io, IFileProvider file, IQckMoxRequestConverter converter) : base(config, io, file, converter)
        { }

        public override async Task<QckMoxMatchResult> Match(HttpRequest request, QckMoxConfig requestConfig)
        {
            var config = requestConfig;
            var globalConfig = GlobalConfig;

            var shouldMatchQueryString = config.Request?.MatchQuery?.Any() is true;
            var shouldMatchHeader = config.Request?.MatchHeader?.Any() is true;
            var shouldMatchParameters = shouldMatchQueryString || shouldMatchHeader;
            if (config.Disabled is true || shouldMatchParameters is true)
            {
                return NoMatchResult;
            }

            var filePath = GetResponseFile(request, requestConfig, excludeResource: true, excludeParameters: true);
            return await GetMatchResult(filePath, requestConfig.Response);
        }
    }
}