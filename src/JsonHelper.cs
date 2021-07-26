using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QckMox
{
    internal class JsonHelper
    {
        public static async Task<JObject> ToJObject(Stream fileContent)
        {
            using (var copy = new MemoryStream())
            {
                fileContent.Position = 0;
                await fileContent.CopyToAsync(copy);

                fileContent.Position = 0;
                copy.Position = 0;

                using(var text = new StreamReader(copy))
                {
                    using(var json = new JsonTextReader(text))
                    {
                        try
                        {
                            var obj = await JToken.ReadFromAsync(json);
                            if (obj.Type != JTokenType.Object)
                            {
                                throw new JsonException("Token type not supported.");
                            }

                            return obj as JObject;
                        }
                        catch (Exception e)
                        {
                            throw new InvalidDataException("Invalid file format.", e);
                        }
                    }
                }
            }
        }
    }
}