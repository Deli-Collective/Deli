using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Deli.Immediate;
using Deli.VFS;
using Deli.Newtonsoft.Json;
using Deli.Runtime.Yielding;
using Deli.Setup;
using Semver;
using UnityEngine;
using Filesystem = Deli.Bootstrap.Constants.Filesystem;

namespace Deli.Runtime
{
	/// <summary>
	///		The stage of the loading sequence which runs over several frames
	/// </summary>
	public class RuntimeStage : Stage<DelayedAssetLoader>
	{
		private readonly Dictionary<Mod, List<DeliBehaviour>> _modBehaviours;
		private readonly Dictionary<Type, object> _wrapperReaders = new();

#pragma warning disable CS1591

		protected override string Name { get; } = "runtime";

#pragma warning restoreCS1591

		/// <summary>
		///		Asset loaders specific to this stage
		/// </summary>
		public AssetLoaderCollection<DelayedAssetLoader> RuntimeAssetLoaders { get; } = new();

		/// <summary>
		///		The collection of all the <see cref="DelayedReader{T}"/>s publicly available. This does not include wrappers for <see cref="ImmediateReader{T}"/>.
		///		For getting readers including <see cref="ImmediateReader{T}"/> wrappers, use <seealso cref="GetReader{T}"/>.
		/// </summary>
		public DelayedReaderCollection DelayedReaders { get; }

		/// <summary>
		///		The collection of all version checkers for all supported domains
		/// </summary>
		public VersionCheckerCollection VersionCheckers { get; }

		internal RuntimeStage(Blob data, Dictionary<Mod, List<DeliBehaviour>> modBehaviours) : base(data)
		{
			_modBehaviours = modBehaviours;

			DelayedReaders = Readers.DefaultCollection(Logger);
			VersionCheckers = Runtime.VersionCheckers.DefaultCollection();
		}

		private IEnumerator LoadMod(Mod mod, Dictionary<string, Mod> lookup, CoroutineRunner runner, CoroutineStopper stopper)
		{
			var assets = mod.Info.Assets?.Runtime;
			if (assets is null) yield break;

			Logger.LogInfo(Locale.LoadingAssets(mod));
			foreach (var asset in assets)
			{
				var loader = GetLoader(mod, lookup, asset, out var loaderMod);

				Exception? bufferThrow = null;

				var buffer = new Queue<Coroutine>();
				foreach (var handle in Glob(mod, asset))
				{
					IEnumerator TryLoad(IEnumerator loaderDelayed)
					{
						bool MoveNext()
						{
							bool next;
							try
							{
								next = loaderDelayed.MoveNext();
							}
							catch (Exception e)
							{
								// Not fatal; throwing in a coroutine only kills the coroutine. We'll still rethrow for the stacktrace, though.
								Logger.LogFatal(Locale.LoaderException(asset.Value, loaderMod, mod, handle));
								bufferThrow = e;
								throw;
							}

							if (!next)
							{
								Logger.LogDebug($"{handle} >| {asset.Value}");
							}

							return next;
						}

						while (MoveNext())
						{
							yield return loaderDelayed.Current;
						}
					}

					var coroutine = runner(TryLoad(loader(this, mod, handle)));
					buffer.Enqueue(coroutine);
				}

				while (buffer.Count > 0)
				{
					if (bufferThrow is not null)
					{
						while (buffer.Count > 0)
						{
							stopper(buffer.Dequeue());
						}

						throw new InvalidOperationException("An exception was thrown by a loading coroutine. All remaining coroutines have been halted.", bufferThrow);
					}

					yield return buffer.Dequeue();
				}
			}
		}

		private VersionCache? ReadCache(FileInfo file)
		{
			using var raw = file.OpenRead();
			using var text = new StreamReader(raw);
			using var json = new JsonTextReader(text);

			var content = Serializer.Deserialize<Dictionary<string, Timestamped<SemVersion?>>?>(json);

			return content is null ? null : new VersionCache(content);
		}

		private Dictionary<string, VersionCache> ReadCaches()
		{
			var result = new Dictionary<string, VersionCache>();

			foreach (var file in Directory.CreateDirectory(Filesystem.CacheDirectory).GetFiles("*.json"))
			{
				var cache = ReadCache(file);
				if (cache is null) continue;

				var domain = Path.GetFileNameWithoutExtension(file.Name);
				result.Add(domain, cache);
			}

			return result;
		}

		private void WriteCaches(Dictionary<string,VersionCache> caches)
		{
			foreach (var cache in caches)
			{
				using var raw = new FileStream(Filesystem.CacheDirectory + "/" + cache.Key + ".json", FileMode.Create, FileAccess.Write, FileShare.None);
				using var text = new StreamWriter(raw);
				using var json = new JsonTextWriter(text);

				Serializer.Serialize(json, cache.Value.Cached);
			}
		}

		private ResultYieldInstruction<SemVersion?>? CheckVersion(string domain, string path, Dictionary<string, VersionCache> caches)
		{
			if (!caches.TryGetValue(domain, out var cache))
			{
				cache = new VersionCache(new Dictionary<string, Timestamped<SemVersion?>>());
				caches.Add(domain, cache);
			}
			else
			{
				var cached = cache[path];
				if (cached is not null)
				{
					return new DummyYieldInstruction<SemVersion?>(cached.Value.Content);
				}
			}

			if (!VersionCheckers.TryGet(domain, out var checker)) return null;

			var nowPresend = DateTime.UtcNow;
			return checker(path).CallbackWith(version =>
			{
				cache[path] = new Timestamped<SemVersion?>(nowPresend, version);
				return version;
			});
		}

		private IEnumerator CheckVersions(IEnumerable<Mod> mods, CoroutineRunner runner, Dictionary<string, VersionCache> caches)
		{
			var domainFilter = new Regex(@"^(?:https?:\/\/)?(?:[^@\/\n]+@)?(?:www\.)?([^:\/?\n]+)(?:\/)?(.*?)(?:\/)?$", RegexOptions.IgnoreCase);
			var buffer = new Queue<Coroutine>();
			foreach (var mod in mods)
			{
				var source = mod.Info.SourceUrl;
				if (source is null) continue;

				var domainMatch = domainFilter.Match(source);
				if (!domainMatch.Success)
				{
					Logger.LogWarning($"Source URL of {mod} is invalid");
					continue;
				}

				var groups = domainMatch.Groups;
				var domain = groups[1].Value;
				var path = groups[2].Value;

				var send = CheckVersion(domain, path, caches)?.CallbackWith(remoteVersion =>
				{
					if (remoteVersion is null)
					{
						Logger.LogWarning($"No versions of {mod} found at its source URL: {source}");
						return;
					}

					var localVersion = mod.Info.Version;
					switch (localVersion.CompareByPrecedence(remoteVersion))
					{
						case -1:
							Logger.LogWarning($"There is a newer version of {mod} available: {remoteVersion}");
							break;
						case 0:
							Logger.LogInfo($"{mod} is up to date");
							break;
						case 1:
							Logger.LogWarning($"You are ahead of the latest version of {mod}: {remoteVersion}");
							break;

						default: throw new ArgumentOutOfRangeException();
					}
				});

				if (send is not null)
				{
					buffer.Enqueue(runner(send));
				}
			}

			foreach (var coroutine in buffer)
			{
				yield return coroutine;
			}
		}

		private IEnumerator RunCore(IEnumerable<Mod> mods, CoroutineRunner runner, CoroutineStopper stopper)
		{
			var lookup = new Dictionary<string, Mod>();
			foreach (var mod in mods)
			{
				lookup.Add(mod.Info.Guid, mod);

				RunModules(mod);
				RunBehaviours(mod);

				yield return LoadMod(mod, lookup, runner, stopper);
			}
		}

		private void RunBehaviours(Mod mod)
		{
			RunPlugin(mod, _modBehaviours, "behaviour");
		}

#pragma warning disable CS1591

		protected override DelayedAssetLoader? GetLoader(Mod mod, string name)
		{
			if (RuntimeAssetLoaders.TryGet(mod, name, out var delayed))
			{
				return delayed;
			}

			if (!SharedAssetLoaders.TryGet(mod, name, out var shared))
			{
				return null;
			}

			IEnumerator Wrapper(RuntimeStage stage, Mod modClosure, IHandle handle)
			{
				shared(stage, modClosure, handle);
				yield break;
			}

			return Wrapper;
		}

#pragma warning restore CS1591

		/// <summary>
		///		Gets a reader from <seealso cref="DelayedReaders"/>, otherwise gets a reader from <see cref="Stage.ImmediateReaders"/> and wraps it.
		/// </summary>
		/// <typeparam name="T">The type to deserialize.</typeparam>
		public DelayedReader<T> GetReader<T>() where T : notnull
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

		internal IEnumerator Run(IEnumerable<Mod> mods, CoroutineRunner runner, CoroutineStopper stopper)
		{
			PreRun();

			var listed = mods.ToList();
			yield return RunCore(listed, runner, stopper);

			var caches = ReadCaches();
			yield return runner(CheckVersions(listed, runner, caches));
			WriteCaches(caches);

			PostRun();
		}
	}
}
