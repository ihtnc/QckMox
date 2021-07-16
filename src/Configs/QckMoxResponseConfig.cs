using System.Collections.Generic;

namespace QckMox.Configs
{
    public class QckMoxResponseConfig
    {
        public string ContentType { get; set; }
        public bool ContentInProp { get; set; }
        public string FileContentProp { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        internal const string CONFIG_KEY = "_qckmox";
    }
}