using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QckMox.IO;

namespace QckMox.Configuration
{
    internal interface IQckMoxConfigurationProvider
    {
        Task<QckMoxAppConfig> GetGlobalConfig();
        Task<QckMoxConfig> GetRequestConfig(HttpRequest request);
        Task<QckMoxResponseFileConfig> GetResponseFileConfig(string filePath);
        Task<QckMoxResponseFileConfig> GetResponseStreamConfig(Stream fileContent);
    }

    internal class QckMoxConfigurationProvider : IQckMoxConfigurationProvider
    {
        private static readonly Regex _responseMapkeyMatcher = new Regex("([A-Za-z]+)[ ]+(.*)");

        private readonly QckMoxAppConfig _config;
        private readonly IIOProvider _io;
        private readonly IFileProvider _file;

        public QckMoxConfigurationProvider(IOptions<QckMoxAppConfig> global, IIOProvider io, IFileProvider file)
        {
            _io = io;
            _file = file;

            var appConfig = global.Value;
            var defaultConfig = QckMoxAppConfig.GetDefaultValues();
            var config = defaultConfig.Merge(appConfig);
            _config = ResolveResponseMapPaths(config);
        }

        public async Task<QckMoxAppConfig> GetGlobalConfig()
        {
            return await Task.FromResult(_config);
        }

        public async Task<QckMoxConfig> GetRequestConfig(HttpRequest request)
        {
            var mockRequestPath = request.Path.Value.Replace(_config.EndPoint, string.Empty, StringComparison.OrdinalIgnoreCase);

            // merge all folder configs referenced by the mockRequestPath
            var breadCrumb = string.Empty;
            var queue = new Queue<string>();
            queue.Enqueue(breadCrumb);

            QckMoxConfig config = _config;

            var uris = mockRequestPath.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var uri in uris)
            {
                breadCrumb = _io.Path.Combine(breadCrumb, uri);
                queue.Enqueue(breadCrumb);
            }

            while(queue.Count > 0)
            {
                var uri = queue.Dequeue();
                var temp = await GetUriConfig(uri);
                config = temp == null ? config : config.Merge(temp);
            }

            return config;
        }

        private async Task<QckMoxConfig> GetUriConfig(string requestUri)
        {
            var configFile = _io.Path.Combine(requestUri, QckMoxConfig.FOLDER_CONFIG_FILE);
            var configFilePath = _io.Path.Combine(_config.ResponseSource, configFile);
            var content = await _file.GetContent(configFilePath);
            if(content is null) { return null; }

            var obj = JObject.Parse(content);
            var config = obj.ToObject<QckMoxConfig>();
            config = ResolveResponseMapPaths(config, requestUri, _config.ResponseSource);
            return config;
        }

        public async Task<QckMoxResponseFileConfig> GetResponseFileConfig(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) is true) { return null; }

            filePath = _io.PathResolver.ResolveFilePath(filePath);
            var content = await _file.GetStreamContent(filePath);
            if(content is null) { return null; }

            return await GetResponseStreamConfig(content);
        }

        public async Task<QckMoxResponseFileConfig> GetResponseStreamConfig(Stream jsonFileContent)
        {
            var content = await JsonHelper.ToJObject(jsonFileContent);
            var prop = content.SelectToken($"{QckMoxResponseFileConfig.CONFIG_KEY}");
            if(prop?.HasValues is not true) { return null; }

            var config = prop.ToObject<QckMoxResponseFileConfig>();
            return config;
        }

        private QckMoxAppConfig ResolveResponseMapPaths(QckMoxAppConfig config)
        {
            return ResolveResponseMapPaths(config, string.Empty, config.ResponseSource);
        }

        private T ResolveResponseMapPaths<T>(T config, string requestUri, string responseSource) where T : QckMoxConfig
        {
            if(config?.ResponseMap?.Any() is not true) { return config; }

            var valueReplacement = string.Empty;
            if(string.IsNullOrWhiteSpace(requestUri) is false)
            {
                valueReplacement = $"{requestUri}/";
            }

            var resolvedMap = new Dictionary<string, string>();

            foreach(var item in config.ResponseMap)
            {
                var key = item.Key;
                var value = item.Value;

                key = _responseMapkeyMatcher
                        .Replace(key, $"$1 {valueReplacement}$2")
                        .Replace('\\', '/');

                if (_io.Path.IsPathRooted(value) is false)
                {
                    value = _io.Path.Combine(responseSource, requestUri, value);
                }

                resolvedMap.Add(key, value);
            }

            config.ResponseMap = resolvedMap;

            return config;
        }
    }
}