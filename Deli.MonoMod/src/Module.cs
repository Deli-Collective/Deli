using ADepIn;

namespace Deli.MonoMod
{
	internal class Module : DeliModule
	{
		public Module()
		{
			var loader = new MonoModAssetLoader(Logger);
			Deli.AddAssetLoader("monomod", loader);
		}
	}
}
