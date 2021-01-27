using Deli.VFS;

namespace Deli.Patcher
{
	/// <summary>
	///		An asset loader that completes in a singular method call.
	/// </summary>
	/// <typeparam name="TStage">The type of <see cref="Stage"/> that this asset loader supports.</typeparam>
	public delegate void ImmediateAssetLoader<in TStage>(TStage stage, Mod mod, IHandle handle) where TStage : Stage;
}
