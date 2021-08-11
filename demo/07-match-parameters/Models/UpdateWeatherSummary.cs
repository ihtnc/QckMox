namespace QckMox.Demo.MatchParameters.Models
{
    public class UpdateRequest
    {
        public string QueryName { get; set; }
        public int QueryValue { get; set; }
        public string HeaderName { get; set; }
        public int HeaderValue { get; set; }
        public string UpdateValue { get; set; }
    }
}