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
        protected IFileProvider FileProvider { get; }
        protected IQckMoxRequestConverter Converter { get; }
        protected QckMoxAppConfig GlobalConfig { get; }

        public QckMoxMatcher(IQckMoxConfigurationProvider config, IFileProvider file, IQckMoxRequestConverter converter)
        {
            ConfigProvider = config;

            var task = config.GetGlobalConfig();
            task.Wait();
            GlobalConfig = task.Result;

            FileProvider = file;
            Converter = converter;
        }

        public abstract Task<QckMoxMatchResult> Match(HttpRequest request, QckMoxConfig config);

        protected internal async Task<QckMoxMatchResult> GetMatchResult(string filePath, QckMoxResponseConfig responseConfig)
        {
            var config = await ConfigProvider.GetResponseFileConfig(filePath);
            config = config.Merge(responseConfig);
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
            var responseFile = $"{requestString}".Trim();
            return $"{responseFile}.json";
        }

        internal static QckMoxMatchResult NoMatchResult => new QckMoxMatchResult { Content = null };
    }
}