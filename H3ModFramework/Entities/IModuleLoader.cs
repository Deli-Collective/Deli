using Atlas;

namespace H3ModFramework
{
    /// <summary>
    /// Interface for module loaders
    /// </summary>
    public interface IModuleLoader
    {
        void LoadModule(IServiceKernel kernel, ModInfo mod, ModInfo.ModuleInfo module);
    }
}