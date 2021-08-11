using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using QckMox.Configuration;
using QckMox.IO;
using QckMox.Request;

namespace QckMox.Matcher
{
    internal interface IQckMoxMatcher
    {
        Task<QckMoxMatchResult> Match(HttpRequest request, QckMoxConfig config);
    }

    internal abstract class QckMoxMatcher : IQckMoxMatcher
    {
        protected IQckMoxConfigurationProvider ConfigProvider { get; }
        protected IIOProvider IOProvider { get; }
        protected IFileProvider FileProvider { get; }

        protected IQckMoxRequestConverter Converter { get; }
        protected QckMoxAppConfig GlobalConfig { get; }

        public QckMoxMatcher(IQckMoxConfigurationProvider config, IIOProvider io, IFileProvider file, IQckMoxRequestConverter converter)
        {
            ConfigProvider = config;

            var task = config.GetGlobalConfig();
            task.Wait();
            GlobalConfig = task.Result;

            IOProvider = io;
            FileProvider = file;
            Converter = converter;
        }

        public abstract Task<QckMoxMatchResult> Match(HttpRequest request, QckMoxConfig config);

        protected internal async Task<QckMoxMatchResult> GetMatchResult(string filePath, QckMoxResponseConfig responseConfig)
        {
            var response = QckMoxResponseFileConfig.Copy(responseConfig);
            var config = await ConfigProvider.GetResponseFileConfig(filePath);
            config = response.Merge(config);
            var fileContent = await FileProvider.GetStreamContent(filePath);
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
            var requestString = Converter.ToRequestString(request, requestConfig, excludeResource: excludeResource, excludeParameters: excludeParameters);
            var mockPath = Converter.GetResourceString(request);

            var folder = IOProvider.Path.Combine(GlobalConfig.ResponseSource, mockPath);

            var responseFile = requestString.ToString(excludeResource: excludeResource, excludeParameters: excludeParameters).Trim();
            responseFile = $"{responseFile}.json";

            var actualFolder = IOProvider.PathResolver.ResolveFolderPath(folder);
            var path = IOProvider.Path.Combine(actualFolder, responseFile);
            if (IOProvider.File.Exists(path) is true) { return path; }

            var filePath = FindResponseFile(folder, requestString, requestConfig, excludeResource: excludeResource, excludeParameters: excludeParameters);
            return filePath;
        }

        private string FindResponseFile(string folder, QckMoxRequestString requestString, QckMoxConfig requestConfig, bool excludeResource = false, bool excludeParameters = false)
        {
            var actualFolder = IOProvider.PathResolver.ResolveFolderPath(folder);
            if (IOProvider.Directory.Exists(actualFolder) is false) { return null; }

            var searchCriteria = $"{requestString.Method}*";

            var files = IOProvider.Directory.GetFiles(actualFolder, searchCriteria);

            files = FilterFiles(files, requestString.Resource);
            var path = SearchFiles(files, requestString, requestConfig.Request);
            if (string.IsNullOrWhiteSpace(path) is true) { return null; }

            return path;
        }

        private string[] FilterFiles(string[] files, string search)
        {
            if (string.IsNullOrWhiteSpace(search) is true) { return files; }
            files = files?.Where(f => IOProvider.Path.GetFileNameWithoutExtension(f).Contains(search, StringComparison.OrdinalIgnoreCase))?.ToArray() ?? new string[0];
            return files;
        }

        private string SearchFiles(string[] files, QckMoxRequestString requestString, QckMoxRequestConfig requestConfig)
        {
            var file = files?.FirstOrDefault(f =>
            {
                var fileName = IOProvider.Path.GetFileNameWithoutExtension(f);
                var valid = QckMoxRequestString.TryParse(fileName, out var converted, requestConfig.QueryTag, requestConfig.HeaderTag);
                if (valid is false) { return false; }

                return converted.Equals(requestString);
            });

            return file;
        }

        internal static QckMoxMatchResult NoMatchResult => new QckMoxMatchResult { Content = null };
    }
}