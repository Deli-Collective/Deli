using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Deli.VFS;
using Deli.VFS.Disk;
using Deli.Newtonsoft.Json;
using Semver;
using UnityEngine;

namespace Deli.Setup
{
	public class RuntimeStage : Stage<DelayedAssetLoader>
	{
		private readonly Dictionary<Mod, List<DeliBehaviour>> _modBehaviours;
		private readonly Dictionary<Type, object> _wrapperReaders = new();

		protected override string Name { get; } = "runtime";

		public NestedServiceCollection<Mod, string, DelayedAssetLoader> DelayedAssetLoaders { get; }

		/// <summary>
		///		The collection of all the <see cref="DelayedReader{T}"/>s publicly available. This does not include wrappers for <see cref="ImmediateReader{T}"/>.
		///		For getting readers including <see cref="ImmediateReader{T}"/> wrappers, use <seealso cref="GetReader{T}"/>.
		/// </summary>
		public DelayedReaderCollection DelayedReaders { get; }

		public VersionCheckerCollection VersionCheckers { get; }

		internal RuntimeStage(Blob data, Dictionary<Mod, List<DeliBehaviour>> modBehaviours) : base(data)
		{
			_modBehaviours = modBehaviours;
			DelayedReaders = new DelayedReaderCollection(Logger)
			{
				BytesReader,
				AssemblyReader
			};
			DelayedAssetLoaders = new NestedServiceCollection<Mod, string, DelayedAssetLoader>
			{
				[Mod, DeliConstants.Assets.AssemblyLoader] = AssemblyLoader
			};
			VersionCheckers = Setup.VersionCheckers.DefaultCollection();
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
			var assets = mod.Info.Assets?.Runtime;
			if (assets is null) yield break;

			Logger.LogDebug($"Loading assets from {mod}...");
			foreach (var asset in assets)
			{
				var loader = GetLoader(mod, lookup, asset, out var loaderMod);

				var buffer = new Queue<Coroutine>();
				foreach (var handle in Glob(mod, asset))
				{
					IEnumerator TryLoad(IEnumerator loaderDelayed)
					{
						bool MoveNext()
						{
							try
							{
								return loaderDelayed.MoveNext();
							}
							catch
							{
								// Not fatal; throwing in a coroutine only kills the coroutine. We'll still rethrow for the stacktrace, though.
								Logger.LogError(Locale.LoaderException(asset.Value, loaderMod, mod, handle));
								throw;
							}
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
					yield return buffer.Dequeue();
				}
			}
		}

		private IEnumerator AssemblyLoader(RuntimeStage stage, Mod mod, IHandle handle)
		{
			yield return AssemblyReader(AssemblyPreloader(handle)).CallbackWith(assembly => AssemblyLoader(stage, mod, assembly));
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

			foreach (var file in Directory.CreateDirectory(DeliConstants.Filesystem.CacheDirectory).GetFiles("*.json"))
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
				using var raw = new FileStream(DeliConstants.Filesystem.CacheDirectory + "/" + cache.Key + ".json", FileMode.Create, FileAccess.Write, FileShare.None);
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
							Logger.LogWarning($"There is a newer version of {mod} available: ({remoteVersion})");
							break;
						case 0:
							Logger.LogInfo($"{mod} is up to date");
							break;
						case 1:
							Logger.LogWarning($"You are ahead of the latest version of {mod}: ({remoteVersion})");
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

		private IEnumerator RunCore(IEnumerable<Mod> mods, CoroutineRunner runner)
		{
			var lookup = new Dictionary<string, Mod>();
			foreach (var mod in mods)
			{
				lookup.Add(mod.Info.Guid, mod);

				RunModules(mod);
				RunBehaviours(mod);

				yield return LoadMod(mod, lookup, runner);
			}
		}

		private void RunBehaviours(Mod mod)
		{
			const string pluginType = "behaviour";
			if (!_modBehaviours.TryGetValue(mod, out var behaviours)) return;

			Logger.LogDebug(Locale.LoadingPlugin(mod, pluginType));
			foreach (var behaviour in behaviours)
			{
				try
				{
					behaviour.Run(this);
				}
				catch
				{
					Logger.LogFatal(Locale.PluginException(mod, pluginType));
					throw;
				}
			}
		}

		internal IEnumerator Run(IEnumerable<Mod> mods, CoroutineRunner runner)
		{
			PreRun();

			var listed = mods.ToList();
			yield return RunCore(listed, runner);

			Logger.LogInfo("Finished Deli stage loading.");

			var caches = ReadCaches();
			yield return runner(CheckVersions(listed, runner, caches));
			WriteCaches(caches);

			Logger.LogInfo("Finished checking all mod versions.");
		}
	}
}
