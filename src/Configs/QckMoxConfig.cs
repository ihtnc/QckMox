using System.Collections.Generic;
using Newtonsoft.Json;

namespace QckMox.Configs
{
    internal class QckMoxConfig : QckMoxConfigValues
    {
        [JsonIgnore]
        public bool Disabled => DisabledConfigValue ?? default;

        [JsonIgnore]
        public Dictionary<string, string> ResponseMap => ResponseMapConfigValue ?? default;

        [JsonIgnore]
        public QckMoxRequestConfig Request => RequestConfigValue ?? default;

        [JsonIgnore]
        public QckMoxResponseConfig Response => ResponseConfigValue ?? default;

        public const string FOLDER_CONFIG_FILE = "_qckmox.json";
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