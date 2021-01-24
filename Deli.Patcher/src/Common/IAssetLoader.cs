using Deli.VFS;

namespace Deli.Patcher
{
    public interface IImmediateAssetLoader<TStage> where TStage : Stage
    {
		void LoadAsset(TStage stage, Mod mod, IHandle handle);
	}

	public interface ISharedAssetLoader : IImmediateAssetLoader<Stage>
	{
	}
}
