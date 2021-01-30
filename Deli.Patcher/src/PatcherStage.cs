using System;
using System.Collections.Generic;
using Deli.VFS;

namespace Deli.Patcher
{
	public class PatcherStage : Stage
	{
		public NestedServiceCollection<Mod, string, ImmediateAssetLoader<PatcherStage>> PatcherAssetLoaders { get; } = new();
		public NestedServiceCollection<string, Mod, Patcher> Patchers { get; } = new();

		internal PatcherStage(Blob data) : base(data)
		{
		}

		private ImmediateAssetLoader<PatcherStage>? GetLoader(Mod mod, string name)
		{
			if (PatcherAssetLoaders.TryGet(mod, name, out var patcher))
			{
				return patcher;
			}

			if (SharedAssetLoaders.TryGet(mod, name, out var shared))
			{
				return shared;
			}

			return null;
		}

		private void LoadMod(Mod mod, Dictionary<string, Mod> lookup)
		{
			var assets = mod.Info.Patchers;
			if (assets is null) return;

			Logger.LogInfo("Loading patchers from " + mod);
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

				foreach (var handle in Glob(mod, asset))
				{
					Logger.LogDebug($"{handle} > {loaderId}");
					loader(this, mod, handle);
				}
			}
		}

		// IEnumerable<Mod> for when one mod doesn't cause all to fail.
		internal IEnumerable<Mod> LoadMods(IEnumerable<Mod> mods)
		{
			var lookup = new Dictionary<string, Mod>();
			foreach (var mod in mods)
			{
				lookup.Add(mod.Info.Guid, mod);

				LoadMod(mod, lookup);
				yield return mod;
			}

			InvokeFinished();
		}
	}
}
