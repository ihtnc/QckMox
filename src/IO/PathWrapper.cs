using System.IO;

namespace QckMox.IO
{
    internal interface IPathWrapper
    {
        string Combine(params string[] paths);
        string GetFileNameWithoutExtension(string path);
        bool IsPathRooted(string path);
        string GetDirectoryName(string path);
    }

    internal class PathWrapper : IPathWrapper
    {
        public string Combine(params string[] paths)=> Path.Combine(paths);

        public string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

        public bool IsPathRooted(string path) => Path.IsPathRooted(path);

        public string GetDirectoryName(string path) => Path.GetDirectoryName(path);
    }
}