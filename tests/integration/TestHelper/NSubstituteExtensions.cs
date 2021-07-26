using System.IO;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.Core;

namespace QckMox.Tests.Integration.TestHelper
{
    internal static class NSubstituteExtensions
    {
        public static ConfiguredCall ReturnsAsStream(this Task<Stream> stream, string content)
        {
            var streamContent = new MemoryStream();
            var copy = new MemoryStream();

            using (var writer = new StreamWriter(streamContent))
            {
                writer.Write(content);
                writer.Flush();

                streamContent.Position = 0;
                streamContent.CopyTo(copy);
            }

            copy.Position = 0;
            return stream.Returns<Stream>(copy);
        }
    }
}