using System.IO;
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

        public RequestMethodMatcher(IQckMoxConfigurationProvider config, IFileProvider file, IQckMoxRequestConverter converter) : base(config, file, converter)
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

            var responseFile = GetResponseFile(request, requestConfig, excludeResource: true, excludeParameters: true);
            var mockPath = Converter.GetResourceString(request);
            var filePath = Path.Combine(globalConfig.ResponseSource, mockPath, responseFile);
            return await GetMatchResult(filePath, requestConfig.Response);
        }
    }
}