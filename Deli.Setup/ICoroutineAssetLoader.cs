using System.Collections;
using Deli.VFS;

namespace Deli
{
    public interface ICoroutineAssetLoader
    {
		IEnumerator LoadAsset(Mod mod, IHandle handle);
	}
}
