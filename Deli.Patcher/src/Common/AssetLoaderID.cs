using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Deli
{
	public readonly struct AssetLoaderID
	{
		public string Mod { get; }
		public string Name { get; }

		public AssetLoaderID(string mod, string name)
		{
			Mod = mod;
			Name = name;
		}

		public override string ToString()
		{
			return Mod + ":" + Name;
		}
	}

	public class AssetLoaderIDJsonConverter : JsonConverter<AssetLoaderID>
	{
		public override void WriteJson(JsonWriter writer, AssetLoaderID value, JsonSerializer serializer)
		{
			writer.WriteValue(value.Mod + ":" + value.Name);
		}

		public override AssetLoaderID ReadJson(JsonReader reader, Type objectType, AssetLoaderID existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var raw = JToken.ReadFrom(reader).ToObject<string>(serializer);
			if (raw is null)
			{
				throw new FormatException("Asset loader IDs cannot be null.");
			}

			var split = raw.Split(':');
			if (split.Length != 2)
			{
				throw new FormatException("Asset loader IDs should only contain 1 colon.");
			}

			return new AssetLoaderID(split[0], split[1]);
		}
	}
}
