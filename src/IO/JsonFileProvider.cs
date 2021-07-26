using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QckMox.IO
{
    internal class JsonFileProvider : IFileProvider
    {
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
            filePath = ResolvePath(filePath);
            if(File.Exists(filePath) is false) { return null; }

            using(var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var json = await JsonHelper.ToJObject(fs);
                return json;
            }
        }

        private string ResolvePath(string filePath)
        {
            if(File.Exists(filePath) is false && Path.IsPathRooted(filePath) is false)
            {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var rootPath = Path.GetDirectoryName(assemblyPath);

                filePath = Path.Combine(rootPath, filePath);
            }

            return filePath;
        }
    }
}