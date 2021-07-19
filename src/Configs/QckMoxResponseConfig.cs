using System.Collections.Generic;

namespace QckMox.Configs
{
    internal class QckMoxResponseConfig
    {
        public string ContentType { get; set; }
        public bool ContentInProp { get; set; }
        public string FileContentProp { get; set; }
        public Dictionary<string, string> Headers { get; set; }

        public const string CONFIG_KEY = "_qckmox";
    }
}