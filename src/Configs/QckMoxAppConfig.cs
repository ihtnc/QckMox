using System.Collections.Generic;

namespace QckMox.Configs
{
    public class QckMoxAppConfig : QckMoxConfig
    {
        public string EndPoint { get; set; }
        public string ResponseSource { get; set; }

        internal const string APP_CONFIG_KEY = "QckMox";

        internal static readonly QckMoxAppConfig Default = new QckMoxAppConfig
        {
            Disabled = false,
            EndPoint = "/api/qckmox/",
            ResponseSource = ".qckmox",
            FileConfigProp = "_qckmox",
            ResponseMap = new Dictionary<string, string>(),
            Request = new QckMoxRequestConfig
            {
                RedirectUnmatched = false,
                QueryMapPrefix = string.Empty,
                HeaderMapPrefix = "qckmox-",
                MatchHeader = new string[0],
                MatchQuery = new string[0]
            },
            Response = new QckMoxResponseConfig
            {
                ContentType = "application/json",
                FileContentProp = "data",
                ContentInProp = false,
                Headers = new Dictionary<string, string>
                {
                    {"X-Powered-By", "QckMox/1.0"}
                }
            }
        };
    }
}