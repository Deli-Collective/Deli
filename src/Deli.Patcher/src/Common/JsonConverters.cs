using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Deli.Newtonsoft.Json;
using Deli.Newtonsoft.Json.Linq;
using Semver;

namespace Deli
{
	internal class SemVersionJsonConverter : JsonConverter<SemVersion?>
	{
		public override void WriteJson(JsonWriter writer, SemVersion? value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value?.ToString());
		}

		public override SemVersion? ReadJson(JsonReader reader, Type objectType, SemVersion? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var raw = serializer.Deserialize<string?>(reader);
			return raw is null ? null : SemVersion.Parse(raw);
		}
	}

	internal class AssetLoaderIDJsonConverter : JsonConverter<AssetLoaderID>
	{
		public override void WriteJson(JsonWriter writer, AssetLoaderID value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value.Mod + ":" + value.Name);
		}

		public override AssetLoaderID ReadJson(JsonReader reader, Type objectType, AssetLoaderID existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var raw = serializer.Deserialize<string?>(reader);
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

	internal class DeepDictionaryJsonConverter : JsonConverter
	{
		private delegate object? Reader(JToken root, JsonSerializer serializer);
		private delegate void Writer(JsonWriter writer, object value, JsonSerializer serializer);

		private static readonly MethodInfo ReaderMethod;
		private static readonly MethodInfo WriterMethod;

		static DeepDictionaryJsonConverter()
		{
			foreach (var method in typeof(DeepDictionaryJsonConverter).GetMethods(BindingFlags.NonPublic | BindingFlags.Static))
			{
				switch (method.Name)
				{
					case nameof(ReadJsonTyped):
						ReaderMethod = method;
						break;
					case nameof(WriteJsonTyped):
						WriterMethod = method;
						break;
				}
			}

			if (ReaderMethod is null || WriterMethod is null)
			{
				throw new InvalidOperationException("Reader or writer methods were not found.");
			}
		}

		private readonly Dictionary<Type, TypeCache> _caches = new();

		public override bool CanConvert(Type objectType)
		{
			return objectType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>) && i.GetGenericArguments()[0] == typeof(string));
		}

		private static Dictionary<string, TValue?> ReadJsonTyped<TValue>(JToken root, JsonSerializer serializer)
		{
			var ret = new Dictionary<string, TValue?>();

			foreach (var token in root)
			{
				if (token is not JProperty property)
				{
					throw new InvalidOperationException("All tokens of a dictionary must be properties.");
				}

				ret.Add(property.Name, property.Value.ToObject<TValue>(serializer));
			}

			return ret;
		}

		private static void WriteJsonTyped<TValue>(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			var dict = value as IDictionary<string, TValue>;

			if (dict is null)
			{
				writer.WriteNull();
				return;
			}

			writer.WriteStartObject();
			foreach (var pair in dict)
			{
				writer.WritePropertyName(pair.Key);
				serializer.Serialize(writer, pair.Value);
			}
			writer.WriteEndObject();
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is null)
			{
				writer.WriteNull();
				return;
			}

			var dictType = value.GetType();
			if (!_caches.TryGetValue(dictType, out var cache))
			{
				cache = new TypeCache(dictType);
				_caches.Add(dictType, cache);
			}

			cache.Writer(writer, value, serializer);
		}

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			var root = JToken.ReadFrom(reader);
			if (root is JValue {Value: null})
			{
				return null;
			}

			if (root is not JObject)
			{
				throw new InvalidOperationException("Expected a dictionary object.");
			}

			if (!_caches.TryGetValue(objectType, out var cache))
			{
				cache = new TypeCache(objectType);
				_caches.Add(objectType, cache);
			}

			return cache.Reader(root, serializer);
		}

		private class TypeCache
		{
			public readonly Reader Reader;
			public readonly Writer Writer;

			public TypeCache(Type dictType)
			{
				var valueType = dictType.GetGenericArguments()[1];
				Reader = (Reader) Delegate.CreateDelegate(typeof(Reader), ReaderMethod.MakeGenericMethod(valueType));
				Writer = (Writer) Delegate.CreateDelegate(typeof(Writer), WriterMethod.MakeGenericMethod(valueType));
			}
		}
	}
}
