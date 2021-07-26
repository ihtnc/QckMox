using System;
using System.IO;
using System.Collections.Generic;
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
        private readonly QckMoxAppConfig _config;
        private readonly IFileProvider _fileProvider;

        public QckMoxConfigurationProvider(IOptions<QckMoxAppConfig> global, IFileProvider fileProvider)
        {
            var appConfig = global.Value;
            var defaultConfig = QckMoxAppConfig.Default;
            var config = defaultConfig
                            .Merge(appConfig)
                            .ResolveResponseMapPaths();

            _config = config;
            _fileProvider = fileProvider;
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
                breadCrumb = Path.Combine(breadCrumb, uri);
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
            var configFile = Path.Combine(requestUri, QckMoxConfig.FOLDER_CONFIG_FILE);
            var configFilePath = Path.Combine(_config.ResponseSource, configFile);
            var content = await _fileProvider.GetContent(configFilePath);
            if(content is null) { return null; }

            var obj = JObject.Parse(content);
            var config = obj.ToObject<QckMoxConfig>()
                            .ResolveResponseMapPaths(requestUri, _config.ResponseSource);

            return config;
        }

        public async Task<QckMoxResponseFileConfig> GetResponseFileConfig(string filePath)
        {
            var content = await _fileProvider.GetStreamContent(filePath);
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
    }
}