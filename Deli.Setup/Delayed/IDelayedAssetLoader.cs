using System.Collections;
using Deli.VFS;

namespace Deli.Setup
{
    public interface IDelayedAssetLoader
    {
		IEnumerator LoadAsset(SetupStage stage, Mod mod, IHandle handle);
	}
}
