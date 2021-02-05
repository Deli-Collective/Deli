using System;
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
					// Try finding the installed dependency
					if (!lookup.TryGetValue(dep.Key, out var resolved))
					{
						_logger.LogFatal($"Mod {mod} depends on {dep.Key} @ {dep.Value}, but it is not installed");
						return false;
					}

					// Check if the installed version satisfies the dependency request
					if (resolved.Info.Version.CompareByPrecedence(dep.Value) == -1)
					{
						_logger.LogFatal($"Mod {mod} depends on {resolved.Info.Name ?? resolved.Info.Guid} @ {dep.Value}, but version {resolved.Info.Version} is installed");
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
				// Used to log, but now throws so the user knows something is wrong
				throw new InvalidOperationException("One or more dependencies are not satisfied.");
			}

			return lookup.Values.TSort(m => m.Info.Dependencies?.Keys.Select(dep => lookup[dep]) ?? Enumerable.Empty<Mod>());
		}
	}
}
