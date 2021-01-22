using Deli.VFS;

namespace Deli.Patcher
{
    public interface IImmediateAssetLoader
    {
		void LoadAsset(IPatcherStage stage, Mod mod, IHandle handle);
	}
}
