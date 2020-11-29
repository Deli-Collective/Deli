using BepInEx.Logging;

namespace Deli
{
	public interface IDeliRuntime
	{
		IAssetLoader Load(ManualLogSource log);
	}
}
