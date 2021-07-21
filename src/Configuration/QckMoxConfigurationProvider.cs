using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QckMox.IO;

namespace QckMox.Configuration
{
    internal interface IQckMoxConfigurationProvider
    {
        QckMoxAppConfig GetGlobalConfig();
        QckMoxConfig GetRequestConfig(string requestUri);
        QckMoxResponseFileConfig GetResponseFileConfig(string filePath, QckMoxConfig folderConfig);
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

        public QckMoxAppConfig GetGlobalConfig()
        {
            return _config;
        }

        public QckMoxConfig GetRequestConfig(string requestUri)
        {
            // merge all folder configs referenced by the requestUri
            var breadCrumb = string.Empty;
            var queue = new Queue<string>();
            queue.Enqueue(breadCrumb);

            QckMoxConfig config = _config;

            var uris = requestUri.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var uri in uris)
            {
                breadCrumb = Path.Combine(breadCrumb, uri);
                queue.Enqueue(breadCrumb);
            }

            while(queue.Count > 0)
            {
                var uri = queue.Dequeue();
                var temp = GetUriConfig(uri);
                config = temp == null ? config : config.Merge(temp);
            }

            return config;
        }

        private QckMoxConfig GetUriConfig(string requestUri)
        {
            var configFile = Path.Combine(requestUri, QckMoxConfig.FOLDER_CONFIG_FILE);
            var configFilePath = Path.Combine(_config.ResponseSource, configFile);
            var content = _fileProvider.GetContent(configFilePath);
            if(content is null) { return null; }

            var obj = JObject.Parse(content);
            var config = obj.ToObject<QckMoxConfig>()
                            .ResolveResponseMapPaths(requestUri, _config.ResponseSource);

            return config;
        }

        public QckMoxResponseFileConfig GetResponseFileConfig(string filePath, QckMoxConfig folderConfig)
        {
            var content = _fileProvider.GetContent(filePath);
            if(content == null) { return null; }

            var file = JObject.Parse(content);
            var prop = file.SelectToken($"{QckMoxResponseFileConfig.CONFIG_KEY}");
            if(prop?.HasValues is not true) { return null; }

            var config = prop.ToObject<QckMoxResponseFileConfig>();
            var newConfig = new QckMoxResponseFileConfig();
            newConfig = newConfig.Merge(folderConfig.Response);
            return newConfig.Merge(config);
        }
    }
}