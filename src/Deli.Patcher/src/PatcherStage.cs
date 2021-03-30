using System.Collections.Generic;
using Deli.Bootstrap;
using Deli.Immediate;

namespace Deli.Patcher
{
	/// <summary>
	///		The immediate stage of the loading sequence which allows for patching assemblies
	/// </summary>
	public sealed class PatcherStage : ImmediateStage<PatcherStage>
	{
#pragma warning disable CS1591

		protected override string Name { get; } = "patcher";
		protected override PatcherStage GenericThis => this;

#pragma warning restore CS1591

		/// <summary>
		///		Asset loaders specific to this stage
		/// </summary>
		public AssetLoaderCollection<AssetLoader<PatcherStage, Empty>> PatcherAssetLoaders { get; } = new();

		/// <summary>
		///		Assembly patchers, which are executed after this stage, but before the setup stage
		/// </summary>
		public PatcherCollection Patchers { get; } = new();

		internal PatcherStage(Blob data) : base(data)
		{
			Readers.Add(ModManifestOf);
			PatcherAssetLoaders[Mod, Constants.Assets.AssemblyLoaderName] = AssemblyLoader;
			ModModules.Add(Mod, new List<DeliModule>
			{
				new Module(Mod)
			});
		}

#pragma warning disable CS1591

		protected override AssetLoader<PatcherStage, Empty>? GetLoader(Mod mod, string name)
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

		protected override Mod.Asset[]? GetAssets(Mod.AssetTable table)
		{
			return table.Patcher;
		}

#pragma warning restore CS1591

		internal IEnumerable<Mod> RunInternal(IEnumerable<Mod> mods) => Run(mods);
	}
}
