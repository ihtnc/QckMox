namespace QckMox.IO
{
    internal interface IIOProvider
    {
        IPathWrapper Path { get; }
        IFileWrapper File { get; }
        IDirectoryWrapper Directory { get; }
        IPathResolver PathResolver { get; }
    }

    internal class IOProvider : IIOProvider
    {
        public IOProvider(IPathResolver pathResolver, IPathWrapper path, IFileWrapper file, IDirectoryWrapper directory)
        {
            PathResolver = pathResolver;
            Path = path;
            File = file;
            Directory = directory;
        }

        public IPathWrapper Path { get; }
        public IFileWrapper File { get; }
        public IDirectoryWrapper Directory { get; }
        public IPathResolver PathResolver { get; }
    }
}