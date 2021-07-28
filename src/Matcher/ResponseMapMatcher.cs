using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;
using QckMox.IO;

namespace QckMox.Matcher
{
    internal class ResponseMapMatcher : QckMoxMatcher
    {

        public ResponseMapMatcher(IQckMoxConfigurationProvider config, IFileProvider file) : base(config, file)
        { }

        public override async Task<QckMoxMatchResult> Match(HttpRequest request, QckMoxConfig requestConfig)
        {
            var config = requestConfig;

            if (config.Disabled is true || config.ResponseMap?.Any() is not true)
            {
                return NoMatchResult;
            }

            var requestString = GetRequestString(request, config);

            var responseFile = string.Empty;
            if(config.ResponseMap.ContainsKey(requestString))
            {
                responseFile = config.ResponseMap[requestString].Trim();
            }

            return await GetMatchResult(responseFile, config.Response);
        }
    }
}