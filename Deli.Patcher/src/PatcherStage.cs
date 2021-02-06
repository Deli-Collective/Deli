using System;
using System.Collections.Generic;
using Deli.VFS;
using Newtonsoft.Json.Linq;

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

		private static JObject JObjectReader(IFileHandle file)
		{
			return JTokenReader(file) as JObject ?? throw new FormatException("Expected a JSON object");
		}

		// IEnumerable<Mod> for when one mod doesn't cause all to fail.
		protected override IEnumerable<Mod> Run(IEnumerable<Mod> mods)
		{
			ImmediateReaders.Add(JTokenReader);
			ImmediateReaders.Add(JObjectReader);
			ImmediateReaders.Add(ModManifestReader);
			ImmediateReaders.Add(BytesReader);
			ImmediateReaders.Add(AssemblyReader);
			PatcherAssetLoaders[Mod, DeliConstants.Assets.AssemblyLoader] = AssemblyLoader;

			return base.Run(mods);
		}

		internal IEnumerable<Mod> RunInternal(IEnumerable<Mod> mods) => Run(mods);
	}
}
