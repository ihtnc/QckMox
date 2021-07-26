using System.IO;
using System.Threading.Tasks;

namespace QckMox.IO
{
    internal interface IFileProvider
    {
        Task<string> GetContent(string filePath);
        Task<Stream> GetStreamContent(string filePath);
    }
}