using ADepIn;
using BepInEx.Logging;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;

namespace Deli
{
	public class JsonAssetReader<T> : IAssetReader<Option<T>>
	{
		private readonly ManualLogSource _log;
		private readonly IAssetReader<Option<JObject>> _jObject;
		private readonly JsonSerializer _serializer;

		public JsonAssetReader(ManualLogSource log, IAssetReader<Option<JObject>> jObject, JsonSerializer serializer)
		{
			_log = log;
			_jObject = jObject;
			_serializer = serializer;
		}

		public Option<T> ReadAsset(byte[] raw)
		{
			return _jObject.ReadAsset(raw).Map(v =>
			{
				T result;
				try
				{
					result = v.ToObject<T>(_serializer);
				}
				catch (JsonSerializationException e)
				{
					_log.LogWarning($"Typed JSON ({typeof(T)}) parse error: " + e.Message);
					_log.LogDebug(e.ToString());

					return Option.None<T>();
				}

				return Option.Some(result);
			}).Flatten();
		}
	}
}
