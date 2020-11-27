using ADepIn;
using BepInEx.Logging;

namespace Deli.MonoMod
{
	internal class Module : IEntryModule<Module>
	{
		public void Load(IServiceKernel kernel)
		{
			var log = kernel.Get<ManualLogSource, string>(Constants.Name + "-MonoMod").Expect("Could not acquire MonoMod log.");

			var loader = new MonoModAssetLoader(log);
			Deli.AddLoader("monomod", loader);
		}
	}
}
