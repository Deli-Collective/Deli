using System;
using System.Collections.Generic;

namespace Deli.Patcher.Common
{
	public abstract class ImmediateStage<TStage> : Stage<ImmediateAssetLoader<TStage>> where TStage : ImmediateStage<TStage>
	{
		protected abstract TStage GenericThis { get; }

		public ImmediateStage(Blob data) : base(data)
		{
		}

		protected abstract Dictionary<string, AssetLoaderID>? GetAssets(Mod.Manifest manifest);

		private void LoadMod(Mod mod, Dictionary<string, Mod> lookup)
		{
			var assets = GetAssets(mod.Info);
			if (assets is null) return;

			Logger.LogInfo($"Loading {Name} assets from {mod}");
			foreach (var asset in assets)
			{
				var loader = GetLoader(mod, lookup, asset);

				foreach (var handle in Glob(mod, asset))
				{
					loader(GenericThis, mod, handle);
				}
			}
		}

		protected virtual IEnumerable<Mod> LoadMods(IEnumerable<Mod> mods)
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
