using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace QckMox.Configuration
{
    internal class QckMoxConfig : QckMoxConfigValues
    {
        private static readonly Regex _responseMapkeyMatcher = new Regex("([A-Za-z]+)[ ]+(.*)");

        [JsonIgnore]
        public bool Disabled => DisabledConfigValue ?? default;

        [JsonIgnore]
        public Dictionary<string, string> ResponseMap => ResponseMapConfigValue ?? default;

        [JsonIgnore]
        public QckMoxRequestConfig Request => RequestConfigValue ?? default;

        [JsonIgnore]
        public QckMoxResponseConfig Response => ResponseConfigValue ?? default;

        public const string FOLDER_CONFIG_FILE = "_qckmox.json";

        public QckMoxConfig ResolveResponseMapPaths(string requestUri, string responseSource)
        {
            if(ResponseMap?.Any() is not true) { return this; }

            var valueReplacement = string.Empty;
            if(string.IsNullOrWhiteSpace(requestUri) is false)
            {
                valueReplacement = $"{requestUri}/";
            }

            var resolvedMap = new Dictionary<string, string>();

            foreach(var item in ResponseMap)
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

            ResponseMapConfigValue = resolvedMap;

            return this;
        }
    }

    internal class QckMoxConfigValues
    {
        [JsonProperty("Disabled")]
        public bool? DisabledConfigValue { get; set; }

        [JsonProperty("ResponseMap")]
        public Dictionary<string, string> ResponseMapConfigValue { get; set; }

        [JsonProperty("Request")]
        public QckMoxRequestConfig RequestConfigValue { get; set; }

        [JsonProperty("Response")]
        public QckMoxResponseConfig ResponseConfigValue { get; set; }
    }
}