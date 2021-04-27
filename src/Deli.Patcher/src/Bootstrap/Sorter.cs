using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Deli.Patcher.Exceptions;

namespace Deli.Bootstrap
{
	internal class Sorter
	{
		private readonly Bootstrapper _bootstrapper;
		private readonly ManualLogSource _logger;

		public Sorter(Bootstrapper bootstrapper, ManualLogSource logger)
		{
			_bootstrapper = bootstrapper;
			_logger = logger;
		}

		private IEnumerable<Mod> CheckDependencies(Dictionary<string, Mod> lookup, IEnumerable<Mod> sorted)
		{
			foreach (var mod in sorted)
			{
				// Check the required Deli version
				if (mod.Info.Require > Constants.Metadata.Version)
				{
					_logger.LogError($"Mod {mod} requires Deli {mod.Info.Require} or greater, please update Deli to use this mod!");
					mod.State.ExceptionsInternal.Add(new DeliUnsatisfiedDependencyException(mod, _bootstrapper.Mod.Info, mod.Info.Require));
					mod.State.IsDisabled = true;
				}

				// Check dependencies
				var deps = mod.Info.Dependencies;
				if (deps is not null)
				{
					foreach (var dep in deps)
					{
						// Try finding the installed dependency
						if (!lookup.TryGetValue(dep.Key, out var resolved))
						{
							_logger.LogError($"Mod {mod} depends on {dep.Key} @ {dep.Value}, but it is not installed");
							mod.State.ExceptionsInternal.Add(new DeliUnsatisfiedDependencyException(mod, dep.Key, dep.Value));
							mod.State.IsDisabled = true;
						}

						// Check if the installed version satisfies the dependency request
						if (resolved.Info.Version.CompareByPrecedence(dep.Value) == -1)
						{
							_logger.LogError($"Mod {mod} depends on {resolved.Info.Name ?? resolved.Info.Guid} @ {dep.Value}, but version {resolved.Info.Version} is installed");
							mod.State.ExceptionsInternal.Add(new DeliUnsatisfiedDependencyException(mod, resolved.Info, dep.Value));
							mod.State.IsDisabled = true;
						}

						// Check if the dependency is already disabled
						if (resolved.State.IsDisabled)
						{
							_logger.LogWarning($"Mod {mod} will not be loaded because it's dependency {resolved.Info.Name ?? resolved.Info.Guid} was not loaded.");
							mod.State.IsDisabled = true;
						}
					}
				}

				if (!mod.State.IsDisabled) yield return mod;
			}
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
			return CheckDependencies(lookup, lookup.Values.TSort(m => m.Info.Dependencies?.Keys.Select(dep => lookup[dep]) ?? Enumerable.Empty<Mod>()));
		}
	}
}
