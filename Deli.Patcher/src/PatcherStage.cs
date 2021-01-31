using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Deli.Patcher.Common;
using Deli.VFS;
using Deli.VFS.Disk;

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

		private static byte[] BytesReader(IFileHandle file)
		{
			if (file is IDiskHandle disk)
			{
				return File.ReadAllBytes(disk.PathOnDisk);
			}

			using var raw = file.OpenRead();
			using var memory = new MemoryStream();

			return memory.ToArray();
		}

		private static Assembly AssemblyReader(IFileHandle file)
		{
			if (file is IDiskHandle disk)
			{
				return Assembly.LoadFile(disk.PathOnDisk);
			}

			var raw = BytesReader(file);
			var symbols = file.WithExtension("mdb");

			return symbols is not IFileHandle symbolsFile ? Assembly.Load(raw) : Assembly.Load(raw, BytesReader(symbolsFile));
		}

		private void AssemblyLoader(Stage stage, Mod mod, IHandle handle)
		{
			AssemblyLoader(stage, mod, AssemblyReader(AssemblyPreloader(handle)));
		}

		// IEnumerable<Mod> for when one mod doesn't cause all to fail.
		protected override IEnumerable<Mod> LoadMods(IEnumerable<Mod> mods)
		{
			ImmediateReaders.Add(BytesReader);
			ImmediateReaders.Add(AssemblyReader);
			SharedAssetLoaders[Mod, DeliConstants.Assets.AssemblyLoader] = AssemblyLoader;

			return base.LoadMods(mods);
		}

		internal IEnumerable<Mod> LoadModsInternal(IEnumerable<Mod> mods) => LoadMods(mods);
	}
}
