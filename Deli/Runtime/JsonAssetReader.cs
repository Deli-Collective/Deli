using System.IO;
using Atlas;
using Valve.Newtonsoft.Json;

namespace Deli
{
    public class JsonAssetReader<T> : IAssetReader<Option<T>>
    {
        private IServiceResolver _services;

        public JsonAssetReader(IServiceResolver services)
        {
            _services = services;
        }

        public Option<T> ReadAsset(byte[] raw)
        {
            var serializer = _services.Get<JsonSerializer>().Expect("JSON serializer not found");

            using (var memory = new MemoryStream(raw))
            using (var text = new StreamReader(memory))
            using (var json = new JsonTextReader(text))
            {
                T result;
                try
                {
                    result = serializer.Deserialize<T>(json);
                }
                catch
                {
                    return Option.None<T>();
                }

                return Option.Some(result);
            }
        }
    }
}