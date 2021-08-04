using System.Reflection;

namespace QckMox.IO
{
    internal interface IPathResolver
    {
        string ResolveFilePath(string filePath);
        string ResolveFolderPath(string folderPath);
    }

    internal class PathResolver: IPathResolver
    {
        private readonly IPathWrapper _path;
        private readonly IFileWrapper _file;
        private readonly IDirectoryWrapper _directory;

        public PathResolver(IPathWrapper path, IFileWrapper file, IDirectoryWrapper directory)
        {
            _path = path;
            _file = file;
            _directory = directory;
        }

        public string ResolveFilePath(string filePath)
        {
            if(_file.Exists(filePath) is false)
            {
                filePath = ResolvePath(filePath);
            }

            return filePath;
        }

        public string ResolveFolderPath(string folderPath)
        {
            if(_directory.Exists(folderPath) is false)
            {
                folderPath = ResolvePath(folderPath);
            }

            return folderPath;
        }

        private string ResolvePath(string path)
        {
            if(_path.IsPathRooted(path) is false)
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var rootPath = _path.GetDirectoryName(assemblyPath);

                path = _path.Combine(rootPath, path);
            }

            return path;
        }
    }
}