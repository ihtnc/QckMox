using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QckMox
{
    internal static class ObjectHelper
    {
        public static T Merge<T, U>(this T config, U updates) where T : U, new()
        {
            var orig = config is not null ? config : new T();
            var json = JObject.FromObject(orig);
            var updated = JObject.FromObject(updates, new JsonSerializer
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });

            // we only need to merge those that do not exists on the original yet
            json.Merge(updated, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

            var newConfig = json.ToObject<T>();
            return newConfig;
        }

        public static T Copy<T>(this T config) where T : new()
        {
            var orig = config is not null ? config : new T();
            var json = JObject.FromObject(orig);
            var newConfig = json.ToObject<T>();
            return newConfig;
        }
    }
}