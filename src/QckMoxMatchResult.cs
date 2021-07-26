using System.Collections.Generic;
using System.IO;

namespace QckMox
{
    public class QckMoxMatchResult
    {
        public bool MatchFound => Content is not null;
        public string ContentType { get; set; }
        public IReadOnlyDictionary<string, string> ResponseHeaders { get; set; }
        public Stream Content { get; set; }
    }
}