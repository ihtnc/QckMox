using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;

namespace QckMox.Matcher
{
    internal interface IQckMoxCustomMatchResultHandler : IQckMoxMatchResultHandler
    { }

    internal class QckMoxCustomMatchResultHandler : IQckMoxCustomMatchResultHandler
    {
        private readonly IQckMoxConfigurationProvider _config;
        private readonly IReadOnlyCollection<IQckMoxCustomMatcher> _matchers;

        public QckMoxCustomMatchResultHandler(IQckMoxConfigurationProvider config, IEnumerable<IQckMoxCustomMatcher> matchers)
        {
            _config = config;
            _matchers = matchers?.ToArray();
        }

        public async Task<QckMoxMatchResult> MatchResponse(HttpRequest request, QckMoxConfig config)
        {
            foreach(var matcher in _matchers)
            {
                var copy = config.Copy();
                var matchResult = await matcher.Match(request, copy);
                if(matchResult.MatchFound is true)
                {
                    return await GetMatchResult(matchResult, copy.Response);
                }
            }

            return QckMoxMatcher.NoMatchResult;
        }

        public async Task<QckMoxMatchResult> GetMatchResult(QckMoxMatchResult customMatch, QckMoxResponseConfig responseConfig)
        {
            var fileContent = customMatch.Content;
            var response = QckMoxResponseFileConfig.Copy(responseConfig);
            var config = await _config.GetResponseStreamConfig(fileContent);
            config = response.Merge(config);
            var content = await QckMoxMatchResultHelper.GetResponseContent(fileContent, config);

            return new QckMoxMatchResult
            {
                ContentType = config.ContentType,
                ResponseHeaders = config.Headers,
                Content = content
            };
        }
    }
}