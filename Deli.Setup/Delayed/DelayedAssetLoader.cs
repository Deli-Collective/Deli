using System.Collections;
using Deli.VFS;

namespace Deli.Setup
{
	/// <summary>
	///		An asset loader that operates over multiple frames.
	/// </summary>
	public delegate IEnumerator DelayedAssetLoader(RuntimeStage stage, Mod mod, IHandle handle);
}
