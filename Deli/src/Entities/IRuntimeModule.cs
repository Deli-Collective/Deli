using BepInEx.Logging;

namespace Deli
{
	/// <summary>
	/// 	Represents the Deli runtime. This is used internally and should not be implemented externally. It may be modified at any time.
	/// </summary>
	public interface IDeliRuntime
	{
		IAssetLoader Load(ManualLogSource log);
	}
}
