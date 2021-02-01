using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Deli.VFS;
using Deli.VFS.Disk;

namespace Deli
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

		protected static byte[] BytesReader(IFileHandle file)
		{
			if (file is IDiskHandle disk)
			{
				return File.ReadAllBytes(disk.PathOnDisk);
			}

			using var raw = file.OpenRead();
			using var memory = new MemoryStream();

			return memory.ToArray();
		}

		protected static Assembly AssemblyReader(IFileHandle file)
		{
			if (file is IDiskHandle disk)
			{
				return Assembly.LoadFile(disk.PathOnDisk);
			}

			var raw = BytesReader(file);
			var symbols = file.WithExtension("mdb");

			return symbols is not IFileHandle symbolsFile ? Assembly.Load(raw) : Assembly.Load(raw, BytesReader(symbolsFile));
		}

		protected void AssemblyLoader(Stage stage, Mod mod, IHandle handle)
		{
			AssemblyLoader(stage, mod, AssemblyReader(AssemblyPreloader(handle)));
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

			foreach (var module in Modules)
			{
				module.RunStage(this);
			}

			InvokeFinished();
		}
	}
}
