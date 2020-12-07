namespace Deli.MonoMod
{
	internal class Module : DeliModule
	{
		public Module()
		{
			Deli.AddAssetLoader("monomod", new AssetLoader(Logger));
		}
	}
}
