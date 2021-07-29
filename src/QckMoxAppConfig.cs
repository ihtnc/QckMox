using System.Collections.Generic;

namespace QckMox
{
    public class QckMoxAppConfig : QckMoxConfig
    {
        public string EndPoint { get; set; }
        public string ResponseSource { get; set; }

        internal const string CONFIG_KEY = "QckMox";

        internal static QckMoxAppConfig GetDefaultValues() => new QckMoxAppConfig
        {
            EndPoint = "/api/qckmox/",
            ResponseSource = ".qckmox",

            Disabled = false,
            ResponseMap = new Dictionary<string, string>(),
            Request = new QckMoxRequestConfig
            {
                UnmatchedRequest = new QckMoxUnmatchedRequestConfig
                {
                    MatchHttpMethod = false,
                    Passthrough = false
                },
                QueryTag = QckMoxRequestConfig.DEFAULT_QUERY_TAG,
                HeaderTag = QckMoxRequestConfig.DEFAULT_HEADER_TAG,
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