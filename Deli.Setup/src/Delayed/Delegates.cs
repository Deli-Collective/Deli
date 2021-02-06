using System.Collections;
using Deli.VFS;

namespace Deli.Setup
{
	/// <summary>
	///		A deserializer that is performed over multiple frames.
	/// </summary>
	/// <typeparam name="T">The type to deserialize to.</typeparam>
	public delegate ResultYieldInstruction<T> DelayedReader<T>(IFileHandle file) where T : notnull;

	/// <summary>
	///		An asset loader that operates over multiple frames.
	/// </summary>
	public delegate IEnumerator DelayedAssetLoader(RuntimeStage stage, Mod mod, IHandle handle);
}
