using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;
using QckMox.IO;

namespace QckMox.Matcher
{
    internal interface IQckMoxMatcher
    {
        Task<QckMoxMatchResult> Match(HttpRequest request);
    }

    internal abstract class QckMoxMatcher : IQckMoxMatcher
    {
        private readonly IQckMoxConfigurationProvider _config;
        private readonly IFileProvider _file;

        public QckMoxMatcher(IQckMoxConfigurationProvider config, IFileProvider file)
        {
            _config = config;

            var task = config.GetGlobalConfig();
            task.Wait();
            GlobalConfig = task.Result;

            _file = file;
        }

        public QckMoxAppConfig GlobalConfig { get; }

        public virtual async Task<QckMoxMatchResult> Match(HttpRequest request)
        {
            var requestConfig = await _config.GetRequestConfig(request);
            return await InternalMatch(request, requestConfig);
        }

        protected internal abstract Task<QckMoxMatchResult> InternalMatch(HttpRequest request, QckMoxConfig requestConfig);

        protected internal async Task<QckMoxMatchResult> GetMatchResult(string filePath, QckMoxResponseConfig responseConfig)
        {
            var config = await _config.GetResponseFileConfig(filePath);
            config = config.Merge(responseConfig);
            var fileContent = await _file.GetStreamContent(filePath);
            var content = fileContent is not null
                ? await QckMoxMatchResultHelper.GetResponseContent(fileContent, config)
                : null;

            return new QckMoxMatchResult
            {
                ContentType = config.ContentType,
                ResponseHeaders = config.Headers,
                Content = content
            };
        }

        protected internal string GetResponseFile(HttpRequest request, QckMoxConfig requestConfig, bool excludeResource = false, bool excludeParameters = false)
        {
            var requestString = GetRequestString(request, requestConfig, excludeResource: excludeResource, excludeParameters: excludeParameters);
            return $"{requestString}.json";
        }

        protected internal string GetRequestString(HttpRequest request, QckMoxConfig requestConfig, bool excludeResource = false, bool excludeParameters = false)
        {
            var config = requestConfig;

            var methodString = GetMethodString(request);
            var uriString = excludeResource is true ? string.Empty : GetResourceString(request).Replace('\\', '/');
            var parameterString = excludeParameters is true ? string.Empty : GetParameterString(request, config);

            var requestString = $"{methodString} {uriString}{parameterString}";
            return requestString.Trim();
        }

        protected internal static string GetMethodString(HttpRequest request)
        {
            return request.Method.ToUpper();
        }

        protected internal string GetResourceString(HttpRequest request)
        {
            return request.Path.Value.Replace(GlobalConfig.EndPoint, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        protected internal static string GetParameterString(HttpRequest request, QckMoxConfig requestConfig)
        {
            var parts = new List<string>();

            if(requestConfig.Request.MatchQuery?.Any() is true)
            {
                foreach(var query in requestConfig.Request.MatchQuery)
                {
                    if(!request.Query.ContainsKey(query)) { continue; }

                    parts.Add($"{requestConfig.Request.QueryMapPrefix}{query}={request.Query[query]}");
                }
            }

            if(requestConfig.Request.MatchHeader?.Any() is true)
            {
                foreach(var header in requestConfig.Request.MatchHeader)
                {
                    if(!request.Headers.ContainsKey(header)) { continue; }

                    parts.Add($"{requestConfig.Request.HeaderMapPrefix}{header}={request.Headers[header]}");
                }
            }

            if(parts.Any() is false) { return string.Empty; }

            var queryString = string.Join('&', parts);
            return $"?{queryString}";
        }

        internal static QckMoxMatchResult NoMatchResult => new QckMoxMatchResult { Content = null };
    }
}