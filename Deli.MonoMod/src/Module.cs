using ADepIn;
using BepInEx.Logging;

namespace Deli.MonoMod
{
	internal class Module : IEntryModule<Module>
	{
		public void Load(IServiceKernel kernel)
		{
			var log = kernel.Get<ManualLogSource, string>(DeliConstants.Name + " MonoMod").Expect("Could not acquire MonoMod log.");

			var loader = new MonoModAssetLoader(log);
			Deli.AddAssetLoader("monomod", loader);
		}
	}
}
