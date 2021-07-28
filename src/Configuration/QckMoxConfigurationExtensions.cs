using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace QckMox.Configuration
{
    internal static class QckMoxConfigurationExtensions
    {
        private static readonly Regex _responseMapkeyMatcher = new Regex("([A-Za-z]+)[ ]+(.*)");

        public static QckMoxAppConfig ResolveResponseMapPaths(this QckMoxAppConfig config)
        {
            config.ResolveResponseMapPaths(string.Empty, config.ResponseSource);
            return config;
        }

        public static QckMoxConfig ResolveResponseMapPaths(this QckMoxConfig config, string requestUri, string responseSource)
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

                if (Path.IsPathRooted(value) is false)
                {
                    value = Path.Combine(responseSource, requestUri, value);
                }

                resolvedMap.Add(key, value);
            }

            config.ResponseMap = resolvedMap;

            return config;
        }
    }
}