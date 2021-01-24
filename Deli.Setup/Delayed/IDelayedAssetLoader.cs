using System.Collections;
using Deli.VFS;

namespace Deli.Setup
{
	/// <summary>
	///		An asset loader that operates over multiple frames.
	/// </summary>
    public interface IDelayedAssetLoader
    {
		IEnumerator LoadAsset(SetupStage stage, Mod mod, IHandle handle);
	}
}
