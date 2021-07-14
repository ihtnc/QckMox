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
            if(requestConfig.Disabled == true) { return null; }

            var responseMapFile = GetResponseMapFile(request, requestConfig);
            var fileConfig = _config.GetResponseFileConfig(responseMapFile, requestConfig);
            var fileContent = _file.GetContent(responseMapFile);
            if(fileContent == null)
            {
                var methodString = GetMapMethodString(request);
                var queryString = GetMapQueryString(request, requestConfig);
                var requestString = $"{methodString} {queryString.TrimStart('?')}".Trim();
                var responseFile = $"{requestString}.json";
                var mockPath = GetMockPath(request);
                var filePath = Path.Combine(_global.ResponseSource, mockPath, responseFile);

                fileConfig = _config.GetResponseFileConfig(filePath, requestConfig);
                fileContent = _file.GetContent(filePath);
            }

            if(fileContent == null && requestConfig.Request.RedirectUnmatched)
            {
                var requestString = GetMapMethodString(request);
                var responseFile = $"{requestString}.json";
                var mockPath = GetMockPath(request);
                var filePath = Path.Combine(_global.ResponseSource, mockPath, responseFile);

                fileConfig = _config.GetResponseFileConfig(filePath, requestConfig);
                fileContent = _file.GetContent(filePath);
            }

            if(fileContent == null) { return null; }

            var objFileContent = JObject.Parse(fileContent);
            if(objFileContent.Type != JTokenType.Object) { return null; }

            fileConfig = fileConfig.Merge(requestConfig.Response);
            var json = GetContent(objFileContent, requestConfig, fileConfig);
            var response = GetContentStream(json, fileConfig);

            return new QckMoxResponse
            {
                ContentType = fileConfig.ContentType,
                Headers = fileConfig.Headers,
                Content = response
            };
        }

        private JToken GetContent(JObject fileContent, QckMoxConfig requestConfig, QckMoxResponseConfig fileConfig)
        {
            JToken json;

            if(fileConfig.ContentInProp == true)
            {
                var hasKey = fileContent.ContainsKey(fileConfig.FileContentProp);
                json = hasKey ? fileContent[fileConfig.FileContentProp] : null;
            }
            else
            {
                var copy = (JObject)fileContent.DeepClone();
                copy.Remove(requestConfig.FileConfigProp);
                json = copy;
            }

            return json;
        }

        private Stream GetContentStream(JToken content, QckMoxResponseFileConfig fileConfig)
        {
            byte[] response;
            if(fileConfig?.Base64Content == true)
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

            if(requestConfig.Request.MatchQuery?.Any() == true)
            {
                foreach(var query in requestConfig.Request.MatchQuery)
                {
                    if(!request.Query.ContainsKey(query)) { continue; }

                    parts.Add($"{requestConfig.Request.QueryMapPrefix}{query}={request.Query[query]}");
                }
            }

            if(requestConfig.Request.MatchHeader?.Any() == true)
            {
                foreach(var header in requestConfig.Request.MatchHeader)
                {
                    if(!request.Headers.ContainsKey(header)) { continue; }

                    parts.Add($"{requestConfig.Request.HeaderMapPrefix}{header}={request.Headers[header]}");
                }
            }

            if(!parts.Any()) { return string.Empty; }

            var queryString = string.Join('&', parts);
            return $"?{queryString}";
        }
    }
}