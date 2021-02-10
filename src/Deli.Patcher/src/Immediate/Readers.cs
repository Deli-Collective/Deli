using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Deli.Newtonsoft.Json;
using Deli.Newtonsoft.Json.Linq;
using Deli.VFS;
using Deli.VFS.Disk;

namespace Deli.Immediate
{
	internal static class Readers
	{
		public static ImmediateReaderCollection DefaultCollection(ManualLogSource logger) => new(logger)
		{
			BytesOf,
			StringOf,
			StringEnumerableOf,
			JTokenOf,
			JObjectOf,
			JArrayOf,
			JValueOf
		};

		private static byte[] BytesOf(IFileHandle file)
		{
			if (file is IDiskHandle disk)
			{
				return File.ReadAllBytes(disk.PathOnDisk);
			}

			using var raw = file.OpenRead();

			var buffer = new byte[raw.Length];
			raw.Read(buffer, 0, buffer.Length);

			return buffer;
		}

		private static string StringOf(IFileHandle file)
		{
			using var raw = file.OpenRead();
			using var reader = new StreamReader(raw);

			return reader.ReadToEnd();
		}

		private static IEnumerable<string> StringEnumerableOf(IFileHandle file)
		{
			using var raw = file.OpenRead();
			using var reader = new StreamReader(raw);

			while (!reader.EndOfStream)
			{
				yield return reader.ReadLine() ?? throw new InvalidOperationException("ReadLine returned null, but the end of stream has not been met.");
			}
		}

		private static JToken JTokenOf(IFileHandle file)
		{
			using var raw = file.OpenRead();
			using var text = new StreamReader(raw);
			using var json = new JsonTextReader(text);

			return JToken.Load(json);
		}

		private static TToken SpecialJTokenOf<TToken>(IFileHandle file, string type) where TToken : JToken
		{
			return JTokenOf(file) as TToken ?? throw new FormatException("Expected a JSON " + type);
		}

		private static JObject JObjectOf(IFileHandle file)
		{
			return SpecialJTokenOf<JObject>(file, "object");
		}

		private static JArray JArrayOf(IFileHandle file)
		{
			return SpecialJTokenOf<JArray>(file, "array");
		}

		private static JValue JValueOf(IFileHandle file)
		{
			return SpecialJTokenOf<JValue>(file, "value");
		}
	}
}
