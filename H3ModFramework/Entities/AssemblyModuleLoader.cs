using System.Reflection;

namespace H3ModFramework
{
    /// <summary>
    /// Module Loader for assemblies.
    /// </summary>
    [ModuleLoader(Name = "Assembly")]
    public class AssemblyModuleLoader : IModuleLoader
    {
        public void LoadModule(ModInfo mod, ModInfo.ModuleInfo module)
        {
            // Load the assembly and scan it for new module loaders and resource type loaders
            var assembly = mod.GetResource<Assembly>(module.FilePath);
            ModuleLoaderAttribute.ScanAssembly(assembly);
            ResourceTypeLoader.ScanAssembly(assembly);

            // Try to discover any mod plugins in the assembly
            foreach (var type in assembly.GetTypesSafe())
            {
                if (!type.IsSubclassOf(typeof(H3VRMod))) continue;
                H3ModFramework.ManagerObject.SetActive(false);
                var modClass = (H3VRMod) H3ModFramework.ManagerObject.AddComponent(type);
                modClass.BaseMod = mod;
                modClass.Logger = H3ModFramework.GetLogger(mod.Name);
                H3ModFramework.ManagerObject.SetActive(true);
            }
        }
    }
}