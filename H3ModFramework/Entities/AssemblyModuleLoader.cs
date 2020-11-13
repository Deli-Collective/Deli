using System;
using System.Reflection;
using Atlas;
using Atlas.Fluent.Impl;
using BepInEx.Logging;

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

                if (type.QuickBindableCtor().MatchSome(out var ctor))
                {
                    var genericArguments = new[] { type, typeof(Unit) };
                    var bindingType = typeof(ConstantServiceBinding<,>).MakeGenericType(genericArguments);
                    var bindMethod = typeof(IServiceBinder).GetMethod(nameof(IServiceBinder.Bind)).MakeGenericMethod(genericArguments);

                    var impl = ctor.Invoke(new object[0]);
                    var binding = Activator.CreateInstance(bindingType, impl);
                    bindMethod.Invoke(kernel, new[] { binding });

                    continue;
                }

                if (!type.IsSubclassOf(typeof(H3VRMod))) continue;

                H3ModFramework.ManagerObject.SetActive(false);
                var modClass = (H3VRMod) H3ModFramework.ManagerObject.AddComponent(type);
                modClass.BaseMod = mod;
                modClass.Logger = kernel.Get<ManualLogSource, string>(mod.Name).Unwrap();
                H3ModFramework.ManagerObject.SetActive(true);
            }
        }
    }
}