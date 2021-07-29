using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;

namespace QckMox.Request
{
    internal interface IQckMoxRequestConverter
    {
        QckMoxRequestString ToRequestString(HttpRequest request, QckMoxConfig requestConfig, bool excludeResource = false, bool excludeParameters = false);
        string GetMethodString(HttpRequest request);
        string GetResourceString(HttpRequest request);
        string GetParameterString(HttpRequest request, QckMoxConfig requestConfig);
    }

    internal class QckMoxRequestConverter: IQckMoxRequestConverter
    {
        private readonly QckMoxAppConfig _globalConfig;

        public QckMoxRequestConverter(IQckMoxConfigurationProvider config)
        {
            var task = config.GetGlobalConfig();
            task.Wait();
            _globalConfig = task.Result;
        }

        public QckMoxRequestString ToRequestString(HttpRequest request, QckMoxConfig requestConfig, bool excludeResource = false, bool excludeParameters = false)
        {
            var config = requestConfig;

            var methodString = GetMethodString(request);
            var uriString = excludeResource is true ? null : GetResourceString(request).Replace('\\', '/');
            var parameterString = excludeParameters is true ? null : GetParameterString(request, config);

            var requestString = string.Join(' ', methodString, uriString, parameterString);
            var parsed = QckMoxRequestString.Parse(requestString, config?.Request?.QueryTag, config?.Request?.HeaderTag);
            return parsed;
        }

        public string GetMethodString(HttpRequest request)
        {
            return request.Method.ToUpper();
        }

        public string GetResourceString(HttpRequest request)
        {
            return request.Path.Value.Replace(_globalConfig.EndPoint, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        public string GetParameterString(HttpRequest request, QckMoxConfig requestConfig)
        {
            var parts = new List<string>();

            if(requestConfig.Request.MatchQuery?.Any() is true)
            {
                foreach(var query in requestConfig.Request.MatchQuery)
                {
                    if(!request.Query.ContainsKey(query)) { continue; }

                    parts.Add($"{requestConfig.Request.QueryTag}{query}={request.Query[query]}");
                }
            }

            if(requestConfig.Request.MatchHeader?.Any() is true)
            {
                foreach(var header in requestConfig.Request.MatchHeader)
                {
                    if(!request.Headers.ContainsKey(header)) { continue; }

                    parts.Add($"{requestConfig.Request.HeaderTag}{header}={request.Headers[header]}");
                }
            }

            if(parts.Any() is false) { return string.Empty; }

            var parameterString = string.Join('&', parts);
            return parameterString;
        }
    }
}