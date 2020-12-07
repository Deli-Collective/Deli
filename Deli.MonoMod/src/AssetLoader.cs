using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ADepIn;
using BepInEx.Logging;
using MonoMod;

namespace Deli.MonoMod
{
	internal class AssetLoader : IAssetLoader
	{
		private readonly ManualLogSource _log;
		private readonly Dictionary<string, List<byte[]>> _mods;

		public AssetLoader(ManualLogSource log)
		{
			_log = log;
			_mods = new Dictionary<string, List<byte[]>>();
		}

		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			void ThrowInvalidFormat()
			{
				throw new FormatException("MonoMod assemblies must be '.mm.dll' files: " + path);
			}

			// dir/DllName.mm.dll -> DllName.mm.dll
			var fileName = Path.GetFileName(path);
			if (Path.GetExtension(fileName) != "dll") ThrowInvalidFormat();

			// DllName.mm.dll -> DllName.mm
			var fileNameMM = Path.GetFileNameWithoutExtension(fileName);
			if (Path.GetExtension(fileNameMM) != "mm") ThrowInvalidFormat();

			// DllName.mm -> DllName
			var fileNameNoExtension = Path.GetFileNameWithoutExtension(fileNameMM);
			// DllName -> DllName.dll
			var originalFileName = Path.ChangeExtension(fileNameNoExtension, "dll");

			var raw = mod.Resources.Get<byte[]>(path).Expect("MonoMod assembly not found at path: " + path);
			_mods.GetOrInsertWith(originalFileName, () =>
			{
				var mods = new List<byte[]>();
				Deli.AddPatcher(originalFileName, new Patcher(_log, mods, fileNameNoExtension));

				return mods;
			}).Add(raw);
		}
	}
}
