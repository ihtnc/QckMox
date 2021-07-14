using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using QckMox.Configs;

namespace QckMox
{
    public interface IQckMoxConfigurationProvider
    {
        QckMoxAppConfig GetGlobalConfig();
        QckMoxConfig GetRequestConfig(string requestUri);
        QckMoxConfig GetFolderConfig(string folderPath);
        QckMoxResponseFileConfig GetResponseFileConfig(string filePath, QckMoxConfig folderConfig);
    }

    public class QckMoxConfigurationProvider : IQckMoxConfigurationProvider
    {
        private readonly QckMoxAppConfig _config;
        private readonly IFileProvider _fileProvider;

        public QckMoxConfigurationProvider(IOptions<QckMoxAppConfig> global, IFileProvider fileProvider)
        {
            var appConfig = global.Value;
            var defaultConfig = GetDefaultConfig();
            _config = defaultConfig.Merge(appConfig);
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
            var queue = new Queue<string>();
            queue.Enqueue(global.ResponseSource);

            QckMoxConfig config = global;

            var uris = requestUri.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var uri in uris) { queue.Enqueue(Path.Combine(queue.Peek(), uri)); }

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
            var configFile = Path.Combine(folderPath, $"{Constants.CONFIG_KEY}.json");
            var content = _fileProvider.GetContent(configFile);
            if(content == null) { return null; }

            var obj = JObject.Parse(content);
            return obj.ToObject<QckMoxConfig>();
        }

        public QckMoxResponseFileConfig GetResponseFileConfig(string filePath, QckMoxConfig folderConfig)
        {
            var content = _fileProvider.GetContent(filePath);
            if(content == null) { return null; }

            var file = JObject.Parse(content);
            var prop = file.SelectToken($"{folderConfig.FileConfigProp}");
            if(prop?.HasValues != true) { return null; }

            var config = prop.ToObject<QckMoxResponseFileConfig>();
            var newConfig = new QckMoxResponseFileConfig();
            newConfig = newConfig.Merge(folderConfig.Response);
            return newConfig.Merge(config);
        }

        private static QckMoxAppConfig GetDefaultConfig()
        {
            return new QckMoxAppConfig
            {
                Disabled = false,
                EndPoint = "/api/qckmox/",
                ResponseSource = ".qckmox",
                FileConfigProp = "__config",
                ResponseMap = new Dictionary<string, string>(),
                Request = new QckMoxRequestConfig
                {
                    RedirectUnmatched = false,
                    QueryMapPrefix = string.Empty,
                    HeaderMapPrefix = "qckmox-",
                    MatchHeader = new string[0],
                    MatchQuery = new string[0]
                },
                Response = new QckMoxResponseConfig
                {
                    ContentType = "application/json",
                    FileContentProp = "__data",
                    ContentInProp = false,
                    Headers = new Dictionary<string, string>
                    {
                        {"X-Powered-By", "QckMox/1.0"}
                    }
                }
            };
        }
    }
}