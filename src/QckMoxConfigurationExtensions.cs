using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QckMox
{
    public static class QckMoxConfigurationExtensions
    {
        public static T Merge<T, U>(this T config, U updates) where T : U, new()
        {
            var orig = config != null ? config : new T();
            var json = JObject.FromObject(orig);
            var updated = JObject.FromObject(updates, new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            json.Merge(updated, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

            var newConfig = json.ToObject<T>();
            return newConfig;
        }
    }
}