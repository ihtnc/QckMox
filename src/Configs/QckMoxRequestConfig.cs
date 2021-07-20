using Newtonsoft.Json;

namespace QckMox.Configs
{
    internal class QckMoxRequestConfig : QckMoxRequestConfigValues
    {
        [JsonIgnore]
        public QckMoxUnmatchedRequestConfig UnmatchedRequest => UnmatchedRequestConfigValue ?? default;

        [JsonIgnore]
        public string QueryMapPrefix => QueryMapPrefixConfigValue ?? default;

        [JsonIgnore]
        public string HeaderMapPrefix => HeaderMapPrefixConfigValue ?? default;

        [JsonIgnore]
        public string[] MatchHeader => MatchHeaderConfigValue ?? default;

        [JsonIgnore]
        public string[] MatchQuery => MatchQueryConfigValue ?? default;
    }

    internal class QckMoxRequestConfigValues
    {
        [JsonProperty("UnmatchedRequest")]
        public QckMoxUnmatchedRequestConfig UnmatchedRequestConfigValue { get; set; }

        [JsonProperty("QueryMapPrefix")]
        public string QueryMapPrefixConfigValue { get; set; }

        [JsonProperty("HeaderMapPrefix")]
        public string HeaderMapPrefixConfigValue { get; set; }

        [JsonProperty("MatchHeader")]
        public string[] MatchHeaderConfigValue { get; set; }

        [JsonProperty("MatchQuery")]
        public string[] MatchQueryConfigValue { get; set; }
    }
}