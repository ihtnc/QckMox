using System.Collections.Generic;
using System.IO;

namespace QckMox
{
    public class QckMoxResponse
    {
        public string ContentType { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Stream Content { get; set; }
    }
}