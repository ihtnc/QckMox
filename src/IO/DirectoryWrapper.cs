using System.IO;

namespace QckMox.IO
{
    internal interface IDirectoryWrapper
    {
        bool Exists(string path);
        string[] GetFiles(string path, string searchPattern);
    }

    internal class DirectoryWrapper : IDirectoryWrapper
    {
        public bool Exists(string path) => Directory.Exists(path);
        public string[] GetFiles(string path, string searchPattern)=>
            Directory.GetFiles(path, searchPattern, new EnumerationOptions
            {
                IgnoreInaccessible = true,
                ReturnSpecialDirectories = true
            });
    }
}