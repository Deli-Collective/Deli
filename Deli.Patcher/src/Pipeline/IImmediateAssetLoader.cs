using Deli.VFS;

namespace Deli.Patcher
{
	/// <summary>
	///		An asset loader that completes in a singular method call.
	/// </summary>
	/// <typeparam name="TStage">The type of <see cref="Stage"/> that this asset loader supports.</typeparam>
    public interface IImmediateAssetLoader<in TStage> where TStage : Stage
    {
		void LoadAsset(TStage stage, Mod mod, IHandle handle);
	}

	/// <summary>
	///		An <see cref="IImmediateAssetLoader{TStage}"/> that can operate on any <see cref="Stage"/>.
	/// </summary>
	public interface ISharedAssetLoader : IImmediateAssetLoader<Stage>
	{
	}
}
