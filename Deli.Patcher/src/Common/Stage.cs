using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Deli.Patcher;
using Deli.Patcher.Common;
using Deli.VFS;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Deli
{
	public abstract class Stage
	{
		public Blob Data { get; }

		private ImmediateReaderCollection JsonReaders => Data.JsonReaders;

		private JsonSerializer Serializer => Data.Serializer;

		protected ManualLogSource Logger => Data.Logger;

		/// <summary>
		///		The collection of all the <see cref="ImmediateAssetLoader{TStage}"/>s registered.
		/// </summary>
		public NestedServiceCollection<Mod, string, ImmediateAssetLoader<Stage>> SharedAssetLoaders => Data.SharedAssetLoaders;

		/// <summary>
		///		The collection of all the <see cref="ImmediateReader{T}"/>s publicly available.
		/// </summary>
		public ImmediateReaderCollection ImmediateReaders => Data.ImmediateReaders;

		/// <summary>
		///		Invoked when all operations that require this stage are complete.
		/// </summary>
		public event Action? Finished;

		protected Stage(Blob data)
		{
			Data = data;
		}

		private static JObject JObjectReader(IFileHandle handle)
		{
			using var raw = handle.OpenRead();
			using var text = new StreamReader(raw);
			using var json = new JsonTextReader(text);

			return JObject.Load(json);
		}

		private T JsonReader<T>(IFileHandle handle)
		{
			return JObjectReader(handle).ToObject<T>(Serializer) ?? throw new FormatException("JSON file contained a null object.");
		}

		protected void InvokeFinished()
		{
			Finished?.Invoke();
		}

		/// <summary>
		///		Creates and adds a JSON <see cref="ImmediateReader{T}"/> for the type provided.
		/// </summary>
		/// <typeparam name="T">The JSON model.</typeparam>
		public ImmediateReader<T> RegisterJson<T>()
		{
			if (JsonReaders.TryGet<T>(out var reader))
			{
				return reader;
			}

			reader = JsonReader<T>;
			JsonReaders.Add(reader);
			ImmediateReaders.Add(reader);

			return reader;
		}

		public readonly struct Blob
		{
			internal ImmediateReaderCollection JsonReaders { get; }
			internal JsonSerializer Serializer { get; }
			internal ManualLogSource Logger { get; }
			internal NestedServiceCollection<Mod, string, ImmediateAssetLoader<Stage>> SharedAssetLoaders { get; }
			internal ImmediateReaderCollection ImmediateReaders { get; }

			internal Blob(ImmediateReaderCollection jsonReaders, JsonSerializer serializer, ManualLogSource logger, NestedServiceCollection<Mod, string, ImmediateAssetLoader<Stage>> sharedAssetLoaders, ImmediateReaderCollection immediateReaders)
			{
				JsonReaders = jsonReaders;
				Serializer = serializer;
				Logger = logger;
				SharedAssetLoaders = sharedAssetLoaders;
				ImmediateReaders = immediateReaders;
			}
		}
	}
}
