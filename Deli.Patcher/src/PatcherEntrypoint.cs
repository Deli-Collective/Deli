using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Deli.Patcher.Bootstrap;
using Mono.Cecil;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Metadata = Deli.DeliConstants.Metadata;
using Git = Deli.DeliConstants.Git;

namespace Deli.Patcher
{
	public delegate void StageHandoff(ManualLogSource logger, Dictionary<string, ISharedAssetLoader> sharedAssetLoaders, ImmediateReaderCollection immediateReaders);

    public static class PatcherEntrypoint
    {
	    private static ManualLogSource _logger = Logger.CreateLogSource(Metadata.Name);
		private static Dictionary<string, List<IPatcher>> _filePatchers = new();
		private static IEnumerable<IPatcher>? _activePatchers;

		public static IEnumerable<string> TargetDLLs
		{
			get
			{
				foreach (var localPatchers in _filePatchers)
				{
					_activePatchers = localPatchers.Value;
					yield return localPatchers.Key;
				}

				_activePatchers = null;
			}
		}

		public static void Initialize()
		{
			_logger.LogInfo($"Deli bootstrap has begun! Version {Metadata.Version} ({Git.Branch} @ {Git.Describe})");

			var serializer = JsonSerializer.Create(new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				ContractResolver = new DefaultContractResolver
				{
					NamingStrategy = new SnakeCaseNamingStrategy()
				}
			});
			var sharedLoaders = new Dictionary<string, ISharedAssetLoader>();
			var immediateReaders = new ImmediateReaderCollection(_logger);
			var stage = new PatcherStage(_logger, serializer, sharedLoaders, immediateReaders, _filePatchers);

			var manifestReader = stage.RegisterImmediateJson<Mod.Manifest>();
			var discovery = new Discovery(_logger, manifestReader);

			foreach (var mod in Sort(discovery.DiscoverMods()))
			{
				var manifest = mod.Info;
			}
		}

		private static bool CheckDependencies(Dictionary<string, Mod> lookup)
		{
			foreach (var mod in lookup.Values)
			{
				var deps = mod.Info.Dependencies;
				if (deps is null) continue;

				foreach (var dep in deps)
				{
					string DepToString()
					{
						return $"{dep.Key} @ {dep.Value}";
					}

					// Try finding the installed dependency
					if (!lookup.TryGetValue(dep.Key, out var resolved))
					{
						_logger.LogError($"Mod {mod} depends on {DepToString()}, but it is not installed!");
						return false;
					}

					// Check if the installed version satisfies the dependency request
					if (!resolved.Info.Version.Satisfies(dep.Value))
					{
						_logger.LogError($"Mod {mod} depends on {DepToString()}, but version {resolved.Info.Version} is installed!");
						return false;
					}
				}
			}
		}

		private static IEnumerable<Mod> Sort(IEnumerable<Mod> mods)
		{
			var lookup = new Dictionary<string, Mod>();
			{
				var conflicts = new Dictionary<string, List<Mod>>();
				foreach (var mod in mods)
				{
					var guid = mod.Info.Guid;
					if (conflicts.TryGetValue(guid, out var conflicting))
					{
						conflicting.Add(mod);
						continue;
					}

					if (lookup.ContainsKey(guid))
					{
						lookup.Remove(guid);

						conflicting = new List<Mod> {mod};
						conflicts.Add(guid, conflicting);
						continue;
					}

					lookup.Add(guid, mod);
				}

				foreach (var conflict in conflicts)
				{
					_logger.LogError($"GUID conflict found between {conflict.Value.Select(m => m.ToString()).JoinStr(", ")} ({conflict.Key}). These mods will not be loaded.");
				}
			}

			_logger.LogInfo($"Found {lookup.Count} mods to load");

			if (!CheckDependencies(lookup))
			{
				_logger.LogError("One or more dependencies are not satisfied. Aborting initialization.");
				return Enumerable.Empty<Mod>();
			}

			return lookup.Values.TSort(m => m.Info.Dependencies?.Keys.Select(dep => lookup[dep]) ?? Enumerable.Empty<Mod>());
		}

		public static void Patch(ref AssemblyDefinition assembly)
		{
			if (_activePatchers is null)
			{
				throw new InvalidOperationException("Implicit contract broken: target DLLs was not enumerated before the patch method.");
			}

			foreach (var patcher in _activePatchers)
			{
				patcher.Patch(ref assembly);
			}
		}

		public static void Handoff(StageHandoff callback)
		{
			PatcherStage.Handoff(callback);
		}
	}
}
