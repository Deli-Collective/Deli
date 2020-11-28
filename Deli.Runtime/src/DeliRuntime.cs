using System.Collections.Generic;
using ADepIn;
using ADepIn.Fluent;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
	[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
	public class DeliRuntime : BaseUnityPlugin, IModule
	{
		public DeliRuntime()
		{
			Entrypoint.Postpatch(this);
		}

		public void Load(IServiceKernel kernel)
		{
			var log = kernel.Get<ManualLogSource>().Unwrap();
			log.LogDebug("Injecting runtime loader...");

			var manager = new GameObject("Deli Manager");
			var loader = new RuntimeAssemblyAssetLoader(manager, log);

			var loaders = kernel.Get<IDictionary<string, IAssetLoader>>().Unwrap();
			loaders[Constants.AssemblyLoaderName] = loader;
		}
	}
}
