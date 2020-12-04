using System.Collections.Generic;
using System.IO;
using ADepIn;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Mono.Cecil;

namespace Deli.MonoMod
{
	internal class MonoModAssetLoader : IAssetLoader
	{
		private static readonly HashSet<string> _resolutionDirs = new HashSet<string>
		{
			Paths.BepInExAssemblyDirectory,
			Paths.ManagedPath,
			Paths.PatcherPluginPath,
			Paths.PluginPath
		};

		private static AssemblyDefinition ResolveFallback(object _, AssemblyNameReference name)
		{
			return TypeLoader.Resolver.Resolve(name);
		}

		private readonly ManualLogSource _log;
		private readonly DefaultAssemblyResolver _resolver;
		private readonly Dictionary<string, List<byte[]>> _monomods;

		public MonoModAssetLoader(ManualLogSource log)
		{
			_log = log;
			_resolver = new DefaultAssemblyResolver();
			_monomods = new Dictionary<string, List<byte[]>>();

			_resolver.ResolveFailure += ResolveFallback;
			foreach (var dir in _resolutionDirs)
			{
				_resolver.AddSearchDirectory(dir);
			}
		}

		private void AddMod(string fileName, byte[] monomod)
		{
			var fileMonomods = _monomods.GetOrInsertWith(fileName, () =>
			{
				var monomods = new List<byte[]>();

				var patcher = new MonoModPatcher(_log, _resolver, monomods);
				Deli.AddPatcher(fileName, patcher);

				return monomods;
			});

			fileMonomods.Add(monomod);
		}

		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			var monomod = mod.Resources.Get<byte[]>(path).Expect("MonoMod assembly not found at path: " + path);

			// dir/DllName.mm.dll -> DllName.mm.dll
			var fileName = Path.GetFileName(path);
			// DllName.mm.dll -> DllName.dll
			var originalFileName = Path.ChangeExtension(Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(fileName)), "dll");

			AddMod(originalFileName, monomod);
		}
	}
}
