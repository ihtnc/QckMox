using System.Collections.Generic;

namespace QckMox.Configs
{
    public class QckMoxConfig
    {
        public bool? Disabled { get; set; }
        public string FileConfigProp { get; set; }
        public Dictionary<string, string> ResponseMap { get; set; }
        public QckMoxRequestConfig Request { get; set; }
        public QckMoxResponseConfig Response { get; set; }

        internal const string CONFIG_KEY = "_qckmox";
    }
}