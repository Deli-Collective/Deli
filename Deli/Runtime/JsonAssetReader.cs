using System.IO;
using Atlas;
using BepInEx.Logging;
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
                catch (JsonReaderException e)
                {
                    if (!_services.Get<ManualLogSource>().MatchSome(out var log))
                    {
                        // We shouldn't swallow the error if it isn't reported.
                        throw;
                    }

                    log.LogWarning($"JSON parse error: " + e.Message);
                    log.LogDebug(e.ToString());

                    return Option.None<T>();
                }

                return Option.Some(result);
            }
        }
    }
}