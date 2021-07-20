using Newtonsoft.Json;

namespace QckMox.Configs
{
    internal class QckMoxResponseFileConfig : QckMoxResponseFileConfigValues
    {
        [JsonIgnore]
        public bool Base64Content => Base64ContentConfigValue ?? default;
    }

    internal class QckMoxResponseFileConfigValues : QckMoxResponseConfig
    {
        [JsonProperty("Base64Content")]
        public bool? Base64ContentConfigValue { get; set; }
    }
}