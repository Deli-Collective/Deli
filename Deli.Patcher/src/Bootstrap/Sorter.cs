using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;

namespace Deli.Patcher.Bootstrap
{
	internal class Sorter
	{
		private ManualLogSource _logger;

		public Sorter(ManualLogSource logger)
		{
			_logger = logger;
		}

		private bool CheckDependencies(Dictionary<string, Mod> lookup)
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

			return true;
		}

		private Dictionary<string, Mod> CreateLookup(IEnumerable<Mod> mods)
		{
			var lookup = new Dictionary<string, Mod>();
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

			return lookup;
		}

		public IEnumerable<Mod> Run(IEnumerable<Mod> mods)
		{
			var lookup = CreateLookup(mods);
			_logger.LogInfo($"Found {lookup.Count} mods to load");

			if (!CheckDependencies(lookup))
			{
				_logger.LogError("One or more dependencies are not satisfied. Aborting initialization.");
				return Enumerable.Empty<Mod>();
			}

			return lookup.Values.TSort(m => m.Info.Dependencies?.Keys.Select(dep => lookup[dep]) ?? Enumerable.Empty<Mod>());
		}
	}
}
