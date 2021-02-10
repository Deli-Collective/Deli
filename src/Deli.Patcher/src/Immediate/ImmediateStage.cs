using System.Collections.Generic;
using System.Reflection;
using Deli.VFS;

namespace Deli.Immediate
{
	public abstract class ImmediateStage<TStage> : Stage<ImmediateAssetLoader<TStage>> where TStage : ImmediateStage<TStage>
	{
		protected abstract TStage GenericThis { get; }

		public ImmediateStage(Blob data) : base(data)
		{
		}

		protected abstract Dictionary<string, AssetLoaderID>? GetAssets(Mod.AssetTable table);

		private void LoadMod(Mod mod, Dictionary<string, Mod> lookup)
		{
			var table = mod.Info.Assets;
			if (table is null) return;

			var assets = GetAssets(table);
			if (assets is null) return;

			Logger.LogDebug(Locale.LoadingAssets(mod));
			foreach (var asset in assets)
			{
				var loader = GetLoader(mod, lookup, asset, out var loaderMod);

				foreach (var handle in Glob(mod, asset))
				{
					try
					{
						loader(GenericThis, mod, handle);
					}
					catch
					{
						Logger.LogFatal(Locale.LoaderException(asset.Value, loaderMod, mod, handle));
						throw;
					}
				}
			}
		}

		protected void AssemblyLoader(Stage stage, Mod mod, IHandle handle)
		{
			var assembly = ImmediateReaders.Get<Assembly>()(AssemblyPreloader(handle));
			AssemblyLoader(stage, mod, assembly);
		}

		protected IEnumerable<Mod> Run(IEnumerable<Mod> mods)
		{
			PreRun();

			var lookup = new Dictionary<string, Mod>();
			foreach (var mod in mods)
			{
				lookup.Add(mod.Info.Guid, mod);

				RunModules(mod);
				LoadMod(mod, lookup);

				yield return mod;
			}
		}
	}
}
