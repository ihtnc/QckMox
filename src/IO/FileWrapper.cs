using System.IO;

namespace QckMox.IO
{
    internal interface IFileWrapper
    {
        bool Exists(string path) => File.Exists(path);
    }

    internal class FileWrapper : IFileWrapper
    {
        public bool Exists(string path) => File.Exists(path);
    }
}