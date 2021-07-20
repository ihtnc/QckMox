using System.Collections.Generic;
using Newtonsoft.Json;

namespace QckMox.Configs
{
    internal class QckMoxResponseConfig : QckMoxResponseConfigValues
    {
        [JsonIgnore]
        public string ContentType => ContentTypeConfigValue ?? default;

        [JsonIgnore]
        public bool ContentInProp => ContentInPropConfigValue ?? default;

        [JsonIgnore]
        public string FileContentProp => FileContentPropConfigValue ?? default;

        [JsonIgnore]
        public Dictionary<string, string> Headers => HeadersConfigValue ?? default;

        public const string CONFIG_KEY = "_qckmox";
    }

    internal class QckMoxResponseConfigValues
    {
        [JsonProperty("ContentType")]
        public string ContentTypeConfigValue { get; set; }

        [JsonProperty("ContentInProp")]
        public bool? ContentInPropConfigValue { get; set; }

        [JsonProperty("FileContentProp")]
        public string FileContentPropConfigValue { get; set; }

        [JsonProperty("Headers")]
        public Dictionary<string, string> HeadersConfigValue { get; set; }
    }
}