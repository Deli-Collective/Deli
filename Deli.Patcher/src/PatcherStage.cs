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

		protected override Dictionary<string, AssetLoaderID>? GetAssets(Mod.Manifest manifest)
		{
			return manifest.Patchers;
		}

		// IEnumerable<Mod> for when one mod doesn't cause all to fail.
		protected override IEnumerable<Mod> LoadMods(IEnumerable<Mod> mods)
		{
			ImmediateReaders.Add(BytesReader);
			ImmediateReaders.Add(AssemblyReader);
			PatcherAssetLoaders[Mod, DeliConstants.Assets.AssemblyLoader] = AssemblyLoader;

			return base.LoadMods(mods);
		}

		internal IEnumerable<Mod> LoadModsInternal(IEnumerable<Mod> mods) => LoadMods(mods);
	}
}
