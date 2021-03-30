using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using Deli.Immediate;
using Deli.Newtonsoft.Json;
using Deli.Newtonsoft.Json.Linq;
using Deli.VFS;
using Deli.VFS.Disk;

namespace Deli.Patcher
{
	internal static class Readers
	{
		public static ReaderCollection DefaultCollection(ManualLogSource logger) => new(logger)
		{
			BytesOf,
			AssemblyOf,
			StringOf,
			StringEnumerableOf,
			JTokenOf,
			JObjectOf,
			JArrayOf,
			JValueOf
		};

		private static byte[] BytesOf(IFileHandle file)
		{
			using var raw = file.OpenRead();

			var buffer = new byte[raw.Length];
			raw.Read(buffer, 0, buffer.Length);

			return buffer;
		}

		private static Assembly AssemblyOf(IFileHandle file)
		{
			if (file is IDiskHandle onDisk)
			{
				return Assembly.LoadFile(onDisk.PathOnDisk);
			}

			var rawAssembly = BytesOf(file);

			var symbolsFile = file.WithExtension("mdb") as IFileHandle ?? file.WithExtension("pdb") as IFileHandle;
			if (symbolsFile is not null)
			{
				var rawSymbols = BytesOf(symbolsFile);

				return Assembly.Load(rawAssembly, rawSymbols);
			}

			return Assembly.Load(rawAssembly);
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
