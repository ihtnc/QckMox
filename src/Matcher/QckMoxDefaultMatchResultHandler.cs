using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace QckMox.Matcher
{
    internal interface IQckMoxDefaultMatchResultHandler : IQckMoxMatchResultHandler
    { }

    internal class QckMoxDefaultMatchResultHandler : IQckMoxDefaultMatchResultHandler
    {
        private readonly Queue<IQckMoxMatcher> _matchers;

        public QckMoxDefaultMatchResultHandler(IEnumerable<IQckMoxMatcher> defaultMatchers)
        {
            _matchers = SortMatchers(defaultMatchers);
        }

        public async Task<QckMoxMatchResult> MatchResponse(HttpRequest request)
        {
            foreach(var matcher in _matchers)
            {
                var matchResult = await matcher.Match(request);
                if(matchResult.MatchFound is true)
                {
                    return matchResult;
                }
            }

            return  QckMoxMatcher.NoMatchResult;
        }

        private Queue<IQckMoxMatcher> SortMatchers(IEnumerable<IQckMoxMatcher> matchers)
        {
            var queue = new Queue<IQckMoxMatcher>();

            var responseMap = matchers.SingleOrDefault(m => m is ResponseMapMatcher);
            var requestParameter = matchers.SingleOrDefault(m => m is RequestParameterMatcher);
            var requestMethod = matchers.SingleOrDefault(m => m is RequestMethodMatcher);

            if (responseMap is null || requestParameter is null || requestMethod is null)
            {
                throw new NotImplementedException("Built-in matchers not found.");
            }

            queue.Enqueue(responseMap);
            queue.Enqueue(requestParameter);
            queue.Enqueue(requestMethod);

            return queue;
        }
    }
}