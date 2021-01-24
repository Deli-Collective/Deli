using System;
using System.Collections.Generic;
using BepInEx.Logging;
using Deli.Patcher;
using Deli.Patcher.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Deli
{
	public abstract class Stage
	{
		protected ManualLogSource Logger { get; }

		protected JsonSerializer Serializer { get; }

		protected JObjectImmediateReader JObjectImmediateReader { get; }

		protected Dictionary<string, ISharedAssetLoader> SharedLoaders { get; }

		/// <summary>
		///		The collection of all the <see cref="IImmediateReader{T}"/>s publicly available.
		/// </summary>
		public ImmediateReaderCollection ImmediateReaders { get; }

		/// <summary>
		///		Invoked when all operations that require this stage are complete.
		/// </summary>
		public event Action? Finished;

		protected Stage(ManualLogSource logger, JsonSerializer serializer, JObjectImmediateReader jObjectImmediateReader, Dictionary<string, ISharedAssetLoader> sharedLoaders, ImmediateReaderCollection immediateReaders)
		{
			Logger = logger;
			Serializer = serializer;
			JObjectImmediateReader = jObjectImmediateReader;
			SharedLoaders = sharedLoaders;
			ImmediateReaders = immediateReaders;

			immediateReaders.Add(jObjectImmediateReader);
		}

		protected static IDisposable AddAssetLoader<TAssetLoader, TStage>(string name, TAssetLoader loader, Dictionary<string, TAssetLoader> loaders)
			where TAssetLoader : IImmediateAssetLoader<TStage> where TStage : Stage
		{
			if (loaders.ContainsKey(name))
			{
				throw new InvalidOperationException($"An {typeof(TAssetLoader)} asset loader with the same name ({name}) already exists.");
			}

			loaders.Add(name, loader);
			return new ActionDisposable(() => loaders.Remove(name));
		}

		public IDisposable AddAssetLoader(string name, ISharedAssetLoader loader)
		{
			return AddAssetLoader<ISharedAssetLoader, Stage>(name, loader, SharedLoaders);
		}

		/// <summary>
		///		Creates and adds a JSON <see cref="IImmediateReader{T}"/> for the type provided.
		/// </summary>
		/// <typeparam name="T">The JSON model.</typeparam>
		public JsonImmediateReader<T> RegisterImmediateJson<T>()
		{
			if (ImmediateReaders.TryGet<T>(out var unknown) && unknown is JsonImmediateReader<T> reader)
			{
				return reader;
			}

			reader = new JsonImmediateReader<T>(JObjectImmediateReader, Serializer);
			ImmediateReaders.Add(reader);

			return reader;
		}
	}
}
