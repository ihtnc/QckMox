using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QckMox.IO
{
    internal class JsonFileProvider : IFileProvider
    {
        private readonly IIOProvider _io;

        public JsonFileProvider(IIOProvider io)
        {
            _io = io;
        }

        public async Task<string> GetContent(string filePath)
        {
            var json = await GetJsonContent(filePath);
            if (json == null) { return null; }

            return json.ToString(Formatting.None);
        }

        public async Task<Stream> GetStreamContent(string filePath)
        {
            var json = await GetJsonContent(filePath);
            if (json == null) { return null; }

            var copy = new MemoryStream();
            using (var content = new MemoryStream())
            {
                using (var text = new StreamWriter(content))
                {
                    using (var writer = new JsonTextWriter(text))
                    {
                        await json.WriteToAsync(writer);
                        await writer.FlushAsync();

                        content.Position = 0;
                        await content.CopyToAsync(copy);
                    }
                }
            }

            copy.Position = 0;
            return copy;
        }

        private async Task<JObject> GetJsonContent(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) is true) { return null; }

            filePath = _io.PathResolver.ResolveFilePath(filePath);
            if(_io.File.Exists(filePath) is false) { return null; }

            using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var json = await JsonHelper.ToJObject(fs);
                return json;
            }
        }
    }
}