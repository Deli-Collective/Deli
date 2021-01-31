using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Logging;
using Deli.Patcher;
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

		protected Mod Mod => Data.Mod;

		protected ManualLogSource Logger => Mod.Logger;

		protected List<DeliModule> Modules { get; } = new();

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

		protected IEnumerable<IHandle> Glob(Mod mod, KeyValuePair<string, AssetLoaderID> asset)
		{
			using var globbed = mod.Resources.Glob(asset.Key).GetEnumerator();

			if (!globbed.MoveNext())
			{
				Logger.LogWarning($"Asset from {mod} of type {asset.Value} did not match any handles: {asset.Key}");
				yield break;
			}

			do
			{
				var handle = globbed.Current!;

				Logger.LogDebug($"{handle} > {asset.Value}");
				yield return handle;
			} while (globbed.MoveNext());
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

		protected IFileHandle AssemblyPreloader(IHandle handle)
		{
			if (handle is not IFileHandle file)
			{
				throw new ArgumentException("Assembly loaders must be provided an assembly file.", nameof(handle));
			}

			return file;
		}

		protected virtual void TypeLoader(Stage stage, Mod mod, Type type)
		{
			if (type.IsAbstract || !typeof(DeliModule).IsAssignableFrom(type)) return;

			var module = (DeliModule) Activator.CreateInstance(type, mod);
			module.RunStage(stage);
		}

		protected virtual void AssemblyLoader(Stage stage, Mod mod, Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				TypeLoader(stage, mod, type);
			}
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
			internal Mod Mod { get; }
			internal ImmediateReaderCollection JsonReaders { get; }
			internal JsonSerializer Serializer { get; }
			internal NestedServiceCollection<Mod, string, ImmediateAssetLoader<Stage>> SharedAssetLoaders { get; }
			internal ImmediateReaderCollection ImmediateReaders { get; }

			internal Blob(Mod mod, ImmediateReaderCollection jsonReaders, JsonSerializer serializer,
				NestedServiceCollection<Mod, string, ImmediateAssetLoader<Stage>> sharedAssetLoaders, ImmediateReaderCollection immediateReaders)
			{
				Mod = mod;
				JsonReaders = jsonReaders;
				Serializer = serializer;
				SharedAssetLoaders = sharedAssetLoaders;
				ImmediateReaders = immediateReaders;
			}
		}
	}

	public abstract class Stage<TLoader> : Stage where TLoader : Delegate
	{
		protected abstract string Name { get; }

		protected Stage(Blob data) : base(data)
		{
		}

		protected abstract TLoader? GetLoader(Mod mod, string name);

		protected TLoader GetLoader(Mod mod, Dictionary<string, Mod> lookup, KeyValuePair<string, AssetLoaderID> asset)
		{
			var loaderID = asset.Value;

			if (!lookup.TryGetValue(loaderID.Mod, out var loaderMod))
			{
				throw new InvalidOperationException($"Mod required for {Name} asset \"{asset.Key}\" of {mod} was not present: {loaderID.Mod}");
			}

			var loader = GetLoader(loaderMod, loaderID.Name);
			if (loader is null)
			{
				throw new InvalidOperationException($"Loader required for {Name} asset \"{asset.Key}\" of {mod} was not present.");
			}

			return loader;
		}
	}
}
