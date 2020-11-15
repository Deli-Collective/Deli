using System.IO;
using Atlas;
using BepInEx.Logging;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;

namespace Deli
{
    public class JObjectAssetReader : IAssetReader<Option<JObject>>
    {
        private readonly IServiceResolver _services;

        public JObjectAssetReader(IServiceResolver services)
        {
            _services = services;
        }

        public Option<JObject> ReadAsset(byte[] raw)
        {
            var serializer = _services.Get<JsonSerializer>().Expect("JSON serializer not found");

            using (var memory = new MemoryStream(raw))
            using (var text = new StreamReader(memory))
            using (var json = new JsonTextReader(text))
            {
                JObject result;
                try 
                {
                    result = JObject.Load(json);
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

                    return Option.None<JObject>();
                }

                return Option.Some(result);
            }
        }
    }
}