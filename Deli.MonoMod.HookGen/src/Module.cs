using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Deli.MonoMod.HookGen
{
	internal class Module : DeliModule
	{
		private readonly Dictionary<string, MemoryStream> _outputs;

		public Module()
		{
			_outputs = new Dictionary<string, MemoryStream>();

			Deli.AddAssetLoader("monomod.hookgen", new AssetLoader(Logger, _outputs));
			Deli.RuntimeStart += LoadOutputs;
		}

		private void LoadOutputs()
		{
			foreach (var output in _outputs)
			{
				Logger.LogInfo("Loading hooks for " + output);
				Assembly.Load(output.Value.ToArray());
			}

			// Allow the GC to clean us up
			Deli.RuntimeStart -= LoadOutputs;
		}
	}
}
