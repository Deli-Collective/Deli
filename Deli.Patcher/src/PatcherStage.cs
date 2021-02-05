using System.Collections.Generic;

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

		// IEnumerable<Mod> for when one mod doesn't cause all to fail.
		protected override IEnumerable<Mod> Run(IEnumerable<Mod> mods)
		{
			ImmediateReaders.Add(BytesReader);
			ImmediateReaders.Add(AssemblyReader);
			PatcherAssetLoaders[Mod, DeliConstants.Assets.AssemblyLoader] = AssemblyLoader;

			return base.Run(mods);
		}

		internal IEnumerable<Mod> RunInternal(IEnumerable<Mod> mods) => Run(mods);
	}
}
