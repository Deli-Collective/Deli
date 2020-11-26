using System.Collections.Generic;
using ADepIn;
using ADepIn.Fluent;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Deli
{
	[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
	public class DeliPostpatch : BaseUnityPlugin, IModule
	{
		public DeliPostpatch()
		{
			Deli.Runtime(this);
		}

		public void Load(IServiceKernel kernel)
		{
			var manager = new GameObject("Deli Manager");
			kernel.Bind<GameObject>().ToConstant(manager);

			var log = kernel.Get<ManualLogSource>().Unwrap();
			var loader = new RuntimeAssemblyAssetLoader(manager, log);

			var loaders = kernel.Get<IDictionary<string, IAssetLoader>>().Unwrap();
			loaders.Add("assembly", loader);
		}
	}
}
