using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QckMox.Configs;

namespace QckMox
{
    public interface IQckMoxResponseFileProvider
    {
        QckMoxResponse GetResponse(HttpRequest request);
    }

    public class QckMoxResponseFileProvider : IQckMoxResponseFileProvider
    {
        private readonly IQckMoxConfigurationProvider _config;
        private readonly IFileProvider _file;
        private readonly QckMoxAppConfig _global;

        public QckMoxResponseFileProvider(IQckMoxConfigurationProvider config, IFileProvider file)
        {
            _config = config;
            _global = config.GetGlobalConfig();
            _file = file;
        }

        public QckMoxResponse GetResponse(HttpRequest request)
        {
            var requestConfig = GetRequestConfig(request);
            if(requestConfig.Disabled is true) { return QckMoxResponse.RedirectResponse; }

            var matchResult = MatchAgainstMap(request, requestConfig);

            // otherwise, try to match a response file from a query string
            if(matchResult.Content is null)
            {
                matchResult = MatchAgainstHttpMethod(request, requestConfig, true);
            }

            // otherwise, if applicable, try to match just to the http method
            // this is to ensure that only requests with query strings get matched
            //   since those without query strings are handled on the previous step already
            var hasQueryString = request.Query.Any();
            var matchHttpMethod = requestConfig.Request.UnmatchedRequest.MatchHttpMethod;
            if(matchResult.Content is null && hasQueryString && matchHttpMethod)
            {
                matchResult = MatchAgainstHttpMethod(request, requestConfig, false);
            }

            var fileConfig = matchResult.Config.Merge(requestConfig.Response);
            if(matchResult.Content is null)
            {
                var isPassthrough = requestConfig.Request.UnmatchedRequest.Passthrough;
                return isPassthrough
                    ? QckMoxResponse.RedirectResponse
                    : QckMoxResponse.GetNotFoundResponse(fileConfig);
            }

            var objFileContent = JToken.Parse(matchResult.Content);
            if(objFileContent.Type != JTokenType.Object) { return null; }

            var json = GetContent((JObject)objFileContent, requestConfig, fileConfig);
            var response = GetContentStream(json, fileConfig);

            return QckMoxResponse.GetSuccessResponse(response, fileConfig);
        }

        private ResponseFileMatchResult MatchAgainstMap(HttpRequest request, QckMoxConfig requestConfig)
        {
            // try to match a response file from a map config
            var responseMapFile = GetResponseMapFile(request, requestConfig);

            return GetMatchResult(responseMapFile, requestConfig);
        }

        private ResponseFileMatchResult MatchAgainstHttpMethod(HttpRequest request, QckMoxConfig requestConfig, bool includeQueryString)
        {
            var methodString = GetMapMethodString(request);
            var queryString = includeQueryString ? GetMapQueryString(request, requestConfig) : string.Empty;
            var requestString = $"{methodString} {queryString.TrimStart('?')}".Trim();
            var responseFile = $"{requestString}.json";
            var mockPath = GetMockPath(request);
            var filePath = Path.Combine(_global.ResponseSource, mockPath, responseFile);

            return GetMatchResult(filePath, requestConfig);
        }

        private ResponseFileMatchResult GetMatchResult(string filePath, QckMoxConfig requestConfig)
        {
            var fileConfig = _config.GetResponseFileConfig(filePath, requestConfig);
            var fileContent = _file.GetContent(filePath);

            return new ResponseFileMatchResult
            {
                Config = fileConfig,
                Content = fileContent
            };
        }

        private JToken GetContent(JObject fileContent, QckMoxConfig requestConfig, QckMoxResponseConfig fileConfig)
        {
            JToken json;

            if(fileConfig.ContentInProp is true)
            {
                var hasKey = fileContent.ContainsKey(fileConfig.FileContentProp);
                json = hasKey ? fileContent[fileConfig.FileContentProp] : null;
            }
            else
            {
                var copy = (JObject)fileContent.DeepClone();
                copy.Remove(QckMoxResponseConfig.CONFIG_KEY);
                json = copy;
            }

            return json;
        }

        private Stream GetContentStream(JToken content, QckMoxResponseFileConfig fileConfig)
        {
            byte[] response;
            if(fileConfig?.Base64Content is true)
            {
                var base64String = content.Value<string>();
                response = Convert.FromBase64String(base64String);
            }
            else
            {
                var contentString = JsonConvert.SerializeObject(content);
                response = Encoding.UTF8.GetBytes(contentString);
            }

            return new MemoryStream(response);
        }

        private string GetResponseMapFile(HttpRequest request, QckMoxConfig requestConfig)
        {
            var config = requestConfig;
            var requestString = GetRequestString(request, config);

            var responseFile = string.Empty;
            if(config.ResponseMap.ContainsKey(requestString))
            {
                responseFile = config.ResponseMap[requestString];
            }

            return responseFile.Trim();
        }

        private string GetMockPath(HttpRequest request)
        {
            return request.Path.Value.Replace(_global.EndPoint, string.Empty, StringComparison.OrdinalIgnoreCase);
        }

        private QckMoxConfig GetRequestConfig(HttpRequest request)
        {
            var mockPath = GetMockPath(request);
            return _config.GetRequestConfig(mockPath);
        }

        private string GetRequestString(HttpRequest request, QckMoxConfig requestConfig)
        {
            var config = requestConfig;

            var mapMethodString = GetMapMethodString(request);
            var mapUriString = GetMockPath(request);
            var mapQueryString = GetMapQueryString(request, config);

            var requestString = $"{mapMethodString} {mapUriString}{mapQueryString}";
            return requestString;
        }

        private static string GetMapMethodString(HttpRequest request)
        {
            return request.Method.ToUpper();
        }

        private static string GetMapQueryString(HttpRequest request, QckMoxConfig requestConfig)
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

        private class ResponseFileMatchResult
        {
            public QckMoxResponseFileConfig Config { get; set; }
            public string Content { get; set; }
        }
    }
}