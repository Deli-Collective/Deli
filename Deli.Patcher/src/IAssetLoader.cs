using Deli.VFS;

namespace Deli
{
    public interface IAssetLoader
    {
		void LoadAsset(Mod mod, IHandle handle);
	}
}
