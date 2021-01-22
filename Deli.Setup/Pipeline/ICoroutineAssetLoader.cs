using System.Collections;
using Deli.VFS;

namespace Deli.Setup
{
    public interface ICoroutineAssetLoader
    {
		IEnumerator LoadAsset(ISetupStage stage, Mod mod, IHandle handle);
	}
}
