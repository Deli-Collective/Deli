using System.IO;
using ADepIn;
using BepInEx.Logging;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;

namespace Deli
{
	public class JObjectAssetReader : IAssetReader<Option<JObject>>
	{
		private readonly ManualLogSource _log;

		public JObjectAssetReader(ManualLogSource log)
		{
			_log = log;
		}

		public Option<JObject> ReadAsset(byte[] raw)
		{
			using var memory = new MemoryStream(raw);
			using var text = new StreamReader(memory);
			using var json = new JsonTextReader(text);

			JObject result;
			try
			{
				result = JObject.Load(json);
			}
			catch (JsonReaderException e)
			{
				_log.LogWarning("JSON parse error: " + e.Message);
				_log.LogDebug(e.ToString());

				return Option.None<JObject>();
			}

			return Option.Some(result);
		}
	}
}
