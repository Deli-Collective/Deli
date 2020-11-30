using BepInEx.Logging;

namespace Deli
{
	/// <summary>
	/// 	Represents the Deli runtime plugin. This is used internally and should not be implemented externally. It may be modified at any time.
	/// </summary>
	public interface IDeliPlugin
	{
		IAssetLoader Load(ManualLogSource log);
	}
}
