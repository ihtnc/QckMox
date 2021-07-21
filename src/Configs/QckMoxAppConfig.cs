using System.Collections.Generic;
using Newtonsoft.Json;

namespace QckMox.Configs
{
    internal class QckMoxAppConfig : QckMoxAppConfigValue
    {
        [JsonIgnore]
        public string EndPoint => EndPointConfigValue ?? default;

        [JsonIgnore]
        public string ResponseSource => ResponseSourceConfigValue ?? default;

        public const string CONFIG_KEY = "QckMox";

        public static readonly QckMoxAppConfig Default = GetDefault();

        public static QckMoxAppConfig GetDefault() => new QckMoxAppConfig
        {
            EndPointConfigValue = "/api/qckmox/",
            ResponseSourceConfigValue = ".qckmox",

            DisabledConfigValue = false,
            ResponseMapConfigValue = new Dictionary<string, string>(),
            RequestConfigValue = new QckMoxRequestConfig
            {
                UnmatchedRequestConfigValue = new QckMoxUnmatchedRequestConfig
                {
                    MatchHttpMethodConfigValue = false,
                    PassthroughConfigValue = false
                },
                QueryMapPrefixConfigValue = string.Empty,
                HeaderMapPrefixConfigValue = "qckmox-",
                MatchHeaderConfigValue = new string[0],
                MatchQueryConfigValue = new string[0]
            },
            ResponseConfigValue = new QckMoxResponseConfig
            {
                ContentTypeConfigValue = "application/json",
                FileContentPropConfigValue = "data",
                ContentInPropConfigValue = false,
                HeadersConfigValue = new Dictionary<string, string>
                {
                    {"X-Powered-By", "QckMox/1.0"}
                }
            }
        };
    }

    internal class QckMoxAppConfigValue : QckMoxConfig
    {
        [JsonProperty("EndPoint")]
        public string EndPointConfigValue { get; set; }

        [JsonProperty("ResponseSource")]
        public string ResponseSourceConfigValue { get; set; }
    }
}