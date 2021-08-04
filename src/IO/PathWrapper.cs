using System.IO;

namespace QckMox.IO
{
    internal interface IPathWrapper
    {
        string Combine(params string[] paths);
        string GetFileName(string path) => Path.GetFileName(path);
        bool IsPathRooted(string path) => Path.IsPathRooted(path);
        string GetDirectoryName(string path) => Path.GetDirectoryName(path);
    }

    internal class PathWrapper : IPathWrapper
    {
        public string Combine(params string[] paths)=> Path.Combine(paths);

        public string GetFileName(string path) => Path.GetFileName(path);

        public bool IsPathRooted(string path) => Path.IsPathRooted(path);

        public string GetDirectoryName(string path) => Path.GetDirectoryName(path);
    }
}