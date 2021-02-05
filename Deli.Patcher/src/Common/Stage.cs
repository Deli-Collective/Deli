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

		protected JsonSerializer Serializer => Data.Serializer;

		protected Mod Mod => Data.Mod;

		protected ManualLogSource Logger => Mod.Logger;

		protected Dictionary<Mod, List<DeliModule>> ModModules => Data.ModModules;

		protected abstract string Name { get; }

		/// <summary>
		///		The collection of all the <see cref="ImmediateAssetLoader{TStage}"/>s registered.
		/// </summary>
		public NestedServiceCollection<Mod, string, ImmediateAssetLoader<Stage>> SharedAssetLoaders => Data.SharedAssetLoaders;

		/// <summary>
		///		The collection of all the <see cref="ImmediateReader{T}"/>s publicly available.
		/// </summary>
		public ImmediateReaderCollection ImmediateReaders => Data.ImmediateReaders;

		protected Stage(Blob data)
		{
			Data = data;
		}

		protected IEnumerable<IHandle> Glob(Mod mod, KeyValuePair<string, AssetLoaderID> asset)
		{
			var glob = asset.Key;
			var loader = asset.Value;

			Logger.LogDebug($"Enumerating glob: {glob}");
			using var globbed = mod.Resources.Glob(glob).GetEnumerator();

			if (!globbed.MoveNext())
			{
				Logger.LogWarning($"Asset glob from {mod} of type {loader} did not match any handles: {glob}");
				yield break;
			}

			do
			{
				var handle = globbed.Current!;

				Logger.LogDebug($"{handle} > {loader}");
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

		protected static IFileHandle AssemblyPreloader(IHandle handle)
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

			if (!ModModules.TryGetValue(mod, out var modules))
			{
				modules = new List<DeliModule>();
				ModModules.Add(mod, modules);
			}
			modules.Add(module);

			try
			{
				module.Run(stage);
			}
			catch
			{
				Logger.LogFatal($"{mod} threw an exception upon running a module for the first time.");
				throw;
			}
		}

		protected virtual void AssemblyLoader(Stage stage, Mod mod, Assembly assembly)
		{
			foreach (var type in assembly.GetExportedTypes())
			{
				TypeLoader(stage, mod, type);
			}
		}

		protected void RunModules(Mod mod)
		{
			if (!ModModules.TryGetValue(mod, out var modules)) return;

			Logger.LogDebug($"Loading stage into {mod} modules...");
			foreach (var module in modules)
			{
				try
				{
					module.Run(this);
				}
				catch
				{
					Logger.LogFatal($"{mod} threw an exception upon running a module.");
					throw;
				}
			}
		}

		protected void PreRun()
		{
			Logger.LogDebug($"Running the {Name} stage...");
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
			internal Dictionary<Mod, List<DeliModule>> ModModules { get; }

			internal Blob(Mod mod, ImmediateReaderCollection jsonReaders, JsonSerializer serializer,
				NestedServiceCollection<Mod, string, ImmediateAssetLoader<Stage>> sharedAssetLoaders, ImmediateReaderCollection immediateReaders,
				Dictionary<Mod, List<DeliModule>> modModules)
			{
				Mod = mod;
				JsonReaders = jsonReaders;
				Serializer = serializer;
				SharedAssetLoaders = sharedAssetLoaders;
				ImmediateReaders = immediateReaders;
				ModModules = modModules;
			}
		}
	}

	public abstract class Stage<TLoader> : Stage where TLoader : Delegate
	{
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
