using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QckMox.Configs;

namespace QckMox
{
    internal interface IQckMoxConfigurationProvider
    {
        QckMoxAppConfig GetGlobalConfig();
        QckMoxConfig GetRequestConfig(string requestUri);
        QckMoxConfig GetFolderConfig(string folderPath);
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
            var config = defaultConfig.Merge(appConfig);
            _config = ResolveResponseMapPaths(config.ResponseSource, config);
            _fileProvider = fileProvider;
        }

        public QckMoxAppConfig GetGlobalConfig()
        {
            return _config;
        }

        public QckMoxConfig GetRequestConfig(string requestUri)
        {
            // merge all folder configs referenced by the requestUri
            var global = _config;
            var breadCrumb = global.ResponseSource;
            var queue = new Queue<string>();
            queue.Enqueue(breadCrumb);

            QckMoxConfig config = global;

            var uris = requestUri.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var uri in uris)
            {
                breadCrumb = Path.Combine(breadCrumb, uri);
                queue.Enqueue(breadCrumb);
            }

            while(queue.Count > 0)
            {
                var folder = queue.Dequeue();
                var temp = GetFolderConfig(folder);
                config = temp == null ? config : config.Merge(temp);
            }

            return config;
        }

        public QckMoxConfig GetFolderConfig(string folderPath)
        {
            var configFile = Path.Combine(folderPath, QckMoxConfig.FOLDER_CONFIG_FILE);
            var content = _fileProvider.GetContent(configFile);
            if(content is null) { return null; }

            var obj = JObject.Parse(content);
            var config = obj.ToObject<QckMoxConfig>();
            config = ResolveResponseMapPaths(folderPath, config);

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

        private T ResolveResponseMapPaths<T>(string basePath, T config) where T: QckMoxConfig
        {
            if(config.ResponseMap?.Any() is true)
            {
                foreach(var item in config.ResponseMap)
                {
                    if (Path.IsPathRooted(item.Value) is false)
                    {
                        config.ResponseMap[item.Key] = Path.Combine(basePath, item.Value);
                    }
                }
            }

            return config;
        }
    }
}