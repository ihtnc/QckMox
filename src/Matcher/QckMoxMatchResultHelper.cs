using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QckMox.Configuration;

namespace QckMox.Matcher
{
    internal class QckMoxMatchResultHelper
    {
        public static async Task<Stream> GetResponseContent(Stream fileContent, QckMoxResponseFileConfig fileConfig)
        {
            var json = null as JToken;

            using(var copy = new MemoryStream())
            {
                fileContent.Position = 0;
                await fileContent.CopyToAsync(copy);

                fileContent.Position = 0;
                copy.Position = 0;

                using(var text = new StreamReader(copy))
                {
                    using(var reader = new JsonTextReader(text))
                    {
                        var content = await JObject.ReadFromAsync(reader) ;
                        json = content;
                    }
                }
            }

            if(fileConfig.ContentInProp is true)
            {
                var hasKey = (json as JObject).ContainsKey(fileConfig.FileContentProp);
                json = hasKey ? json[fileConfig.FileContentProp] : null;
            }
            else
            {
                var copy = (JObject)json.DeepClone();
                copy.Remove(QckMoxResponseConfig.CONFIG_KEY);
                json = copy;
            }

            return GetContentStream(json, fileConfig);
        }

        private static Stream GetContentStream(JToken content, QckMoxResponseFileConfig fileConfig)
        {
            byte[] response;
            if(fileConfig?.Base64Content is true)
            {
                var base64String = content.Value<string>();
                response = Convert.FromBase64String(base64String);
            }
            else
            {
                var contentString = JsonConvert.SerializeObject(content);
                response = Encoding.UTF8.GetBytes(contentString);
            }

            return new MemoryStream(response);
        }
    }
}