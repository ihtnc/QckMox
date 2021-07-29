namespace QckMox
{
    public class QckMoxRequestConfig
    {
        public QckMoxUnmatchedRequestConfig UnmatchedRequest { get; set; }
        public string QueryTag { get; set; }
        public string HeaderTag { get; set; }
        public string[] MatchHeader { get; set; }
        public string[] MatchQuery { get; set; }

        internal const string DEFAULT_QUERY_TAG = "";
        internal const string DEFAULT_HEADER_TAG = "qmx-";
    }
}