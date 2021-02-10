using System.Collections.Generic;
using Deli.Bootstrap;
using Deli.Immediate;

namespace Deli.Patcher
{
	public class PatcherStage : ImmediateStage<PatcherStage>
	{
		protected override string Name { get; } = "patcher";
		protected override PatcherStage GenericThis => this;

		public NestedServiceCollection<Mod, string, ImmediateAssetLoader<PatcherStage>> PatcherAssetLoaders { get; } = new();
		public NestedServiceCollection<string, Mod, Patcher> Patchers { get; } = new();

		internal PatcherStage(Blob data) : base(data)
		{
			ImmediateReaders.Add(ModManifestOf);
			PatcherAssetLoaders[Mod, Constants.Assets.AssemblyLoader] = AssemblyLoader;
		}

		protected override ImmediateAssetLoader<PatcherStage>? GetLoader(Mod mod, string name)
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

		protected override Dictionary<string, AssetLoaderID>? GetAssets(Mod.AssetTable table)
		{
			return table.Patcher;
		}

		internal IEnumerable<Mod> RunInternal(IEnumerable<Mod> mods) => Run(mods);
	}
}
