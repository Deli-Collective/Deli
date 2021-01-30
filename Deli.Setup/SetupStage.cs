using System;
using System.Collections;
using System.Collections.Generic;
using Deli.Patcher;
using Deli.VFS;
using UnityEngine;

namespace Deli.Setup
{
	public class SetupStage : Stage
	{
		private readonly Dictionary<Type, object> _wrapperReaders = new();

		public NestedServiceCollection<Mod, string, DelayedAssetLoader> DelayedAssetLoaders { get; } = new();

		/// <summary>
		///		The collection of all the <see cref="DelayedReader{T}"/>s publicly available. This does not include wrappers for <see cref="ImmediateReader{T}"/>.
		///		For getting readers including <see cref="ImmediateReader{T}"/> wrappers, use <seealso cref="GetReader{T}"/>.
		/// </summary>
		public DelayedReaderCollection DelayedReaders { get; }

		internal SetupStage(Blob data) : base(data)
		{
			DelayedReaders = new DelayedReaderCollection(Logger);
		}

		private DelayedAssetLoader? GetLoader(Mod mod, string name)
		{
			if (DelayedAssetLoaders.TryGet(mod, name, out var delayed))
			{
				return delayed;
			}

			if (!SharedAssetLoaders.TryGet(mod, name, out var shared))
			{
				return null;
			}

			IEnumerator Wrapper(SetupStage stage, Mod mod, IHandle handle)
			{
				shared(stage, mod, handle);
				yield break;
			}

			return Wrapper;
		}

		/// <summary>
		///		Gets a reader from <seealso cref="DelayedReaders"/>, otherwise gets a reader from <see cref="Stage.ImmediateReaders"/> and wraps it.
		/// </summary>
		/// <typeparam name="T">The type to deserialize.</typeparam>
		public DelayedReader<T> GetReader<T>()
		{
			var type = typeof(T);
			if (DelayedReaders.TryGet<T>(out var reader))
			{
				_wrapperReaders.Remove(type);
				return reader;
			}

			if (_wrapperReaders.TryGetValue(type, out var obj))
			{
				return (DelayedReader<T>) obj;
			}

			var immediate = ImmediateReaders.Get<T>();
			DelayedReader<T> wrapper = handle => new DummyYieldInstruction<T>(immediate(handle));
			_wrapperReaders.Add(typeof(T), wrapper);

			return wrapper;
		}

		private IEnumerator LoadMod(Mod mod, Dictionary<string, Mod> lookup, CoroutineRunner runner)
		{
			var assets = mod.Info.Assets;
			if (assets is null) yield break;

			Logger.LogInfo("Loading assets from " + mod);
			foreach (var asset in assets)
			{
				var loaderId = asset.Value;

				if (!lookup.TryGetValue(loaderId.Mod, out var loaderMod))
				{
					throw new InvalidOperationException($"Mod required for asset \"{asset.Key}\" of {mod} was not present: {loaderId.Mod}");
				}

				var loader = GetLoader(loaderMod, loaderId.Name);
				if (loader is null)
				{
					throw new InvalidOperationException($"Loader required for asset \"{asset.Key}\" of {mod} was not present.");
				}

				var buffer = new Queue<Coroutine>();
				foreach (var handle in Glob(mod, asset))
				{
					Logger.LogDebug($"{handle} > {loaderId}");
					var coroutine = runner(loader(this, mod, handle));
					buffer.Enqueue(coroutine);
				}

				while (buffer.Count > 0)
				{
					yield return buffer.Dequeue();
				}
			}
		}

		public IEnumerator LoadMods(IEnumerable<Mod> mods, CoroutineRunner runner)
		{
			var lookup = new Dictionary<string, Mod>();
			foreach (var mod in mods)
			{
				lookup.Add(mod.Info.Guid, mod);

				yield return LoadMod(mod, lookup, runner);
			}

			InvokeFinished();
		}
	}
}
