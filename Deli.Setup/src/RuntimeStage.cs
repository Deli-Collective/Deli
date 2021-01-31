using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using Deli.Patcher;
using Deli.VFS;
using Deli.VFS.Disk;
using UnityEngine;

namespace Deli.Setup
{
	public class RuntimeStage : Stage<DelayedAssetLoader>
	{
		private readonly Dictionary<Type, object> _wrapperReaders = new();

		protected override string Name { get; } = "runtime";

		public NestedServiceCollection<Mod, string, DelayedAssetLoader> DelayedAssetLoaders { get; } = new();

		/// <summary>
		///		The collection of all the <see cref="DelayedReader{T}"/>s publicly available. This does not include wrappers for <see cref="ImmediateReader{T}"/>.
		///		For getting readers including <see cref="ImmediateReader{T}"/> wrappers, use <seealso cref="GetReader{T}"/>.
		/// </summary>
		public DelayedReaderCollection DelayedReaders { get; }

		internal RuntimeStage(Blob data) : base(data)
		{
			DelayedReaders = new DelayedReaderCollection(Logger);
		}

		protected override DelayedAssetLoader? GetLoader(Mod mod, string name)
		{
			if (DelayedAssetLoaders.TryGet(mod, name, out var delayed))
			{
				return delayed;
			}

			if (!SharedAssetLoaders.TryGet(mod, name, out var shared))
			{
				return null;
			}

			IEnumerator Wrapper(RuntimeStage stage, Mod mod, IHandle handle)
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

		private static ResultYieldInstruction<byte[]> BytesReader(IFileHandle file)
		{
			var stream = file.OpenRead();
			var buffer = new byte[stream.Length];

			return new AsyncYieldInstruction<Stream>(stream, (self, callback, state) => self.BeginRead(buffer, 0, buffer.Length, callback, state),
				(self, result) => self.EndRead(result)).CallbackWith(() =>
			{
				stream.Dispose();
				return buffer;
			});
		}

		private static ResultYieldInstruction<Assembly> AssemblyReader(IFileHandle file)
		{
			if (file is IDiskHandle disk)
			{
				return new DummyYieldInstruction<Assembly>(Assembly.LoadFile(disk.PathOnDisk));
			}

			var raw = BytesReader(file);

			if (file.WithExtension("mdb") is not IFileHandle symbols)
			{
				return raw.CallbackWith(Assembly.Load);
			}

			var symbolsRaw = BytesReader(symbols);
			return raw.ContinueWith(() => symbolsRaw).CallbackWith(() => Assembly.Load(raw.Result, symbolsRaw.Result));
		}

		private IEnumerator LoadMod(Mod mod, Dictionary<string, Mod> lookup, CoroutineRunner runner)
		{
			var assets = mod.Info.Runtime;
			if (assets is null) yield break;

			Logger.LogInfo("Loading assets from " + mod);
			foreach (var asset in assets)
			{
				var loader = GetLoader(mod, lookup, asset);
				var buffer = new Queue<Coroutine>();
				foreach (var handle in Glob(mod, asset))
				{
					var coroutine = runner(loader(this, mod, handle));
					buffer.Enqueue(coroutine);
				}

				while (buffer.Count > 0)
				{
					yield return buffer.Dequeue();
				}
			}
		}

		private IEnumerator AssemblyLoader(RuntimeStage stage, Mod mod, IHandle handle)
		{
			yield return AssemblyReader(AssemblyPreloader(handle)).CallbackWith(assembly => AssemblyLoader(stage, mod, assembly));
		}

		internal IEnumerator LoadMods(IEnumerable<Mod> mods, CoroutineRunner runner)
		{
			DelayedReaders.Add(BytesReader);
			DelayedReaders.Add(AssemblyReader);
			DelayedAssetLoaders[Mod, DeliConstants.Assets.AssemblyLoader] = AssemblyLoader;

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
