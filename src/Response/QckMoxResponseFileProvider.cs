using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;
using QckMox.Matcher;

namespace QckMox.Response
{
    internal interface IQckMoxResponseFileProvider
    {
        Task<QckMoxResponse> GetResponse(HttpRequest request);
    }

    internal class QckMoxResponseFileProvider : IQckMoxResponseFileProvider
    {
        private readonly IQckMoxConfigurationProvider _config;
        private readonly IQckMoxDefaultMatchResultHandler _defaultMatcher;
        private readonly IQckMoxCustomMatchResultHandler _customMatcher;

        public QckMoxResponseFileProvider(IQckMoxConfigurationProvider config, IQckMoxDefaultMatchResultHandler defaultMatcher, IQckMoxCustomMatchResultHandler customMatcher)
        {
            _config = config;
            _defaultMatcher = defaultMatcher;
            _customMatcher = customMatcher;
        }

        public async Task<QckMoxResponse> GetResponse(HttpRequest request)
        {
            var requestConfig = null as QckMoxConfig;

            try
            {
                requestConfig = await _config.GetRequestConfig(request);
                if(requestConfig.Disabled is true) { return QckMoxResponse.RedirectResponse; }

                var matchResult = await _customMatcher.MatchResponse(request, requestConfig);
                if (matchResult.MatchFound is false)
                {
                    matchResult = await _defaultMatcher.MatchResponse(request, requestConfig);
                }

                if(matchResult.MatchFound is false)
                {
                    var isPassthrough = requestConfig.Request.UnmatchedRequest.Passthrough;
                    return isPassthrough is true
                        ? QckMoxResponse.RedirectResponse
                        : QckMoxResponse.GetNotFoundResponse(requestConfig.Response.Headers);
                }

                return QckMoxResponse.GetSuccessResponse(matchResult.Content, matchResult.ContentType, matchResult.ResponseHeaders);
            }
            catch
            {
                var globalConfig = await _config.GetGlobalConfig();
                var headers = requestConfig?.Response?.Headers
                    ?? globalConfig.Response?.Headers
                    ?? new Dictionary<string, string>();

                return QckMoxResponse.GetErrorResponse(headers);
            }
        }
    }
}