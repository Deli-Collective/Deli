using System.Collections.Generic;
using System.IO;
using ADepIn;
using BepInEx.Logging;

namespace Deli.MonoMod.HookGen
{
	internal class AssetLoader : IAssetLoader
	{
		private readonly ManualLogSource _log;
		private readonly Dictionary<string, MemoryStream> _outputs;

		public AssetLoader(ManualLogSource log, Dictionary<string, MemoryStream> outputs)
		{
			_log = log;
			_outputs = outputs;
		}

		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			var config = mod.Resources.Get<IEnumerable<string>>(path).Expect("Hook generation config not found at path: " + path);

			// Add hook generator to each line (DLL file name) in file
			foreach (var entry in config)
			{
				if (_outputs.ContainsKey(entry)) continue;

				var output = new MemoryStream();
				_outputs.Add(entry, output);

				Deli.AddPatcher(entry, new Patcher(_log, output, entry));
			}
		}
	}
}
