using ADepIn;
using BepInEx.Logging;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;

namespace Deli
{
    public class JsonAssetReader<T> : IAssetReader<Option<T>>
    {
        private readonly IServiceResolver _services;

        public JsonAssetReader(IServiceResolver services)
        {
            _services = services;
        }

        public Option<T> ReadAsset(byte[] raw)
        {
            var serializer = _services.Get<JsonSerializer>().Expect("JSON serializer not found");
            var jObject = _services.Get<IAssetReader<Option<JObject>>>().Expect("JSON parser not found");

            return jObject.ReadAsset(raw).Map(v =>
            {
                T result;
                try
                {
                    result = v.ToObject<T>(serializer);
                }
                catch (JsonSerializationException e)
                {
                    if (!_services.Get<ManualLogSource>().MatchSome(out var log))
                        // We shouldn't swallow the error if it isn't reported.
                        throw;

                    log.LogWarning($"Typed JSON ({typeof(T)}) parse error: " + e.Message);
                    log.LogDebug(e.ToString());

                    return Option.None<T>();
                }

                return Option.Some(result);
            }).Flatten();
        }
    }
}