namespace QckMox.Configs
{
    public class QckMoxRequestConfig
    {
        public bool RedirectUnmatched { get; set; }
        public string QueryMapPrefix { get; set; }
        public string HeaderMapPrefix { get; set; }
        public string[] MatchHeader { get; set; }
        public string[] MatchQuery { get; set; }
    }
}