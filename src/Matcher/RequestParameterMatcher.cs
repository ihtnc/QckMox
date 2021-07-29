using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;
using QckMox.IO;
using QckMox.Request;

namespace QckMox.Matcher
{
    internal class RequestParameterMatcher : QckMoxMatcher
    {

        public RequestParameterMatcher(IQckMoxConfigurationProvider config, IFileProvider file, IQckMoxRequestConverter converter) : base(config, file, converter)
        { }

        public override async Task<QckMoxMatchResult> Match(HttpRequest request, QckMoxConfig requestConfig)
        {
            var config = requestConfig;

            var shouldMatchQueryString = config.Request?.MatchQuery?.Any() is true;
            var shouldMatchHeader = config.Request?.MatchHeader?.Any() is true;
            var shouldMatchParameters = shouldMatchQueryString || shouldMatchHeader;
            if (config.Disabled is true || shouldMatchParameters is false)
            {
                return NoMatchResult;
            }

            var result = await FindMatch(request, config, false);

            var shouldHandleUnmatched = config.Request?.UnmatchedRequest?.MatchHttpMethod is true;
            if (result.MatchFound is false && shouldHandleUnmatched is true)
            {
                result = await FindMatch(request, config, true);
            }

            return result;
        }

        private async Task<QckMoxMatchResult> FindMatch(HttpRequest request, QckMoxConfig requestConfig, bool excludeParameters)
        {
            var responseFile = GetResponseFile(request, requestConfig, excludeResource: true, excludeParameters: excludeParameters);
            var mockPath = Converter.GetResourceString(request);

            var filePath = Path.Combine(GlobalConfig.ResponseSource, mockPath, responseFile);
            var result = await GetMatchResult(filePath, requestConfig.Response);
            return result;
        }
    }
}