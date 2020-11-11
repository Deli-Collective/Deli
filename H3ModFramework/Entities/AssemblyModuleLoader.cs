using System;
using System.Reflection;
using Atlas;
using Atlas.Fluent.Impl;

namespace H3ModFramework
{
    /// <summary>
    /// Module Loader for assemblies.
    /// </summary>
    public class AssemblyModuleLoader : IModuleLoader
    {
        public void LoadModule(IServiceKernel kernel, ModInfo mod, ModInfo.ModuleInfo module)
        {
            // Load the assembly and scan it for new module loaders and resource type loaders
            var assembly = mod.GetResource<Assembly>(module.Path);

            // Try to discover any mod plugins in the assembly
            foreach (var type in assembly.GetTypesSafe())
            {
                if (kernel.LoadEntryType(type).IsSome)
                {
                    continue;
                }

                if (type.IsQuickBindable())
                {
                    var bindingType = typeof(ConstantServiceBinding<>).MakeGenericType(type);
                    var bindMethod = typeof(IServiceBinder).GetMethod(nameof(IServiceBinder.Bind)).MakeGenericMethod(type);

                    var impl = Activator.CreateInstance(type);
                    var binding = Activator.CreateInstance(bindingType, impl);
                    bindMethod.Invoke(kernel, new[] { binding });

                    continue;
                }

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