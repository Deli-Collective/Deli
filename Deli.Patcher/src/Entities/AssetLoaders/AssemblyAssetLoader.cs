using System;
using System.Reflection;
using ADepIn;
using BepInEx.Logging;

namespace Deli
{
	public class AssemblyAssetLoader : IAssetLoader
	{
		protected ManualLogSource Log { get; }

		public AssemblyAssetLoader(ManualLogSource log)
		{
			Log = log;
		}

		public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
		{
			// Load the assembly and scan it for new module loaders and resource type loaders
			var rawAssembly = mod.Resources.Get<byte[]>(path).Expect("Assembly not found at path: " + path);

			// If the assembly debugging symbols are also included load those too.
			var assembly = mod.Resources.Get<byte[]>(path + ".mdb").MatchSome(out var symbols) ? Assembly.Load(rawAssembly, symbols) : Assembly.Load(rawAssembly);

			foreach (var type in assembly.GetTypesSafe(Log))
			{
				// If the type is a kernel entry module, load it and continue
				if (kernel.LoadEntryType(type).IsSome) continue;

				// Check if the type has either quick bind
				if (QuickBindUtilizer.TryBind(kernel, type)) continue;

				TypeCallback(kernel, mod, path, type);
			}
		}

		protected virtual void TypeCallback(IServiceKernel kernel, Mod mod, string path, Type type)
		{
		}
	}
}
