using Atlas;

namespace Deli
{
    /// <summary>
    /// Interface for asset loaders
    /// </summary>
    public interface IAssetLoader
    {
        void LoadAsset(IServiceKernel kernel, Mod mod, string path);
    }
}