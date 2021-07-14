using System.IO;
using System.Reflection;

namespace QckMox
{
    public interface IFileProvider
    {
        string GetContent(string filePath);
    }

    public class FileProvider : IFileProvider
    {
        public string GetContent(string filePath)
        {
            if(!File.Exists(filePath) && !Path.IsPathRooted(filePath))
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var rootPath = Path.GetDirectoryName(assemblyPath);

                filePath = Path.Combine(rootPath, filePath);
            }

            if(!File.Exists(filePath)) { return null; }

            string content = null;
            using(var reader = File.OpenText(filePath))
            {
                content = reader.ReadToEnd();
            }

            return content;
        }
    }
}