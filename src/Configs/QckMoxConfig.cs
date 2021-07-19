using System.Collections.Generic;

namespace QckMox.Configs
{
    internal class QckMoxConfig
    {
        public bool Disabled { get; set; }
        public Dictionary<string, string> ResponseMap { get; set; }
        public QckMoxRequestConfig Request { get; set; }
        public QckMoxResponseConfig Response { get; set; }

        public const string FOLDER_CONFIG_FILE = "_qckmox.json";
    }
}