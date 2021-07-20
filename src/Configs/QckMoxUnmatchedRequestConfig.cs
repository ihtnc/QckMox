using Newtonsoft.Json;

namespace QckMox.Configs
{
    internal class QckMoxUnmatchedRequestConfig : QckMoxUnmatchedRequestConfigValues
    {
        [JsonIgnore]
        public bool MatchHttpMethod => MatchHttpMethodConfigValue ?? default;

        [JsonIgnore]
        public bool Passthrough => PassthroughConfigValue ?? default;

    }

    internal class QckMoxUnmatchedRequestConfigValues
    {
        [JsonProperty("MatchHttpMethod")]
        public bool? MatchHttpMethodConfigValue { get; set; }

        [JsonProperty("Passthrough")]
        public bool? PassthroughConfigValue { get; set; }

    }
}